
using System;
using System.Data.SqlClient;

namespace BWA.bigWebDesk.BLL
{
	public class ResolutionCats : BWA.bigWebDesk.DAL.ResolutionCats
	{
        public ResolutionCats(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static void AddCategory(Guid organizationId, int deptID,
                                       int loginCompJuncID,
                                       string name,
                                       bool resolved)
        {
            try
            {
                ResolutionCats cat = new ResolutionCats(organizationId);
                cat.AddNew();
                cat.DId = deptID;
                cat.VchName = name;
                cat.IntCreated = loginCompJuncID;
                cat.DtCreated = DateTime.UtcNow;
                cat.BtInactive = false;
                cat.BtResolved = resolved;
                cat.Save();
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
