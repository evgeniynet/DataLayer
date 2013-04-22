using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;
using System.Xml.Serialization;

namespace bigWebApps.bigWebDesk.Data
{
    public class NotificationRules : DBAccess
    {
        public enum TicketEvent
        {
            NewTicket = 0,
            PlaceOnHoldTicket = 1,
            PartsOnOrderTicket = 2,
            CloseTicket = 3,
            DeleteTicket = 4,
            TicketUpdates = 5,
            ChangePriority = 6,
            ChangeLevel = 7,
            ChangeClass = 8,
            TransferTicket = 9,
            ChangeLocation = 10,
            ChangeAccount = 11,
            RequestPart = 12,
            EnterLaborCosts = 13,
            EnterMiscCosts = 14,
            EnterTravelCosts = 15,
            UploadTicketFiles = 16,
            TicketResponse = 17,
            TicketConfirmation = 18,
            ReOpenTicket = 19,
            PickUpTicket = 20,
            DirectMail = 21,
            OrderPart = 22,
            ReceivePart = 23,
            DeleteLostPart = 24,
            DirectMailAsHTML = 25,
            ChangeEndUser = 26,
            ChangeProject = 27
        }

        public enum UserEmail
        {
            Normal = 0,
            Mobile = 1
        }

        public enum SendToState
        {
            NotSend = 0,
            NormalEmail = 1,
            MobileEmail = 2,
            AllEmails = 3
        }

        public NotificationRules()
        {
        }

        public static DataRow SelectOne(int DeptID, int NotificationRuleID)
        {
            return SelectRecord("sp_SelectNotificationRules", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", NotificationRuleID) });
        }

        public static DataRow SelectOne(int DeptID, bool IsForTech)
        {
            SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
            _pId.Value = 0;
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            _pInactive.Value = DBNull.Value;
            SqlParameter _pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            _pUserId.Value = DBNull.Value;
            return SelectRecord("sp_SelectNotificationRules", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pId, _pInactive, _pUserId, new SqlParameter("@btForTech", IsForTech) });
        }

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(DeptID, InactiveStatus.DoesntMatter);
        }

        public static DataTable SelectAll(int DeptID, InactiveStatus inactiveStatus)
        {
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            if (inactiveStatus == InactiveStatus.DoesntMatter)
                _pInactive.Value = DBNull.Value;
            else
                _pInactive.Value = inactiveStatus;

            return SelectRecords("sp_SelectNotificationRules", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pInactive });
        }

        public static DataTable SelectByUser(int DeptID, int UserID)
        {
            return SelectByUser(DeptID, UserID, InactiveStatus.DoesntMatter);
        }

        public static DataTable SelectByUser(int DeptID, int UserID, InactiveStatus inactiveStatus)
        {
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            if (inactiveStatus == InactiveStatus.DoesntMatter)
                _pInactive.Value = DBNull.Value;
            else
                _pInactive.Value = inactiveStatus;

            return SelectRecords("sp_SelectNotificationRules", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", SqlDbType.Int), _pInactive, new SqlParameter("@UserId", UserID) });
        }

        public static DataTable SelectUsers(int DeptID, int NotificationRuleID)
        {
            return SelectUsers(DeptID, NotificationRuleID, 0);
        }

        public static DataTable SelectUsers(int DeptID, int NotificationRuleID, int UserId)
        {
            SqlParameter _pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            if (UserId != 0) _pUserId.Value = UserId;
            else _pUserId.Value = DBNull.Value;

            return SelectRecords("sp_SelectNotificationRuleUsers", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@NotificationRuleId", NotificationRuleID), _pUserId });
        }

        public static DataTable SelectEvents(int DeptID, int NotificationRuleID)
        {
            return SelectEvents(DeptID, NotificationRuleID, false);
        }

        public static DataTable SelectEvents(int DeptID, bool IsForTech)
        {
            return SelectEvents(DeptID, 0, IsForTech);
        }

        public static DataTable SelectEvents(int DeptID, int NotificationRuleID, bool IsForTech)
        {
            SqlParameter _pNotificationRuleID = new SqlParameter("@NotificationRuleId", SqlDbType.Int);
            if (NotificationRuleID != 0) _pNotificationRuleID.Value = NotificationRuleID;
            else _pNotificationRuleID.Value = DBNull.Value;
            return SelectRecords("sp_SelectNotificationRuleEvents", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pNotificationRuleID, new SqlParameter("@btForTech", IsForTech) });
        }

        public static int Update(int DeptID, int RuleId, int UserId, string RuleName, int UpdatedUserId, bool Inactive)
        {
            return Update(DeptID, RuleId, UserId, RuleName, UpdatedUserId, Inactive, false, false, false);
        }

        public static int Update(int DeptID, int RuleId, int UserId, string RuleName, int UpdatedUserId, bool Inactive, bool BuiltIn, bool ForTech, bool ReceiveTriggerEvents)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pRuleId = new SqlParameter("@Id", SqlDbType.Int);
            _pRuleId.Direction = ParameterDirection.InputOutput;
            if (RuleId != 0) _pRuleId.Value = RuleId;
            else _pRuleId.Value = DBNull.Value;
            SqlParameter _pUserId = new SqlParameter("@UserId", SqlDbType.Int);
            if (UserId != 0) _pUserId.Value = UserId;
            else _pUserId.Value = DBNull.Value;
            UpdateData("sp_UpdateNotificationRule", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), _pRuleId, _pUserId, new SqlParameter("@RuleName", RuleName), new SqlParameter("@UpdatedByUserId", UpdatedUserId), new SqlParameter("@btInactive", Inactive), new SqlParameter("@btBuiltIn", BuiltIn), new SqlParameter("@btForTech", ForTech), new SqlParameter("@btReceiveTriggerEvents", ReceiveTriggerEvents) });
            if ((int)_pRVAL.Value < 0) return (int)_pRVAL.Value;
            else return (int)_pRuleId.Value;
        }

        public static int UpdateUser(int DeptID, int NotificationRuleId, int UserId, int UserGroupId, byte NotificationEmail)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateNotificationRuleUser", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@NotificationRuleId", NotificationRuleId), new SqlParameter("@UserId", UserId), new SqlParameter("@UserGroupId", UserGroupId), new SqlParameter("@NotificationEmail", NotificationEmail) });
            return (int)_pRVAL.Value;
        }

        public static void DeleteUsers(int DeptID, int NotificationRuleId)
        {
            UpdateData("sp_DeleteNotificationRuleUsers", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@NotificationRuleId", NotificationRuleId) });
        }

        public static int UpdateEvent(int DeptID, int NotificationRuleId, TicketEvent NotificationEvent, bool Enabled)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateNotificationRuleEvent", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@NotificationRuleId", NotificationRuleId), new SqlParameter("@EventType", (byte)NotificationEvent), new SqlParameter("@Enabled", Enabled) });
            return (int)_pRVAL.Value;
        }

        public static void DeleteEvents(int DeptID, int NotificationRuleId)
        {
            DeleteEvents(Guid.Empty, DeptID, NotificationRuleId);
        }

        public static void DeleteEvents(Guid OrgID, int DeptID, int NotificationRuleId)
        {
            UpdateData("sp_DeleteNotificationRuleEvents", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@NotificationRuleId", NotificationRuleId) }, OrgID);
        }

        public static void Delete(int DeptID, int NotificationRuleId)
        {
            UpdateData("sp_DeleteNotificationRule", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", NotificationRuleId) });
        }


        public static int RaiseNotificationEvent(int DeptID, int UserId, int ScheduledTicketId, DateTime RunTime, FileItem[] tktFiles)
        {
            NotificationEventsQueue.DeleteEvents(Guid.Empty, DeptID, ScheduledTicketId);
            return RaiseNotificationEvent(Guid.Empty, DeptID, UserId, TicketEvent.NewTicket, null, null, ScheduledTicketId, RunTime, tktFiles);
        }

        public static int RaiseNotificationEvent(Guid OrgId, int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt)
        {
            return RaiseNotificationEvent(OrgId, DeptID, UserId, tktEvent, tkt, null, 0, DateTime.MinValue, null);
        }

        public static int RaiseNotificationEvent(int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt)
        {
            return RaiseNotificationEvent(Guid.Empty, DeptID, UserId, tktEvent, tkt, null, 0, DateTime.MinValue, null);
        }

        public static int RaiseNotificationEvent(int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt, Ticket oldTkt)
        {
            return RaiseNotificationEvent(Guid.Empty, DeptID, UserId, tktEvent, tkt, oldTkt, 0, DateTime.MinValue, null);
        }

        public static int RaiseNotificationEvent(int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt, Ticket oldTkt, FileItem[] tktFiles)
        {
            return RaiseNotificationEvent(Guid.Empty, DeptID, UserId, tktEvent, tkt, oldTkt, tktFiles);
        }

        public static int RaiseNotificationEvent(Guid OrgId, int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt, Ticket oldTkt, FileItem[] tktFiles)
        {
            return RaiseNotificationEvent(OrgId, DeptID, UserId, tktEvent, tkt, oldTkt, 0, DateTime.MinValue, tktFiles);
        }

        public static int RaiseNotificationEvent(Guid OrgID, int DeptID, int UserId, TicketEvent tktEvent, Ticket tkt, Ticket oldTkt, int ScheduledTicketId, DateTime RunTime, FileItem[] tktFiles)
        {
            XmlSerializer _serializer = new XmlSerializer(typeof(Ticket));
            string _objStateNew = string.Empty;
            if (tkt != null)
            {
                TextWriter _stream = new StringWriter();
                _serializer.Serialize(_stream, tkt);
                _objStateNew = _stream.ToString();
                _stream.Close();
            }
            string _objStateOld = string.Empty;
            if (oldTkt != null)
            {
                TextWriter _stream = new StringWriter();
                _serializer.Serialize(_stream, oldTkt);
                _objStateOld = _stream.ToString();
                _stream.Close();
            }
            return NotificationEventsQueue.InsertEvent(OrgID, DeptID, UserId, tktEvent, _objStateNew, _objStateOld, tktFiles, ScheduledTicketId, RunTime);
        }

        public static SendToState SelectSendToUserState(int DeptID, int TktID, int ToUserId, TicketEvent TktEvent)
        {
            SqlParameter _pState = new SqlParameter("@State", SqlDbType.TinyInt);
            _pState.Direction = ParameterDirection.Output;
            UpdateData("sp_SelectNotificationRuleState", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@ToUserId", ToUserId), new SqlParameter("@TktEvent", (byte)TktEvent), _pState });
            return (SendToState)(byte)_pState.Value;
        }

        public class NotificationRule
        {
            protected TicketCriterias.TicketCriteria m_TicketCriteria;
            protected bool[] m_TicketEvents = new bool[28];
            protected string m_Name = string.Empty;
            protected bool m_Inactive = false;
            protected int m_UserId = 0;
            protected bool m_BuiltIn = false;
            protected bool m_ForTech = false;
            protected bool m_ReceiveTriggerEvents = false;
            protected UserEmail m_UserEmail = UserEmail.Normal;

            public NotificationRule(int DeptID)
            {
                m_TicketCriteria = new TicketCriterias.TicketCriteria(DeptID);
            }

            public NotificationRule(int DeptID, bool IsForTech)
                : this(DeptID)
            {
                m_ForTech = IsForTech;
                m_BuiltIn = true;
                DataRow _row = SelectOne(DeptID, IsForTech);
                int _id = 0;
                if (_row != null)
                {
                    _id = (int)_row["Id"];
                    m_ReceiveTriggerEvents = (bool)_row["btReceiveTriggerEvents"];
                }
                DataTable _dt = SelectEvents(DeptID, IsForTech);
                if (_dt.Rows.Count > 0)
                {
                    foreach (DataRow _r in _dt.Rows) m_TicketEvents[(byte)_r["EventType"]] = true;
                }
                else
                {
                    for (int i = 0; i < m_TicketEvents.Length; i++) m_TicketEvents[i] = true;
                }
                _dt = SelectUsers(DeptID, _id);
                if (_dt.Rows.Count > 0) m_UserEmail = (UserEmail)(byte)_dt.Rows[0]["NotificationEmail"];
            }

            public NotificationRule(int DeptID, int NotificationRuleID)
            {
                DataRow _row = TicketCriterias.SelectOneForNotificationRule(DeptID, NotificationRuleID);
                if (_row != null) m_TicketCriteria = new TicketCriterias.TicketCriteria(DeptID, (int)_row["Id"]);
                else m_TicketCriteria = new TicketCriterias.TicketCriteria(DeptID);
                m_TicketCriteria.NotificationRuleID = NotificationRuleID;
                _row = SelectOne(DeptID, NotificationRuleID);
                if (_row == null) return;
                m_Name = _row["RuleName"].ToString();
                m_Inactive = (bool)_row["btInactive"];
                if (!_row.IsNull("UserId")) m_UserId = (int)_row["UserId"];
                m_BuiltIn = (bool)_row["btBuiltIn"];
                m_ForTech = (bool)_row["btForTech"];
                this.ReadOnly = (bool)_row["ReadOnly"];
                m_ReceiveTriggerEvents = (bool)_row["btReceiveTriggerEvents"];
                DataTable _dt = SelectEvents(DeptID, NotificationRuleID);
                foreach (DataRow _r in _dt.Rows) m_TicketEvents[(byte)_r["EventType"]] = true;
                _dt = SelectUsers(DeptID, NotificationRuleID);
                if (_dt.Rows.Count > 0) m_UserEmail = (UserEmail)(byte)_dt.Rows[0]["NotificationEmail"];
            }

            public bool IsTicketEventEnabled(TicketEvent tevent)
            {
                return m_TicketEvents[(int)tevent];
            }

            public void SetTicketEventState(TicketEvent tevent, bool IsEnabled)
            {
                m_TicketEvents[(int)tevent] = IsEnabled;
            }

            public string Name
            {
                get { return m_Name; }
                set { m_Name = value; }
            }

            public bool Inactive
            {
                get { return m_Inactive; }
                set { m_Inactive = value; }
            }

            public int UserId
            {
                get { return m_UserId; }
                set { m_UserId = value; }
            }

            public bool BuiltIn
            {
                get { return m_BuiltIn; }
                set { m_BuiltIn = value; }
            }

            public bool ForTech
            {
                get { return m_ForTech; }
                set { m_ForTech = value; }
            }

            public bool ReadOnly { get; set; }

            public bool ReceiveTriggerEvents
            {
                get { return m_ReceiveTriggerEvents; }
                set { m_ReceiveTriggerEvents = value; }
            }

            public UserEmail NotificationEmail
            {
                get { return m_UserEmail; }
                set { m_UserEmail = value; }
            }

            public TicketCriterias.TicketCriteria TicketCriteria
            {
                get { return m_TicketCriteria; }
                set { m_TicketCriteria = value; }
            }
        }
    }
}
