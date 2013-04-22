using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Levels.
	/// </summary>
	public class Levels: DBAccess
	{
		public static DataTable SelectAllFull(int DeptID)
		{
			return SelectAllFull(Guid.Empty, DeptID);
		}

		public static DataTable SelectAllFull(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectLevels", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
		}

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

	    public static DataTable SelectAll(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectLevelsLite", new SqlParameter[]{new SqlParameter("@DId", DeptID)}, OrgId);
		}

		public static DataTable SelectEscalationService(int DeptID)
		{
			return SelectRecords("sp_SelectEscalationServiceRep", new SqlParameter[]{new SqlParameter("@DId", DeptID)});
		}

        public static DataTable SelectAll(int DeptID, int UserID)
        {
            return SelectAll(Guid.Empty, DeptID, UserID);
        }

	    public static DataTable SelectAll(Guid OrgId, int DeptID, int UserID)
		{
			return GlobalFilters.SetFilter(OrgId, DeptID, UserID, SelectAll(OrgId, DeptID), "tintLevel", GlobalFilters.FilterType.Levels);
		}

		public static DataRow SelectOne(int DeptID, int tintLevel)
		{
			return SelectRecord("sp_SelectLevelsLite", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@tintLevel", tintLevel) });
		}

		public static int SelectMaxLevel(int DeptID)
		{
			SqlParameter _pMaxLevel = new SqlParameter("@tintMaxLevel", SqlDbType.TinyInt);
			_pMaxLevel.Direction = ParameterDirection.Output;
			UpdateData("sp_SelectLevelMax", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pMaxLevel });
			if (_pMaxLevel.Value != DBNull.Value) return Convert.ToInt32(_pMaxLevel.Value);
			else return 0;
		}

		public static DataRow SelectOneFull(int DeptID, int tintLevel)
		{
			return SelectRecord("sp_SelectLevels", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@tintLevel", tintLevel) });
		}

		public static void Delete(int DeptID, byte tintLevel)
		{
			UpdateData("sp_DeleteLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@tintLevel", tintLevel) });            
		}

		public static void Insert(int DeptID, string LevelName, string Description, int LastResortTectId, int RoutingType)
		{
			Insert(Guid.Empty, DeptID, LevelName, Description, LastResortTectId, RoutingType);
		}

		public static void Insert(Guid OrgId, int DeptID, string LevelName, string Description, int LastResortTectId, int RoutingType)
		{
			UpdateData("sp_InsertTktLevel", new SqlParameter[]{ 
				new SqlParameter("@DId", DeptID), 
				new SqlParameter("@LevelName", LevelName), 
				new SqlParameter("@Description", Description), 
				new SqlParameter("@intLastResort", LastResortTectId), 
				new SqlParameter("@tintRoutingType", RoutingType) }, OrgId);
		}

		public static void Update(int DeptID, byte tintLevel, string LevelName, string Description, int LastResortTectId, int RoutingType)
		{            
			UpdateData("sp_UpdateLevel", new SqlParameter[]{ 
				new SqlParameter("@Mode", 1), // Update mode (hardcoded in SP)
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@tintLevel", tintLevel),
				new SqlParameter("@LName", LevelName), 
				new SqlParameter("@Desc", Description), 
				new SqlParameter("@intLastResortId", LastResortTectId), 
				new SqlParameter("@tintRoutingType", RoutingType) });
		}

		public static void SetDefaultLevel(int DeptID, byte tintLevel)
		{
			SetDefaultLevel(Guid.Empty, DeptID, tintLevel);
		}

		public static void SetDefaultLevel(Guid OrgId, int DeptID, byte tintLevel)
		{
			UpdateData("sp_UpdateLevelDefault", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@tintLevel", tintLevel)}, OrgId);
		}

		public static void UpdateLevelTech(int DeptID, int UserId, int Level)
		{
			SqlParameter _pLevel = new SqlParameter("@tintLevel", DBNull.Value);
			if (Level > 0)
				_pLevel.Value = Level;

			UpdateData("sp_UpdateLevelTech", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@UserId", UserId),
				_pLevel});
		}

	}
}
