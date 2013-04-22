using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Priorities.
	/// </summary>
	public class Priorities: DBAccess
	{
		public enum MoveDirection : int
		{ 
			Up = 1,
			Down = 0
		}

		public static DataTable SelectAll(int DeptID)
		{
			return SelectRecords("sp_SelectPrioritiesLite", new SqlParameter[]{new SqlParameter("@DId", DeptID)});
		}

		public static DataTable SelectAllFull(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectPriorities", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
		}

		public static DataTable SelectAllFull(int DeptID)
		{
			return SelectAllFull(Guid.Empty, DeptID);
		}

		public static void SetupDefaultPriorities(int DeptID)
		{
			UpdateData("sp_UpdatePrioritiesSetupDefaults",new SqlParameter("@DId", DeptID));
		}

		public static void SetDefaultPriority(int DeptID, int priorityId)
		{
			UpdateData("sp_UpdatePriorityDefault", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Default", priorityId)});
		}

		public static DataRow SelectOneFull(int DeptID, int priorityId)
		{
			return SelectRecord("sp_SelectPriority", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", priorityId) });
		}

		public static int Insert(int DeptID, string name, string description, bool restrictForUsers,
								  byte SLAPercentage, byte SLADays, byte SLAHours, byte SLAMinutes,
								  bool SLASkipSat, bool SLASkipSun, bool SLASkipHolidays, bool SLACountBusinnesHours,
								  byte SLARPercentage, byte SLARDays, byte SLARHours, byte SLARMinutes,
								  bool SLARSkipSat, bool SLARSkipSun, bool SLARSkipHolidays, bool SLARCountBusinnesHours)
		{
			return Update(DeptID, -1, name, description, restrictForUsers,
								   SLAPercentage, SLADays, SLAHours, SLAMinutes,
								  SLASkipSat, SLASkipSun, SLASkipHolidays, SLACountBusinnesHours,
								  SLARPercentage, SLARDays, SLARHours, SLARMinutes,
								  SLARSkipSat, SLARSkipSun, SLARSkipHolidays, SLARCountBusinnesHours);
		}

		public static int Update(int DeptID, int priorityId, string name, string description, bool restrictForUsers,
								  byte SLAPercentage, byte SLADays, byte SLAHours, byte SLAMinutes,
								  bool SLASkipSat, bool SLASkipSun, bool SLASkipHolidays, bool SLACountBusinnesHours,
								  byte SLARPercentage, byte SLARDays, byte SLARHours, byte SLARMinutes,
								  bool SLARSkipSat, bool SLARSkipSun, bool SLARSkipHolidays, bool SLARCountBusinnesHours)
		{
            return Update(Guid.Empty, DeptID, priorityId, name, description, restrictForUsers, SLAPercentage, SLADays, SLAHours, SLAMinutes, SLASkipSat, SLASkipSun, SLASkipHolidays, SLACountBusinnesHours, SLARPercentage, SLARDays, SLARHours, SLARMinutes, SLARSkipSat, SLARSkipSun, SLARSkipHolidays, SLARCountBusinnesHours);
		}

		public static int Update(Guid OrgId, int DeptID, int priorityId, string name, string description, bool restrictForUsers,
								  byte SLAPercentage, byte SLADays, byte SLAHours, byte SLAMinutes,
								  bool SLASkipSat, bool SLASkipSun, bool SLASkipHolidays, bool SLACountBusinnesHours,
								  byte SLARPercentage, byte SLARDays, byte SLARHours, byte SLARMinutes,
								  bool SLARSkipSat, bool SLARSkipSun, bool SLARSkipHolidays, bool SLARCountBusinnesHours)
		{
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdatePriority", new SqlParameter[]{_pRVAL,
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Id", priorityId),
				new SqlParameter("@Name", name),
				new SqlParameter("@Description", description),
				new SqlParameter("@btRstrctUsr", restrictForUsers),
				new SqlParameter("@SLAPercentage", SLAPercentage),
				new SqlParameter("@SLADays", SLADays),
				new SqlParameter("@SLAHours", SLAHours),
				new SqlParameter("@SLAMinutes", SLAMinutes),
				new SqlParameter("@btSkipSaturday", SLASkipSat),
				new SqlParameter("@btSkipSunday", SLASkipSun),
				new SqlParameter("@btSkipHolidays", SLASkipHolidays),
				new SqlParameter("@btUseBusHours", SLACountBusinnesHours),
				new SqlParameter("@SLAResponsePercentage", SLARPercentage),
				new SqlParameter("@SLAResponseDays", SLARDays),
				new SqlParameter("@SLAResponseHours", SLARHours),
				new SqlParameter("@SLAResponseMinutes", SLARMinutes),
				new SqlParameter("@btResponseSkipSaturday", SLARSkipSat),
				new SqlParameter("@btResponseSkipSunday", SLARSkipSun),
				new SqlParameter("@btResponseSkipHolidays", SLARSkipHolidays),
				new SqlParameter("@btResponseUseBusHours", SLARCountBusinnesHours)
			}, OrgId);
            return (int)_pRVAL.Value;
		}

		public static int Delete(int DeptID, int priorityId)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_DeletePriority", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Id", priorityId),
				pReturnValue
			});
			return (int)pReturnValue.Value;
		}

		public static void ChangeOrder(int DeptID, int priorityId, MoveDirection direction)
		{
			UpdateData("sp_UpdatePriorityOrder", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DeptID),
				new SqlParameter("@PriorityId", priorityId),
				new SqlParameter("@MoveUp", (int)direction)
			});
		}

		public static int TransferPriority(int DeptID, int fromPriorityId, int toPriorityId)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_TransferPriority", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@OldId", fromPriorityId),
				new SqlParameter("@NewId", toPriorityId),
				pReturnValue
			});
			return (int)pReturnValue.Value;
		}
	}
}
