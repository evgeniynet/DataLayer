
using System;
using System.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;

namespace BWA.bigWebDesk.BLL
{
    public class LoginCompanyJunc : BWA.bigWebDesk.DAL.tbl_LoginCompanyJunc
    {
        public LoginCompanyJunc(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static int GetLoginCompanyID(Guid organizationId, int loginID,
                                            int deptID)
        {
            LoginCompanyJunc lc = new LoginCompanyJunc(organizationId);
            lc.Where.Company_id.Value = deptID;
            lc.Where.Company_id.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lc.Where.Login_id.Value = loginID;
            lc.Where.Login_id.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            lc.Query.Load();
            return (lc.RowCount == 0 ? 0 : lc.Id);
        }

        public static int GetLoginID(Guid organizationId, int ID)
        {
            LoginCompanyJunc lc = new LoginCompanyJunc(organizationId);
            lc.Where.Id.Value = ID;
            lc.Where.Id.Operator = WhereParameter.Operand.Equal;
            lc.Query.Load();
            return (lc.RowCount == 0 ? 0 : lc.Login_id);
        }

        //add data to DB table tbl_LoginCompanyJunc
        public static int AddLoginCompany(Guid organizationId, int loginID,
                                           int deptID,
                                           UserType.BWDUser usrType)
        {
            LoginCompanyJunc lc = new LoginCompanyJunc(organizationId);
            lc.AddNew();
            lc.Login_id = loginID;
            lc.Company_id = deptID;
            lc.UserType_Id = (int)usrType;
            lc.CheckinStatus = false;
            lc.ConfigHourlyRate = 0;
            lc.ConfigPartialSetup = false;
            lc.ConfigEmailNewTicket = true;
            lc.ConfigEmailTicketResponse = true;
            lc.ConfigEmailUserNewTicket = true;
            lc.ConfigEmailUserTicketResponse = true;
            lc.BitAllowQueEmailParsing = false;
            lc.BtCfgCCRep = false;
            lc.BtGlobalFilterEnabled = false;
            lc.BtLimitToAssignedTkts = false;
            lc.BtDisabledReports = false;
            lc.BtUserInactive = false;
            lc.Save();
            return lc.Id;
        }

        public virtual IDataReader LogonOrganizations(int LoginId)
        {
            if (LoginId < 0)
                return null;

            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_SelectLoginChooseOrganization";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddInParameter(dbCommand, "LoginId", DbType.Int32, LoginId);

            return base.LoadFromSqlReader(dbCommand);
        }

    }
}
