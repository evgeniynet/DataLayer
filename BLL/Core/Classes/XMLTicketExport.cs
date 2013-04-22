using System;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Xml;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for XMLTicketExport.
    /// </summary>
    public class XMLTicketExport : DBAccess
    {

        public static DataTable SelectCustomFields(int departmentId)
        {
            return SelectRecords("sp_SelectCustomFields", new[] { new SqlParameter("@DepartmentId", departmentId),
                new SqlParameter("@ClassID", -1) });
        }

        public static DataTable SelectTickets(int departmentId, DateTime startDate, DateTime endDate, int lastTicketId)
        {
            return SelectTickets(departmentId, startDate, endDate, lastTicketId, 0);
        }

        public static DataTable SelectTickets(int departmentId, DateTime startDate, DateTime endDate, int lastTicketId, int numberOfTicketToReturn)
        {
            string sqlQuery = string.Empty;
            if (numberOfTicketToReturn > 0)
                sqlQuery = string.Format("SELECT TOP {0} *, dbo.fxGetUserLocationName({1}, LocationId) AS LocationName, dbo.fxGetUserLocationName({1}, AccountLocationId) AS AccountLocation, dbo.fxGetFullClassName({1},ClassId) AS ClassName, dbo.fxGetUserFolderName({1},FolderId) AS FolderName FROM vw_XMLTktExport WHERE DepartmentId = {1} AND TicketId > {2} AND CreateTime BETWEEN '{3}' AND '{4}' ORDER BY TicketId", numberOfTicketToReturn, departmentId, lastTicketId, Functions.FormatSQLDateTime(startDate), Functions.FormatSQLDateTime(endDate));
            else
                sqlQuery = string.Format("SELECT *, dbo.fxGetUserLocationName({0}, LocationId) AS LocationName, dbo.fxGetUserLocationName({0}, AccountLocationId) AS AccountLocation, dbo.fxGetFullClassName({0},ClassId) AS ClassName, dbo.fxGetUserFolderName({0},FolderId) AS FolderName FROM vw_XMLTktExport WHERE DepartmentId = {0} AND TicketId > {1} AND CreateTime BETWEEN '{2}' AND '{3}' ORDER BY TicketId", departmentId, lastTicketId, Functions.FormatSQLDateTime(startDate), Functions.FormatSQLDateTime(endDate));
            return SelectByQuery(sqlQuery);
        }

        public static void LastDownload(int departmentId, DateTime startDate, DateTime endDate)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateXMLDownloadDates",
                        new[] {pReturnValue,
                               new SqlParameter("@DepartmentId", departmentId),
                               new SqlParameter("@StartDate", startDate),
                               new SqlParameter("@EndDate", endDate)});
        }


        public static int SelectTicketCountByDate(int departmentId, DateTime startDate, DateTime endDate)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_SelectTktCountByDate",
                        new[] {pReturnValue,
                               new SqlParameter("@DepartmentId", departmentId),
                               new SqlParameter("@StartDate", startDate),
                               new SqlParameter("@EndDate", endDate)});

            return (int)pReturnValue.Value;
        }
    }

    public class XMLTickets
    {
        private XMLTicketStructure _structure = null;
        private System.Web.UI.HtmlTextWriter _writer = null;

        public XMLTickets(ref HtmlTextWriter writer, ref XMLTicketStructure structure, string selectedFields)
        {
            _writer = writer;

            _structure = structure;

            if (_structure != null && !string.IsNullOrEmpty(selectedFields))
                foreach (string field in selectedFields.Split('|'))
                    if (!string.IsNullOrEmpty(field))
                        _structure.SetUse(field, true);
        }

        public void SetWriter(ref HtmlTextWriter writer)
        {
            _writer = writer;
        }

        public HtmlTextWriter GetWriter()
        {
            return _writer;
        }

        private void ExportHeader()
        {
            string result = string.Empty;

            if (_structure != null)
                result = _structure.ToString();

            if (_writer != null)
            {
                _writer.Write(result);
                _writer.Flush();
            }
        }

        public void Export(int departmentId, DateTime startDate, DateTime endDate, int lastTicketId)
        {
            Export(departmentId, startDate, endDate, lastTicketId, 0);
        }

        public void Export(int departmentId, DateTime startDate, DateTime endDate, int lastTicketId, int numberOfTicketToReturn)
        {
            string crLf = new string(new[] { (char)13, (char)10 });
            string result = string.Empty;

            int lastTId = lastTicketId;

            ExportHeader();

            while (!(lastTId < 0))
            {
                DataTable tickets = XMLTicketExport.SelectTickets(departmentId, startDate, endDate, lastTId, numberOfTicketToReturn);
                if (tickets != null)
                {
                    if (tickets.Rows.Count == 0)
                        lastTId = -1;

                    foreach (DataRow drTicket in tickets.Rows)
                    {
                        lastTId = drTicket.IsNull("TicketId") ? -1 : (int)drTicket["TicketId"];

                        result = "<Tickets>" + crLf;

                        DataTable dtStructure = _structure.GetDataTable();

                        foreach (DataRow drStructure in dtStructure.Rows)
                        {
                            if (!bool.Parse(drStructure["IsUse"].ToString())) continue;

                            string fieldType = drStructure["Type"].ToString();
                            bool fieldCustom = bool.Parse(drStructure["IsCustom"].ToString());
                            bool isCdata = bool.Parse(drStructure["IsUseCDATA"].ToString());

                            string fieldValue = String.Empty;
                            string fieldName = drStructure["Name"].ToString();

                            if (!fieldCustom)
                            {
                                if (!drTicket.Table.Columns.Contains(fieldName))
                                    continue;

                                fieldValue = drTicket[fieldName].ToString();

                                if (!string.IsNullOrEmpty(fieldValue))
                                {
                                    switch (fieldType)
                                    {
                                        case "datetime":
                                            try
                                            {
                                                result += "<" + drStructure["Name"] + ">" + Functions.DB2UserDateTime(DateTime.Parse(fieldValue)).ToString("yyyy-MM-ddTHH:mm:00") + "</" + drStructure["Name"] + ">" + crLf;
                                            }
                                            catch { }
                                            break;
                                        case "currency":
                                            try
                                            {
                                                double dblValue = double.Parse(drTicket[fieldName].ToString());
                                                fieldValue = dblValue.ToString();
                                                result += "<" + drStructure["Name"] + ">" + fieldValue + "</" + drStructure["Name"] + ">" + crLf;
                                            }
                                            catch { }
                                            break;
                                        default:
                                            if (!isCdata)
                                                result += "<" + drStructure["Name"] + ">" + XMLTicketStructure.EscapeFieldName(fieldValue) + "</" + drStructure["Name"] + ">" + crLf;
                                            else
                                            {
                                                result += "<" + drStructure["Name"] + "><![CDATA[";
                                                fieldValue = fieldValue.Replace("<br>", crLf);
                                                result += fieldValue;
                                                result += "]]></" + drStructure["Name"] + ">";
                                                result += crLf;
                                            }
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                switch (fieldName)
                                {
                                    case "TicketPosts":
                                        {
                                            Ticket.LogCollection ticketPosts = new Ticket.LogCollection(departmentId, (int)drTicket["TicketId"]);
                                            result += "<" + drStructure["Name"] + "><![CDATA[";
                                            fieldValue = string.Empty;
                                            foreach (Ticket.LogEntry ticketPost in ticketPosts)
                                            {
                                                if (!string.IsNullOrEmpty(fieldValue))
                                                    result += "\n\n================================================================================================\n";

                                                fieldValue = "********** ";
                                                fieldValue += Functions.DisplayDateTime(ticketPost.CreatedDate, true);
                                                fieldValue += "\t\t" + ticketPost.LogType + "\t\t\t" + ticketPost.UserLastName + ", " + ticketPost.UserFirstName + " **********\n\n" + ticketPost.LogNote;
                                                fieldValue = fieldValue.Replace("<br>", crLf);
                                                result += fieldValue;
                                            }
                                            result += "]]></" + drStructure["Name"] + ">";
                                            result += crLf;

                                        }
                                        break;
                                    //case "Assets":
                                    //    DataTable dtAssets = Tickets.SelectAssets(departmentId, lastTId);
                                    //    if (dtAssets != null && dtAssets.Rows.Count > 0)
                                    //    {
                                    //        result += "<" + drStructure["Name"] + "><![CDATA[";
                                    //        fieldValue = string.Empty;
                                    //        foreach (DataRow drAsset in dtAssets.Rows)
                                    //        {
                                    //            fieldValue += XMLTicketStructure.EscapeFieldName(drAsset["Name"].ToString()) + ";";
                                    //            fieldValue += XMLTicketStructure.EscapeFieldName(drAsset["SerialNumber"].ToString()) + ";";
                                    //            fieldValue += XMLTicketStructure.EscapeFieldName(drAsset["AssetTypeName"].ToString()) + ";";
                                    //            fieldValue += XMLTicketStructure.EscapeFieldName(drAsset["AssetMakeName"].ToString()) + ";";
                                    //            fieldValue += XMLTicketStructure.EscapeFieldName(drAsset["AssetModelName"].ToString()) + crLf;
                                    //        }
                                    //        fieldValue = fieldValue.TrimEnd((char)10).TrimEnd((char)13);
                                    //        result += fieldValue + "]]></" + drStructure["Name"] + ">"+crLf;
                                    //    }
                                    //    break;
                                    default:
                                        if (drTicket.Table.Columns.Contains("CustomXML") && !drTicket.IsNull("CustomXML"))
                                        {
                                            string customXML = drTicket["CustomXML"].ToString();
                                            if (!string.IsNullOrEmpty(customXML))
                                            {
                                                try
                                                {
                                                    XmlDocument xmlDocument = new XmlDocument();
                                                    xmlDocument.LoadXml(customXML);

                                                    XmlNode rootNode = xmlDocument.DocumentElement;
                                                    if (rootNode != null)
                                                    {
                                                        string xpath = "field[@id='" + drStructure["CustomId"] + "']/value";
                                                        XmlNode node = rootNode.SelectSingleNode(xpath);

                                                        if (node != null)
                                                        {
                                                            if (!string.IsNullOrEmpty(node.InnerText))
                                                                result += "<" + fieldName + "><![CDATA[" + node.InnerText + "]]></" + fieldName + ">" + crLf;
                                                        }
                                                    }
                                                }
                                                catch { }
                                            }
                                        }
                                        break;
                                }
                            }
                        }

                        result += "</Tickets>" + crLf;

                        if (result != ("<Tickets>" + crLf + "</Tickets>" + crLf) && _writer != null)
                        {
                            _writer.Write(result);
                            _writer.Flush();
                        }
                    }
                }
                else
                    lastTId = -1;
            }

            ExportFooter(departmentId, startDate, endDate);
        }

        private void ExportFooter(int departmentId, DateTime startDate, DateTime endDate)
        {
            string result = "</dataroot></root>";

            if (_writer != null)
            {
                _writer.Write(result);
                _writer.Flush();
            }

            XMLTicketExport.LastDownload(departmentId, startDate, endDate);
        }

    }

    public class XMLTicketStructure
    {
        private DataTable dtFields = null;

        public XMLTicketStructure()
        {
            dtFields = new DataTable();

            dtFields.TableName = "TicketStructure";

            DataColumn id = new DataColumn();
            id.DataType = Type.GetType("System.Int32");
            id.AllowDBNull = false;
            id.AutoIncrement = true;
            id.AutoIncrementSeed = 1;
            id.AutoIncrementStep = 1;
            id.Caption = "Id";
            id.ColumnName = "Id";

            DataColumn name = new DataColumn();
            name.DataType = Type.GetType("System.String");
            name.AllowDBNull = false;
            name.Caption = "Name";
            name.ColumnName = "Name";
            name.DefaultValue = string.Empty;

            DataColumn type = new DataColumn();
            type.DataType = Type.GetType("System.String");
            type.AllowDBNull = false;
            type.Caption = "Type";
            type.ColumnName = "Type";
            type.DefaultValue = string.Empty;

            DataColumn sqlType = new DataColumn();
            sqlType.DataType = Type.GetType("System.String");
            sqlType.AllowDBNull = false;
            sqlType.Caption = "SqlType";
            sqlType.ColumnName = "SqlType";
            sqlType.DefaultValue = string.Empty;

            DataColumn xsdType = new DataColumn();
            xsdType.DataType = Type.GetType("System.String");
            xsdType.AllowDBNull = false;
            xsdType.Caption = "XsdType";
            xsdType.ColumnName = "XsdType";
            xsdType.DefaultValue = string.Empty;

            DataColumn isNull = new DataColumn();
            isNull.DataType = Type.GetType("System.Boolean");
            isNull.AllowDBNull = false;
            isNull.Caption = "IsNull";
            isNull.ColumnName = "IsNull";
            isNull.DefaultValue = false;

            DataColumn custom = new DataColumn();
            custom.DataType = Type.GetType("System.Boolean");
            custom.AllowDBNull = false;
            custom.Caption = "IsCustom";
            custom.ColumnName = "IsCustom";
            custom.DefaultValue = false;

            DataColumn use = new DataColumn();
            use.DataType = Type.GetType("System.Boolean");
            use.AllowDBNull = false;
            use.Caption = "IsUse";
            use.ColumnName = "IsUse";
            use.DefaultValue = false;

            DataColumn useData = new DataColumn();
            useData.DataType = Type.GetType("System.Boolean");
            useData.AllowDBNull = false;
            useData.Caption = "IsUseCDATA";
            useData.ColumnName = "IsUseCDATA";
            useData.DefaultValue = false;

            DataColumn length = new DataColumn();
            length.DataType = Type.GetType("System.Int32");
            length.AllowDBNull = false;
            length.Caption = "MaxLength";
            length.ColumnName = "MaxLength";

            DataColumn customId = new DataColumn();
            customId.DataType = Type.GetType("System.Int32");
            customId.AllowDBNull = false;
            customId.Caption = "CustomId";
            customId.ColumnName = "CustomId";

            dtFields.Columns.Add(id);
            dtFields.Columns.Add(name);
            dtFields.Columns.Add(type);
            dtFields.Columns.Add(sqlType);
            dtFields.Columns.Add(xsdType);
            dtFields.Columns.Add(isNull);
            dtFields.Columns.Add(custom);
            dtFields.Columns.Add(use);
            dtFields.Columns.Add(useData);
            dtFields.Columns.Add(length);
            dtFields.Columns.Add(customId);
            dtFields.PrimaryKey = new DataColumn[] { dtFields.Columns[0] };
        }

        public DataTable GetDataTable()
        {
            return dtFields;
        }

        public void AddTicketField(string fieldName, string fieldType, string sqlType, string xsdType, bool isNull, bool isCustom, bool isUse, int maxLength, int customId, bool isUseCdata)
        {
            if (dtFields != null)
            {
                DataRow row = dtFields.NewRow();
                row["Name"] = fieldName;
                row["Type"] = fieldType;
                row["SqlType"] = sqlType;
                row["XsdType"] = xsdType;

                row["IsNull"] = isNull;
                row["IsCustom"] = isCustom;
                row["IsUse"] = isUse;
                row["IsUseCDATA"] = isUseCdata;

                row["MaxLength"] = maxLength;
                row["CustomId"] = customId;

                dtFields.Rows.Add(row);
            }
        }

        public void SetUse(string xColumnName, bool isUse)
        {
            if (dtFields != null)
            {
                DataRow columnRow = GetColumnRow(xColumnName);
                if (columnRow != null)
                    columnRow["IsUse"] = isUse;
            }
        }

        protected DataRow GetColumnRow(string xColumnName)
        {
            DataRow result = null;

            if (dtFields != null && !string.IsNullOrEmpty(xColumnName))
            {
                string expression = "Name='" + xColumnName + "'";
                DataRow[] foundRows = dtFields.Select(expression);
                if (foundRows != null && foundRows.Length == 1)
                    result = foundRows[0];
            }

            return result;
        }

        public override string ToString()
        {
            string result = string.Empty;

            result += "<?xml version=\"1.0\" encoding=\"ISO-8859-1\"?>";
            result += "<root xmlns:xsd=\"http://www.w3.org/2000/10/XMLSchema\" xmlns:od=\"urn:schemas-microsoft-com:officedata\">";
            result += "<xsd:schema>";
            result += "<xsd:element name=\"dataroot\"><xsd:complexType><xsd:choice maxOccurs=\"unbounded\"><xsd:element ref=\"Tickets\"/></xsd:choice></xsd:complexType></xsd:element>";
            result += "<xsd:element name=\"Tickets\"><xsd:annotation><xsd:appinfo>";
            result += "<od:index index-name=\"TicketNumber\" index-key=\"TicketNumber\" primary=\"no\" unique=\"no\" clustered=\"no\"/>";
            result += "</xsd:appinfo></xsd:annotation><xsd:complexType><xsd:sequence>";

            if (dtFields != null)
                foreach (DataRow drField in dtFields.Rows)
                    if (bool.Parse(drField["IsUse"].ToString()))
                        switch (drField["Type"].ToString())
                        {
                            case "longinteger":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\" minOccurs=\"0\" od:jetType=\"longinteger\" od:sqlSType=\"int\"><xsd:simpleType><xsd:restriction base=\"xsd:integer\"/></xsd:simpleType></xsd:element>";
                                break;
                            case "yesno":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\" od:jetType=\"yesno\" od:sqlSType=\"bit\" od:nonNullable=\"yes\" type=\"xsd:byte\"/>";
                                break;
                            case "text":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\"  minOccurs=\"0\" od:jetType=\"text\" od:sqlSType=\"nvarchar\"><xsd:simpleType><xsd:restriction base=\"xsd:string\"><xsd:maxLength value=\"";
                                result += drField["MaxLength"].ToString();
                                result += "\" /></xsd:restriction></xsd:simpleType></xsd:element>";
                                break;
                            case "memo":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\" minOccurs=\"0\" od:jetType=\"memo\" od:sqlSType=\"ntext\"><xsd:simpleType><xsd:restriction base=\"xsd:string\"><xsd:maxLength value=\"";
                                result += drField["MaxLength"].ToString();
                                result += "\" /></xsd:restriction></xsd:simpleType></xsd:element>";
                                break;
                            case "datetime":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\"  minOccurs=\"0\" od:jetType=\"datetime\" od:sqlSType=\"datetime\" type=\"xsd:timeInstant\"/>";
                                break;
                            case "currency":
                                result += "<xsd:element name=\"";
                                result += drField["Name"].ToString();
                                result +=
                                    "\"  minOccurs=\"0\" od:jetType=\"currency\" od:sqlSType=\"money\" type=\"xsd:double\"/>";
                                break;
                        }

            result += "</xsd:sequence></xsd:complexType></xsd:element></xsd:schema>";
            result += "<dataroot xmlns:xsi=\"http://www.w3.org/2000/10/XMLSchema-instance\">";

            return result;
        }

        public static string EscapeFieldName(string name)
        {
            string result = name;

            result = Regex.Replace(result, @"(\s|/|\\|-|,|=|&)", "_");
            result = Regex.Replace(result, "([^A-Za-z0-9_])", "");

            return result;
        }
    }

}
