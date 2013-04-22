using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class MailGroups : DBAccess
    {
        public MailGroups()
        {
        }

        public static DataTable SelectAll(int DeptID)
        {
            return SelectRecords("sp_SelectMailGroups", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID) });
        }

        public static DataRow SelectByName(int DeptID, string MailGroupName)
        {
            return SelectRecord("sp_SelectMailGroups", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupName", MailGroupName) });
        }

        public static DataRow SelectById(int DeptID, int MailGroupId)
        {
            return SelectRecord("sp_SelectMailGroups", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupID", MailGroupId) });
        }

        public static void Delete(int DeptID, int MailGroupId)
        {
            UpdateData("sp_DeleteMailGroup", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupID", MailGroupId) });
        }

        public static void Update(int DeptID, int MailGroupId, string MailGroupName)
        {
            UpdateData("sp_UpdateMailGroup", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@MailGroupID", MailGroupId), new SqlParameter("@Name", MailGroupName) });
        }

        public static void Insert(int DeptID, string MailGroupName)
        {
            UpdateData("sp_InsertMailGroup", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@Name", MailGroupName) });
        }
    }
}
