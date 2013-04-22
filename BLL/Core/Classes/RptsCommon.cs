using System;
using System.Data;
using System.Web;
using System.Globalization;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public enum ReportType : int
    {
        NotSet = -1,
        TicketCount = 0,
        SLAPriority = 1,
        TimeCost = 2
    }

    public enum DateRange : int
    {
        Custom = 0,
        Today = 1,
        ThisWeek = 2,
        ThisMonth = 3,
        ThisYear = 4,
        ThisFiscalYear = 5,
        LastWeek = 6,
        LastMonth = 7,
        LastYear = 8,
        LastFiscalYear = 9,
        Rolling30Days = 10,
        Rolling90Days = 11,
        Rolling365Days = 12
    }

    public enum EqualRange
    {
        Less,
        Equal,
        Greater
    }

    public enum Grouping
    {
        None,
        Account,
        Class,
        CreationCategory,
        Location,
        Month,
        Priority,
        ResolutionCategory,
        SubmissionCategory,
        Technician,
        AccountLocation,
        TicketLevel,
        SupportGroup
    }

    public enum TechnicianType
    {
        All,
        Global,
        Filtered,
        CallCenterRep
    }

    public enum HandledByCallCenter : int
    {
        All = -1,
        Yes = 1,
        No = 0
    }

    public class Fltr : DBAccess, ICloneable
    {
        private int _id = 0;
        private string _name = string.Empty;
        private ReportType _reptype = ReportType.NotSet;
        private DateRange _range = DateRange.Custom;
        private DateTime _start = DateTime.UtcNow.Date;
        private DateTime _end = DateTime.UtcNow;
        private Grouping _yaxis = Grouping.None;
        private Grouping _subyaxis = Grouping.None;
        private EqualRange _age_equal = EqualRange.Less;
        private TechnicianType technicianType = TechnicianType.All;
        private HandledByCallCenter handledByCallCenter = HandledByCallCenter.All;
        private int _priority = 0;
        private int _class = 0;
        private int _classlevel = 0;
        private bool _classnull = false;
        private int _subclasslevel = 0;
        private int _creationcategory = 0;
        private int _submissioncat = 0;
        private int _resolutioncat = 0;
        private int _location = 0;
        private int _locationtype = 0;
        private int _sublocationtype = 0;
        private int _technician = 0;
        private int _submittedby = 0;
        private int _closedby = 0;
        private int _account = 0;
        private int _accountLocation = 0;
        private int _accountParentLocation = 0; // This filter includes all sublocations of specified location.
        private int _did = 0;
        private int _uid = 0;
        private int _houroffset = 0;
        private int _month = 0;
        private int _age = -1;
        //tkt #3949: Level Filter added to Ticket Count Report
        private int _ticket_level = 0;
        //tkt #3632: Add Support Groups to Ticket Count Report criteria
        private int _support_group = 0;

        private string _asset_filter = string.Empty;

        private int _sla_graph_width_id = 0;
        private int _sla_graph_view_id = 0;


        public static bool NoData()
        {
            return UserSetting.GetSettings("RPT") == null || String.IsNullOrEmpty(UserSetting.GetSettings("RPT")["FID"]);
        }

        public static Fltr Load()
        {
            return new Fltr(0, 0, 0);
        }

        public static void Del()
        {
            UserSetting.RemoveSettings("RPT");
        }

        public static Fltr Get(int DId, int Id)
        {
            return new Fltr(DId, Id);
        }


        private static DataRow SelectFilter(int DeptID, int Id)
        {
            SqlParameter _pUId = new SqlParameter("@UId", SqlDbType.Int);
            _pUId.Value = DBNull.Value;
            SqlParameter _pReportType = new SqlParameter("@ReportType", SqlDbType.TinyInt);
            _pReportType.Value = DBNull.Value;
            return SelectRecord("sp_SelectReportFilters", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pUId, _pReportType, new SqlParameter("@Id", Id) });
        }

        public Fltr(int DId, int Id)
        {
            _did = DId;
            _id = Id;
            DataRow _row = SelectFilter(DId, _id);
            _uid = (int)_row["UId"];
            if (_row == null) return;
            _name = _row["Name"].ToString();
            _reptype = (ReportType)(byte)_row["ReportType"];
            string[] _state = _row["FilterState"].ToString().Split('&');
            System.Collections.Specialized.StringDictionary _sd = new System.Collections.Specialized.StringDictionary();
            for (int i = 0; i < _state.Length; i++)
            {
                if (_state[i].Length == 0) continue;
                string[] _item = _state[i].Split('=');
                if (_item.Length > 1) _sd.Add(_item[0], _item[1]);
                else _sd.Add(_item[0], string.Empty);
            }
            string[] expectedFormats = { "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyyHH:mm:ss", "MM.dd.yyyy HH:mm:ss", "MM.dd.yyyyHH:mm:ss" };
            IFormatProvider culture = new CultureInfo("en-US", false);
            if (_sd.ContainsKey("ds") && _sd["ds"].Length > 0) _start = DateTime.ParseExact(HttpUtility.UrlDecode(_sd["ds"]), expectedFormats, culture, DateTimeStyles.None);
            if (_sd.ContainsKey("de") && _sd["de"].Length > 0) _end = DateTime.ParseExact(HttpUtility.UrlDecode(_sd["de"]), expectedFormats, culture, DateTimeStyles.None);
            if (_sd.ContainsKey("dr") && _sd["dr"].Length > 0) this.DateRange = ConvertStringToRange(HttpUtility.UrlDecode(_sd["dr"]));
            if (_sd.ContainsKey("ya") && _sd["ya"].Length > 0)
            {
                string _val = HttpUtility.UrlDecode(_sd["ya"]);
                if (_val.IndexOf(Grouping.Location.ToString()) >= 0)
                {
                    _yaxis = Grouping.Location;
                    _locationtype = int.Parse(_val.Split(',')[1]);
                }
                else if (_val.IndexOf(Grouping.Class.ToString()) >= 0)
                {
                    _yaxis = Grouping.Class;
                    string[] _arr = _val.Split(',');
                    if (_arr.Length > 1) _classlevel = int.Parse(_arr[1]);
                }
                else _yaxis = (Grouping)Enum.Parse(typeof(Grouping), _val, true);
            }
            if (_sd.ContainsKey("sya") && _sd["sya"].Length > 0)
            {
                string _val = HttpUtility.UrlDecode(_sd["sya"]);
                if (_val.IndexOf(Grouping.Location.ToString()) >= 0)
                {
                    _subyaxis = Grouping.Location;
                    _sublocationtype = int.Parse(_val.Split(',')[1]);
                }
                else if (_val.IndexOf(Grouping.Class.ToString()) >= 0)
                {
                    _subyaxis = Grouping.Class;
                    string[] _arr = _val.Split(',');
                    if (_arr.Length > 1) _subclasslevel = int.Parse(_arr[1]);
                }
                else _subyaxis = (Grouping)Enum.Parse(typeof(Grouping), _val, true);
            }
            if (_sd.ContainsKey("prt") && _sd["prt"].Length > 0) _priority = int.Parse(_sd["prt"]);
            if (_sd.ContainsKey("cls") && _sd["cls"].Length > 0) _class = int.Parse(_sd["cls"]);
            if (_sd.ContainsKey("ctg") && _sd["ctg"].Length > 0) _creationcategory = int.Parse(_sd["ctg"]);
            if (_sd.ContainsKey("stg") && _sd["stg"].Length > 0) _submissioncat = int.Parse(_sd["stg"]);
            if (_sd.ContainsKey("rtg") && _sd["rtg"].Length > 0) _resolutioncat = int.Parse(_sd["rtg"]);
            if (_sd.ContainsKey("lct") && _sd["lct"].Length > 0) _location = int.Parse(_sd["lct"]);
            if (_sd.ContainsKey("tch") && _sd["tch"].Length > 0) _technician = int.Parse(_sd["tch"]);
            if (_sd.ContainsKey("sby") && _sd["sby"].Length > 0) _submittedby = int.Parse(_sd["sby"]);
            if (_sd.ContainsKey("cby") && _sd["cby"].Length > 0) _closedby = int.Parse(_sd["cby"]);
            if (_sd.ContainsKey("acc") && _sd["acc"].Length > 0) _account = int.Parse(_sd["acc"]);
            if (_sd.ContainsKey("accl") && _sd["accl"].Length > 0) _accountLocation = int.Parse(_sd["accl"]);
            if (_sd.ContainsKey("accpl") && _sd["accpl"].Length > 0) _accountParentLocation = int.Parse(_sd["accpl"]);
            if (_sd.ContainsKey("tcht") && _sd["tcht"].Length > 0) technicianType = (TechnicianType)Enum.Parse(typeof(TechnicianType), HttpUtility.UrlDecode(_sd["tcht"]), true);
            if (_sd.ContainsKey("hcc") && _sd["hcc"].Length > 0) handledByCallCenter = (HandledByCallCenter)Enum.Parse(typeof(HandledByCallCenter), HttpUtility.UrlDecode(_sd["hcc"]), true);
            if (_sd.ContainsKey("lvl") && _sd["lvl"].Length > 0) _ticket_level = int.Parse(_sd["lvl"]);
            if (_sd.ContainsKey("sg") && _sd["sg"].Length > 0) _support_group = int.Parse(_sd["sg"]);
            if (_sd.ContainsKey("age") && _sd["age"].Length > 0) _age = int.Parse(_sd["age"]);
            if (_sd.ContainsKey("ager") && _sd["ager"].Length > 0) _age_equal = (EqualRange)Enum.Parse(typeof(EqualRange), HttpUtility.UrlDecode(_sd["ager"]), true);
            if (_sd.ContainsKey("ass") && _sd["ass"].Length > 0) _asset_filter = HttpUtility.UrlDecode(_sd["ass"]);
            if (_sd.ContainsKey("slaw") && _sd["slaw"].Length > 0) _sla_graph_width_id = int.Parse(_sd["slaw"]);
            if (_sd.ContainsKey("slag") && _sd["slag"].Length > 0) _sla_graph_view_id = int.Parse(_sd["slag"]);
        }

        public Fltr(int DId, int UId, int HourOffset)
        {
            _did = DId;
            _uid = UId;

            this.LoadReportDefaults(_did, _uid, false);

            _houroffset = HourOffset;
            _end = _end.AddHours(-_houroffset);
            _start = _start.AddMonths(-1).AddHours(-_houroffset);
            UserSetting _c = UserSetting.GetSettings("RPT");
            if (_c == null) return;

            if (!string.IsNullOrEmpty(_c["FID"])) _id = int.Parse(_c["FID"]);
            if (!string.IsNullOrEmpty(_c["FNAME"])) _name = HttpUtility.UrlDecode(_c["FNAME"]);
            if (!string.IsNullOrEmpty(_c["FTYPE"])) _reptype = (ReportType)Enum.Parse(typeof(ReportType), _c["FTYPE"], true);

            string[] expectedFormats = { "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyyHH:mm:ss", "MM.dd.yyyy HH:mm:ss", "MM.dd.yyyyHH:mm:ss" };
            IFormatProvider culture = new CultureInfo("en-US", false);

            if (!string.IsNullOrEmpty(_c["STDT"]))
            {
                try
                {
                    string _str_start = HttpUtility.UrlDecode(_c["STDT"]);
                    _start = DateTime.ParseExact(_str_start, expectedFormats, culture, DateTimeStyles.None);
                }
                catch { }
            };

            if (!string.IsNullOrEmpty(_c["ENDT"]))
            {
                try
                {
                    string _str_end = HttpUtility.UrlDecode(_c["ENDT"]);
                    _end = DateTime.ParseExact(_str_end, expectedFormats, culture, DateTimeStyles.None);
                }
                catch { }
            };

            if (!string.IsNullOrEmpty(_c["RANGE"])) this.DateRange = ConvertStringToRange(_c["RANGE"]);//(DateRange)Enum.Parse(typeof(DateRange),_c["RANGE"],true);
            if (!string.IsNullOrEmpty(_c["SLAYAX"]))
            {
                if (_c["SLAYAX"].IndexOf(Grouping.AccountLocation.ToString()) >= 0)
                {
                    _yaxis = Grouping.AccountLocation;
                }
                else if (_c["SLAYAX"].IndexOf(Grouping.Location.ToString()) >= 0)
                {
                    _yaxis = Grouping.Location;
                    _locationtype = int.Parse(HttpUtility.UrlDecode(_c["SLAYAX"]).Split(',')[1]);
                }
                else if (_c["SLAYAX"].IndexOf(Grouping.Class.ToString()) >= 0)
                {
                    _yaxis = Grouping.Class;
                    string[] _arr = HttpUtility.UrlDecode(_c["SLAYAX"]).Split(',');
                    if (_arr.Length > 1) _classlevel = int.Parse(_arr[1]);
                }
                else _yaxis = (Grouping)Enum.Parse(typeof(Grouping), _c["SLAYAX"], true);
            }
            if (!string.IsNullOrEmpty(_c["SLASYAX"]))
            {
                if (_c["SLASYAX"].IndexOf(Grouping.AccountLocation.ToString()) >= 0)
                {
                    _subyaxis = Grouping.AccountLocation;
                }
                else if (_c["SLASYAX"].IndexOf(Grouping.Location.ToString()) >= 0)
                {
                    _subyaxis = Grouping.Location;
                    _sublocationtype = int.Parse(HttpUtility.UrlDecode(_c["SLASYAX"]).Split(',')[1]);
                }
                else if (_c["SLASYAX"].IndexOf(Grouping.Class.ToString()) >= 0)
                {
                    _subyaxis = Grouping.Class;
                    string[] _arr = HttpUtility.UrlDecode(_c["SLASYAX"]).Split(',');
                    if (_arr.Length > 1) _subclasslevel = int.Parse(_arr[1]);
                }
                else _subyaxis = (Grouping)Enum.Parse(typeof(Grouping), _c["SLASYAX"], true);
            }
            if (!string.IsNullOrEmpty(_c["SLAPRT"])) _priority = int.Parse(_c["SLAPRT"]);
            if (!string.IsNullOrEmpty(_c["SLACLS"])) _class = int.Parse(_c["SLACLS"]);
            if (!string.IsNullOrEmpty(_c["SLACTG"])) _creationcategory = int.Parse(_c["SLACTG"]);
            if (!string.IsNullOrEmpty(_c["SLASTG"])) _submissioncat = int.Parse(_c["SLASTG"]);
            if (!string.IsNullOrEmpty(_c["SLARTG"])) _resolutioncat = int.Parse(_c["SLARTG"]);
            if (!string.IsNullOrEmpty(_c["SLALCT"])) _location = int.Parse(_c["SLALCT"]);
            if (!string.IsNullOrEmpty(_c["SLATCH"])) _technician = int.Parse(_c["SLATCH"]);
            if (!string.IsNullOrEmpty(_c["SLASBY"])) _submittedby = int.Parse(_c["SLASBY"]);
            if (!string.IsNullOrEmpty(_c["SLACBY"])) _closedby = int.Parse(_c["SLACBY"]);
            if (!string.IsNullOrEmpty(_c["SLAACC"])) _account = int.Parse(_c["SLAACC"]);
            if (!string.IsNullOrEmpty(_c["SLAACCL"])) _accountLocation = int.Parse(_c["SLAACCL"]);
            if (!string.IsNullOrEmpty(_c["SLAACCPL"])) _accountParentLocation = int.Parse(_c["SLAACCPL"]);

            if (!string.IsNullOrEmpty(_c["TECHTYPE"])) technicianType = (TechnicianType)Enum.Parse(typeof(TechnicianType), _c["TECHTYPE"], true);
            if (!string.IsNullOrEmpty(_c["CALLCENTER"])) handledByCallCenter = (HandledByCallCenter)Enum.Parse(typeof(HandledByCallCenter), _c["CALLCENTER"], true);

            //tkt #3949: Level Filter added to Ticket Count Report
            if (!string.IsNullOrEmpty(_c["TICKETLEVEL"])) _ticket_level = int.Parse(_c["TICKETLEVEL"]);
            //tkt #3632: Add Support Groups to Ticket Count Report criteria
            if (!string.IsNullOrEmpty(_c["SUPPORTGROUP"])) _support_group = int.Parse(_c["SUPPORTGROUP"]);

            if (!string.IsNullOrEmpty(_c["AGEDAY"])) _age = int.Parse(HttpUtility.UrlDecode(_c["AGEDAY"]));
            if (!string.IsNullOrEmpty(_c["AGERANGE"])) this.AgeRange = (EqualRange)Enum.Parse(typeof(EqualRange), _c["AGERANGE"], true);

            if (!string.IsNullOrEmpty(_c["ASSETFILTER"])) _asset_filter = HttpUtility.UrlDecode(_c["ASSETFILTER"]);

            if (!string.IsNullOrEmpty(_c["SLAGRAPHWIDTH"])) _sla_graph_width_id = int.Parse(_c["SLAGRAPHWIDTH"]);
            if (!string.IsNullOrEmpty(_c["SLAGRAPHVIEW"])) _sla_graph_view_id = int.Parse(_c["SLAGRAPHVIEW"]);
        }

        public Fltr(int DId, int UId, int HourOffset, DateTime Start, DateTime End)
            : this(DId, UId, HourOffset)
        {
            _start = Start;
            _end = End;
        }

        public Fltr(int DId, int UId, int HourOffset, DateTime Start, DateTime End, DateRange Range)
            : this(DId, UId, HourOffset, Start, End)
        {
            this.DateRange = Range;
        }

        public Fltr(int DId, int UId, int HourOffset, DateTime Start, DateTime End, DateRange Range, Grouping YAxis)
            : this(DId, UId, HourOffset, Start, End, Range)
        {
            _yaxis = YAxis;
        }

        public Fltr(int DId, int UId, int HourOffset, DateTime Start, DateTime End, DateRange Range, Grouping YAxis, Grouping SubYAxis, int PriorityID, int ClassID, int ClassLevel, int SubClassLevel, int CreationCategoryID, int LocationID, int LocationTypeID, int SubLocationTypeID, int TechnicianID, int SubmittedByID, int ClosedByID, int AccountID, int AccountLocationId, int accountParentLocationId, int SubmissionCategoryID, int ResolutionCategoryID, int AgeDay, EqualRange AgeRange, int TicketLevelID, int SupportGroupID, TechnicianType technicianType, HandledByCallCenter handledByCallCenter, string asset_filter)
            : this(DId, UId, HourOffset, Start, End, Range, YAxis)
        {
            _subyaxis = SubYAxis;
            _priority = PriorityID;
            _class = ClassID;
            _classlevel = ClassLevel;
            _subclasslevel = SubClassLevel;
            _creationcategory = CreationCategoryID;
            _submissioncat = SubmissionCategoryID;
            _resolutioncat = ResolutionCategoryID;
            _location = LocationID;
            _locationtype = LocationTypeID;
            _sublocationtype = SubLocationTypeID;
            _technician = TechnicianID;
            _submittedby = SubmittedByID;
            _closedby = ClosedByID;
            _account = AccountID;
            _accountLocation = AccountLocationId;
            _accountParentLocation = accountParentLocationId;
            _age = AgeDay;
            _age_equal = AgeRange;
            this.technicianType = technicianType;
            this.handledByCallCenter = handledByCallCenter;
            //tkt #3949: Level Filter added to Ticket Count Report
            _ticket_level = TicketLevelID;
            //tkt #3632: Add Support Groups to Ticket Count Report criteria
            _support_group = SupportGroupID;

            _asset_filter = asset_filter;
        }

        public Fltr(int DId, int UId, int HourOffset, DateTime Start, DateTime End, DateRange Range, Grouping YAxis, Grouping SubYAxis, int PriorityID, int ClassID, int ClassLevel, int SubClassLevel, int CreationCategoryID, int LocationID, int LocationTypeID, int SubLocationTypeID, int TechnicianID, int SubmittedByID, int ClosedByID, int AccountID, int AccountLocationId, int accountParentLocationId, int SubmissionCategoryID, int ResolutionCategoryID, int AgeDay, EqualRange AgeRange, int TicketLevelID, int SupportGroupID, TechnicianType technicianType, HandledByCallCenter handledByCallCenter, string asset_filter, int sla_graph_width_id, int sla_graph_view_id)
            : this(DId, UId, HourOffset, Start, End, Range, YAxis, SubYAxis, PriorityID, ClassID, ClassLevel, SubClassLevel, CreationCategoryID, LocationID, LocationTypeID, SubLocationTypeID, TechnicianID, SubmittedByID, ClosedByID, AccountID, AccountLocationId, accountParentLocationId, SubmissionCategoryID, ResolutionCategoryID, AgeDay, AgeRange, TicketLevelID, SupportGroupID, technicianType, handledByCallCenter, asset_filter)
        {
            _sla_graph_width_id = sla_graph_width_id;
            _sla_graph_view_id = sla_graph_view_id;

        }

        public string GetQueryString()
        {
            string _res = string.Empty;
            _res += "&ds=" + HttpUtility.UrlEncode(_start.ToString("MM/dd/yyyy HH:mm:ss"));
            _res += "&de=" + HttpUtility.UrlEncode(_end.ToString("MM/dd/yyyy HH:mm:ss"));
            _res += "&dr=" + HttpUtility.UrlEncode(ConvertRangeToString(_range));
            if (_yaxis == Grouping.Location) _res += "&ya=" + HttpUtility.UrlEncode(Grouping.Location.ToString() + "," + LocationTypeID.ToString());
            else if (_yaxis == Grouping.Class) _res += "&ya=" + HttpUtility.UrlEncode(Grouping.Class.ToString() + "," + ClassLevel.ToString());
            else _res += "&ya=" + HttpUtility.UrlEncode(_yaxis.ToString());
            if (_subyaxis == Grouping.Location) _res += "&sya=" + HttpUtility.UrlEncode(Grouping.Location.ToString() + "," + SubLocationTypeID.ToString());
            else if (_subyaxis == Grouping.Class) _res += "&sya=" + HttpUtility.UrlEncode(Grouping.Class.ToString() + "," + SubClassLevel.ToString());
            else _res += "&sya=" + HttpUtility.UrlEncode(_subyaxis.ToString());
            if (_priority != 0) _res += "&prt=" + _priority.ToString();
            if (_class != 0) _res += "&cls=" + _class.ToString();
            if (_creationcategory != 0) _res += "&ctg=" + _creationcategory.ToString();
            if (_submissioncat != 0) _res += "&stg=" + _submissioncat.ToString();
            if (_resolutioncat != 0) _res += "&rtg=" + _resolutioncat.ToString();
            if (_location != 0) _res += "&lct=" + _location.ToString();
            if (_technician != 0) _res += "&tch=" + _technician.ToString();
            if (_submittedby != 0) _res += "&sby=" + _submittedby.ToString();
            if (_closedby != 0) _res += "&cby=" + _closedby.ToString();
            if (_account != 0) _res += "&acc=" + _account.ToString();
            if (_accountLocation != 0) _res += "&accl=" + _accountLocation.ToString();
            if (_accountParentLocation != 0) _res += "&accpl=" + _accountParentLocation.ToString();
            _res += "&tcht=" + HttpUtility.UrlEncode(technicianType.ToString());
            _res += "&hcc=" + HttpUtility.UrlEncode(handledByCallCenter.ToString());
            if (_ticket_level != 0) _res += "&lvl=" + _ticket_level.ToString();
            if (_support_group != 0) _res += "&sg=" + _support_group.ToString();
            if (_age >= 0) _res += "&age=" + _age.ToString();
            _res += "&ager=" + HttpUtility.UrlEncode(_age_equal.ToString());
            if (_asset_filter.Length > 0) _res += "&ass=" + HttpUtility.UrlEncode(_asset_filter);
            if (_sla_graph_width_id != 0) _res += "&slaw=" + _sla_graph_width_id.ToString();
            if (_sla_graph_view_id != 0) _res += "&slag=" + _sla_graph_view_id.ToString();
            return _res.TrimStart('&');
        }

        public void Save()
        {
            UserSetting _c = UserSetting.GetSettings("RPT");
            _c["FID"] = _id.ToString();
            _c["FNAME"] = HttpUtility.UrlDecode(_name);
            _c["FTYPE"] = _reptype.ToString();
            _c["STDT"] = _start.ToString("MM/dd/yyyy HH:mm:ss");
            _c["ENDT"] = _end.ToString("MM/dd/yyyy HH:mm:ss");
            _c["RANGE"] = ConvertRangeToString(_range);
            if (_yaxis == Grouping.Location) _c["SLAYAX"] = Grouping.Location.ToString() + "," + LocationTypeID.ToString();
            else if (_yaxis == Grouping.Class) _c["SLAYAX"] = Grouping.Class.ToString() + "," + ClassLevel.ToString();
            else _c["SLAYAX"] = _yaxis.ToString();
            if (_subyaxis == Grouping.Location) _c["SLASYAX"] = Grouping.Location.ToString() + "," + SubLocationTypeID.ToString();
            else if (_subyaxis == Grouping.Class) _c["SLASYAX"] = Grouping.Class.ToString() + "," + SubClassLevel.ToString();
            else _c["SLASYAX"] = _subyaxis.ToString();
            _c["SLAPRT"] = _priority.ToString();
            _c["SLACLS"] = _class.ToString();
            _c["SLACTG"] = _creationcategory.ToString();
            _c["SLASTG"] = _submissioncat.ToString();
            _c["SLARTG"] = _resolutioncat.ToString();
            _c["SLALCT"] = _location.ToString();
            _c["SLATCH"] = _technician.ToString();
            _c["SLASBY"] = _submittedby.ToString();
            _c["SLACBY"] = _closedby.ToString();
            _c["SLAACC"] = _account.ToString();
            _c["SLAACCL"] = _accountLocation.ToString();
            _c["SLAACCPL"] = _accountParentLocation.ToString();

            _c["TECHTYPE"] = technicianType.ToString();
            _c["CALLCENTER"] = handledByCallCenter.ToString();

            //tkt #3949: Level Filter added to Ticket Count Report
            _c["TICKETLEVEL"] = _ticket_level.ToString();
            //tkt #3632: Add Support Groups to Ticket Count Report criteria
            _c["SUPPORTGROUP"] = _support_group.ToString();

            _c["AGEDAY"] = _age.ToString();
            _c["AGERANGE"] = _age_equal.ToString();

            _c["ASSETFILTER"] = _asset_filter;

            _c["SLAGRAPHWIDTH"] = _sla_graph_width_id.ToString();
            _c["SLAGRAPHVIEW"] = _sla_graph_view_id.ToString();
        }

        private string ConvertRangeIdToString(int x_range_id)
        {
            string result = "";

            switch (x_range_id)
            {
                case 0:
                    result = "Custom";
                    break;
                case 1:
                    result = "Today";
                    break;
                case 2:
                    result = "ThisWeek";
                    break;
                case 3:
                    result = "ThisMonth";
                    break;
                case 4:
                    result = "ThisYear";
                    break;
                case 5:
                    result = "ThisFiscalYear";
                    break;
                case 6:
                    result = "LastWeek";
                    break;
                case 7:
                    result = "LastMonth";
                    break;
                case 8:
                    result = "LastYear";
                    break;
                case 9:
                    result = "LastFiscalYear";
                    break;
                case 10:
                    result = "Rolling30Days";
                    break;
                case 11:
                    result = "Rolling90Days";
                    break;
                case 12:
                    result = "Rolling365Days";
                    break;
            };

            return result;
        }

        private DateRange ConvertStringToRange(string x_range_value)
        {
            DateRange result = DateRange.Custom;

            switch (x_range_value)
            {
                case "Custom":
                    result = DateRange.Custom;
                    break;
                case "Today":
                    result = DateRange.Today;
                    break;
                case "ThisWeek":
                    result = DateRange.ThisWeek;
                    break;
                case "ThisMonth":
                    result = DateRange.ThisMonth;
                    break;
                case "ThisYear":
                    result = DateRange.ThisYear;
                    break;
                case "ThisFiscalYear":
                    result = DateRange.ThisFiscalYear;
                    break;
                case "LastWeek":
                    result = DateRange.LastWeek;
                    break;
                case "LastMonth":
                    result = DateRange.LastMonth;
                    break;
                case "LastYear":
                    result = DateRange.LastYear;
                    break;
                case "LastFiscalYear":
                    result = DateRange.LastFiscalYear;
                    break;
                case "Rolling30Days":
                    result = DateRange.Rolling30Days;
                    break;
                case "Rolling90Days":
                    result = DateRange.Rolling90Days;
                    break;
                case "Rolling365Days":
                    result = DateRange.Rolling365Days;
                    break;
            };

            return result;
        }

        private string ConvertRangeToString(DateRange x_range_value)
        {
            string result = "";

            switch (x_range_value)
            {
                case DateRange.Custom:
                    result = "Custom";
                    break;
                case DateRange.Today:
                    result = "Today";
                    break;
                case DateRange.ThisWeek:
                    result = "ThisWeek";
                    break;
                case DateRange.ThisMonth:
                    result = "ThisMonth";
                    break;
                case DateRange.ThisYear:
                    result = "ThisYear";
                    break;
                case DateRange.ThisFiscalYear:
                    result = "ThisFiscalYear";
                    break;
                case DateRange.LastWeek:
                    result = "LastWeek";
                    break;
                case DateRange.LastMonth:
                    result = "LastMonth";
                    break;
                case DateRange.LastYear:
                    result = "LastYear";
                    break;
                case DateRange.LastFiscalYear:
                    result = "LastFiscalYear";
                    break;
                case DateRange.Rolling30Days:
                    result = "Rolling30Days";
                    break;
                case DateRange.Rolling90Days:
                    result = "Rolling90Days";
                    break;
                case DateRange.Rolling365Days:
                    result = "Rolling365Days";
                    break;
            };

            return result;
        }

        public void ResetToDefaults()
        {
            _id = 0;
            _name = string.Empty;
            _reptype = ReportType.NotSet;
            _range = DateRange.Custom;
            _start = DateTime.UtcNow.Date;
            _end = DateTime.UtcNow;
            _yaxis = Grouping.None;
            _subyaxis = Grouping.None;
            _age_equal = EqualRange.Less;
            technicianType = TechnicianType.All;
            handledByCallCenter = HandledByCallCenter.All;
            _priority = 0;
            _class = 0;
            _classlevel = 0;
            _classnull = false;
            _subclasslevel = 0;
            _creationcategory = 0;
            _submissioncat = 0;
            _resolutioncat = 0;
            _location = 0;
            _locationtype = 0;
            _sublocationtype = 0;
            _technician = 0;
            _submittedby = 0;
            _closedby = 0;
            _account = 0;
            _accountLocation = 0;
            _accountParentLocation = 0;
            _month = 0;
            _age = -1;
            _ticket_level = 0;
            _support_group = 0;
            _asset_filter = string.Empty;
            _sla_graph_width_id = 0;
            _sla_graph_view_id = 0;
            LoadReportDefaults(_did, _uid, true);
            _end = _end.AddHours(-_houroffset);
            _start = _start.AddMonths(-1).AddHours(-_houroffset);
        }

        private void LoadReportDefaults(int DId, int UId, bool ForceLoad)
        {
            bool isUploadDefaults = false;

            string _range = "";

            UserSetting _r = UserSetting.GetSettings("RPT");
            UserSetting _w = UserSetting.GetSettings("RPT");
            if (ForceLoad) isUploadDefaults = true;
            else
            {
                if (_r == null)
                    isUploadDefaults = true;
                else
                {
                    if (_r["RANGE"] == null)
                        isUploadDefaults = true;
                    else
                    {
                        if (_r["RANGE"].Length == 0)
                            isUploadDefaults = true;

                        if (isUploadDefaults == false)
                        {
                            if ((_r["STDT"] == null) || (_r["ENDT"] == null))
                                isUploadDefaults = true;
                            else
                            {
                                if ((_r["STDT"].Length == 0) && (_r["ENDT"].Length == 0))
                                    isUploadDefaults = true;
                            };
                        }
                        else
                            isUploadDefaults = true;
                    };
                };
            }

            if (isUploadDefaults == true)
            {
                _range = "10";
                _w["RANGE"] = ConvertRangeIdToString(Int32.Parse(_range));

                _w["STDT"] = DateTime.UtcNow.Date.AddYears(-1).ToString("MM/dd/yyyy HH:mm:ss");

                _w["ENDT"] = DateTime.UtcNow.Date.ToString("MM/dd/yyyy HH:mm:ss");

                if (_range.Length > 0) this.DateRange = (DateRange)Enum.Parse(typeof(DateRange), _range, true);

                _w["IsConfig"] = "";
            }
        }

        public int ID
        {
            get { return _id; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public ReportType ReportType
        {
            get { return _reptype; }
            set { _reptype = value; }
        }

        public TechnicianType TechnicianType
        {
            get { return technicianType; }
            set { technicianType = value; }
        }

        public HandledByCallCenter HandledByCallCenter
        {
            get { return handledByCallCenter; }
            set { handledByCallCenter = value; }
        }

        public DateTime StartDate
        {
            get { return _start; }
            set { _start = value; }
        }

        public DateTime EndDate
        {
            get { return _end; }
            set { _end = value; }
        }

        public DateRange DateRange
        {
            get { return _range; }
            set
            {
                _range = value;
                if (_range == DateRange.Custom) return;
                DateTime _today = DateTime.UtcNow.Date; //today date
                int _dayOfWeek = (int)_today.AddDays(-1).DayOfWeek; //today's day of week
                DateTime _thisMonth = new DateTime(_today.Year, _today.Month, 1);
                DateTime _thisYear = new DateTime(_today.Year, 1, 1);
                DateTime _calendarDate = DateTime.UtcNow.Date;
                DateTime _thisFiscalYear = DateTime.UtcNow.Date;

                switch (_range)
                {
                    case DateRange.Today:
                        _start = _today;
                        _end = _today.AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.ThisWeek:
                        _start = _today.AddDays(-_dayOfWeek);
                        _end = _today.AddDays(-_dayOfWeek + 6).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.ThisMonth:
                        _start = _thisMonth;
                        _end = _thisMonth.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.ThisYear:
                        _start = _thisYear;
                        _end = _thisYear.AddYears(1).AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.ThisFiscalYear:
                        if (Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth != null &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth > 0 &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay != null &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay > 0)
                            _calendarDate = new DateTime(DateTime.UtcNow.Date.Year, (int)Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth, (int)Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay);
                        else
                            _calendarDate = new DateTime(DateTime.UtcNow.Date.Year, 1, 1);
                        _thisFiscalYear = new DateTime((_today.Month >= _calendarDate.Month ? _today.Year : _today.AddYears(-1).Year), _calendarDate.Month, _calendarDate.Day);
                        _start = _thisFiscalYear;
                        _end = _thisFiscalYear.AddYears(1).AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.LastWeek:
                        _start = _today.AddDays(-_dayOfWeek - 7);
                        _end = _today.AddDays(-_dayOfWeek - 1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.LastMonth:
                        _start = _thisMonth.AddMonths(-1);
                        _end = _thisMonth.AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.LastYear:
                        _start = _thisYear.AddYears(-1);
                        _end = _thisYear.AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.LastFiscalYear:
                        if (Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth != null &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth > 0 &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay != null &&
                            Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay > 0)
                            _calendarDate = new DateTime(DateTime.UtcNow.Date.Year, (int)Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartMonth, (int)Micajah.Common.Security.UserContext.Current.SelectedOrganization.FiscalYearStartDay);
                        else
                            _calendarDate = new DateTime(DateTime.UtcNow.Date.Year, 1, 1);
                        _thisFiscalYear = new DateTime((_today.Month >= _calendarDate.Month ? _today.Year : _today.AddYears(-1).Year), _calendarDate.Month, _calendarDate.Day);
                        _start = _thisFiscalYear.AddYears(-1);
                        _end = _thisFiscalYear.AddDays(-1).AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.Rolling30Days:
                        _start = _today.AddDays(-30);
                        _end = _today.AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.Rolling90Days:
                        _start = _today.AddDays(-90);
                        _end = _today.AddHours(23).AddMinutes(59);
                        break;
                    case DateRange.Rolling365Days:
                        _start = _today.AddDays(-365);
                        _end = _today.AddHours(23).AddMinutes(59);
                        break;
                }
                _start = _start.AddHours(-_houroffset);
                _end = _end.AddHours(-_houroffset);
            }
        }

        public EqualRange AgeRange
        {
            get { return _age_equal; }
            set
            {
                _age_equal = value;
            }
        }

        public Grouping YAxis
        {
            get { return _yaxis; }
            set { _yaxis = value; }
        }

        public Grouping SubYAxis
        {
            get { return _subyaxis; }
            set { _subyaxis = value; }
        }

        public int PriorityID
        {
            get { return _priority; }
            set { _priority = value; }
        }

        //tkt #3949: Level Filter added to Ticket Count Report
        public int TicketLevelID
        {
            get { return _ticket_level; }
            set { _ticket_level = value; }
        }
        //tkt #3632: Add Support Groups to Ticket Count Report criteria
        public int SupportGroupID
        {
            get { return _support_group; }
            set { _support_group = value; }
        }

        public string AssetFilter
        {
            get { return _asset_filter; }
            set { _asset_filter = value; }
        }

        public int SLAGraphWidthID
        {
            get { return _sla_graph_width_id; }
            set { _sla_graph_width_id = value; }
        }

        public int SLAGraphViewID
        {
            get { return _sla_graph_view_id; }
            set { _sla_graph_view_id = value; }
        }

        public int ClassID
        {
            get { return _class; }
            set { _class = value; }
        }

        public int ClassLevel
        {
            get { return _classlevel; }
            set { _classlevel = value; }
        }

        public bool ClassIsNull
        {
            get { return _classnull; }
            set { _classnull = value; }
        }

        public int SubClassLevel
        {
            get { return _subclasslevel; }
            set { _subclasslevel = value; }
        }

        public int CreationCategoryID
        {
            get { return _creationcategory; }
            set { _creationcategory = value; }
        }

        public int SubmissionCategoryID
        {
            get { return _submissioncat; }
            set { _submissioncat = value; }
        }

        public int ResolutionCategoryID
        {
            get { return _resolutioncat; }
            set { _resolutioncat = value; }
        }

        public int LocationID
        {
            get { return _location; }
            set { _location = value; }
        }

        public int LocationTypeID
        {
            get { return _locationtype; }
            set { _locationtype = value; }
        }

        public int SubLocationTypeID
        {
            get { return _sublocationtype; }
            set { _sublocationtype = value; }
        }

        public int MonthID
        {
            get { return _month; }
            set { _month = value; }
        }

        public int TechnicianID
        {
            get { return _technician; }
            set { _technician = value; }
        }

        public int SubmittedByID
        {
            get { return _submittedby; }
            set { _submittedby = value; }
        }

        public int ClosedByID
        {
            get { return _closedby; }
            set { _closedby = value; }
        }

        public int AccountID
        {
            get { return _account; }
            set { _account = value; }
        }

        public int AccountLocationId
        {
            get { return _accountLocation; }
            set { _accountLocation = value; }
        }

        public int AccountParentLocationId
        {
            get { return _accountParentLocation; }
            set { _accountParentLocation = value; }
        }

        public int AgeDays
        {
            get { return _age; }
            set { _age = value; }
        }

        #region ICloneable Members

        public object Clone()
        {
            // TODO:  Add Filter.Clone implementation
            return new Fltr(_did, _uid, _houroffset, _start, _end, _range, _yaxis, _subyaxis, _priority, _class, _classlevel, _subclasslevel, _creationcategory, _location, _locationtype, _sublocationtype, _technician, _submittedby, _closedby, _account, _accountLocation, _accountParentLocation, _submissioncat, _resolutioncat, _age, _age_equal, _ticket_level, _support_group, technicianType, handledByCallCenter, _asset_filter);
        }

        #endregion
    }
}
