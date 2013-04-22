using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Classes.
	/// </summary>
	public class Classes: DBAccess
	{
		public static DataRow SelectOne(int DeptID, int ClassId)
		{
			return SelectOne(Guid.Empty, DeptID, ClassId);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID, int ClassId)
		{
			return SelectRecord("sp_SelectClass", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@ClassID", ClassId) }, OrgID);
		}

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }
        
        public static DataTable SelectAll(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectClassList", new SqlParameter[]{new SqlParameter("@DId", DeptID)}, OrgId);
		}

		public static DataTable SelectAll(int DeptID, int UserID)
		{
			return PostFilter(GlobalFilters.SetFilter(DeptID, UserID, SelectAll(DeptID), "id", GlobalFilters.FilterType.Classes),-1);
		}

        public static DataTable SelectAllParent(int DeptID, int ClassId)
        {
            return SelectAllParent(Guid.Empty, DeptID, ClassId);
        }

	    public static DataTable SelectAllParent(Guid OrgId, int DeptID, int ClassId)
		{
            return SelectByQuery("SELECT C.*, PC.Level, PC.IsLastChild FROM dbo.fxGetAllParentClasses(" + DeptID.ToString() + "," + ClassId.ToString() + ") PC INNER JOIN tbl_class C ON C.company_id = " + DeptID.ToString() + " AND C.id=PC.Id ORDER BY Level", OrgId);
		}

        public static DataTable SelectByInactiveStatus(int DeptID, InactiveStatus inactiveStatus)
        {
            return SelectByInactiveStatus(Guid.Empty ,DeptID, inactiveStatus, -1);
        }

		public static DataTable SelectByInactiveStatus(Guid OrgId, int DeptID, InactiveStatus inactiveStatus)
		{
            return SelectByInactiveStatus(OrgId, DeptID, inactiveStatus, -1);
		}

        public static DataTable SelectByInactiveStatus(int DeptID, InactiveStatus inactiveStatus, int ParentId)
        {
            return SelectByInactiveStatus(Guid.Empty, DeptID, inactiveStatus, ParentId);
        }

	    public static DataTable SelectByInactiveStatus(Guid OrgId, int DeptID, InactiveStatus inactiveStatus, int ParentId)
		{
			SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
			if (inactiveStatus == InactiveStatus.DoesntMatter)
				_pInactive.Value = DBNull.Value;
			else
				_pInactive.Value = inactiveStatus;
			SqlParameter _pParentId = new SqlParameter("@ParentId", SqlDbType.Int);
			if (ParentId == -1) _pParentId.Value = DBNull.Value;
			else _pParentId.Value = ParentId;
            return SelectRecords("sp_SelectClassList", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pInactive, _pParentId }, OrgId);
		}

		private static DataTable PostFilter(DataTable dt, int ParentId)
		{
			string _filter = dt.DefaultView.RowFilter.ToLower();
			if (_filter.Length == 0 || _filter.Contains("null")) return dt;
			if (dt.Rows.Count==0) return dt;
			int _did=(int)dt.Rows[0]["company_id"];
			dt.DefaultView.RowFilter = string.Empty;
			string _filter0=_filter;
			_filter = _filter.Replace("id in (", string.Empty).Replace(")", string.Empty).Replace(", ",",");
			string[] _arr = _filter.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			_filter = "," + _filter + ",";
			if (ParentId < 0)
			{
				foreach (DataRow _row in dt.Rows)
				{
					if (!_row.IsNull("ParentId") && _filter.Contains("," + _row["ParentId"].ToString() + ",") && !_filter.Contains("," + _row["Id"].ToString() + ",")) _filter += _row["Id"].ToString() + ",";
				} 
				foreach (string _str in _arr)
				{
					string _pid = string.Empty;
					dt.DefaultView.RowFilter = "Id=" + _str;
					if (dt.DefaultView.Count > 0) _pid = dt.DefaultView[0]["ParentId"].ToString();
					while (_pid.Length > 0)
					{
						if (!_filter.Contains("," + _pid + ",")) _filter += _pid + ",";
						dt.DefaultView.RowFilter = "Id=" + _pid;
						if (dt.DefaultView.Count > 0) _pid = dt.DefaultView[0]["ParentId"].ToString();
						else _pid = string.Empty;
					}
				}
			}
			else
			{
				DataTable _dtAll = SelectAll(_did);
				foreach (string _str in _arr)
				{
					string _pid = string.Empty;
					dt.DefaultView.RowFilter = "Id=" + _str;
					if (dt.DefaultView.Count > 0) _pid = dt.DefaultView[0]["ParentId"].ToString();
					else
					{
						_dtAll.DefaultView.RowFilter = "Id=" + _str;
						if (_dtAll.DefaultView.Count > 0) _pid = _dtAll.DefaultView[0]["ParentId"].ToString();
					}
					while (_pid.Length > 0)
					{
						if (!_filter.Contains("," + _pid + ",")) _filter += _pid + ",";
						dt.DefaultView.RowFilter = "Id=" + _pid;
						if (dt.DefaultView.Count > 0) _pid = dt.DefaultView[0]["ParentId"].ToString();
						else
						{
							_dtAll.DefaultView.RowFilter = "Id=" + _pid;
							if (_dtAll.DefaultView.Count > 0) _pid = _dtAll.DefaultView[0]["ParentId"].ToString();
							else _pid = string.Empty;
						}
					}
				}
			}
			if (ParentId > 0)
			{
				DataTable _dt1 = SelectAllParent(_did, (int)dt.Rows[0]["id"]);
				_dt1.DefaultView.RowFilter = _filter0;
				if (_dt1.DefaultView.Count > 0)
				{
					foreach (DataRow _row in dt.Rows)
					{
						if (!_filter.Contains("," + _row["Id"].ToString() + ",")) _filter += _row["Id"].ToString() + ",";
					}
				}
			}
			_filter = "id IN (" + _filter.Trim(',') + ")";
			dt.DefaultView.RowFilter = _filter;
			return dt;
		}

        public static DataTable SelectByInactiveStatus(int DeptID, int UserID, InactiveStatus inactiveStatus)
        {
            return SelectByInactiveStatus(Guid.Empty, DeptID, UserID, inactiveStatus, -1);
        }

		public static DataTable SelectByInactiveStatus(Guid OrgId, int DeptID, int UserID, InactiveStatus inactiveStatus)
		{
            return SelectByInactiveStatus(OrgId, DeptID, UserID, inactiveStatus, -1);
		}

        public static DataTable SelectByInactiveStatus(int DeptID, int UserID, InactiveStatus inactiveStatus, int ParentId)
        {
            return SelectByInactiveStatus(Guid.Empty, DeptID, UserID, inactiveStatus, ParentId);
        }

	    public static DataTable SelectByInactiveStatus(Guid OrgId, int DeptID, int UserID, InactiveStatus inactiveStatus, int ParentId)
		{
            return PostFilter(GlobalFilters.SetFilter(OrgId, DeptID, UserID, SelectByInactiveStatus(OrgId, DeptID, inactiveStatus, ParentId), "id", GlobalFilters.FilterType.Classes), ParentId);
		}

		public static DataTable SelectAllActive(Guid OrgId, int DeptID)
		{
			return SelectRecords("sp_SelectClassList", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@btInactive", false)}, OrgId);
		}
        public static DataTable SelectAllActive(int DeptID)
        {
            return SelectAllActive(Guid.Empty, DeptID);
        }

		public static DataTable SelectAllActive(Guid OrgId, int DeptID, int UserID)
		{
			return PostFilter(GlobalFilters.SetFilter(DeptID, UserID, SelectAllActive(OrgId, DeptID), "id", GlobalFilters.FilterType.Classes), -1);
		}

        public static DataTable SelectAllActive(int DeptID, int UserID)
        {
            return SelectAllActive(Guid.Empty, DeptID, UserID);
        }
        public static DataTable SelectAllActiveForUsers(int DeptID)
        {
            return SelectAllActiveForUsers(Guid.Empty, DeptID);
        }

	    public static DataTable SelectAllActiveForUsers(Guid OrgId, int DeptID)
		{
            return SelectAllActiveForUsers(OrgId, DeptID, -1);
		}

        public static DataTable SelectAllActiveForUsers(int DeptID, int ParentId)
        {
            return SelectAllActiveForUsers(Guid.Empty, DeptID, ParentId);
        }

	    public static DataTable SelectAllActiveForUsers(Guid OrgId, int DeptID, int ParentId)
		{
			SqlParameter _pParentId = new SqlParameter("@ParentId", SqlDbType.Int);
			if (ParentId < 0) _pParentId.Value = DBNull.Value;
			else _pParentId.Value = ParentId;
            return SelectRecords("sp_SelectClassList", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@btInactive", false), _pParentId, new SqlParameter("@bitRestrictToTechs", false) }, OrgId);
		}

		public static DataTable SelectClassLevels(int DeptID, int ClassId)
		{
			return SelectRecords("sp_SelectClassLevels", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@ClassId", ClassId) });
		}

		public static DataTable SelectClassAssignedTechs(int DeptID, int ClassId)
		{
			return SelectRecords("sp_SelectClassAssignedTechs", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@ClassId", ClassId) });
		}

		public static DataTable SelectClassLevelAssignedTechs(int DeptID, int ClassId, int ClassLevelId)
		{
			return SelectRecords("sp_SelectClassAssignedLvlTechs", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@intClassId", ClassId), new SqlParameter("@intClssLvlId", ClassLevelId) });
		}

		public static void InsertClassLevel(int DeptID, int ClassId, int Level)
		{
			UpdateData("sp_InsertClassLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@ClassId", ClassId), new SqlParameter("@tintLevel", Level) });
		}

		public static void UpdateClassLevel(int DeptID, int ClassId, int ClassLevelId, int LastResortTechId, int ClassType, int RoutingType)
		{
			UpdateData("sp_UpdateClassLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@intClassId", ClassId), new SqlParameter("@intClassLvlId", ClassLevelId), new SqlParameter("@intLastResortId", LastResortTechId), new SqlParameter("@tintRoutingType", RoutingType), new SqlParameter("@tintClassType", ClassType) });
		}

		public static void DeleteClassLevel(int DeptID, int ClassId, int ClassLevelId)
		{
			UpdateData("sp_DeleteClassLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@intClassId", ClassId), new SqlParameter("@intClassLvlId", ClassLevelId) });
		}

		public static void DeleteClassLevelTech(int DeptID, int ClassId, int ClassLevelTechId)
		{
			UpdateData("sp_DeleteClssLvlTech", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@intClassId", ClassId), new SqlParameter("@intClssLvlTechId", ClassLevelTechId) });
		}

		public static void UpdateClassLevelTech(int DeptID, int ClassId, int ClassLevelId, int TechId, int LocationId)
		{
			SqlParameter _pLocationId=new SqlParameter("@LocationId", SqlDbType.Int);
			if (LocationId!=0) _pLocationId.Value=LocationId;
			else _pLocationId.Value=DBNull.Value;
			UpdateData("sp_UpdateClassSubTechs", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@ClassId", ClassId), new SqlParameter("@UserId", TechId), _pLocationId, new SqlParameter("@intClssLvlId", ClassLevelId) });
		}

		public static void DeleteClassSubTech(int ClassId, int ClassSubTechId)
		{
			UpdateData("sp_DeleteClassSubTechs", new SqlParameter[] { new SqlParameter("@ClassId", ClassId), new SqlParameter("@TechJctnId", ClassSubTechId) });
		}

		public static int Insert(int DeptID, int ParentClassId, string ClassName, int LastResortTechId, byte ClassType)
		{
			return Update(DeptID, ParentClassId, ClassName, LastResortTechId, ClassType, 1, -1, false, null, false, 0, 0, false, true, "", -1);
		}

        public static int Update(int DeptID, int ParentClassId, string ClassName, int LastResortTechId, int ClassType, int ConfigDistributedRouting, int ClassId, bool RestrictToTechs, string Description, bool AllowEmailParsing, int PriorityId, int LevelOverride, bool IsInactive,
            bool kbPortal, string kbPortalAlias)
        {
            return Update(Guid.Empty, DeptID, ParentClassId, ClassName, LastResortTechId, ClassType, ConfigDistributedRouting, ClassId, RestrictToTechs, Description, AllowEmailParsing, PriorityId, LevelOverride, IsInactive, kbPortal, kbPortalAlias, -1);
        }

        public static int Update(int DeptID, int ParentClassId, string ClassName, int LastResortTechId, int ClassType, int ConfigDistributedRouting, int ClassId, bool RestrictToTechs, string Description, bool AllowEmailParsing, int PriorityId, int LevelOverride, bool IsInactive,
            bool kbPortal, string kbPortalAlias, int kbPortalOrder)
        {
            return Update(Guid.Empty, DeptID, ParentClassId, ClassName, LastResortTechId, ClassType, ConfigDistributedRouting, ClassId, RestrictToTechs, Description, AllowEmailParsing, PriorityId, LevelOverride, IsInactive, kbPortal, kbPortalAlias, kbPortalOrder);
        }

        public static int Update(Guid OrgId, int DeptID, int ParentClassId, string ClassName, int LastResortTechId, int ClassType, int ConfigDistributedRouting, int ClassId, bool RestrictToTechs, string Description, bool AllowEmailParsing, int PriorityId, int LevelOverride, bool IsInactive, bool kbPortal, string kbPortalAlias, int kbPortalOrder)
		{
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

			SqlParameter pPriorityId = new SqlParameter("@PriorityId", SqlDbType.Int);
			if (PriorityId == 0)
				pPriorityId.Value = DBNull.Value;
			else
				pPriorityId.Value = PriorityId;

			

			SqlParameter pLevelOverride = new SqlParameter("@tintLevelOverride", SqlDbType.TinyInt);
			if (LevelOverride == 0)
				pLevelOverride.Value = DBNull.Value;
			else
				pLevelOverride.Value = LevelOverride;

			SqlParameter pParentId = new SqlParameter("@ParentId", SqlDbType.Int);
			if (ParentClassId == 0) pParentId.Value = DBNull.Value;
			else pParentId.Value = ParentClassId;

            SqlParameter pKBPortalOrder = new SqlParameter("@KBPortalOrder", SqlDbType.TinyInt);
            if (kbPortalOrder < 0) pKBPortalOrder.Value = DBNull.Value;
            else pKBPortalOrder.Value = kbPortalOrder;

			UpdateData("sp_UpdateClass",
						new SqlParameter[] {_pRVAL,
											new SqlParameter("@DId", DeptID),
											pParentId,
											new SqlParameter("@ClassName", ClassName),
											new SqlParameter("@LastResortTechId", LastResortTechId),
											new SqlParameter("@tintClassType", ClassType),
											new SqlParameter("@ConfigDistributedRouting", ConfigDistributedRouting),
											new SqlParameter("@ClassId", ClassId),
											new SqlParameter("@bitRestrictToTechs", RestrictToTechs),
											new SqlParameter("@txtDesc", Description),
											new SqlParameter("@bitAllowEmailParsing", AllowEmailParsing),
											pPriorityId,
											pLevelOverride,
											new SqlParameter("@btInactive", IsInactive),
											new SqlParameter("@KBPortal", kbPortal),
											new SqlParameter("@KBPortalAlias", kbPortalAlias),
                                            pKBPortalOrder
											}, OrgId);
            return (int)_pRVAL.Value;
		}

		public static int Delete(int DeptID, int ClassId)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_DeleteClass",
						new SqlParameter[] {
											new SqlParameter("@DId", DeptID),
											new SqlParameter("@ClassID", ClassId),
											pReturnValue
											});

			return (int)pReturnValue.Value;
		}
		public static int Transfer(int DeptID, int fromClassId, int toClassId)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_TransferClass",
						new SqlParameter[] {
											new SqlParameter("@DId", DeptID),
											new SqlParameter("@OldClassId", fromClassId),
											new SqlParameter("@NewClassId", toClassId),
											pReturnValue
											});

			return (int)pReturnValue.Value;
		}
	}
}
