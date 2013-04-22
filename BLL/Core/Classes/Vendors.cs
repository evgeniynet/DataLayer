using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Priorities.
	/// </summary>
	public class Vendors: DBAccess
	{
		public static DataTable SelectAll(int DeptID)
		{
            return SelectRecords("sp_SelectVendor", new SqlParameter[] { new SqlParameter("@companyId", DeptID), new SqlParameter("@code", 1)});
		}

        public static DataRow SelectOne(int DeptID, int VendorID)
        {
            return SelectRecord("sp_SelectVendor", new SqlParameter[] { new SqlParameter("@companyId", DeptID), new SqlParameter("@code", 2), new SqlParameter("@vendorId", VendorID) });
        }

        public static void Delete(int DepartmentId, int VendorId)
        {
            UpdateData("sp_DeleteVendor", new SqlParameter[] { new SqlParameter("@CompanyId", DepartmentId), new SqlParameter("@VendorId", VendorId) });
        }

        
        public static int Update(int DepartmentId, int VendorId, string name, string phone, string fax, string accountNumber, string notes)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateVendor", new SqlParameter[] {
                new SqlParameter("@DId", DepartmentId), 
                new SqlParameter("@VendorId", VendorId),
                new SqlParameter("@Name", name),
                new SqlParameter("@Phone", phone),
                new SqlParameter("@Fax", fax),
                new SqlParameter("@AccountNumber", accountNumber),
                new SqlParameter("@Notes", notes),
                pReturnValue
            });
            
            return (int)pReturnValue.Value;
        }

        public static int Insert(int DepartmentId, string name, string phone, string fax, string accountNumber, string notes)
        {
            return Update(DepartmentId, 0, name, phone, fax, accountNumber, notes);
        }
	}
}
