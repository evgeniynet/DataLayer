using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Collections;
using System.Xml;
using System.IO;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.Generic;

namespace bigWebApps.bigWebDesk.Data
{
    public class FreshBooks : DBAccess
    {
        public static void DeleteFreshBooksLinks(int DeptID)
        {
            UpdateData("sp_DeleteFreshBooksLinks", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DeptID)
			});
        }

        private static string FBRequest(Config config, string requestMethod, Hashtable htAddElements, out XmlTextReader xmlReader,
            string oAuthConsumerKey, string oAuthSecret, string parentElement)
        {
            string result = "Unexpected error.";
            xmlReader = null;
            HttpWebRequest fbRequest = (HttpWebRequest)WebRequest.Create(config.FBURL + "/api/2.1/xml-in");
            fbRequest.Headers.Add("Authorization", "OAuth realm=\"\",oauth_consumer_key=\""
                + Uri.EscapeDataString(oAuthConsumerKey) + "\",oauth_token=\"" + Uri.EscapeDataString(config.FBAccessToken)
                + "\",oauth_signature_method=\"PLAINTEXT\",oauth_signature=\"" + Uri.EscapeDataString(oAuthSecret + "&" + config.FBAccessTokenSecret)
                + "\",oauth_timestamp=\"" + Timestamp + "\",oauth_nonce=\"" + Nonce + "\", oauth_version=\"1.0\"");
            StringWriter sw = new StringWriter();
            XmlWriter xw = XmlWriter.Create(sw);
            xw.WriteStartDocument();
            xw.WriteStartElement("request");
            xw.WriteAttributeString("method", requestMethod);
            if (parentElement != "")
            {
                xw.WriteStartElement(parentElement);
            }
            if (htAddElements != null)
            {
                foreach (string key in htAddElements.Keys)
                {
                    xw.WriteElementString(key, htAddElements[key].ToString());
                }
            }
            if (parentElement != "")
            {
                xw.WriteEndElement();
            }
            xw.WriteEndElement();
            xw.Close();
            string command = sw.ToString();
            sw.Close();
            fbRequest.Method = "POST";
            fbRequest.ContentType = "application/xml";
            fbRequest.ContentLength = command.Length;
            StreamWriter requestWriter = new StreamWriter(fbRequest.GetRequestStream());
            try
            {
                requestWriter.Write(command);
            }
            catch
            {
                throw;
            }
            finally
            {
                requestWriter.Close();
                requestWriter = null;
            }

            try
            {
                using (HttpWebResponse fbResponse = (HttpWebResponse)fbRequest.GetResponse())
                {
                    Stream receiveStream = fbResponse.GetResponseStream();
                    StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                    xmlReader = new XmlTextReader(new StringReader(readStream.ReadToEnd()));
                    xmlReader.Read();
                    xmlReader.Read();
                    xmlReader.Read();
                    xmlReader.MoveToFirstAttribute();
                    xmlReader.MoveToNextAttribute();
                    string xValue = xmlReader.Value;
                    if (xmlReader.Name == "status")
                    {
                        switch (xValue)
                        {
                            case "ok":
                                return "ok";
                            case "fail":
                                xmlReader.Read();
                                xmlReader.Read();
                                xmlReader.Read();
                                if (xmlReader.Value.IndexOf("Client limit of") >= 0)
                                {
                                    result = "Your Freshbook’s account has reached its limit for Customers. Please upgrade your FB account before proceeding";
                                }
                                else
                                {
                                    result = xmlReader.Value;
                                }
                                break;
                            default:
                                result = "Unrecognized API response status.";
                                break;
                        }
                    }
                    else
                    {
                        result = "Unrecognized API response.";
                    }
                    receiveStream.Close();
                    fbResponse.Close();
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        private static long Timestamp
        {
            get { return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds; }
        }

        private static string Nonce
        {
            get { return Guid.NewGuid().ToString("N"); }
        }

        public static string GetClientList(Config config, out List<FBClient> arrClients, string oAuthConsumerKey, string oAuthSecret, int perPage, int page)
        {
            arrClients = new List<FBClient>();
            Hashtable ht = new Hashtable();
            ht.Add("folder", "active");
            if (perPage > 0)
            {
                ht.Add("per_page", perPage);
            }
            if (page > 0)
            {
                ht.Add("page", page);
            }
            XmlTextReader xmlReader = null;
            string result = FBRequest(config, "client.list", ht, out xmlReader, oAuthConsumerKey, oAuthSecret, "");
            if (result == "ok")
            {
                int pageNumber = 1;
                FBClient fbClient = new FBClient();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "clients":
                                pageNumber = GetPagesCount(xmlReader);
                                break;
                            case "client":
                                fbClient = new FBClient();
                                break;
                            case "client_id":
                                fbClient.ClientID = GetXMLTextNodeValueInt(xmlReader);
                                break;
                            case "organization":
                                fbClient.OrgName = GetXMLTextNodeValueStr(xmlReader);
                                break;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement
                       && xmlReader.Name == "client")
                    {
                        arrClients.Add(fbClient);
                    }
                }
                if (pageNumber > page)
                {
                    arrClients.Add(new FBClient(-(page + 1), "More..."));
                }
            }
            return result;
        }

        public static string GetStaffList(Config config, out List<FBStaff> arrStaff, string oAuthConsumerKey, string oAuthSecret, int perPage, int page)
        {
            arrStaff = new List<FBStaff>();
            Hashtable ht = new Hashtable();
            if (perPage > 0)
            {
                ht.Add("per_page", perPage);
            }
            if (page > 0)
            {
                ht.Add("page", page);
            }
            XmlTextReader xmlReader = null;
            string result = FBRequest(config, "staff.list", ht, out xmlReader, oAuthConsumerKey, oAuthSecret, "");
            if (result == "ok")
            {
                int pageNumber = 1;
                FBStaff fbStaff = new FBStaff();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "staff_members":
                                pageNumber = GetPagesCount(xmlReader);
                                break;
                            case "member":
                                fbStaff = new FBStaff();
                                break;
                            case "staff_id":
                                fbStaff.StaffID = GetXMLTextNodeValueInt(xmlReader);
                                break;
                            case "first_name":
                                fbStaff.FirstName = GetXMLTextNodeValueStr(xmlReader);
                                break;
                            case "last_name":
                                fbStaff.LastName = GetXMLTextNodeValueStr(xmlReader);
                                break;
                            case "email":
                                fbStaff.Email = GetXMLTextNodeValueStr(xmlReader);
                                break;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement
                       && xmlReader.Name == "member")
                    {
                        arrStaff.Add(fbStaff);
                    }
                }
                if (pageNumber > page)
                {
                    arrStaff.Add(new FBStaff(-(page + 1), "More..."));
                }
            }
            return result;
        }

        public static string GetProjectsList(Config config, out List<FBProject> arrProjects, string oAuthConsumerKey, string oAuthSecret,
            int perPage, int page, int clientID, int staffID)
        {
            arrProjects = new List<FBProject>();
            Hashtable ht = new Hashtable();
            if (perPage > 0)
            {
                ht.Add("per_page", perPage);
            }
            if (page > 0)
            {
                ht.Add("page", page);
            }
            if (clientID > 0)
            {
                ht.Add("client_id", clientID);
            }
            XmlTextReader xmlReader = null;
            string result = FBRequest(config, "project.list", ht, out xmlReader, oAuthConsumerKey, oAuthSecret, "");
            if (result == "ok")
            {
                int pageNumber = 1;
                bool assignedStaff = false;
                FBProject fbProject = new FBProject();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "projects":
                                pageNumber = GetPagesCount(xmlReader);
                                break;
                            case "project":
                                assignedStaff = false;
                                fbProject = new FBProject();
                                break;
                            case "project_id":
                                fbProject.ProjectID = GetXMLTextNodeValueInt(xmlReader);
                                break;
                            case "client_id":
                                fbProject.ClientID = GetXMLTextNodeValueInt(xmlReader);
                                break;
                            case "name":
                                fbProject.Name = GetXMLTextNodeValueStr(xmlReader);
                                break;
                            case "staff_id":
                                if (!assignedStaff && GetXMLTextNodeValueInt(xmlReader) == staffID)
                                {
                                    assignedStaff = true;
                                }
                                break;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement
                       && xmlReader.Name == "project")
                    {
                        if (assignedStaff)
                        {
                            arrProjects.Add(fbProject);
                        }
                    }
                }
                if (pageNumber > page)
                {
                    arrProjects.Add(new FBProject(-(page + 1), "More..."));
                }
            }
            return result;
        }

        public static string GetTasksList(Config config, out List<FBTask> arrTasks, string oAuthConsumerKey, string oAuthSecret,
            int perPage, int page, int projectID)
        {
            arrTasks = new List<FBTask>();
            Hashtable ht = new Hashtable();
            if (perPage > 0)
            {
                ht.Add("per_page", perPage);
            }
            if (page > 0)
            {
                ht.Add("page", page);
            }
            if (projectID > 0)
            {
                ht.Add("project_id", projectID);
            }
            XmlTextReader xmlReader = null;
            string result = FBRequest(config, "task.list", ht, out xmlReader, oAuthConsumerKey, oAuthSecret, "");
            if (result == "ok")
            {
                int pageNumber = 1;
                FBTask fbTask = new FBTask();
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        switch (xmlReader.Name)
                        {
                            case "tasks":
                                pageNumber = GetPagesCount(xmlReader);
                                break;
                            case "task":
                                fbTask = new FBTask();
                                break;
                            case "task_id":
                                fbTask.TaskID = GetXMLTextNodeValueInt(xmlReader);
                                break;
                            case "name":
                                fbTask.Name = GetXMLTextNodeValueStr(xmlReader);
                                break;
                        }
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement
                       && xmlReader.Name == "task")
                    {
                        arrTasks.Add(fbTask);
                    }
                }
                if (pageNumber > page)
                {
                    arrTasks.Add(new FBTask(-(page + 1), "More..."));
                }
            }
            return result;
        }

        public static string CreateTimeEntry(Guid orgID, int dID, Config config, string oAuthConsumerKey, string oAuthSecret,
            int staffID, int projectID, int taskID, decimal hours, string notes, DateTime date, int timeLogID, bool isProjectLog)
        {
            Hashtable ht = new Hashtable();
            if (staffID > 0)
            {
                ht.Add("staff_id", staffID);
            }
            ht.Add("project_id", projectID);
            ht.Add("task_id", taskID);
            ht.Add("hours", hours);
            if (notes != "")
            {
                ht.Add("notes", notes);
            }
            if (date != DateTime.MinValue)
            {
                ht.Add("date", date.ToString("yyyy-MM-dd"));
            }
            
            XmlTextReader xmlReader = null;
            string result = FBRequest(config, "time_entry.create", ht, out xmlReader, oAuthConsumerKey, oAuthSecret, "time_entry");
            if (result == "ok")
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        if (xmlReader.Name == "time_entry_id")
                        {
                            int fbTimeEntryID = GetXMLTextNodeValueInt(xmlReader);
                            if (fbTimeEntryID > 0)
                            {
                                LinkFreshBooksTimeEntry(orgID, dID, timeLogID, fbTimeEntryID, isProjectLog);
                            }
                        }
                    }
                }
            }
            return result;
        }

        private static string GetXMLTextNodeValueStr(XmlTextReader xmlReader)
        {
            if (xmlReader.Read() && xmlReader.NodeType == XmlNodeType.Text)
            {
                return xmlReader.Value;
            }
            return "";
        }

        private static int GetXMLTextNodeValueInt(XmlTextReader xmlReader)
        {
            int intElementValue;
            int.TryParse(GetXMLTextNodeValueStr(xmlReader), out intElementValue);
            return intElementValue;
        }

        private static int GetPagesCount(XmlTextReader xmlReader)
        {
            xmlReader.MoveToAttribute("pages");
            return int.Parse(xmlReader.Value);
        }

        public static void UpdateData(Guid orgID, int companyID, int userID, int fbStaffID, int accountID, int fbClientId, int projectID,
            int fbProjectID, int taskTypeID, int fbTaskTypeID)
        {
            UpdateData("sp_UpdateFreshBooksData", new SqlParameter[]{
				new SqlParameter("@CompanyID", companyID),
                new SqlParameter("@UserID", userID),
                new SqlParameter("@FBStaffID", fbStaffID),
                new SqlParameter("@AccountID", accountID),
                new SqlParameter("@FBClientId", fbClientId),
                new SqlParameter("@ProjectID", projectID),
                new SqlParameter("@FBProjectID", fbProjectID),
                new SqlParameter("@TaskTypeID", taskTypeID),
                new SqlParameter("@FBTaskTypeID", fbTaskTypeID)
			}, orgID);
        }

        public static void LinkFreshBooksTimeEntry(Guid orgID, int dID, int timeLogID, int fbTimeEntryID, bool isProjectLog)
        {
            UpdateData("sp_LinkFreshBooksTimeEntry", new SqlParameter[]{
				new SqlParameter("@DId", dID),
                new SqlParameter("@TimeLogID", timeLogID),
                new SqlParameter("@FBTimeEntryID", fbTimeEntryID),
                new SqlParameter("@IsProjectLog", isProjectLog)
			}, orgID);
        }
    }

    [DataContract(Name = "fb_client")]
    public class FBClient
    {
        private string m_OrgName = "";
        private int m_ClientID = 0;

        public FBClient()
        {
        }

        public FBClient(int clientID, string orgName)
        {
            ClientID = clientID;
            OrgName = orgName;
        }

        [DataMember(Name = "organization")]
        public string OrgName
        {
            get
            {
                return m_OrgName;
            }
            set
            {
                m_OrgName = value;
            }
        }

        [DataMember(Name = "client_id")]
        public int ClientID
        {
            get
            {
                return m_ClientID;
            }
            set
            {
                m_ClientID = value;
            }
        }
    }

    [DataContract(Name = "fb_staff")]
    public class FBStaff
    {
        private string m_FirstName = "";
        private string m_LastName = "";
        private string m_Email = "";
        private int m_StaffID = 0;

        public FBStaff()
        {
        }

        public FBStaff(int staffID, string email)
        {
            StaffID = staffID;
            Email = email;
        }

        [DataMember(Name = "staff_id")]
        public int StaffID
        {
            get
            {
                return m_StaffID;
            }
            set
            {
                m_StaffID = value;
            }
        }

        [DataMember(Name = "first_name")]
        public string FirstName
        {
            get
            {
                return m_FirstName;
            }
            set
            {
                m_FirstName = value;
            }
        }

        [DataMember(Name = "last_name")]
        public string LastName
        {
            get
            {
                return m_LastName;
            }
            set
            {
                m_LastName = value;
            }
        }

        [DataMember(Name = "email")]
        public string Email
        {
            get
            {
                return m_Email;
            }
            set
            {
                m_Email = value;
            }
        }

        [DataMember(Name = "full_name")]
        public string FullName
        {
            get
            {
                if (FirstName.Trim() == "" && LastName.Trim() == "")
                {
                    return Email;
                }
                return (FirstName.Trim() + " " + LastName.Trim()).Trim();
            }
        }
    }

    [DataContract(Name = "fb_project")]
    public class FBProject
    {
        private string m_Name = "";
        private int m_ProjectID = 0;
        private int m_ClientID = 0;

        public FBProject()
        {
        }

        public FBProject(int projectID, string orgName)
        {
            ProjectID = projectID;
            Name = orgName;
        }

        [DataMember(Name = "name")]
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        [DataMember(Name = "project_id")]
        public int ProjectID
        {
            get
            {
                return m_ProjectID;
            }
            set
            {
                m_ProjectID = value;
            }
        }

        [DataMember(Name = "client_id")]
        public int ClientID
        {
            get
            {
                return m_ClientID;
            }
            set
            {
                m_ClientID = value;
            }
        }
    }

    [DataContract(Name = "fb_task")]
    public class FBTask
    {
        private string m_Name = "";
        private int m_TaskID = 0;

        public FBTask()
        {
        }

        public FBTask(int taskID, string name)
        {
            TaskID = taskID;
            Name = name;
        }

        [DataMember(Name = "name")]
        public string Name
        {
            get
            {
                return m_Name;
            }
            set
            {
                m_Name = value;
            }
        }

        [DataMember(Name = "task_id")]
        public int TaskID
        {
            get
            {
                return m_TaskID;
            }
            set
            {
                m_TaskID = value;
            }
        }
    }
}
