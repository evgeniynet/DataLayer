using System;
using System.IO;
using System.Xml;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class Parts : DBAccess
    {
        public static DataTable SelectToBeOrdered(int DepartmentId)
        {
            return SelectRecords("sp_SelectPartsToBeOrdered", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
        }

        public static DataTable SelectAwaitingArrival(int DepartmentId)
        {
            return SelectRecords("sp_SelectPartsAwaitingArrival", new SqlParameter[] { new SqlParameter("@CompanyId", DepartmentId) });
        }

        public static DataTable SelectStaging(int DepartmentId)
        {
            return SelectByQuery("SELECT * FROM tbl_part WHERE company_id = " + DepartmentId.ToString() + " AND status ='staging'");
        }

        public static void MakePartStaging(int DepartmentId, int PartId, string VendorDescription, string PartNumber, int Qty4Tkt, int Qty2Inv, decimal Cost)
        {
            UpdateData("sp_UpdatePartStaging", new SqlParameter[] { 
                new SqlParameter("@CompanyId", DepartmentId),
                new SqlParameter("@PartId", PartId),
                new SqlParameter("@VendorDescription", VendorDescription),
                new SqlParameter("@PartNumber", PartNumber),
                new SqlParameter("@Qty4Tkt", Qty4Tkt),
                new SqlParameter("@Qty2Inv", Qty2Inv),
                new SqlParameter("@Cost", Cost)
            });
        }

        public static void RemovePartStaging(int DepartmentId, int PartId)
        {
            UpdateData("sp_UpdatePartRemoveStaging", new SqlParameter[] { 
                new SqlParameter("@CompanyId", DepartmentId),
                new SqlParameter("@PartId", PartId)
            });
        }

        public static void RemoveAllStaging(int DepartmentId)
        {
            RemovePartStaging(DepartmentId, -1);
        }

        static void UpdatePartOrder(int DepartmentId, int VendorId, int LocationId, string RepName, string ConfirmationNumber, string PONumber, decimal TotalOrderCost, DateTime ExpectedArrivalDate, int UserId)
        {
            SqlParameter pLocationId = new SqlParameter("@LocationId", SqlDbType.Int);
            if (LocationId > 0)
                pLocationId.Value = LocationId;
            else
                pLocationId.Value = DBNull.Value;

            SqlParameter pExpectedArrivalDate = new SqlParameter("@ExpectedArrivalDate", SqlDbType.SmallDateTime);
            if (ExpectedArrivalDate > DateTime.MinValue)
                pExpectedArrivalDate.Value = ExpectedArrivalDate;
            else
                pExpectedArrivalDate.Value = DBNull.Value;

            UpdateData("sp_UpdatePartOrder", new SqlParameter[] { 
                new SqlParameter("@CompanyId", DepartmentId),
                new SqlParameter("@VendorId", VendorId),
                pLocationId,
                new SqlParameter("@RepName", RepName),
                new SqlParameter("@ConfirmationNumber", ConfirmationNumber),
                new SqlParameter("@PONumber", PONumber),
                new SqlParameter("@TotalOrderCost", TotalOrderCost),
                pExpectedArrivalDate,
                new SqlParameter("@UserId", UserId)
            });
        }

        static DataTable SelectTicketIdsForPartsOrder(int DepartmentId)
        {
            return SelectRecords("sp_SelectPartsOrderDisTickIds", new SqlParameter[] { new SqlParameter("@CompanyId", DepartmentId) });
        }

        public static void CreatePartsOrder(int DepartmentId, int VendorId, int LocationId, string RepName, string ConfirmationNumber, string PONumber, decimal TotalOrderCost, DateTime ExpectedArrivalDate, int UserId, string ticketCustomName)
        {
            string logDescription = "Part(s) for this " + ticketCustomName + " were ordered.";
            foreach (DataRow rTicketId in SelectTicketIdsForPartsOrder(DepartmentId).Rows)
            {
                Ticket.InsertLogMessage(DepartmentId, (int)rTicketId["ticket_id"], UserId, "Parts Ordered", string.Empty, logDescription);
                Data.NotificationRules.RaiseNotificationEvent(DepartmentId, UserId, NotificationRules.TicketEvent.OrderPart, new Ticket(DepartmentId, (int)rTicketId["ticket_id"], true));
            }

            UpdatePartOrder(DepartmentId, VendorId, LocationId, RepName, ConfirmationNumber, PONumber, TotalOrderCost, ExpectedArrivalDate, UserId);
        }
    }
}
