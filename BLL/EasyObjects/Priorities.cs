using System;
using System.Data;
using System.Data.Common;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;

namespace BWA.bigWebDesk.BLL
{
	public class Priorities : BWA.bigWebDesk.DAL.Priorities
	{
        public Priorities(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }
		
		public virtual void sp_UpdatePrioritiesSetupDefaults (int DId)
		{
			//  Create the Database object, using the default database service. The
			//  default database service is determined through configuration.
			Database db = GetDatabase();
			
			string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_UpdatePrioritiesSetupDefaults";
			DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);
			
			// Add procedure parameters
			db.AddInParameter(dbCommand, "DId", DbType.Int32, DId);

			base.LoadFromSqlNoExec(dbCommand);
		}
	}
}
