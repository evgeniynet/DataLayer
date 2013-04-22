using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace bigWebApps.bigWebDesk
{
    /// <summary>
    /// The class provides fuctionality to work with an appropriate Custom Names list depending on company.
    /// </summary>
    public class CustomNames : Data.DBAccess
    {
        #region Local variables

        private CustomName _ticket = null; //Ticket – e.g. Work Order, Case, Issue
        private CustomName _account = null; //Account – e.g. Client, Customer
        private CustomName _technician = null; //Technician – e.g. Service Rep, Rep, Agent
        private CustomName _maintenance = null; //Scheduled Maintenance – e.g. Preventive Maintenance
        private CustomName _location = null; //Location – e.g. Sites, Building, Campus, Area
        private CustomName _enduser = null; //End User - e.g Requestor, Teacher/Staff, Client, Customer

        #endregion

        #region Global properties

        //Ticket property
        public CustomName Ticket { get { return _ticket; } }

        //Account property
        public CustomName Account { get { return _account; } }

        //Technician property
        public CustomName Technician { get { return _technician; } }

        //Maintenance property
        public CustomName Maintenance { get { return _maintenance; } }

        //Location property
        public CustomName Location { get { return _location; } }

        //Location property
        public CustomName EndUser { get { return _enduser; } }

        #endregion

        #region Public methods

        private CustomNames()
        {
            _ticket = new CustomName("Ticket", "Tickets", "Tkt", "Tkts");
            _account = new CustomName("Account", "Accounts", "Acc", "Accs");
            _technician = new CustomName("Technician", "Technicians", "Tech", "Techs");
            _maintenance = new CustomName("Scheduled Maintenance", "Scheduled Maintenances", "SM", "SMs");
            _location = new CustomName("Location", "Locations", "Loc", "Locs");
            _enduser = new CustomName("End User", "End Users", "User", "Users");
        }

        private CustomNames(Guid OrgID, int DeptID)
        {
            bool btCfgCUSN = false;
            Config config = Config.GetConfig(OrgID, DeptID);
            if (config != null)
                btCfgCUSN = config.CustomNames;

            DataTable _dt = SelectRecords("sp_SelectCustomNames", new SqlParameter[] { new SqlParameter("@CompanyId", (btCfgCUSN ? (object)DeptID : DBNull.Value)) }, OrgID);
            foreach (DataRow _row in _dt.Rows)
            {
                switch ((int)_row["TermId"])
                {
                    case 1: //Ticket
                        _ticket = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                    case 2: //Account
                        _account = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                    case 3: //Technician
                        _technician = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                    case 4: //Maintenance
                        _maintenance = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                    case 5: //Location
                        _location = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                    case 6: //End User
                        _enduser = new CustomName(_row["FullSingular"].ToString(), _row["FullPlural"].ToString(), _row["AbbreviatedSingular"].ToString(), _row["AbbreviatedPlural"].ToString());
                        break;
                }
            }
        }

        public static CustomNames GetCustomNames()
        {
            return new CustomNames();
        }

        public static CustomNames GetCustomNames(int departmentId)
        {
            return GetCustomNames(Guid.Empty, departmentId);
        }

        public static CustomNames GetCustomNames(Guid OrgID, int departmentId)
        {
            if (System.Web.HttpContext.Current == null) return new CustomNames(OrgID, departmentId);

            CustomNames returnValue;

            returnValue = (CustomNames)System.Web.HttpContext.Current.Cache.Get("CustomNamesDepartment_" + GetCurrentOrgID(OrgID).ToString() + "_" + departmentId);

            if (returnValue == null)
            {
                returnValue = new CustomNames(OrgID, departmentId);
                System.Web.HttpContext.Current.Cache.Add("CustomNamesDepartment_" + GetCurrentOrgID(OrgID).ToString() + "_" + departmentId, returnValue, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(1), System.Web.Caching.CacheItemPriority.Normal, null);
            }

            return returnValue;
        }

        public static void ClearCache(Guid OrgID, int departmentId)
        {
            System.Web.HttpContext.Current.Cache.Remove("CustomNamesDepartment_" + GetCurrentOrgID(OrgID).ToString() + "_" + departmentId);
        }

        /// <summary>
        /// Constructor of the class. Initializes all custom names.
        /// </summary>
        /// <param name="ticketName">Custom name for "Ticket"</param>
        /// <param name="accountName">Custom name for "Account"</param>
        /// <param name="technicianName">Custom name for "Technician"</param>
        /// <param name="maintenanceName">Custom name for "Maintenance"</param>
        /// <param name="locationName">Custom name for "Location"</param>
        public CustomNames(CustomName ticketName, CustomName accountName, CustomName technicianName, CustomName maintenanceName, CustomName locationName, CustomName endUserName)
        {
            //store all names to local variables
            _ticket = ticketName;
            _account = accountName;
            _technician = technicianName;
            _maintenance = maintenanceName;
            _location = locationName;
            _enduser = endUserName;
        }

        public string Replace(string value)
        {
            Type customNameType = typeof(CustomName);
            foreach (PropertyInfo p1 in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (p1.PropertyType == customNameType)
                {
                    object obj1 = p1.GetValue(this, null);
                    foreach (PropertyInfo p2 in customNameType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        object obj2 = p2.GetValue(obj1, null);
                        if (obj2 != null)
                            value = value.Replace("$" + p1.Name + "-" + p2.Name + "$", obj2.ToString());
                    }
                }
            }

            return value;
        }

        #endregion
    }

    /// <summary>
    /// The class provides fuctionality to work with one item of the Custom Names list.
    /// </summary>
    public class CustomName
    {
        #region Local variables

        private string _fullSingular;
        private string _fullPlural;
        private string _abbreviatedSingular;
        private string _abbreviatedPlural;

        #endregion

        #region Global properties

        public string FullSingular { get { return _fullSingular; } }
        public string FullPlural { get { return _fullPlural; } }
        public string AbbreviatedSingular { get { return _abbreviatedSingular; } }
        public string AbbreviatedPlural { get { return _abbreviatedPlural; } }

        public string fullSingular { get { return _fullSingular.ToLower(); } }
        public string fullPlural { get { return _fullPlural.ToLower(); } }
        public string abbreviatedSingular { get { return _abbreviatedSingular.ToLower(); } }
        public string abbreviatedPlural { get { return _abbreviatedPlural.ToLower(); } }

        #endregion

        #region Public methods

        public CustomName(string Fullsingular, string Fullplural, string Abbreviatedsingular, string Abbreviatedplural)
        {
            _fullSingular = Fullsingular;
            _fullPlural = Fullplural;
            _abbreviatedSingular = Abbreviatedsingular;
            _abbreviatedPlural = Abbreviatedplural;
        }

        #endregion
    }
}
