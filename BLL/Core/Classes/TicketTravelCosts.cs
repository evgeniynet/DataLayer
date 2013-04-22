using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class TicketTravelCosts : DBAccess
    {
        public TicketTravelCosts()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static DataTable SelectAll(int DepartmentId, int TicketId)
        {
            return SelectRecords("sp_SelectTicketTravelCosts", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@TicketId", TicketId) });
        }

        public static void Insert(int DepartmentId, int TicketId, string StartLocation, string EndLocation,
            int Distance, decimal DistanceRate, DateTime date)
        {
            Insert(DepartmentId, TicketId, StartLocation, EndLocation, Distance, DistanceRate, date, "");
        }

        public static void Insert(int DepartmentId, int TicketId, string StartLocation, string EndLocation, 
            int Distance, decimal DistanceRate, DateTime date, string note)
        {
            UpdateData("sp_InsertTicketTravelCosts", new SqlParameter[]{
                new SqlParameter("@DepartmentId", DepartmentId),
                new SqlParameter("@TicketId", TicketId),
                new SqlParameter("@StartLocation", StartLocation),
                new SqlParameter("@EndLocation", EndLocation),
                new SqlParameter("@Distance", Distance),
                new SqlParameter("@DistanceRate", DistanceRate),
                new SqlParameter("@Date", date),
                new SqlParameter("@Note", note)
            });
        }

        public static DataTable Select(int DepartmentId, int TicketId, int ticketTravelCostID)
        {
            return SelectRecords("sp_SelectTicketTravelCost", new SqlParameter[]
            {
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@TicketId", TicketId),
                new SqlParameter("@TicketTravelCostID", ticketTravelCostID)
            });
        }

        public static void Delete(int DepartmentId, int TicketId, int ticketTravelCostID)
        {
            UpdateData("sp_DeleteTicketTravelCost", new SqlParameter[]
            {
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@TicketId", TicketId),
                new SqlParameter("@TicketTravelCostID", ticketTravelCostID)
            });
        }

        public static void Update(int DepartmentId, int TicketId, string StartLocation, string EndLocation,
            int Distance, decimal DistanceRate, DateTime date, int ticketTravelCostID)
        {
            Update(DepartmentId, TicketId, StartLocation, EndLocation, Distance, DistanceRate, date, ticketTravelCostID, "");
        }

        public static void Update(int DepartmentId, int TicketId, string StartLocation, string EndLocation,
            int Distance, decimal DistanceRate, DateTime date, int ticketTravelCostID, string note)
        {
            UpdateData("sp_UpdateTicketTravelCosts", new SqlParameter[]{
                new SqlParameter("@DepartmentId", DepartmentId),
                new SqlParameter("@TicketId", TicketId),
                new SqlParameter("@StartLocation", StartLocation),
                new SqlParameter("@EndLocation", EndLocation),
                new SqlParameter("@Distance", Distance),
                new SqlParameter("@DistanceRate", DistanceRate),
                new SqlParameter("@Date", date),
                new SqlParameter("@TicketTravelCostID", ticketTravelCostID),
                new SqlParameter("@Note", note)
            });
        }
    }
}
