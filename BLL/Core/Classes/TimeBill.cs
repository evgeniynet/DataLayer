using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class TimeBill : DBAccess
    {
        public static DataTable SelectTimeBills(int companyID, DateTime beginDate, DateTime endDate)
        {
            return SelectTimeBills(Guid.Empty,companyID,beginDate,endDate);
        }

        public static DataTable SelectTimeBills(Guid OrgId, int companyID, DateTime beginDate, DateTime endDate)
        {
            return SelectRecords("sp_SelectTimeBills", 
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BeginDate", beginDate),
                    new SqlParameter("@EndDate", endDate)
                }, OrgId);
        }

        public static DataRow SelectLastSuccessDR(int companyID, int techID)
        {
            return SelectRecord("sp_SelectTimeBillLastDRRange",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@TechId", techID)
                });
        }

        public static int CreateTimeBill(int companyID, int userId, int techId, DateTime beginDate, DateTime endDate)
        {
            int billID = 0;
            SqlParameter pBillID = new SqlParameter("@BillID", billID);
            pBillID.Direction = ParameterDirection.InputOutput;
            SqlParameter[] _params = new SqlParameter[6];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@UserId", userId);
            _params[2] = new SqlParameter("@TechId", techId);
            _params[3] = new SqlParameter("@BeginDate", beginDate);
            _params[4] = new SqlParameter("@EndDate", endDate);
            _params[5] = pBillID;
            UpdateData("sp_InsertTimeBill", _params);
            int.TryParse(pBillID.Value.ToString(), out billID);
            return billID;
        }

        public static DataRow SelectTimeBill(int companyID, int billID)
        {
            return SelectRecord("sp_SelectTimeBill",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BillId", billID)
                });
        }

        public static DataTable SelectTimeBillLogs(int companyID, int billID)
        {
            return SelectTimeBillLogs(Guid.Empty, companyID, billID);
        }

        public static DataTable SelectTimeBillLogs(Guid OrgId, int companyID, int billID)
        {
            return SelectRecords("sp_SelectTimeBillLogs",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BillId", billID)
                },OrgId);
        }

        public static DataTable SelectTimeBillsForExport(int companyID, string sWhereUpdateStatus)
        {
            return SelectRecords("sp_SelectTimeBillsForExport",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@Ids", sWhereUpdateStatus)                    
                });
        }

        public static DataTable SelectUnbilledLogs(int companyID, DateTime beginDate, DateTime endDate)
        {
            return SelectRecords("sp_SelectUnbilledTimeLogs",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BeginDate", beginDate),
                    new SqlParameter("@EndDate", endDate)
                });
        }

        public static DataTable AmountGroupByAccount(int companyID, int billID)
        {
            return SelectRecords("sp_SelectTimeBillAmountGroupByAccount",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BillID", billID)
                });
        }

        public static DataTable SelectTimeBillAmountGroupByTaskType(int companyID, int billID)
        {
            return SelectRecords("sp_SelectTimeBillAmountGroupByTaskType",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BillId", billID)
                });
        }

        public static DataTable SelectTimeLogsForBill(int companyID, DateTime beginDate, DateTime endDate, int userID)
        {
            return SelectRecords("sp_SelectTimeLogsForBill",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BeginDate", beginDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@UserID", userID)
                });
        }

        public static void CalculateStartSopDates(int DeptID, out DateTime beginDateDT, out DateTime endDateDT, int techID)
        {
            DataRow _drLastDR = SelectLastSuccessDR(DeptID, techID);
            DateTime ActualDate = Functions.DB2UserDateTime(DateTime.UtcNow);
            if (_drLastDR == null)
            {
                beginDateDT = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0);
                endDateDT = new DateTime(ActualDate.Year, ActualDate.Month, ActualDate.Day, 23, 59, 0);
            }
            else
            {
                DateTime EndDate = ((DateTime)_drLastDR["EndDate"]);
                beginDateDT = (new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, 0, 0, 0)).AddDays(1);
                if (beginDateDT > (new DateTime(ActualDate.Year, ActualDate.Month, ActualDate.Day, 23, 59, 0)))
                    beginDateDT = new DateTime(ActualDate.Year, ActualDate.Month, ActualDate.Day, 0, 0, 0);
                endDateDT = new DateTime(ActualDate.Year, ActualDate.Month, ActualDate.Day, 23, 59, 0);
            }
            beginDateDT = new DateTime(beginDateDT.Year, beginDateDT.Month, beginDateDT.Day, 0, 0, 0);
            endDateDT = new DateTime(endDateDT.Year, endDateDT.Month, endDateDT.Day, 23, 59, 0);
        }

        public static DataTable SelectTimeLogGroupByTaskType(int companyID, DateTime beginDate, DateTime endDate, int userID)
        {
            return SelectRecords("sp_SelectTimeLogGroupByTaskType",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BeginDate", beginDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@UserID", userID)
                });
        }

        public static void DeleteTicketTimeFromBill(int companyID, int ticketTimeID, int billID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TicketTimeID", ticketTimeID);
            _params[2] = new SqlParameter("@BillID", billID);
            UpdateData("sp_DeleteTicketTimeFromBill", _params);
        }

        public static void DeleteProjectTimeFromBill(int companyID, int projectTimeID, int billID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@ProjectTimeID", projectTimeID);
            _params[2] = new SqlParameter("@BillID", billID);
            UpdateData("sp_DeleteProjectTimeFromBill", _params);
        }
    }
}
