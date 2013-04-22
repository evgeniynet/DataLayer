using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for UnassignedQueues.
    /// </summary>
    public class UnassignedQueues : DBAccess
    {
        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

        public static DataTable SelectAll(Guid OrgId, int DeptID)
        {
            return SelectRecords("sp_SelectUnassignedQueList", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID) }, OrgId);
        }

        public static DataRow SelectOne(int DeptID, int QueueID)
        {
            return SelectRecord("sp_SelectUnassignedQueDetail", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@QueueId", QueueID) });
        }

        public static DataTable SelectTechs(int DeptID)
        {
            return SelectRecords("sp_SelectTechs", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }

        public static int Update(int DeptID, int QueueId, string QueueName, string QueueMail, bool AllowQueueEmailParsing)
        {
            return Update(Guid.Empty, DeptID, QueueId, QueueName, QueueMail, AllowQueueEmailParsing);
        }

        public static int Update(Guid OrgId, int DeptID, int QueueId, string QueueName, string QueueMail, bool AllowQueueEmailParsing)
        {
            SqlParameter _pQueueMail = new SqlParameter("@QueEmail", DBNull.Value);
            if (QueueMail.Length > 0)
                _pQueueMail.Value = QueueMail;
            
            SqlParameter _pQueueId = new SqlParameter("@Id", QueueId);
            _pQueueId.Direction = ParameterDirection.InputOutput;

            UpdateData("sp_UpdateUnassignedQue",
                        new SqlParameter[] {
                                            new SqlParameter("@DId", DeptID),
                                            _pQueueId,
                                            new SqlParameter("@QueName", QueueName),
                                            _pQueueMail,
                                            new SqlParameter("@bitAllowQueEmailParsing", AllowQueueEmailParsing)
                                            }, OrgId);
            return (int)_pQueueId.Value;
        }
        public static void UpdateQueueMembers(int departmentId, int queueId, List<QueueMember> queueMembers)
        {
            List<QueueMember> oldQueueMembers = SelectQueueMembers(departmentId, queueId);
            foreach (QueueMember oldQueueMember in oldQueueMembers)
            {
                bool isDelete = true;
                foreach (QueueMember queueMember in queueMembers)
                {
                    isDelete = !(oldQueueMember.Id > 0 && oldQueueMember.Id == queueMember.Id);
                    if (!isDelete)
                        break;
                }

                if (isDelete)
                    DeleteQueueMember(departmentId, queueId, oldQueueMember.Id);
            }

            foreach (QueueMember queueMember in queueMembers)
            {
                bool isInsert = true;

                if (queueMember.Id > 0)
                    continue;

                foreach (QueueMember oldQueueMember in oldQueueMembers)
                {
                    if (queueMember.UserId > 0 && oldQueueMember.UserId == queueMember.UserId)
                    {
                        isInsert = false;
                        break;
                    }

                    if (queueMember.UserId == 0 && oldQueueMember.UserId == 0 && !string.IsNullOrEmpty(queueMember.UserEmail) && oldQueueMember.UserEmail == queueMember.UserEmail)
                    {
                        isInsert = false;
                        break;
                    }
                }

                if (isInsert)
                    AddQueueMember(departmentId, queueId, queueMember.UserId, queueMember.UserEmail);
            }
        }

        public static void AddQueueMember(int departmentId, int queueId, int userId, string userEmail)
        {
            AddQueueMember(Guid.Empty, departmentId, queueId, userId, userEmail);
        }

        public static void AddQueueMember(Guid OrgId, int departmentId, int queueId, int userId, string userEmail)
        {
            if (userId > 0 || !string.IsNullOrEmpty(userEmail))
                UpdateData("sp_InsertQueueMember",
                           new SqlParameter[]
                               {
                                   new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@QueueId", queueId),
                                   new SqlParameter("@UserId", userId), new SqlParameter("@UserEmail", userEmail)  }, OrgId);
        }

        public static int Delete(int DeptID, int QueueId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DeleteLogin", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@UId", QueueId) });
            return (int)_pRVAL.Value;
        }
        public static void DeleteQueueMember(int departmentId, int queueId, int id)
        {
            UpdateData("sp_DeleteQueueMember", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@QueueId", queueId), new SqlParameter("@Id", id) });
        }


        public static void Transfer(int DeptID, string LoggedInUserName, int OldUserId, int NewUserId)
        {
            UpdateData("sp_TransferUser",
                        new SqlParameter[] {
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@old_user_id", OldUserId),
                                            new SqlParameter("@new_user_id", NewUserId),
                                            new SqlParameter("@UserName", LoggedInUserName)
                                            });
        }


        public static DataTable SelectQueueMembersTable(int departmentId, int queueId)
        {
            return SelectRecords("sp_SelectQueueMembers",
                                 new SqlParameter[]
                                     {
                                         new SqlParameter("@DepartmentId", departmentId),
                                         new SqlParameter("@QueueId", queueId)
                                     });
        }

        public static List<QueueMember> SelectQueueMembers(int departmentId, int queueId)
        {
            DataTable dtQueueMembers = SelectQueueMembersTable(departmentId, queueId);
            List<QueueMember> queueMembers = new List<QueueMember>();
            if (dtQueueMembers != null)
                foreach (DataRow drQueueMember in dtQueueMembers.Rows)
                {
                    QueueMember queueMember = new QueueMember(drQueueMember.IsNull("Id") ? 0 : (int)drQueueMember["Id"], drQueueMember.IsNull("UserId") ? 0 : (int)drQueueMember["UserId"], drQueueMember.IsNull("UserName") ? string.Empty : (string)drQueueMember["UserName"], drQueueMember.IsNull("UserEmail") ? string.Empty : (string)drQueueMember["UserEmail"]);
                    queueMembers.Add(queueMember);
                }

            return queueMembers;
        }

        [Serializable]
        public class QueueMember
        {
            private Guid gId;
            private int id;
            private int userId;
            private string userName;
            private string userEmail;

            public QueueMember(int userId, string userName, string userEmail)
            {
                this.gId = Guid.NewGuid();
                this.UserId = userId;
                this.UserName = userName;
                this.UserEmail = userEmail;
            }

            public QueueMember(int id, int userId, string userName, string userEmail)
                : this(userId, userName, userEmail)
            {
                this.id = id;
            }

            public string UserEmail
            {
                get { return userEmail; }
                set { userEmail = value; }
            }

            public string UserName
            {
                get { return userName; }
                set { userName = value; }
            }

            public int UserId
            {
                get { return userId; }
                set { userId = value; }
            }

            public int Id
            {
                get { return id; }
            }

            public Guid GId
            {
                get { return gId; }
            }
        }

    }


}
