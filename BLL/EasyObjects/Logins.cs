using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using BWA.bigWebDesk.DAL;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;

namespace BWA.bigWebDesk.BLL
{
    public class Logins : BWA.bigWebDesk.DAL.tbl_Logins
    {
        static string ERR_DUPLICATE_LOGIN = "Login {0} already exists in database.";

        public Logins(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static bool LoginExists(Guid organizationId, string emailAddress)
        {
            Logins lg = new Logins(organizationId);
            lg.Where.Email.Value = emailAddress;
            lg.Where.Email.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lg.Query.Load();
            return (lg.RowCount > 0);
        }

        public static int GetLoginID(Guid organizationId, string emailAddress)
        {
            Logins lg = new Logins(organizationId);
            lg.Where.Email.Value = emailAddress;
            lg.Where.Email.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lg.Query.Load();
            return (lg.RowCount > 0 ? lg.Id : 0);
        }

        public static string GetLoginEmail(Guid organizationId, int ID)
        {
            Logins lg = new Logins(organizationId);
            return lg.LoadByPrimaryKey(ID) ? lg.Email : string.Empty;
        }

        public static String GetLoginPassword(Guid organizationId, string emailAddress)
        {
            Logins lg = new Logins(organizationId);
            lg.Where.Email.Value = emailAddress;
            lg.Where.Email.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lg.Query.Load();
            return (lg.RowCount > 0 ? lg.Password : String.Empty);
        }

        public static Logins GetLoginData(Guid organizationId, string emailAddress)
        {
            Logins lg = new Logins(organizationId);
            lg.Where.Email.Value = emailAddress;
            lg.Where.Email.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lg.Query.Load();
            return (lg.RowCount > 0 ? lg : null);
        }

        // for external use - web service
        public static DataSet GetLoginData(Guid organizationId, int loginID)
        {
            Logins lg = new Logins(organizationId);
            lg.Where.Id.Value = loginID;
            lg.Where.Id.Operator = WhereParameter.Operand.Equal;
            lg.Query.AddResultColumn(tbl_LoginsSchema.FirstName);
            lg.Query.AddResultColumn(tbl_LoginsSchema.LastName);
            lg.Query.AddResultColumn(tbl_LoginsSchema.Email);
            lg.Query.Load();
            if (lg.DefaultView.Table.Rows.Count > 0)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(lg.DefaultView.Table);
                return ds;
            }
            return null;
        }

        public static int AddLogin(Guid organizationId, string firstName,
                                   string lastName,
                                   string emailAddress)
        {
            string password = bigWebApps.bigWebDesk.Data.Logins.GenerateRandomPassword();

            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(emailAddress, firstName, lastName, null, null, null,
                        null, null, null, null, null, null, null, null, null, (string)null, organizationId, password, false, false);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            try
            {

                Logins lg = new Logins(organizationId);
                lg.AddNew();
                lg.FirstName = firstName;
                lg.LastName = lastName;
                lg.Email = emailAddress;
                lg.Password = password;
                lg.ConfigUnassignedQue = false;
                lg.Save();

                if (error != null)
                    throw error;

                return lg.Id;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case 2601:
                        throw new DuplicateLoginException(String.Format(ERR_DUPLICATE_LOGIN, emailAddress), ex);
                    default:
                        throw new ApplicationException(ex.Message, ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public virtual string UpdateLogin(
                        int code,
                        int DId,
                        int UId,
                        string user_email_old,
                        string user_password,
                        string user_firstname,
                        string user_lastname,
                        string user_title,
                        string user_email,
                        string Phone,
                        string MobilePhone,
                        int location_id,
                        string user_room,
                        int intUserType,
                        byte tintLevel,
                        string user_note,
                        bool btUpdateAcct,
                        int intAcctId,
                        int intAcctLocId,
                        int intSupGroupId,
                        bool btCallCentreRep,
                        string vchOrganization,
                        string LdapUserSID,
                        string LdapUserAccount
            )
        {
            string _result = string.Empty;
            MC3DeptInfo _instInfo = GetInstanceInfoByDepartmentId(DId);

            Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
            Guid _loginId = _login.GetLoginId(user_email_old);

            System.Collections.ArrayList groupIdList = bigWebApps.bigWebDesk.Data.Logins.GetUserGroups(_instInfo.OrganizationId, _instInfo.InstanceId, intUserType, _loginId);

            Exception error = null;

            try
            {
                if (_loginId == Guid.Empty)
                    Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(user_email_old, user_email, user_firstname, user_lastname, null, Phone, MobilePhone, null, user_title, null, null, null, null, null, null, null, Micajah.Common.Bll.Support.ConvertListToString(groupIdList), _instInfo.OrganizationId, user_password, false, false);
                else
                {
                    Micajah.Common.Bll.Providers.UserProvider.UpdateUser(_loginId, user_email, user_firstname, user_lastname, null, Phone, MobilePhone, null, user_title, null, null, null, null, null, null, null, null, null, null, groupIdList, _instInfo.OrganizationId, false);

                    if (!string.IsNullOrEmpty(user_password))
                    {
                        _login.ChangePassword(_loginId, user_password);
                    }
                }
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_UpdateLogin";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "code", DbType.Int32, code);
            db.AddInParameter(dbCommand, "DId", DbType.Int32, DId);
            db.AddInParameter(dbCommand, "UId", DbType.Int32, UId);
            db.AddInParameter(dbCommand, "user_password", DbType.AnsiString, user_password);
            db.AddInParameter(dbCommand, "user_firstname", DbType.AnsiString, user_firstname);
            db.AddInParameter(dbCommand, "user_lastname", DbType.AnsiString, user_lastname);
            db.AddInParameter(dbCommand, "user_title", DbType.AnsiString, user_title);
            db.AddInParameter(dbCommand, "user_email", DbType.AnsiString, user_email);
            db.AddInParameter(dbCommand, "Phone", DbType.AnsiString, Phone);
            db.AddInParameter(dbCommand, "MobilePhone", DbType.AnsiString, MobilePhone);
            db.AddInParameter(dbCommand, "location_id", DbType.Int32, location_id);

            if (user_room.Length > 0)
                db.AddInParameter(dbCommand, "user_room", DbType.AnsiString, user_room);
            else
                db.AddInParameter(dbCommand, "user_room", DbType.AnsiString, DBNull);

            db.AddInParameter(dbCommand, "intUserType", DbType.Int32, intUserType);

            if (tintLevel > 0)
                db.AddInParameter(dbCommand, "tintLevel", DbType.Byte, tintLevel);
            else
                db.AddInParameter(dbCommand, "tintLevel", DbType.Byte, DBNull);

            db.AddInParameter(dbCommand, "user_note", DbType.AnsiString, user_note);
            db.AddInParameter(dbCommand, "btUpdateAcct", DbType.Boolean, btUpdateAcct);

            if (intAcctId > 0)
                db.AddInParameter(dbCommand, "intAcctId", DbType.Int32, intAcctId);
            else
                db.AddInParameter(dbCommand, "intAcctId", DbType.Int32, DBNull);

            if (intAcctLocId > 0)
                db.AddInParameter(dbCommand, "intAcctLocId", DbType.Int32, intAcctLocId);
            else
                db.AddInParameter(dbCommand, "intAcctLocId", DbType.Int32, DBNull);

            if (intSupGroupId > 0)
                db.AddInParameter(dbCommand, "intSupGroupId", DbType.Int32, intSupGroupId);
            else
                db.AddInParameter(dbCommand, "intSupGroupId", DbType.Int32, DBNull);

            db.AddInParameter(dbCommand, "btCallCentreRep", DbType.Boolean, btCallCentreRep);

            if (vchOrganization.Length > 0)
                db.AddInParameter(dbCommand, "vchOrganization", DbType.AnsiString, vchOrganization);
            else
                db.AddInParameter(dbCommand, "vchOrganization", DbType.AnsiString, DBNull);

            if (LdapUserSID.Length > 0)
                db.AddInParameter(dbCommand, "LdapUserSID", DbType.AnsiString, LdapUserSID);
            else
                db.AddInParameter(dbCommand, "LdapUserSID", DbType.AnsiString, DBNull);

            if (LdapUserAccount.Length > 0)
                db.AddInParameter(dbCommand, "LdapUserAccount", DbType.AnsiString, LdapUserAccount);
            else
                db.AddInParameter(dbCommand, "LdapUserAccount", DbType.AnsiString, DBNull);

            //NOT SUPPORTED BY EASY OBJECTS
            //db.AddOutParameter(dbCommand, "@RETURN_VALUE", DbType.Int32, DBNull);

            base.LoadFromSqlNoExec(dbCommand);


            /*
            int ErrorCode=0;

            string _error_code = db.GetParameterValue(dbCommand, "@RETURN_VALUE").ToString();
            if (_error_code.Length > 0)
                ErrorCode = Int32.Parse(_error_code);
            else
                ErrorCode = 0;

            if (ErrorCode == 1)
                _result = "Email adress: " + user_email +" already exist.";
            */

            if (error != null)
                throw error;

            return _result;
        }

        private class MC3DeptInfo
        {
            public Guid OrganizationId = Guid.Empty;
            public Guid InstanceId = Guid.Empty;

            public MC3DeptInfo()
            {
            }
            public MC3DeptInfo(Guid org_id, Guid inst_id)
            {
                OrganizationId = org_id;
                InstanceId = inst_id;
            }
        }

        private MC3DeptInfo GetInstanceInfoByDepartmentId(int department_id)
        {
            MC3DeptInfo _result = new MC3DeptInfo();

            string sqlCommand = "SELECT company_guid FROM tbl_company WHERE company_id=" + department_id.ToString();
            Database db = GetDatabase();
            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);
            IDataReader _reader = base.LoadFromSqlReader(dbCommand);

            while (_reader.Read())
            {
                _result.InstanceId = (Guid)_reader[0];
                break;
            }
            _reader.Close();

            if (_result.InstanceId == Guid.Empty) return _result;

            sqlCommand = "SELECT OrganizationId FROM Mc_Instance WHERE InstanceId='" + _result.InstanceId.ToString() + "'";
            dbCommand = db.GetSqlStringCommand(sqlCommand);
            _reader = base.LoadFromSqlReader(dbCommand);
            while (_reader.Read())
            {
                _result.OrganizationId = (Guid)_reader[0];
                break;
            }
            _reader.Close();

            return _result;
        }

        public virtual int IsLoginExist(string user_sid, string user_name, string user_password, bool is_exist, int user_external_id)
        {
            int _result = -1;

            Database db = GetDatabase();

            string sqlCommand = string.Empty;

            if (user_name.Length > 0)
            {
                Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
                if (!_login.LoginNameExists(user_name)) return _result;

                if (!is_exist)
                    sqlCommand = "Select Id from tbl_Logins where Email = '" + user_name.Replace("'", "''") + "' AND Password = '" + user_password.Replace("'", "''") + "'";
                else
                    sqlCommand = "Select Id from tbl_Logins where Email = '" + user_name.Replace("'", "''") + "'";

                if (user_external_id > 0)
                    sqlCommand += " AND Id = " + user_external_id.ToString();

                DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

                IDataReader _reader = base.LoadFromSqlReader(dbCommand);

                if (_reader != null)
                {
                    while (_reader.Read())
                    {
                        _result = (int)_reader[0];
                        break;
                    };

                    _reader.Close();
                }
            }

            if ((is_exist) && (_result == -1))
            {
                if (user_sid.Length > 0)
                {
                    sqlCommand = "Select Id from tbl_Logins where LdapUserSID = '" + user_sid + "'";
                    DbCommand _ldap_command = db.GetSqlStringCommand(sqlCommand);

                    IDataReader _ldap_reader = base.LoadFromSqlReader(_ldap_command);

                    if (_ldap_reader != null)
                    {
                        while (_ldap_reader.Read())
                        {
                            _result = (int)_ldap_reader[0];
                            break;
                        };

                        _ldap_reader.Close();
                    };

                };
            };

            return _result;
        }

        public virtual bool IsHaveLogonOrganizations(int LoginId)
        {
            bool _result = false;

            if (LoginId < 0)
                return _result;

            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_SelectLoginChooseOrganization";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "LoginId", DbType.Int32, LoginId);

            IDataReader _reader = base.LoadFromSqlReader(dbCommand);
            if (_reader != null)
            {
                while (_reader.Read())
                {
                    _result = true;
                    break;
                };

                _reader.Close();
            };

            return _result;
        }

        public virtual IDataReader SelectLoginDetailbyEmail(string Email)
        {
            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_SelectLoginDetailbyEmail";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "Email", DbType.AnsiString, Email);

            return base.LoadFromSqlReader(dbCommand);
        }

        public virtual IDataReader SelectLoginDetailbyId(int login_id)
        {
            Database db = GetDatabase();

            string sqlCommand = string.Empty;

            sqlCommand = "SELECT id, Email, FirstName, LastName, Password, Title, Phone, MobilePhone, LdapUserSID, LdapUserAccount FROM tbl_Logins WHERE ConfigUnassignedQue = 0 AND Id = " + login_id.ToString();

            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            return base.LoadFromSqlReader(dbCommand);
        }

        public virtual IDataReader SelectLoginCompanyDetails(int department_id, int login_id)
        {
            Database db = GetDatabase();

            string sqlCommand = string.Empty;

            sqlCommand = "SELECT LJ.Id, LocationId, room, usertype_id, tintLevel, notes, SupGroupId, btCfgCCRep, vchOrganization FROM tbl_LoginCompanyJunc LJ LEFT OUTER JOIN tbl_Logins L ON LJ.Login_id = L.id WHERE LJ.company_id = " + department_id.ToString() + " AND LJ.login_Id = " + login_id.ToString();

            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            return base.LoadFromSqlReader(dbCommand);
        }

        public virtual bool InactivateLogin
        (
                int UId,
                int DId,
                bool btAllTkt,
                string user_name
        )
        {
            bool _result = true;

            MC3DeptInfo _instInfo = GetInstanceInfoByDepartmentId(DId);
            if (_instInfo.OrganizationId == Guid.Empty) return false;

            Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
            Guid _userId = _login.GetLoginId(user_name);
            if (_userId == Guid.Empty) return false;

            Micajah.Common.Bll.Organization _org = Micajah.Common.Bll.Providers.OrganizationProvider.GetOrganization(_instInfo.OrganizationId);
            if (_org == null) return false;
            Micajah.Common.Bll.Providers.UserProvider.UpdateUserActive(_userId, _org.OrganizationId, false);

            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_InactivateLogin";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "UId", DbType.Int32, UId);
            db.AddInParameter(dbCommand, "DId", DbType.Int32, DId);
            db.AddInParameter(dbCommand, "btAllTkt", DbType.Boolean, btAllTkt);

            base.LoadFromSqlNoExec(dbCommand);

            /*
            int ErrorCode = 0;
			
            string _error_code = db.GetParameterValue(dbCommand, "@RETURN_VALUE").ToString();
            if (_error_code.Length > 0)
                ErrorCode = Int32.Parse(_error_code);
            else
                ErrorCode = 0;

            if (ErrorCode == 1)
                _result = true;
            */

            return _result;
        }

        public virtual void InsertLoginToken
       (
            int Token,
            string Seed
       )
        {
            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_InsertLoginToken";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "Token", DbType.Int32, Token);
            db.AddInParameter(dbCommand, "Seed", DbType.AnsiString, Seed);

            base.LoadFromSqlNoExec(dbCommand);
        }

        public virtual string GetLoginPassword(int login_id)
        {
            string _result = string.Empty;

            Database db = GetDatabase();

            string sqlCommand = string.Empty;

            sqlCommand = "Select Password from tbl_Logins where Id = " + login_id.ToString();

            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            IDataReader _reader = base.LoadFromSqlReader(dbCommand);

            if (_reader != null)
            {
                while (_reader.Read())
                {
                    if (!_reader.IsDBNull(0))
                        _result = _reader[0].ToString();

                    break;
                };

                _reader.Close();
            };

            return _result;
        }

        public virtual int IsLoginActive(int department_id, int user_id, string user_name)
        {
            int _result = -1;

            Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
            Guid _loginId = _login.GetLoginId(user_name);
            if (_loginId == Guid.Empty) return _result;

            MC3DeptInfo _instInfo = GetInstanceInfoByDepartmentId(department_id);

            if (_login.LoginIsActiveInOrganization(_loginId, _instInfo.OrganizationId))
                _result = 1;
            else
                _result = 0;

            Database db = GetDatabase();

            string sqlCommand = "SELECT btUserInactive FROM tbl_LoginCompanyJunc WHERE company_id =" + department_id.ToString() + " AND id =" + user_id.ToString();

            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            IDataReader _reader = base.LoadFromSqlReader(dbCommand);

            if (_reader != null)
            {
                while (_reader.Read())
                {
                    bool _res = (bool)_reader[0];
                    if (!_res)
                        _result = 1;
                    else
                        _result = 0;

                    break;
                };

                _reader.Close();
            };

            return _result;
        }

        /// <summary>
        /// Returns bwd logins with their departments
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns>datatable with the following columns: "Id", "LdapUserAccount", "CompanyId"</returns>
        public DataTable GetDomainAccounts(string domainName)
        {
            DataTable dt = new DataTable("Logins");
            dt.Columns.Add("Id", typeof(int));
            dt.Columns.Add("LdapUserAccount", typeof(string));
            dt.Columns.Add("CompanyId", typeof(int));
            Database db = GetDatabase();
            string sqlCommand = string.Format("SELECT l.id as Id, l.LdapUserAccount as LdapUserAccount, lc.company_id as CompanyId, l.LdapUserSID as LdapUserSID, (lc.btUserInactive + 1)%2 as IsActive " +
        "FROM TBL_LOGINS l " +
        "inner join tbl_LoginCompanyJunc lc on lc.Login_Id = l.Id " +
        "WHERE LdapUserAccount LIKE '{0}\\%'", domainName);
            DataSet dataset = new DataSet();
            dataset.Tables.Add(dt);
            db.LoadDataSet(CommandType.Text, sqlCommand, dataset, new string[] { "Logins" });
            return dataset.Tables["Logins"];
        }
    }
}
