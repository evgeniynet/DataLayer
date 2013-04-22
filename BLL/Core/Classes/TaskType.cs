using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{   
    public class TaskType : DBAccess
    {
        public static DataTable SelectAll(int DepartmentID)
        {
            return SelectRecords("sp_SelectTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID) });
        }

        public static DataTable SelectAll(Guid OrgId, int DepartmentID)
        {
            return SelectRecords("sp_SelectTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID) }, OrgId);
        }

        public static DataTable SelectAll(int DepartmentID, bool IsActive)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@DepartmentID", DepartmentID);
            _params[1] = new SqlParameter("@Active", SqlDbType.Bit);
            _params[1].Value = IsActive ? 1 : 0;
            return SelectRecords("sp_SelectTaskTypes", _params);
        }

        public static DataTable SelectProjectAssignedTaskTypes(int DepartmentID, int TechID, int ProjectID)
        {
            return SelectRecords("sp_SelectAssignedTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@TechID", TechID), new SqlParameter("@ProjectID", ProjectID) });
        }

        public static DataTable SelectTicketAssignedTaskTypes(Guid OrgId, int DepartmentID, int TechID, int TicketID)
        {
            return SelectRecords("sp_SelectAssignedTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@TechID", TechID), new SqlParameter("@TicketID", TicketID) }, OrgId);
        }

        public static DataTable SelectTicketAssignedTaskTypes(int DepartmentID, int TechID, int TicketID)
        {
            return SelectTicketAssignedTaskTypes(Guid.Empty, DepartmentID, TechID, TicketID);
        }

        public static DataTable SelectAccountAssignedTaskTypes(int DepartmentID, int TechID, int AccountID)
        {
            return SelectRecords("sp_SelectAssignedTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@TechID", TechID), new SqlParameter("@AcctID", AccountID) });
        }

        public static DataRow SelectTaskType(int DepartmentID, int TaskTypeID)
        {
            return SelectTaskType(Guid.Empty, DepartmentID, TaskTypeID);
        }

        public static DataRow SelectTaskType(Guid OrgId, int DepartmentID, int TaskTypeID)
        {
            return SelectRecord("sp_SelectTaskType", new SqlParameter[] { new SqlParameter("@DepartmentID", DepartmentID), new SqlParameter("@TaskTypeID", TaskTypeID) }, OrgId);
        }

        public static void UpdateTaskType(int DepartmentID, int TaskTypeID, string Name, decimal HourlyRate, bool ActiveStatus,
            bool appliesToTickets, bool appliesToProjects, string QBAccount, string QBItem, decimal Cost)
        {
            SqlParameter[] _params = new SqlParameter[10];
            _params[0] = new SqlParameter("@DepartmentID", DepartmentID);
            _params[1] = new SqlParameter("@TaskTypeID", TaskTypeID);
            _params[2] = new SqlParameter("@TaskTypeName", Name);
            _params[3] = new SqlParameter("@HourlyRate", SqlDbType.SmallMoney);
            if (HourlyRate == decimal.MaxValue) _params[3].Value = DBNull.Value;
            else _params[3].Value = HourlyRate;
            _params[4] = new SqlParameter("@Active", ActiveStatus);
            _params[5] = new SqlParameter("@AppliesToTickets", appliesToTickets);
            _params[6] = new SqlParameter("@AppliesToProjects", appliesToProjects);
            _params[7] = new SqlParameter("@QBAccount", QBAccount);
            _params[8] = new SqlParameter("@QBItem", QBItem);
            _params[9] = new SqlParameter("@Cost", SqlDbType.SmallMoney);
            if (Cost == decimal.MaxValue) _params[9].Value = DBNull.Value;
            else _params[9].Value = Cost;
            UpdateData("sp_UpdateTaskType", _params);
        }

        public static int InsertTaskType(int DepartmentID, string Name, decimal HourlyRate, bool ActiveStatus,
            bool appliesToTickets, bool appliesToProjects, string QBAccount, string QBItem, decimal Cost)
        {
            SqlParameter[] _params = new SqlParameter[10];
            _params[0] = new SqlParameter("@DepartmentID", DepartmentID);
            _params[1] = new SqlParameter("@TaskTypeName", Name);
            _params[2] = new SqlParameter("@HourlyRate", SqlDbType.SmallMoney);
            if (HourlyRate == decimal.MaxValue) _params[2].Value = DBNull.Value;
            else _params[2].Value = HourlyRate;
            _params[3] = new SqlParameter("@Active", ActiveStatus);
            _params[4] = new SqlParameter("@AppliesToTickets", appliesToTickets);
            _params[5] = new SqlParameter("@AppliesToProjects", appliesToProjects);
            _params[6] = new SqlParameter("@QBAccount", QBAccount);
            _params[7] = new SqlParameter("@QBItem", QBItem);
            _params[8] = new SqlParameter("@Cost", SqlDbType.SmallMoney);
            if (Cost == decimal.MaxValue) _params[8].Value = DBNull.Value;
            else _params[8].Value = Cost;
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            _params[9] = _pRVAL;
            UpdateData("sp_InsertTaskType", _params);
            return (int)_params[9].Value;
        }

        public static DataTable SelectTaskTypeTechs(int taskTypeID, int companyID)
        {
            return SelectRecords("sp_SelectTaskTypeTechs",
                new SqlParameter[] { new SqlParameter("@TaskTypeID", taskTypeID),
                new SqlParameter("@CompanyID", companyID) });
        }

        public static DataTable SelectTechTaskTypes(int companyID, int userId)
        {
            return SelectRecords("sp_SelectTechTaskTypes",
                new SqlParameter[] { 
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@UserID", userId)
                 });
        }

        public static void InsertTaskTypeTech(int companyID, int taskTypeID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@TaskTypeID", taskTypeID);
            _params[2] = new SqlParameter("@TechID", techID);
            UpdateData("sp_InsertTaskTypeTech", _params);
        }

        public static void DeleteTaskTypeTech(int taskTypeTechID, int companyID)
        {
            UpdateData("sp_DeleteTaskTypeTech", new SqlParameter[]
                {
                 new SqlParameter("@TaskTypeTechID", taskTypeTechID),
                 new SqlParameter("@CompanyID", companyID)
                });
        }

        public static void DeleteAllTaskTypeTechs(int companyID, int taskTypeID)
        {
            UpdateData("sp_DeleteAllTaskTypeTechs", new SqlParameter[] { 
                new SqlParameter("@DId", companyID),
                new SqlParameter("@TaskTypeID", taskTypeID) });
        }

        public static DataRow DefaultLogsCount(int companyID)
        {
            return SelectRecord("sp_SelectDefaultTimeLogsCount", new SqlParameter[] { new SqlParameter("@DepartmentID", companyID) });
        }

        public static void UpdateDefaultTimeLogs(int companyID, int selectedTaskType)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@DepartmentID", companyID);
            _params[1] = new SqlParameter("@TaskTypeID", selectedTaskType);            
            UpdateData("sp_UpdateDefaultTimeLogs", _params);
        }

        // --- TaskType Tech relation with Costs
        public static DataTable SelectTaskTypeTechCosts(int companyID, int techID)
        {
            return SelectRecords("sp_SelectTaskTypeTechCosts",
                new SqlParameter[] { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@TechID", techID)
                });
        }

        public static void InsertTaskTypeCost(int companyID, int taskTypeID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TaskTypeID", taskTypeID);
            _params[2] = new SqlParameter("@TechID", techID);
            UpdateData("sp_InsertTaskTypeTechCost", _params);
        }

        public static void DeleteTaskTypeCost(int companyID, int taskTypeID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TaskTypeID", taskTypeID);
            _params[2] = new SqlParameter("@TechID", techID);
            UpdateData("sp_DeleteTaskTypeTechCost", _params);
        }

        public static void UpdateTaskTypeTechCost(int companyID, int techID, int taskTypeID, decimal newCost)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@TaskTypeID", taskTypeID);
            _params[2] = new SqlParameter("@TechID", techID);
            _params[3] = new SqlParameter("@NewCost", newCost);
            UpdateData("sp_UpdateTaskTypeTechCost", _params);
        }

        public static DataTable SelectTaskTypeOverridenCosts(int companyID, int taskTypeId)
        {
            return SelectRecords("sp_SelectTaskTypeOverriddenCosts",
                new SqlParameter[] { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@TaskTypeId", taskTypeId)
                });
        }

        public static void DeleteTaskTypeTechBillableRate(int taskTypeTechBillableRateID, int companyID)
        {
            UpdateData("sp_DeleteTaskTypeTechBillableRate", new SqlParameter[]
            {
                 new SqlParameter("@TaskTypeTechBillableRateID", taskTypeTechBillableRateID),
                 new SqlParameter("@CompanyID", companyID),
            });
        }

        public static void UpdateTaskTypeTechBillableRate(int departmentID, int taskTypeId, int techId,
            decimal hourlyRate)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@DepartmentID", departmentID);
            _params[1] = new SqlParameter("@TaskTypeId", taskTypeId);
            _params[2] = new SqlParameter("@TechId", techId);
            _params[3] = new SqlParameter("@HourlyRate", hourlyRate);
            UpdateData("sp_UpdateTaskTypeTechBillableRate", _params);
        }

        public static DataTable SelectTaskTypeTechBillableRates(int companyID, int techID)
        {
            return SelectRecords("sp_SelectTaskTypeTechBillableRates",
                new SqlParameter[] { 
                    new SqlParameter("@DId", companyID),
                    new SqlParameter("@TechID", techID)
                });
        }
    }
}