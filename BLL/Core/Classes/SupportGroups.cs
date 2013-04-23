using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for SupportGroups.
	/// </summary>
	public class SupportGroups: DBAccess
	{

		public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

		public static DataTable SelectAll(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectSupportGroups", new SqlParameter[]{new SqlParameter("@DId", DeptID)}, OrgId);
		}

        public static DataTable SelectAll(int DeptID, int UserID)
        {
            return GlobalFilters.SetFilter(DeptID, UserID, SelectAll(DeptID), "id", GlobalFilters.FilterType.SupportGroups);
        }

        public static string SelectSupportGroupName(int DeptID, int GroupId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            SqlParameter _pName = new SqlParameter("@vchName", SqlDbType.NVarChar, 50);
            _pName.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectSupportGroup",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@Id", GroupId),
                                            _pName                                                                                                   
                                            });
            return _pName.Value.ToString();
        }

        public static int SelectSupportGroupId(int departmentId, string groupName)
        {
            SqlParameter pId = new SqlParameter("@Id", SqlDbType.Int);
            pId.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectSupportGroupByName",
                        new SqlParameter[] {new SqlParameter("@DId", departmentId),
                                            new SqlParameter("@vchName", groupName),
                                            pId                                                                                                   
                                            });
            return (int)pId.Value;
        }

        public static DataTable SelectNotSupportGroupMembers(int DeptID, int GroupId)
        {
            return SelectRecords("sp_SelectSupportGrpMmbrsDD", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SupGroupId", GroupId) });
        }        
      
        public static DataTable SelectSupportGroupMembers(int DeptID, int GroupId)
        {
            return SelectRecords("sp_SelectSupportGrpMmbrs", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SupGroupId", GroupId) });
        }        

        //-1: Group name is required. Cannot be blank.
        //-2: Group already exists in this department.
        public static int Update(int DeptID, int GroupId, string GroupName)
        {
            return Update(Guid.Empty, DeptID, GroupId, GroupName);
        }

        public static int Update(Guid OrgId, int DeptID, int GroupId, string GroupName)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateSupportGroup",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@Id", GroupId),
                                            new SqlParameter("@vchName", GroupName)                                                                                       
                                            }, OrgId);
            return (int)_pRVAL.Value;
        }

        public static int UpdateSupportGroupMember(int DeptID, int GroupId, int UserId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateSupportGroupMember",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@GroupId", GroupId),
                                            new SqlParameter("@UId", UserId)                                            
                                            });
            return (int)_pRVAL.Value;
        }

        public static int Delete(int DeptID, int GroupId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DeleteSupportGroup", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@Id", GroupId) });
            return (int)_pRVAL.Value;
        }

        public static int DeleteGroupMember(int DeptID, int GroupId, int UserId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DeleteSupportGrpMmbr", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@GroupId", GroupId), new SqlParameter("@UId", UserId) });
            return (int)_pRVAL.Value;
        }
        
        public static void Transfer(int DeptID, int OldGroupId, int NewGroupId)
        {
            UpdateData("sp_TransferSupportGroup",
                        new SqlParameter[] {
                                            new SqlParameter("@DepartmentId", DeptID),
                                            new SqlParameter("@OldSupGroupId", OldGroupId),
                                            new SqlParameter("@NewSupGroupId", NewGroupId)
                                            });
        }
        
    }
}
