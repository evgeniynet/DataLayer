using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Companies.
	/// </summary>
	public class Companies : DBAccess
	{
		public static int UpdateData(int departmentId, string storedProcName, SqlParameter sqlParameter)
		{
			return UpdateData(storedProcName, sqlParameter);
		}

		public static int UpdateData(int departmentId, string storedProcName, SqlParameter[] sqlParameters)
		{
			return UpdateData(storedProcName, sqlParameters);
		}

		public static int UpdateByQuery(int departmentId, string query)
		{
			return UpdateByQuery(query);
		}

		public static int Insert(Guid OrgId, Guid InstId, string CompanyName)
		{
			SqlParameter pDeptId = new SqlParameter("@company_id", SqlDbType.Int);
			pDeptId.Direction = ParameterDirection.Output;
			UpdateData("Addtbl_company", new SqlParameter[] { pDeptId, new SqlParameter("@company_name", CompanyName), new SqlParameter("@company_guid", InstId) }, OrgId);
			return (int)pDeptId.Value;
		}

		public static void UpdateName(Guid orgId, Guid instId, string companyName)
		{
			UpdateByQuery("UPDATE tbl_company SET company_name = @CompanyName WHERE company_guid = @CompanyGuid", new SqlParameter[] { new SqlParameter("@CompanyName", companyName), new SqlParameter("@CompanyGuid", instId) }, orgId);
		}

		public static DataRow SelectOne(int DeptID)
		{
			return SelectOne(Guid.Empty, DeptID);
		}

		public static int LookUpDeptIDByTicketID(Guid OrgID, int TicketID)
		{
			DataTable _dt = SelectByQuery("SELECT company_id FROM tbl_ticket WHERE Id=" + TicketID.ToString(), OrgID);
			if (_dt != null && _dt.Rows.Count > 0) return (int)_dt.Rows[0][0];
			else return 0;
		}

		public static DataRow SelectOneBase(Guid OrgID, int DeptId)
		{
			DataTable _dt = SelectByQuery("SELECT * FROM tbl_company WHERE company_id =" + DeptId.ToString(), OrgID);
			if (_dt != null && _dt.Rows.Count > 0) return _dt.Rows[0];
			else return null;
		}

		public static DataRow SelectOneBase(Guid OrgID, Guid instanceId)
		{
			DataTable _dt = SelectByQuery("SELECT * FROM tbl_company WHERE company_guid = '" + instanceId.ToString() + "'", OrgID);
			if (_dt != null && _dt.Rows.Count > 0) return _dt.Rows[0];
			else return null;
		}

		public static DataRow SelectOne(int DeptID, string DeptName)
		{
			return SelectOne(Data.DBAccess.GetCurrentOrgID(), DeptID, DeptName);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID)
		{
			return SelectRecord("sp_SelectCompany", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID) }, OrgID);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID, string DeptName)
		{
			return SelectRecord("sp_SelectCompany", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@CompanyName", DeptName) }, OrgID);
		}

		public static DataRow SelectOne(Guid companyGuid)
		{
			return SelectOne(Guid.Empty, companyGuid);
		}

		public static Guid GetOrganizationGuidByDepartment(Guid BWASupportOrgId, Guid DepartmentGuid)
		{
			DataTable _dt = Data.DBAccess.SelectByQuery("SELECT OrganizationId FROM Mc_Instance WHERE InstanceId='" + DepartmentGuid.ToString() + "'", BWASupportOrgId);
			if (_dt.Rows.Count > 0) return (Guid)_dt.Rows[0][0];
			else return Guid.Empty;
		}

		public static DataRow SelectOne(Guid OrgID, Guid companyGuid)
		{
			return SelectRecord("sp_SelectCompanyByGuid", new SqlParameter[] { new SqlParameter("@CompanyGuid", companyGuid) }, OrgID);
		}

		public static int SelectDepartmentId(Guid OrgId, Guid InstId)
		{
			return (int)SelectOne(OrgId, InstId)["company_id"];
		}

		public static byte GetPrintFontSize(int DeptID, int UserId)
		{
			SqlParameter _pPrintFontSize = new SqlParameter("@PrintFontSize", SqlDbType.TinyInt);
			_pPrintFontSize.Direction = ParameterDirection.Output;
			UpdateData("sp_GetPrintFontSize", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pPrintFontSize });
			if (_pPrintFontSize.Value == DBNull.Value) return 0;
			else return (byte)_pPrintFontSize.Value;
		}

		public static void CopyConfigurationSettings(Guid OrgIdFrom, int DeptIdFrom, Guid OrgIdTo, int DeptIdTo, int UserId)
		{
			//Create New Tickets Queue
			int _newTktQueueId = UnassignedQueues.Update(OrgIdTo, DeptIdTo, -1, "New Tickets", string.Empty, true);
			UnassignedQueues.AddQueueMember(OrgIdTo, DeptIdTo, _newTktQueueId, UserId, string.Empty);
			//Copy Ticket Levels
			DataTable _dtLevels = Levels.SelectAllFull(OrgIdFrom, DeptIdFrom);
			foreach (DataRow _row in _dtLevels.Rows)
			{
				Levels.Insert(OrgIdTo, DeptIdTo, _row["LevelName"].ToString(), _row["Description"].ToString(), _newTktQueueId, (byte)_row["tintRoutingType"]);
				if (!_row.IsNull("bitDefault") && (bool)_row["bitDefault"]) Levels.SetDefaultLevel(OrgIdTo, DeptIdTo, (byte)_row["tintLevel"]);
			}
			//Copy Creation Categories
			DataTable _dtCreationCats = CreationCategories.SelectAll(OrgIdFrom, DeptIdFrom);
			foreach (DataRow _row in _dtCreationCats.Rows) CreationCategories.Insert(OrgIdTo, DeptIdTo, UserId, _row["Name"].ToString(), (bool)_row["btInactive"]);
			//Copy Priorities
			DataTable _dtPrioritiesMap = new DataTable("PrioritiesMap");
			_dtPrioritiesMap.Columns.Add("PriorityIdFrom", typeof(int));
			_dtPrioritiesMap.Columns.Add("PriorityIdTo", typeof(int));
			_dtPrioritiesMap.PrimaryKey = new DataColumn[] { _dtPrioritiesMap.Columns[0] };
			DataTable _dtPriorities = Priorities.SelectAllFull(OrgIdFrom, DeptIdFrom);
			foreach (DataRow _row in _dtPriorities.Rows)
			{
				DataRow _newRow = _dtPrioritiesMap.NewRow();
				_newRow[0] = _row["Id"];
				_newRow[1] = Priorities.Update(OrgIdTo, DeptIdTo, -1, _row["Name"].ToString(), _row["Description"].ToString(), (bool)_row["btRstrctUsr"], (byte)(_row.IsNull("SLAPercentage") ? 0 : _row["SLAPercentage"]), (byte)_row["SLADays"], (byte)_row["SLAHours"], (byte)_row["SLAMinutes"], (bool)_row["btSkipSaturday"], (bool)_row["btSkipSunday"], (bool)_row["btSkipHolidays"], (bool)_row["btUseBusHours"], (byte)(_row.IsNull("SLAResponsePercentage") ? 0 : _row["SLAResponsePercentage"]), (byte)_row["SLAResponseDays"], (byte)_row["SLAResponseHours"], (byte)_row["SLAResponseMinutes"], (bool)_row["btResponseSkipSaturday"], (bool)_row["btResponseSkipSunday"], (bool)_row["btResponseSkipHolidays"], (bool)_row["btResponseUseBusHours"]);
				_dtPrioritiesMap.Rows.Add(_newRow);
			}
			//Copy Support Groups
			DataTable _dtSupportGroups = SupportGroups.SelectAll(OrgIdFrom, DeptIdFrom);
			DataTable _dtSupportGroupsMap = new DataTable("SupportGroupsMap");
			_dtSupportGroupsMap.Columns.Add("SupportGroupIdFrom", typeof(int));
			_dtSupportGroupsMap.Columns.Add("SupportGroupIdTo", typeof(int));
			_dtSupportGroupsMap.PrimaryKey = new DataColumn[] { _dtSupportGroupsMap.Columns[0] };
			foreach (DataRow _row in _dtSupportGroups.Rows) 
			{   
				DataRow _newRow = _dtSupportGroupsMap.NewRow();
				_newRow[0] = _row["id"];
				_newRow[1]=SupportGroups.Update(OrgIdTo, DeptIdTo, 0, _row["vchName"].ToString());
				_dtSupportGroupsMap.Rows.Add(_newRow);
			}
			//Copy Ticket Classes
			DataTable _dtClassesMap = new DataTable("ClassesMap");
			_dtClassesMap.Columns.Add("ClassIdFrom", typeof(int));
			_dtClassesMap.Columns.Add("ClassIdTo", typeof(int));
			_dtClassesMap.PrimaryKey = new DataColumn[] { _dtClassesMap.Columns[0] };
			DataTable _dtClasses = Classes.SelectAll(OrgIdFrom, DeptIdFrom);
			_dtClasses.DefaultView.Sort = "Level";
			foreach (DataRowView _rowv in _dtClasses.DefaultView)
			{
				int _pClassId = 0;
				if (!_rowv.Row.IsNull("ParentId")) _pClassId = (int)_dtClassesMap.Rows.Find(_rowv["ParentId"])[1];
				int _PriorityId = 0;
				if (!_rowv.Row.IsNull("intPriorityId")) _PriorityId = (int)_dtPrioritiesMap.Rows.Find(_rowv["intPriorityId"])[1];
				DataRow _newRow = _dtClassesMap.NewRow();
				_newRow[0] = _rowv["id"];
                _newRow[1] = Classes.Update(OrgIdTo, DeptIdTo, _pClassId, _rowv["Name"].ToString(), _newTktQueueId, (byte)_rowv["tintClassType"], (byte)_rowv["ConfigDistributedRouting"], -1, (bool)_rowv["bitRestrictToTechs"], _rowv["txtDesc"].ToString(), (bool)_rowv["bitAllowEmailParsing"], _PriorityId, (_rowv.Row.IsNull("tintLevelOverride") ? (byte)0 : (byte)_rowv["tintLevelOverride"]), (bool)_rowv["btInactive"], (bool)_rowv["KBPortal"], _rowv["KBPortalAlias"].ToString(), (_rowv.Row.IsNull("KBPortalOrder") ? -1 : (int)_rowv["KBPortalOrder"]));
				_dtClassesMap.Rows.Add(_newRow);
			}
			//Copy Resolution Categories
			DataTable _dtResolutionCats = Data.ResolutionCategories.SelectAll(OrgIdFrom, DeptIdFrom);
			foreach (DataRow _row in _dtResolutionCats.Rows)
			{
				Data.ResolutionCategories.Insert(OrgIdTo, DeptIdTo, UserId, _row["Name"].ToString(), (bool)_row["btInactive"], (bool)_row["btResolved"]);
			}
			//Copy Custom Fields
			DataTable _dtCustomFields = Data.CustomFields.SelectTicketCustomFields(DeptIdFrom, 0, OrgIdFrom);
			foreach (DataRow _row in _dtCustomFields.Rows)
			{
				int _classId = 0;
				if (!_row.IsNull("ClassID")) _classId = (int)_dtClassesMap.Rows.Find(_row["ClassID"])[1];
				Data.CustomFields.Insert(OrgIdTo, DeptIdTo, _row["Caption"].ToString(), (byte)_row["Type"], _row["Choices"].ToString(), (bool)_row["required"], (bool)_row["DisableUserEditing"], _row["DefaultValue"].ToString(), (int)_row["Position"], (bool)_row["IsForTech"], _classId);
			}
			//Copy Location Types
			DataTable _dtLocationTypes=Locations.LocationTypes.SelectAll(OrgIdFrom, DeptIdFrom);
			DataTable _dtPredefineLocationTypes = Locations.LocationTypes.SelectAll(OrgIdTo, DeptIdTo);
			DataTable _dtLocationTypesMap = new DataTable("LocationTypesMap");
			_dtLocationTypesMap.Columns.Add("LocationTypeIdFrom", typeof(int));
			_dtLocationTypesMap.Columns.Add("LocationTypeIdTo", typeof(int));
			_dtLocationTypesMap.PrimaryKey = new DataColumn[] { _dtLocationTypesMap.Columns[0] };
			foreach(DataRow _row in _dtLocationTypes.Rows)
			{
				DataRow _newRow = _dtLocationTypesMap.NewRow();
				_newRow[0] = _row["Id"];
				if (_row.IsNull("DId")) _newRow[1] = _row["Id"];
				else
				{
					_dtPredefineLocationTypes.DefaultView.RowFilter = "Not DId IS NULL AND Name='" + _row["Name"].ToString().Replace("'", "''") + "'";
					if (_dtPredefineLocationTypes.DefaultView.Count > 0) _newRow[1] = _dtPredefineLocationTypes.DefaultView[0]["Id"];
					else _newRow[1] = Locations.LocationTypes.Update(OrgIdTo, DeptIdTo, 0, _row["Name"].ToString(), (int)_row["HierarchyLevel"]);
				}
				_dtLocationTypesMap.Rows.Add(_newRow);
			}

			//Copy Internal Locations
			DataTable _dtIntLocations = Locations.SelectByQuery("SELECT *, 1 AS Level FROM Locations WHERE DId=" + DeptIdFrom.ToString()+" AND AccountId IS NULL", OrgIdFrom);
			_dtIntLocations.PrimaryKey = new DataColumn[] { _dtIntLocations.Columns[0] };
			DataTable _dtLocationsMap = new DataTable("LocationsMap");
			_dtLocationsMap.Columns.Add("LocationIdFrom", typeof(int));
			_dtLocationsMap.Columns.Add("LocationIdTo", typeof(int));
			_dtLocationsMap.PrimaryKey = new DataColumn[] { _dtLocationsMap.Columns[0] };
			foreach (DataRow _row in _dtIntLocations.Rows)
			{
				if (_row.IsNull("ParentId")) continue;
				int _level = (int)_row["Level"];
				int _parentId=(int)_row["ParentId"];
				DataRow _parentRow=_dtIntLocations.Rows.Find(_parentId);
				while (_parentId != 0 && _parentRow != null)
				{
					_level++;
					if (_parentRow.IsNull("ParentId")) _parentId = 0;
					else
					{
						_parentId = (int)_parentRow["ParentId"];
						_parentRow = _dtIntLocations.Rows.Find(_parentId);
					}
				}
				_row["Level"] = _level;
			}
			_dtIntLocations.DefaultView.Sort = "Level";
			foreach (DataRowView _rowv in _dtIntLocations.DefaultView)
			{
				int _pLocationId = 0;
				if (!_rowv.Row.IsNull("ParentId")) _pLocationId = (int)_dtLocationsMap.Rows.Find(_rowv["ParentId"])[1];
				int? _locTypeId=null;
				if (!_rowv.Row.IsNull("LocationTypeId")) _locTypeId=(int)_dtLocationTypesMap.Rows.Find(_rowv["LocationTypeId"])[1];
				bool? _status = null;
				if (!_rowv.Row.IsNull("Status")) _status = (bool)_rowv["Status"];
				int? _auditPeriodDays = null;
				if (!_rowv.Row.IsNull("AuditPeriodDays")) _auditPeriodDays = (int)_rowv["AuditPeriodDays"];
				DataRow _newRow = _dtLocationsMap.NewRow();
				_newRow[0] = _rowv["Id"];
				_newRow[1] = Locations.Update(OrgIdTo, DeptIdTo, 0, _pLocationId, 0, _locTypeId, _rowv["Name"].ToString(), _status, _rowv["Description"].ToString(), (bool)_rowv["IsDefault"], (bool)_rowv["CfgEnableAudit"], null, _auditPeriodDays);
				_dtLocationsMap.Rows.Add(_newRow);
			}
			//Copy Account Configuration
			DataTable _dtAccountsCfg = Accounts.SelectByQuery("SELECT * FROM AccountsCfg WHERE DId=" + DeptIdFrom, OrgIdFrom);
			string _insertAcctCfg=string.Empty;

			if (_dtAccountsCfg.Rows.Count > 0)
			{
				DataRow _row = _dtAccountsCfg.Rows[0];
				_insertAcctCfg = "INSERT INTO AccountsCfg (DId, intNextAcctNum, ";
				for (int i = 1; i < 16; i++) _insertAcctCfg += "btCust" + i.ToString() + "On, btCust" + i.ToString() + "Type, vchCust" + i.ToString() + "Cap, vchCust" + i.ToString() + "Options, btCust" + i.ToString() + "Req, vchCust" + i.ToString() + "Default, ";
				_insertAcctCfg += "btDateCust1On, vchDateCust1Cap, btDateCust1Req, btDateCust2On, vchDateCust2Cap, btDateCust2Req, isProject) ";
				_insertAcctCfg += "VALUES (" + DeptIdTo.ToString() + ", " + _row["intNextAcctNum"].ToString() + ", ";
				for (int i = 1; i < 16; i++) _insertAcctCfg += Functions.SqlBit(_row["btCust" + i.ToString() + "On"]) + ", " + Functions.SqlBit(_row["btCust" + i.ToString() + "Type"]) + ", '" + Functions.SqlStr(_row["vchCust" + i.ToString() + "Cap"]) + "', '" + Functions.SqlStr(_row["vchCust" + i.ToString() + "Options"]) + "', " + Functions.SqlBit(_row["btCust" + i.ToString() + "Req"]) + ", '" + Functions.SqlStr(_row["vchCust" + i.ToString() + "Default"]) + "', ";
				_insertAcctCfg += Functions.SqlBit(_row["btDateCust1On"]) + ", '" + Functions.SqlStr(_row["vchDateCust1Cap"]) + "', " + Functions.SqlBit(_row["btDateCust1Req"]) + ", " + Functions.SqlBit(_row["btDateCust2On"]) + ", '" + Functions.SqlStr(_row["vchDateCust2Cap"]) + "', " + Functions.SqlBit(_row["btDateCust2Req"]) + ", " + Functions.SqlBit(_row["isProject"]) + ")";
			}
			else _insertAcctCfg = "INSERT INTO AccountsCfg (DId) VALUES (" + DeptIdTo.ToString() + ")";
			Accounts.UpdateByQuery(_insertAcctCfg, OrgIdTo);

			//Copy Accounts
			DataTable _dtAccounts = Accounts.SelectByQuery("SELECT * FROM Accounts WHERE DId="+DeptIdFrom.ToString(), OrgIdFrom);
			DataTable _dtAccountsMap = new DataTable("AccountsMap");
			_dtAccountsMap.Columns.Add("AccountIdFrom", typeof(int));
			_dtAccountsMap.Columns.Add("AccountIdTo", typeof(int));
			_dtAccountsMap.PrimaryKey = new DataColumn[] { _dtAccountsMap.Columns[0] };
			foreach (DataRow _row in _dtAccounts.Rows)
			{
				int _suppGroupId=0;
				if (!_row.IsNull("SupGroupId")) _suppGroupId=(int)_dtSupportGroupsMap.Rows.Find(_row["SupGroupId"])[1];
				int _locId=0;
				if (!_row.IsNull("LocationId")) _locId=(int)_dtLocationsMap.Rows.Find(_row["LocationId"])[1];
				string[] _cust = new string[16];
				DateTime[] _dtcust = new DateTime[2];
				for (int i = 1; i < 16; i++) _cust[i-1]=_row["vchCust"+i.ToString()].ToString();
				for (int i = 1; i < 3; i++) 
				{
					if (!_row.IsNull("dtCust"+i.ToString())) _dtcust[i-1]=(DateTime)_row["dtCust"+i.ToString()];
					else _dtcust[i-1]=DateTime.MinValue;
				}
				string _timeZone = null;
				if (!_row.IsNull("TimeZone")) _timeZone = _row["TimeZone"].ToString();
				DataRow _newRow = _dtAccountsMap.NewRow();
				_newRow[0] = _row["Id"];
				_newRow[1] = Accounts.UpdateAccount(OrgIdTo, DeptIdTo, UserId, 0, _row["vchName"].ToString(), (bool)_row["btActive"], (bool)_row["btOrgAcct"], UserId, _suppGroupId, _locId, _row["vchEmailSuffix"].ToString(), _row["vchAcctNum"].ToString(), _row["vchRef1Num"].ToString(), _row["vchRef2Num"].ToString(), (bool)_row["AccLevelTimeTracking"], _cust, _dtcust, _row["Address1"].ToString(), _row["Address2"].ToString(), _row["City"].ToString(), _row["State"].ToString(), _row["ZipCode"].ToString(), _row["Country"].ToString(), _timeZone, _row["Phone1"].ToString(), _row["Phone2"].ToString());
				_dtAccountsMap.Rows.Add(_newRow);
				//Copy Account Locations
				DataTable _dtAcctLocations = Locations.SelectByQuery("SELECT *, 1 AS Level FROM Locations WHERE DId=" + DeptIdFrom.ToString() + " AND AccountId=" + _row["Id"].ToString(), OrgIdFrom);
				_dtAcctLocations.PrimaryKey = new DataColumn[] { _dtAcctLocations.Columns[0] };
				foreach (DataRow _rowl in _dtAcctLocations.Rows)
				{
					if (_rowl.IsNull("ParentId")) continue;
					int _level = (int)_rowl["Level"];
					int _parentId = (int)_rowl["ParentId"];
					DataRow _parentRow = _dtIntLocations.Rows.Find(_parentId);
					while (_parentId != 0 && _parentRow != null)
					{
						_level++;
						if (_parentRow.IsNull("ParentId")) _parentId = 0;
						else
						{
							_parentId = (int)_parentRow["ParentId"];
							_parentRow = _dtIntLocations.Rows.Find(_parentId);
						}
					}
					_rowl["Level"] = _level;
				}
				_dtAcctLocations.DefaultView.Sort = "Level";
				foreach (DataRowView _rowv in _dtAcctLocations.DefaultView)
				{
					int _pLocationId = 0;
					if (!_rowv.Row.IsNull("ParentId")) _pLocationId = (int)_dtLocationsMap.Rows.Find(_rowv["ParentId"])[1];
					int? _locTypeId = null;
					if (!_rowv.Row.IsNull("LocationTypeId")) _locTypeId = (int)_dtLocationTypesMap.Rows.Find(_rowv["LocationTypeId"])[1];
					bool? _status = null;
					if (!_rowv.Row.IsNull("Status")) _status = (bool)_rowv["Status"];
					int? _auditPeriodDays = null;
					if (!_rowv.Row.IsNull("AuditPeriodDays")) _auditPeriodDays = (int)_rowv["AuditPeriodDays"];
					DataRow _newRowL = _dtLocationsMap.NewRow();
					_newRowL[0] = _rowv["Id"];
					_newRowL[1] = Locations.Update(OrgIdTo, DeptIdTo, 0, _pLocationId, (int)_newRow[1], _locTypeId, _rowv["Name"].ToString(), _status, _rowv["Description"].ToString(), (bool)_rowv["IsDefault"], (bool)_rowv["CfgEnableAudit"], null, _auditPeriodDays);
					_dtLocationsMap.Rows.Add(_newRowL);
				}
			}

			//Create email dropbox
			DataTable _dtDropBoxes = EmailDropBoxes.SelectAll(OrgIdFrom, DeptIdFrom);
			if (_dtDropBoxes.Rows.Count > 0)
			{
				foreach (DataRow _row in _dtDropBoxes.Rows)
				{
					int _classId = 0;
					if (!_row.IsNull("ClassId")) _classId = (int)_dtClassesMap.Rows.Find(_row["ClassId"])[1];
					int _pNormalId = 0;
					int _pHighId = 0;
					int _pLowId = 0;
                    byte _pLevel = 0;
					if (!_row.IsNull("NormalPriorityId")) _pNormalId = (int)_dtPrioritiesMap.Rows.Find(_row["NormalPriorityId"])[1];
					if (!_row.IsNull("HighPriorityId")) _pHighId = (int)_dtPrioritiesMap.Rows.Find(_row["HighPriorityId"])[1];
					if (!_row.IsNull("LowPriorityId")) _pLowId = (int)_dtPrioritiesMap.Rows.Find(_row["LowPriorityId"])[1];
                    if (!_row.IsNull("tintLevel")) _pLevel = (byte)_row["tintLevel"];
                    EmailDropBoxes.Update(OrgIdTo, DeptIdTo, 0, Micajah.Common.Bll.Support.GeneratePseudoUnique(), _row["DropBoxName"].ToString(), 0, _classId, _pLevel, 0, 0, 0, string.Empty);
				}
			}
			else EmailDropBoxes.Update(OrgIdTo, DeptIdTo, 0, Micajah.Common.Bll.Support.GeneratePseudoUnique(), "Default incoming emails dropbox", _newTktQueueId, 0, 0, 0, 0, 0, string.Empty);
		}

		public static void UpdateSerialNumberName(int DepartmentId, string SerialNumberName)
		{
			UpdateData(DepartmentId, "sp_UpdateSerialNumberName", new SqlParameter[] { new SqlParameter("@CompanyId", DepartmentId), new SqlParameter("@SerialNumberName", SerialNumberName) });
		}

		public static int UpdateLdapSettings(int DeptID, bool is_ldap_enable, bool is_ldap_mail_enable, bool is_ldap_auto_redirect_login_page, string ldap_local_url)
		{
			int _result = 0;

			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;

			SqlParameter _pGeneralLdapEnable = new SqlParameter("@btGeneralLdap", SqlDbType.Bit);
			_pGeneralLdapEnable.Value = is_ldap_enable;

			SqlParameter _pLdapMailEnable = new SqlParameter("@btLdap", SqlDbType.Bit);
			_pLdapMailEnable.Value = is_ldap_mail_enable;

			SqlParameter _pLdapAutoRedirectLoginPage = new SqlParameter("@btLdapARLP", SqlDbType.Bit);
			_pLdapAutoRedirectLoginPage.Value = is_ldap_auto_redirect_login_page;

			SqlParameter _pLdapLocalURL = new SqlParameter("@LdapLocalURL", DBNull.Value);
			if (ldap_local_url.Length > 0)
				_pLdapLocalURL.Value = ldap_local_url;

			UpdateData(DeptID, "sp_UpdateLdapSettings", new SqlParameter[] { _pRVAL, new SqlParameter("@DepartmentId", DeptID), _pGeneralLdapEnable, _pLdapMailEnable, _pLdapAutoRedirectLoginPage, _pLdapLocalURL });

			_result = (int)_pRVAL.Value;

			return _result;
		}

		public static int GetBusinessDayLength(Config cfg)
		{
			int _result = 1440;

			DateTime current_time = DateTime.UtcNow.Date;
			int hours = current_time.Hour;
			int mins = current_time.Minute;
			int secs = current_time.Second;
			int msecs = current_time.Millisecond;

			current_time = current_time.AddHours(-hours);
			current_time = current_time.AddMinutes(-mins);
			current_time = current_time.AddSeconds(-secs);
			current_time = current_time.AddMilliseconds(-msecs);

			DateTime start_time, end_time;
			start_time = current_time;
			end_time = current_time;

			hours = cfg.BusHourStart;
			mins = cfg.BusMinStart;

			start_time = start_time.AddHours(hours);
			start_time = start_time.AddMinutes(mins);

			hours = cfg.BusHourStop;
			mins = cfg.BusMinStop;

			end_time = end_time.AddHours(hours);
			end_time = end_time.AddMinutes(mins);

			if (end_time > start_time)
			{
				TimeSpan _ts = end_time - start_time;
				_result = (int)_ts.TotalMinutes;
			};

			return _result;
		}

		public static DataTable SelectHolidays(int DeptID, int Year)
		{
			return SelectRecords("sp_SelectHolidays", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@vchYear", Year.ToString()) });
		}

		public static DataTable SelectCompanyStatuses(Guid OrgID)
		{
			return SelectRecords("sp_SelectCompanyStatuses", OrgID);
		}

		public static DataTable SelectCompanyStatuses()
		{
			return SelectCompanyStatuses(Guid.Empty);
		}

		public static DataTable SelectApplicationRenewalReport(int[] bWAAccountIDs, int companyActiveStatus, int companyStatusID, DateTime startDate, DateTime endDate)
		{
			string sBWAAccountIDs = string.Empty;

			foreach (int bWAAccountID in bWAAccountIDs)
				sBWAAccountIDs += bWAAccountID.ToString() + ",";
			sBWAAccountIDs = sBWAAccountIDs.Trim(',');

			if (sBWAAccountIDs != string.Empty)
			{
				string sqlQuery = string.Format(@"SELECT [intBWAId] AS BWAID, [BWAAccountId], [ActiveStatus], [company_id] AS DeptID, [company_name] AS DeptName, [dtExpiration] AS RenewalDate, [AltEmail] AS AltLogin, [configLocationTracking] AS LocationTracking
							FROM [dbo].[CompanyView]
							WHERE [BWAAccountId] IN ({0}) AND [dtExpiration] BETWEEN '{1}' AND '{2}'
							", sBWAAccountIDs, Functions.FormatSQLDateTime(startDate), Functions.FormatSQLDateTime(endDate));

				if (companyActiveStatus >= 0)
					sqlQuery += " AND [ActiveStatus]=" + companyActiveStatus.ToString();

				if (companyStatusID > 0)
					sqlQuery += " AND [StatusID]=" + companyStatusID.ToString();

				sqlQuery += " ORDER BY [dtExpiration] ASC";

				return SelectByQuery(sqlQuery);
			}
			else
				return null;
		}

		public static DataTable SelectResellerReport(int[] bwaAccountIds, string[] excludeEmailSuffixes)
		{
			if (bwaAccountIds.Length == 0)
				return null;

			string inClause = string.Empty;
			foreach (int i in bwaAccountIds)
				inClause += i.ToString() + ",";
			inClause = inClause.Trim(',');


			string excludeSuffixesSQL = string.Empty;
			if (excludeEmailSuffixes != null)
			{
				excludeSuffixesSQL = " INNER JOIN dbo.tbl_Logins AS L ON LJ.login_id = L.id";
				foreach (string excludeEmailSuffix in excludeEmailSuffixes)
					excludeSuffixesSQL += " AND L.Email NOT LIKE '%@" + excludeEmailSuffix.Replace("'", "''") + "'";
			}


			string query = @"SELECT C.BWAAccountId, max(C.ActiveUsers) AS MaxLoginsInDept, count(*) AS NumberOfDepartments
							FROM
							(
								SELECT C.BWAAccountId
										-- selecting the number of active users in each department (excluding of specified suffixes)
									   ,(SELECT count(*) FROM tbl_LoginCompanyJunc AS LJ" + excludeSuffixesSQL + @" WHERE (company_id = C.company_id) AND (btUserInactive = 0) AND (LJ.UserType_Id <> 4)) AS ActiveUsers
								FROM  tbl_company AS C
								WHERE (BWAAccountId IN (" + inClause + @") AND C.ActiveStatus=1)
							) AS C
							GROUP BY C.BWAAccountId";

			return SelectByQuery(query);
		}

		public static void UpdateTechsHourlyRate(int DepartmentId, int TechId, decimal HourlyRate
			, bool IsBillable, string QBAccount)
		{
			SqlParameter _prmHourlyRate = new SqlParameter("@HourlyRate", DBNull.Value);
			if (HourlyRate >= 0)
			{
				_prmHourlyRate.Value = HourlyRate;
			}

			UpdateData(DepartmentId, "sp_UpdateTechConfigHourlyRate", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DepartmentId),
				new SqlParameter("@TechId", TechId),
				_prmHourlyRate,
				new SqlParameter("@IsBillable", (IsBillable ? 1 : 0)),
				new SqlParameter("@QBAccount", QBAccount)
			});
		}

		public static DataTable SelectTechsHourlyRates(int DepartmentId)
		{
			return SelectTechsHourlyRates(DepartmentId, InactiveStatus.DoesntMatter);
		}

		public static DataTable SelectTechsHourlyRates(int DepartmentId, InactiveStatus TechStatus)
		{
			SqlParameter[] _params = new SqlParameter[2];
			_params[0] = new SqlParameter("@DepartmentId", DepartmentId);
			_params[1] = new SqlParameter("@Inactive", SqlDbType.Bit);
			switch (TechStatus)
			{
				case InactiveStatus.DoesntMatter: _params[1].Value = DBNull.Value;
					break;
				case InactiveStatus.Active: _params[1].Value = 0;
					break;
				case InactiveStatus.Inactive: _params[1].Value = 1;
					break;
			}
			return SelectRecords("sp_SelectTechsHourlyRates", _params);
		}

		public static DataTable SelectTechListForPartsConfig(int DepartmentId)
		{
			return SelectRecords("sp_SelectTechListForPartsConfig", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
		}

		public static DataTable SelectPartsNotifyTechs(int DepartmentId)
		{
			return SelectRecords("sp_SelectPartsNotifyTechs", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
		}

		public static void AddPartsNotifyTech(int DepartmentId, int UserId)
		{
			SqlParameter pCode = new SqlParameter("@Code", 0);
			pCode.SqlValue = 0;

			UpdateData("sp_UpdatePartsNotifyTech", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DepartmentId),
				new SqlParameter("@UserId", UserId),
				pCode
			});
		}

		public static void DeletePartsNotifyTech(int DepartmentId, int UserId)
		{
			UpdateData("sp_UpdatePartsNotifyTech", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DepartmentId),
				new SqlParameter("@UserId", UserId),
				new SqlParameter("@Code", 1)
			});
		}


		public static DataRow SelectHoliday(int DeptID, int holidayId)
		{
			return SelectRecord("sp_SelectHoliday", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", holidayId) });
		}

		public static DataTable SelectCustomNames(int DepartmentId)
		{
			return SelectRecords("sp_SelectCompanyCustomNames", new SqlParameter[] { new SqlParameter("@DId", DepartmentId) });
		}

		public static DataTable SelectCustomNamesStore()
		{
			return SelectRecords("sp_SelectCustomNamesStore");
		}

		public static void UpdateCustomName(int DepartmentId, string TermNodeName, int TermVariantId)
		{
			UpdateData(DepartmentId, "sp_UpdateCustomName", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),
				new SqlParameter("@TermNodeName", TermNodeName),
				new SqlParameter("@TermVariantId", TermVariantId)
			});
			CustomNames.ClearCache(Guid.Empty, DepartmentId);
		}

		public static void UpdateTimeTrackingOptions(int DepartmentId, bool ForceTimeEntry, bool MultipleTimeEntry, bool AllowEnterTimeOnTktDetail, bool AllowStartStopTime, bool AllowRemainingHours, bool AllowTimeEntriesOnTktLog)
		{
			UpdateData(DepartmentId, "sp_UpdateTimeTrackingOptions", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),                
				new SqlParameter("@btForceTimeEntry", ForceTimeEntry),                
				new SqlParameter("@btMultipleTimeEntry", MultipleTimeEntry),                
				new SqlParameter("@btEnterTimeOnTktDtl", AllowEnterTimeOnTktDetail),
				new SqlParameter("@AllowStartStopTime", AllowStartStopTime),
				new SqlParameter("@AllowRemainingHours", AllowRemainingHours),
				new SqlParameter("@AllowTimeEntriesOnTktLog", AllowTimeEntriesOnTktLog)
			});
		}

		public static void UpdateGeneralOptions(int DepartmentId, int TicketReopenLimit, int? TechTicketReopenLimit, bool ReqClosureNote, bool AllowClosureDate, bool ForceTimeEntry,
			bool AllowCreationTime, bool AfterHoursAlert, bool SupportGroups, bool TicketIdMethod, bool CallCenterSupport, bool SubmissionCategory,
			bool MultipleTimeEntry, bool SignBlockTktPrn, bool CustomNames, bool AllowCCTktClosing, bool RemoteAssistance,
			bool SchedTktTech, int PrintFontSize, bool CustomFieldsForTech, string Currency, bool TechEscalateOnly, bool TechAbleToReopen,
			bool LimitTechs, bool DisableTransferTktsToCheckedOUTTechs, decimal? DefaultDistanceRate, bool AllowSelectAnyLocation,
			bool AllowSUsersToChooseAnyLogin, bool AllowSUserToChooseAnyLocation, bool DisallowTechsToCreateLocations, string emailSuffixes, bool ForceLowestClassNode, bool AllowTransferTicketUser, bool DisplayAddNewUserLink, bool RequireTktInitialPost)
		{
			SqlParameter pTicketReopenLimit = new SqlParameter("@intUTROL", TicketReopenLimit);
			if (TicketReopenLimit < 0)
				pTicketReopenLimit.Value = 90;

			SqlParameter pTechTicketReopenLimit = new SqlParameter("@intTTROL", TechTicketReopenLimit);
			if (TechTicketReopenLimit == null)
				pTechTicketReopenLimit.Value = DBNull.Value;
			else
				if (TechTicketReopenLimit.Value < 0)
					pTechTicketReopenLimit.Value = 90;

			SqlParameter pDefaultDistanceRate = new SqlParameter("@DefaultDistanceRate", SqlDbType.Money);

			if (DefaultDistanceRate != null)
				pDefaultDistanceRate.Value = DefaultDistanceRate;
			else
				pDefaultDistanceRate.Value = DBNull.Value;

			UpdateData(DepartmentId, "sp_UpdateGeneralOptions", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),
				pTicketReopenLimit,
				pTechTicketReopenLimit,
				new SqlParameter("@btReqClosureNote", ReqClosureNote),
				new SqlParameter("@btAllowClosureDate", AllowClosureDate),
				new SqlParameter("@btForceTimeEntry", ForceTimeEntry),
				new SqlParameter("@btAllowCreationTime", AllowCreationTime),
				new SqlParameter("@btAfterHoursAlert", AfterHoursAlert),
				new SqlParameter("@btSupportGroups", SupportGroups),
				new SqlParameter("@btTicketIdMethod", TicketIdMethod),
				new SqlParameter("@btCallCenterSupport", CallCenterSupport),
				new SqlParameter("@btSubmissionCategory", SubmissionCategory),
				new SqlParameter("@btMultipleTimeEntry", MultipleTimeEntry),
				new SqlParameter("@btSignBlockTktPrn", SignBlockTktPrn),
				new SqlParameter("@btCustomNames", CustomNames),
				new SqlParameter("@btAllowCCTktClosing", AllowCCTktClosing),
				new SqlParameter("@btRemoteAssistance", RemoteAssistance),
				new SqlParameter("@btSchedTktTech", SchedTktTech),
				new SqlParameter("@printFontSize", PrintFontSize),
				new SqlParameter("@btCustomFieldsForTech", CustomFieldsForTech),
				new SqlParameter("@charCurrency", (Currency != null && Currency != string.Empty)?Currency:"$"),
				new SqlParameter("@btCfgTEDO", TechEscalateOnly),
				new SqlParameter("@btCfgTROL", TechAbleToReopen),                
				new SqlParameter("@btCfgLimTechAsts", LimitTechs),
				new SqlParameter("@btCfgDTTCOT", DisableTransferTktsToCheckedOUTTechs),
				pDefaultDistanceRate,
				new SqlParameter("@btCfgAllowSelectAnyLocation", AllowSelectAnyLocation),
				new SqlParameter("@btAllowSUserToChooseAnyLogin", AllowSUsersToChooseAnyLogin),
				new SqlParameter("@btAllowSUserToChooseAnyLocation", AllowSUserToChooseAnyLocation),
				new SqlParameter("@btDisallowTechsToCreateLocations", DisallowTechsToCreateLocations),
				new SqlParameter("@emailSuffixes", emailSuffixes),
				new SqlParameter("@btCfgFLCN", ForceLowestClassNode),
				new SqlParameter("@btCfgATTU", AllowTransferTicketUser),
				new SqlParameter("@btDisplayAddUserLink", DisplayAddNewUserLink),                
				new SqlParameter("@btCfgRTIP", RequireTktInitialPost)
			});
		}

		public static int UpdateHoliday(int DeptID, int holidayId, string name, DateTime startDate, DateTime endDate)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_UpdateHoliday", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Id", holidayId),
				new SqlParameter("@Name", name),
				new SqlParameter("@dtStart", startDate),
				new SqlParameter("@dtStop", endDate),
				pReturnValue
			});
			return (int)pReturnValue.Value;
		}

		public static AssetsConfig SelectAssetUniqueCaptions(int DepartmentId)
		{
			return SelectAssetUniqueCaptions(Guid.Empty, DepartmentId);
		}

		public static AssetsConfig SelectAssetUniqueCaptions(Guid organizationId, int DepartmentId)
		{
			DataRow drAssetConfig = SelectRecord("sp_SelectAssetUniqueCaptions", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId) }, organizationId);
			AssetsConfig assetsConfig;

			if (drAssetConfig != null)
				assetsConfig = new AssetsConfig(drAssetConfig["AssetsUnique1Caption"].ToString(), drAssetConfig["AssetsUnique2Caption"].ToString(), drAssetConfig["AssetsUnique3Caption"].ToString(), drAssetConfig["AssetsUnique4Caption"].ToString(), drAssetConfig["AssetsUnique5Caption"].ToString(), drAssetConfig["AssetsUnique6Caption"].ToString(), drAssetConfig["AssetsUnique7Caption"].ToString());
			else
				assetsConfig = new AssetsConfig();
			return assetsConfig;
		}

		public static int GetBWAAccountId(int DepartmentId)
		{
			SqlParameter pBWAAccountId = new SqlParameter("@intBwaAcct", SqlDbType.Int);
			pBWAAccountId.Direction = ParameterDirection.Output;

			UpdateData("sp_SelectBwaAcctId", new SqlParameter[]{
				new SqlParameter("@DId", DepartmentId),
				pBWAAccountId
			});
			return pBWAAccountId.Value != DBNull.Value ? (int)pBWAAccountId.Value : 0;
		}

		public static int InsertHoliday(int DeptID, string name, DateTime startDate, DateTime endDate)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_UpdateHoliday", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Id", -1),
				new SqlParameter("@Name", name),
				new SqlParameter("@dtStart", startDate),
				new SqlParameter("@dtStop", endDate),
				pReturnValue
			});
			return (int)pReturnValue.Value;
		}

		public static void DeleteHoliday(int DeptID, int holidayId)
		{

			UpdateData("sp_DeleteHoliday", new SqlParameter[]{
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@Id", holidayId)
			});
		}

		public static void UpdateTimeLogHourlyRate(int departmentID, int userID, int taskTypeID)
		{
			UpdateData("sp_UpdateTimeLogHourlyRate", new SqlParameter[]{
				new SqlParameter("@DId", departmentID),
				new SqlParameter("@UId", userID),
				new SqlParameter("@TaskTypeID", taskTypeID)
			});
		}

		public static void UpdateTechConfigHourlyBillableRate(int DepartmentId, int TechId
			, decimal hourlyBillableRate)
		{
			SqlParameter _prmHourlyBillableRate = new SqlParameter("@configHourlyBillableRate", DBNull.Value);
			if (hourlyBillableRate >= 0)
			{
				_prmHourlyBillableRate.Value = hourlyBillableRate;
			}

			UpdateData(DepartmentId, "sp_UpdateTechConfigHourlyBillableRate", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DepartmentId),
				new SqlParameter("@TechId", TechId),
				_prmHourlyBillableRate
			});
		}

		public class Department
		{
			private Config m_Config = null;
			private CustomNames m_CustomNames = null;
			private Guid m_OrgID = Guid.Empty;
			private string m_PseudoOrgID = string.Empty;
			private Guid m_DeptGuid = Guid.Empty;
			private int m_DeptID = 0;
			private string m_PseudoDeptID = string.Empty;
			private string m_OrganizationName = string.Empty;
			private string m_InstanceName = string.Empty;
			private string m_Name = string.Empty;

			public Department()
			{
			}

			void InitDepartment(Guid OrgID, int DeptID)
			{
				m_DeptID = DeptID;
				DataRow _row = Companies.SelectOne(OrgID, DeptID);
				if (_row == null) return;
				m_Name = _row["company_name"].ToString().Trim(' ', '.');
				m_DeptGuid = Guid.Parse(_row["company_guid"].ToString());
				Micajah.Common.Bll.Instance _inst = OrgID != Guid.Empty ? Micajah.Common.Bll.Providers.InstanceProvider.GetInstance(m_DeptGuid, OrgID) : Micajah.Common.Bll.Providers.InstanceProvider.GetInstance(m_DeptGuid);
				m_PseudoDeptID = _inst.PseudoId;
				m_InstanceName = _inst.Name;
				m_OrgID = _inst.OrganizationId;
				m_PseudoOrgID = _inst.Organization.PseudoId;
				m_OrganizationName = _inst.Organization.Name;
				m_Config = new Config(OrgID, m_DeptGuid, DeptID);
				m_CustomNames = CustomNames.GetCustomNames(m_OrgID, DeptID);
			}

			public Department(int DeptID)
			{
				InitDepartment(Guid.Empty, DeptID);
			}

			public Department(Guid OrgID, int DeptID)
			{
				InitDepartment(OrgID, DeptID);
			}

			public int ID
			{
				get { return m_DeptID; }
				set { m_DeptID = value; }
			}

			public Guid InstanceID
			{
				get { return m_DeptGuid; }
				set { m_DeptGuid = value; }
			}

			public string PseudoID
			{
				get { return m_PseudoDeptID; }
				set { m_PseudoDeptID = value; }
			}

			public Guid OrganizationId
			{
				get { return m_OrgID; }
				set { m_OrgID = value; }
			}

			public string OrganizationPseudoId
			{
				get { return m_PseudoOrgID; }
				set { m_PseudoOrgID = value; }
			}

			public string OrganizationName
			{
				get { return m_OrganizationName; }
				set { m_OrganizationName = value; }
			}

			public string InstanceName
			{
				get { return m_InstanceName; }
				set { m_InstanceName = value; }
			}

			public string Name
			{
				get { return m_Name; }
				set { m_Name = value; }
			}

			public Config Config
			{
				get { return m_Config; }
			}

			public CustomNames CustomNames
			{
				get { return m_CustomNames; }
			}
		}
	}
}
