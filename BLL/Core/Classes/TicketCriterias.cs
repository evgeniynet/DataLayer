using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class TicketCriterias : DBAccess
    {
        public enum FilterType
        {
            CommonFilter = 0,
            Locations = 1,
            Classes = 2,
            UnassignedQueue = 3,
            SupportGroups = 4,
            Levels = 5,
            Accounts = 6,
            Priorities = 7
        }

        public enum CriteriaType
        {
            GlobalFilter=0,
            UserFilter=1,
            NotificationRule=2
        }

        public enum CriteriaState
        {
            NoFilter,
            LimitToAssignedTickets,
            DisabledReports,
            FilterEnabled
        }

        public TicketCriterias()
        {
        }

        public static DataRow SelectOne(int DeptID, int TicketCriteriaID)
        {
            return SelectRecord("sp_SelectTicketCriterias", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", TicketCriteriaID) });
        }

        public static DataRow SelectOneForNotificationRule(int DeptID, int NotificationRuleId)
        {
            SqlParameter _pTicketCriteriaId = new SqlParameter("@Id", SqlDbType.Int);
            _pTicketCriteriaId.Value = DBNull.Value;
            SqlParameter _pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            _pUserId.Value = DBNull.Value;
            return SelectRecord("sp_SelectTicketCriterias", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pTicketCriteriaId, _pUserId, new SqlParameter("@NotificationRuleId", NotificationRuleId)});
        }

        public static DataTable SelectCriteriaDataByType(int DeptID, FilterType ftype, int TicketCriteriaID)
        {
            SqlParameter _pTicketCriteriaId=new SqlParameter("@TicketCriteriaId", SqlDbType.Int);
            if (TicketCriteriaID!=0) _pTicketCriteriaId.Value=TicketCriteriaID;
            else _pTicketCriteriaId.Value=DBNull.Value;
            return SelectRecords("sp_SelectTicketCriteriaDataByType", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@FilterType", (byte)ftype), _pTicketCriteriaId});
        }

        public static int Update(int DeptID, int CriteriaID, int UserId, int NotificationRuleId, CriteriaType ctype, string CriteriaName, CriteriaState cstate)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pCriteriaId = new SqlParameter("@Id", SqlDbType.Int);
            _pCriteriaId.Direction = ParameterDirection.InputOutput;
            if (CriteriaID != 0) _pCriteriaId.Value = CriteriaID;
            else _pCriteriaId.Value = DBNull.Value;
            SqlParameter _pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            if (UserId != 0) _pUserId.Value = UserId;
            else _pUserId.Value = DBNull.Value;
            SqlParameter _pNotificationRuleId = new SqlParameter("@NotificationRuleId", SqlDbType.Int);
            if (NotificationRuleId != 0) _pNotificationRuleId.Value = NotificationRuleId;
            else _pNotificationRuleId.Value = DBNull.Value;
            SqlParameter _pBitLimitToAssignedTkts = new SqlParameter("@btLimitToAssignedTkts", SqlDbType.Bit);
            SqlParameter _pBitDisabledReports = new SqlParameter("@btDisabledReports", SqlDbType.Bit);
            SqlParameter _pBitNoFilter = new SqlParameter("@btNoFilter", SqlDbType.Bit);
            switch (cstate)
            {
                case CriteriaState.DisabledReports:
                    _pBitDisabledReports.Value = true;
                    _pBitLimitToAssignedTkts.Value = false;
                    _pBitNoFilter.Value = true;
                    break;
                case CriteriaState.FilterEnabled:
                    _pBitDisabledReports.Value = false;
                    _pBitLimitToAssignedTkts.Value = false;
                    _pBitNoFilter.Value = false;
                    break;
                case CriteriaState.LimitToAssignedTickets:
                    _pBitDisabledReports.Value = false;
                    _pBitLimitToAssignedTkts.Value = true;
                    _pBitNoFilter.Value = false;
                    break;
                case CriteriaState.NoFilter:
                    _pBitDisabledReports.Value = false;
                    _pBitLimitToAssignedTkts.Value = false;
                    _pBitNoFilter.Value = true;
                    break;
            }
            UpdateData("sp_UpdateTicketCriteria", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), _pCriteriaId, _pUserId, _pNotificationRuleId, new SqlParameter("@CriteriaType", (byte)ctype), new SqlParameter("@CriteriaName", CriteriaName), _pBitLimitToAssignedTkts, _pBitDisabledReports, _pBitNoFilter });
            if ((int)_pRVAL.Value < 0) return (int)_pRVAL.Value;
            else return (int)_pCriteriaId.Value;
        }

        public static void Delete(int DeptID, int CriteriaID)
        {
            UpdateData("sp_DeleteTicketCriteria", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", CriteriaID) });
        }

        public static int UpdateCriteriaDataByType(int DeptID, int CriteriaID, FilterType ftype, int ItemID, bool IsExclude, bool Enabled)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateTicketCriteriaData", new SqlParameter[] {_pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@TicketCriteriaId", CriteriaID), new SqlParameter("@FilterType", (byte)ftype), new SqlParameter("@FilterDataId", ItemID), new SqlParameter("@btExclude", IsExclude), new SqlParameter("@Enabled", Enabled)});
            return (int)_pRVAL.Value;
        }

        public static void DeleteCriteriaDataByType(int DeptID, int CriteriaID, FilterType ftype)
        {
            UpdateData("sp_DeleteTicketCriteriaData", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@TicketCriteriaId", CriteriaID), new SqlParameter("@FilterType", (byte)ftype)});
        }

        public class TicketCriteria : DBAccess
        {
            protected int m_DeptID=0;
            protected int m_ID=0;
            protected int m_UserID=0;
            protected int m_GlobalFilterUserId = 0;
            protected bool m_IsGlobalFilterEnabled = false;
            protected int m_NotificationRuleID = 0;
            protected CriteriaType m_CriteriaType = CriteriaType.GlobalFilter;
            protected CriteriaState m_CriteriaState = CriteriaState.NoFilter;
            protected string m_Name = string.Empty;
            protected bool[] m_FilterTypeStates = new bool[8] { false, false, false, false, false, false, false, false };

            public TicketCriteria(int DeptID)
            {
                m_DeptID=DeptID;
            }

            public TicketCriteria(int DeptID, int TicketCriteriaID)
            {
                m_DeptID = DeptID;
                if (TicketCriteriaID == 0) return;
                DataRow _row = SelectOne(DeptID, TicketCriteriaID);
                if (_row == null) return;
                m_ID = TicketCriteriaID;
                m_UserID = _row.IsNull("UserId") ? 0 : (int)_row["UserId"];
                m_NotificationRuleID = _row.IsNull("NotificationRuleId") ? 0 : (int)_row["NotificationRuleId"];
                m_Name = _row["CriteriaName"].ToString();
                m_CriteriaType = (CriteriaType)((byte)_row["CriteriaType"]);
                if (!(bool)_row["btNoFilter"]) m_CriteriaState = CriteriaState.FilterEnabled;
                else if ((bool)_row["btDisabledReports"]) m_CriteriaState = CriteriaState.DisabledReports;
                else if ((bool)_row["btLimitToAssignedTkts"]) m_CriteriaState = CriteriaState.LimitToAssignedTickets;
                DataTable _dt=SelectCriteriaDataByType(DeptID, FilterType.CommonFilter, TicketCriteriaID);
                foreach (DataRow _r in _dt.Rows) m_FilterTypeStates[(int)_r["ID"]]=(bool)_r["State"];
            }

            public DataTable GetFilterDataByType(FilterType ftype)
            {
                if (m_IsGlobalFilterEnabled)
                {
                    switch (ftype)
                    {
                        case FilterType.Accounts:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.Accounts);
                        case FilterType.Classes:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.Classes);
                        case FilterType.Levels:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.Levels);
                        case FilterType.Locations:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.Locations);
                        case FilterType.SupportGroups:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.SupportGroups);
                        case FilterType.UnassignedQueue:
                            return GlobalFilters.SetFilter(m_DeptID, m_GlobalFilterUserId, SelectCriteriaDataByType(m_DeptID, ftype, m_ID), "ID", GlobalFilters.FilterType.UnassignedQueue);
                        default:
                            return SelectCriteriaDataByType(m_DeptID, ftype, m_ID);
                    }
                }
                else return SelectCriteriaDataByType(m_DeptID, ftype, m_ID);
            }

            public bool IsFilterEnabled(FilterType ftype)
            {
               return m_FilterTypeStates[(int)ftype];
            }

            public void SetFilterState(FilterType ftype, bool IsEnabled)
            {
                m_FilterTypeStates[(int)ftype] = IsEnabled;
            }

            public int ID
            {
                get { return m_ID; }
            }

            public int DepartmentID
            {
                get { return m_DeptID; }
                set { m_DeptID = value; }
            }

            public int NotificationRuleID
            {
                get { return m_NotificationRuleID; }
                set { m_NotificationRuleID = value; }
            }

            public int UserID
            {
                get { return m_UserID; }
                set { m_UserID = value; }
            }

            public int GlobalFilterUserID
            {
                get { return m_GlobalFilterUserId; }
                set 
                { 
                    m_GlobalFilterUserId = value;
                    if (m_GlobalFilterUserId != 0) m_IsGlobalFilterEnabled = GlobalFilters.IsFilterEnabled(m_DeptID, m_GlobalFilterUserId, GlobalFilters.FilterState.EnabledGlobalFilters);
                    else m_IsGlobalFilterEnabled = false;
                }
            }

            public CriteriaType Type
            {
                get { return m_CriteriaType; }
                set { m_CriteriaType = value; }
            }

            public CriteriaState State
            {
                get { return m_CriteriaState; }
                set { m_CriteriaState = value; }
            }

            public string Name
            {
                get { return m_Name; }
                set { m_Name = value; }
            }
        }
    }
}
