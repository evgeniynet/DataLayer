using System;
using System.Data;
using System.Web;
using System.Globalization;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Reports.
    /// </summary>
    public class Reports : DBAccess
    {
        public static bool isRecursive = false;

        static long GetLongValue(ref DataRow row, string fieldName)
        {
            return ((row != null && !row.IsNull(fieldName)) ? (long)row[fieldName] : 0);
        }

        static long GetLongValue(DataRow row, string fieldName)
        {
            return ((row != null && !row.IsNull(fieldName)) ? (long)row[fieldName] : 0);
        }

        static int GetIntValue(ref DataRow row, string fieldName)
        {
            return ((row != null && !row.IsNull(fieldName)) ? (int)row[fieldName] : 0);
        }

        static int GetIntValue(DataRow row, string fieldName)
        {
            return ((row != null && !row.IsNull(fieldName)) ? (int)row[fieldName] : 0);
        }

        public static DataTable SelectFilters(int DeptID, int UserId)
        {
            ReportType reptype = ReportType.TicketCount;
            return SelectRecords("sp_SelectReportFilters", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), new SqlParameter("@ReportType", (int)reptype) });
        }

        public static int UpdateFilter(int DeptID, int Id, int UserId, string name, string filterstate)
        {
            ReportType reptype = ReportType.TicketCount;
            SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
            _pId.Direction = ParameterDirection.InputOutput;
            _pId.Value = Id;
            UpdateData("sp_UpdateReportFilter", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), new SqlParameter("@ReportType", (int)reptype), new SqlParameter("@Name", name), new SqlParameter("@FilterState", filterstate) });
            return (int)_pId.Value;
        }

        public static void DeleteFilter(int DeptID, int Id)
        {
            UpdateData("sp_DeleteReportFilter", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
        }

        public static DataTable TicketsCount(int DId, Fltr filter, bool OnHoldStatus, bool PartsTracking, string SortExpession, string GlobalFilterSQL, bool IsSLA)
        {
            DateTime start = filter.StartDate;
            DateTime end = filter.EndDate;

            string _sqlPreSelect = string.Empty;
            string _sqlSelect = string.Empty;
            string _sqlWhere = "WHERE T.company_id=" + DId.ToString() + " AND ((T.CreateTime BETWEEN '" + Functions.FormatSQLDateTime(start) + "' AND '" + Functions.FormatSQLDateTime(end) + "') OR (T.ClosedTime BETWEEN '" + Functions.FormatSQLDateTime(start) + "' AND '" + Functions.FormatSQLDateTime(end) + "' AND T.Status='Closed')) ";
            string _sqlGroup = string.Empty;
            string _sqlOrder = string.Empty;
            if (!OnHoldStatus) _sqlWhere += "AND T.Status<>'On Hold' ";
            if (!PartsTracking) _sqlWhere += "AND T.Status<>'Parts On Order' ";
            if (SortExpession.Length > 0 && SortExpession.IndexOf("YAxis") >= 0 && filter.YAxis == Grouping.Month)
            {
                _sqlOrder = "ORDER BY Month " + SortExpession.Replace("YAxis", string.Empty);
            }
            else if (SortExpession.Length > 0) _sqlOrder = "ORDER BY " + SortExpession;

            string _range = "'" + Functions.FormatSQLDateTime(start) + "' AND '" + Functions.FormatSQLDateTime(end) + "'";
            string _sqlSelectFields =
                        "SUM(CASE WHEN T.CreateTime BETWEEN " + _range + " THEN 1 ELSE 0 END) AS TotalCount, "
                      + "SUM(CASE WHEN T.status = 'Open' AND T.CreateTime BETWEEN " + _range + " THEN 1 ELSE 0 END) AS OpenCount, "
                      + "SUM(CASE WHEN T.status = 'Closed' AND T.ClosedTime BETWEEN " + _range + " THEN 1 ELSE 0 END) AS ClosedCount, "
                      + "SUM(CASE WHEN T.status = 'On Hold' AND T.CreateTime BETWEEN " + _range + " THEN 1 ELSE 0 END) AS HoldCount, "
                      + "SUM(CASE WHEN T.status = 'Parts On Order' AND T.CreateTime BETWEEN " + _range + " THEN 1 ELSE 0 END) AS PartsCount, ";
            
            if (IsSLA)
            {
                _sqlPreSelect += "DECLARE @WorkDays char(7); ";
                _sqlPreSelect += "DECLARE @StartBusinnessTime int; ";
                _sqlPreSelect += "DECLARE @EndBusinnessTime int; ";
                _sqlPreSelect += "SELECT @WorkDays = I.WorkingDays, @StartBusinnessTime = dbo.fxGetConfigValueStr(" + DId.ToString() + ", 'tinyBusHourStart')*60 + dbo.fxGetConfigValueStr(" + DId.ToString() + ", 'tinyBusMinStart'), @EndBusinnessTime = dbo.fxGetConfigValueStr(" + DId.ToString() + ", 'tinyBusHourStop')*60 + dbo.fxGetConfigValueStr(" + DId.ToString() + ", 'tinyBusMinStop') FROM tbl_company C INNER JOIN Mc_Instance I ON I.InstanceId = C.company_guid WHERE C.company_id = " + DId.ToString() + "; ";
                _sqlPreSelect += "IF @WorkDays IS NULL SET @WorkDays = '1111100'; ";
                _sqlPreSelect += "IF @StartBusinnessTime IS NULL SET @StartBusinnessTime = 0; ";
                _sqlPreSelect += "IF @EndBusinnessTime IS NULL SET @EndBusinnessTime = 1439; ";

                _sqlSelectFields += "AVG(cast(CASE T.status WHEN 'Open' THEN dbo.fxGetOperationalMinutes(" + DId.ToString() + ",T.CreateTime, GETUTCDATE(), @WorkDays, @StartBusinnessTime, @EndBusinnessTime) ELSE NULL END as bigint)) AS AvgOpen, "
                        + "AVG(cast(CASE WHEN T.status='Closed' AND T.ClosedTime BETWEEN '" + Functions.FormatSQLDateTime(start) + "' AND '" + Functions.FormatSQLDateTime(end) + "' THEN dbo.fxGetOperationalMinutes(" + DId.ToString() + ",T.createtime,T.closedtime, @WorkDays, @StartBusinnessTime, @EndBusinnessTime) ELSE null END as bigint)) AS AvgClosed, "
                        + "AVG(cast(CASE T.status WHEN 'On Hold' THEN dbo.fxGetOperationalMinutes(" + DId.ToString() + ",T.CreateTime, GETUTCDATE(), @WorkDays, @StartBusinnessTime, @EndBusinnessTime) ELSE null END as bigint)) AS AvgHold, "
                        + "AVG(cast(CASE T.status WHEN 'Parts On Order' THEN dbo.fxGetOperationalMinutes(" + DId.ToString() + ",T.CreateTime, GETUTCDATE(), @WorkDays, @StartBusinnessTime, @EndBusinnessTime) ELSE null END as bigint)) AS AvgParts, ";
            }
            else
            {
                _sqlSelectFields += "AVG(cast(CASE T.status WHEN 'Open' THEN DATEDIFF(minute, T.createtime, GETUTCDATE()) ELSE null END as bigint)) AS AvgOpen, "
                      + "AVG(cast(CASE WHEN T.status='Closed' AND T.ClosedTime BETWEEN '" + Functions.FormatSQLDateTime(start) + "' AND '" + Functions.FormatSQLDateTime(end) + "' THEN DATEDIFF(minute, T.createtime, T.closedtime) ELSE null END as bigint)) AS AvgClosed, "
                      + "AVG(cast(CASE T.status WHEN 'On Hold' THEN DATEDIFF(minute, T.createtime, GETUTCDATE()) ELSE null END as bigint)) AS AvgHold, "
                      + "AVG(cast(CASE T.status WHEN 'Parts On Order' THEN DATEDIFF(minute, T.createtime, GETUTCDATE()) ELSE null END as bigint)) AS AvgParts, ";
            }
            _sqlSelectFields += "0 AS Level ";

            if (_sqlOrder == string.Empty)
                _sqlOrder = "ORDER BY YAxis ASC";

            switch (filter.YAxis)
            {
                case Grouping.Account:
                    _sqlSelect = "SELECT ISNULL(A.Id, CASE WHEN ISNULL(T.btNoAccount, 0)=0 THEN -1 ELSE -2 END) AS ID, MAX(A.vchName) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN Accounts A ON A.DId=" + DId.ToString() + " AND A.Id=T.intAcctId ";
                    _sqlGroup = "GROUP BY ISNULL(A.Id, CASE WHEN ISNULL(T.btNoAccount, 0)=0 THEN -1 ELSE -2 END) ";
                    break;
                case Grouping.AccountLocation:
                    _sqlSelect = "SELECT ISNULL(T.intAcctId,CASE WHEN ISNULL(T.btNoAccount, 0)=0 THEN -1 ELSE -2 END) AS ID, ISNULL(T.AccountLocationId,-1) AS SubID, MAX(ISNULL((CASE WHEN ISNULL(T.btNoAccount,0) = 1 THEN '(No Account)' ELSE dbo.fxGetAccountName(" + DId.ToString() + ", T.intAcctId) END),'') + ISNULL(' / ' + dbo.fxGetUserLocationName(" + DId.ToString() + ", T.AccountLocationId),'')) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T ";
                    _sqlGroup = "GROUP BY ISNULL(T.intAcctId, CASE WHEN ISNULL(T.btNoAccount, 0)=0 THEN -1 ELSE -2 END), T.AccountLocationId ";
                    break;
                case Grouping.Class:
                    _sqlSelect = "SELECT ISNULL(C.GroupId,0) AS ID, MAX(C.Name) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN dbo.fxMapClassesByLevel(" + DId.ToString() + "," + filter.ClassLevel.ToString() + ") C ON T.class_id=C.Id ";
                    _sqlGroup = "GROUP BY C.GroupId ";
                    break;
                case Grouping.CreationCategory:
                    _sqlSelect = "SELECT ISNULL(CC.Id,-1) AS ID, MAX(CC.vchName) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN CreationCats CC ON CC.DId=" + DId.ToString() + " AND CC.Id=T.CreationCatsId ";
                    _sqlGroup = "GROUP BY CC.Id ";
                    break;
                case Grouping.SubmissionCategory:
                    _sqlSelect = "SELECT ISNULL(SC.Id,-1) AS ID, MAX(SC.vchName) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN SubmissionCategories SC ON T.intSubmissionCatId=SC.Id ";
                    _sqlGroup = "GROUP BY SC.Id ";
                    break;
                case Grouping.ResolutionCategory:
                    _sqlSelect = "SELECT ISNULL(RC.Id,-1) AS ID, MAX(CASE WHEN RC.btResolved=1 THEN 'Resolved-' ELSE 'Unresolved-' END+RC.vchName) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN ResolutionCats RC ON RC.DId=" + DId.ToString() + " AND RC.Id=T.ResolutionCatsId ";
                    _sqlGroup = "GROUP BY RC.Id ";
                    break;
                case Grouping.Location:
                    _sqlSelect = "SELECT ISNULL(LC.GroupId,-" + filter.LocationTypeID.ToString() + ") AS ID, MAX(LC.Name) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN fxMapLocationsByType(" + DId.ToString() + ",NULL, NULL," + filter.LocationTypeID.ToString() + ",NULL) LC ON T.LocationId=LC.Id ";
                    _sqlGroup = "GROUP BY LC.GroupId ";
                    break;
                case Grouping.Month:
                    _sqlSelect = "SELECT CAST(CAST(YEAR(T.CreateTime) AS nvarchar(4))+'/'+CAST(MONTH(T.CreateTime) AS nvarchar(2))+'/1' AS datetime) AS Month, MAX(CAST(CAST(YEAR(T.CreateTime) AS nvarchar(4))+CAST(MONTH(T.CreateTime) AS nvarchar(2)) AS int)) AS ID, MAX(DATENAME(month,T.CreateTime)+' '+CAST(YEAR(T.CreateTime) AS nvarchar(4))) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T ";
                    _sqlGroup = "GROUP BY CAST(CAST(YEAR(T.CreateTime) AS nvarchar(4))+'/'+CAST(MONTH(T.CreateTime) AS nvarchar(2))+'/1' AS datetime) ";
                    break;
                case Grouping.Priority:
                    _sqlSelect = "SELECT ISNULL(P.Id, -1) AS ID, MAX(CAST(P.tintPriority as nvarchar(5))+'-'+P.Name) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN Priorities P ON P.DId=" + DId.ToString() + " AND P.Id=T.PriorityId ";
                    _sqlGroup = "GROUP BY P.Id ";
                    break;
                //tkt #3949: Level Filter added to Ticket Count Report
                case Grouping.TicketLevel:
                    _sqlSelect = "SELECT ISNULL(CAST(L.tintLevel as int), -1) AS ID, MAX(CAST(L.tintLevel as nvarchar(5))+ CASE WHEN L.LevelName IS NOT NULL THEN '-'+L.LevelName ELSE '' END ) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T LEFT OUTER JOIN TktLevels L ON L.DId=" + DId.ToString() + " AND L.tintLevel=T.tintLevel ";
                    _sqlGroup = "GROUP BY L.tintLevel ";
                    break;
                //tkt #3632: Add Support Groups to Ticket Count Report criteria
                case Grouping.SupportGroup:
                    _sqlSelect = "SELECT ISNULL(CAST(SG.id as int), -1) AS ID, MAX(SG.vchName) AS YAxis, " + _sqlSelectFields
                    + "FROM tbl_ticket T LEFT OUTER JOIN Accounts AC ON AC.DId=" + DId.ToString() + " AND AC.id=T.intAcctId INNER JOIN tbl_LoginCompanyJunc LJT ON LJT.company_id=" + DId.ToString() + " AND LJT.id=T.Technician_id LEFT OUTER JOIN SupportGroups SG ON SG.DId=" + DId.ToString() + " AND SG.Id=LJT.SupGroupId ";
                    _sqlGroup = "Group By SG.id ";
                    break;
                case Grouping.Technician:
                    _sqlSelect = "SELECT ISNULL(LJT.id,-1) AS ID, MAX(dbo.fxGetUserName(L.FirstName, L.LastName, L.Email)) AS YAxis, " + _sqlSelectFields
                        + "FROM tbl_ticket T "
                        + "INNER JOIN tbl_LoginCompanyJunc LJT ON LJT.company_id=" + DId.ToString() + " AND LJT.id=T.Technician_id "
                        + "INNER JOIN tbl_Logins L ON LJT.login_id = L.id ";
                    _sqlGroup = "GROUP BY LJT.id ";
                    break;
            }
            _sqlSelect += "INNER JOIN tbl_LoginCompanyJunc LJU ON LJU.company_id=" + DId.ToString() + " AND LJU.id=T.user_id ";

            if (filter.AccountID > 0) _sqlWhere += "AND T.intAcctId=" + filter.AccountID.ToString() + " ";
            else if (filter.AccountID < 0) _sqlWhere += "AND T.intAcctId IS NULL AND ISNULL(t.btNoAccount, 0) = " + (filter.AccountID == -2 ? "1" : "0") + " ";
            if (filter.AccountLocationId > 0) _sqlWhere += "AND T.AccountLocationId=" + filter.AccountLocationId.ToString() + " ";
            else if (filter.AccountLocationId < 0) _sqlWhere += "AND T.AccountLocationId IS NULL ";
            if (filter.AccountParentLocationId > 0) _sqlWhere += "AND T.AccountLocationId IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + DId.ToString() + "," + filter.AccountParentLocationId.ToString() + ")) ";
            else if (filter.AccountParentLocationId < 0) _sqlWhere += "AND T.AccountLocationId IS NULL ";

            if (filter.ClassID > 0) _sqlWhere += "AND T.class_id IN (SELECT Id FROM dbo.fxGetAllChildClasses(" + DId.ToString() + "," + filter.ClassID.ToString() + ",NULL)) ";
            else if (filter.ClassID < 0) _sqlWhere += "AND T.class_id=" + Convert.ToString(-filter.ClassID).ToString();
            else if (filter.ClassID == 0 && filter.ClassIsNull) _sqlWhere += "AND T.class_id IS NULL ";
            if (filter.CreationCategoryID > 0) _sqlWhere += "AND T.CreationCatsId=" + filter.CreationCategoryID.ToString() + " ";
            else if (filter.CreationCategoryID < 0) _sqlWhere += "AND T.CreationCatsId IS NULL ";
            if (filter.SubmissionCategoryID > 0) _sqlWhere += "AND T.intSubmissionCatId=" + filter.SubmissionCategoryID.ToString() + " ";
            else if (filter.SubmissionCategoryID < 0) _sqlWhere += "AND T.intSubmissionCatId IS NULL ";
            if (filter.ResolutionCategoryID > 0) _sqlWhere += "AND T.ResolutionCatsId=" + filter.ResolutionCategoryID.ToString() + " ";
            else if (filter.ResolutionCategoryID == -1) _sqlWhere += "AND T.ResolutionCatsId IS NULL ";
            else if (filter.ResolutionCategoryID == -2) _sqlWhere += "AND T.btResolved=0 ";
            else if (filter.ResolutionCategoryID == -3) _sqlWhere += "AND T.btResolved=1 ";
            if (filter.LocationID > 0) _sqlWhere += "AND T.LocationId IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + DId.ToString() + "," + filter.LocationID.ToString() + ")) ";
            else if (filter.LocationID < 0) _sqlWhere += "AND (T.LocationId IS NULL OR T.LocationId IN (SELECT Id FROM fxMapLocationsByType(" + DId.ToString() + ",NULL,NULL," + Convert.ToString(-filter.LocationID) + ",NULL) WHERE GroupId IS NULL)) ";
            if (filter.MonthID > 0) _sqlWhere += "AND YEAR(T.CreateTime)=" + filter.MonthID.ToString().Substring(0, 4) + " AND MONTH(T.CreateTime)=" + filter.MonthID.ToString().Substring(4) + " ";
            if (filter.PriorityID != 0) _sqlWhere += "AND T.PriorityId=" + filter.PriorityID.ToString() + " ";
            else if (filter.PriorityID < 0) _sqlWhere += "AND T.PriorityId IS NULL ";

            if (filter.AssetFilter.Length > 0)
            {
                string _assets = string.Empty;
                string[] _arrSN = filter.AssetFilter.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string _sn in _arrSN)
                {
                    DataTable _dt = Data.Assets.SelectAssetByUniqueField(DId, _sn);
                    foreach (DataRow _row in _dt.Rows)
                        _assets = _assets + _row["Id"].ToString() + ",";
                };

                if (_assets.Length > 0)
                    _assets = _assets.Remove(_assets.Length - 1, 1);

                string _asset_tickets = string.Empty;
                if (_assets.Length > 0)
                {
                    DataTable _asset_tickets_table = Data.Logins.SelectByQueryExt("Select Distinct TicketId from TicketAssets where DId=" + DId.ToString() + " AND AssetId in (" + _assets + ")");
                    if (_asset_tickets_table != null)
                    {
                        for (int _index = 0; _index < _asset_tickets_table.Rows.Count; _index++)
                        {
                            _asset_tickets = _asset_tickets + _asset_tickets_table.Rows[_index]["TicketId"].ToString() + ",";
                        };

                        if (_asset_tickets.Length > 0)
                            _asset_tickets = _asset_tickets.Remove(_asset_tickets.Length - 1, 1);

                        if (_asset_tickets.Length > 0)
                        {
                            _sqlWhere = _sqlWhere + " AND T.Id in (" + _asset_tickets + ") ";
                        }
                        else
                        {
                            _sqlWhere = _sqlWhere + " AND T.Id=0 ";
                        };
                    };
                }
                else
                {
                    _sqlWhere = _sqlWhere + " AND T.Id=0 ";
                };
            };

            //tkt #3949: Level Filter added to Ticket Count Report
            if (filter.TicketLevelID > 0) _sqlWhere += "AND T.tintLevel=" + filter.TicketLevelID.ToString() + " ";
            else if (filter.TicketLevelID < 0) _sqlWhere += "AND T.tintLevel IS NULL ";
            //tkt #3632: Add Support Groups to Ticket Count Report criteria
            if (filter.SupportGroupID > 0) _sqlWhere += "AND Exists(select INNER_SG.id from tbl_ticket INNER_T INNER JOIN tbl_LoginCompanyJunc INNER_LCJ ON INNER_T.Technician_id = INNER_LCJ.id LEFT OUTER JOIN SupportGroups INNER_SG ON INNER_LCJ.SupGroupId=INNER_SG.Id where INNER_T.company_id=T.company_id and INNER_T.id=T.id and INNER_SG.id=" + filter.SupportGroupID.ToString() + ") ";
            else if (filter.SupportGroupID < 0) _sqlWhere += "AND Exists(select INNER_SG.id from tbl_ticket INNER_T INNER JOIN tbl_LoginCompanyJunc INNER_LCJ ON INNER_T.Technician_id = INNER_LCJ.id LEFT OUTER JOIN SupportGroups INNER_SG ON INNER_LCJ.SupGroupId=INNER_SG.Id where INNER_T.company_id=T.company_id and INNER_T.id=T.id and INNER_SG.id IS NULL) ";
            if (filter.TechnicianType != TechnicianType.All && filter.YAxis != Grouping.Technician && filter.YAxis != Grouping.SupportGroup)
            {
                _sqlSelect += "INNER JOIN tbl_LoginCompanyJunc LJT ON LJT.company_id=" + DId.ToString() + " AND LJT.id=T.Technician_id ";
            }
            switch (filter.TechnicianType)
            {
                case TechnicianType.Filtered:
                    _sqlWhere += "AND (LJT.btGlobalFilterEnabled=1 OR LJT.btLimitToAssignedTkts=1 OR LJT.btDisabledReports=1) ";
                    break;
                case TechnicianType.Global:
                    _sqlWhere += "AND (LJT.btGlobalFilterEnabled=0 AND LJT.btLimitToAssignedTkts=0 AND LJT.btDisabledReports=0 AND LJT.btCfgCCRep=0) ";
                    break;
                case TechnicianType.CallCenterRep:
                    _sqlWhere += "AND LJT.btCfgCCRep=1 ";
                    break;
                //btCfgCCRep
            }
            if (filter.HandledByCallCenter != HandledByCallCenter.All)
                _sqlWhere += string.Format("AND T.btHandledByCC={0} ", ((int)filter.HandledByCallCenter)).ToString();

            if (filter.TechnicianID > 0) _sqlWhere += "AND T.Technician_id=" + filter.TechnicianID.ToString() + " ";
            else if (filter.TechnicianID < 0) _sqlWhere += "AND T.Technician_id IS NULL ";
            if (filter.SubmittedByID > 0) _sqlWhere += "AND T.Created_id=" + filter.SubmittedByID.ToString() + " ";
            else if (filter.SubmittedByID < 0) _sqlWhere += "AND T.Created_id IS NULL ";
            if (filter.ClosedByID > 0) _sqlWhere += "AND T.Closed_id=" + filter.ClosedByID.ToString() + " ";
            else if (filter.ClosedByID < 0) _sqlWhere += "AND T.Closed_id IS NULL ";

            if (filter.AgeDays != -1)
            {
                string _date_diff = "(CASE T.status WHEN 'Closed' then DATEDIFF(day, T.CreateTime, T.ClosedTime) ELSE DATEDIFF(day, T.CreateTime, GETUTCDATE()) END)";

                switch (filter.AgeRange)
                {
                    case EqualRange.Less:
                        _sqlWhere += " AND " + _date_diff + "<" + filter.AgeDays.ToString();
                        break;
                    case EqualRange.Equal:
                        _sqlWhere += " AND " + _date_diff + "=" + filter.AgeDays.ToString();
                        break;
                    case EqualRange.Greater:
                        _sqlWhere += " AND " + _date_diff + ">" + filter.AgeDays.ToString();
                        break;
                };
            };

            _sqlWhere += GlobalFilterSQL;

            if (filter.SubYAxis == Grouping.None || (filter.SubYAxis == filter.YAxis && filter.ClassLevel == filter.SubClassLevel && filter.LocationTypeID == filter.SubLocationTypeID))
            {
                DataTable ResultDT = SelectByQuery(_sqlPreSelect + _sqlSelect + _sqlWhere + _sqlGroup + _sqlOrder);

                if (isRecursive == false)
                {
                    int total_created = 0;
                    int total_opened = 0;
                    int total_closed = 0;
                    int total_holded = 0;
                    int total_parts = 0;

                    long total_avg_opened = 0;
                    long total_avg_closed = 0;
                    long total_avg_holded = 0;
                    long total_avg_parts = 0;

                    int _avg_opened_count = 0;
                    int _avg_closed_count = 0;
                    int _avg_holded_count = 0;
                    int _avg_parts_count = 0;

                    string row_name = "Total:";

                    for (int i = 0; i < ResultDT.Rows.Count; i++)
                    {
                        DataRow current_row = ResultDT.Rows[i];
                        total_created += (int)current_row["TotalCount"];
                        total_opened += (int)current_row["OpenCount"];
                        total_closed += (int)current_row["ClosedCount"];
                        total_holded += (int)current_row["HoldCount"];
                        total_parts += (int)current_row["PartsCount"];

                        long _avg_opened = GetLongValue(ref current_row, "AvgOpen");

                        if (_avg_opened > 0)
                            _avg_opened_count++;

                        long _avg_closed = GetLongValue(ref current_row, "AvgClosed");

                        if (_avg_closed > 0)
                            _avg_closed_count++;

                        long _avg_holded = GetLongValue(ref current_row, "AvgHold");

                        if (_avg_holded > 0)
                            _avg_holded_count++;

                        long _avg_parts = GetLongValue(ref current_row, "AvgParts");

                        if (_avg_parts > 0)
                            _avg_parts_count++;

                        total_avg_opened += _avg_opened;
                        total_avg_closed += _avg_closed;
                        total_avg_holded += _avg_holded;
                        total_avg_parts += _avg_parts;
                    };

                    DataRow new_row = ResultDT.NewRow();
                    new_row["ID"] = 0;
                    new_row["YAxis"] = row_name;
                    new_row["TotalCount"] = total_created;
                    new_row["OpenCount"] = total_opened;
                    new_row["ClosedCount"] = total_closed;
                    new_row["HoldCount"] = total_holded;
                    new_row["PartsCount"] = total_parts;

                    double _float_number = 0;
                    if (_avg_opened_count > 0)
                    {
                        _float_number = (double)total_avg_opened / (double)_avg_opened_count;
                        total_avg_opened = (int)_float_number;
                    };

                    if (_avg_closed_count > 0)
                    {
                        _float_number = (double)total_avg_closed / (double)_avg_closed_count;
                        total_avg_closed = (int)_float_number;
                    };

                    if (_avg_holded_count > 0)
                    {
                        _float_number = (double)total_avg_holded / (double)_avg_holded_count;
                        total_avg_holded = (int)_float_number;
                    };

                    if (_avg_parts_count > 0)
                    {
                        _float_number = (double)total_avg_parts / (double)_avg_parts_count;
                        total_avg_parts = (int)_float_number;
                    };

                    if (total_avg_opened > 0)
                        new_row["AvgOpen"] = total_avg_opened;

                    if (total_avg_closed > 0)
                        new_row["AvgClosed"] = total_avg_closed;

                    if (total_avg_holded > 0)
                        new_row["AvgHold"] = total_avg_holded;

                    if (total_avg_parts > 0)
                        new_row["AvgParts"] = total_avg_parts;

                    new_row["Level"] = 0;
                    ResultDT.Rows.InsertAt(new_row, ResultDT.Rows.Count + 1);
                };

                return ResultDT;
            };

            int full_total_created = 0;
            int full_total_opened = 0;
            int full_total_closed = 0;
            int full_total_holded = 0;
            int full_total_parts = 0;

            long full_total_avg_opened = 0;
            long full_total_avg_closed = 0;
            long full_total_avg_holded = 0;
            long full_total_avg_parts = 0;

            int _full_avg_opened_count = 0;
            int _full_avg_closed_count = 0;
            int _full_avg_holded_count = 0;
            int _full_avg_parts_count = 0;

            DataTable _DT = SelectByQuery(_sqlPreSelect + _sqlSelect + _sqlWhere + _sqlGroup + _sqlOrder);
            Fltr _f = (Fltr)filter.Clone();
            _f.SubYAxis = Grouping.None;
            for (int i = 0; i < _DT.Rows.Count; i++)
            {
                DataRow _row = _DT.Rows[i];
                DataTable _subDT = null;

                _f.YAxis = filter.SubYAxis;
                _f.LocationTypeID = _f.SubLocationTypeID;
                _f.ClassLevel = _f.SubClassLevel;
                switch (filter.YAxis)
                {
                    case Grouping.Account:
                        if (filter.SubYAxis == Grouping.Location) _f.YAxis = Grouping.AccountLocation;
                        _f.AccountID = (int)_row["ID"];
                        break;
                    case Grouping.AccountLocation:
                        _f.AccountID = (int)_row["ID"];
                        _f.AccountLocationId = (int)_row["SubId"];
                        break;
                    case Grouping.Class:
                        _f.ClassID = (int)_row["ID"];
                        if (_f.ClassID == 0) _f.ClassIsNull = true;
                        break;
                    case Grouping.CreationCategory:
                        _f.CreationCategoryID = (int)_row["ID"];
                        break;
                    case Grouping.SubmissionCategory:
                        _f.SubmissionCategoryID = (int)_row["ID"];
                        break;
                    case Grouping.ResolutionCategory:
                        _f.ResolutionCategoryID = (int)_row["ID"];
                        break;
                    case Grouping.Location:
                        _f.LocationID = (int)_row["ID"];
                        break;
                    case Grouping.Month:
                        _f.MonthID = (int)_row["ID"];
                        break;
                    case Grouping.Priority:
                        _f.PriorityID = (int)_row["ID"];
                        break;
                    //tkt #3949: Level Filter added to Ticket Count Report
                    case Grouping.TicketLevel:
                        _f.TicketLevelID = (int)_row["ID"];
                        break;
                    //tkt #3632: Add Support Groups to Ticket Count Report criteria
                    case Grouping.SupportGroup:
                        _f.SupportGroupID = (int)_row["ID"];
                        break;
                    case Grouping.Technician:
                        _f.TechnicianID = (int)_row["ID"];
                        break;
                }

                isRecursive = true;
                _subDT = TicketsCount(DId, _f, OnHoldStatus, PartsTracking, SortExpession, GlobalFilterSQL, IsSLA);
                isRecursive = false;

                if (_subDT.Columns.Contains("SubId") && !_DT.Columns.Contains("SubId"))
                    _DT.Columns.Add("SubId", typeof(int));

                foreach (DataRow _srow in _subDT.Rows)
                {
                    i++;
                    DataRow _newrow = _DT.NewRow();
                    _newrow["ID"] = _srow["ID"];

                    if (_subDT.Columns.Contains("SubId"))
                        _newrow["SubId"] = _srow["SubId"];

                    _newrow["YAxis"] = _srow["YAxis"];
                    _newrow["TotalCount"] = _srow["TotalCount"];
                    _newrow["OpenCount"] = _srow["OpenCount"];
                    _newrow["ClosedCount"] = _srow["ClosedCount"];
                    _newrow["HoldCount"] = _srow["HoldCount"];
                    _newrow["PartsCount"] = _srow["PartsCount"];
                    _newrow["AvgOpen"] = _srow["AvgOpen"];
                    _newrow["AvgClosed"] = _srow["AvgClosed"];
                    _newrow["AvgHold"] = _srow["AvgHold"];
                    _newrow["AvgParts"] = _srow["AvgParts"];

                    _newrow["Level"] = (int)_row["ID"] == 0 ? -1 : _row["ID"];

                    full_total_created += (int)_srow["TotalCount"];
                    full_total_opened += (int)_srow["OpenCount"];
                    full_total_closed += (int)_srow["ClosedCount"];
                    full_total_holded += (int)_srow["HoldCount"];
                    full_total_parts += (int)_srow["PartsCount"];

                    long _full_avg_opened = GetLongValue(_srow, "AvgOpen");

                    if (_full_avg_opened > 0)
                        _full_avg_opened_count++;

                    long _full_avg_closed = GetLongValue(_srow, "AvgClosed");

                    if (_full_avg_closed > 0)
                        _full_avg_closed_count++;

                    long _full_avg_holded = GetLongValue(_srow, "AvgHold");

                    if (_full_avg_holded > 0)
                        _full_avg_holded_count++;

                    long _full_avg_parts = GetLongValue(_srow, "AvgParts");

                    if (_full_avg_parts > 0)
                        _full_avg_parts_count++;

                    full_total_avg_opened += _full_avg_opened;
                    full_total_avg_closed += _full_avg_closed;
                    full_total_avg_holded += _full_avg_holded;
                    full_total_avg_parts += _full_avg_parts;

                    _DT.Rows.InsertAt(_newrow, i);
                }
            }

            string full_row_name = "Total:";

            DataRow full_new_row = _DT.NewRow();
            full_new_row["ID"] = 0;
            full_new_row["YAxis"] = full_row_name;
            full_new_row["TotalCount"] = full_total_created;
            full_new_row["OpenCount"] = full_total_opened;
            full_new_row["ClosedCount"] = full_total_closed;
            full_new_row["HoldCount"] = full_total_holded;
            full_new_row["PartsCount"] = full_total_parts;

            double _full_float_number = 0;
            if (_full_avg_opened_count > 0)
            {
                _full_float_number = (double)full_total_avg_opened / (double)_full_avg_opened_count;
                full_total_avg_opened = (int)_full_float_number;
            };

            if (_full_avg_closed_count > 0)
            {
                _full_float_number = (double)full_total_avg_closed / (double)_full_avg_closed_count;
                full_total_avg_closed = (int)_full_float_number;
            };

            if (_full_avg_holded_count > 0)
            {
                _full_float_number = (double)full_total_avg_holded / (double)_full_avg_holded_count;
                full_total_avg_holded = (int)_full_float_number;
            };

            if (_full_avg_parts_count > 0)
            {
                _full_float_number = (double)full_total_avg_parts / (double)_full_avg_parts_count;
                full_total_avg_parts = (int)_full_float_number;
            };

            if (full_total_avg_opened > 0)
                full_new_row["AvgOpen"] = full_total_avg_opened;

            if (full_total_avg_closed > 0)
                full_new_row["AvgClosed"] = full_total_avg_closed;

            if (full_total_avg_holded > 0)
                full_new_row["AvgHold"] = full_total_avg_holded;

            if (full_total_avg_parts > 0)
                full_new_row["AvgParts"] = full_total_avg_parts;

            full_new_row["Level"] = 0;
            _DT.Rows.InsertAt(full_new_row, _DT.Rows.Count + 1);

            return _DT;
        }

        public static DataTable Averages(int DepartmentId, DateTime StartDate, DateTime EndDate, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportAverages", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@dtStart", StartDate), 
                new SqlParameter("@dtEnd", EndDate),
                new SqlParameter("@btSLA", IsSLA)});
        }

        public static DataTable TechnicianStandard(int DepartmentId, DateTime StartDate, DateTime EndDate, string Actv, string WorkDays, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportTechnicianStandard", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate),
                new SqlParameter("@WorkDays", WorkDays),
                new SqlParameter("@btSLA", IsSLA),
                new SqlParameter("@Active", Actv == "0")});
        }

        public static DataTable TechnicianStandardAlt(int DepartmentId, DateTime StartDate, DateTime EndDate, string Actv, string WorkDays, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportAltTechnicianStandard", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate),
                new SqlParameter("@WorkDays", WorkDays),
                new SqlParameter("@btSLA", IsSLA),
                new SqlParameter("@Active", Actv == "0")});
        }

        public static DataTable ClassStandard(int DepartmentId, DateTime StartDate, DateTime EndDate, int ClassLevel, string WorkDays, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportClassStandard", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate),
                new SqlParameter("@ClassLevel", ClassLevel),
                new SqlParameter("@WorkDays", ClassLevel),
                new SqlParameter("@btSLA", IsSLA)});
        }

        public static DataTable CategoryStandard(int DepartmentId, DateTime StartDate, DateTime EndDate, string WorkDays, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportCategoryStandard", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate),
                new SqlParameter("@WorkDays", WorkDays),
                new SqlParameter("@btSLA", IsSLA)});
        }

        public static DataTable LocationStandard(int DepartmentId, int LocationTypeId, DateTime StartDate, DateTime EndDate, string WorkDays, bool IsSLA)
        {
            return SelectRecords("sp_SelectReportLocationStandard", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@LocationTypeId", LocationTypeId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate),
                new SqlParameter("@WorkDays", WorkDays),
                new SqlParameter("@btSLA", IsSLA)});
        }

        public static DataTable TechnicianTime(int DepartmentId, DateTime StartDate, DateTime EndDate)
        {
            return SelectRecords("sp_SelectReportTechTime", new SqlParameter[] { 
                new SqlParameter("@DId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate) });
        }

        public static DataTable TechnicianTimeDetails(int DepartmentId, int TechnicianId, DateTime StartDate, DateTime EndDate)
        {
            return SelectRecords("sp_SelectReportTechTimeDetail", new SqlParameter[] { 
                new SqlParameter("@DId", DepartmentId), 
                new SqlParameter("@TechId", TechnicianId),
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate) });
        }
        public static DataTable TechCheckInOutStatuses(int DepartmentId)
        {
            return SelectRecords("sp_SelectReportTechCheckIn", new SqlParameter[] { 
                new SqlParameter("@DId", DepartmentId) });
        }
    }
}
