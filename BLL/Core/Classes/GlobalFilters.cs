using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Web;
using System.Collections;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for GlobalFilters.
	/// </summary>
	public class GlobalFilters: DBAccess
	{
		public enum FilterState
		{
			NotSet,
			EnabledGlobalFilters,
			LimitToAssignedTickets,
			DisabledReports
		}

		public enum FilterType
		{
			Locations=1,
			Classes=2,
			UnassignedQueue=3,
			SupportGroups=4,
			Levels=5,
			Accounts=6,
			GlobalFilterTypes=0
		}

		private FilterState m_State = FilterState.NotSet;
		private bool[] m_Type = new bool[] { false, false, false, false, false, false, false};

        public GlobalFilters(int DeptID, int UserID) : this(Guid.Empty, DeptID, UserID)
        {}

	    public GlobalFilters(Guid OrgId, int DeptID, int UserID)
		{
            DataRow _row = SelectState(OrgId, DeptID, UserID);
			if (_row == null) return;
			if ((bool)_row["btGlobalFilterEnabled"]) m_State = FilterState.EnabledGlobalFilters;
			else if ((bool)_row["btLimitToAssignedTkts"]) m_State = FilterState.LimitToAssignedTickets;
			else if ((bool)_row["btDisabledReports"]) m_State = FilterState.DisabledReports;
            DataTable _dt = SelectFilterByType(OrgId, DeptID, UserID, FilterType.GlobalFilterTypes);
			foreach (DataRow _r in _dt.Rows)
				if ((bool)_r["State"]) m_Type[(int)_r["Filter"]] = true;
		}

        public static DataTable SetFilter(int DeptID, int UserID, DataTable datatable, string filtercolname, FilterType type)
        {
            return  SetFilter(Guid.Empty, DeptID, UserID, datatable, filtercolname, type);
        }

	    public static DataTable SetFilter(Guid OrgId, int DeptID, int UserID, DataTable datatable, string filtercolname, FilterType type)
		{
			if (!IsFilterEnabled(OrgId, DeptID, UserID, FilterState.EnabledGlobalFilters)) return datatable;
            if (!IsFilterEnabled(OrgId, DeptID, UserID, type)) return datatable;
			DataView _dv=SelectFilterByType(DeptID, UserID, type, true).DefaultView;

			if (_dv.Count==0 || filtercolname==null || filtercolname.Length==0)
			{
				if((_dv.Count==0)&&(filtercolname.Length>0)) 
				{
					string _empty_filter=" IN (NULL)";
					datatable.DefaultView.RowFilter=filtercolname+_empty_filter;
				};

				return datatable;
			};
			
			string _filter=" IN ("+_dv[0][0].ToString();
			for (int i=1; i<_dv.Count; i++) _filter+=", "+_dv[i][0].ToString();
			_filter+=")";
			if (type == FilterType.Accounts)
			{
				foreach(DataColumn dCol in datatable.Columns)
				{
					if(dCol.ColumnName == "AcctRepId")
					{
						_filter += " OR AcctRepId = " + UserID;
					}
				}
			}
			datatable.DefaultView.RowFilter=filtercolname+_filter;
			return datatable;
		}

		public bool IsFilterEnabled(params FilterState[] state)
		{
			foreach (FilterState _fs in state)
				if (_fs == m_State) return true;
			return false;
		}

		public bool IsFilterEnabled(params FilterType[] type)
		{
			foreach (FilterType _ft in type)
				if (m_Type[(int)_ft]) return true;
			return false;
		}

		public static bool IsFilterContains(int DeptID, int UserID, FilterType type, int ID)
		{
			DataTable _dt = SelectFilterByType(DeptID, UserID, type);
			if (type == FilterType.Locations)
			{
				DataTable _dtl = Data.Locations.SelectParentLocations(DeptID, ID);
				string _f = string.Empty;
				foreach (DataRow _row in _dtl.Rows)
				{
					if (_f.Length > 0) _f += ", " + _row["Id"].ToString();
					else _f = _row["Id"].ToString();
				}
				if (_f.Length > 0)
				{
					_dt.DefaultView.RowFilter = "ID IN (" + _f + ")";
				}
				else return true;
			}
			else if (type == FilterType.Classes)
			{
				DataTable _dtc = Data.Classes.SelectAllParent(DeptID, ID);
				string _f = string.Empty;
				foreach (DataRow _row in _dtc.Rows)
				{
					if (_f.Length > 0) _f += ", " + _row["Id"].ToString();
					else _f = _row["Id"].ToString();
				}
				if (_f.Length > 0)
				{
					_dt.DefaultView.RowFilter = "ID IN (" + _f + ")";
				}
				else return true;
			}
			else _dt.DefaultView.RowFilter = "ID=" + ID.ToString() + "AND State=" + true.ToString();
			if (_dt.DefaultView.Count > 0) return true;
			else return false;
		}

        public static bool IsFilterEnabled(int DeptID, int UserID, params FilterState[] state)
        {
            return IsFilterEnabled(Guid.Empty, DeptID, UserID, state);
        }

	    public static bool IsFilterEnabled(Guid OrgId, int DeptID, int UserID, params FilterState[] state)
		{
            DataRow _row = SelectState(OrgId, DeptID, UserID);
			if (_row==null) return false;
			for (int i=0; i<state.Length; i++)
			{
				switch (state[i])
				{
					case FilterState.DisabledReports:
						if ((bool)_row["btDisabledReports"]) return true;
						break;
					case FilterState.EnabledGlobalFilters:
						if ((bool)_row["btGlobalFilterEnabled"]) return true;
						break;
					case FilterState.LimitToAssignedTickets:
						if ((bool)_row["btLimitToAssignedTkts"]) return true;
						break;
				}
			}
			return false;
		}
        public static bool IsFilterEnabled(int DeptID, int UserID, params FilterType[] type)
        {
           return IsFilterEnabled(Guid.Empty, DeptID, UserID, type);
        }

	    public static bool IsFilterEnabled(Guid OrgId, int DeptID, int UserID, params FilterType[] type)
		{
            DataTable _dt = SelectFilterByType(OrgId, DeptID, UserID, FilterType.GlobalFilterTypes);
			for (int i=0; i<type.Length; i++)
			{
				foreach (DataRow _row in _dt.Rows)
					if ((bool)_row["State"] && (FilterType)Enum.Parse(typeof(FilterType),_row["Filter"].ToString(),true)==type[i]) return true;
			}
			return false;
		}

        public static DataRow SelectState(int DeptID, int UserID)
        {
            return  SelectState(Guid.Empty, DeptID, UserID);
        }

	    public static DataRow SelectState(Guid OrgId, int DeptID, int UserID)
		{
            return SelectRecord("sp_SelectUserDetails", new SqlParameter[] { new SqlParameter("@Did", DeptID), new SqlParameter("@Id", UserID) }, OrgId);
		}

        public static DataTable SelectFilterByType(int DeptID, int UserID, FilterType type)
        {
            return SelectFilterByType(Guid.Empty, DeptID, UserID, (int)type);
        }
		public static DataTable SelectFilterByType(Guid OrgId, int DeptID, int UserID, FilterType type)
		{
            return SelectFilterByType(OrgId, DeptID, UserID, (int)type);
		}

        public static DataTable SelectFilterByType(int DeptID, int UserID, int type)
        {
            return SelectFilterByType(Guid.Empty, DeptID, UserID, type);
        }

	    public static DataTable SelectFilterByType(Guid OrgId, int DeptID, int UserID, int type)
		{
            return SelectRecords("sp_SelectUserGlobalFiltersByType", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID), new SqlParameter("@FilterTypeId", type) }, OrgId);
		}

        public static DataTable SelectFilterByType(int DeptID, int UserID, FilterType type, bool IsSelected)
        {
            return SelectFilterByType(Guid.Empty, DeptID, UserID, type, IsSelected);
        }

	    public static DataTable SelectFilterByType(Guid OrgId, int DeptID, int UserID, FilterType type, bool IsSelected)
		{
            DataTable _dt = SelectFilterByType(OrgId, DeptID, UserID, type);
			_dt.DefaultView.RowFilter="State="+IsSelected.ToString();
			return _dt;
		}

        public static string SelectFilterByTypeToString(int DeptID, int UserID, FilterType type)
        {
            return SelectFilterByTypeToString(Guid.Empty, DeptID, UserID, type);
        }

	    public static string SelectFilterByTypeToString(Guid OrgId, int DeptID, int UserID, FilterType type)
		{
			string _res = string.Empty;
            DataView _dv = SelectFilterByType(OrgId, DeptID, UserID, type, true).DefaultView;
			foreach (DataRowView _drv in _dv) _res += _drv["ID"].ToString()+",";
			if (_res.Length > 0) _res = _res.Remove(_res.Length - 1);
			return _res;
		}

		public static void UpdateState(int DeptID, int UserID, bool LimitToAssignedTkts, bool ReportsDisabled, bool GlobalFilterEnabled)
		{
			UpdateData("sp_UpdateUserGlobalFiltersState", 
				new SqlParameter[]{
									  new SqlParameter("@CompanyId", DeptID), 
									  new SqlParameter("@UserId", UserID), 
									  new SqlParameter("@btLimitToAssignedTkts", LimitToAssignedTkts), 
									  new SqlParameter("@btDisabledReports", ReportsDisabled), 
									  new SqlParameter("@btGlobalFilterEnabled", GlobalFilterEnabled)
								  });
		}

		public static void UpdateFilterByType(int UserID, int FilterDataID, bool State, FilterType type)
		{
			UpdateData("sp_UpdateUserGlobalFilterByType", 
				new SqlParameter[]{
									  new SqlParameter("@UId", UserID), 
									  new SqlParameter("@FilterDataId", FilterDataID), 
									  new SqlParameter("@State", State), 
									  new SqlParameter("@FilterTypeId", (int)type)
								  }); 

		}

		public static void ClearUserWorklistFilter(int DeptId, int UserID)
		{
			UpdateData("sp_UpdateWrkLstFilter",
				new SqlParameter[]{
									  new SqlParameter("@DId", DeptId), 
									  new SqlParameter("@UId", UserID)
								  });
			UpdateByQuery("sp_UpdateWrkLstFilterStatus "+DeptId.ToString()+", "+UserID.ToString()+", 0");
		}

		//Valeriy Gooz: Add read user global filter functions
		private static string GFilterString(UserAuth user_auth, FilterType FilterTypeID)
		{
			string result="";

			result=GFDataString(user_auth, FilterTypeID, "", "ID" );

			if (result.Length>1)			
				result=result.Substring(1, result.Length-2);
			else
				result="";

			return result;
		}

		private static string GFDataString(UserAuth user_auth, FilterType FilterID, string FilterData, string FieldName)
		{
			string result="";

			if(FilterData.Length>0)
				return FilterData;

			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@DId", user_auth.lngDId);
			parameters[1] = new SqlParameter("@UId", user_auth.lngUId);
			parameters[2] = new SqlParameter("@FilterTypeId", FilterID);

			//open database connection

			//fill the drop down box
			DataTable _dt = SelectRecords("sp_SelectUserGlobalFiltersByType", parameters);
			foreach (DataRow _row in _dt.Rows)
			{
				if(((bool)_row["State"]))
				{					
					result=result+",";
					result=result+_row[FieldName].ToString();					
				};				
			}

			result=result+",";

			//refill the parameters list to avoid an error
			for(int i=0;i<parameters.Length;i++) parameters[i] = new SqlParameter(parameters[i].ParameterName, parameters[i].Value);

			//close database connection

			return result;
		}

		private static bool AllowGFilterData(UserAuth user_auth, int ID, FilterType FilterTypeID)
		{
			bool result=true;
		
			string global_filter_string=GFDataString(user_auth, FilterType.GlobalFilterTypes, "", "Filter");
			string filter_string=","+((int)FilterTypeID).ToString()+",";
			string dynamic_global_filter_string=GFDataString(user_auth, FilterTypeID, "", "ID");
			string dynamic_filter_string=","+ID.ToString()+",";

			if(global_filter_string.IndexOf(filter_string)>=0)
			{	
				if(dynamic_global_filter_string.IndexOf(dynamic_filter_string)<0)
				{
					result=false;
				};
			};

			return result;
		}

		public static string GlobalFiltersSqlWhere(UserAuth user_auth, Config user_config, string TableTicketAlias, string TableGroupAlias, string FieldGroupIDAlias)
		{
			//TableTicketAlias: tkt.
			//TableGroupAlias: tlj2.
			//FieldGroupIDAlias: SupGroupID		
			//"tkt.", "tlj2.", "SupGroupID"

			if (!IsFilterEnabled(user_auth.lngDId, user_auth.lngUId, new FilterState[] { FilterState.EnabledGlobalFilters, FilterState.LimitToAssignedTickets }))
				return string.Empty;

			string strFxIncMyTkts = TableTicketAlias + "user_id=" + user_auth.lngUId.ToString() + " OR " + TableTicketAlias + "technician_id=" + user_auth.lngUId.ToString();

			if (user_auth.BitCheckGFilter("tkt")) return " AND (" + strFxIncMyTkts + ")";

			bool btCfgLTR=user_config.LocationTracking;
			bool btCfgCLT=user_config.ClassTracking;
			bool btCfgUAQ=user_config.UnassignedQue;
			bool btCfgESG=user_config.SupportGroups;
			bool btCfgLVL=user_config.TktLevels;
			bool btCfgACT=user_config.AccountManager;

			string strFxSql = string.Empty;

			if (btCfgLTR && !AllowGFilterData(user_auth, 0, FilterType.Locations))
			{
				if (GFilterString(user_auth, FilterType.Locations).Length>0)
					strFxSql = TableTicketAlias + "LocationId IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList("+user_auth.lngDId.ToString()+",'"+GFilterString(user_auth, FilterType.Locations)+"'))";
				else
					strFxSql=TableTicketAlias +"LocationId IS NULL";
			};

			if (btCfgCLT && !AllowGFilterData(user_auth, 0, FilterType.Classes))
			{
                if (strFxSql.Length > 0) strFxSql += " AND ";

				if (GFilterString(user_auth, FilterType.Classes).Length>0)
					strFxSql+=TableTicketAlias +"class_id IN (SELECT Id FROM dbo.fxGetAllChildClassesFromList("+user_auth.lngDId.ToString()+",'"+ GFilterString(user_auth, FilterType.Classes) +"'))";
				else
					strFxSql+=TableTicketAlias +"class_id IS NULL";
			};
				
			if (btCfgUAQ && !AllowGFilterData(user_auth, 0, FilterType.UnassignedQueue))
			{
                if (strFxSql.Length > 0) strFxSql += " AND ";

                if (GFilterString(user_auth, FilterType.UnassignedQueue).Length > 0)
					strFxSql+=TableTicketAlias +"technician_id IN ("+ GFilterString(user_auth, FilterType.UnassignedQueue) +")";
				else
					strFxSql+=TableTicketAlias +"technician_id IS NULL";
			};

			if (btCfgESG && !AllowGFilterData(user_auth, 0, FilterType.SupportGroups))
			{
                if (strFxSql.Length > 0) strFxSql += " AND ";

				if (GFilterString(user_auth, FilterType.SupportGroups).Length>0)
					strFxSql+=TableGroupAlias + FieldGroupIDAlias +" IN ("+ GFilterString(user_auth, FilterType.SupportGroups) +")";
				else
					strFxSql+=TableGroupAlias + FieldGroupIDAlias +" IS NULL";
			};

			if (btCfgLVL && !AllowGFilterData(user_auth, 0, FilterType.Levels))
			{
                if (strFxSql.Length > 0) strFxSql += " AND ";

				if (GFilterString(user_auth, FilterType.Levels).Length>0)
					strFxSql+=TableTicketAlias +"tintLevel IN ("+ GFilterString(user_auth, FilterType.Levels) +")";
				else
					strFxSql+=TableTicketAlias +"tintLevel IS NULL";
			};

			if (btCfgACT && !AllowGFilterData(user_auth, 0, FilterType.Accounts))
			{
                string filteredAccountsString = GFilterString(user_auth, FilterType.Accounts);
                if (!string.IsNullOrEmpty(filteredAccountsString))
                {
                    if (strFxSql.Length > 0) strFxSql += " AND ";

                    strFxSql+="(intAcctId IN (" + filteredAccountsString + ")" + (filteredAccountsString.Contains("-1") ? " OR intAcctId IS NULL" : string.Empty)+")";
                }
			};

            if (strFxSql.Length > 0) return " AND (" + strFxIncMyTkts + " OR (" + strFxSql + "))";
            return " AND (" + strFxIncMyTkts + ")";
		}
	}																																														   
}
