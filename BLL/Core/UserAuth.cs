using System;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Web;
using System.Xml.Serialization;
using bigWebApps.bigWebDesk.Data;
using Micajah.Common.Security;

namespace bigWebApps.bigWebDesk
{
    [Serializable]
    public class UserAuth
    {
        #region Local variables

        private bool _isOk = false; //Authentication result
        private string _errMessage = string.Empty;

        private string _tk = ""; //Token
        private int _ui = -1; //UserId
        private int _di = -1; //DeptId
        private Guid _oi = Guid.Empty; //Organization Id
        private Guid _ii = Guid.Empty; //Instance Id
        private string _cf = ""; //config string
        private string _em = string.Empty; //login email
        private string _pm = ""; //permissions
        private string _lo = ""; //Logo
        private string _dn = ""; //DepartmentName
        private string _fn = ""; //FirstName
        private string _ln = ""; //LastName
        private int _sg = -1; //Support Group
        private bool _ps = false; //Config Partial Setup
        private bool _fr = false; //Frame Mode 1=on 0=off
        private int _tt = 0; //Ticket Timer 0=Days Old, 1=SLA
        private int _st = 0; //Super User Type 0=not super user, 1=Acct based, 2=location based
        private int _si = -1; //Super User Id, id of the account or location in which the super user belongs
        private string _sd = ""; //Super User Domain, name of account or location
        private string _sr = string.Empty; //Super User Root Location Id
        private string _sn = string.Empty; //Super User Root Location Name
        private UserAuth.UserRole _role = UserRole.StandardUser; //VGOOZ: added user role
        private string _gf = string.Empty; //VGOOZ 12-MAY-2005: added global filters
        private bool _persistance = false;
        private string _password = string.Empty; //Password
        private TimeZoneInfo _tzi = null;
        private int _tf = 0; //AM/PM time format
        private int _df = 0; //MM/dd/yyyy date format

        #endregion

        #region Global properties & enumerations
        //MRUDKOVSKI: 10-APR-2005 - Transfered UserRole to "Global properties & enumerations" region
        //VGOOZ: Added user roles 
        public enum UserRole
        {
            StandardUser = 1,
            SuperUser = 5,
            Technician = 2,
            Administrator = 3
        }

        public TimeZoneInfo TimeZone
        {
            get { return _tzi ?? (_tzi = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")); }
        }

        public int TimeFormat { get { return _tf; } }

        public int DateFormat { get { return _df; } }

        //MRUDKOVSKI: 10-MAY-2005 - Fixed a small bug with user roles (the propery was not serialized because had no "set" method).
        [XmlAttribute("role")] //Role
        public UserAuth.UserRole Role { get { return _role; } set { _role = value; } }

        [XmlAttribute("tk")] //Token
        public string varUToken { get { return _tk; } set { _tk = (value != null ? value : ""); } }

        [XmlAttribute("oi")] //OrgId
        public Guid OrgID { get { return _oi; } set { _oi = value; } }
        [XmlIgnore] // Additional service property for OrganizationId
        private string _strOId
        {
            get { return _oi.ToString(); }
            set
            {
                _oi = (!string.IsNullOrEmpty(value) ? Guid.Parse(value) : Guid.Empty);
            }
        }

        [XmlAttribute("ui")] //UserId
        public int lngUId { get { return _ui; } set { _ui = value; } }
        [XmlIgnore] // Additional service property for UserID
        private string _strUId
        {
            get { return _ui.ToString(); }
            set
            {
                _ui = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1);
            }
        }

        [XmlAttribute("di")] //DeptId
        public int lngDId { get { return _di; } set { _di = value; } }
        [XmlIgnore] // Additional service property for DeptID
        private string _strDId
        {
            get { return _di.ToString(); }
            set
            {
                _di = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1);
            }
        }

        [XmlAttribute("dguid")] //DepartmentGUID
        public Guid InstanceID { get { return _ii; } set { _ii = value; } }
        [XmlIgnore] // Additional service property for DepartmentGUID
        private string _strGDGuid
        {
            get { return _ii.ToString(); }
            set { _ii = (!string.IsNullOrEmpty(value) ? Guid.Parse(value) : Guid.Empty); }
        }

        [XmlAttribute("cf")] //config string
        public string strCfg { get { return _cf; } set { _cf = ((value != null) ? value : ""); } }

        [XmlAttribute("gf")] //global filter string
        public string strGFilter { get { return _gf; } set { _gf = ((value != null) ? value : ""); } }

        [XmlAttribute("em")] //global filter string
        public string strEmail { get { return _em; } set { _em = ((value != null) ? value : ""); } }

        [XmlAttribute("pm")] //user permissions
        public string strGPerm { get { return _pm; } set { _pm = ((value != null) ? value : ""); } }

        [XmlAttribute("tt")] //Ticket Timer
        public int tintTicketTimer { get { return _tt; } set { _tt = value; } }
        [XmlIgnore] // Additional service property for Ticket Timer
        private string _strTicketTimer
        {
            get { return _tt.ToString(); }
            set
            {
                _tt = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1);
            }
        }

        [XmlAttribute("dn")] //DepartmentName
        public string strGDName { get { return _dn; } set { _dn = ((value != null) ? value : ""); } }

        [XmlAttribute("fn")] //FirstName
        public string strGFName { get { return _fn; } set { _fn = ((value != null) ? value : ""); } }

        [XmlAttribute("ln")] //LastName
        public string strGLName { get { return _ln; } set { _ln = ((value != null) ? value : ""); } }

        [XmlAttribute("sg")] //Support Group
        public int lngGSpGrp { get { return _sg; } set { _sg = value; } }
        [XmlIgnore] // Additional service property for Support Group
        private string _strGSpGrp
        {
            get { return _sg.ToString(); }
            set
            {
                _sg = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1);
            }
        }

        [XmlAttribute("fr")] //Frame Mode 1=on 0=off
        public bool btGFrame { get { return _fr; } set { _fr = value; } }
        [XmlIgnore] // Additional service property for Frame Mode
        private string _strGFrame
        {
            get { return (_fr ? "1" : "0"); }
            set
            {
                _fr = (value != null ? ((value.Trim().Length > 0) ? (value == "1") : false) : false);
            }
        }

        [XmlAttribute("st")] //Super User Type 0=not super user, 1=Acct based, 2=location based
        public int sintGSUserType { get { return _st; } set { _st = value; } }
        [XmlIgnore] // Additional service property for Super User Type
        private string _strGSUserType
        {
            get { return _st.ToString(); }
            set
            {
                _st = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1);
            }
        }

        [XmlAttribute("si")] //Super User Id, id of the account or location in which the super user belongs
        public int lngGSUserId { get { return _si; } set { _si = value; } }
        [XmlIgnore] // Additional service property for Super User Id
        private string _strGSUserId
        {
            get { return _si.ToString(); }
            set { _si = (value != null ? ((value.Trim().Length > 0) ? Int32.Parse(value) : -2) : -1); }
        }

        [XmlAttribute("sd")] //Super User Domain, name of account or location
        public string vchGSUserDomain { get { return _sd; } set { _sd = ((value != null) ? value : ""); } }

        [XmlAttribute("sr")] //Super User Root Location Id
        public string strGSUserRootLocationId { get { return _sr; } set { _sr = value; } }
        [XmlAttribute("sn")] //Super User Root Location Name
        public string strGSUserRootLocationName { get { return _sn; } set { _sn = ((value != null) ? value : ""); } }

        //MRUDKOVSKI: 20-MAY-2005 - Added CustomNames property (read-only)
        [XmlIgnore]
        public CustomNames customNames
        {
            get
            {
                return CustomNames.GetCustomNames(this.OrgID, this.lngDId);
            }
        }

        [XmlIgnore]
        public bool IsConfigPartialSetup
        {
            get { return _ps; }
            set { _ps = value; }
        }

        [XmlIgnore]
        public bool IsOk { get { return _isOk; } set { _isOk = value; } }
        [XmlIgnore]
        public string ErrorMessage { get { return _errMessage; } set { _errMessage = value; } }
        [XmlIgnore]
        public bool IsPersistance { get { return _persistance; } set { _persistance = value; } }
        #endregion

        #region Public methods

        public static bool ValidateLoginInCompany(Guid organizationId, Guid instanceId, string loginEmail, out int userId, out DataRow userRow, out string errorMessage)
        {
            return ValidateLoginInCompany(bigWebApps.bigWebDesk.Data.Companies.SelectOne(organizationId, instanceId), organizationId, loginEmail, out  userId, out userRow, out errorMessage);
        }

        public static bool ValidateLoginInCompany(DataRow companyRow, Guid organizationId, string loginEmail, out int userId, out DataRow userRow, out string errorMessage)
        {
            int accId = 0;
            userId = 0;
            errorMessage = null;
            userRow = null;

            if (companyRow == null)
            {
                errorMessage = "The Department cannot be located in the bigWebApps database.";
                return false;
            }

            int _did = (int)companyRow["company_id"];

            if (string.IsNullOrEmpty(loginEmail))
                return false;

            int _userStatus = Data.Logins.SelectLoginExists(organizationId, _did, loginEmail, out userId, out accId);
            if (_userStatus == 1)
            {
                errorMessage = "The user account cannot be located in the bigWebApps database.";
                return false;
            }
            else if (_userStatus == 2)
            {
                errorMessage = "The user account is not associated with this Department.";
                return false;
            }

            userRow = Data.Logins.SelectUserDetails(organizationId, _did, userId);
            if (userRow == null)
            {
                errorMessage = "The user account is not associated with this Department.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Serialize UserAuth object into XML
        /// </summary>
        /// <returns>String representation of the class</returns>
        public override string ToString()
        {
            string res = "";
            XmlSerializer xs = new XmlSerializer(typeof(UserAuth));
            StringWriter sw = new StringWriter();
            xs.Serialize(sw, this);
            res = sw.ToString();
            sw.Close();
            return res;
        }

        /// <summary>
        /// Load from user context data and parse it
        /// </summary>
        public void Load()
        {
            this.IsOk = false;
            if (UserContext.Current == null) return;

            if (!LoadFromUserContext()) return;

            this.IsOk = true;
        }

        public void Load(Guid orgId, int deptId, string loginEmail)
        {
            this.OrgID = orgId;
            Load(Data.Companies.SelectOne(orgId, deptId), loginEmail);
        }

        public void Load(Guid orgId, Guid companyGuid, string loginEmail)
        {
            this.OrgID = orgId;
            Load(Data.Companies.SelectOne(orgId, companyGuid), loginEmail);
        }

        public void Load(Guid orgId, DataRow companyRow, string loginEmail)
        {
            this.OrgID = orgId;
            Load(companyRow, loginEmail);
        }

        public void Load(DataRow companyRow, string loginEmail)
        {
            int _userId = 0;
            DataRow _row = null;
            this.IsOk = false;
            string message = null;

            if (!ValidateLoginInCompany(companyRow, OrgID, loginEmail, out _userId, out _row, out message))
            {
                this._errMessage = message;
                return;
            }

            int _did = (int)companyRow["company_id"];
            this.strGDName = companyRow["company_name"].ToString();
            this._strGDGuid = companyRow["company_guid"].ToString();

            this._strUId = _userId.ToString();
            this._strDId = _did.ToString();
            this.strEmail = loginEmail;
            this.strGFName = _row["FirstName"].ToString();
            this.strGLName = _row["LastName"].ToString();
            this._password = _row["Password"].ToString();
            if (!_row.IsNull("tintTicketTimer")) this.tintTicketTimer = (byte)_row["tintTicketTimer"];
            else this.tintTicketTimer = (byte)_row["tintDTicketTimer"];
            if (!_row.IsNull("configPartialSetup")) this.IsConfigPartialSetup = (bool)_row["configPartialSetup"];
            this._strGSpGrp = _row["SupGroupId"].ToString();
            this._strGFrame = "0";
            switch ((int)_row["UserType_Id"])
            {
                case 1:
                    this.strGPerm = ",usr";
                    _role = UserRole.StandardUser;
                    break;
                case 2:
                    this.strGPerm = ",tch";
                    _role = UserRole.Technician;
                    break;
                case 3:
                    this.strGPerm = ",tch,adm";
                    _role = UserRole.Administrator;
                    break;
                case 4:
                    this.strGPerm = ",que";
                    _role = UserRole.StandardUser;
                    break;
                case 5:
                    this.strGPerm = ",usr";
                    _role = UserRole.StandardUser;
                    int _utype = 0;
                    int _sutype = 0;
                    int _suid = 0;
                    string _surlocid = string.Empty;
                    string _surlocname = string.Empty;
                    string _sudomain = string.Empty;
                    int _res = Data.Logins.SelectSuperUserInfo(OrgID, _did, _userId, ref _utype, ref _sutype, ref _suid, ref _surlocid, ref _surlocname, ref _sudomain);
                    if (_res >= 0 && _sutype != 0 && _suid != 0)
                    {
                        this.strGPerm += ",susr";
                        _role = UserRole.SuperUser;
                        this._strGSUserType = _sutype.ToString();
                        this._strGSUserId = _suid.ToString();
                        this.vchGSUserDomain = _sudomain;
                        this.strGSUserRootLocationId = _surlocid;
                        this.strGSUserRootLocationName = _surlocname;
                    }
                    break;
            }
            if (!_row.IsNull("btGlobalFilterEnabled") && (bool)_row["btGlobalFilterEnabled"]) this.strGFilter = ",gf";
            if (!_row.IsNull("btLimitToAssignedTkts") && (bool)_row["btLimitToAssignedTkts"]) this.strGFilter += ",tkt";
            if (this.strGFilter.IndexOf(",gf") >= 0 && this.strGFilter.IndexOf(",tkt") >= 0 && !_row.IsNull("btDisabledReports") && (bool)_row["btDisabledReports"]) this.strGFilter += ",rpt";
            this.IsOk = true;
            Micajah.Common.Dal.OrganizationDataSet.UserRow _usrRow = Micajah.Common.Bll.Providers.UserProvider.GetUserRow(loginEmail);
            if (_usrRow != null)
            {
                string timeZoneId = (_usrRow.IsTimeZoneIdNull() ? null : _usrRow.TimeZoneId);
                int? timeFormat = (_usrRow.IsTimeFormatNull() ? null : new int?(_usrRow.TimeFormat));
                int? dateFormat = (_usrRow.IsDateFormatNull() ? null : new int?(_usrRow.DateFormat));

                if (string.IsNullOrEmpty(timeZoneId) || (!timeFormat.HasValue) || (!dateFormat.HasValue))
                {
                    Micajah.Common.Bll.Instance inst = Micajah.Common.Bll.Providers.InstanceProvider.GetInstance(this.InstanceID, this.OrgID);
                    if (inst != null)
                    {
                        if (string.IsNullOrEmpty(timeZoneId))
                            timeZoneId = inst.TimeZoneId;

                        if (!timeFormat.HasValue)
                            timeFormat = inst.TimeFormat;

                        if (!dateFormat.HasValue)
                            dateFormat = inst.DateFormat;
                    }
                }

                if (string.IsNullOrEmpty(timeZoneId))
                    timeZoneId = "Eastern Standard Time";

                if (!timeFormat.HasValue)
                    timeFormat = 0;

                if (!dateFormat.HasValue)
                    dateFormat = 0;

                _tzi = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                _tf = timeFormat.Value;
                _df = dateFormat.Value;
            }
        }

        /// <summary>
        /// Authenticate the user data stored in user context
        /// </summary>
        /// <returns>Result of authentication</returns>
        public bool IsTokenValid()
        {
            bool res = false; //Initially the user is not authenticated

            if (this.lngDId != 0 && this.lngUId != 0 && this.varUToken.Length > 0)
            {
                res = (Data.Logins.UpdateUserToken(this.OrgID, this.lngDId, this.lngUId, 2) == this.varUToken);
            }

            return res;
        }

        public static bool IsTokenValid(Guid OrgID, string LoginEmail, string Token)
        {
            DataTable _dt = Data.Logins.SelectLogin(OrgID, LoginEmail, Token);
            if (_dt.Rows.Count > 0 && !_dt.Rows[0].IsNull("Seed")) return true;
            else return false;
        }

        public static bool IsHashValid(Guid OrgID, string LoginEmail, string HashString)
        {
            DataRow _row = Data.Logins.SelectLoginDetailsByEmail(OrgID, LoginEmail);
            if (_row == null) return false;
            string _pass = _row["Password"].ToString();
            if (HashString != CreateMD5hash(_pass.Length > 0 ? _pass : LoginEmail)) return false;
            return true;
        }

        public static bool IsPasswordValid(Guid OrgID, string LoginEmail, string Password)
        {
            DataRow _row = Data.Logins.SelectLoginDetailsByEmail(OrgID, LoginEmail);
            if (_row == null) return false;
            string _pass = _row["Password"].ToString();
            if (_pass != Password) return false;
            return true;
        }

        public void Save(bool IsRememberMe)
        {
            _persistance = IsRememberMe;
            Save();
        }

        public void Save()
        {
            Save(null);
        }

        public void Save(bool IsRememberMe, UserContext user)
        {
            _persistance = IsRememberMe;
            Save(user);
        }

        public void Save(UserContext user)
        {
            if (HttpContext.Current == null) return;

            if (user == null) user = UserContext.Current;

            SaveToUserContext(user);
        }

        //MRUDKOVSKI: 19-MAY-2005 - Transfered classes to the place because it's public
        //MRUDKOVSKI: 19-MAY-2005 - Added appropriate structured commentaries
        //VGOOZ 12-MAY-2005: added method for check user role
        /// <summary>
        /// Check if the property "this._role" belongs to the set "role"
        /// </summary>
        /// <param name="role">Set of the roles to cheking</param>
        /// <returns>Result of the checking</returns>
        public bool IsInRole(params UserRole[] role)
        {
            return IsInRole(UserContext.Current, role);
        }

        private bool IsInRole(UserContext user, params UserRole[] role)
        {
            if (user != null)
                return user.IsInRole(Logins.GetRoleName(role));
            else
                for (int i = 0; i < role.Length; i++) if (role[i] == this._role) return true;
            return false;
        }

        //MRUDKOVSKI: 19-MAY-2005 - Transfered the class to the place because it's public
        //MRUDKOVSKI: 19-MAY-2005 - Added appropriate structured commentaries
        //VGOOZ 12-AMY-2005: added method for check userglobal filters
        /// <summary>
        /// Check if the property "this._gf" belongs to the set "gfilter"
        /// </summary>
        /// <param name="gfilter">Set of the global filters to cheking</param>
        /// <returns>Result of the checking</returns>
        public bool IsGlobalFilter(params GlobalFilters.FilterState[] gfilter)
        {
            string[] _s = this._gf.Split(',');
            for (int i = 1; i < _s.Length; i++)
            {
                GlobalFilters.FilterState _gft;
                switch (_s[i])
                {
                    case "gf":
                        _gft = GlobalFilters.FilterState.EnabledGlobalFilters;
                        break;
                    case "tkt":
                        _gft = GlobalFilters.FilterState.LimitToAssignedTickets;
                        break;
                    case "rpt":
                        _gft = GlobalFilters.FilterState.DisabledReports;
                        break;
                    default:
                        continue;
                }
                for (int j = 0; j < gfilter.Length; j++) if (gfilter[j] == _gft) return true;
            }
            return false;
        }

        //Valeriy Gooz: 
        public bool BitCheckGFilter(string CheckList)
        {
            string GFilter = this.strGFilter;
            return fxValidatePerm(GFilter, CheckList);
        }

        public bool BitCheckPerm(string CheckList)
        {
            string GPerm = this.strGPerm;
            return fxValidatePerm(GPerm, CheckList);
        }

        public void ClearLoginSettings()
        {
            if (HttpContext.Current != null)
            {
                UserSetting.ClearSettings();
                DeleteFromUserContext();
            }
        }

        public static string CreateMD5Seed()
        {
            Random r = new Random();

            string leftPart = r.Next(99999).ToString();
            if (leftPart.Length < 5)
                leftPart = new string('0', 5 - leftPart.Length) + leftPart;

            string rightPart = r.Next(99999).ToString();
            if (rightPart.Length < 5)
                rightPart = new string('0', 5 - rightPart.Length) + rightPart;

            return leftPart + "." + rightPart;
        }

        public static string CreateMD5hash(string data)
        {
            string _result = string.Empty;

            MD5 _md5 = new MD5CryptoServiceProvider();

            if (_md5 != null)
            {
                System.Text.ASCIIEncoding _encoding = new System.Text.ASCIIEncoding();
                if (_encoding != null)
                {
                    byte[] _byte_array = _encoding.GetBytes(data);

                    byte[] _result_byte_array = _md5.ComputeHash(_byte_array);
                    if (_result_byte_array != null)
                    {
                        _result = ToHexString(_result_byte_array);
                        _result = _result.ToLower();
                    }
                }
            }

            return _result;
        }

        private static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {
                                '0', '1', '2', '3', '4', '5', '6', '7',
                                '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        public static int CreateLoginToken()
        {
            Random r = new Random();
            return (int)((2147483647 - 1001) * r.NextDouble() + 1000);
        }


        #endregion

        #region Private methods

        private void DeleteFromUserContext()
        {
            UserContext user = UserContext.Current;
            if (user != null)
                user["UserAuth"] = null;
        }

        private bool LoadFromUserContext()
        {
            UserContext user = UserContext.Current;
            if (user != null)
            {
                _tzi = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
                _tf = user.TimeFormat;
                _df = user.DateFormat;
                UserAuth auth = user["UserAuth"] as UserAuth;
                if (auth != null)
                {
                    this._tk = auth._tk;
                    this._ui = auth._ui;
                    this._di = auth._di;
                    this._oi = auth._oi;
                    this._ii = auth._ii;
                    this._cf = auth._cf;
                    this._em = auth._em;
                    this._pm = auth._pm;
                    this._lo = auth._lo;
                    this._dn = auth._dn;
                    this._fn = auth._fn;
                    this._ln = auth._ln;
                    this._sg = auth._sg;
                    this._ps = auth._ps;
                    this._fr = auth._fr;
                    this._tt = auth._tt;
                    this._st = auth._st;
                    this._si = auth._si;
                    this._sr = auth._sr;
                    this._sn = auth._sn;
                    this._role = auth._role;
                    this._gf = auth._gf;
                    this._password = auth._password;

                    return true;
                }
            }

            return false;
        }

        private void SaveToUserContext()
        {
            SaveToUserContext(UserContext.Current);
        }

        private void SaveToUserContext(UserContext user)
        {
            if (user != null)
                user["UserAuth"] = this;
        }

        private void ValidateHash(string hashString)
        {
            if (hashString != CreateMD5hash(this._password.Length > 0 ? this._password : this.strEmail))
            {
                this._errMessage = "Login hash string is invalid.";
                this.IsOk = false;
            }
        }

        /// <summary>
        /// Decrypt string data.
        /// The algorithm is from ASP 3.0 code
        /// </summary>
        /// <param name="fxstrText">String for decoding</param>
        /// <param name="fxstrEPwd">Passworf string</param>
        /// <returns>Decrypted string</returns>
        private string Decrypt(string fxstrText, string fxstrEPwd)
        {
            string fxstrBuff = "";
            string _fxstrText = HttpUtility.UrlDecode(fxstrText);
            if (_fxstrText != null)
            {
                for (int i = 0; i < _fxstrText.Length; i++)
                {
                    char c = _fxstrText[i];
                    char p = fxstrEPwd[(i + 1) % fxstrEPwd.Length];
                    c = (char)((c - p) & 255);
                    fxstrBuff = fxstrBuff + c;
                }
            }
            return fxstrBuff;
        }

        private string Encrypt(string _fxstrText, string fxstrEPwd)
        {
            string fxstrBuff = "";
            string fxstrText = _fxstrText;// System.Text.Encoding.GetEncoding(1252).GetString(System.Text.Encoding.Convert(System.Text.Encoding.UTF8, System.Text.Encoding.GetEncoding(1252), System.Text.Encoding.UTF8.GetBytes(_fxstrText)));
            if (fxstrText != null)
            {
                for (int i = 0; i < fxstrText.Length; i++)
                {
                    char c = fxstrText[i];
                    char p = fxstrEPwd[(i + 1) % fxstrEPwd.Length];
                    c = (char)((c + p) & 255);
                    fxstrBuff = fxstrBuff + c;
                }
            }

            return HttpUtility.UrlEncode(fxstrBuff);
        }

        private bool fxValidatePerm(string fxstrPerm, string fxstrChkLst)
        {
            bool fxbtReturn = false;
            string[] fxarrPerm = fxstrPerm.Split(',');
            string[] fxarrChkLst = fxstrChkLst.Split(',');
            for (int i = 0; (i < fxarrPerm.Length) && !fxbtReturn; i++)
            {
                for (int j = 0; (j < fxarrChkLst.Length) && !fxbtReturn; j++)
                {
                    fxbtReturn = (fxarrPerm[i].Trim() == fxarrChkLst[j].Trim());
                }
            }
            return fxbtReturn;
        }

        #endregion
    }
}
