
using System;
using System.Data.SqlClient;

namespace BWA.bigWebDesk.BLL
{
	public class ClassTechnicianJctn : BWA.bigWebDesk.DAL.tbl_ClassTechnicianJctn
	{		
		public ClassTechnicianJctn(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static void AddClassTechnician(Guid organizationId, int classID,
                                              int loginCompJuncID)
        {
            try
            {
                ClassTechnicianJctn ct = new ClassTechnicianJctn(organizationId);
                ct.AddNew();
                ct.Class_id = classID;
                ct.LoginCompanyJunc_id = loginCompJuncID;
                ct.Save();
            }
            catch (SqlException ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new ApplicationException(ex.Message, ex);
            }
        }
	}
}
