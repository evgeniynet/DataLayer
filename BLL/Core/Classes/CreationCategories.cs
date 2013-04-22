using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for CreationCategories.
	/// </summary>
	public class CreationCategories: DBAccess
	{
		public static DataRow Select(int DeptID, int ID)
		{
			return Select(Guid.Empty, DeptID, ID);
		}

		public static DataRow Select(Guid OrgID, int DeptID, int ID)
		{
			return SelectRecord("sp_SelectCategories", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", ID) }, OrgID);
		}

		public static DataTable SelectAll(int DeptID)
		{
			return SelectAll(Guid.Empty, DeptID);
		}

		public static DataTable SelectAll(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectCategories", new SqlParameter[]{new SqlParameter("@DId", DeptID)}, OrgId);
		}

		public static DataTable SelectAllActive(int DeptID)
		{
			return SelectRecords("sp_SelectCategories", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DBNull.Value), new SqlParameter("@btInactive", false)});
		}

		public static DataTable SelectByInactiveStatus(int DeptID, InactiveStatus inactiveStatus)
		{
			SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
			if (inactiveStatus == InactiveStatus.DoesntMatter)
				_pInactive.Value = DBNull.Value;
			else
				_pInactive.Value = inactiveStatus;

			return SelectRecords("sp_SelectCategories", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DBNull.Value), _pInactive });
		}
		
		public static void Insert(int DeptID, int UserID, string resolutionName)
		{
			Insert(Guid.Empty, DeptID, UserID, resolutionName, false);
		}

		public static void Insert(int DeptID, int UserID, string categoryName, bool isInactive)
		{
			Insert(Guid.Empty, DeptID, UserID, categoryName, isInactive);
		}

		public static void Insert(Guid OrgId, int DeptID, int UserID, string categoryName, bool isInactive)
		{
			UpdateData("sp_InsertCategory",
						new SqlParameter[] {
											new SqlParameter("@DepartmentId", DeptID),
											new SqlParameter("@UId", UserID),
											new SqlParameter("@CategoryName", categoryName),
											new SqlParameter("@btInactive", isInactive)
											}, OrgId);
		}

		public static int Update(int DeptID, int CategoryId, int UserID, string categoryName, bool isInactive)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_UpdateCategory",
						new SqlParameter[] {
											new SqlParameter("@DepartmentId", DeptID),
											new SqlParameter("@CategoryId", CategoryId),
											new SqlParameter("@UId", UserID),
											new SqlParameter("@CategoryName", categoryName),
											new SqlParameter("@btInactive", isInactive),
											new SqlParameter("@CheckOnlyOpen", false),
											pReturnValue
											});
			return (int)pReturnValue.Value;
		}

		public static void Transfer(int DeptID, int fromCategoryId, int toCategoryId, bool transferAll)
		{
			UpdateData("sp_TransferCategory",
						new SqlParameter[] {
											new SqlParameter("@DepartmentId", DeptID),
											new SqlParameter("@OldCategoryId", fromCategoryId),
											new SqlParameter("@NewCategoryId", toCategoryId),
											new SqlParameter("@btTransferAll", transferAll)
											});
		}

		public static int Inactivate(int DeptID, int CategoryId, int UserID, bool checkOnlyOpen)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_UpdateCategory",
						new SqlParameter[] {
											new SqlParameter("@DepartmentId", DeptID),
											new SqlParameter("@CategoryId", CategoryId),
											new SqlParameter("@UId", UserID),
											new SqlParameter("@CategoryName", DBNull.Value),
											new SqlParameter("@btInactive", true),
											new SqlParameter("@CheckOnlyOpen", checkOnlyOpen),
											pReturnValue
											});
			return (int)pReturnValue.Value;
		}

	}
}
