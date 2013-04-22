using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class RatePlan : DBAccess
    {
        public static DataTable SelectAll(int DepartmentID)
        {
            return SelectRecords("sp_SelectRatePlans", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID) });
        }

        public static DataTable SelectRatePlan(int DepartmentID, int RatePlanID)
        {
            return SelectRecords("sp_SelectRatePlan", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@RatePlanID", RatePlanID) });
        }

        public static DataRow SelectRatePlanName(int DepartmentID, int RatePlanID)
        {
            return SelectRecord("sp_SelectRatePlanName", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@RatePlanID", RatePlanID) });
        }

        public static void UpdateRatePlan(int DepartmentID, int RatePlanId, int TaskTypeId, decimal HourlyRate)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@DepartmentID", DepartmentID);
            _params[1] = new SqlParameter("@RatePlanId", RatePlanId);
            _params[2] = new SqlParameter("@TaskTypeId", TaskTypeId);
            _params[3] = new SqlParameter("@HourlyRate", HourlyRate);
            UpdateData("sp_UpdateRatePlan", _params);
        }

        public static void UpdateRatePlanName(int DepartmentID, int RatePlanId, string Name)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DepartmentID", DepartmentID);
            _params[1] = new SqlParameter("@RatePlanId", RatePlanId);
            _params[2] = new SqlParameter("@Name", Name);            
            UpdateData("sp_UpdateRatePlanName", _params);
        }

        public static int InsertRatePlanName(int DepartmentID, string Name)
        {
            SqlParameter NewRatePlanId = new SqlParameter("@RatePlanId", SqlDbType.Int);
            NewRatePlanId.Direction = ParameterDirection.ReturnValue;
            NewRatePlanId.Value = DBNull.Value;
            UpdateData("sp_InsertRatePlan", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@Name", Name), NewRatePlanId });
            return NewRatePlanId.Value != DBNull.Value ? (int)NewRatePlanId.Value : 0;            
        }

        public static void DeleteRatePlan(int ratePlanID, int companyID)
        {
            UpdateData("sp_DeleteRatePlan", new SqlParameter[]
            {
                 new SqlParameter("@RatePlanID", ratePlanID),
                 new SqlParameter("@CompanyID", companyID),
            });
        }

        public static void DeleteRatePlanRate(int ratePlanRatesID, int companyID)
        {
            UpdateData("sp_DeleteRatePlanRate", new SqlParameter[]
            {
                 new SqlParameter("@RatePlanRatesID", ratePlanRatesID),
                 new SqlParameter("@CompanyID", companyID),
            });
        }
    }
}