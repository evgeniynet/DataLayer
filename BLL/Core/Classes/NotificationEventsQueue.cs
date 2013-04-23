using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class NotificationEventsQueue : DBAccess
    {
        public NotificationEventsQueue()
        {
        }

        public static int InsertEvent(int DeptId, int CreatedByUserId, NotificationRules.TicketEvent tktEvent, string ObjectState)
        {
            return InsertEvent(Guid.Empty, DeptId, CreatedByUserId, tktEvent, ObjectState, string.Empty, null, 0, DateTime.MinValue);
        }

        public static int InsertEvent(int DeptId, int CreatedByUserId, NotificationRules.TicketEvent tktEvent, string ObjectState, FileItem[] EventFiles)
        {
            return InsertEvent(Guid.Empty, DeptId, CreatedByUserId, tktEvent, ObjectState, EventFiles);
        }

        public static int InsertEvent(Guid OrgID, int DeptId, int CreatedByUserId, NotificationRules.TicketEvent tktEvent, string ObjectState, FileItem[] EventFiles)
        {
            return InsertEvent(OrgID, DeptId, CreatedByUserId, tktEvent, ObjectState, string.Empty, EventFiles, 0, DateTime.MinValue);
        }

        public static int InsertEvent(Guid OrgID, int DeptId, int CreatedByUserId, NotificationRules.TicketEvent tktEvent, string ObjectStateNew, string ObjectStateOld, FileItem[] EventFiles, int ScheduledTicketId, DateTime RunTime)
        {
            SqlParameter _pId=new SqlParameter("@Id", SqlDbType.Int);
            _pId.Direction=ParameterDirection.InputOutput;
            _pId.Value=DBNull.Value;
            SqlParameter _pObjectStateNew = new SqlParameter("@ObjectStateNew", SqlDbType.NText);
            if (ObjectStateNew.Length > 0) _pObjectStateNew.Value = ObjectStateNew;
            else _pObjectStateNew.Value = DBNull.Value;
            SqlParameter _pObjectStateOld = new SqlParameter("@ObjectStateOld", SqlDbType.NText);
            if (ObjectStateOld.Length > 0) _pObjectStateOld.Value = ObjectStateOld;
            else _pObjectStateOld.Value = DBNull.Value;
            SqlParameter _pScheduledTicketId = new SqlParameter("@ScheduledTicketId", SqlDbType.Int);
            if (ScheduledTicketId != 0) _pScheduledTicketId.Value = ScheduledTicketId;
            else _pScheduledTicketId.Value = DBNull.Value;
            SqlParameter _pRunTime = new SqlParameter("@RunTime", SqlDbType.SmallDateTime);
            if (RunTime == DateTime.MinValue) _pRunTime.Value = DBNull.Value;
            else _pRunTime.Value = RunTime;
            UpdateData("sp_UpdateNotificationEventsQueue", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptId), new SqlParameter("@CreatedByUserId", CreatedByUserId), new SqlParameter("@EventType", (int)tktEvent), _pObjectStateNew, _pObjectStateOld, _pScheduledTicketId, _pRunTime}, OrgID);
            int _id = (int)_pId.Value;
            if (EventFiles == null) return _id;
            foreach (FileItem _file in EventFiles) InsertFile(OrgID, DeptId, _id, _file);
            return _id;
        }

        public static void UpdateEventRunTime(Guid OrgId, int DeptId, int EventId, DateTime RunTime)
        {
            SqlParameter _pRunTime = new SqlParameter("@RunTime", SqlDbType.SmallDateTime);
            if (RunTime == DateTime.MinValue) _pRunTime.Value = DateTime.UtcNow;
            else _pRunTime.Value = RunTime;
            UpdateData("sp_UpdateNotificationEventsQueueRunTime", new SqlParameter[] {_pRunTime, new SqlParameter("@DId", DeptId), new SqlParameter("@Id", EventId)}, OrgId);
        }

        public static int InsertFile(int DeptId, int NotificationEventsQueueId, FileItem EventFile)
        {
            return InsertFile(Guid.Empty, DeptId, NotificationEventsQueueId, EventFile);
        }

        public static int InsertFile(Guid OrgID, int DeptId, int NotificationEventsQueueId, FileItem EventFile)
        {
            SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
            _pId.Direction = ParameterDirection.InputOutput;
            _pId.Value = DBNull.Value;
            SqlParameter _pFileData = new SqlParameter("@FileData", SqlDbType.Image);
            _pFileData.Value = EventFile.Data;
            UpdateData("sp_UpdateNotificationEventsQueueFile", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptId), new SqlParameter("@NotificationEventsQueueId", NotificationEventsQueueId), new SqlParameter("@FileName", EventFile.Name), new SqlParameter("@FileSize", EventFile.Size), _pFileData }, OrgID);
            return (int)_pId.Value;
        }

        public static DataTable SelectNotificationEventFiles(Guid orgID, int DeptID, int ScheduleTicketId)
        {
            SqlParameter _pNotificationEventsQueueId = new SqlParameter("@NotificationEventsQueueId", SqlDbType.Int);
            _pNotificationEventsQueueId.Value = DBNull.Value;
            return SelectRecords("sp_SelectNotificationEventFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pNotificationEventsQueueId, new SqlParameter("@ScheduledTicketId", ScheduleTicketId) }, orgID);
        }

        public static DataTable SelectNotificationEventFiles(int DeptID, int ScheduleTicketId)
        {
            return SelectNotificationEventFiles(Guid.Empty, DeptID, ScheduleTicketId);
        }

        public static FileItem[] SelectNotificationEventFilesToArray(Guid orgID, int DeptID, int ScheduledTicketId)
        {
            DataTable _dt = SelectNotificationEventFiles(orgID, DeptID, ScheduledTicketId);
            FileItem[] _arr = new FileItem[_dt.Rows.Count];
            for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new FileItem((int)_dt.Rows[i]["Id"], _dt.Rows[i]["FileName"].ToString(), (int)_dt.Rows[i]["FileSize"], (DateTime)_dt.Rows[i]["dtUpdated"], !_dt.Rows[i].IsNull("FileData") ? (byte[])_dt.Rows[i]["FileData"] : null);
            return _arr;
        }

        public static FileItem[] SelectNotificationEventFilesToArray(int DeptID, int ScheduledTicketId)
        {
            return SelectNotificationEventFilesToArray(Guid.Empty, DeptID, ScheduledTicketId);
        }

        private static void DeleteEvent(Guid OrgID, int DeptID, int NotificationEventId, int ScheduledTicketId)
        {
            SqlParameter _pNotificationEventId = new SqlParameter("@NotificationEventId", SqlDbType.Int);
            if (NotificationEventId != 0) _pNotificationEventId.Value = NotificationEventId;
            else _pNotificationEventId.Value = DBNull.Value;
            SqlParameter _pScheduledTicketId = new SqlParameter("@ScheduledTicketId", SqlDbType.Int);
            if (ScheduledTicketId != 0) _pScheduledTicketId.Value = ScheduledTicketId;
            else _pScheduledTicketId.Value = DBNull.Value;
            UpdateData("sp_DeleteNotificationEventsQueue", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pNotificationEventId, _pScheduledTicketId }, OrgID);
        }

        public static void DeleteEvent(int DeptID, int NotificationEventId)
        {
            DeleteEvent(Guid.Empty, DeptID, NotificationEventId, 0);
        }

        public static void DeleteEvent(Guid OrgID, int DeptID, int NotificationEventId)
        {
            DeleteEvent(OrgID, DeptID, NotificationEventId, 0);
        }

        public static void DeleteEvents(int DeptID, int ScheduledTicketId)
        {
            DeleteEvent(Guid.Empty, DeptID, 0, ScheduledTicketId);
        }

        public static void DeleteEvents(Guid OrgID, int DeptID, int ScheduledTicketId)
        {
            DeleteEvent(OrgID, DeptID, 0, ScheduledTicketId);
        }

        public static void DeleteEvents(int DeptID)
        {
            DeleteEvent(DeptID, 0);
        }
    }
}
