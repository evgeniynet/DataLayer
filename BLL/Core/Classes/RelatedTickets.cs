using System;
using System.Web;
using System.Data;
using System.Collections;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class RelatedTickets : DBAccess
    {
        public enum TicketRelationType
        {
            None = -1,
            MasterTickets = 0,
            SubTickets = 1,
            RelatedTickets = 2
        }

        public static void Delete(int DeptID, int TicketID, int RelatedTicketID)
        {
            UpdateData("sp_DeleteRelatedTicket", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TicketID), new SqlParameter("@RelatedTicketId", RelatedTicketID) });
        }

        public static int Insert(int DeptID, int TicketID, int RelatedTicketID, TicketRelationType RelationType)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_InsertRelatedTicket", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TicketID), new SqlParameter("@RelatedTicketId", RelatedTicketID), new SqlParameter("@RelationType", (byte)RelationType)});
            if (_pRVAL.Value != DBNull.Value) return (int)_pRVAL.Value;
            else return 0;
        }
    }
}
