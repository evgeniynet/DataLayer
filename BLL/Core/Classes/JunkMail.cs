using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class JunkMail : DBAccess
    {
        public static DataTable SelectAll(int DepartmentId)
        {
            return SelectRecords("sp_SelectJunkMailRules", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
        }

        public static DataRow SelectOne(int DepartmentId, int RuleId)
        {
            return SelectRecord("sp_SelectJunkMailRule", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@Id", RuleId) });
        }


        public static void Delete(int DepartmentId, int RuleId, int UserId)
        {
            UpdateData("sp_UpdateJunkMailRule", new SqlParameter[] {
                new SqlParameter("@DId", DepartmentId),
                new SqlParameter("@Id", RuleId),
                new SqlParameter("@MO", "d"),
                new SqlParameter("@UId", UserId)
            });
        }

        public static void Update(int DepartmentId, int RuleId, int UserId, string email, string subject)
        {
            UpdateData("sp_UpdateJunkMailRule", new SqlParameter[] {
                new SqlParameter("@DId", DepartmentId),
                new SqlParameter("@Id", RuleId),
                new SqlParameter("@MO", "u"),
                new SqlParameter("@UId", UserId),
                new SqlParameter("@vchEmail", email),
                new SqlParameter("@vchSubject", subject)
            });
        }
    }
}
