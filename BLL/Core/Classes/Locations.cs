using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Text;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Locations.
	/// </summary>
	public class Locations : DBAccess
	{
		public static int SelectLocationsCount(Guid OrgId, Guid InstId)
		{
			int DId = Companies.SelectDepartmentId(OrgId, InstId);
			DataTable _dt = SelectByQuery("SELECT Count(*) FROM Locations WHERE DId=" + DId.ToString() + " AND Inactive=0", OrgId);
			return (int)_dt.Rows[0][0];
		}

		public static int SelectAuditLocationsCount(Guid OrgId, Guid InstId)
		{
			int DId = Companies.SelectDepartmentId(OrgId, InstId);
			DataTable _dt = SelectByQuery("SELECT Count(*) FROM Locations WHERE DId=" + DId.ToString() + " AND Inactive=0 AND CfgEnableAudit=1", OrgId);
			return (int)_dt.Rows[0][0];
		}

		public static DataTable SelectAuditors(int DeptID)
		{
			return SelectRecords("sp_SelectLocationAuditors", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
		}

		public static DataTable SelectAll(int DeptID)
		{
			return SelectRecords("sp_SelectLocations", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
		}

		public static DataTable SelectTree(int DeptID)
		{
			return SelectTree(DeptID, 0, -1);
		}

		public static DataTable SelectTree(int DeptID, int AccID)
		{
			return SelectTree(DeptID, AccID, -1);
		}

		public static DataTable SelectTree(int DeptID, int AccID, int ParentId)
		{
			return SelectTree(DeptID, AccID, ParentId, string.Empty);
		}

		public static DataTable SelectTree(int DeptID, int AccID, int ParentId, InactiveStatus inactiveStatus)
		{
			return SelectTree(DeptID, AccID, ParentId, string.Empty, inactiveStatus);
		}

		public static DataTable SelectTree(int DeptID, int AccID, int ParentId, string Search)
		{
			return SelectTree(DeptID, AccID, ParentId, Search, InactiveStatus.DoesntMatter);
		}

		public static DataTable SelectFilteredTree(int DeptID, int AccID, string NameFilter, bool ShowInactive)
		{
			return SelectRecords("sp_SelectLocationsFilteredTree", 
				new SqlParameter[] {
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@AccId", AccID),
					new SqlParameter("@NameFilter", NameFilter),
					new SqlParameter("@ShowInactive", ShowInactive)});
		}

		public static DataTable SelectTree(int DeptID, int AccID, int ParentId, string Search, InactiveStatus inactiveStatus)
		{
			SqlParameter _pParentId = new SqlParameter("@ParentId", SqlDbType.Int);
			if (ParentId < 0) _pParentId.Value = DBNull.Value;
			else _pParentId.Value = ParentId;

			SqlParameter pInactiveStatus = new SqlParameter("@Inactive", SqlDbType.Bit);
			if (inactiveStatus == InactiveStatus.DoesntMatter) pInactiveStatus.Value = DBNull.Value;
			else pInactiveStatus.Value = (inactiveStatus == InactiveStatus.Inactive) ? true : false;

			if (Search.Length > 0)
			{
				if (AccID < 0)
					AccID = 0;
				return SelectRecords("sp_SelectLocationsSearch", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pParentId, new SqlParameter("@AccId", AccID), new SqlParameter("@Search", "%" + Search + "%"), pInactiveStatus });
			}
			else
			{
				SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
				_pId.Value = DBNull.Value;
				return SelectRecords("sp_SelectLocationsTree", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pId, new SqlParameter("@AccId", AccID), _pParentId, pInactiveStatus });
			}
		}

		public static DataTable SelectTopLevelLocations(int DepartmentId)
		{
			return SelectTopLevelLocations(DepartmentId, 0);
		}

		public static DataTable SelectTopLevelLocations(int DepartmentId, int AccountId)
		{
			string _sql_query = "select Id, Name from Locations where ParentId IS NULL";
			if (AccountId > 0)
				_sql_query += " AND AccountId = " + AccountId;
			else
				_sql_query += " AND AccountId IS NULL";
			_sql_query += " AND DID=" + DepartmentId;
			return SelectByQuery(_sql_query);
		}

		public static DataTable SelectLocationUsers(int DeptID, int LocationId, string FirstName, string LastName, string EMail)
		{
			string _first_name = Security.SQLInjectionBlock(FirstName.Replace("'", "''"));
			string _last_name = Security.SQLInjectionBlock(LastName.Replace("'", "''"));
			string _email = Security.SQLInjectionBlock(EMail.Replace("'", "''"));

			_first_name = "'" + _first_name + "%'";
			_last_name = "'" + _last_name + "%'";
			_email = "'" + _email + "%'";

			string _sql_query = "SELECT TOP 50 vw.id, vw.vchFullName, vw.vchEmail, ISNULL(vw.intLocationId,0) AS intLocationId, vw.vchLocationName ";
			_sql_query += "FROM vw_Logins vw ";
			_sql_query += "WHERE vw.DId=" + DeptID.ToString() + " AND vw.btUserInactive=0 AND vw.vchLastName LIKE " + _last_name + " AND vw.vchFirstName LIKE " + _first_name + " AND vw.vchEmail LIKE " + _email + " ";

			if (LocationId > 0)
				_sql_query += "AND vw.intLocationId in (select Id from dbo.fxGetAllChildLocations(" + DeptID.ToString() + ", " + LocationId.ToString() + "))";
			else if (LocationId == -1)
				_sql_query += "AND vw.intLocationId is NULL";

			return SelectByQuery(_sql_query);
		}

		public static DataTable SelectLocationUsers(int DeptID, int AccountId, int AccLocationId, string FirstName, string LastName, string EMail)
		{
			string _first_name = Security.SQLInjectionBlock(FirstName.Replace("'", "''"));
			string _last_name = Security.SQLInjectionBlock(LastName.Replace("'", "''"));
			string _email = Security.SQLInjectionBlock(EMail.Replace("'", "''"));

			_first_name = "'" + _first_name + "%'";
			_last_name = "'" + _last_name + "%'";
			_email = "'" + _email + "%'";

			string _sql_query = "SELECT TOP 50 vw.id, vw.vchFullName, vw.vchEmail, vw.vchAccLocationName, ISNULL(vw.intLocationId,0) AS intLocationId, vw.vchLocationName ";
			_sql_query += "FROM vw_Logins vw ";
			_sql_query += "WHERE vw.DId=" + DeptID.ToString() + " AND ISNULL(vw.intAccountId,-1)=" + AccountId.ToString() + " AND vw.btUserInactive=0 AND vw.vchLastName LIKE " + _last_name + " AND vw.vchFirstName LIKE " + _first_name + " AND vw.vchEmail LIKE " + _email + " ";

			if (AccLocationId > 0)
				_sql_query += "AND vw.AccountLocationId in (select Id from dbo.fxGetAllChildLocations(" + DeptID.ToString() + ", " + AccLocationId.ToString() + "))";
			else if (AccLocationId == -1)
				_sql_query += "AND vw.AccountLocationId is NULL";

			return SelectByQuery(_sql_query);
		}

		public static DataTable SelectTreeActive(int DeptID)
		{
			return SelectTreeActive(DeptID, 0);
		}

		public static DataTable SelectTreeActive(int DeptID, int AccID)
		{
			return SelectTreeActive(DeptID, AccID, -1, string.Empty);
		}

		public static DataTable SelectTreeActive(int DeptID, int AccID, int ParentId)
		{
			return SelectTreeActive(DeptID, AccID, ParentId, string.Empty);
		}

		public static DataTable SelectTreeActive(int DeptID, int AccID, int ParentId, string Search)
		{
			return SelectTreeActive( DeptID, AccID, ParentId, Search, Guid.Empty);
		}

		public static DataTable SelectTreeActive(int DeptID, int AccID, int ParentId, string Search, Guid orgId)
		{
			SqlParameter _pParentId = new SqlParameter("@ParentId", SqlDbType.Int);
			if (ParentId < 0) _pParentId.Value = DBNull.Value;
			else _pParentId.Value = ParentId;
			if (!string.IsNullOrEmpty(Search)) return SelectRecords("sp_SelectLocationsSearch", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pParentId, new SqlParameter("@AccId", AccID), new SqlParameter("@Search", Search), new SqlParameter("@Inactive", false) }, orgId);
			else
			{
				SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
				_pId.Value = DBNull.Value;
				return SelectRecords("sp_SelectLocationsTree", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pId, new SqlParameter("@AccId", AccID), _pParentId, new SqlParameter("@Inactive", false) }, orgId);
			}
		}

		public static string GetLocationFullName(int DeptID, int LocationID)
		{
			return GetLocationFullName(DeptID, LocationID, Guid.Empty);
		}

		public static string GetLocationFullName(int DeptID, int LocationID, Guid orgId)
		{
			string _result = "";
			string _sql_query = "";

			_sql_query = "Select dbo.fxGetUserLocationName(" + DeptID.ToString();
			_sql_query += ", " + LocationID.ToString() + ") as Name";


			DataTable _dt = SelectByQuery(_sql_query, orgId);
			if (_dt != null)
			{
				if (_dt.Rows.Count == 1)
				{
					if (_dt.Rows[0]["Name"] != null)
						_result = _dt.Rows[0]["Name"].ToString();
				};
			};

			return _result;
		}

		public static DataTable SelectParentLocations(int DeptID, int LocationID)
		{
			string _sql_query = "";

			_sql_query = "Select Id, dbo.fxGetUserLocationName(" + DeptID.ToString();
			_sql_query += ", Id) as Name from dbo.fxGetAllParentLocations(";
			_sql_query += DeptID.ToString();
			_sql_query += ",";
			_sql_query += LocationID.ToString();
			_sql_query += ")";

			return SelectByQuery(_sql_query);
		}

		public static DataTable SelectParentLocationsWithNames(int DeptID, int LocationID)
		{
			string sqlQuery = string.Format("SELECT * FROM fxGetAllParentLocationsWithNames({0},{1})", DeptID.ToString(), LocationID.ToString());
			return SelectByQuery(sqlQuery);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID)
		{
			return SelectTreeActiveFiltered(DeptID, UserID, -1, -1, string.Empty);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int ParentId)
		{
			return SelectTreeActiveFiltered(DeptID, UserID, -1, ParentId, string.Empty);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int ParentId, string Search)
		{
			return SelectTreeActiveFiltered(DeptID, UserID, -1, ParentId, Search);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int accountId, int ParentId, string Search)
		{
			return SelectTreeActiveFiltered(DeptID, UserID, accountId, ParentId, Search, Guid.Empty);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int accountId, int ParentId, string Search, Guid orgId)
		{
			if (ParentId == 0)
			{
				ParentId = -1;
				DataTable _dt = GlobalFilters.SetFilter(orgId, DeptID, UserID, SelectTreeActive(DeptID, accountId, 0, Search, orgId), "Id", GlobalFilters.FilterType.Locations);
				_dt.DefaultView.Sort = "Id";
				_dt.DefaultView.ApplyDefaultSort = true;
				foreach (DataRow _row in _dt.Rows)
				{
					if (_dt.DefaultView.Find(_row["Id"]) == -1) _row.Delete();
					else if (_dt.DefaultView.Find(_row["ParentId"]) == -1) _row["ParentId"] = DBNull.Value;
				}
				if (ParentId < 0)
				{
					foreach (DataRow _row in _dt.Rows)
						if (_row.RowState != DataRowState.Deleted && !_row.IsNull("ParentId")) _row.Delete();
				}
				_dt.AcceptChanges();
				_dt.DefaultView.Sort = "Name";
				return _dt;
			}
			else return SelectTreeActive(DeptID, accountId, ParentId, Search, orgId);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int accountId /*internal -1*/, int LocationId)
		{
			return SelectTreeActiveFiltered(DeptID, UserID, accountId, LocationId, Guid.Empty);
		}

		public static DataTable SelectTreeActiveFiltered(int DeptID, int UserID, int accountId /*internal -1*/, int LocationId, Guid orgId)
		{
			DataTable _dtRaw = SelectRecords("sp_SelectLocationsUpTree",
				new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AccId", accountId), new SqlParameter("@LocationId", LocationId) }, orgId);
			DataTable _dt = GlobalFilters.SetFilter(orgId, DeptID, UserID, _dtRaw, "Id", GlobalFilters.FilterType.Locations);            
			return _dtRaw;            
		}

		public static DataTable SelectTreeAct(int DeptID, int UserID, int accountId /*internal -1*/, int LocationId, Guid orgId)
		{
			DataTable _dtRaw = SelectRecords("sp_SelectLocationsUpTree",
				new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AccId", accountId), new SqlParameter("@LocationId", LocationId) }, orgId);
			//DataTable _dt = GlobalFilters.SetFilter(orgId, DeptID, UserID, _dtRaw, "Id", GlobalFilters.FilterType.Locations);            
			return _dtRaw;
		}

		public static DataTable SelectUserAuditLocations(int DeptID, int UserID, int accountId)
		{
			DataTable _dtRaw = SelectRecords("sp_SelectUserAuditLocations",
				new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AccId", accountId), new SqlParameter("@UserID", UserID) });
			DataTable _dt = GlobalFilters.SetFilter(DeptID, UserID, _dtRaw, "Id", GlobalFilters.FilterType.Locations);
			return _dt;
		}

		public static DataRow SelectOne(int DeptID, int LocationID)
		{
			return SelectOne(Guid.Empty, DeptID, LocationID);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID, int LocationID)
		{
			return SelectRecord("sp_SelectLocationDetail", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationID) }, OrgID);
		}

		public static DataRow SelectOneByFullName(int DeptID, int AccID, string FullName)
		{
			int _parentId = 0;
			return SelectOneByFullName(DeptID, AccID, FullName, ref _parentId);
		}

		public static DataRow SelectOneByFullName(int DeptID, int AccID, string FullName, ref int ParentId)
		{
			if (FullName.Length == 0) return null;
			string[] _arr = FullName.Split('/');
			for (int i = 0; i < _arr.Length - 1; i++)
			{
				DataTable _t = SelectTree(DeptID, AccID, ParentId, _arr[i].Trim());
				if (_t.Rows.Count > 0) ParentId = (int)_t.Rows[0]["Id"];
				else
				{
					ParentId = -1;
					return null;
				}
			}
			DataTable _dt = SelectTree(DeptID, AccID, ParentId, _arr[_arr.Length - 1].Trim());
			if (_dt.Rows.Count > 0) return _dt.Rows[0];
			return null;
		}

		public static DataRow SelectOneNew(int DeptID, int Id)
		{
			return SelectRecord("sp_SelectLocationsTree", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
		}

		public static DataTable SelectAll(int DeptID, int UserID)
		{
			return GlobalFilters.SetFilter(DeptID, UserID, SelectAllActive(DeptID), "id", GlobalFilters.FilterType.Locations);
		}

		public static DataTable SelectAllActive(int DeptID)
		{
			return SelectRecords("sp_SelectLocations", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@btInactive", false) });
		}

		public static DataTable SelectAllActive(int DeptID, int UserID)
		{
			return GlobalFilters.SetFilter(DeptID, UserID, SelectAllActive(DeptID), "id", GlobalFilters.FilterType.Locations);
		}

		public static int Delete(int DeptID, int Id)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;
			UpdateData("sp_DeleteLocation", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", Id) });
			return (int)_pRVAL.Value;
		}

		public static void Transfer(int DeptID, int IdFrom, int IdTo)
		{
			UpdateData("sp_TransferLocation", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationIdFrom", IdFrom), new SqlParameter("@LocationIdTo", IdTo) });
		}

		public static void Move(int DeptID, int Id, int ParentId)
		{
			UpdateData("sp_MoveLocation", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id), new SqlParameter("@ParentId", ParentId) });
		}

		public static void Copy(int DeptID, int Id, int ParentId)
		{
			UpdateData("sp_CopyLocation", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id), new SqlParameter("@ParentId", ParentId) });
		}

		public static bool IsNameExists(int DeptID, int AccountId, int ParentId, int? TypeId, string name, int Id)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;
			SqlCommand _cmd = CreateSqlCommand("sp_SelectLocationNameExists", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@AccountId", AccountId), new SqlParameter("@ParentId", ParentId), new SqlParameter("@TypeId", TypeId), new SqlParameter("@Name", name), new SqlParameter("@Id", Id) });
			if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
			_cmd.ExecuteNonQuery();
			_cmd.Connection.Close();
			//			if (cnn.State!=ConnectionState.Closed) cnn.Close();
			if ((int)_pRVAL.Value == 1) return true;
			else return false;
		}

		public static bool IsNameExists(int DeptID, int AccountId, int ParentId, int TypeId, string name)
		{
			return IsNameExists(DeptID, AccountId, ParentId, TypeId, name, 0);
		}

		public static void Update(int DeptID, int LocationId,
			string Country, string State, string City, string Address1, string Address2, string ZipCode, string Phone1, string Phone2,
			int? WorkPlaces, string RoomNumber, float? RoomSize, int? LocationTypeId)
		{
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "Country", Country, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "State", State, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "City", City, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "Address1", Address1, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "Address2", Address2, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "ZipCode", ZipCode, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "Phone1", Phone1, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "Phone2", Phone2, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "WorkPlaces", WorkPlaces.ToString(), LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "RoomNumber", RoomNumber, LocationTypeId);
			Data.Locations.LocationTypeProperties.SetValue(DeptID, LocationId, "RoomSize", RoomSize.ToString(), LocationTypeId);
		}

		public static int Update(int DeptID, int Id, int ParentId, int AccountId, int? LocationTypeId, string Name, bool? Status,
			string Description, bool IsDefault, bool? AuditEnable, int? AuditorId, int? AuditPeriodDays)
		{
			return Update(Guid.Empty, DeptID, Id, ParentId, AccountId, LocationTypeId, Name, Status, Description, IsDefault, AuditEnable, AuditorId, AuditPeriodDays);
		}

		public static int Update(Guid OrgId, int DeptID, int Id, int ParentId, int AccountId, int? LocationTypeId, string Name, bool? Status, 
			string Description, bool IsDefault, bool? AuditEnable, int? AuditorId, int? AuditPeriodDays)
		{
			SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
			_pId.Direction = ParameterDirection.InputOutput;
			_pId.Value = Id;

			UpdateData("sp_UpdateLocation", new SqlParameter[]{_pId, 
											new SqlParameter("@DId", DeptID), 
											new SqlParameter("@ParentId", ParentId),
											new SqlParameter("@AccountId", AccountId),
											new SqlParameter("@LocationTypeId", LocationTypeId), 
											new SqlParameter("@Name", Name), 
											new SqlParameter("@Status", Status),
											new SqlParameter("@Description", Description), 
											new SqlParameter("@IsDefault", IsDefault),
											new SqlParameter("@AuditorId", AuditorId),
											new SqlParameter("@AuditPeriodDays", AuditPeriodDays),
											new SqlParameter("@EnableLocationAudit", AuditEnable)}, OrgId);
			return (int)_pId.Value;
		}

		public static DataTable SelectRecentUserInternalLocations(int departmentId, int userId)
		{
			return SelectRecords("sp_SelectUserRecentInternalLocations", new SqlParameter[]
				   {
					new SqlParameter("@DepartmentId", departmentId),
					new SqlParameter("@UserId", userId)
				   });
		}

		public static DataTable SelectRecentUserLocations(int departmentId, int userId, int accountId)
		{
			return SelectRecords("sp_SelectUserRecentAccountLocations", new SqlParameter[]
				   {
					new SqlParameter("@DepartmentId", departmentId),
					new SqlParameter("@UserId", userId),
					new SqlParameter("@AccountId", accountId)
				   });
		}

		private static DataTable SelectLocationsByAccountID(int dId, int accountID)
		{
			return SelectRecords("sp_SelectLocationsByAccountID", new SqlParameter[]
				   {
					new SqlParameter("@DId", dId),
					new SqlParameter("@AccId", accountID)
				   });
		}


		//********** Location tab and Audit wizard
		public static DataTable SelectLocationsWithTicketCount(int DeptID, int AccID, int ParentId, string Search, InactiveStatus inactiveStatus)
		{            
			SqlParameter pInactiveStatus = new SqlParameter("@Inactive", SqlDbType.Bit);
			if (inactiveStatus == InactiveStatus.DoesntMatter)
				pInactiveStatus.Value = DBNull.Value;
			else
				pInactiveStatus.Value = (inactiveStatus == InactiveStatus.Inactive) ? true : false;
			SqlParameter pSearch = new SqlParameter("@Search", SqlDbType.NVarChar);
			if (!String.IsNullOrEmpty(Search))
				pSearch.Value = "%" + Search + "%";
			else
				pSearch.Value = DBNull.Value;

			return SelectRecords("sp_SelectLocationsWithTicketCount",
				new SqlParameter[]
				{
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@Id", DBNull.Value),                    
					new SqlParameter("@AccId", AccID), // 0 - NULL = Internal Account, -1 = Internal Location
					new SqlParameter("@ParentId", ParentId), // 0 - top, NULL - all
					pSearch,
					pInactiveStatus
				});
		}


		public static DataRow SelectLocationAuditor(int DeptID, int LocationId, int UserId)
		{
			return SelectRecord("sp_SelectLocationAuditor", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId), new SqlParameter("@UserId", UserId) });
		}

		public static void UpdateLocationAuditor(int DeptID, int LocationId, int? AuditorId)
		{
			UpdateLocationAuditor(DeptID, LocationId, AuditorId, -1, -1);
		}

		public static void UpdateLocationAuditor(int DeptID, int LocationId, int? AuditorId, int? AditPeriodDays)
		{
			UpdateLocationAuditor(DeptID, LocationId, AuditorId, AditPeriodDays, -1);
		}

		public static void UpdateLocationAuditor(int DeptID, int LocationId, int? AuditorId, int? AditPeriodDays, int EnableAudit)
		{
			SqlParameter spAuditor = new SqlParameter("@AuditorId", SqlDbType.Int);
			if (AuditorId != null)
				spAuditor.Value = (int)AuditorId;
			else
				spAuditor.Value = DBNull.Value;
			UpdateData("sp_UpdateLocationAuditor", new SqlParameter[]{
											new SqlParameter("@DId", DeptID), 
											new SqlParameter("@LocationId", LocationId),										    
											new SqlParameter("@AuditorId", AuditorId),
											new SqlParameter("@AditPeriodDays", AditPeriodDays),
											new SqlParameter("@EnableAudit", EnableAudit)});
		}

		// Bulk Assets section
		public static DataTable SelectLocationAuditBulkAssets(int DeptID, int LocationId)
		{
			return SelectRecords("sp_SelectLocationAuditAssetsBulk", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@LocationId", LocationId) });
		}

		public static int SaveLocationAuditResults(int DeptID, int UserId, int LocationId, List<LocationBulkAssetAudit> AssetList, List<LocationBulkAssetAudit> AssetAddList)
		{
			int AuditId = Data.Locations.InsertAuditHistory(DeptID, UserId, LocationId);                
			DateTime AuditDate = DateTime.UtcNow;
			StringBuilder sb = new StringBuilder();
			foreach (LocationBulkAssetAudit a in AssetList)            
				sb.Append(String.Format(
					"IF EXISTS(SELECT Id FROM AssetBulkLocation WHERE DId={0} AND AssetBulkId={1} AND LocationId={2} AND AuditId={8}) " + 
					"UPDATE [AssetBulkLocation] SET [AuditDate] = '{3}', [Quantity] = {4}, [ExcessQuantity] = {5}, [Created] = '{6}', [CreatedBy] = {7} " +
					"WHERE DId={0} AND AssetBulkId={1} AND LocationId={2} AND AuditId={8} " +
					"ELSE " +
					"INSERT INTO AssetBulkLocation (DId, AssetBulkId, LocationId, AuditDate, Quantity, ExcessQuantity, Created, CreatedBy, AuditId) VALUES ({0}, {1}, {2}, '{3}', {4}, {5}, '{6}', {7}, {8})    ",
					DeptID, a.AssetId, LocationId, AuditDate.ToString(), a.Qty, a.NeededQty, AuditDate.ToString(), UserId, AuditId));
			foreach (LocationBulkAssetAudit a in AssetAddList)
				sb.Append(String.Format(
					"IF EXISTS(SELECT Id FROM AssetBulkLocation WHERE DId={0} AND AssetBulkId={1} AND LocationId={2} AND AuditId={8}) " +
					"UPDATE [AssetBulkLocation] SET [AuditDate] = '{3}', [Quantity] = {4}, [ExcessQuantity] = {5}, [Created] = '{6}', [CreatedBy] = {7} " +
					"WHERE DId={0} AND AssetBulkId={1} AND LocationId={2} AND AuditId={8} " +
					"ELSE " +
					"INSERT INTO AssetBulkLocation (DId, AssetBulkId, LocationId, AuditDate, Quantity, ExcessQuantity, Created, CreatedBy, AuditId) VALUES ({0}, {1}, {2}, '{3}', {4}, {5}, '{6}', {7}, {8})    ",
					DeptID, a.AssetId, LocationId, AuditDate.ToString(), a.Qty, a.NeededQty, AuditDate.ToString(), UserId, AuditId));
			int SaveRes = 0;
			string Query = sb.ToString();
			if (!String.IsNullOrEmpty(Query))
				SaveRes = UpdateByQuery(Query);
			return AuditId;
		}

		public class LocationBulkAssetAudit
		{
			public int AssetId { get; set; }
			public int Qty { get; set; }
			public int NeededQty { get; set; }

			public LocationBulkAssetAudit(int sAssetId, int sQty, int sNeededQty)
			{
				AssetId = sAssetId;
				Qty = sQty;
				NeededQty = sNeededQty;
			}
		}

		// Tagged assets section
		public static DataTable SelectLocationAuditTaggedAssets(int DeptID, int LocationId, bool WithPortable, int UserId)
		{
			return SelectRecords("sp_SelectLocationAuditAssetsTagged", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@LocationId", LocationId), new SqlParameter("@WithPortable", WithPortable), new SqlParameter("@UserId", UserId) });            
		}

		public static DataTable SelectLocationAuditAssetsPortable(int DeptID, int UserId)
		{
			return SelectRecords("sp_SelectLocationAuditAssetsPortable", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@UserId", UserId) });
		}
		
		public static int SaveLocationAuditResults(UserAuth u, int LocationId, List<LocationTaggedAssetAudit> AssetList, int AuditId, bool AddedAsset)
		{            
			SqlParameter[] param = new SqlParameter[AssetList.Count];

			DateTime AuditDate = DateTime.UtcNow;
			StringBuilder sb = new StringBuilder();
			if (AddedAsset)
			{
				if (AssetList.Count > 0)
					sb.Append("DECLARE @AssetNonActiveStatus bit ");
				foreach (LocationTaggedAssetAudit a in AssetList)
					//sb.Append(String.Format("UPDATE Assets SET LocationId={0}, Lost=0 WHERE [DepartmentId]={1} AND Id={2}   ", LocationId, u.lngDId, a.AssetId));
					sb.Append(String.Format(
						"SELECT @AssetNonActiveStatus = AssetStatusCompany.NonActive " +
						"FROM Assets LEFT OUTER JOIN AssetStatusCompany ON AssetStatusCompany.DId={1} and AssetStatusCompany.AssetStatusID=Assets.StatusId " +
						"WHERE Assets.DepartmentId={1} AND Assets.Id={2} " +
						"IF (@AssetNonActiveStatus = 1) EXEC sp_InsertAssetLog {1}, {3}, {2}, '{4}'      " +
						"UPDATE Assets SET LocationId={0}, Lost=0, StatusId=CASE WHEN @AssetNonActiveStatus = 1 THEN 1 ELSE StatusId END WHERE [DepartmentId]={1} AND Id={2}   ", LocationId, u.lngDId, a.AssetId, u.lngUId, "This asset''s status was changed to ''Active'' during audit wizard because it had non-active status assigned."));
			}
			for (int i = 0; i < AssetList.Count; i++)
			{
				LocationTaggedAssetAudit a = AssetList[i];
				SqlParameter spNote = new SqlParameter("@spNote" + i.ToString(), a.Note);
				sb.Append(String.Format(
					"IF EXISTS(SELECT Id FROM LocationAuditTaggedAssets WHERE DId = {0} AND LocationId = {1} AND AssetId = {2} AND AuditId = {8}) " +
					"UPDATE LocationAuditTaggedAssets SET " +
					"[AuditDate] = '{3}', [Status] = {4}, [Note] = @spNote{7}, [Created] = '{5}', [AuditorId] = {6}, [AddedAsset] = {9}, [Transferred] = {10}, [SourceLocationId] = {11} " +
					"WHERE DId = {0} AND LocationId = {1} AND AssetId = {2} AND AuditId = {8} " +
					"ELSE " +
					"INSERT INTO LocationAuditTaggedAssets (DId, LocationId, AssetId, AuditDate, Status, Created, AuditorId, Note, AuditId, AddedAsset, Transferred, SourceLocationId) VALUES ({0}, {1}, {2}, '{3}', {4}, '{5}', {6}, @spNote{7}, {8}, {9}, {10}, {11})    ",
					u.lngDId, LocationId, a.AssetId, AuditDate.ToString(), (a.Status == null ? "NULL" : (a.Status == true ? "1" : "0")), AuditDate.ToString(), u.lngUId, i.ToString(), AuditId, AddedAsset ? "1" : "0",
					((AddedAsset && ((int?)LocationId != a.LocationId)) ? "1" : "0"), ((AddedAsset && ((int?)LocationId != a.LocationId)) ? a.LocationId.ToString() : "NULL")));
				param[i] = spNote;

				if (!AddedAsset)
					sb.Append(String.Format("UPDATE Assets SET Lost={0}, LostOn={1}, AuditNote=@spNote{2} WHERE [DepartmentId]={3} AND Id={4}    ",
						a.Status == false ? "1" : "0", a.Status == false ? "'" + AuditDate.ToString() + "'" : "NULL", i.ToString(), u.lngDId, a.AssetId));
			}
			int SaveRes = 0;
			string Query = sb.ToString();
			if (!String.IsNullOrEmpty(Query))
			{
				SaveRes = UpdateByQuery(Query, param);
				foreach (LocationTaggedAssetAudit a in AssetList)
					if (AddedAsset && ((int?)LocationId != a.LocationId))
						Data.Asset.InsertAssetLog(u.lngDId, u.lngUId, a.AssetId, "Transferred to new " + u.customNames.Location.fullSingular + " during audit wizard (" + AuditId.ToString() + ")");
				foreach (LocationTaggedAssetAudit a in AssetList)
					if (!AddedAsset && a.Status == false)
						Data.Asset.InsertAssetLog(u.lngDId, u.lngUId, a.AssetId, "This asset was reported not at this " + u.customNames.Location.fullSingular + " during audit wizard (" + AuditId.ToString() + "). Note: " + a.Note);
			}
			else
				SaveRes = 1;
			return SaveRes;
		}

		public static int SaveLocationPortableAuditResults(UserAuth u, int AuditId, List<LocationTaggedAssetAudit> AssetList, bool AddedAsset)
		{// how to update previously saved audits results
			if (AuditId == 0)
				AuditId = InsertAuditHistory(u.lngDId, u.lngUId, 0);
			SqlParameter[] param = new SqlParameter[AssetList.Count];

			DateTime AuditDate = DateTime.UtcNow;
			StringBuilder sb = new StringBuilder();
			if (AddedAsset)
			{
				if (AssetList.Count > 0)
					sb.Append("DECLARE @AssetNonActiveStatus bit ");
				foreach (LocationTaggedAssetAudit a in AssetList)
					sb.Append(String.Format(
						"SELECT @AssetNonActiveStatus = AssetStatusCompany.NonActive " +
						"FROM Assets LEFT OUTER JOIN AssetStatusCompany ON AssetStatusCompany.DId={0} and AssetStatusCompany.AssetStatusID=Assets.StatusId " +
						"WHERE Assets.DepartmentId={0} AND Assets.Id={1} " +
						"IF (@AssetNonActiveStatus = 1) EXEC sp_InsertAssetLog {0}, {2}, {1}, '{3}'      " +
						"UPDATE Assets SET Lost=0, StatusId=CASE WHEN @AssetNonActiveStatus = 1 THEN 1 ELSE StatusId END WHERE [DepartmentId]={0} AND Id={1}   ", u.lngDId, a.AssetId, u.lngUId, "This asset''s status was changed to ''Active'' during audit wizard because it had non-active status assigned."));
			}
			for (int i = 0; i < AssetList.Count; i++)
			{
				LocationTaggedAssetAudit a = AssetList[i];
				SqlParameter spNote = new SqlParameter("@spNote" + i.ToString(), a.Note);
				sb.Append(String.Format(
					"IF EXISTS(SELECT Id FROM LocationAuditTaggedAssets WHERE DId = {0} AND LocationId IS NULL AND AssetId = {2} AND AuditId = {8}) " +
					"UPDATE LocationAuditTaggedAssets SET " +
					"[AuditDate] = '{3}', [Status] = {4}, [Note] = @spNote{7}, [Created] = '{5}', [AuditorId] = {6}, [AddedAsset] = {9}, [Transferred] = {10}, [SourceLocationId] = {11} " +
					"WHERE DId = {0} AND LocationId IS NULL AND AssetId = {2} AND AuditId = {8} " +
					"ELSE " +
					"INSERT INTO LocationAuditTaggedAssets " +
					"(DId, LocationId, AssetId, AuditDate, Status, Created, AuditorId, Note, AuditId, AddedAsset, Transferred, SourceLocationId) " +
					"VALUES ({0}, {1}, {2}, '{3}', {4}, '{5}', {6}, @spNote{7}, {8}, {9}, {10}, {11})    ",
					u.lngDId, (AddedAsset && a.LocationId != null) ? a.LocationId.ToString() : "NULL", a.AssetId, AuditDate.ToString(),
					(a.Status == null ? "NULL" : (a.Status == true ? "1" : "0")), AuditDate.ToString(), u.lngUId, i.ToString(), AuditId, AddedAsset ? "1" : "0",
					0/*((AddedAsset && ((int?)LocationId != a.LocationId)) ? "1" : "0")*/, 
					"NULL"/*((AddedAsset && ((int?)LocationId != a.LocationId)) ? a.LocationId.ToString() : "NULL")*/));
				param[i] = spNote;

				if (!AddedAsset)
					sb.Append(String.Format("UPDATE Assets SET Lost={0}, LostOn={1}, AuditNote=@spNote{2} WHERE [DepartmentId]={3} AND Id={4}    ",
						a.Status == false ? "1" : "0", a.Status == false ? "'" + AuditDate.ToString() + "'" : "NULL", i.ToString(), u.lngDId, a.AssetId));
			}
			int SaveRes = 0;
			string Query = sb.ToString();
			if (!String.IsNullOrEmpty(Query))
			{
				SaveRes = UpdateByQuery(Query, param);
				/*foreach (LocationTaggedAssetAudit a in AssetList)
					if (AddedAsset && ((int?)LocationId != a.LocationId))
						Data.Asset.InsertAssetLog(u.lngDB, u.lngDId, u.lngUId, a.AssetId, "Transferred to new " + u.customNames.Location.fullSingular + " during audit wizard (" + AuditId.ToString() + ")");*/
				foreach (LocationTaggedAssetAudit a in AssetList)
					if (!AddedAsset && a.Status == false)
						Data.Asset.InsertAssetLog(u.lngDId, u.lngUId, a.AssetId, "This asset was reported not at this " + u.customNames.Location.fullSingular + " during audit wizard (" + AuditId.ToString() + "). Note: " + a.Note);
			}
			return AuditId;
		}

		public class LocationTaggedAssetAudit
		{
			public int AssetId { get; set; }
			public bool? Status { get; set; }
			public string Note { get; set; }
			public int? LocationId { get; set; }

			public LocationTaggedAssetAudit(int sAssetId, bool? sStatus, string sNote, int? sLocationId)
			{
				AssetId = sAssetId;
				Status = sStatus;
				Note = sNote;
				LocationId = sLocationId;
			}
		}

		public static int InsertAuditHistory(int DeptID, int UserId, int LocationId)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;
			UpdateData("sp_InsertLocationAudit",
						new[] { 
								pReturnValue,
								new SqlParameter("@DId", DeptID),
								new SqlParameter("@UserId", UserId),
								new SqlParameter("@LocationId", LocationId)
								});

			return (int)pReturnValue.Value;
		}

		public static void CompleteLocationAudit(int DeptID, int UserId, int AuditId, int LocationId)
		{
			if (AuditId == 0)
				AuditId = InsertAuditHistory(DeptID, UserId, LocationId);
			string Query = String.Format("UPDATE LocationAuditHistory SET Completed = 1, AuditDate='{2}' WHERE DId={0} AND Id={1}   ", DeptID.ToString(), AuditId.ToString(), DateTime.UtcNow.ToString());
			int SaveRes = 0;            
			if (!String.IsNullOrEmpty(Query))
				SaveRes = UpdateByQuery(Query);                        
		}
		
		public static DataTable SelectUserAuditHistory(int DeptID, int UserId)
		{
			return SelectRecords("sp_SelectUserAuditHistory", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UserId", UserId) });
		}

		public static DataTable SelectUserAssignedAudits(int DeptID, int UserId)
		{
			return SelectUserAssignedAudits(DeptID, UserId, 0, null, true);
		}

		public static DataTable SelectUserAssignedAudits(int DeptID, int UserId, int LocationId, int? DueDays, bool IncludePortableAudit)
		{
			return SelectRecords("sp_SelectUserAssignedAudits", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UserId", UserId), new SqlParameter("@LocationId", LocationId), new SqlParameter("@DueDays", DueDays), new SqlParameter("@IncludePortableAudit", IncludePortableAudit) });
		}

		public static DataRow SelectAudit(int DeptID, int AuditId)
		{
			return SelectRecord("sp_SelectLocationAudit", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AuditId", AuditId) });
		}

		public static DataTable SelectAuditBulkAssets(int DeptID, int AuditId)
		{
			return SelectRecords("sp_SelectLocationAuditAssetsBulkSet", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AuditId", AuditId) });
		}

		public static DataTable SelectAuditTaggedAssets(int DeptID, int AuditId)
		{
			return SelectRecords("sp_SelectLocationAuditAssetsTaggedSet", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AuditId", AuditId) });
		}

		public static DataTable SelectUsers(int DeptID, int LocationId)
		{
			return SelectRecords("sp_SelectLocationUsers", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId) });
		}

		public static DataTable SelectAssets(int DeptID, int LocationId)
		{
			return SelectRecords("sp_SelectLocationAssets", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId) });
		}

		public static DataTable SelectBulkAssets(int DeptID, int LocationId)
		{
			return SelectRecords("sp_SelectLocationBulkAssets", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId) });
		}


		// Select audit settings for several locations
		public static DataTable SelectLocationsAuditSettings(int DeptID, string LocationsId)
		{               
			DataTable _table = SelectByQuery(String.Format("SELECT  L.Id, L.CfgEnableAudit, ISNULL(l.AuditorId, 0) AS AuditorId, l.AuditPeriodDays, dbo.fxGetLocationName({0}, L.Id) AS LocationName " +
			"FROM Locations L " +
			"WHERE L.DId = {0} AND L.Id IN ({1}) " +
			"ORDER BY LocationName", DeptID.ToString(), LocationsId));
			return _table;
		}

		public class LocationAuditSettings
		{
			public int LocationId { get; set; }
			public bool AuditEnable { get; set; }
			public int AuditorId { get; set; }
			public int Frequency { get; set; }

			public LocationAuditSettings(int sLocationId, bool sAuditEnable, int sAuditorId, int sFrequency)
			{
				LocationId = sLocationId;
				AuditEnable = sAuditEnable;
				AuditorId = sAuditorId;
				Frequency = sFrequency;
			}
		}

		public static int SaveBulkLocationsAuditSettings(int DeptID, List<LocationAuditSettings> AuditSettings)
		{   
			StringBuilder sb = new StringBuilder();
			foreach (LocationAuditSettings a in AuditSettings)
				sb.Append(String.Format("UPDATE Locations SET CfgEnableAudit={0}, AuditorId={1}, AuditPeriodDays={2} WHERE [DId]={3} AND Id={4}   ",
					a.AuditEnable ? "1" : "0", a.AuditorId > 0 ? a.AuditorId.ToString() : "NULL", a.Frequency > 0 ? a.Frequency.ToString() : "NULL", DeptID, a.LocationId));
			int SaveRes = 0;
			string Query = sb.ToString();
			if (!String.IsNullOrEmpty(Query))
				SaveRes = UpdateByQuery(Query);
			return SaveRes;
		}

		public static DataTable SelectUserAssignedAuditingLocations(int DeptID, string LocationsId)
		{
			DataTable _table = SelectByQuery(
				String.Format("SELECT l.AuditorId, l.AuditPeriodDays, L.Id, lo.FirstName, lo.LastName, lo.Password, lo.Email, dbo.fxGetLocationName({0}, L.Id) AS LocationName, dbo.fxGetLocationAuditDueDays({0}, L.Id) AS DueDays, tlj.UserType_Id " +
					"FROM Locations L " +
					"LEFT OUTER JOIN tbl_LoginCompanyJunc tlj ON tlj.company_id = {0} AND l.AuditorId = tlj.id " +
					"LEFT OUTER JOIN tbl_Logins lo ON lo.id = tlj.login_Id " +
					"WHERE " +
					"L.DId = {0} AND L.CfgEnableAudit = 1 AND L.Id IN ({1}) AND AuditorId IS NOT NULL AND " +
					"dbo.fxGetLocationAuditDueDays({0}, L.Id) < 50 " +
					"ORDER BY AuditorId, DueDays, LocationName", DeptID.ToString(), LocationsId));            
			return _table;
		}

		public static string SendAuditNotifications(UserAuth usr, string Locations, bool send_as_html)
		{
			string _subject = "Inventory Update Request";
			DataRow drUserDetails = Logins.SelectLoginDetailsByEmail(usr.strEmail);
			string UserPhone = String.Empty;
			if (drUserDetails != null && !drUserDetails.IsNull("Phone"))
				UserPhone = drUserDetails["Phone"].ToString();
			string _from = "\"" + usr.strGFName + " " + usr.strGLName + " - " + usr.strGDName + "\"" + "<" + usr.strEmail + ">";
			string MailSign = send_as_html ?
				String.Format("{0}&nbsp;{1}<br />{2}<br />{3}", usr.strGFName, usr.strGLName, usr.strEmail, UserPhone) :
				String.Format("{0} {1}\r\n{2}\r\n{3}", usr.strGFName, usr.strGLName, usr.strEmail, UserPhone);
			string _return_string = string.Empty;
			DataTable dt = Data.Locations.SelectUserAssignedAuditingLocations(usr.lngDId, Locations);
			if (dt != null && dt.Rows.Count > 0)
			{
				int AuditorId = (int)dt.Rows[0]["AuditorId"];
				string _email = dt.Rows[0]["Email"].ToString();
				string _password = dt.Rows[0]["Password"].ToString();
				string FirstName = dt.Rows[0]["FirstName"].ToString();
				bool _isTech = !((int)dt.Rows[0]["UserType_Id"] == 1 || (int)dt.Rows[0]["UserType_Id"] == 5);

				StringBuilder sbLocationsNow = new StringBuilder();
				StringBuilder sbLocationsUpcoming = new StringBuilder();
				string deptLink = Functions.GetDeptWebAddress(new Data.Companies.Department(usr.lngDId), _email, _password, _isTech) + "&audit=true&lid=";
				
				for (int i = 0; i < dt.Rows.Count; i++)
				{
					if ((int)dt.Rows[i]["AuditorId"] != AuditorId || (i == dt.Rows.Count - 1))
					{
						if (i == dt.Rows.Count - 1)
						{
							int DueDaysk = (int)dt.Rows[i]["DueDays"];
							if (DueDaysk <= 0)
								sbLocationsNow.Append(dt.Rows[i]["LocationName"] + (send_as_html ? "<br />" : "\r\n"));
							else
								sbLocationsUpcoming.Append(dt.Rows[i]["LocationName"] + (send_as_html ? "<br />" : "\r\n"));
						}
						string link = deptLink + dt.Rows[i]["Id"].ToString();
						StringBuilder _body = new StringBuilder();
						if (send_as_html)
						{
							_body.Append("<!DOCTYPE HTML PUBLIC\"-//IETF//DTD HTML//EN\"><html><body>");
							_body.AppendFormat("Hi {0},<br /><br />", FirstName);
							_body.AppendFormat("Your help is needed for {0}'s inventory audit.<br /><br />", usr.strGDName);

							if (sbLocationsNow.Length > 0)
							{
								_body.Append("<div style=\"padding: 10px; background-color:yellow; width: 98%;\">");
								_body.Append("Audits Due Now<br />-----------------------<br />");
								_body.AppendFormat("{0}<br /><br />", sbLocationsNow.ToString());
								_body.Append("</div>");
							}
							if (sbLocationsUpcoming.Length > 0)
							{
								_body.Append("<div style=\"padding: 10px; background-color:yellow; width: 98%;\">");
								_body.Append("Upcoming<br />-----------------------<br />");
								_body.AppendFormat("{0}<br /><br />", sbLocationsUpcoming.ToString());
								_body.Append("</div>");
							}
							_body.Append("<br />");
							_body.AppendFormat("Click the following link to perform this audit. It will only take a few moments.<br /><br />");
							_body.AppendFormat("<a href='{0}'>{1}</a><br /><br />", link, link.Replace("www.", string.Empty));
							_body.AppendFormat("Not the auditor for these locations?  Click the link above, then you can remove yourself from these locations.<br /><br />");                            
						}
						else
						{
							_body.AppendFormat("Hi {0},\r\n\r\n", FirstName);
							_body.AppendFormat("Your help is needed for {0}'s inventory audit.\r\n\r\n", usr.strGDName);

							if (sbLocationsNow.Length > 0)
							{
								_body.Append("Audits Due Now\r\n-----------------------\r\n");
								_body.AppendFormat("{0}\r\n\r\n", sbLocationsNow.ToString());
							}
							if (sbLocationsUpcoming.Length > 0)
							{
								_body.Append("Upcoming\r\n-----------------------\r\n");
								_body.AppendFormat("{0}\r\n\r\n", sbLocationsUpcoming.ToString());
							}
							_body.Append("\r\n");
							_body.AppendFormat("Click the following link to perform this audit. It will only take a few moments.\r\n\r\n");
							_body.AppendFormat("{0}\r\n\r\n", link);
							_body.AppendFormat("Not the auditor for these locations?  Click the link above, then you can remove yourself from these locations.\r\n\r\n");
						}
						_body.Append(MailSign);
						if (send_as_html)
							_body.Append("</body></html>");

						try
						{
							Data.MailNotification _mn = new Data.MailNotification(usr.lngDId, usr.lngUId, _from, _email, _subject, _body.ToString());
							string cur_operation_result = String.Empty;
							if (System.Configuration.ConfigurationManager.AppSettings["UseWindowServiceForNotifications"] != null && System.Configuration.ConfigurationManager.AppSettings["UseWindowServiceForNotifications"].ToLower() == "true")
								cur_operation_result += _mn.Commit(Data.MailNotification.UseSendMailEngine.NotificationService, send_as_html);
							else
								cur_operation_result += _mn.Commit(Data.MailNotification.UseSendMailEngine.SystemWebMail, send_as_html);
							if (!String.IsNullOrEmpty(cur_operation_result))
								_return_string += cur_operation_result + "<br />";                            
						}
						catch
						{
							_return_string += "Mail notification service failed." + "<br />"; // ex.Message
						};
						sbLocationsNow = new StringBuilder();
						sbLocationsUpcoming = new StringBuilder();
						AuditorId = (int)dt.Rows[i]["AuditorId"];
						_email = dt.Rows[i]["Email"].ToString();
						_password = dt.Rows[i]["Password"].ToString();
						FirstName = dt.Rows[i]["FirstName"].ToString();
					}
					int DueDays = (int)dt.Rows[i]["DueDays"];
					if (DueDays <= 0)
						sbLocationsNow.Append(dt.Rows[i]["LocationName"] + (send_as_html ? "<br />" : "\r\n"));
					else
						sbLocationsUpcoming.Append(dt.Rows[i]["LocationName"] + (send_as_html ? "<br />" : "\r\n"));
				}
			}
			return _return_string;
		}


		public class LocationAliases : DBAccess
		{
			public static DataTable SelectAll(int DeptID, int? LocationId)
			{
				return SelectRecords("sp_SelectLocationAliases", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId) });
			}

			public static DataRow SelectOne(int DeptID, int Id)
			{
				SqlParameter _pLocationId = new SqlParameter("@LocationId", SqlDbType.Int);
				_pLocationId.Value = DBNull.Value;
				return SelectRecord("sp_SelectLocationAliases", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pLocationId, new SqlParameter("@Id", Id) });
			}

			public static int Update(int DeptID, int Id, int LocationId, string AliasName)
			{
				SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				_pRVAL.Direction = ParameterDirection.ReturnValue;
				SqlParameter _pAliasId = new SqlParameter("@Id", SqlDbType.Int);
				_pAliasId.Direction = ParameterDirection.InputOutput;
				if (Id != 0) _pAliasId.Value = Id;
				else _pAliasId.Value = DBNull.Value;
				UpdateData("sp_UpdateLocationAlias", new SqlParameter[] { _pRVAL, _pAliasId, new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId), new SqlParameter("@AliasName", AliasName) });
				if ((int)_pRVAL.Value < 0) return (int)_pRVAL.Value;
				else return (int)_pAliasId.Value;
			}

			public static void Delete(int DeptID, int Id)
			{
				UpdateData("sp_DeleteLocationAlias", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}

			public static int UpdateLocationAliases(int DeptID, int LocationId, List<string> NewLocationAliases)
			{
				StringBuilder sb = new StringBuilder(String.Format("DELETE FROM [LocationAliases] WHERE DId={0} AND LocationId={1}    ", DeptID, LocationId));
				foreach (string a in NewLocationAliases)
					sb.Append(String.Format("INSERT INTO LocationAliases (DId, LocationId, LocationAliasName) VALUES ({0}, {1}, '{2}')    ",
						DeptID, LocationId, a));
				int SaveRes = 0;
				string Query = sb.ToString();
				if (!String.IsNullOrEmpty(Query))
					SaveRes = UpdateByQuery(Query);
				return SaveRes;
			}
		}

		public class LocationTypes : DBAccess
		{
			public static DataTable SelectAll(int DeptID)
			{
				return SelectAll(Guid.Empty, DeptID);
			}

			public static DataTable SelectAll(Guid OrgId, int DeptID)
			{
				return SelectRecords("sp_SelectLocationTypes", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
			}

			public static DataRow SelectOne(int DeptID, int Id)
			{
				return SelectRecord("sp_SelectLocationTypes", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}

			public static DataRow SelectGroup(int DeptID, int GroupId)
			{
				return SelectRecord("sp_SelectLocationTypePropertieGroup", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@GroupId", GroupId) });
			}

			public static DataTable SelectGroups(int DeptID, int LocationTypeId)
			{
				return SelectRecords("sp_SelectLocationTypePropertieGroup", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationTypeId", LocationTypeId) });
			}

			public static void DeleteGroup(int DeptID, int GroupId)
			{
				UpdateData("sp_DeleteLocationTypePropertieGroup", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@GroupId", GroupId) });
			}

			public static void SaveGroup(int DeptID, int TypeId, int GroupId, string Name)
			{
				UpdateData("sp_UpdateLocatioTypePropertiesGroup", new SqlParameter[] { 
					new SqlParameter("@DId", DeptID), 
					new SqlParameter("@TypeId", TypeId),
					new SqlParameter("@GroupId", GroupId),
					new SqlParameter("@Name", Name) });
			}

			public static bool IsNameExists(int DeptID, string name, int Id)
			{
				SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				_pRVAL.Direction = ParameterDirection.ReturnValue;
				SqlCommand _cmd = CreateSqlCommand("sp_SelectLocationTypeNameExists", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@Name", name), new SqlParameter("@Id", Id) });
				if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
				_cmd.ExecuteNonQuery();
				_cmd.Connection.Close();
				//				if (cnn.State!=ConnectionState.Closed) cnn.Close();
				if ((int)_pRVAL.Value == 1) return true;
				else return false;
			}

			public static bool IsNameExists(int DeptID, string name)
			{
				return IsNameExists(DeptID, name, 0);
			}

			public static int Update(int DeptID, int Id, string Name, int HierarchyLevel)
			{
				return Update(Guid.Empty, DeptID, Id, Name, HierarchyLevel);
			}

			public static int Update(Guid OrgId, int DeptID, int Id, string Name, int HierarchyLevel)
			{
				SqlParameter _pId = new SqlParameter("@Id", Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateLocationType", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptID), new SqlParameter("@Name", Name), new SqlParameter("@HierarchyLevel", HierarchyLevel) }, OrgId);
				return (int)_pId.Value;
			}

			public static void Delete(int DeptID, int Id)
			{
				UpdateData("sp_DeleteLocationType", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}
		}

		public class UserLocations : DBAccess
		{
			public static DataTable SelectAll(int DeptID, int UserId)
			{
				return SelectRecords("sp_SelectUserLocations", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId) });
			}
			/*
						public static DataTable SelectLocationsTree(int DeptID, int UserId)
						{
							return SelectLocationsTree(DeptID, UserId, -1);
						}

						public static DataTable SelectLocationsTree(int DeptID, int UserId, string Search)
						{
							return SelectLocationsTree(DeptID, UserId, -1, Search);
						}

						public static DataTable SelectLocationsTree(int DeptID, int UserId, int ParentId)
						{
							return SelectLocationsTree(DeptID, UserId, ParentId, string.Empty);
						}

						public static DataTable SelectLocationsTree(int DeptID, int UserId, int ParentId, string Search)
						{
							if (Search.Length>0)
							{
								SqlParameter _pid=new SqlParameter("@ParentId", SqlDbType.Int);
								if (ParentId<0) _pid.Value=DBNull.Value;
								else _pid.Value=ParentId;
								return SelectRecords("sp_SelectLocationsSearch", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pid, new SqlParameter("@Search", Search)});
							}
							else
							{
								SqlParameter _pid=new SqlParameter("@ParentId", SqlDbType.Int);
								if (ParentId<0) _pid.Value=DBNull.Value;
								else _pid.Value=ParentId;
								return SelectRecords("sp_SelectLocationsTreeForUser", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pid});
							}
						}

						public static DataTable SelectLocationsTreeFiltered(int DeptID, int UserId)
						{
							return SelectLocationsTreeFiltered(DeptID, UserId, -1, string.Empty);
						}

						public static DataTable SelectLocationsTreeFiltered(int DeptID, int UserId, string Search)
						{
							return SelectLocationsTreeFiltered(DeptID, UserId, -1, Search);
						}

						public static DataTable SelectLocationsTreeFiltered(int DeptID, int UserId, int ParentId)
						{
							return SelectLocationsTreeFiltered(DeptID, UserId, ParentId, string.Empty);
						}

						public static DataTable SelectLocationsTreeFiltered(int DeptID, int UserId, int ParentId, string Search)
						{
							DataTable _dt=GlobalFilters.SetFilter(DeptID, UserId, SelectLocationsTree(DeptID, UserId, ParentId, Search), "Id", GlobalFilters.FilterType.Locations);
							_dt.DefaultView.Sort="Id";
							_dt.DefaultView.ApplyDefaultSort=true;
							foreach(DataRow _row in _dt.Rows)
							{
								if (_dt.DefaultView.Find(_row["Id"])==-1) _row.Delete();
								else if (_dt.DefaultView.Find(_row["ParentId"])==-1) _row["ParentId"]=DBNull.Value;
							}
							_dt.AcceptChanges();
							_dt.DefaultView.Sort="Name";
							return _dt;
						}
			*/
			public static void Delete(int DeptID, int Id)
			{
				UpdateData("sp_DeleteUserLocation", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}

			public static int Update(int DeptID, int Id, int UserId, int LocationId, bool IsPrimary)
			{
				SqlParameter _pId = new SqlParameter("@Id", Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateUserLocation", new SqlParameter[]{_pId, 
																		  new SqlParameter("@DId", DeptID), 
																		  new SqlParameter("@UId", UserId),
																		  new SqlParameter("@LocationId", LocationId), 
																		  new SqlParameter("@IsPrimary", IsPrimary)});
				return (int)_pId.Value;
			}
		}

		public enum TechPoolType
		{
			Standard = 0,
			GlobalLevel = 3
		}

		public enum RoutingMethod
		{
			Distributed = 1,
			LeastOfTickets = 0
		}

		public class Routings
		{
			public static DataTable SelectLocationLevels(int DeptId, int LocationId)
			{
				return SelectRecords("sp_SelectLocationRouteLevels", new SqlParameter[] { new SqlParameter("@DId", DeptId), new SqlParameter("@LocationId", LocationId) });
			}

			public static int UpdateLocationLevel(int DeptID, int Id, int LocationId, int TktLevel, int TechPoolType, int RoutingMethod, int LastResortTechId)
			{
				SqlParameter _pId = new SqlParameter("@Id", Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateLocationRouteLevel", new SqlParameter[]{_pId, 
																			new SqlParameter("@DId", DeptID), 
																			new SqlParameter("@LocationId", LocationId),
																			new SqlParameter("@TicketLevel", TktLevel), 
																			new SqlParameter("@TechPoolType", TechPoolType), 
																			new SqlParameter("@RoutingMethod", RoutingMethod), 
																			new SqlParameter("@LastResortTechId", LastResortTechId)});
				return (int)_pId.Value;
			}

			public static void DeleteLocationLevel(int DeptID, int Id)
			{
				UpdateData("sp_DeleteLocationRouteLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}

			public static DataTable SelectTechniciansForLocationLevel(int DeptId, int LocationRouteLevelId)
			{
				return SelectRecords("sp_SelectLocationRouteLevelTechs", new SqlParameter[] { new SqlParameter("@DId", DeptId), new SqlParameter("@LocationRouteLevelId", LocationRouteLevelId) });
			}

			public static int AddTechnicianToLocationLevel(int DeptID, int LocationRouteLevelId, int TechId)
			{
				SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
				_pId.Value = DBNull.Value;
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateLocationRouteLevelTech", new SqlParameter[]{_pId, 
																				new SqlParameter("@DId", DeptID), 
																				new SqlParameter("@LocationRouteLevelId", LocationRouteLevelId),
																				new SqlParameter("@TechId", TechId)});
				return (int)_pId.Value;
			}

			public static void DeleteTechnicianFromLocationLevel(int DeptID, int Id)
			{
				UpdateData("sp_DeleteLocationRouteLevelTech", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}
		}

		public enum PropertyType
		{
			Text = 0,
			Integer = 1,
			Numeric = 2,
			DateTime = 3,
			Enumeration = 4
		}

		public class LocationTypeProperties : DBAccess
		{
			public static DataTable SelectAll(int DeptID, int LocationTypeID)
			{
				return SelectRecords("sp_SelectLocationTypeProperties", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationTypeId", LocationTypeID) });
			}

			public static DataTable SelectAllGroups(int DeptID, int LocationTypeID)
			{
				return SelectRecords("sp_SelectLocationTypeProperties", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationTypeId", LocationTypeID) });
			}

			public static DataRow SelectOne(int DeptID, int LocationTypeID, int Id)
			{
				return SelectRecord("sp_SelectLocationTypeProperties", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationTypeId", LocationTypeID), new SqlParameter("@Id", Id) });
			}

			public static string GetValue(int DeptID, int LocationId, int LocationTypePropertyId)
			{
				SqlParameter _pValue = new SqlParameter("@PropertyValue", SqlDbType.NVarChar, 255);
				_pValue.Direction = ParameterDirection.Output;
				SqlCommand _cmd = CreateSqlCommand("sp_GetLocationPropertyValue", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId), new SqlParameter("@LocationTypePropertyId", LocationTypePropertyId), _pValue });
				if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
				_cmd.ExecuteNonQuery();
				_cmd.Connection.Close();
				//				if (cnn.State!=ConnectionState.Closed) cnn.Close();
				return _pValue.Value.ToString();
			}

			public static DataTable SelectValues(int DeptID, int LocationId)
			{
				return SelectRecords("sp_SelectLocationTypePropertieValues", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@LocationId", LocationId)});
			}

			public static int SetValue(int DeptID, int LocationId, int LocationTypePropertyId, string PropertyValue)
			{
				SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
				_pId.Direction = ParameterDirection.InputOutput;
				_pId.Value = DBNull.Value;
				UpdateData("sp_UpdateLocationPropertyValue", new SqlParameter[] {
					_pId, 
					new SqlParameter("@DId", DeptID), 
					new SqlParameter("@LocationId", LocationId), 
					new SqlParameter("@LocationTypePropertyId", LocationTypePropertyId), 
					new SqlParameter("@PropertyValue", PropertyValue) });
				return (int)_pId.Value;
			}

			public static void SetValue(int DeptID, int LocationId, string propertyName, string PropertyValue,
				int? locationTypeId)
			{
				UpdateData("sp_UpdateLocationPropertyValueByName", new SqlParameter[] { 
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@LocationId", LocationId),
					new SqlParameter("@PropertyName", propertyName), 
					new SqlParameter("@PropertyValue", PropertyValue), 
					new SqlParameter("@LocationTypeId", locationTypeId) });
			}

			public static bool IsNameExists(int DeptID, int LocationTypeId, string name, int Id)
			{
				SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				_pRVAL.Direction = ParameterDirection.ReturnValue;
				SqlCommand _cmd = CreateSqlCommand("sp_SelectLocationTypePropertyNameExists", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@LocationTypeId", LocationTypeId), new SqlParameter("@Name", name), new SqlParameter("@Id", Id) });
				if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
				_cmd.ExecuteNonQuery();
				_cmd.Connection.Close();
				//				if (cnn.State!=ConnectionState.Closed) cnn.Close();
				if ((int)_pRVAL.Value == 1) return true;
				else return false;
			}

			public static bool IsNameExists(int DeptID, int LocationTypeId, string name)
			{
				return IsNameExists(DeptID, LocationTypeId, name, 0);
			}

			public static int Update(int DeptID, int Id, int LocationTypeId, string Name, int DataType, string Enumeration, string Description, int? GroupId)
			{
				SqlParameter _pId = new SqlParameter("@Id", Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateLocationTypeProperty", new SqlParameter[]{_pId, 
																				  new SqlParameter("@DId", DeptID), 
																				  new SqlParameter("@LocationTypeId", LocationTypeId),
																				  new SqlParameter("@Name", Name), 
																				  new SqlParameter("@DataType", DataType), 
																				  new SqlParameter("@Enumeration", Enumeration), 
																				  new SqlParameter("@Description", Description),
																				  new SqlParameter("@GroupId", GroupId)});
				return (int)_pId.Value;
			}

			public static void Move(int DeptID, int LocationTypeId, int? SorcePropId, int? SorceGroupId, int? DestPropId, int? DestGroupId, bool MoveToFirst)
			{
				UpdateData("sp_MoveLocationTypeProperty", new SqlParameter[]{
					new SqlParameter("@DId", DeptID), 
					new SqlParameter("@LocationTypeId", LocationTypeId), 
					new SqlParameter("@SorcePropId", SorcePropId),
					new SqlParameter("@SorceGroupId", SorceGroupId), 
					new SqlParameter("@DestPropId", DestPropId), 
					new SqlParameter("@DestGroupId", DestGroupId),
					new SqlParameter("@MoveToFirst", MoveToFirst),
				});
			}

			public static void Delete(int DeptID, int Id)
			{
				UpdateData("sp_DeleteLocationTypeProperty", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
			}
		}
	}
}
