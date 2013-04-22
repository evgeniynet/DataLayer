using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Logins.
    /// </summary>
    public class Logins : DBAccess
    {
        public enum UserType
        {
            NotSet = 0,
            StandardUser = 1,
            Technician = 2,
            Administrator = 3,
            Queue = 4,
            SuperUser = 5
        }

        public enum ActiveStatus
        {
            NotSet = -1,
            Active = 0,
            InActive = 1
        }

        public enum UserLastLogin
        {
            NotSet = -1,
            Days30 = -30,
            Days60 = -60,
            Days90 = -90,
            Days180 = -180,
            Days365 = -365,
            Days730 = -730,
            Naver = -999
        }

        public enum EmailType
        {
            HTML = 0,
            Text = 1,
            ShortText = 2
        }

        public enum PasswordRetrievalResult
        {
            Ok, Failed, InvalidEmail, NoDeptsAssociated
        }

        public class Filter
        {
            private string _firstName = string.Empty;
            private string _lastName = string.Empty;
            private string _eMail = string.Empty;
            private int _locationID = 0;
            private int _accountID = 0;
            private int _acclocationID = 0;
            private UserType _userType = UserType.NotSet;
            private ActiveStatus _userStatus = ActiveStatus.Active;
            private UserLastLogin _userLastLogin = UserLastLogin.NotSet;
            private bool _useGlobalFilters = false;
            private bool _cfgLocationsEnabled = false;
            private bool _cfgAccountsEnabled = false;
            private string _search = string.Empty;
            private bool _searchAccountsToo = false;

            public Filter(string Search)
            {
                _search = Search;
            }

            public Filter(string FirstName, string LastName, string EMail)
            {
                _firstName = FirstName;
                _lastName = LastName;
                _eMail = EMail;
            }

            public Filter(string FirstName, string LastName, string EMail, int LocationID, int AccountID, int AccLocationID)
                : this(FirstName, LastName, EMail)
            {
                _locationID = LocationID;
                _accountID = AccountID;
                _acclocationID = AccLocationID;
            }

            public string FirstName
            {
                get { return _firstName; }
                set { _firstName = value; }
            }

            public string LastName
            {
                get { return _lastName; }
                set { _lastName = value; }
            }

            public string EMail
            {
                get { return _eMail; }
                set { _eMail = value; }
            }

            public int LocationID
            {
                get { return _locationID; }
                set { _locationID = value; }
            }

            public int AccountID
            {
                get { return _accountID; }
                set { _accountID = value; }
            }

            public int AccLocationID
            {
                get { return _acclocationID; }
                set { _acclocationID = value; }
            }

            public UserType Type
            {
                get { return _userType; }
                set { _userType = value; }
            }

            public ActiveStatus Status
            {
                get { return _userStatus; }
                set { _userStatus = value; }
            }

            public UserLastLogin LastLogin
            {
                get { return _userLastLogin; }
                set { _userLastLogin = value; }
            }

            public bool UseGlobalFilters
            {
                get { return _useGlobalFilters; }
                set { _useGlobalFilters = value; }
            }

            public bool ConfigLocationsEnabled
            {
                get { return _cfgLocationsEnabled; }
                set { _cfgLocationsEnabled = value; }
            }

            public bool ConfigAccountsEnabled
            {
                get { return _cfgAccountsEnabled; }
                set { _cfgAccountsEnabled = value; }
            }

            public string SearchString
            {
                get { return _search; }
                set { _search = value; }
            }

            public bool SearchAccountsToo
            {
                get { return _searchAccountsToo; }
                set { _searchAccountsToo = value; }
            }
        }

        public static System.Collections.ArrayList GetUserGroups(Guid orgId, Guid instId, int userTypeId, string loginName)
        {
            Micajah.Common.Bll.Providers.LoginProvider login = new Micajah.Common.Bll.Providers.LoginProvider();
            return GetUserGroups(orgId, instId, userTypeId, login.GetLoginId(loginName));
        }

        public static System.Collections.ArrayList GetUserGroups(Guid orgId, Guid instId, int userTypeId, Guid loginId)
        {
            // Gets the groups that's associated to the role in the instance.
            System.Collections.ArrayList roleGroupIdList = Micajah.Common.Bll.Providers.GroupProvider.GetGroupIdList(orgId, instId, GetRoleName((UserAuth.UserRole)userTypeId));
            System.Collections.ArrayList userGroupIdList = null;

            // Validates if the user already exists.
            if (loginId == Guid.Empty)
            {
                userGroupIdList = roleGroupIdList;
            }
            else
            {
                // Gets the groups of the user in the organization.
                userGroupIdList = Micajah.Common.Bll.Providers.UserProvider.GetUserGroupIdList(orgId, loginId);

                // Gets the groups of the user in the instance and removes their from the list.
                foreach (Guid gid in Micajah.Common.Bll.Providers.UserProvider.GetUserGroupIdList(orgId, instId, loginId))
                {
                    if (userGroupIdList.Contains(gid))
                        userGroupIdList.Remove(gid);
                }

                // Associates the user to the new groups in the instance.
                userGroupIdList.AddRange(roleGroupIdList);
            }

            return userGroupIdList;
        }

        public static string[] GetRoleName(params UserAuth.UserRole[] userTypeId)
        {
            System.Collections.Generic.List<string> _rl = new System.Collections.Generic.List<string>();
            for (int i = 0; i < userTypeId.Length; i++)
            {
                if (userTypeId[i] == UserAuth.UserRole.Administrator)
                {
                    _rl.Add("OrgAdmin");
                    _rl.Add("InstAdmin");
                }
                else _rl.Add(userTypeId[i].ToString());
            }
            return _rl.ToArray();
        }

        public static UserAuth.UserRole GetRole(string shortName)
        {
            UserAuth.UserRole result = UserAuth.UserRole.StandardUser;
            if (!Enum.TryParse<UserAuth.UserRole>(shortName, out result))
                result = UserAuth.UserRole.StandardUser;
            if (shortName == "OrgAdmin" || shortName == "InstAdmin")
                result = UserAuth.UserRole.Administrator;
            return result;
        }

        public static int SelectLastLoginDaysAgo(Guid OrgId, Guid InstId)
        {
            int DId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT TOP 1 ISNULL(DATEDIFF(DAY,dtLastLogin,GETUTCDATE()),-1) AS DaysAgo FROM tbl_LoginCompanyJunc WHERE company_id=" + DId.ToString() + " ORDER BY dtLastLogin DESC", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static int SelectLoginsCount(Guid OrgId, Guid InstId)
        {
            int DId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM tbl_LoginCompanyJunc WHERE company_id=" + DId.ToString() + " AND btUserInactive=0 AND UserType_Id<>4", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static int SelectTechsCount(Guid OrgId, Guid InstId)
        {
            int DId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM tbl_LoginCompanyJunc WHERE company_id=" + DId.ToString() + " AND btUserInactive=0 AND (UserType_Id=2 OR UserType_Id=3)", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static DataRow SelectEmailPrefs(int DeptID, int UserID)
        {
            return SelectRecord("sp_SelectEmailPref", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@UserId", UserID) });
        }

        public static DataTable SelectTechnicians(int DeptID)
        {
            return SelectTechnicians(Guid.Empty, DeptID);
        }

        public static DataTable SelectTechnicians(Guid OrgId, int DeptID)
        {
            return SelectRecords("sp_SelectTechs", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
        }

        /*
        public static DataTable SelectMailGroups(int DeptID)
        {
            return SelectRecords("sp_SelectMailGroups", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID) });
        }
        */

        public static DataTable SelectDomainUsers(string DomainName)
        {
            return SelectRecords("sp_SelectDomainUsers", new SqlParameter[] { new SqlParameter("@domainName", DomainName) });
        }

        public static DataTable SelectUserTypes()
        {
            return SelectRecords("sp_SelectUserTypes", new SqlParameter[] { });
        }

        public static DataTable SelectSuperUserUsersList(int DeptID, int UserId, int SuperUserTypeId, int AccountId, string RootLocationIdList)
        {

            SqlParameter _pSUTypeId = new SqlParameter("@tintSUserType", SqlDbType.TinyInt);
            _pSUTypeId.Value = SuperUserTypeId;

            SqlParameter _pAccId = new SqlParameter("@AccId", SqlDbType.Int);
            if (AccountId != 0) _pAccId.Value = AccountId;
            else _pAccId.Value = DBNull.Value;

            return SelectRecords("sp_SelectSuperUserUserList", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pSUTypeId, _pAccId, new SqlParameter("@vchUserRootLocationIdList", RootLocationIdList) });
        }

        public static DataTable SelectTechniciansHourlyRates(int DeptID)
        {
            return SelectRecords("sp_SelectTechsHourlyRates", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID) });
        }

        public static decimal SelectTechHourlyRate(int DepartmentID, int TechID, int taskTypeId)
        {
            return SelectTechHourlyRate(Guid.Empty, DepartmentID, TechID, taskTypeId);
        }

        public static decimal SelectTechHourlyRate(Guid OrgID, int DepartmentID, int TechID, int taskTypeId)
        {
            SqlParameter HourlyRate = new SqlParameter("@smHourlyRate", SqlDbType.SmallMoney);
            HourlyRate.Direction = ParameterDirection.Output;
            SqlCommand _cmd = CreateSqlCommand("sp_SelectTechHourlyRate", new SqlParameter[] { new SqlParameter("@DId", DepartmentID), new SqlParameter("@UId", TechID), new SqlParameter("@TaskTypeID", taskTypeId), HourlyRate }, OrgID);
            if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
            _cmd.ExecuteNonQuery();
            _cmd.Connection.Close();
            return HourlyRate.Value != DBNull.Value ? (decimal)HourlyRate.Value : 0;
        }

        public static DataTable SelectMailGroups(int DeptID)
        {
            return SelectRecords("sp_SelectMailGroups", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID) });
        }

        public static DataTable SelectTechsNoQues(int DeptID)
        {
            return SelectRecords("sp_SelectTechsNoQues", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }

        public static DataTable SelectAllUserList(int DeptID)
        {
            return SelectRecords("sp_alluserlist", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }

        public static DataTable SelectMailGroupUsers(int DeptID, int MailGroupID)
        {
            return SelectRecords("sp_SelectMailGroupUsers", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupId", MailGroupID) });
        }

        public static DataTable SelectMyParts(int DepartmentId, int UserId)
        {
            return SelectRecords("sp_SelectPartsMyParts", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@UId", UserId) });
        }

        public static DataTable SelectUserProfileTkts(int DepartmentId, int UserId)
        {
            return SelectRecords("sp_SelectUserProfileTkts", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@UId", UserId) });
        }

        public static DataRow SelectUserProfileAssets(int DepartmentId, int UserId)
        {
            return SelectRecord("sp_SelectUserProfileAssets", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@UId", UserId) });
        }

        public static DataTable SelectUserProfileCfgData(int DepartmentId, int UserId)
        {
            return SelectRecords("sp_SelectUserProfileCfgData", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@UId", UserId) });
        }

        public static DataRow SelectUserDetails(int DeptID, int UserID)
        {
            return SelectUserDetails(Guid.Empty, DeptID, UserID);
        }

        public static DataRow SelectUserDetails(Guid OrgID, int DeptID, int UserID)
        {
            return SelectRecord("sp_SelectUserDetails", new SqlParameter[] { new SqlParameter("@Did", DeptID), new SqlParameter("@Id", UserID) }, OrgID);
        }

        public static DataRow SelectLoginInfo(int DeptId, int LoginId)
        {
            return SelectRecord("sp_SelectLoginInfo", new SqlParameter[] { new SqlParameter("@DId", DeptId), new SqlParameter("@LId", LoginId) });
        }

        public static DataRow SelectLoginInfo(Guid OrgID, int DeptId, string Email)
        {
            SqlParameter _pLoginId = new SqlParameter("@LId", SqlDbType.Int);
            _pLoginId.Value = DBNull.Value;
            return SelectRecord("sp_SelectLoginInfo", new SqlParameter[] { new SqlParameter("@DId", DeptId), _pLoginId, new SqlParameter("@EMail", Email) }, OrgID);
        }

        public static int SelectLoginIdFromUserId(int DeptID, int UserID)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_SelectLoginIdFromUserId", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID) });

            return (int)_pRVAL.Value;
        }

        public static DataTable SelectSystemAdministrators(Guid OrgID, int DeptId)
        {
            return SelectRecords("sp_SelectSysAdmins", new SqlParameter[] { new SqlParameter("@DId", DeptId) }, OrgID);
        }

        //public static DataTable SelectCustomNames(int DeptID)
        //{
        //    return SelectCustomNames(-1, DeptID);
        //}

        public static DataTable SelectCustomNames(Guid OrgID, int DeptID)
        {
            return SelectRecords("sp_SelectCustomNames", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID) }, OrgID);
        }

        public static DataRow SelectLoginDetailsByEmail(string email)
        {
            return SelectLoginDetailsByEmail(Guid.Empty, email);
        }

        //public static DataRow SelectLoginDetailsByEmail(string email, ref int DbNumber)
        //{
        //    if (DbNumber<0) DbNumber = GetCurrentDbNumber();
        //    DataRow _res = SelectLoginDetailsByEmail(DbNumber, email);
        //    if (_res != null) return _res;
        //    foreach (int _db in GetDbNumbers())
        //    {
        //        if (_db == DbNumber) continue;
        //        _res=SelectLoginDetailsByEmail(_db, email);
        //        if (_res != null)
        //        {
        //            DbNumber = _db;
        //            return _res;
        //        }
        //    }
        //    return null;
        //}

        public static DataRow SelectLoginDetailsByEmail(Guid OrgID, string email)
        {
            return SelectRecord("sp_SelectLoginDetailbyEmail", new SqlParameter[] { new SqlParameter("@Email", email) }, OrgID);
        }

        public static int InactivateUser(int DeptID, int UserID, bool AllTkts)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_InactivateLogin", new SqlParameter[] { _pRVAL, new SqlParameter("@UId", UserID), new SqlParameter("@DId", DeptID), new SqlParameter("@btAllTkt", AllTkts) });
            return (int)_pRVAL.Value;
        }

        public static int TransferUser(int DeptID, string LoggedInUserName, int OldUserID, int NewUserID, bool AllTkts)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_TransferToActiveUser", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@old_user_id", OldUserID), new SqlParameter("@new_user_id", NewUserID), new SqlParameter("@btAllTkt", AllTkts), new SqlParameter("@LoggedInUserName", LoggedInUserName) });

            return (int)_pRVAL.Value;
        }

        public static int DemoteLogin(int DeptID, int UserID, int UserType, bool AllTkts)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DemoteLogin", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID), new SqlParameter("@intUserType", UserType), new SqlParameter("@btAllTkt", AllTkts) });

            return (int)_pRVAL.Value;
        }

        public static int SelectLoginExists(int DeptID, string EMail, out int UserID, out int AccountID)
        {
            return SelectLoginExists(Guid.Empty, DeptID, EMail, out UserID, out AccountID);
        }

        public static int SelectLoginExists(Guid OrgID, int DeptID, string EMail, out int UserID, out int AccountID)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            SqlParameter _pUId = new SqlParameter("@UId", SqlDbType.Int);
            SqlParameter _pAccId = new SqlParameter("@AcctId", SqlDbType.Int);

            _pRVAL.Direction = ParameterDirection.ReturnValue;
            _pUId.Direction = ParameterDirection.Output;
            _pAccId.Direction = ParameterDirection.Output;
            SqlCommand _cmd = CreateSqlCommand("sp_SelectLoginExists", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@vchEmailAddress", EMail), _pUId, _pAccId }, OrgID);
            if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
            _cmd.ExecuteNonQuery();
            _cmd.Connection.Close();
            if (_pUId.Value != DBNull.Value) UserID = (int)_pUId.Value;
            else UserID = 0;
            if (_pAccId.Value != DBNull.Value) AccountID = (int)_pAccId.Value;
            else AccountID = 0;
            return (int)_pRVAL.Value;
        }

        public static void UpdateUserLastLoginDate(Guid OrgId, int UId)
        {
            UpdateByQuery("UPDATE tbl_LoginCompanyJunc SET dtLastLogin = GETUTCDATE() WHERE id = " + UId.ToString(), OrgId);
        }

        public static void UpdateUserType(Guid OrgId, int UId, int UserType)
        {
            UpdateByQuery(string.Format(System.Globalization.CultureInfo.InvariantCulture, "UPDATE tbl_LoginCompanyJunc SET UserType_Id = {0} WHERE id = {1}", UserType, UId), OrgId);
        }

        public static string UpdateUserToken(Guid OrgID, int DeptId, int UserId, int Action)
        {
            string _result = string.Empty; //Initially the user is not authenticated

            if (DeptId >= 0)
            {
                //Fill the query parameters
                SqlParameter[] pars = new SqlParameter[4];
                pars[0] = new SqlParameter("@DId",
                    SqlDbType.Int,
                    4, /* Size */
                    ParameterDirection.Input,
                    false, /* is nullable */
                    0, /* byte precision */
                    0, /* byte scale */
                    string.Empty,
                    DataRowVersion.Default,
                    DeptId);
                pars[1] = new SqlParameter("@UId",
                    SqlDbType.Int,
                    4, /* Size */
                    ParameterDirection.Input,
                    false, /* is nullable */
                    0, /* byte precision */
                    0, /* byte scale */
                    string.Empty,
                    DataRowVersion.Default,
                    UserId);
                pars[2] = new SqlParameter("@Action",
                    SqlDbType.TinyInt,
                    1, /* Size */
                    ParameterDirection.Input,
                    false, /* is nullable */
                    0, /* byte precision */
                    0, /* byte scale */
                    string.Empty,
                    DataRowVersion.Default,
                    Action);
                pars[3] = new SqlParameter("@Token",
                    SqlDbType.BigInt,
                    8, /* Size */
                    ParameterDirection.Output,
                    false, /* is nullable */
                    0, /* byte precision */
                    0, /* byte scale */
                    string.Empty,
                    DataRowVersion.Default,
                    null);

                //Query authentication token from DB
                UpdateData("sp_UpdateUserToken", pars, OrgID);

                _result = pars[3].Value.ToString();
            }

            return _result;
        }

        public static void SelectLogOut(int departmentId, int userId, int loginToken, string loginSeed, out int loginId, out string email, out string password)
        {
            SqlParameter pLoginId = new SqlParameter("@LId", SqlDbType.Int);
            pLoginId.Direction = ParameterDirection.Output;

            SqlParameter pEmail = new SqlParameter("@vchEmail", SqlDbType.VarChar, 50);
            pEmail.Direction = ParameterDirection.Output;

            SqlParameter pPassword = new SqlParameter("@vchPass", SqlDbType.VarChar, 50);
            pPassword.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectLogOut", new SqlParameter[] {
			new SqlParameter("@DId", departmentId),
				new SqlParameter("@UId", userId),
				new SqlParameter("@intLoginToken", loginToken),
				new SqlParameter("@vchLoginSeed", loginSeed),
				pLoginId,
				pEmail,
				pPassword
			});
            loginId = pLoginId.Value != DBNull.Value ? (int)pLoginId.Value : 0;
            email = pEmail.Value != DBNull.Value ? (string)pEmail.Value : string.Empty;
            password = pPassword.Value != DBNull.Value ? (string)pPassword.Value : string.Empty;

        }

        public static DataTable SelectLogin(Guid OrgID, string EMail, string Token)
        {
            return SelectRecords("sp_SelectLogin", new SqlParameter[] { new SqlParameter("@Email", EMail), new SqlParameter("@LoginToken", Token) }, OrgID);
        }

        //public static int SelectLoginOrganizationCount(string email)
        //{
        //    int _res = 0;
        //    foreach (int _db in GetDbNumbers())
        //    {
        //        DataRow _row = SelectRecord("sp_SelectLoginOrganizationCount", new SqlParameter[] { new SqlParameter("@Email", email) });
        //        if (_row != null && !_row.IsNull("RecordCount")) _res += (int)_row["RecordCount"];
        //    }
        //    return _res;
        //}

        public static DataRow SelectDepartmentLogin(Guid OrgID, int DeptID, string DeptName)
        {
            return SelectRecord("sp_SelectDepartmentLogin", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@vchName", DeptName) }, OrgID);
        }

        //public static DataTable SelectDepartmentLogin(int DeptID, string DeptName)
        //{
        //    return SelectRecords("sp_SelectDepartmentLogin", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@vchName", DeptName) });
        //}

        public static DataRow SelectTechCheckInStatus(int DeptID, int UserId)
        {
            return SelectRecord("sp_SelectTechCheckInStatus", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@UserId", UserId) });
        }

        public static DataTable SelectUsersByFilter(int DeptID, int UserID, int TOPCount, Filter UsersFilter)
        {
            return SelectUsersByFilter(Guid.Empty, DeptID, UserID, TOPCount, UsersFilter);
        }

        public static DataTable SelectUsersByFilter(Guid OrgId, int DeptID, int UserID, int TOPCount, Filter UsersFilter)
        {
            string _sqlSelect = "SELECT " + (TOPCount >= 0 ? "TOP " + TOPCount.ToString() : string.Empty);
            string _sqlGroupBy = string.Empty;
            string _sqlOrderBy = string.Empty;
            if (UsersFilter.SearchAccountsToo)
            {
                _sqlSelect += " LJ.id AS Id, L.Email, LJ.id AS UserID, LJ.UserType_Id, L.FirstName, L.LastName, L.Title, dbo.fxGetUserName(L.FirstName, L.LastName, L.Email) AS UserFullName, UA.AccountId, dbo.fxGetUserName(L.FirstName, L.LastName, '') + ' - ' + CASE WHEN Accounts.DId IS NULL THEN ISNULL(tbl_company.company_name, '') + ' (Internal)' ELSE Accounts.vchName END + ' - ' + L.Email AS FullName, COUNT(tbl_ticket.id) AS TicketCount, ISNULL(UA.AccountLocationId, 0) AS AccountLocationId, ISNULL(dbo.fxGetUserLocationName(" + DeptID.ToString() + ", UA.AccountLocationId), '') AS AccountLocationName, dbo.fxGetUserLocationName(" + DeptID.ToString() + ", LJ.LocationId) AS DeptLocation, "
                    + " L.MobileEmail, CASE L.MobileEmailType WHEN 0 THEN 'HTML' WHEN 1 THEN 'Text' WHEN 2 THEN 'Short Text' ELSE 'Undefined' END AS MobileEmailTypeName "
                    + " FROM tbl_LoginCompanyJunc LJ INNER JOIN tbl_Logins L ON L.id = LJ.login_id INNER JOIN tbl_company ON LJ.company_id = tbl_company.company_id LEFT OUTER JOIN UserAccounts UA ON UA.DepartmentId = " + DeptID.ToString() + " AND UA.UserId = LJ.Id LEFT OUTER JOIN Accounts ON Accounts.DId=" + DeptID.ToString() + " AND Accounts.Id = UA.AccountId  LEFT OUTER JOIN tbl_ticket ON tbl_ticket.company_id = " + DeptID.ToString() + " AND tbl_ticket.user_id = LJ.id AND (tbl_ticket.intAcctId = UA.AccountId OR (tbl_ticket.intAcctId IS NULL AND UA.AccountId IS NULL))";
                _sqlGroupBy = " GROUP BY LJ.id, UA.AccountId, L.LastName, L.FirstName, Accounts.DId, tbl_company.company_name, Accounts.vchName, L.Email, LJ.UserType_Id, UA.AccountLocationId, UA.AccountLocationId";
                _sqlOrderBy = " ORDER BY L.LastName, L.FirstName, LJ.id, TicketCount DESC";
            }
            else
            {
                _sqlSelect += " LJ.id AS Id, L.Email, LJ.Id AS UserID, LJ.UserType_Id, L.FirstName, L.LastName, L.Title, dbo.fxGetUserName(L.FirstName, L.LastName, L.Email) AS UserFullName, dbo.fxGetFullUserName(L.FirstName, L.LastName, L.Email) AS FullName, LJ.LocationId, dbo.fxGetUserLocationName(" + DeptID.ToString() + ", LJ.LocationId) AS DeptLocation, "
                    + " L.MobileEmail, CASE L.MobileEmailType WHEN 0 THEN 'HTML' WHEN 1 THEN 'Text' WHEN 2 THEN 'Short Text' ELSE 'Undefined' END AS MobileEmailTypeName "
                    + " FROM tbl_LoginCompanyJunc LJ"
                    + " INNER JOIN tbl_Logins L ON L.id=LJ.login_id"
                    + " LEFT OUTER JOIN Locations DL ON DL.DId=" + DeptID.ToString() + " AND DL.Id=LJ.LocationId";
                _sqlOrderBy = " ORDER BY L.LastName, L.FirstName";
            }
            string _sqlWhere = " WHERE LJ.company_id=" + DeptID.ToString();
            if (UsersFilter.SearchString.Length > 0)
            {
                string _searchString = Security.SQLInjectionBlock(UsersFilter.SearchString.Replace("'", "''"));
                _sqlWhere += " AND (L.LastName+', '+L.FirstName LIKE '" + _searchString + "%'";
                _sqlWhere += " OR L.FirstName+' '+L.LastName LIKE '" + _searchString + "%'";
                _sqlWhere += " OR L.Email LIKE '" + _searchString + "%'";
                if (UsersFilter.SearchAccountsToo)
                {
                    _sqlWhere += " OR (Accounts.DId IS NULL AND ISNULL(tbl_company.company_name, '') + ' (Internal)' LIKE '" + _searchString + "%')";
                    _sqlWhere += " OR (Accounts.DId IS NOT NULL AND Accounts.vchName LIKE '" + _searchString + "%')";
                }
                _sqlWhere += ")";
            }
            else
            {
                if (UsersFilter.FirstName.Length > 0) _sqlWhere += " AND (" + OrLogic(UsersFilter.FirstName, "L.FirstName") + ")";
                if (UsersFilter.LastName.Length > 0) _sqlWhere += " AND (" + OrLogic(UsersFilter.LastName, "L.LastName") + ")";
                if (UsersFilter.EMail.Length > 0) _sqlWhere += " AND (" + OrLogic(UsersFilter.EMail, "L.Email") + ")";
                if (UsersFilter.LocationID != 0) _sqlWhere += " AND EXISTS(SELECT ULC.Id FROM UserLocations UL INNER JOIN dbo.fxGetAllChildLocations(" + DeptID.ToString() + ", " + UsersFilter.LocationID.ToString() + ") ULC ON UL.LocationId=ULC.Id WHERE UL.DId=" + DeptID.ToString() + " AND UL.UId=LJ.Id)";
                if (UsersFilter.AccountID > 0)
                {
                    if (UsersFilter.SearchAccountsToo) _sqlWhere += " AND UA.AccountId=" + UsersFilter.AccountID.ToString();
                    else _sqlWhere += " AND " + UsersFilter.AccountID.ToString() + " IN (SELECT AccountId FROM UserAccounts WHERE DepartmentId=" + DeptID.ToString() + " AND UserId=LJ.Id)";
                }
                else if (UsersFilter.AccountID == -1)
                {
                    if (UsersFilter.SearchAccountsToo) if (UsersFilter.SearchAccountsToo) _sqlWhere += " AND UA.AccountId IS NULL";
                        else _sqlWhere += " AND (NOT EXISTS(SELECT AccountId FROM UserAccounts WHERE DepartmentId=" + DeptID.ToString() + " AND UserId=LJ.Id) OR EXISTS(SELECT AccountId FROM UserAccounts WHERE DepartmentId=" + DeptID.ToString() + " AND UserId=LJ.Id AND AccountId IS NULL))";
                }
                if (UsersFilter.AccountID != 0 && UsersFilter.AccLocationID != 0)
                {
                    if (UsersFilter.AccountID > 0)
                        _sqlWhere += " AND EXISTS(SELECT ULC.Id FROM UserAccounts UA INNER JOIN dbo.fxGetAllChildLocations(" + DeptID.ToString() + ", " + UsersFilter.AccLocationID.ToString() + ") ULC ON UA.AccountLocationId=ULC.Id WHERE UA.DepartmentId=" + DeptID.ToString() + " AND UA.AccountId=" + UsersFilter.AccountID.ToString() + " AND UA.UserId=LJ.Id)";
                    else if (UsersFilter.AccountID == -1) _sqlWhere += " AND EXISTS(SELECT ULC.Id FROM UserLocations UL INNER JOIN dbo.fxGetAllChildLocations(" + DeptID.ToString() + ", " + UsersFilter.AccLocationID.ToString() + ") ULC ON UL.LocationId=ULC.Id WHERE UL.DId=" + DeptID.ToString() + " AND UL.UId=LJ.Id)";
                }
            }
            if (UsersFilter.Type != UserType.NotSet) _sqlWhere += " AND LJ.UserType_Id=" + ((int)UsersFilter.Type).ToString();
            else _sqlWhere += " AND LJ.UserType_Id<>4";
            if (UsersFilter.Status != ActiveStatus.NotSet) _sqlWhere += " AND LJ.btUserInactive=" + ((int)UsersFilter.Status).ToString();
            if (UsersFilter.LastLogin != UserLastLogin.NotSet) _sqlWhere += " AND LJ.dtlastLogin<DATEADD(day," + ((int)UsersFilter.LastLogin).ToString() + ", GETUTCDATE())";

            //Check Global Filters
            if (UserID != 0 && UsersFilter.UseGlobalFilters)
            {
                GlobalFilters _gf = new GlobalFilters(OrgId, DeptID, UserID);
                string _gfWhere = string.Empty;
                /*if (UsersFilter.ConfigLocationsEnabled && _gf.IsFilterEnabled(GlobalFilters.FilterState.EnabledGlobalFilters) && _gf.IsFilterEnabled(GlobalFilters.FilterType.Locations))
                {
                    string _strLoc = GlobalFilters.SelectFilterByTypeToString(DeptID, UserID, GlobalFilters.FilterType.Locations);
                    if (_strLoc.Length > 0) _gfWhere += "(EXISTS(SELECT Id FROM UserLocations WHERE DId=" + DeptID.ToString() + " AND UId=LJ.Id AND LocationId IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList(" + DeptID.ToString() + ", '" + _strLoc + "'))) OR LJ.LocationId IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList(" + DeptID.ToString() + ", '" + _strLoc + "'))";
                }*/
                if (UsersFilter.ConfigAccountsEnabled && _gf.IsFilterEnabled(GlobalFilters.FilterState.EnabledGlobalFilters) && _gf.IsFilterEnabled(GlobalFilters.FilterType.Accounts))
                {
                    string _strAcc = GlobalFilters.SelectFilterByTypeToString(OrgId, DeptID, UserID, GlobalFilters.FilterType.Accounts);
                    if (_strAcc.Length > 0)
                    {
                        if (_gfWhere.Length > 0) _gfWhere += " OR ";
                        else _gfWhere += "(";
                        _gfWhere += "EXISTS(SELECT Id FROM UserAccounts WHERE DepartmentId=" + DeptID.ToString() + " AND UserId=LJ.Id AND AccountId IN (" + _strAcc + ")" + (_strAcc.Contains("-1") ? " OR AccountId IS NULL" : string.Empty) + ")";
                    }
                }
                if (_gfWhere.Length > 0) _sqlWhere += " AND " + _gfWhere + ")";
            }

            return SelectByQuery(_sqlSelect + _sqlWhere + _sqlGroupBy + _sqlOrderBy, OrgId);
        }

        private static string OrLogic(string request, string dbname)
        {
            string resultQuery = string.Empty;
            if (request.Contains(","))
            {
                foreach (string value in request.Split(','))
                {
                    if (!string.IsNullOrEmpty(value))
                        resultQuery += string.Format(" {0} LIKE '{1}%' OR ", dbname, Security.SQLInjectionBlock(value.Replace("'", "''")));
                }
                resultQuery = resultQuery.Substring(0, resultQuery.Length - 3);
            }
            else
                resultQuery = string.Format(" {0} LIKE '{1}%' ", dbname, Security.SQLInjectionBlock(request.Replace("'", "''")));
            return resultQuery;
        }

        public static DataTable SelectByQueryExt(string x_query)
        {
            return SelectByQuery(x_query);
        }

        public static int InsertPartialUser(int DeptID, int LoginID, string FirstName, string LastName, string Email, string Password, int LocationId, int AccountId, int AccountLocationId, string Title, string Organization, string Phone, string MobilePhone)
        {
            SqlParameter _pRVal = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVal.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pLoginId = new SqlParameter("@LoginId", SqlDbType.Int);
            if (LoginID != 0) _pLoginId.Value = LoginID;
            else _pLoginId.Value = DBNull.Value;
            SqlParameter _pLocationId = new SqlParameter("@location", SqlDbType.Int);
            if (LocationId != 0) _pLocationId.Value = LocationId;
            else _pLocationId.Value = DBNull.Value;
            SqlParameter _pAccountId = new SqlParameter("@intAcctId", SqlDbType.Int);
            if (AccountId != 0) _pAccountId.Value = AccountId;
            else _pAccountId.Value = DBNull.Value;
            SqlParameter _pAccountLocationId = new SqlParameter("@intAcctLoc", SqlDbType.Int);
            if (AccountLocationId != 0) _pAccountLocationId.Value = AccountLocationId;
            else _pAccountLocationId.Value = DBNull.Value;
            UpdateData("sp_InsertPartialUser", new SqlParameter[] { _pRVal, new SqlParameter("@DId", DeptID), _pLoginId, new SqlParameter("@FirstName", FirstName), new SqlParameter("@LastName", LastName), new SqlParameter("@Email", Email), new SqlParameter("@Password", Password), _pLocationId, _pAccountId, _pAccountLocationId, new SqlParameter("@Title", Title), new SqlParameter("@vchOrganization", Organization), new SqlParameter("@Phone", Phone), new SqlParameter("@MobilePhone", MobilePhone) });
            return (int)_pRVal.Value;
        }

        public static void UpdateAccountLocation(int DeptID, int UserID, int AccountId, int AccountLocationId)
        {
            UpdateAccountLocation(DeptID, UserID, AccountId, AccountLocationId, Guid.Empty);
        }

        public static void UpdateAccountLocation(int DeptID, int UserID, int AccountId, int AccountLocationId, Guid orgId)
        {
            UpdateData("sp_UpdateAcctUserLocation", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID), new SqlParameter("@intAcctId", AccountId), new SqlParameter("@intAcctLocId", AccountLocationId) }, orgId);
        }

        public static void RemoveFromAccount(int DepartmentId, int UserId, int AccountId)
        {
            UpdateData("sp_DeleteUserFomAccount", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@UserId", UserId), new SqlParameter("@AccountId", AccountId) });
        }

        public static void ConfigureMailGroupUsers(int DeptID, int MailGroupID, int ActionType, string LoginList)
        {
            UpdateData("sp_ConfigureMailGroupUsers", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupID", MailGroupID), new SqlParameter("@Action", ActionType), new SqlParameter("@list", LoginList) });
        }

        public static void UpdateTechCheckInStatus(int DeptID, int UserId, bool Status)
        {
            UpdateData("sp_UpdateTechCheckInStatus", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UserId", UserId), new SqlParameter("@Status", Status) });
        }

        public static int UpdateProfile(Guid OrgId, Guid InstId, int DeptID, int UserID, string firstName, string lastName,
                                        string email, string title, string password, string phone,
                                        string mobilePhone, int creationCatId, string mobileEmail, int mobileEmailType, string timeZoneId, int? timeFormatId, int? dateFormat)
        {

            DataRow _row = SelectUserDetails(OrgId, DeptID, UserID);
            string _old_email = _row["Email"].ToString();
            int _userType = (int)_row["UserType_Id"];

            Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
            Guid _loginId = _login.GetLoginId(_old_email);

            System.Collections.ArrayList userGroupIdList = GetUserGroups(OrgId, InstId, _userType, _loginId);

            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.UpdateUser(_loginId, email, firstName, lastName, null, phone, mobilePhone, null, title, null, null, null, null, null, null, null, timeZoneId, timeFormatId, dateFormat, userGroupIdList, OrgId, true);
            }
            catch (ArgumentException)
            {
                return 1;
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            if (!string.IsNullOrEmpty(password))
                _login.ChangePassword(_loginId, password, false, false);

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pPassword = new SqlParameter("@password", DBNull.Value);

            SqlParameter pMobileEmail = new SqlParameter("@MobileEmail", SqlDbType.VarChar, 50);
            if (mobileEmail.Length > 0) pMobileEmail.Value = mobileEmail;
            else pMobileEmail.Value = DBNull.Value;
            SqlParameter pMobileEmailType = new SqlParameter("@MobileEmailType", SqlDbType.TinyInt);
            pMobileEmailType.Value = mobileEmailType;

            if (password != null && password != string.Empty)
                pPassword.Value = password;

            UpdateData("sp_UpdateUserProfile",
                new SqlParameter[] {
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@UId", UserID),
					new SqlParameter("@firstname", firstName),
					new SqlParameter("@lastname", lastName),
					new SqlParameter("@email", email),
					new SqlParameter("@Title", title),
					pPassword,
					new SqlParameter("@phone", phone),
					new SqlParameter("@MobilePhone", mobilePhone),
					new SqlParameter("@creationCatId", creationCatId),
                    pMobileEmail,
                    pMobileEmailType,
					pReturnValue
				}, OrgId
            );

            if (error != null)
                throw error;

            return (int)pReturnValue.Value;
        }

        //public static int InsertLoginToken(int Token, string Seed)
        //{
        //    SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
        //    pReturnValue.Direction = ParameterDirection.ReturnValue;
        //    foreach (int _db in GetDbNumbers())
        //    UpdateData("sp_InsertLoginToken",
        //        new SqlParameter[] {
        //            new SqlParameter("@Token", Token),
        //            new SqlParameter("@Seed", Seed),                    
        //            pReturnValue
        //        }, _db
        //    );

        //    return (int)pReturnValue.Value;
        //}

        public static int UpdateProfileLite(int DeptID, int UserID, string phone, string mobilePhone)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateUserProfileLite",
                new SqlParameter[] {
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@UId", UserID),
					new SqlParameter("@phone", phone),
					new SqlParameter("@MobilePhone", mobilePhone),
					pReturnValue
				}
            );

            return (int)pReturnValue.Value;
        }

        public static int UpdateLogin(
                        Guid OrgId,
                        Guid InstId,
                        int code,
                        int DId,
                        ref int UId,
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
                        string LdapUserAccount,
                        string MobileEmail,
                        int MobileEmailType,
                        bool IsOrgAdmin
            )
        {
            string _old_email = user_email;

            if (UId != 0)
            {
                DataRow _row = SelectUserDetails(DId, UId);
                _old_email = _row["Email"].ToString();
            }

            Micajah.Common.Bll.Providers.LoginProvider _login = new Micajah.Common.Bll.Providers.LoginProvider();
            Guid LoginId = _login.GetLoginId(_old_email);

            System.Collections.ArrayList userGroupIdList = GetUserGroups(OrgId, InstId, intUserType, LoginId);

            if (IsOrgAdmin && !userGroupIdList.Contains(Guid.Empty)) userGroupIdList.Add(Guid.Empty);
            else if (!IsOrgAdmin && userGroupIdList.Contains(Guid.Empty)) userGroupIdList.Remove(Guid.Empty);

            Exception error = null;

            if (LoginId == Guid.Empty)
            {
                try
                {
                    Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(_old_email, user_email, user_firstname, user_lastname, null, Phone, MobilePhone, null, user_title, null, null, null, null, null, null, null, Micajah.Common.Bll.Support.ConvertListToString(userGroupIdList), OrgId, user_password, false, true);
                }
                catch (ArgumentException)
                {
                    return 1;
                }
                catch (System.Net.Mail.SmtpException ex)
                {
                    error = ex;
                }
            }
            else
            {
                try
                {
                    Micajah.Common.Bll.Providers.UserProvider.UpdateUser(LoginId, user_email, user_firstname, user_lastname, null, Phone, MobilePhone, null, user_title, null, null, null, null, null, null, null, null, null, null, userGroupIdList, OrgId, true);
                }
                catch (ArgumentException)
                {
                    return 1;
                }
                catch (System.Net.Mail.SmtpException ex)
                {
                    error = ex;
                }

                if (!string.IsNullOrEmpty(user_password))
                    _login.ChangePassword(LoginId, user_password, false);
            }

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pUId = new SqlParameter("@UId", SqlDbType.Int);
            pUId.Direction = ParameterDirection.InputOutput;
            pUId.Value = UId;

            SqlParameter pUserPassword = new SqlParameter("@user_password", SqlDbType.VarChar, 50);
            if (user_password.Length > 0) pUserPassword.Value = user_password;
            else pUserPassword.Value = DBNull.Value;

            SqlParameter pUserRoom = new SqlParameter("@user_room", DBNull.Value);
            if (user_room.Length > 0)
                pUserRoom.Value = user_room;

            SqlParameter pLevel = new SqlParameter("@tintLevel", DBNull.Value);
            if (tintLevel > 0)
                pLevel.Value = tintLevel;

            SqlParameter pAccountId = new SqlParameter("@intAcctId", DBNull.Value);
            if (intAcctId > 0)
                pAccountId.Value = intAcctId;

            SqlParameter pAccountLocId = new SqlParameter("@intAcctLocId", DBNull.Value);
            if (intAcctLocId > 0)
                pAccountLocId.Value = intAcctLocId;

            SqlParameter pSupGroupId = new SqlParameter("@intSupGroupId", DBNull.Value);
            if (intSupGroupId > 0)
                pSupGroupId.Value = intSupGroupId;

            SqlParameter pOrganization = new SqlParameter("@vchOrganization", DBNull.Value);
            if (vchOrganization.Length > 0)
                pOrganization.Value = vchOrganization;

            SqlParameter pLdapUserSID = new SqlParameter("@LdapUserSID", DBNull.Value);
            if (LdapUserSID.Length > 0)
                pLdapUserSID.Value = LdapUserSID;

            SqlParameter pLdapUserAccount = new SqlParameter("@LdapUserAccount", DBNull.Value);
            if (LdapUserAccount.Length > 0)
                pLdapUserAccount.Value = LdapUserAccount;
            SqlParameter pMobileEmail = new SqlParameter("@MobileEmail", SqlDbType.VarChar, 50);
            if (MobileEmail.Length > 0) pMobileEmail.Value = MobileEmail;
            SqlParameter pMobileEmailType = new SqlParameter("MobileEmailType", SqlDbType.TinyInt);
            pMobileEmailType.Value = MobileEmailType;
            UpdateData("sp_UpdateLogin",
                new SqlParameter[] {
					pReturnValue,
					new SqlParameter("@DId", DId),
					pUId,
					new SqlParameter("@code", code),
					pUserPassword,
					new SqlParameter("@user_firstname", user_firstname),
					new SqlParameter("@user_lastname", user_lastname),
					new SqlParameter("@user_title", user_title),
					new SqlParameter("@user_email", user_email),
					new SqlParameter("@Phone", Phone),
					new SqlParameter("@MobilePhone", MobilePhone),
					new SqlParameter("@location_id", location_id),
					new SqlParameter("@user_note", user_note),
					new SqlParameter("@btUpdateAcct", btUpdateAcct),
					new SqlParameter("@intUserType", intUserType),
					new SqlParameter("@btCallCentreRep", btCallCentreRep),
					pUserRoom,
					pLevel,
					pAccountId,
					pAccountLocId,
					pSupGroupId,
					pOrganization,
					pLdapUserSID,
					pLdapUserAccount,
                    pMobileEmail,
                    pMobileEmailType}, OrgId);

            UId = (int)pUId.Value;

            if (error != null)
                throw error;

            return (int)pReturnValue.Value;
        }

        public static int CreateLoginViaWizard(
            Guid OrgID,
            int DepartmentId,
            string Email,
            string FirstName,
            string LastName,
            string Password,
            string Title,
            int locationId,
            string vchEmailSuffix,
            int accountLocationId,
            string Phone,
            string MobilePhone
            )
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pLocationId = new SqlParameter("@intDeptLoc", SqlDbType.Int);
            if (locationId > 0) pLocationId.Value = locationId;
            else pLocationId.Value = DBNull.Value;

            SqlParameter pAccountLocationId = new SqlParameter("@intAcctLoc", SqlDbType.Int);
            if (accountLocationId > 0) pAccountLocationId.Value = accountLocationId;
            else pAccountLocationId.Value = DBNull.Value;

            UpdateData("sp_InsertLogInFromWizard",
                new SqlParameter[] {
					pReturnValue,
					new SqlParameter("@DepartmentId", DepartmentId),
					new SqlParameter("@Email", Email),
					new SqlParameter("@FirstName", FirstName),
					new SqlParameter("@LastName", LastName),
					new SqlParameter("@Password", Password),
					new SqlParameter("@Title", Title),
					pLocationId,
					new SqlParameter("@vchEmailSuffix", vchEmailSuffix),
					pAccountLocationId,
					new SqlParameter("@Room", DBNull.Value),
					new SqlParameter("@Phone", Phone),
					new SqlParameter("@MobilePhone", MobilePhone)
					}, OrgID);

            return (int)pReturnValue.Value;
        }

        public static int CreateLoginViaWizard(
            Guid OrgID,
            int DepartmentId,
            string Email,
            string Password,
            int locationId,
            string vchEmailSuffix,
            int accountLocationId
            )
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pLocationId = new SqlParameter("@intDeptLoc", SqlDbType.Int);
            if (locationId > 0) pLocationId.Value = locationId;
            else pLocationId.Value = DBNull.Value;

            SqlParameter pAccountLocationId = new SqlParameter("@intAcctLoc", SqlDbType.Int);
            if (accountLocationId > 0) pAccountLocationId.Value = accountLocationId;
            else pAccountLocationId.Value = DBNull.Value;

            UpdateData("sp_InsertLogInFromWizard",
                new SqlParameter[] {
					pReturnValue,
					new SqlParameter("@DepartmentId", DepartmentId),
					new SqlParameter("@Email", Email),
					new SqlParameter("@Password", Password),
					pLocationId,
					new SqlParameter("@vchEmailSuffix", vchEmailSuffix),
					pAccountLocationId
					}, OrgID);

            return (int)pReturnValue.Value;
        }

        public static int SelectSuperUserInfo
        (
                        int DId,
                        int UId,
                        ref int user_type,
                        ref int super_user_type,
                        ref int super_user_id,
                        ref string super_user_root_location_id,
                        ref string super_user_root_location_name,
                        ref string domain_name
        )
        {
            return SelectSuperUserInfo(Guid.Empty, DId, UId, ref user_type, ref super_user_type, ref super_user_id, ref super_user_root_location_id, ref super_user_root_location_name, ref domain_name);
        }
        public static int SelectSuperUserInfo
        (
                        Guid OrgID,
                        int DId,
                        int UId,
                        ref int user_type,
                        ref int super_user_type,
                        ref int super_user_id,
                        ref string super_user_root_location_id,
                        ref string super_user_root_location_name,
                        ref string domain_name
        )
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pDeptId = new SqlParameter("@DId", SqlDbType.Int);
            pDeptId.Value = DId;
            pDeptId.Direction = ParameterDirection.Input;

            SqlParameter pUserId = new SqlParameter("@UId", SqlDbType.Int);
            pUserId.Value = UId;
            pUserId.Direction = ParameterDirection.Input;

            SqlParameter pUserTypeId = new SqlParameter("@intUserType", SqlDbType.Int);
            pUserTypeId.Value = DBNull.Value;
            pUserTypeId.Direction = ParameterDirection.Output;

            SqlParameter pSuperUserTypeId = new SqlParameter("@tintSUserType", SqlDbType.TinyInt);
            pSuperUserTypeId.Value = DBNull.Value;
            pSuperUserTypeId.Direction = ParameterDirection.Output;

            SqlParameter pSuperUserId = new SqlParameter("@intSUserId", SqlDbType.Int);
            pSuperUserId.Value = DBNull.Value;
            pSuperUserId.Direction = ParameterDirection.Output;

            SqlParameter pSuperUserLocationId = new SqlParameter("@vchSUserRootLocationId", SqlDbType.VarChar);
            pSuperUserLocationId.Size = 1000;
            pSuperUserLocationId.Value = DBNull.Value;
            pSuperUserLocationId.Direction = ParameterDirection.Output;

            SqlParameter pSuperUserLocationName = new SqlParameter("@vchSUserRootLocationName", SqlDbType.VarChar);
            pSuperUserLocationName.Value = DBNull.Value;
            pSuperUserLocationName.Size = 5000;
            pSuperUserLocationName.Direction = ParameterDirection.Output;

            SqlParameter pDomainName = new SqlParameter("@vchDomainName", SqlDbType.VarChar);
            pDomainName.Value = DBNull.Value;
            pDomainName.Size = 100;
            pDomainName.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectSuperUserInfo",
                new SqlParameter[] {
					pReturnValue,
					pDeptId,
					pUserId,
					pUserTypeId, 
					pSuperUserTypeId,
					pSuperUserId, 
					pSuperUserLocationId,
					pSuperUserLocationName, 
					pDomainName
					}, OrgID);

            if ((pUserTypeId.Value != null) && (pUserTypeId.Value != DBNull.Value))
                Int32.TryParse(pUserTypeId.Value.ToString(), out user_type);

            if ((pSuperUserTypeId.Value != null) && (pSuperUserTypeId.Value != DBNull.Value))
                Int32.TryParse(pSuperUserTypeId.Value.ToString(), out super_user_type);

            if ((pSuperUserId.Value != null) && (pSuperUserId.Value != DBNull.Value))
                Int32.TryParse(pSuperUserId.Value.ToString(), out super_user_id);

            if ((pSuperUserLocationId.Value != null) && (pSuperUserLocationId.Value != DBNull.Value))
                super_user_root_location_id = pSuperUserLocationId.Value.ToString();

            if ((pSuperUserLocationName.Value != null) && (pSuperUserLocationName.Value != DBNull.Value))
                super_user_root_location_name = pSuperUserLocationName.Value.ToString();

            if ((pDomainName.Value != null) && (pDomainName.Value != DBNull.Value))
                domain_name = pDomainName.Value.ToString();

            return (int)pReturnValue.Value;
        }

        public static int VerifyLogin
        (
                        int DId,
                        string email,
                        ref int UserId,
                        ref int LoginId
        )
        {
            return VerifyLogin(DId, email, ref UserId, ref LoginId, Guid.Empty);
        }

        public static int VerifyLogin
        (
                        int DId,
                        string email,
                        ref int UserId,
                        ref int LoginId,
                        Guid orgID
        )
        {
            UserId = 0;
            LoginId = 0;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            pUserId.Value = DBNull.Value;
            pUserId.Direction = ParameterDirection.Output;

            SqlParameter pLoginId = new SqlParameter("@LoginId", SqlDbType.Int);
            pLoginId.Value = DBNull.Value;
            pLoginId.Direction = ParameterDirection.Output;

            UpdateData("sp_VerifyLogin",
                new SqlParameter[] {
					pReturnValue,
					new SqlParameter("@DId", DId),
					new SqlParameter("@vchEmail", email),
					pUserId,
					pLoginId}, orgID);

            if ((pUserId.Value != null) && (pUserId.Value != DBNull.Value))
                UserId = (int)pUserId.Value;

            if ((pLoginId.Value != null) && (pLoginId.Value != DBNull.Value))
            {
                LoginId = (int)pLoginId.Value;
                return (int)pReturnValue.Value;
            }
            return 0;
        }

        public static int InsertUser
        (
                        Guid OrgID,
                        Guid InstID,
                        int DId,
                        int LoginId,
                        int UserType,
                        string UserEmail
        )
        {
            Guid _loginGuid = Guid.Empty;
            Micajah.Common.Dal.OrganizationDataSet.UserRow _userRow = Micajah.Common.Bll.Providers.UserProvider.GetUserRow(UserEmail, OrgID);
            if (_userRow != null) _loginGuid = _userRow.UserId;
            System.Collections.ArrayList userGroupIdList = GetUserGroups(OrgID, InstID, UserType, _loginGuid);

            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(UserEmail, Micajah.Common.Bll.Support.ConvertListToString(userGroupIdList), OrgID);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            int result = InsertLoginCompanyJunc(OrgID, DId, LoginId, UserType);

            if (error != null)
                throw error;

            return result;
        }

        public static int InsertUser
        (
                        Guid OrgID,
                        Guid InstID,
                        int DId,
                        int LoginId,
                        int UserType,
                        string UserEmail,
                        string firstName,
                        string lastName,
                        string phone,
                        string password
        )
        {
            Guid _loginGuid = Guid.Empty;
            Micajah.Common.Dal.OrganizationDataSet.UserRow _userRow = Micajah.Common.Bll.Providers.UserProvider.GetUserRow(UserEmail, OrgID);
            if (_userRow != null) _loginGuid = _userRow.UserId;
            System.Collections.ArrayList userGroupIdList = GetUserGroups(OrgID, InstID, UserType, _loginGuid);

            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(UserEmail, firstName, lastName, null, phone, "", null, "", null, null, null, null, null, null, null, userGroupIdList, OrgID, password, false, false);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            int result = InsertLoginCompanyJunc(OrgID, DId, LoginId, UserType);

            if (error != null)
                throw error;

            return result;
        }

        public static int InsertLoginCompanyJunc(Guid OrgID,
                        int DId,
                        int LoginId,
                        int UserType)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_InsertLoginCompanyJunc",
                new SqlParameter[] {
					pReturnValue,
					new SqlParameter("@DId", DId),
					new SqlParameter("@LoginId", LoginId),
					new SqlParameter("@UserType", UserType)                    
					}, OrgID);

            return (int)pReturnValue.Value;
        }

        public static int UpdateLogin(int LoginId, string Email, string FirstName, string LastName, string Password, string Title, string Phone, string MobilePhone)
        {
            return UpdateLogin(Data.DBAccess.GetCurrentOrgID(), LoginId, Email, FirstName, LastName, Password, Title, Phone, MobilePhone);
        }

        public static int UpdateLogin(Guid OrgID, int LoginId, string Email, string FirstName, string LastName, string Password, string Title, string Phone, string MobilePhone)
        {
            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(Email, FirstName, LastName, null, Phone, MobilePhone,
                        null, Title, null, null, null, null, null, null, null, (string)null, OrgID, Password == "0000" ? null : Password, false, true);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            int result = UpdateLoginSlave(OrgID, LoginId, Email, FirstName, LastName, Password, Title, Phone, MobilePhone);

            if (error != null)
                throw error;

            return result;
        }

        public static int UpdateLogin(Guid OrgID, Guid InstId, string Email, string FirstName, string LastName, string Password, string Phone, int userType)
        {
            System.Collections.ArrayList userGroupIdList = GetUserGroups(OrgID, InstId, userType, Guid.Empty);

            if (userGroupIdList.Contains(Guid.Empty)) userGroupIdList.Remove(Guid.Empty);

            Exception error = null;

            try
            {
                Micajah.Common.Bll.Providers.UserProvider.AddUserToOrganization(Email, FirstName, LastName, null, Phone, string.Empty,
                        null, string.Empty, null, null, null, null, null, null, null, Micajah.Common.Bll.Support.ConvertListToString(userGroupIdList), OrgID, Password == "0000" ? null : Password, false, true);
            }
            catch (System.Net.Mail.SmtpException ex)
            {
                error = ex;
            }

            int result = UpdateLoginSlave(OrgID, 0, Email, FirstName, LastName, Password, string.Empty, Phone, string.Empty);

            if (error != null)
                throw error;

            return result;
        }

        public static int UpdateLoginSlave(Guid OrgID, int LoginId, string Email, string FirstName, string LastName, string Password, string Title, string Phone, string MobilePhone)
        {
            SqlParameter _pLoginId = new SqlParameter("@LoginId", SqlDbType.Int);
            _pLoginId.Direction = ParameterDirection.InputOutput;
            if (LoginId != 0) _pLoginId.Value = LoginId;
            else _pLoginId.Value = DBNull.Value;
            SqlParameter _pPassword = new SqlParameter("@Password", SqlDbType.VarChar, 50);
            if (Password != null && Password.Length > 0)
            {
                if (Password == "0000") _pPassword.Value = GenerateRandomPassword();
                else _pPassword.Value = Password;
            }
            else _pPassword.Value = DBNull.Value;
            SqlParameter _pTitle = new SqlParameter("@Title", SqlDbType.VarChar, 30);
            if (Title.Length > 0) _pTitle.Value = Title;
            else _pTitle.Value = DBNull.Value;
            SqlParameter _pPhone = new SqlParameter("@Phone", SqlDbType.VarChar, 20);
            if (Phone.Length > 0) _pPhone.Value = Phone;
            else _pPhone.Value = DBNull.Value;
            SqlParameter _pMobilePhone = new SqlParameter("@MobilePhone", SqlDbType.VarChar, 20);
            if (MobilePhone.Length > 0) _pMobilePhone.Value = MobilePhone;
            else _pMobilePhone.Value = DBNull.Value;

            UpdateData("sp_InsertLogin", new SqlParameter[]{_pLoginId, 
				new SqlParameter("@Email", Email), 
				new SqlParameter("@FirstName", FirstName), 
				new SqlParameter("@LastName", LastName), 
				_pPassword,
				_pTitle,
				_pPhone,
				_pMobilePhone}, OrgID);
            return (int)_pLoginId.Value;
        }

        public static int InsertLogin(Guid organizationId, string Email, string FirstName, string LastName, string Password, string Title, string Phone, string MobilePhone)
        {
            return UpdateLogin(organizationId, 0, Email, FirstName, LastName, Password, Title, Phone, MobilePhone);
        }

        public static string GenerateRandomPassword()
        {
            return Micajah.Common.Application.WebApplication.LoginProvider.GeneratePassword(4, 0).ToLower();
        }

        /*
                public static PasswordRetrievalResult RetrievePassword(string email, bool send_as_html)
                {
                    return RetrievePassword(email, -1, 0, send_as_html);
                }

                public static PasswordRetrievalResult RetrievePassword(string email, int dbNumber, int DeptId, bool send_as_html)
                {
                    int _db_number=dbNumber;
                    DataRow rLoginDetails = Data.Logins.SelectLoginDetailsByEmail(email, ref _db_number);

                    if (rLoginDetails == null || rLoginDetails.IsNull("id"))
                        return PasswordRetrievalResult.InvalidEmail;

                    int departmentId = 0;
                    int userId = 0;
                    DataTable loginCompanyJunc;

                    if (_db_number==dbNumber && DeptId!=0)
                    {
                        loginCompanyJunc = Data.Logins.SelectByQuery("SELECT id, company_id FROM tbl_LoginCompanyJunc WHERE company_id="+DeptId.ToString()+" AND login_id = " + rLoginDetails["id"].ToString(), _db_number);
                        if (loginCompanyJunc.Rows.Count == 0) loginCompanyJunc = Data.Logins.SelectByQuery("SELECT id, company_id FROM tbl_LoginCompanyJunc WHERE login_id = " + rLoginDetails["id"].ToString(), _db_number);
                    }
                    else
                        loginCompanyJunc = Data.Logins.SelectByQuery("SELECT id, company_id FROM tbl_LoginCompanyJunc WHERE login_id = " + rLoginDetails["id"].ToString(), _db_number);
                    if (loginCompanyJunc == null || loginCompanyJunc.Rows.Count == 0)
                        return PasswordRetrievalResult.NoDeptsAssociated;

                    try
                    { departmentId = (int)loginCompanyJunc.Rows[0]["company_id"]; }
                    catch { return PasswordRetrievalResult.NoDeptsAssociated; }
                    try
                    { userId = (int)loginCompanyJunc.Rows[0]["id"]; }
                    catch { return PasswordRetrievalResult.NoDeptsAssociated; }

                    if (departmentId == 0 || userId == 0)
                        return PasswordRetrievalResult.NoDeptsAssociated;

                    string emailSubject = "Your bigWebApps Password.";
                    string emailTextTop = "Your login and password for bigWebApps:";
                    string emailTextBottom = "http://" + HttpContext.Current.Request.ServerVariables["SERVER_NAME"] + "<br><br>bigWebApps Support<br>support@bigwebapps.com";

                    if (loginCompanyJunc.Rows.Count == 1)
                    {
                        string _custTxt = CustomTexts.GetCustomText(_db_number, departmentId, "ForgotPasswordEmailSubject");
                        if (_custTxt.Length > 0) emailSubject = _custTxt;
                        _custTxt = CustomTexts.GetCustomText(_db_number, departmentId, "ForgotPasswordTextTop");
                        if (_custTxt.Length > 0) emailTextTop = _custTxt;
                        _custTxt = CustomTexts.GetCustomText(_db_number, departmentId, "ForgotPasswordTextBottom");
                        if (_custTxt.Length > 0) emailTextBottom = _custTxt;
                    }

                    string emailBody = string.Empty;
                    if (send_as_html)
                    {
                        emailBody = "<!DOCTYPE HTML PUBLIC\"-//IETF//DTD HTML//EN\"><html><body>";

                        emailBody += emailTextTop + "<br><br>";
                        emailBody += "Login: " + (string)rLoginDetails["Email"] + "<br>";
                        emailBody += "Password: " + (string)rLoginDetails["Password"] + "<br><br>";
                        emailBody += emailTextBottom;
                        emailBody += "</body></html>";
                    }
                    else
                    {
                        emailBody = Functions.RemoveHTML(emailTextTop) + "\n\n";
                        emailBody += "Login:    " + (string)rLoginDetails["Email"] + "\n";
                        emailBody += "Password: " + (string)rLoginDetails["Password"] + "\n\n";
                        emailBody += Functions.RemoveHTML(emailTextBottom);
                    }
                    Data.MailNotification mailNotification = new Data.MailNotification(
                        _db_number,
                        departmentId,
                        userId,
                        "noreply@bigwebapps.com",
                        email,
                        emailSubject,
                        emailBody);

                    try
                    {
                        if (mailNotification != null)
                        {
                            string returnString = string.Empty;

                            string isMailServiceInstalled = System.Configuration.ConfigurationManager.AppSettings["UseWindowServiceForNotifications"];

                            if (isMailServiceInstalled == null)
                                returnString = mailNotification.Commit(Data.MailNotification.UseSendMailEngine.SystemWebMail, send_as_html);
                            else
                            {
                                if (isMailServiceInstalled.ToLower() == "true")
                                    returnString = mailNotification.Commit(Data.MailNotification.UseSendMailEngine.NotificationService, send_as_html);
                                else
                                    returnString = mailNotification.Commit(Data.MailNotification.UseSendMailEngine.SystemWebMail, send_as_html);
                            }

                            if (returnString.Length > 0)
                                return PasswordRetrievalResult.Failed;

                            return PasswordRetrievalResult.Ok;
                        }
                    }
                    catch (Exception)
                    {
                        return PasswordRetrievalResult.Failed;
                    }

                    return PasswordRetrievalResult.Failed;
                }
        */
        public static DataTable SelectUsersAndAccounts(int companyID, string searchString, bool searchAccountToo)
        {
            return SelectRecords("sp_SelectUsersAndAccounts", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@SearchString", searchString),
					new SqlParameter("@SearchAccountToo", searchAccountToo)
				   });
        }

        public static DataTable SelectTechAccountsProjects(int companyID, int SelectedTechID)
        {
            return SelectRecords("sp_SelectTechAccountsProjects", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@TechID", SelectedTechID)                    
				   });
        }

        public static DataTable SelectSuperUserUserSearch(int companyID, int uId, int superUserTypeId
            , string rootLocationIdList, string searchString)
        {
            SqlParameter _pSUTypeId = new SqlParameter("@tintSUserType", SqlDbType.TinyInt);
            _pSUTypeId.Value = superUserTypeId;

            return SelectRecords("sp_SelectSuperUserUserSearch", new SqlParameter[]
				   {
					new SqlParameter("@DId", companyID),
					new SqlParameter("@UId", uId),
					_pSUTypeId,
					new SqlParameter("@vchUserRootLocationIdList", rootLocationIdList),
					new SqlParameter("@SearchString", searchString)
				   });
        }

        public static void UpdateUserPrimaryLocation(int departmentID, int userID,
           int locationID)
        {
            UpdateUserPrimaryLocation(departmentID, userID, locationID, Guid.Empty);
        }

        public static void UpdateUserPrimaryLocation(int departmentID, int userID,
           int locationID, Guid orgId)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DepartmentId", departmentID);
            _params[1] = new SqlParameter("@UserID", userID);
            _params[2] = new SqlParameter("@LocationID", locationID);
            UpdateData("sp_UpdateUserPrimaryLocation", _params, orgId);
        }

        public static void UpdateUserProfileLocation(int DeptID, int UserID, int locationId)
        {
            SqlParameter pLocationId = new SqlParameter("@LocationId", DBNull.Value);
            if (locationId > 0)
                pLocationId.Value = locationId;

            UpdateData("sp_UpdateUserProfileLocation",
                       new SqlParameter[]
						   {
							   new SqlParameter("@DId", DeptID),
							   new SqlParameter("@UId", UserID),
							   pLocationId
						   });
        }

        public static void UpdateUserProfileTicketTime(int DeptID, int UserID, int ticketTimer)
        {
            SqlParameter pTicketTimer = new SqlParameter("@tintTicketTimer", DBNull.Value);
            if (ticketTimer >= 0)
                pTicketTimer.Value = ticketTimer;

            UpdateData("sp_UpdateUserProfileTicketTime",
                       new SqlParameter[]
						   {
							   new SqlParameter("@DId", DeptID),
							   new SqlParameter("@UId", UserID),
							   pTicketTimer
						   });
        }

        public static void UpdateUserProfilePrintFontSize(int DeptID, int UserID, int printFontSize)
        {
            SqlParameter pPrintFontSize = new SqlParameter("@printFontSize", DBNull.Value);
            if (printFontSize > 0)
                pPrintFontSize.Value = printFontSize;

            UpdateData("sp_UpdateUserProfilePrintFontSize",
                       new SqlParameter[]
						   {
							   new SqlParameter("@DId", DeptID),
							   new SqlParameter("@UId", UserID),
							   pPrintFontSize
						   });
        }
    }
}
