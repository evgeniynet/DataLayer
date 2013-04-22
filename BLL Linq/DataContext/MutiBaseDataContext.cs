using System;
using System.Data;
using System.Data.SqlClient;
using Micajah.Common.Security;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace System.Linq
{
    public static class MutiBaseQueryable
    {
        public static IQueryable<TSource> ImmediateLoad<TSource>(this IQueryable<TSource> source)
        {
            object o;
            foreach (TSource s in source) o = s;
            return source;
        }
        public static System.Data.Linq.ISingleResult<TSource> ImmediateLoad<TSource>(this System.Data.Linq.ISingleResult<TSource> source)
        {
            object o;
            foreach (TSource s in source) o = s;
            return source;
        }
        public static TSource FirstOrNull<TSource>(this IEnumerable<TSource> source) where TSource:class
        {
            foreach (TSource s in source) return s;
            return null;
        }

        private static int Delete(System.Data.Common.DbCommand c)
        {
            //"SELECT [t0].[CompanyID], [t0].[AccessKeyID], [t0].[UserID], [t0].[AccessKeyTypeID], [t0].[PublicAccessKey], [t0].[PrivateAccessKey]\r\nFROM [dbo].[AccessKey] AS [t0]\r\nWHERE [t0].[AccessKeyID] = @p0"
            string t = c.CommandText.ToLower();

            int n1 = t.IndexOf("from ");
            if (n1 < 1) throw new Exception("Can execute DELETE LINQ statement!");

            int n2 = t.IndexOf(" as ",n1);
            if (n2 < 1) throw new Exception("Can execute DELETE LINQ statement!");

            int n3 = t.IndexOf("where ",n1);
            if (n3 < 1) throw new Exception("Can execute DELETE LINQ statement!");

            string From = c.CommandText.Substring(n1,n2-n1)+" ";
            string Where = c.CommandText.Substring(n3).Replace("[t0].",string.Empty);

            c.CommandText = "DELETE " + From + Where;

            bool NeedOpenConnection = (c.Connection.State & ConnectionState.Open) == 0;
            try
            {
                if(NeedOpenConnection) c.Connection.Open();
                c.ExecuteNonQuery();
            }
            finally 
            {
                if(NeedOpenConnection) c.Connection.Close(); 
            }           
            return 0;
        }

        public static int Delete<TEntity>(this System.Data.Linq.Table<TEntity> source, System.Linq.Expressions.Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            System.Data.Linq.DataContext dc = source.Context;
            System.Data.Common.DbCommand c = dc.GetCommand(source.Where(predicate));
            return Delete(c);
        }

        public static int Delete<TEntity>(this System.Data.Linq.Table<TEntity> source, System.Linq.Expressions.Expression<Func<TEntity, int, bool>> predicate) where TEntity : class
        {
            System.Data.Linq.DataContext dc = source.Context;
            System.Data.Common.DbCommand c = dc.GetCommand(source.Where(predicate));
            return Delete(c);
        }
    }
}
namespace lib.bwa.bigWebDesk.LinqBll.Context
{
    public class MutiBaseDataContext : LinqDBDataContext
    {
        public Guid? OrganizationId {get; private set;}
        public Guid? InstaceId {get; private set;}
        public int? DepartmentId { get; set;}

        readonly bool UseExternalConnection;
        const string ExternalTransactionName = "MutiBaseDataContextTransaction";
        SqlConnection ExternalConnection;
        SqlTransaction ExternalTransaction;

        struct stConnectionInfo
        {
            public SqlConnection ExternalConnection;
            public SqlTransaction ExternalTransaction;
            public string ConnectionString;
            public Guid OrganizationId;
            public Guid InstanceId;
            //public int DepartmentId;
        }

        public enum TransactionMode { ImmediateOpenConnection }

        //static string MCConnectionString { get { return System.Configuration.ConfigurationManager.ConnectionStrings["Micajah.Common.ConnectionString"].ConnectionString; } }

        /*static string GetConnectionString(SqlDataReader McReader)
        {
            string Password = McReader["Password"].ToString();
            string UserName = McReader["UserName"].ToString();
            string ServerName = McReader["ServerName"].ToString();
            string DatabaseName = McReader["DatabaseName"].ToString();
            string Port = McReader["Port"].ToString();
            return "Data Source=" + ServerName + ";Initial Catalog=" + DatabaseName + ";Persist Security Info=True;User ID=" + UserName + ";Password=" + Password;
        }*/

        /*static stConnectionInfo GetConnection(string Where, string ParamName, object ParamValue, bool UseExternalConnection)
        {
            stConnectionInfo ci = new stConnectionInfo();

            using (SqlConnection McConnection = new SqlConnection(MCConnectionString))
            {
                McConnection.Open();
                using (SqlCommand McCommand = McConnection.CreateCommand())
                {
                    McCommand.CommandText = "SELECT o.OrganizationId, d.Password, d.UserName, s.Name as ServerName, d.Name as DatabaseName, s.Port " +
                                          "FROM Mc_Organization o " +
                                          "join Mc_Database d on o.DatabaseId=d.DatabaseId " +
                                          "join Mc_DatabaseServer s on d.DatabaseServerId=s.DatabaseServerId " +
                                          "where o.Active=1 and d.Deleted=0 and s.Deleted=0 and " + Where;
                    McCommand.CommandType = CommandType.Text;
                    McCommand.Parameters.Add(new SqlParameter(ParamName, ParamValue));
                    SqlDataReader McReader = McCommand.ExecuteReader();
                    if (!McReader.Read()) throw new Exception("Can not read Micajah Common Database");
                    ci.OrganizationId = (Guid)McReader["OrganizationId"];
                    ci.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(ci.OrganizationId);
                }
            }

            if (UseExternalConnection)
            {
                ci.ExternalConnection = new SqlConnection(ci.ConnectionString);
                ci.ExternalConnection.Open();
                ci.ExternalTransaction = ci.ExternalConnection.BeginTransaction(ExternalTransactionName);
            }

            return ci;
        }*/

        static stConnectionInfo GetConnection(string OrgSimpleId, bool UseExternalConnection)
        {
            stConnectionInfo ci = new stConnectionInfo();
            var org = Micajah.Common.Bll.Providers.OrganizationProvider.GetOrganizationByPseudoId(OrgSimpleId);
            if(org==null || org.OrganizationId==Guid.Empty) return ci;
            return GetConnection(org.OrganizationId, UseExternalConnection);
            //return GetConnection("o.SimpleId=@SimpleId", "@SimpleId", OrgSimpleId, UseExternalConnection);
        }

        static stConnectionInfo GetConnection(Guid OrgID, bool UseExternalConnection)
        {
            stConnectionInfo ci = new stConnectionInfo();
            ci.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(OrgID);
            ci.OrganizationId = OrgID;
            if (UseExternalConnection)
            {
                ci.ExternalConnection = new SqlConnection(ci.ConnectionString);
                ci.ExternalConnection.Open();
                ci.ExternalTransaction = ci.ExternalConnection.BeginTransaction(ExternalTransactionName);
            }
            return ci;
            //return GetConnection("o.OrganizationId=@OrganizationId", "@OrganizationId", OrgID, UseExternalConnection);
        }

        static System.Data.Linq.Mapping.MappingSource GetMapping()
        {
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetAssembly(typeof(MutiBaseDataContext));
            System.IO.Stream s = ass.GetManifestResourceStream("lib.bwa.bigWebDesk.LinqBll.DataContext.LinqDB.xml");
            System.Data.Linq.Mapping.XmlMappingSource ms = System.Data.Linq.Mapping.XmlMappingSource.FromStream(s);
            return ms;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                base.Dispose(disposing);
            }
            catch
            {
            }
            if (UseExternalConnection && disposing)
            {
                try
                {
                    ExternalTransaction.Rollback();
                    ExternalTransaction.Dispose();
                    ExternalConnection.Close();
                    ExternalConnection.Dispose();
                }
                catch
                {
                }
            }
        }

        public override void SubmitChanges(System.Data.Linq.ConflictMode failureMode)
        {
            base.SubmitChanges(failureMode);
            if (UseExternalConnection)
            {
                ExternalTransaction.Commit();
                ExternalTransaction = ExternalConnection.BeginTransaction(ExternalTransactionName);
                base.Transaction = ExternalTransaction;
            }
        }

        private MutiBaseDataContext(Guid OrgID, int? departmentId, stConnectionInfo ci, System.Data.Linq.Mapping.MappingSource map) :
            base(ci.ConnectionString, map)
        {
            OrganizationId = OrgID;
            DepartmentId = departmentId;
            UseExternalConnection = false;
        }

        private MutiBaseDataContext(Guid OrgID, int? departmentId, stConnectionInfo ci, System.Data.Linq.Mapping.MappingSource map, TransactionMode UseTransaction) :
            base(ci.ExternalConnection, map)
        {
            OrganizationId = OrgID;
            DepartmentId = departmentId;
            UseExternalConnection = true;

            ExternalConnection = ci.ExternalConnection;
            ExternalTransaction = ci.ExternalTransaction;
            base.Transaction = ExternalTransaction;
        }

        public MutiBaseDataContext(Guid OrgID, int? departmentId) :
            this(OrgID, departmentId, GetConnection(OrgID, false), GetMapping())
        { }

        public MutiBaseDataContext(Guid OrgID, int? departmentId, TransactionMode UseTransaction) :
            this(OrgID, departmentId, GetConnection(OrgID, true), GetMapping(), UseTransaction)
        { }

        static public MutiBaseDataContext Create(string OrgAlias, string InstAlias, Guid OrgGuid, Guid InstGuid)
        {
            stConnectionInfo ci = new stConnectionInfo();
            ci.ExternalConnection = null;
            ci.ExternalTransaction = null;

            if(OrgGuid==Guid.Empty)
            {
                var org = Micajah.Common.Bll.Providers.OrganizationProvider.GetOrganizationByPseudoId(OrgAlias);
                if (org == null || org.OrganizationId==Guid.Empty) return null;
                OrgGuid = org.OrganizationId;
            }
            ci.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(OrgGuid);
            ci.OrganizationId = OrgGuid;

            if (InstGuid == Guid.Empty)
            {
                var inst = Micajah.Common.Bll.Providers.InstanceProvider.GetInstanceByPseudoId(InstAlias, OrgGuid);
                if (inst == null || inst.InstanceId == Guid.Empty) return null;
                InstGuid = inst.InstanceId;
            }
            ci.InstanceId = InstGuid;

            MutiBaseDataContext dc = new MutiBaseDataContext(ci.OrganizationId, null, ci, GetMapping());
            int? CompanyID = null;
            var qq = from c in dc.Tbl_company where c.Company_guid == InstGuid select (int?)c.Company_id;
            foreach (var q in qq)
            {
                CompanyID = q;
            }
            dc.DepartmentId = CompanyID;
  


            /*MutiBaseDataContext dc = null;
            stConnectionInfo ci=new stConnectionInfo();
            if (OrgGuid != Guid.Empty) ci = GetConnection(OrgGuid, false);
            if (ci.OrganizationId != Guid.Empty && !string.IsNullOrEmpty(OrgAlias)) ci = GetConnection(OrgAlias, false);

            if (ci.OrganizationId == Guid.Empty && InstGuid!=Guid.Empty)
            {
                using (SqlConnection McConnection = new SqlConnection(MCConnectionString))
                {
                    McConnection.Open();
                    using (SqlCommand McCommand = McConnection.CreateCommand())
                    {
                        McCommand.CommandText = "SELECT distinct d.Password, d.UserName, s.Name as ServerName, d.Name as DatabaseName, s.Port " +
                                              "FROM Mc_Organization o " +
                                              "join Mc_Database d on o.DatabaseId=d.DatabaseId " +
                                              "join Mc_DatabaseServer s on d.DatabaseServerId=s.DatabaseServerId " +
                                              "where o.Active=1 and d.Deleted=0 and s.Deleted=0";
                        McCommand.CommandType = CommandType.Text;
                        SqlDataReader McReader = McCommand.ExecuteReader();
                        List<string> UsedConnectionStrings = new List<string>();

                        Guid FindOrganizationId = Guid.Empty;
                        string FindConnectionString = null;
                        int FindDepartmentId=0;

                        while (McReader.Read())
                        {
                            string NextConnectionString = GetConnectionString(McReader);
                            if (UsedConnectionStrings.Contains(NextConnectionString)) continue;
                            UsedConnectionStrings.Add(NextConnectionString);

                            //try
                            //{
                                using (SqlConnection Connection = new SqlConnection(NextConnectionString))
                                {
                                    Connection.Open();
                                    using (SqlCommand Command = Connection.CreateCommand())
                                    {
                                        Command.CommandType = CommandType.Text;
                                        Command.CommandText = "SELECT i.OrganizationId, c.company_id FROM Mc_Instance i join tbl_Company c on c.company_guid=i.InstanceId where i.InstanceId=@InstanceId and i.Deleted=0";
                                        Command.Parameters.Add(new SqlParameter("@InstanceId", InstGuid));
                                        SqlDataReader Reader = Command.ExecuteReader();
                                        if (!Reader.Read()) continue;
                                        Guid NextOrganizationId = (Guid)Reader["OrganizationId"];
                                        if (FindOrganizationId != Guid.Empty)
                                        {
                                            if (FindOrganizationId != NextOrganizationId) return null;
                                            continue;
                                        }
                                        FindOrganizationId = NextOrganizationId;
                                        FindConnectionString = NextConnectionString;
                                        FindDepartmentId=(int)Reader["company_id"];
                                    }
                                }
                            //}
                            //catch { }
                        }

                        ci.OrganizationId = FindOrganizationId;
                        ci.ConnectionString = FindConnectionString;
                        ci.InstanceId = InstGuid;
                        ci.DepartmentId = FindDepartmentId;
                    }
                }
            }

            if (ci.OrganizationId == Guid.Empty) return null;

            dc = new MutiBaseDataContext(ci.OrganizationId, null, ci, GetMapping());
            dc.OrganizationId = ci.OrganizationId;
            dc.DepartmentId = ci.DepartmentId;
            dc.InstaceId = ci.InstanceId;
            if(InstGuid!=Guid.Empty && ci.DepartmentId<1)
            {
                int? CompanyID=null;
                var qq = from c in dc.Tbl_company where c.Company_guid == InstGuid select (int?)c.Company_id;
                foreach (var q in qq)
                {
                    CompanyID = q;
                }
                dc.DepartmentId = CompanyID;
            }*/
            /*if (dc.DepartmentId == null && !string.IsNullOrEmpty(InstAlias))
            {
                int? CompanyID = null;
                var qq = from i in dc.Mc_Instance join c in dc.Tbl_company on i.InstanceId equals c.Company_guid where i.SimpleId == InstAlias select (int?)c.Company_id;
                foreach (var q in qq)
                {
                    CompanyID = q;
                }
                dc.DepartmentId = CompanyID;
            }*/

            return dc;
        }

    }

    public class AccountedData
    {
        private List<string> InitializedData;
        public AccountedData()
        {
            InitializedData = new List<string>();
        }
        public void AddData(string Name)
        {
            if (InitializedData.Contains(Name)) return;
            InitializedData.Add(Name);
        }
        public bool IsAdded(string Name)
        {
            return InitializedData.Contains(Name);
        }
    }
}