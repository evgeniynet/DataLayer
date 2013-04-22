using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for CustomTexts.
	/// </summary>
	public class CustomTexts: DBAccess
	{
		public static string GetCustomText(int DeptID, string Alias)
		{
			return GetCustomText(Guid.Empty, DeptID, Alias);
		}

		public static string GetCustomText(Guid OrgID, int DeptID, string Alias)
		{
			string result="";
			DataRow _custom_text=SelectRecord("sp_SelectCustomText", new SqlParameter[]{new SqlParameter("@Did", DeptID), new SqlParameter("@Type", Alias)}, OrgID);
			if (_custom_text!=null)
			{
				if (!_custom_text.IsNull("txtText"))
				{
					result=(string)_custom_text["txtText"];					
				};
			};

			return result;
		}

		public static DataTable SelectCustomTexts(int DepartmentId)
		{
			return SelectRecords("sp_SelectCustomTexts", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
		}

		public static void UpdateCustomText(int DepartmentId, string AliasName, string Text)
		{
			UpdateData("sp_UpdateCustomText", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),
				new SqlParameter("@Type", AliasName),
				new SqlParameter("@txtText", Text)
			});
		}

		public static void DeleteCustomText(int DepartmentId, string AliasName)
		{
			UpdateData("sp_DeleteCustomText", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),
				new SqlParameter("@Type", AliasName)
			});
		}
	}
}
