using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class Invoice : DBAccess
    {
        public static DataTable SelectInvoices(int companyID, DateTime beginDate, DateTime endDate, bool archived)
        {
            return SelectInvoices(Guid.Empty, companyID, beginDate, endDate, archived);
        }

        public static DataTable SelectInvoices(Guid orgId, int companyID, DateTime beginDate, DateTime endDate, bool archived)
        {
            return SelectRecords("sp_SelectInvoices", 
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@BeginDate", beginDate),
                    new SqlParameter("@EndDate", endDate),
                    new SqlParameter("@Archived", archived)
                }, orgId);
        }

        public static DataRow SelectLastSuccessDR(int companyID)
        {
            return SelectRecord("sp_SelectInvoiceLastDRRange",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID)
                });
        }

        public static DataTable SelectUnbilledProjects(int companyID, DateTime beginDate, DateTime endDate)
        {
            bool showAllProjects = false;
            if (beginDate == DateTime.MinValue && endDate == DateTime.MaxValue)
            {
                showAllProjects = true;
            }
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@BeginDate", SqlDbType.SmallDateTime);
            if (beginDate == DateTime.MinValue)
            {
                _params[1].Value = new DateTime(2000, 1, 1);
            }
            else
            {
                _params[1].Value = beginDate;
            }
            _params[2] = new SqlParameter("@EndDate", SqlDbType.SmallDateTime);
            if (endDate == DateTime.MaxValue)
            {
                _params[2].Value = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 0);
            }
            else
            {
                _params[2].Value = endDate;
            }
            _params[3] = new SqlParameter("@ShowAllProjects", showAllProjects);

            return SelectRecords("sp_SelectUnbilledProjects", _params);
        }

        public static int CreateInvoice(int companyID, int userId, int accountId, int projectId, DateTime beginDate,
            DateTime endDate, bool calculateRange)
        {
            SqlParameter[] _params = new SqlParameter[8];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@UserId", userId);
            _params[2] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[2].Value = DBNull.Value;
            else _params[2].Value = accountId;
            _params[3] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[3].Value = DBNull.Value;
            else _params[3].Value = projectId;
            _params[4] = new SqlParameter("@BeginDate", beginDate);
            _params[5] = new SqlParameter("@EndDate", endDate);
            _params[6] = new SqlParameter("@CalculateRange", calculateRange);
            int invoiceID = 0;
            SqlParameter pInvoiceID = new SqlParameter("@InvoiceID", invoiceID);
            pInvoiceID.Direction = ParameterDirection.InputOutput;
            _params[7] = pInvoiceID;
            UpdateData("sp_InsertInvoice", _params);
            int.TryParse(pInvoiceID.Value.ToString(), out invoiceID);
            return invoiceID;
        }

        public static DataRow SelectInvoice(int companyID, int invoiceID)
        {
            return SelectRecord("sp_SelectInvoice",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@InvoiceId", invoiceID)
                });
        }

        public static DataTable SelectInvoiceTimeLogs(int companyID, int invoiceID)
        {
            return SelectRecords("sp_SelectInvoiceTimeLogs",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@InvoiceId", invoiceID)
                });
        }

        public static DataTable SelectInvoiceForExport(int companyID, int invoiceID)
        {
            return SelectRecords("sp_SelectInvoiceForExport",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@InvoiceID", invoiceID)                    
                });
        }

        public static DataTable SelectInvoiceRetainers(int companyID, int invoiceID)
        {
            return SelectRecords("sp_SelectInvoiceRetainers",
                new SqlParameter[] 
                { 
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@InvoiceID", invoiceID)                    
                });
        }

        public static DataTable SelectTicketTimeForInvoice(int companyID, DateTime beginDate, DateTime endDate,
            int accountId, int projectId)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@BeginDate", beginDate);
            _params[2] = new SqlParameter("@EndDate", endDate);
            _params[3] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[3].Value = DBNull.Value;
            else _params[3].Value = accountId;
            _params[4] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[4].Value = DBNull.Value;
            else _params[4].Value = projectId;
            return SelectRecords("sp_SelectTicketTimeForInvoice", _params);
        }

        public static DataTable SelectProjectTimeForInvoice(int companyID, DateTime beginDate, DateTime endDate,
            int accountId, int projectId)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@BeginDate", beginDate);
            _params[2] = new SqlParameter("@EndDate", endDate);
            _params[3] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[3].Value = DBNull.Value;
            else _params[3].Value = accountId;
            _params[4] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[4].Value = DBNull.Value;
            else _params[4].Value = projectId;
            return SelectRecords("sp_SelectProjectTimeForInvoice", _params);
        }

        public static DataTable SelectBillingDataForInvoice(int companyID, int accountId, int projectId)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[1].Value = DBNull.Value;
            else _params[1].Value = accountId;
            _params[2] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[2].Value = DBNull.Value;
            else _params[2].Value = projectId;
            return SelectRecords("sp_SelectBillingDataForInvoice", _params);
        }

        public static DataTable SelectRetainersForInvoice(int companyID, DateTime beginDate, DateTime endDate,
            int accountId, int projectId)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@BeginDate", beginDate);
            _params[2] = new SqlParameter("@EndDate", endDate);
            _params[3] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[3].Value = DBNull.Value;
            else _params[3].Value = accountId;
            _params[4] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[4].Value = DBNull.Value;
            else _params[4].Value = projectId;
            return SelectRecords("sp_SelectRetainersForInvoice", _params);
        }

        public static void DeleteTicketTimeFromInvoice(int companyID, int ticketTimeID, int invoiceID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TicketTimeID", ticketTimeID);
            _params[2] = new SqlParameter("@InvoiceID", invoiceID);
            UpdateData("sp_DeleteTicketTimeFromInvoice", _params);
        }

        public static void DeleteProjectTimeFromInvoice(int companyID, int projectTimeID, int invoiceID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@ProjectTimeID", projectTimeID);
            _params[2] = new SqlParameter("@InvoiceID", invoiceID);
            UpdateData("sp_DeleteProjectTimeFromInvoice", _params);
        }

        public static DataTable SelectTravelTimeForInvoice(int companyID,
            int accountId, int projectId, DateTime beginDate, DateTime endDate)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountId == -1) _params[1].Value = DBNull.Value;
            else _params[1].Value = accountId;
            _params[2] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectId == 0) _params[2].Value = DBNull.Value;
            else _params[2].Value = projectId;
            _params[3] = new SqlParameter("@BeginDate", beginDate);
            _params[4] = new SqlParameter("@EndDate", endDate);
            return SelectRecords("sp_SelectTravelTimeForInvoice", _params);
        }

        public static void DeleteTicketTravelCostsFromInvoice(int companyID, int ticketTravelCostsID, int invoiceID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TicketTravelCostsID", ticketTravelCostsID);
            _params[2] = new SqlParameter("@InvoiceID", invoiceID);
            UpdateData("sp_DeleteTicketTravelCostsFromInvoice", _params);
        }

        public static DataTable SelectInvoiceTicketTravelCosts(int companyID, int invoiceID)
        {
            return SelectRecords("sp_SelectInvoiceTicketTravelCosts",
                new SqlParameter[] 
                { 
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@InvoiceID", invoiceID)                    
                });
        }

        public static void DeleteInvoice(int companyID, int invoiceID)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@DepartmentId", companyID);
            _params[1] = new SqlParameter("@InvoiceID", invoiceID);
            UpdateData("sp_DeleteInvoice", _params);
        }

        public static void CalculateStartSopDates(int DeptID, out DateTime beginDateDT, out DateTime endDateDT)
        {
            DataRow _drLastDR = SelectLastSuccessDR(DeptID);
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

    }
}

