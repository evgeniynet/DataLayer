using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Web;
using Micajah.Common.Security;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for DBAccess.
    /// </summary>
    [Serializable()]
    public class DBAccess
    {
        //-1: Get User Database; 0: Get Default Database; >0: Get Database by number

        private static SqlConnection GetDbConnection(System.Guid OrgID)
        {
            return new SqlConnection(GetConnectionString(OrgID));
        }

        public static Guid GetCurrentOrgID()
        {
            return GetCurrentOrgID(Guid.Empty);
        }

        public static Guid GetCurrentOrgID(Guid OrgID)
        {
            if (OrgID == Guid.Empty)
            {
                UserContext user = UserContext.Current;
                return ((user == null) ? Guid.Empty : user.SelectedOrganizationId);
            }
            else return OrgID;
        }

        public static string GetConnectionString(Guid OrgID)
        {
            if (OrgID == Guid.Empty)
            {
                UserContext user = UserContext.Current;
                if (user != null)
                {
                    if (user.SelectedOrganization != null)
                        return user.SelectedOrganization.ConnectionString;
                }
                if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Session != null && System.Web.HttpContext.Current.Session["OrgId"] != null)
                    OrgID = (Guid)System.Web.HttpContext.Current.Session["OrgId"];
            }
            else
            {
                byte[] _guid = OrgID.ToByteArray();
                if (_guid[0] != 0 && _guid[1] == 0 && _guid[2] == 0 && _guid[15] == 0)
                {
                    if (ConfigurationManager.ConnectionStrings["bigWebDesk.ConnectionString" + _guid[0].ToString()] != null)
                    {
                        if (!string.IsNullOrEmpty(ConfigurationManager.ConnectionStrings["bigWebDesk.ConnectionString" + _guid[0].ToString()].ConnectionString))
                            return ConfigurationManager.ConnectionStrings["bigWebDesk.ConnectionString" + _guid[0].ToString()].ConnectionString;
                    }
                }
            }

            string conn = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(OrgID, false);
            if (string.IsNullOrEmpty(conn))
            {
                if (ConfigurationManager.ConnectionStrings["bigWebDesk.ConnectionString"] != null)
                    conn = ConfigurationManager.ConnectionStrings["bigWebDesk.ConnectionString"].ConnectionString;
            }

            return conn;
        }


        protected static DataTable SelectRecords(string StoredProcName)
        {
            return SelectRecords(StoredProcName, Guid.Empty);
        }

        protected static DataTable SelectRecords(string StoredProcName, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                DataTable tb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(connection.CreateCommand());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.CommandText = StoredProcName;
                da.Fill(tb);
                return tb;
            }
        }

        public static DataTable SelectRecords(string StoredProcName, SqlParameter[] param)
        {
            return SelectRecords(StoredProcName, param, Guid.Empty);
        }

        protected static DataTable SelectRecords(string StoredProcName, SqlParameter[] param, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                DataTable tb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(connection.CreateCommand());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.CommandText = StoredProcName;
                da.SelectCommand.Parameters.AddRange(param);
                da.Fill(tb);
                da.SelectCommand.Parameters.Clear();
                return tb;
            }
        }

        protected static DataRow SelectRecord(string StoredProcName, SqlParameter param)
        {
            return SelectRecord(StoredProcName, new SqlParameter[] { param });
        }

        protected static DataRow SelectRecord(string StoredProcName, SqlParameter[] param)
        {
            return SelectRecord(StoredProcName, param, Guid.Empty);
        }

        protected static DataRow SelectRecord(string StoredProcName, SqlParameter[] param, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                DataTable tb = new DataTable();
                SqlDataAdapter da = new SqlDataAdapter(connection.CreateCommand());
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.CommandText = StoredProcName;
                da.SelectCommand.Parameters.AddRange(param);
                da.Fill(tb);
                da.SelectCommand.Parameters.Clear();
                if (tb.Rows.Count > 0) return tb.Rows[0];
                else return null;
            }
        }

        public static DataTable SelectByQuery(string Query)
        {
            return SelectByQuery(Query, Guid.Empty);
        }

        public static DataTable SelectByQuery(string Query, Guid OrgID)
        {
            try
            {
                using (SqlConnection connection =
               GetDbConnection(OrgID))
                {
                    SqlDataAdapter da = new SqlDataAdapter(connection.CreateCommand());
                    da.SelectCommand.CommandType = CommandType.Text;
                    da.SelectCommand.CommandText = Query;
                    DataTable tb = new DataTable();
                    da.Fill(tb);
                    return tb;
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(
                    string.Format("{0} The following statement has been executed: {1}.", ex.Message, Query),
                    ex);
            }
        }

        public static int UpdateByQuery(string Query)
        {
            return UpdateByQuery(Query, null, Guid.Empty);
        }

        public static int UpdateByQuery(string Query, SqlParameter[] param)
        {
            return UpdateByQuery(Query, param, Guid.Empty);
        }

        protected static int UpdateByQuery(string Query, Guid OrgID)
        {
            return UpdateByQuery(Query, null, OrgID);
        }


        protected static int UpdateByQuery(string Query, SqlParameter[] param, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = Query;
                if (param != null && param.Length > 0)
                    cmd.Parameters.AddRange(param);
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
                int _res = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Connection.Close();
                return _res;
            }
        }
        /*
        public static SqlCommand CreateSqlCommand(string StoredProcName, SqlParameter[] param)
        {
            return CreateSqlCommand(StoredProcName, param, Guid.Empty);
        }

        protected static SqlCommand CreateSqlCommand(string StoredProcName, SqlParameter[] param, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = StoredProcName;
                cmd.Parameters.AddRange(param);
                return cmd;
            }
        }
        */
        public static SqlCommand CreateSqlCommand(string StoredProcName, SqlParameter[] param)
        {
            return CreateSqlCommand(StoredProcName, param, Guid.Empty);
        }

        protected static SqlCommand CreateSqlCommand(string StoredProcName, SqlParameter[] param, Guid OrgID)
        {
            SqlCommand cmd = GetDbConnection(OrgID).CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = StoredProcName;
            cmd.Parameters.AddRange(param);
            return cmd;
        }

        //MRUDKOVSKI: 09-JUL-2005 - changed protected to public
        public static int UpdateData(string StoredProcName, SqlParameter param)
        {
            return UpdateData(StoredProcName, new SqlParameter[] { param });
        }

        public static int UpdateData(string StoredProcName, SqlParameter[] param)
        {
            return UpdateData(StoredProcName, param, Guid.Empty);
        }

        //MRUDKOVSKI: 09-JUL-2005 - changed protected to public
        protected static int UpdateData(string StoredProcName, SqlParameter[] param, Guid OrgID)
        {
            using (SqlConnection connection =
               GetDbConnection(OrgID))
            {
                SqlCommand cmd = connection.CreateCommand();
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = StoredProcName;
                cmd.Parameters.AddRange(param);
                int _res = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Connection.Close();
                return _res;
            }
        }
    }
}
