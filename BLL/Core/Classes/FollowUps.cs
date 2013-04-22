using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for FollowUps
    /// </summary>
    public class FollowUps : DBAccess
    {
        public enum ListType : int
        {
            FollowUpList = 1,
            ScheduledDate = 2,
            SLAResponse = 3,
            SLACompletion = 4,
            ConfirmationsAsTechAll = 5,
            ConfirmationsAsTechConfirmed = 6,
            ConfirmationsAsTechUnConfirmed = 7,
            ConfirmationsAsUserUnConfirmed = 8,
            NextSteps = 9
        }

        public FollowUps(){}

        public class Filter
        {
            public Filter() { }

            ListType listType;
            int days;
            int departmentId;
            int technicianId;
            int accountId;

            public Filter(ListType ListType, int DepartmentId, int Days, int TechnicianId, int AccountId)
            {
                this.accountId = AccountId;
                this.days = Days;
                this.departmentId = DepartmentId;
                this.listType = ListType;
                this.technicianId = TechnicianId;
            }

            public DataTable FilteredItems()
            {
                return Select(listType, departmentId, days, technicianId, accountId);
            }
        }

        public static DataTable Select(ListType ListType, int DepartmentId, int Days, int TechnicianId, int AccountId)
        {
            System.Data.SqlClient.SqlParameter[] sqlParams = {
                new System.Data.SqlClient.SqlParameter("@DId", DBNull.Value), 
                new System.Data.SqlClient.SqlParameter("@UId", "-1"),
                new System.Data.SqlClient.SqlParameter("@intAcctId", DBNull.Value),
                new System.Data.SqlClient.SqlParameter("@sintDayAdvance", DBNull.Value),
                new System.Data.SqlClient.SqlParameter("@tintListType", (int)ListType)
            };

            if (DepartmentId > 0) sqlParams[0].SqlValue = DepartmentId;
            if (TechnicianId > 0) sqlParams[1].SqlValue = TechnicianId;
            if (AccountId > 0) sqlParams[2].SqlValue = AccountId;
            if (Days > 0) sqlParams[3].SqlValue = Days;

            return SelectRecords("sp_SelectFollowUpList", sqlParams);
        }

        public static DataTable Select(ListType ListType, int DepartmentId, int Days)
        {
            return Select(ListType, DepartmentId, Days, -1, -1);
        }

        
    }
}
