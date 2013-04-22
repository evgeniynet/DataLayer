using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for CreationCategories.
	/// </summary>
	public class SubmissionCategories: DBAccess
	{
		public static DataTable SelectAll()
		{
			return SelectRecords("sp_SelectSubmissionCats", new SqlParameter[]{new SqlParameter("@Id", DBNull.Value)});
		}

        public static DataTable SelectAllSelectable()
        {
            return SelectRecords("sp_SelectSubmissionCats", new SqlParameter[] {new SqlParameter("@Id", DBNull.Value), new SqlParameter("@btSelectable", true)});
        }

		public static DataTable SelectAllChild()
		{
			return SelectByQuery("SELECT Id, ParentId, vchName as Name, bitSelectable FROM SubmissionCategories WHERE NOT ParentId IS NULL ORDER BY vchName");
		}
	}
}
