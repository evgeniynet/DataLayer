using System;
using System.Data;
using System.Text;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class SearchEventArgs : EventArgs
    {
        private string msg;

        public SearchEventArgs()
        { }

        public SearchEventArgs(string messageData)
            : base()
        {
            msg = messageData;
        }
        public string Message
        {
            get { return msg; }
            set { msg = value; }
        }
    }

    public class AssetBulk : DBAccess
    {
        public static int InsertBulkAsset(int departmentId, int userId, int categoryId, int typeId, string AssetName, string AssetDescription)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pUserId = new SqlParameter("@UserId", userId);
            UpdateData("sp_InsertAssetBulk",
                        new[] { 
                                pReturnValue,
                                new SqlParameter("@DepartmentId", departmentId),
                                new SqlParameter("@UserId", userId),
                                new SqlParameter("@AssetCategory", categoryId),
                                new SqlParameter("@AssetType", typeId),                                
                                new SqlParameter("@Name", AssetName),
                                new SqlParameter("@Description", AssetDescription)
                                });

            return (int)pReturnValue.Value;
        }

        public static DataTable SelectBulkAssets(int departmentId, string search)
        {
            SqlParameter pSearchPattern = new SqlParameter("@Pattern", DBNull.Value);
            if (!String.IsNullOrEmpty(search))
                pSearchPattern.Value = Security.SQLInjectionBlock(search);
            return SelectRecords("sp_SelectAssetsBulk", new[] { new SqlParameter("@DepartmentId", departmentId), pSearchPattern });
        }

        public static DataTable SelectBulkAssets(int departmentId, string search, string customSort, bool ExcelMode)
        {
            StringBuilder sb = new StringBuilder("SELECT "
            + ((!ExcelMode) ? "a.Id, a.CategoryId, a.TypeId, " : String.Empty)
            + "a.Name, a.[Description],  ac.Name AS CategoryName, at.Name AS TypeName, "
            + "SUM(al.Quantity) AS QuantityCount, SUM(al.ExcessQuantity) AS ExcessCount "
            + "FROM AssetBulk a "
            + "LEFT OUTER JOIN AssetCategories ac ON a.DepartmentId = ac.DepartmentId AND a.CategoryId = ac.id "
            + "LEFT OUTER JOIN AssetTypes at ON a.DepartmentId = at.DepartmentId AND a.TypeId = at.id "            
            //+ "LEFT OUTER JOIN AssetBulkLocation al ON a.DepartmentId = al.DId AND a.Id = al.AssetBulkId "
            //+ "AND al.AuditDate = (SELECT TOP 1 AuditDate FROM AssetBulkLocation abl WHERE abl.DId = al.DId AND abl.AssetBulkId = al.AssetBulkId ORDER BY abl.AuditDate DESC) "

            + "LEFT OUTER JOIN AssetBulkLocation al " 
            + "ON a.DepartmentId = al.DId AND a.Id = al.AssetBulkId  "
            + "AND al.Id IN ( "
	        + "SELECT Id "
	        + "FROM "
		    + "AssetBulkLocation abl "
			+ "INNER JOIN (SELECT DISTINCT ll.LocationId FROM AssetBulkLocation ll WHERE ll.DId = al.DId AND ll.AssetBulkId = al.AssetBulkId) o ON " 
			+ "abl.DId = al.DId "
			+ "AND "
            + "abl.AssetBulkId = al.AssetBulkId "
			+ "AND "
			+ "abl.LocationId = o.LocationId "
			+ "AND "
			+ "abl.AuditDate = (SELECT TOP 1 AuditDate FROM AssetBulkLocation tt WHERE tt.DId = abl.DId AND tt.AssetBulkId = abl.AssetBulkId AND tt.LocationId = o.LocationId ORDER BY tt.AuditDate DESC)) "
            + "WHERE "
            + "a.DepartmentId=" + departmentId.ToString() + " AND a.Active = 1 AND (" + (String.IsNullOrEmpty(search) ? "0=0" : "1=0") + " OR a.Name LIKE '%" + search + "%' OR ac.Name LIKE '%" + search + "%' OR at.Name LIKE '%" + search + "%') "
            + "GROUP BY a.Id, a.Name, a.[Description], a.CategoryId, a.TypeId, ac.Name, at.Name "
            + "ORDER BY ");
            string order = "ac.Name, a.CategoryId, at.Name, a.TypeId, a.Name, a.Id";
            if (String.IsNullOrEmpty(customSort))
                customSort = "1_ASC";
            string[] sarr = customSort.Split('_');
            if (sarr.Length > 0)
            {
                if (sarr[0] == "1")
                {
                    order = "ac.Name, a.CategoryId, at.Name, a.TypeId, a.Name, a.Id";
                    if (sarr.Length > 1)
                        if (sarr[1] == "DESC")
                            order = "ac.Name DESC, a.CategoryId DESC, at.Name DESC, a.TypeId DESC, a.Name DESC, a.Id DESC";
                }
                else
                {
                    order = "a.Name, a.Id, ac.Name, a.CategoryId, at.Name, a.TypeId";
                    if (sarr.Length > 1)
                        if (sarr[1] == "DESC")
                            order = "a.Name DESC, a.Id DESC, ac.Name DESC, a.CategoryId DESC, at.Name DESC, a.TypeId DESC";
                }

            }
            sb.Append(order);
            DataTable _table = SelectByQuery(sb.ToString());
            return _table;
        }

        public static DataRow SelectBulkAsset(int departmentId, int assetId)
        {
            return SelectRecord("sp_SelectAssetBulk", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetBulkByLocations(int DeptID, int AssetID, int LocationParentId)
        {
            return SelectRecords("sp_SelectAssetBulkByLocations",
                new SqlParameter[]
                {
                    new SqlParameter("@DId", DeptID),
                    new SqlParameter("@AssetId", AssetID),
                    new SqlParameter("@ParentId", LocationParentId) // 0 - top, NULL is not allowed
                });
        }

        public static void UpdateBulkAsset(int departmentId, int userId, int assetId, int categoryId, int typeId, string assetName, string assetDescription)
        {
            UpdateData("sp_UpdateAssetBulk",
                new[] { new SqlParameter("@DepartmentId", departmentId),
                    new SqlParameter("@UId", userId),
                    new SqlParameter("@AssetId", assetId),                                                                                                                           
                    new SqlParameter("@Name", assetName),
                    new SqlParameter("@Description", assetDescription),
                    new SqlParameter("@AssetCategory", categoryId),
                    new SqlParameter("@AssetType", typeId)});
        }

        public static void Delete(int departmentId, int userId, int assetId)
        {
            UpdateData("sp_DeleteAssetBulk",
                new[] { new SqlParameter("@DepartmentId", departmentId),
                    new SqlParameter("@UId", userId),
                    new SqlParameter("@AssetId", assetId)});
        }


        public class ExportBulkAssets : Assets.ExcelDocument
        {
            public ExportBulkAssets(ref System.Web.UI.HtmlTextWriter writer)
            {
                SetWriter(ref writer);
            }

            private void CreateAssetSheet(ref DataTable table)
            {
                System.Web.UI.HtmlTextWriter _writer = GetWriter();
                if ((table != null) && (_writer != null))
                {
                    DataRow[] foundRows = table.Select();
                    if (foundRows != null)
                    {
                        if (foundRows.Length > 0)
                        {
                            Assets.ExcelColumns columns = null;
                            columns = CreateColumns(ref columns, ref table);

                            string _start_worksheet = "<Worksheet ss:Name=\"BulkAssets\">";
                            string _end_worksheet = "</Worksheet>";
                            _writer.Write(_start_worksheet);
                            Assets.ExcelTable _table = this.CreateWorkSheet("BulkAssets", ref columns);
                            if (_table != null)
                            {
                                AddHeader(ref _table, ref columns);
                                _writer.Write(_table.ToHeader());
                                bool is_white = true;
                                for (int row_index = 0; row_index < foundRows.Length; row_index++)
                                {
                                    DataRow _row = foundRows[row_index];
                                    if (_row != null)
                                    {
                                        Assets.ExcelRow excel_row = new Assets.ExcelRow(_table);
                                        if (excel_row != null)
                                        {
                                            UpdateRow(ref excel_row, ref table, ref _row, is_white);
                                            is_white = !is_white;
                                            _writer.Write(excel_row.ToString());
                                            _writer.Flush();
                                        }
                                    }
                                }
                                _writer.Write(_table.ToFooter());
                            }
                            _writer.Write(_end_worksheet);
                        }
                    }
                }
            }

            public string Export(ref DataTable table)
            {
                string result = String.Empty;
                System.Web.UI.HtmlTextWriter _writer = GetWriter();
                if (_writer != null)
                {
                    string _document = ToString();
                    if (_document.Length > "</Workbook>".Length)
                        _document = _document.Remove(_document.Length - "</Workbook>".Length - 1, "</Workbook>".Length);
                    _writer.Write(_document);
                    _writer.Flush();
                    CreateAssetSheet(ref table);
                    _writer.Write("</Workbook>");
                    _writer.Flush();
                }
                return result;
            }
        }
    }
}