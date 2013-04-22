using System;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for WorklistFilters.
	/// </summary>
	public class Worklist : DBAccess
	{
		public enum BrowseColumn
		{
			Blank = -1,
			DateCreated = 11,
			EndUser = 0,
			Technician = 1,
			Account = 2,
			Location = 3,
			Class = 4,
			Priority = 5,
			Level = 6,
            Project = 8,
			SupportGroup = 9,
			Time = 12,
			TicketNumber = 7,
			Updated = 13,
            SLACompletion = 14,
            SLAResponse = 15
		}

		public enum SortMode
		{
			NotSet = -1,
			MyTickets = 0, //sort=mts
			MyTicketsAsUser = 1, //sort=mut
			MyTicketsAsUserAndTech = 2, //sort=muatt
			TechTickets = 3, //sort=tech
			TicketsAsUserNotTech = 4, //sort=user
			TicketsAsSuperUserNotTech = 5, //sort=susr
			SuperUserTickets = 6, //sort=susra
			SupportGroupTickets = 7, //sort=sgt
			MyTicketsAsAlternateTech = 8, //sort=mats
		}

		public enum TicketStatusMode
		{
			All,
			AllOpen,
			Open,
			Close,
			OnHold,
			PartsOnOrder,
			OpenClosed
		}


		public enum NewMessagesMode
		{
			NotSet = -1,
			UserAndTech = 0,
			User = 1,
			Technician = 2
		}

		public static string[] SelectBrowseColumns(int DeptID, int UserId)
		{
			DataRow _row = SelectRecord("sp_SelectWrkLstFilter", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId) });
			if (_row != null) return new string[] { _row["vchWrkLstFields"].ToString(), _row["vchWrkLstSort"].ToString() };
			else return new string[] { string.Empty, string.Empty };
		}

		public static void UpdateBrowseColumns(int DeptID, int UserId, string fields, string sort)
		{
			SqlParameter _pFilterEnabled = new SqlParameter("@btFilterEnabled", SqlDbType.Bit);
			_pFilterEnabled.Direction = ParameterDirection.InputOutput;
			_pFilterEnabled.Value = DBNull.Value;
			SqlParameter _pFields = new SqlParameter("@vchWrkLstFields", SqlDbType.VarChar, 30);
			_pFields.Direction = ParameterDirection.InputOutput;
			if (fields.Length > 0) _pFields.Value = fields;
			else _pFields.Value = DBNull.Value;
			SqlParameter _pSort = new SqlParameter("@vchWrkLstSort", SqlDbType.VarChar, 25);
			_pSort.Direction = ParameterDirection.InputOutput;
			if (sort.Length > 0) _pSort.Value = sort;
			else _pSort.Value = DBNull.Value;
			UpdateData("sp_SelectWrkLstFields", new SqlParameter[] { new SqlParameter("@Mode", "UFields"), new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pFilterEnabled, _pFields, _pSort });
		}

		public class ColumnsSetting
		{
			private BrowseColumn[] m_Col = new BrowseColumn[] { };
			private BrowseColumn[] m_SortCol = new BrowseColumn[] { };

			private bool[] m_SortOrder = new bool[] { false };

            private string[] m_SQLColName = new string[]{"dbo.fxGetUserName2(lgu.FirstName, lgu.LastName, lgu.Email) + CASE WHEN ISNULL(lgu.Title, '') = '' THEN '' ELSE '<br class=brVisible><span class=subTitle>' + lgu.Title + '</span>' END",
				"dbo.fxGetUserName2(lgt.FirstName, lgt.LastName, lgt.Email) + CASE WHEN ISNULL(lgt.Title, '') = '' THEN '' ELSE '<br class=brVisible><span class=subTitle>' + lgt.Title + '</span>' END",
				"acct.vchName",
				"dbo.fxGetUserLocationName(0, tkt.LocationId)",
				"dbo.fxGetFullClassName(0, tkt.class_id)",
				"pri.tintPriority",
				"tkt.tintLevel",
				"ISNULL(tkt.TicketNumberPrefix,'')+CAST(tkt.TicketNumber AS nvarchar(10))",
				"prj.Name",
				"sg.vchName",
				string.Empty,
				"tkt.CreateTime",
				"tkt.TotalHours",
				"tkt.UpdatedTime",
                "tkt.dtSLAComplete",
                "CASE WHEN (tkt.Created_id IS NOT NULL AND ISNULL(tkt.Created_id, 0) = tkt.Technician_id) OR tkt.btInitResponse = 1 THEN NULL ELSE tkt.dtSLAResponse END"
			};

			private string[] m_SQLColAlias = new string[]{"vchFullUserName",
				"vchFullTechName",
				"vchAcctName",
				"LocationName",
				"ClassName",
				"tintPriority",
				"tintLevel",
				"TicketNumberFull",
				"Project",
				"vchName",
				string.Empty,
				"CreateTime",
				"TotalHours",
				"Updated",
                "SLACompletion",
                "SLAResponse"
			};

			private bool _IsInit = false;

			public ColumnsSetting()
			{
				UserSetting _c = UserSetting.GetSettings("WrkList2");
				if (!_c.IsDefined || string.IsNullOrEmpty(_c["FLD"]) || string.IsNullOrEmpty(_c["OB"])) return;
				else InitObject(HttpUtility.UrlDecode(_c["FLD"]), HttpUtility.UrlDecode(_c["OB"]));
			}

			public ColumnsSetting(int DeptID, int UserID)
				: this()
			{
				if (_IsInit) return;
				string[] _arr = SelectBrowseColumns(DeptID, UserID);
				InitObject(_arr[0], _arr[1]);
				SaveToSession();
			}

			protected void InitObject(string fields, string sort)
			{
				if (fields.Length == 0) fields = "0,1,2,3,4,5,6"; //DEFAULT FIELDS
				string[] _arr = fields.Split(',');
				m_Col = new BrowseColumn[_arr.Length];
				for (int i = 0; i < _arr.Length; i++) m_Col[i] = (BrowseColumn)int.Parse(_arr[i]);
				if (sort.Length > 0)
				{
					_arr = sort.Split(',');
					m_SortCol = new BrowseColumn[_arr.Length];
					m_SortOrder = new bool[_arr.Length];
					for (int i = 0; i < _arr.Length; i++)
					{
						string[] _arrTemp = _arr[i].Split('-');
						m_SortCol[i] = (BrowseColumn)int.Parse(_arrTemp[0]);
						if (_arrTemp.Length > 1 && _arrTemp[1] == "DESC") m_SortOrder[i] = true;
						else m_SortOrder[i] = false;
					}
				}
				_IsInit = true;
			}

			protected void RedimColList(int index)
			{
				BrowseColumn[] _list = new BrowseColumn[index + 1];
				m_Col.CopyTo(_list, 0);
				for (int i = m_Col.Length; i <= index; i++) _list[i] = BrowseColumn.Blank;
				m_Col = _list;
			}

			protected void RedimSortColList(int index)
			{
				BrowseColumn[] _list = new BrowseColumn[index + 1];
				m_SortCol.CopyTo(_list, 0);
				for (int i = m_SortCol.Length; i <= index; i++) _list[i] = BrowseColumn.Blank;
				m_SortCol = _list;
				bool[] _list2 = new bool[index + 1];
				m_SortOrder.CopyTo(_list2, 0);
				m_SortOrder = _list2;
			}

			#region Public Properties

			public void SaveToSession()
			{
				Save(0, 0);
			}

			public void Save(int DeptID, int UserId)
			{
				UserSetting _c = UserSetting.GetSettings("WrkList2");
				string _fields = string.Empty;
				string _sort = string.Empty;
				foreach (BrowseColumn _bc in m_Col)
				{
					if (_bc != BrowseColumn.Blank)
					{
						if (_fields.Length > 0) _fields += "," + ((int)_bc).ToString();
						else _fields = ((int)_bc).ToString();
					}
				}
				for (int i = 0; i < m_SortCol.Length; i++)
				{
					if (m_SortCol[i] != BrowseColumn.Blank)
					{
						if (_sort.Length > 0) _sort += "," + ((int)m_SortCol[i]).ToString();
						else _sort = ((int)m_SortCol[i]).ToString();
						if (m_SortOrder[i]) _sort += "-DESC";
						else _sort += "-ASC";
					}
				}

				if (_fields.Length == 0) _fields = "8";

				_c["FLD"] = _fields;
				_c["OB"] = _sort;

				if (DeptID == 0 || UserId == 0) return;

				UpdateBrowseColumns(DeptID, UserId, _fields, _sort);
			}

			public BrowseColumn GetBrowseColumn(int index)
			{
				if (index >= 0 && index < m_Col.Length) return m_Col[index];
				else return BrowseColumn.Blank;
			}

			public void SetBrowseColumn(int index, BrowseColumn col)
			{
				if (index >= m_Col.Length) RedimColList(index);
				m_Col[index] = col;
			}

			public BrowseColumn GetSortColumn(int index)
			{
				if (index >= 0 && index < m_SortCol.Length) return m_SortCol[index];
				else return BrowseColumn.Blank;
			}

			public int GetSortColumn(BrowseColumn browseColumn)
			{
				if (browseColumn == BrowseColumn.Blank) return -1;

				for (int i = 0; i < m_SortCol.Length; i++)
					if (m_SortCol[i] == browseColumn)
						return i;

				return -1;
			}

			public void SetSortColumn(int index, BrowseColumn col)
			{
				if (index >= m_SortCol.Length) RedimSortColList(index);
				m_SortCol[index] = col;
			}

			public bool GetSortOrderDesc(int index)
			{
				if (index >= 0 && index < m_SortOrder.Length) return m_SortOrder[index];
				else return false;
			}

			public void SetSortOrderDesc(int index, bool desc)
			{
				if (index >= m_SortOrder.Length) RedimSortColList(index);
				m_SortOrder[index] = desc;
			}

			public string GetBrowseColCaption(BrowseColumn col)
			{
				switch (col)
				{
					case BrowseColumn.DateCreated:
						return "Date Created";
					case BrowseColumn.SupportGroup:
						return "Support Group";
					case BrowseColumn.EndUser:
						return "End User";
                    case BrowseColumn.SLACompletion:
                        return "SLA Completion";
                    case BrowseColumn.SLAResponse:
                        return "SLA Response";
					default:
						return col.ToString();
				}
			}

			public string GetColSQLName(BrowseColumn col)
			{
				int _index = (int)col;
				if (_index >= 0 && _index < m_SQLColName.Length) return m_SQLColName[_index];
				else return string.Empty;
			}

			public string GetColSQLAlias(BrowseColumn col)
			{
				int _index = (int)col;
				if (_index >= 0 && _index < m_SQLColName.Length) return m_SQLColAlias[_index];
				else return string.Empty;
			}

			public int BrowseColumnsCount
			{
				get { return m_Col.Length; }
			}

			public int SortColumnsCount
			{
				get { return m_SortCol.Length; }
			}
			#endregion
		}

		public class QueryFilter
		{
			private string m_SQLWhere = string.Empty;
			private int m_TktId = 0;
			private RelatedTickets.TicketRelationType m_TktRelationType = RelatedTickets.TicketRelationType.None;
			private int m_UserId = 0;
			private int m_TechId = 0;
			private bool m_IgnoreFilter = false;
			private NewMessagesMode m_NewMessagesMode = NewMessagesMode.NotSet;
			private bool m_ShowFollowUpTicketsOnly = false;
			private int m_AccId = 0;
			private int m_AccUserId = 0;
			private int m_AccLocationId = 0;
			private int m_LocationId = 0;
			private int m_FolderId = 0;
			private string m_SearchStatus = string.Empty;
			private SortMode m_SortMode = SortMode.NotSet;
			private TicketStatusMode m_TicketStatus = TicketStatusMode.All;
			private int m_PageIndex = 0;
			private int m_SortColIndex = -1;
			private bool m_IsSortColDesc = false;
			private bool m_IsUseSql = false;
			private string m_SortColSQLAlias = string.Empty;
			private bool m_IsPrintMode = false;
			private string reportReferrer = string.Empty;
			private string m_QSep = "&";
			private int m_ProjectID = 0;
            private string m_SQLJoin = string.Empty;

			public QueryFilter()
			{
				if (HttpContext.Current.Session == null) return;

				UserSetting _c = UserSetting.GetSettings("Worklist");
				if (!_c.IsDefined || string.IsNullOrEmpty(_c["QS"]))
				{
					m_SortMode = SortMode.MyTickets;
					return;
				}
				InitObject(HttpUtility.UrlDecode(_c["QS"]));
			}

			public QueryFilter(string querystring)
			{
				InitObject(querystring);
			}

			public QueryFilter(string querystring, string separator)
			{
				m_QSep = separator;
				InitObject(querystring);
			}

			private void InitObject(string querystring)
			{
				if (querystring.Length == 0) return;
				NameValueCollection query = new NameValueCollection();
				string[] _arr = querystring.Split(new string[] { m_QSep }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string _arrElem in _arr)
				{
					string[] _a = _arrElem.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
					if (_a[0].Length > 0) query.Add(_a[0], _a.Length > 1 ? _a[1] : string.Empty);
				}
				if (query["sql"] == "1")
				{
					m_IsUseSql = true;
					UserSetting _c = UserSetting.GetSettings("Worklist");
					if (_c.IsDefined && !string.IsNullOrEmpty(_c["Sql"])) m_SQLWhere = HttpUtility.UrlDecode(_c["Sql"]);
                    if (_c.IsDefined && !string.IsNullOrEmpty(_c["SqlJoin"])) m_SQLJoin = HttpUtility.UrlDecode(_c["SqlJoin"]);
				}
				switch (query["sort"])
				{
					case "mts":
						m_SortMode = SortMode.MyTickets;
						break;
					case "mut":
						m_SortMode = SortMode.MyTicketsAsUser;
						break;
					case "mats":
						m_SortMode = SortMode.MyTicketsAsAlternateTech;
						break;
					case "muatt":
						m_SortMode = SortMode.MyTicketsAsUserAndTech;
						break;
					case "tech":
						m_SortMode = SortMode.TechTickets;
						break;
					case "user":
						m_SortMode = SortMode.TicketsAsUserNotTech;
						break;
					case "susr":
						m_SortMode = SortMode.TicketsAsSuperUserNotTech;
						break;
					case "susra":
						m_SortMode = SortMode.SuperUserTickets;
						break;
					case "sgt":
						m_SortMode = SortMode.SupportGroupTickets;
						break;
				}
				string _status = query["statusId"] != null ? HttpUtility.UrlDecode(query["statusId"]) : (query["st"] != null ? HttpUtility.UrlDecode(query["st"]) : string.Empty);
				switch (_status)
				{
					case "open":
						m_TicketStatus = TicketStatusMode.Open;
						break;
					case "closed":
						m_TicketStatus = TicketStatusMode.Close;
						break;
					case "part":
						m_TicketStatus = TicketStatusMode.PartsOnOrder;
						break;
					case "on hold":
						m_TicketStatus = TicketStatusMode.OnHold;
						break;
					case "allopen":
						m_TicketStatus = TicketStatusMode.AllOpen;
						break;
					case "notclosed":
						m_TicketStatus = TicketStatusMode.AllOpen;
						break;
					case "all":
						m_TicketStatus = TicketStatusMode.All;
						break;
					case "oc":
						m_TicketStatus = TicketStatusMode.OpenClosed;
						break;
				}
				switch (query["nm"])
				{
					case "0":
						m_NewMessagesMode = NewMessagesMode.UserAndTech;
						break;
					case "1":
						m_NewMessagesMode = NewMessagesMode.User;
						break;
					case "2":
						m_NewMessagesMode = NewMessagesMode.Technician;
						break;
				}
				if (query["fu"] == "1") m_ShowFollowUpTicketsOnly = true;
				if (query["techid"] != null && query["techid"].Length > 0)
				{
					try { m_TechId = int.Parse(query["techid"]); }
					catch { }
				}
				if (query["uid"] != null && query["uid"].Length > 0)
				{
					try { m_UserId = int.Parse(query["uid"]); }
					catch { }
				}
				if (m_UserId == 0)
				{
					if (query["userId"] != null && query["userId"].Length > 0)
					{
						try { m_UserId = int.Parse(query["userId"]); }
						catch { }
					}
				}
				if (query["aid"] != null && query["aid"].Length > 0)
				{
					try { m_AccId = int.Parse(query["aid"]); }
					catch { }
				}
				if (query["auid"] != null && query["auid"].Length > 0)
				{
					try { m_AccUserId = int.Parse(query["auid"]); }
					catch { }
				}
				if (query["alid"] != null && query["alid"].Length > 0)
				{
					try { m_AccLocationId = int.Parse(query["alid"]); }
					catch { }
				}

				if (!string.IsNullOrEmpty(query["lid"]))
					int.TryParse(query["lid"], out m_LocationId);
				
				if (query["folderid"] != null && query["folderid"].Length > 0)
				{
					try { m_FolderId = int.Parse(query["folderid"]); }
					catch { }
				}
				if (query["if"] == "1") m_IgnoreFilter = true;
				if (query["pg"] != null && query["pg"].Length > 0)
				{
					try { m_PageIndex = int.Parse(query["pg"]); }
					catch { }
				}
				if (query["ob"] != null && query["ob"].Length > 0)
				{
					try { m_SortColIndex = int.Parse(query["ob"]); }
					catch { }
				}
				if (query["obd"] == "desc") m_IsSortColDesc = true;
				if (query["mo"] == "prn") m_IsPrintMode = true;

				if (query["pid"] != null && query["pid"].Length > 0)
				{
					try { m_ProjectID = int.Parse(query["pid"]); }
					catch { }
				}

				if (!string.IsNullOrEmpty(UserSetting.GetSettings("RPT")["ReportReferrerUrl"]))
					reportReferrer = HttpUtility.UrlEncode(UserSetting.GetSettings("RPT")["ReportReferrerUrl"]);
			}

			public string GetQueryString()
			{
				string _query = m_QSep;
				switch (m_SortMode)
				{
					case SortMode.MyTickets:
						_query += "sort=mts";
						break;
					case SortMode.MyTicketsAsAlternateTech:
						_query += "sort=mats";
						break;
					case SortMode.MyTicketsAsUser:
						_query += "sort=mut";
						break;
					case SortMode.MyTicketsAsUserAndTech:
						_query += "sort=muatt";
						break;
					case SortMode.TechTickets:
						_query += "sort=tech";
						break;
					case SortMode.TicketsAsUserNotTech:
						_query += "sort=user";
						break;
					case SortMode.TicketsAsSuperUserNotTech:
						_query += "sort=susr";
						break;
					case SortMode.SuperUserTickets:
						_query += "sort=susra";
						break;
					case SortMode.SupportGroupTickets:
						_query += "sort=sgt";
						break;
				}
				_query += m_QSep + "statusId=";
				switch (m_TicketStatus)
				{
					case TicketStatusMode.All:
						_query += "all";
						break;
					case TicketStatusMode.AllOpen:
						_query += "allopen";
						break;
					case TicketStatusMode.Open:
						_query += "open";
						break;
					case TicketStatusMode.Close:
						_query += "closed";
						break;
					case TicketStatusMode.PartsOnOrder:
						_query += "part";
						break;
					case TicketStatusMode.OnHold:
						_query += "on hold";
						break;
					case TicketStatusMode.OpenClosed:
						_query += "oc";
						break;
				}
				switch (m_NewMessagesMode)
				{
					case NewMessagesMode.UserAndTech:
						_query += m_QSep + "nm=0";
						break;
					case NewMessagesMode.User:
						_query += m_QSep + "nm=1";
						break;
					case NewMessagesMode.Technician:
						_query += m_QSep + "nm=2";
						break;
				}
				if (m_ShowFollowUpTicketsOnly) _query += m_QSep + "fu=1";
				if (m_TechId != 0) _query += m_QSep + "techid=" + m_TechId.ToString();
				if (m_UserId != 0) _query += m_QSep + "uid=" + m_UserId.ToString();
				if (m_AccId != 0) _query += m_QSep + "aid=" + m_AccId.ToString();
				if (m_AccUserId != 0) _query += m_QSep + "auid=" + m_AccUserId.ToString();
				if (m_AccLocationId != 0) _query += m_QSep + "alid=" + m_AccLocationId.ToString();
				if (m_LocationId != 0) _query += m_QSep + "lid=" + m_LocationId.ToString();
				if (m_FolderId != 0) _query += m_QSep + "folderid=" + m_FolderId.ToString();
				if (m_IgnoreFilter) _query += m_QSep + "if=1";
				if (m_PageIndex >= 0) _query += m_QSep + "pg=" + m_PageIndex.ToString();
				if (m_SortColIndex >= 0) _query += m_QSep + "ob=" + m_SortColIndex.ToString();
				if (m_IsSortColDesc) _query += m_QSep + "obd=desc";
				if (m_IsUseSql) _query += m_QSep + "sql=1";
				if (m_IsPrintMode) _query += m_QSep + "mo=prn";
				if (reportReferrer != string.Empty) _query += m_QSep + "report=" + reportReferrer;
				if (m_ProjectID != 0) _query += m_QSep + "pid=" + m_ProjectID.ToString();
				if (_query.Length > 0) _query = _query.Substring(1);
				return _query;
			}

			public string GetWorkListTitle(UserAuth usr)
			{
				string _title = string.Empty;

				string _acc = m_AccId != 0 ? usr.customNames.Account.FullSingular + " " : string.Empty;

				bool _secUser = usr.IsInRole(UserAuth.UserRole.Administrator, UserAuth.UserRole.Technician);

				string _status = string.Empty;
				if (m_TicketStatus == TicketStatusMode.All) _status = "All ";
				else if (m_TicketStatus == TicketStatusMode.AllOpen) _status = "All Open ";
				else if (m_TicketStatus == TicketStatusMode.Close) _status = "Closed ";
				else if (m_TicketStatus == TicketStatusMode.OnHold) _status = "On Hold ";
				else if (m_TicketStatus == TicketStatusMode.Open) _status = "Open ";
				else if (m_TicketStatus == TicketStatusMode.PartsOnOrder) _status = "Parts On Order ";
				else if (m_TicketStatus == TicketStatusMode.OpenClosed) _status = "Open + Closed ";

				switch (m_SortMode)
				{
					case SortMode.MyTickets:
						if (_secUser)
						{
							if (m_TicketStatus == TicketStatusMode.All || m_TicketStatus == TicketStatusMode.AllOpen)
								_title = "As " + usr.customNames.Technician.AbbreviatedSingular;
							else
								_title = _status.TrimEnd(' ');
						}
						else
						{
							if (m_TicketStatus == TicketStatusMode.AllOpen)
								_title = "All My Open " + usr.customNames.Ticket.FullPlural;
							else if (m_TicketStatus == TicketStatusMode.All)
								_title = _status + "My " + usr.customNames.Ticket.FullPlural;
							else
								_title = "My " + _status + usr.customNames.Ticket.FullPlural;
						}
						break;
					case SortMode.MyTicketsAsAlternateTech:
						_title = "As Alternate " + usr.customNames.Technician.AbbreviatedSingular;
						break;
					case SortMode.MyTicketsAsUser:
						_title = "As " + usr.customNames.EndUser.FullSingular;
						break;
					case SortMode.SupportGroupTickets:
						_title = _status + "Support Group's";
						break;
					default:
						_title = _status.TrimEnd(' ');
						break;
				}
				if (_title.Length == 0) _title = "Work List";
				return _title;
			}

			public void SaveToSession()
			{
				UserSetting _c = UserSetting.GetSettings("Worklist");
				_c["QS"] = HttpUtility.UrlEncode(GetQueryString());
				_c["Sql"] = HttpUtility.UrlEncode(m_SQLWhere);
                _c["SqlJoin"] = HttpUtility.UrlEncode(m_SQLJoin);
			}

			public string SQLWhere
			{
				get { return m_SQLWhere; }
				set { m_SQLWhere = value; }
			}

            public string SQLJoin
            {
                get { return m_SQLJoin; }
                set { m_SQLJoin = value; }
            }

			public int TicketId
			{
				get { return m_TktId; }
				set { m_TktId = value; }
			}

			public int UserId
			{
				get { return m_UserId; }
				set { m_UserId = value; }
			}

			public int TechnicianId
			{
				get { return m_TechId; }
				set { m_TechId = value; }
			}

			public bool IgnoreFilter
			{
				get { return m_IgnoreFilter; }
				set { m_IgnoreFilter = value; }
			}

			public NewMessagesMode ShowNewMessages
			{
				get { return m_NewMessagesMode; }
				set { m_NewMessagesMode = value; }
			}

			public bool ShowFollowUpTicketsOnly
			{
				get { return m_ShowFollowUpTicketsOnly; }
				set { m_ShowFollowUpTicketsOnly = value; }
			}

			public int AccountId
			{
				get { return m_AccId; }
				set { m_AccId = value; }
			}

			public int AccountUserId
			{
				get { return m_AccUserId; }
				set { m_AccUserId = value; }
			}

			public int AccountLocationId
			{
				get { return m_AccLocationId; }
				set { m_AccLocationId = value; }
			}

			public int LocationId
			{
				get { return m_LocationId; }
				set { m_LocationId = value; }
			}

			public int FolderId
			{
				get { return m_FolderId; }
				set { m_FolderId = value; }
			}

			public SortMode Sort
			{
				get { return m_SortMode; }
				set { m_SortMode = value; }
			}

			public TicketStatusMode TicketStatus
			{
				get { return m_TicketStatus; }
				set { m_TicketStatus = value; }
			}

			public RelatedTickets.TicketRelationType TicketRelation
			{
				get { return m_TktRelationType; }
				set { m_TktRelationType = value; }
			}

			public int PageIndex
			{
				get { return m_PageIndex; }
				set { m_PageIndex = value; }
			}

			public bool IsUseSql
			{
				get { return m_IsUseSql; }
				set { m_IsUseSql = value; }
			}

			public int SortColumnIndex
			{
				get { return m_SortColIndex; }
				set { m_SortColIndex = value; }
			}

			public bool IsSortColumnDesc
			{
				get { return m_IsSortColDesc; }
				set { m_IsSortColDesc = value; }
			}

			public string SortColumnSQLAlias
			{
				get { return m_SortColSQLAlias; }
				set { m_SortColSQLAlias = value; }
			}

			public bool IsPrintMode
			{
				get { return m_IsPrintMode; }
				set { m_IsPrintMode = value; }
			}

			public int ProjectID
			{
				get { return m_ProjectID; }
				set { m_ProjectID = value; }
			}
		}

		public class Filter
		{
			public enum FilterType
			{
				AllTickets = -1,
				TicketsAssignedToItems = 0,
				TicketsNOTAsignedToItems = 1
			}
			string m_Classes = string.Empty;
			bool m_IsIncludeClasses = true;
			string m_Locations = string.Empty;
			bool m_IsIncludeLocations = true;
			string m_Accounts = string.Empty;
			bool m_IsIncludeAccounts = true;
			string m_Statuses = string.Empty;
			string m_Levels = string.Empty;
			bool m_IsEnabled = false;
			FilterType m_Folders = FilterType.AllTickets;
			FilterType m_Projects = FilterType.AllTickets;
			string m_Priority = string.Empty;

			public Filter()
			{
				UserSetting _c = UserSetting.GetSettings("WrkList1");
				if (_c == null || _c.Values.Count < 8) return;
				InitObject(_c["CLF"], _c["LOF"], _c["ACF"], _c["STF"], _c["LVF"], _c["PRF"], _c["FilEna"] == "1" ? true : false, _c["FOF"], _c["PJF"]);
			}

			public Filter(int DepID, int UserId)
				: this()
			{
				if (_IsInit) return;
				DataRow _row = SelectFilterByUser(DepID, UserId);
				if (_row != null) InitObject(_row["vchClasses"].ToString(), _row["vchLocations"].ToString(), _row["vchAccounts"].ToString(), _row["vchStatus"].ToString(), _row["vchLevels"].ToString(), _row["vchPriority"].ToString(), (bool)_row["btFilterEnabled"], _row["btFolder"].ToString(), _row["btProject"].ToString());
				SaveToSession();
			}

			private bool _IsInit = false;

			private void InitObject(string classes, string locations, string accounts, string statuses, string levels, string priority, bool isenabled, string folders, string projects)
			{
				if (classes != null && classes.Length > 0)
				{
					if (classes.Substring(0, 1) == "0") m_IsIncludeClasses = false;
					m_Classes = classes;
				}
				if (locations != null && locations.Length > 0)
				{
					if (locations.Substring(0, 1) == "0") m_IsIncludeLocations = false;
					m_Locations = locations;
				}
				if (accounts != null && accounts.Length > 0)
				{
					if (accounts.Substring(0, 1) == "0") m_IsIncludeAccounts = false;
					m_Accounts = accounts;
				}
				if (statuses != null) m_Statuses = statuses;
				if (levels != null) m_Levels = levels;
				if (priority != null) m_Priority = priority;
				m_IsEnabled = isenabled;
				if (folders != null && folders.Length > 0)
				{
					if (folders == "1" || folders.ToLower() == "true") m_Folders = FilterType.TicketsNOTAsignedToItems;
					else if (folders == "0" || folders.ToLower() == "false") m_Folders = FilterType.TicketsAssignedToItems;
				}
				if (projects != null && projects.Length > 0)
				{
					if (projects == "1" || projects.ToLower() == "true") m_Projects = FilterType.TicketsNOTAsignedToItems;
					else if (projects == "0" || projects.ToLower() == "false") m_Projects = FilterType.TicketsAssignedToItems;
				}
				_IsInit = true;
			}

			public void SaveToSession()
			{
				UserSetting _c = UserSetting.GetSettings("WrkList1");
				_c["CLF"] = m_Classes;
				_c["LOF"] = m_Locations;
				_c["ACF"] = m_Accounts;
				_c["STF"] = m_Statuses;
				_c["LVF"] = m_Levels;
				_c["PRF"] = m_Priority;
				_c["FilEna"] = m_IsEnabled ? "1" : "0";
				switch (m_Folders)
				{
					case FilterType.AllTickets:
						_c["FOF"] = string.Empty;
						break;
					case FilterType.TicketsAssignedToItems:
						_c["FOF"] = "0";
						break;
					case FilterType.TicketsNOTAsignedToItems:
						_c["FOF"] = "1";
						break;
				}
				switch (m_Projects)
				{
					case FilterType.AllTickets:
						_c["PJF"] = string.Empty;
						break;
					case FilterType.TicketsAssignedToItems:
						_c["PJF"] = "0";
						break;
					case FilterType.TicketsNOTAsignedToItems:
						_c["PJF"] = "1";
						break;
				}
			}

			public void Save(int DeptID, int UserId)
			{
				SaveToSession();
				UpdateFilter(DeptID, UserId, m_Locations, m_Classes, m_Statuses, m_Priority, m_Levels, m_Accounts, m_Folders, m_Projects);
				UpdateFilterStatus(DeptID, UserId, m_IsEnabled);
			}

			public string Classes
			{
				get { return m_Classes; }
				set { m_Classes = value; }
			}

			public bool IsClasseInclude
			{
				get { return m_IsIncludeClasses; }
				set { m_IsIncludeClasses = value; }
			}

			public string Locations
			{
				get { return m_Locations; }
				set { m_Locations = value; }
			}

			public bool IsLocationsInclude
			{
				get { return m_IsIncludeLocations; }
				set { m_IsIncludeLocations = value; }
			}

			public string Accounts
			{
				get { return m_Accounts; }
				set { m_Accounts = value; }
			}

			public bool IsAccountsInclude
			{
				get { return m_IsIncludeAccounts; }
				set { m_IsIncludeAccounts = value; }
			}

			public string Statuses
			{
				get { return m_Statuses; }
				set { m_Statuses = value; }
			}

			public string Levels
			{
				get { return m_Levels; }
				set { m_Levels = value; }
			}

			public string Priority
			{
				get { return m_Priority; }
				set { m_Priority = value; }
			}

			public bool IsEnabled
			{
				get { return m_IsEnabled; }
				set { m_IsEnabled = value; }
			}

			public FilterType Folders
			{
				get { return m_Folders; }
				set { m_Folders = value; }
			}

			public FilterType Projects
			{
				get { return m_Projects; }
				set { m_Projects = value; }
			}
		}

		public static DataRow SelectFilterByUser(int DeptID, int UserID)
		{
			return SelectRecord("sp_SelectWrkLstFilter", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID) });
		}

		public static void UpdateFilter(int DeptID, int UserID, string locations, string classes, string status, string priority, string levels, string accounts, Filter.FilterType folder, Filter.FilterType project)
		{
			UpdateData("sp_UpdateWrkLstFilter",
				new SqlParameter[]{
									  new SqlParameter("@DId", DeptID),
									  new SqlParameter("@UId", UserID),
									  new SqlParameter("@vchLocations", locations),
									  new SqlParameter("@vchClasses", classes),
									  new SqlParameter("@vchStatus", status),
									  new SqlParameter("@vchPriority", priority),
									  folder==Filter.FilterType.AllTickets ? new SqlParameter("@btFolder", DBNull.Value) : (folder==Filter.FilterType.TicketsAssignedToItems ? new SqlParameter("@btFolder", false) : new SqlParameter("@btFolder", true)),
									  project==Filter.FilterType.AllTickets ? new SqlParameter("@btProject", DBNull.Value) : (project==Filter.FilterType.TicketsAssignedToItems ? new SqlParameter("@btProject", false) : new SqlParameter("@btProject", true)),
									  new SqlParameter("@vchLevels", levels),
									  new SqlParameter("@vchAccounts", accounts)});
		}

		public static void UpdateFilterStatus(int DeptID, int UserID, bool IsEnabled)
		{
			UpdateData("sp_UpdateWrkLstFilterStatus", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID), new SqlParameter("@btAction", IsEnabled) });
		}

        //Uses in WebApi
        public static DataTable SelectTicketsByFilter(Guid OrgId, int DeptId, int UserId, bool IsUser, bool IsTech, bool IsAltTech, string statuses, bool IsTechAdmin)
        {
            string _query = "SELECT 0 AS DaysOld, ISNULL(tkt.TicketNumberPrefix,'')+CAST(tkt.TicketNumber AS nvarchar(10)) AS TicketNumberFull, ";
            _query += "tkt.Id, tkt.Status, tkt.CreateTime, tkt.ClosedTime, tkt.class_id, tkt.location_id, tkt.LocationId, tkt.PriorityId, tkt.SerialNumber, tkt.Subject, tkt.Note, tkt.Workpad, tkt.CreationCatsId, tkt.TicketNumber, tkt.CustomXML, tkt.PartsCost, tkt.LaborCost, tkt.TravelCost, tkt.MiscCost, tkt.Created_id, ";
            _query += "lgcu.firstname AS created_firstname, lgcu.lastname AS created_lastname, lgcu.Email AS created_email, lgcu.MobileEmail AS created_mobileemail, lgcu.MobileEmailType AS created_mobileemailtype, lgcu.Phone AS created_phone, lgcu.MobilePhone AS created_mobilephone, ";
            _query += "tkt.dtSLAComplete, tkt.dtSLAResponse, tkt.btInitResponse, tkt.dtReqComp, tkt.ReqCompNote, tkt.dtFollowUp, tkt.FollowUpNote, tkt.dtSLAComplete, tkt.dtSLAResponse, tkt.btInitResponse, tkt.intSLACompleteUsed, 0 as intSLAResponseUsed, tkt.tintLevel, tl.LevelName, tkt.btViaEmailParser, ";
            _query += "tkt.user_id, lgu.Title AS user_title, lgu.firstname AS user_firstname, lgu.lastname AS user_lastname, lgu.Email AS user_email, lgu.MobileEmail AS user_mobileemail, lgu.MobileEmailType AS user_mobileemailtype, lgu.Phone AS user_phone, lgu.MobilePhone AS user_mobilephone, tlj1.btUserInactive as user_inactive, ";
            _query += "tlj2.Usertype_id AS technician_TypeId, tlj2.QueEmailAddress as vchQueEmailAddress, tkt.Technician_id, lgt.firstname AS technician_firstname, lgt.lastname AS technician_lastname, lgt.Email AS technician_email, lgt.MobileEmail AS technician_mobileemail, lgt.MobileEmailType AS technician_mobileemailtype, lgt.Phone AS technician_phone, lgt.MobilePhone AS technician_mobilephone, ";
            _query += "dbo.fxGetUserLocationName(" + DeptId.ToString() + ", tkt.LocationId) AS LocationName, dbo.fxGetFullClassName(" + DeptId.ToString() + ", tkt.class_id) AS class_name, tkt.folder_id AS FolderId, dbo.fxRecurseFolders(" + DeptId.ToString() + ", tkt.folder_id, 0, '') as FolderPath, cat.vchName AS CategoryName, p.tintPriority, p.Name as PriName, isnull(acct.id,-1) AS intAcctId, isnull(acct.vchName, co.company_name + ' (Internal)') as vchAcctName, tkt.AccountLocationId, ";
            _query += "dbo.fxGetUserLocationName(" + DeptId.ToString() + ", tkt.AccountLocationId) as AccountLocationName, tkt.ClosureNote, tkt.ResolutionCatsId, ISNULL(res.btResolved, ISNULL(tkt.btResolved, 0)) btResolved, res.vchName as ResolutionName, lguc.FirstName+' '+lguc.LastName ConfirmedBy, tkt.btConfirmed, tkt.dtConfirmed, tkt.vchConfirmedNote, tlj2.SupGroupId AS SupportGroupID, sg.vchName AS SupportGroupName, tkt.vchIdMethod AS TicketIdMethod, ";
            _query += "tkt.btHandledByCC AS btHandledByCallCentre, sc.vchName as SubmissionCategory, tkt.EmailCC, tkt.intTktTimeMin, tkt.TicketNumberPrefix, tkt.ProjectID, dbo.fxGetFullProjectName(" + DeptId.ToString() + ", tkt.ProjectID) AS ProjectName, tkt.NextStep, (SELECT COUNT(*) FROM RelatedTickets WHERE DId=" + DeptId.ToString() + " AND TicketId=tkt.id) AS RelatedTktsCount, tkt.TotalHours, tkt.RemainingHours, tkt.NextStepDate, tkt.SchedTicketID, tkt.UpdatedTime, tkt.PseudoId, tkt.EstimatedTime, ";
            _query += "tkt.KB, tkt.KBType, tkt.KBPublishLevel, tkt.KBSearchDesc, tkt.KBAlternateId, tkt.KBHelpfulCount, CASE WHEN ISNULL(c.KBPortalAlias, '') = '' THEN c.Name ELSE c.KBPortalAlias END AS KBPortalAlias";
            _query += ", tkt.newtechpost, tkt.newuserpost, tkt.dtFollowUp, dbo.fxSelectInitialPost(tkt.company_id,tkt.id) AS InitPost ";
            _query += " FROM tbl_ticket tkt";
            _query += " INNER JOIN tbl_LoginCompanyJunc tlj1 ON tlj1.company_id=" + DeptId.ToString() + " AND tlj1.id=tkt.user_id INNER JOIN tbl_Logins lgu ON lgu.id=tlj1.login_id";
            _query += " INNER JOIN tbl_LoginCompanyJunc tlj2 ON tlj2.company_id=" + DeptId.ToString() + " AND tlj2.id=tkt.technician_id INNER JOIN tbl_Logins lgt ON lgt.id=tlj2.login_id";
            _query += " LEFT OUTER JOIN tbl_class c ON c.company_id =" + DeptId.ToString() + " AND tkt.class_id = c.id";
            _query += " LEFT OUTER JOIN SupportGroups sg ON sg.DId=" + DeptId.ToString() + " AND sg.Id=tlj2.SupGroupId";
            _query += " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj3 ON tlj3.company_id=" + DeptId.ToString() + " AND tlj3.id=tkt.Created_id LEFT OUTER JOIN tbl_Logins lgcu ON lgcu.id=tlj3.login_id";
            _query += " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj4 ON tlj4.company_id=" + DeptId.ToString() + " AND tlj4.id=tkt.intConfirmedBy LEFT OUTER JOIN tbl_Logins lguc ON lguc.id=tlj4.login_id";
            _query += " LEFT OUTER JOIN ResolutionCats rc ON rc.DId=" + DeptId.ToString() + " AND rc.Id=tkt.ResolutionCatsId";
            _query += " LEFT OUTER JOIN Accounts acct ON acct.DId=" + DeptId.ToString() + " AND acct.id=tkt.intAcctId";
            _query += " LEFT OUTER JOIN Priorities p ON p.DId=" + DeptId.ToString() + " AND p.id=tkt.PriorityId";
            _query += " LEFT OUTER JOIN TktLevels tl ON tl.DId=" + DeptId.ToString() + " AND tl.tintLevel=tkt.tintLevel";
            _query += " LEFT OUTER JOIN CreationCats cat on cat.DId =" + DeptId.ToString() + " AND tkt.CreationCatsId = cat.id";
            _query += " LEFT OUTER JOIN ResolutionCats res ON res.DId=" + DeptId.ToString() + " AND res.Id=tkt.ResolutionCatsId";
            _query += " LEFT OUTER JOIN SubmissionCategories sc ON sc.Id=tkt.intSubmissionCatId";
            _query += " INNER JOIN tbl_company co ON co.company_id=tkt.company_id";
            _query += " LEFT OUTER JOIN TicketAssignment AS TA ON TA.DepartmentId = tkt.company_id AND TA.TicketId = tkt.Id AND TA.UserId = " + UserId.ToString() + " AND (TA.AssignmentType = 1 OR TA.AssignmentType = 2)  AND TA.IsPrimary = 0 AND TA.StopDate IS NULL";

            _query += " WHERE tkt.company_id=" + DeptId.ToString();

            if (IsUser && !IsTech && !IsAltTech) _query += " AND (tkt.user_id=" + UserId.ToString() + " OR TA.UserId=" + UserId.ToString() + ") AND (TA.assignmenttype = 1 OR TA.assignmenttype IS NULL)"; //AND tkt.technician_id<>" + UserId.ToString() + ";
            if (!IsUser && IsTech && !IsAltTech) _query += " AND tkt.technician_id=" + UserId.ToString() + " AND (TA.assignmenttype = 1 OR TA.assignmenttype IS NULL)"; //AND tkt.user_id <> " + UserId.ToString() + 
            if (!IsUser && !IsTech && IsAltTech) _query +=" AND TA.UserId = " + UserId.ToString() + " AND TA.UserId <> tkt.technician_id AND TA.assignmenttype = 2";
            if (IsUser && IsTech && !IsAltTech) _query +=" AND (tkt.user_id=" + UserId.ToString() + " OR TA.UserId=" + UserId.ToString() + " or tkt.technician_id=" + UserId.ToString() + ")  AND (TA.assignmenttype = 1 OR TA.assignmenttype IS NULL)";
            if (IsUser && !IsTech && IsAltTech) _query +=" AND (tkt.user_id=" + UserId.ToString() + " OR TA.UserId=" + UserId.ToString() + ") AND tkt.technician_id<>" + UserId.ToString();
            if (!IsUser && IsTech && IsAltTech) _query += " AND tkt.user_id<>" + UserId.ToString() + " AND (TA.assignmenttype <> 1 OR TA.assignmenttype IS NULL)";

            string status_query = string.Empty;
            foreach (string status in statuses.Split(','))
            {
                switch (status.ToLower())
                {
                    case "closed":
                        status_query += " OR tkt.status='Closed' ";
                        break;
                    case "open":
                        status_query += " OR tkt.status='Open' ";
                        break;
                    case "onhold":
                        status_query += " OR tkt.status='On Hold' ";
                        break;
                    case "partsonorder":
                        status_query += " OR tkt.status='Parts On Order' ";
                        break;
                }
            }

            if (!string.IsNullOrEmpty(status_query))
                _query += " AND (" + status_query.Substring(3) + ")";

            _query += " ORDER BY tkt.CreateTime ASC";

            return SelectByQuery(_query, OrgId);
        }

		//Uses in WebApi
		public static DataTable SelectTicketsByFilter(Guid OrgId, int DeptId, int UserId, QueryFilter qfilter, bool IsTechAdmin)
		{
            string _query = "SELECT 0 AS DaysOld, ISNULL(tkt.TicketNumberPrefix,'')+CAST(tkt.TicketNumber AS nvarchar(10)) AS TicketNumberFull, ";
			_query +="tkt.Id, tkt.Status, tkt.CreateTime, tkt.ClosedTime, tkt.class_id, tkt.location_id, tkt.LocationId, tkt.PriorityId, tkt.SerialNumber, tkt.Subject, tkt.Note, tkt.Workpad, tkt.CreationCatsId, tkt.TicketNumber, tkt.CustomXML, tkt.PartsCost, tkt.LaborCost, tkt.TravelCost, tkt.MiscCost, tkt.Created_id, ";
			_query +="lgcu.firstname AS created_firstname, lgcu.lastname AS created_lastname, lgcu.Email AS created_email, lgcu.MobileEmail AS created_mobileemail, lgcu.MobileEmailType AS created_mobileemailtype, lgcu.Phone AS created_phone, lgcu.MobilePhone AS created_mobilephone, ";
			_query +="tkt.dtSLAComplete, tkt.dtSLAResponse, tkt.btInitResponse, tkt.dtReqComp, tkt.ReqCompNote, tkt.dtFollowUp, tkt.FollowUpNote, tkt.dtSLAComplete, tkt.dtSLAResponse, tkt.btInitResponse, tkt.intSLACompleteUsed, 0 as intSLAResponseUsed, tkt.tintLevel, tl.LevelName, tkt.btViaEmailParser, ";
			_query +="tkt.user_id, lgu.Title AS user_title, lgu.firstname AS user_firstname, lgu.lastname AS user_lastname, lgu.Email AS user_email, lgu.MobileEmail AS user_mobileemail, lgu.MobileEmailType AS user_mobileemailtype, lgu.Phone AS user_phone, lgu.MobilePhone AS user_mobilephone, tlj1.btUserInactive as user_inactive, ";
			_query +="tlj2.Usertype_id AS technician_TypeId, tlj2.QueEmailAddress as vchQueEmailAddress, tkt.Technician_id, lgt.firstname AS technician_firstname, lgt.lastname AS technician_lastname, lgt.Email AS technician_email, lgt.MobileEmail AS technician_mobileemail, lgt.MobileEmailType AS technician_mobileemailtype, lgt.Phone AS technician_phone, lgt.MobilePhone AS technician_mobilephone, ";
			_query +="dbo.fxGetUserLocationName(" + DeptId.ToString()+", tkt.LocationId) AS LocationName, dbo.fxGetFullClassName("+DeptId.ToString()+", tkt.class_id) AS class_name, tkt.folder_id AS FolderId, dbo.fxRecurseFolders("+DeptId.ToString()+", tkt.folder_id, 0, '') as FolderPath, cat.vchName AS CategoryName, p.tintPriority, p.Name as PriName, isnull(acct.id,-1) AS intAcctId, isnull(acct.vchName, co.company_name + ' (Internal)') as vchAcctName, tkt.AccountLocationId, ";
			_query +="dbo.fxGetUserLocationName(" + DeptId.ToString()+", tkt.AccountLocationId) as AccountLocationName, tkt.ClosureNote, tkt.ResolutionCatsId, ISNULL(res.btResolved, ISNULL(tkt.btResolved, 0)) btResolved, res.vchName as ResolutionName, lguc.FirstName+' '+lguc.LastName ConfirmedBy, tkt.btConfirmed, tkt.dtConfirmed, tkt.vchConfirmedNote, tlj2.SupGroupId AS SupportGroupID, sg.vchName AS SupportGroupName, tkt.vchIdMethod AS TicketIdMethod, ";
            _query += "tkt.btHandledByCC AS btHandledByCallCentre, sc.vchName as SubmissionCategory, tkt.EmailCC, tkt.intTktTimeMin, tkt.TicketNumberPrefix, tkt.ProjectID, dbo.fxGetFullProjectName(" + DeptId.ToString() + ", tkt.ProjectID) AS ProjectName, tkt.NextStep, (SELECT COUNT(*) FROM RelatedTickets WHERE DId=" + DeptId.ToString() + " AND TicketId=tkt.id) AS RelatedTktsCount, tkt.TotalHours, tkt.RemainingHours, tkt.NextStepDate, tkt.SchedTicketID, tkt.UpdatedTime, tkt.PseudoId, tkt.EstimatedTime, ";
		    _query += "tkt.KB, tkt.KBType, tkt.KBPublishLevel, tkt.KBSearchDesc, tkt.KBAlternateId, tkt.KBHelpfulCount, CASE WHEN ISNULL(c.KBPortalAlias, '') = '' THEN c.Name ELSE c.KBPortalAlias END AS KBPortalAlias";
			_query += ", tkt.newtechpost, tkt.newuserpost, tkt.dtFollowUp, dbo.fxSelectInitialPost(tkt.company_id,tkt.id) AS InitPost ";
			_query += " FROM tbl_ticket tkt";
			_query += " INNER JOIN tbl_LoginCompanyJunc tlj1 ON tlj1.company_id=" + DeptId.ToString() + " AND tlj1.id=tkt.user_id INNER JOIN tbl_Logins lgu ON lgu.id=tlj1.login_id";
			_query += " INNER JOIN tbl_LoginCompanyJunc tlj2 ON tlj2.company_id=" + DeptId.ToString() + " AND tlj2.id=tkt.technician_id INNER JOIN tbl_Logins lgt ON lgt.id=tlj2.login_id";
		    _query += " LEFT OUTER JOIN tbl_class c ON c.company_id =" + DeptId.ToString() + " AND tkt.class_id = c.id";
			_query += " LEFT OUTER JOIN SupportGroups sg ON sg.DId="+ DeptId.ToString()+" AND sg.Id=tlj2.SupGroupId";
			_query += " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj3 ON tlj3.company_id=" + DeptId.ToString() + " AND tlj3.id=tkt.Created_id LEFT OUTER JOIN tbl_Logins lgcu ON lgcu.id=tlj3.login_id";
			_query += " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj4 ON tlj4.company_id=" + DeptId.ToString()+" AND tlj4.id=tkt.intConfirmedBy LEFT OUTER JOIN tbl_Logins lguc ON lguc.id=tlj4.login_id";
			_query += " LEFT OUTER JOIN ResolutionCats rc ON rc.DId=" + DeptId.ToString() + " AND rc.Id=tkt.ResolutionCatsId";
			_query += " LEFT OUTER JOIN Accounts acct ON acct.DId=" + DeptId.ToString() + " AND acct.id=tkt.intAcctId";
			_query += " LEFT OUTER JOIN Priorities p ON p.DId=" + DeptId.ToString() + " AND p.id=tkt.PriorityId";
			_query += " LEFT OUTER JOIN TktLevels tl ON tl.DId=" + DeptId.ToString()+" AND tl.tintLevel=tkt.tintLevel";
			_query += " LEFT OUTER JOIN CreationCats cat on cat.DId =" + DeptId.ToString()+" AND tkt.CreationCatsId = cat.id";
			_query += " LEFT OUTER JOIN ResolutionCats res ON res.DId=" + DeptId.ToString()+" AND res.Id=tkt.ResolutionCatsId";
			_query += " LEFT OUTER JOIN SubmissionCategories sc ON sc.Id=tkt.intSubmissionCatId";
			_query += " INNER JOIN tbl_company co ON co.company_id=tkt.company_id";
			if (qfilter.Sort == SortMode.MyTicketsAsAlternateTech)
				_query += " JOIN TicketAssignment AS TA ON TA.DepartmentId = tkt.company_id AND TA.TicketId = tkt.Id AND TA.UserId = " + UserId.ToString() + " AND TA.AssignmentType = " + ((int)Ticket.TicketAssignmentType.Technician).ToString() + " AND IsPrimary = 0 AND StopDate IS NULL";
			else 
				_query += " LEFT OUTER JOIN TicketAssignment AS TA ON TA.DepartmentId = tkt.company_id AND TA.TicketId = tkt.Id AND TA.UserId = " + UserId.ToString() + " AND TA.AssignmentType = " + ((int)Ticket.TicketAssignmentType.User).ToString() + " AND TA.IsPrimary = 0 AND TA.StopDate IS NULL";

			_query += " WHERE tkt.company_id=" + DeptId.ToString();

            if (qfilter.SQLWhere.Length > 0)
            {
                _query += Security.SQLInjectionBlock(qfilter.SQLWhere);
            }

			switch (qfilter.Sort)
			{
				case SortMode.MyTickets:
					if (IsTechAdmin) _query += " AND tkt.technician_id=" + UserId.ToString();
					else _query += " AND (tkt.user_id=" + UserId.ToString() + " OR TA.UserId=" + UserId.ToString() + ")";
					break;
				case SortMode.MyTicketsAsUser:
					_query += " AND tkt.user_id=" + UserId.ToString();
					break;
                case SortMode.MyTicketsAsUserAndTech:
                    if (!IsTechAdmin)
                    {
                        _query += " AND (tkt.user_id=" + UserId.ToString() + " OR tkt.technician_id=" +
                                  UserId.ToString() + " OR TA.UserId=" + UserId.ToString() + ")";
                    }
                    else
                    {
                        _query += " AND (tkt.user_id=" + UserId.ToString() + " OR tkt.technician_id=" +
                                  UserId.ToString() + ")";
                    }
                    break;
                case SortMode.TechTickets:
                    _query += " AND tkt.technician_id = " + qfilter.TechnicianId.ToString();
                    break;
                case SortMode.MyTicketsAsAlternateTech:
                    _query += " AND TA.UserId = " + UserId.ToString() + " AND TA.UserId <> tkt.technician_id";
                    break;
                case SortMode.TicketsAsUserNotTech:
                    if (IsTechAdmin)
                    {
                        _query += "  AND (tkt.user_id=" + qfilter.UserId.ToString() + " OR TA.UserId=" + UserId.ToString()
                            + ") AND tkt.technician_id<>" + qfilter.UserId.ToString();
                    }
                    else
                    {
                        _query += "  AND tkt.user_id=" + qfilter.UserId.ToString() + " AND tkt.technician_id<>" + qfilter.UserId.ToString();
                    }
                    break;
                case SortMode.SupportGroupTickets:
                    _query += " AND tlj2.SupGroupID=(SELECT SupGroupId FROM tbl_LoginCompanyJunc WHERE company_id=" + DeptId.ToString() + " AND id=" + (qfilter.UserId != 0 ? qfilter.UserId.ToString() : UserId.ToString()) + ")";
                    break;
                default:
                    if (qfilter.Sort == SortMode.SuperUserTickets || qfilter.Sort == SortMode.TicketsAsSuperUserNotTech)
                    {
                        string accQuery = " (tkt.intAcctId IN (SELECT AccountId FROM UserAccounts WHERE DepartmentId="
                            + DeptId.ToString() + " AND UserId=" +
                            UserId.ToString() + ") OR (tkt.intAcctId IS NULL AND ((SELECT COUNT(*) FROM UserAccounts WHERE DepartmentId="
                            + DeptId.ToString() + " AND UserId=" + UserId.ToString() + " AND AccountId IS NULL) > 0)))";
                        if (qfilter.Sort == SortMode.TicketsAsSuperUserNotTech) _query += "  AND tkt.user_id=" + qfilter.UserId.ToString() + " AND tkt.technician_id<>" + qfilter.UserId.ToString();
                    }
                    else
                    {
                        if (!IsTechAdmin) _query += " AND tkt.user_id=" + UserId.ToString();
                    }
                    if (qfilter.AccountId != 0)
                    {
                        if (qfilter.AccountId > 0)
                            _query += " AND tkt.intAcctId=" + qfilter.AccountId.ToString();
                        else
                        {
                            _query += " AND tkt.intAcctId IS NULL AND ISNULL(tkt.btNoAccount, 0)=" + (qfilter.AccountId == -2 ? "1" : "0");
                        }
                        if (qfilter.AccountUserId != 0) _query += " AND tkt.user_id=" + qfilter.AccountUserId.ToString();
                        if (qfilter.AccountLocationId != 0) _query += " AND tkt.AccountLocationId IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + DeptId.ToString() + ", " + qfilter.AccountLocationId.ToString() + "))";
                    }
                    else
                        if (qfilter.AccountLocationId != 0) // Location's tickets
                            _query += " AND tkt.intAcctId IS NULL AND tkt.AccountLocationId = " + qfilter.AccountLocationId.ToString();

                    if (qfilter.LocationId > 0)
                        _query += " AND tkt.LocationId = " + qfilter.LocationId.ToString();

                    if (qfilter.ProjectID > 0)
                    {
                        _query += " AND tkt.ProjectID=" + qfilter.ProjectID.ToString();
                    }
                    break;
			}

			if (qfilter.ShowNewMessages == NewMessagesMode.UserAndTech) _query += " AND ((tkt.NewUserPost=1 AND tkt.technician_id =" + UserId.ToString() + " AND tkt.user_id<>" + UserId.ToString() + ") OR (tkt.NewTechPost=1 AND  tkt.user_id=" + UserId.ToString() + " AND technician_id<>" + UserId.ToString() + "))";
			else if (qfilter.ShowNewMessages == NewMessagesMode.User) _query += " AND tkt.newuserpost=1";
			else if (qfilter.ShowNewMessages == NewMessagesMode.Technician) _query += " AND tkt.newtechpost=1";

			if (qfilter.FolderId != 0) _query += " AND tkt.folder_id=" + qfilter.FolderId.ToString();

			if (qfilter.ShowFollowUpTicketsOnly) _query += " AND tkt.dtFollowUp IS NOT NULL";

			switch (qfilter.TicketStatus)
			{
				case TicketStatusMode.AllOpen:
					_query += "  AND tkt.status<>'Closed'";
					break;
				case TicketStatusMode.Close:
					_query += " AND tkt.status='Closed'";
					break;
				case TicketStatusMode.Open:
					_query += " AND tkt.status='Open'";
					break;
				case TicketStatusMode.OnHold:
					_query += " AND tkt.status='On Hold'";
					break;
				case TicketStatusMode.PartsOnOrder:
					_query += " AND tkt.status='Parts On Order'";
					break;
				case TicketStatusMode.OpenClosed:
					_query += " AND (tkt.status='Open' OR tkt.status='Closed')";
					break;
			}

			_query += " ORDER BY ";
			switch (qfilter.SortColumnSQLAlias)
			{
				case "LocationName":
					_query += "dbo.fxGetUserLocationName(" + DeptId.ToString() + ", tkt.LocationId)";
					break;
				case "TicketNumberFull":
					_query += "tkt.TicketNumber";
					break;
				case "CreateTime":
					_query += "tkt.CreateTime";
					break;
				default:
					if (string.IsNullOrEmpty(qfilter.SortColumnSQLAlias)) _query += "tkt.CreateTime";
					else _query += qfilter.SortColumnSQLAlias;
					break;
			}
			_query += qfilter.IsSortColumnDesc ? " DESC" : " ASC";

			return SelectByQuery(_query, OrgId);
		}

        public static DataTable SelectTicketsByFilter(UserAuth usr, Config cfg, ColumnsSetting colset, Filter filter, QueryFilter qfilter)
        {
            return SelectTicketsByFilter(usr, cfg, colset, filter, qfilter, false, 0);
        }

		public static DataTable SelectTicketsByFilter(UserAuth usr, Config cfg, ColumnsSetting colset, Filter filter, QueryFilter qfilter, bool reqSortFilter, int bussinesDayLength)
		{
            string _query = "SELECT 0 as DaysOld, tkt.id as TicketId, tkt.Status, tkt.technician_id, tkt.user_id, tkt.TicketNumber, tkt.TicketNumberPrefix, ISNULL(tkt.TicketNumberPrefix,'')+CAST(tkt.TicketNumber AS nvarchar(10)) AS TicketNumberFull, tkt.CreateTime, tkt.ClosedTime";
			_query += ", tkt.newtechpost, tkt.newuserpost, tkt.dtFollowUp, tkt.subject, tkt.NextStep, tkt.NextStepDate, tkt.SchedTicketID";
			if (!qfilter.IsPrintMode) _query += ", ISNULL((SELECT TOP 1 1 FROM Mfs_File fsf WHERE fsf.OrganizationId='" + usr.InstanceID.ToString() + "' AND fsf.DepartmentId='" + usr.InstanceID.ToString() + "' AND fsf.LocalObjectId=CAST(tkt.id AS varchar(255))), 0) as files_count";
			else _query += ", dbo.fxSelectInitialPost(tkt.company_id,tkt.id) AS InitPost";
			if (cfg.ResolutionTracking) _query += ", ISNULL(tkt.btResolved, 0) btResolved, CASE ISNULL(tkt.btResolved, 0) WHEN 1 THEN 'Resolved' ELSE 'UnResolved' END+CASE WHEN NOT rc.vchName IS NULL AND LEN(rc.vchName)>0 THEN ' - '+rc.vchName ELSE '' END AS Resolution";
			if (cfg.ResolutionTracking && cfg.ConfirmationTracking) _query += ", CASE WHEN ISNULL(tkt.btResolved, 0)=1 THEN tkt.btConfirmed ELSE CAST (1 AS bit) END AS btConfirmed";
			else if (cfg.ConfirmationTracking) _query += ", tkt.btConfirmed";
			for (int i = 0; i < colset.BrowseColumnsCount; i++)
			{
				if (colset.GetColSQLName(colset.GetBrowseColumn(i)).Length == 0) continue;
				else if (colset.GetBrowseColumn(i) == BrowseColumn.TicketNumber) continue;
				switch (colset.GetBrowseColumn(i))
				{
					case BrowseColumn.Account:
						if (!cfg.AccountManager) continue;
						break;
					case BrowseColumn.Location:
						if (!cfg.LocationTracking) continue;
						break;
					case BrowseColumn.Class:
						if (!cfg.ClassTracking) continue;
						break;
					case BrowseColumn.Level:
						if (!cfg.TktLevels) continue;
						break;
					case BrowseColumn.Priority:
						if (!cfg.PrioritiesGeneral) continue;
						break;
					case BrowseColumn.SupportGroup:
						if (!cfg.SupportGroups) continue;
						break;
                    case BrowseColumn.Project:
                        if (!cfg.ProjectTracking || !cfg.EnableTicketToProjectRelation) continue;
                        break;
					case BrowseColumn.Time:
						if (!cfg.TimeTracking) continue;
						break;
				}
				if (colset.GetBrowseColumn(i) == BrowseColumn.Location) _query += ", dbo.fxGetUserLocationName(" + usr.lngDId.ToString() + ", tkt.LocationId) AS " + colset.GetColSQLAlias(colset.GetBrowseColumn(i));
                else if (colset.GetBrowseColumn(i) == BrowseColumn.Account) _query += ", ISNULL(" + colset.GetColSQLName(colset.GetBrowseColumn(i)) + ",  CASE WHEN ISNULL(tkt.btNoAccount, 0) = 0 THEN co.company_name + ' (Internal)' ELSE '' END) + CASE WHEN tkt.AccountLocationId IS NULL THEN '' ELSE '<br class=brVisible><span class=subTitle>' + dbo.fxGetUserLocationName(" + usr.lngDId.ToString() + ", tkt.AccountLocationId) + '</span>' END AS " + colset.GetColSQLAlias(colset.GetBrowseColumn(i));
				else if (colset.GetBrowseColumn(i) == BrowseColumn.Class) _query += ", dbo.fxGetFullClassName(" + usr.lngDId.ToString() + ", tkt.class_id) AS " + colset.GetColSQLAlias(colset.GetBrowseColumn(i));
				else if (colset.GetBrowseColumn(i) == BrowseColumn.Time) _query += ", tkt.TotalHours, tkt.RemainingHours, tkt.EstimatedTime";
                else if (colset.GetBrowseColumn(i) == BrowseColumn.SLAResponse) _query += ", " + colset.GetColSQLName(colset.GetBrowseColumn(i)) + " AS " + colset.GetColSQLAlias(colset.GetBrowseColumn(i)) + ", tkt.Created_id, tkt.btInitResponse";
				else _query += ", " + colset.GetColSQLName(colset.GetBrowseColumn(i)) + " AS " + colset.GetColSQLAlias(colset.GetBrowseColumn(i));
			}
			if (qfilter.TicketId != 0 && qfilter.TicketRelation != RelatedTickets.TicketRelationType.None)
			{
				_query += " FROM RelatedTickets RT INNER JOIN tbl_ticket tkt ON tkt.company_id=" + usr.lngDId.ToString() + " AND tkt.Id=RT.RelatedTicketId";
			}
			else _query += " FROM tbl_ticket tkt";
			_query += " INNER JOIN tbl_LoginCompanyJunc tlj1 ON tlj1.company_id=" + usr.lngDId.ToString() + " AND tlj1.id=tkt.user_id INNER JOIN tbl_Logins lgu ON lgu.id=tlj1.login_id";
			_query += " INNER JOIN tbl_LoginCompanyJunc tlj2 ON tlj2.company_id=" + usr.lngDId.ToString() + " AND tlj2.id=tkt.technician_id INNER JOIN tbl_Logins lgt ON lgt.id=tlj2.login_id";
            _query += " INNER JOIN tbl_company co ON co.company_id=tkt.company_id";
			if (cfg.ResolutionTracking) _query += " LEFT OUTER JOIN ResolutionCats rc ON rc.DId=" + usr.lngDId.ToString() + " AND rc.Id=tkt.ResolutionCatsId";
			if (cfg.AccountManager) _query += " LEFT OUTER JOIN Accounts acct ON acct.DId=" + usr.lngDId.ToString() + " AND acct.id=tkt.intAcctId";
			if (cfg.PrioritiesGeneral) _query += " LEFT OUTER JOIN Priorities pri ON pri.DId=" + usr.lngDId.ToString() + " AND pri.id=tkt.PriorityId";
			if (cfg.SupportGroups) _query += " LEFT OUTER JOIN SupportGroups sg ON sg.DId=" + usr.lngDId.ToString() + " AND sg.Id=tlj2.SupGroupId";
            if (cfg.ProjectTracking && cfg.EnableTicketToProjectRelation) _query += " LEFT OUTER JOIN Project prj ON prj.CompanyID=" + usr.lngDId.ToString() + " AND prj.ProjectID=tkt.ProjectID";

			if (qfilter.Sort == SortMode.MyTicketsAsAlternateTech)
				_query += " JOIN TicketAssignment AS TA ON TA.DepartmentId = tkt.company_id AND TA.TicketId = tkt.Id AND TA.UserId = " + usr.lngUId.ToString() + " AND TA.AssignmentType = " + ((int)Ticket.TicketAssignmentType.Technician).ToString() + " AND IsPrimary = 0 AND StopDate IS NULL";
			else 
				_query += " LEFT OUTER JOIN TicketAssignment AS TA ON TA.DepartmentId = tkt.company_id AND TA.TicketId = tkt.Id AND TA.UserId = " + usr.lngUId.ToString() + " AND TA.AssignmentType = " + ((int)Ticket.TicketAssignmentType.User).ToString() + " AND TA.IsPrimary = 0 AND TA.StopDate IS NULL";

            if (qfilter.SQLJoin.Length > 0) _query += Security.SQLInjectionBlock(qfilter.SQLJoin);

			if (qfilter.TicketId != 0 && qfilter.TicketRelation != RelatedTickets.TicketRelationType.None)
			{
				_query += " WHERE RT.DId=" + usr.lngDId.ToString() + " AND RT.TicketId=" + qfilter.TicketId.ToString() + " AND RelationType=" + ((int)qfilter.TicketRelation).ToString();
			}
			else _query += " WHERE tkt.company_id=" + usr.lngDId.ToString();

            if (qfilter.SQLWhere.Length > 0)
            {
                _query += Security.SQLInjectionBlock(qfilter.SQLWhere);
            }
            if (qfilter.SQLWhere.Length == 0 || reqSortFilter)
            {
                switch (qfilter.Sort)
                {
                    case SortMode.MyTickets:
                        if (usr.IsInRole(UserAuth.UserRole.StandardUser, UserAuth.UserRole.SuperUser)) _query += " AND (tkt.user_id=" + usr.lngUId.ToString() + " OR TA.UserId=" + usr.lngUId.ToString() + ")";
                        else _query += " AND tkt.technician_id=" + usr.lngUId;
                        break;
                    case SortMode.MyTicketsAsUser:
                        _query += " AND tkt.user_id=" + usr.lngUId.ToString();
                        break;
                    case SortMode.MyTicketsAsUserAndTech:
                        if (usr.IsInRole(UserAuth.UserRole.StandardUser, UserAuth.UserRole.SuperUser))
                        {
                            _query += " AND (tkt.user_id=" + usr.lngUId.ToString() + " OR tkt.technician_id=" +
                                      usr.lngUId.ToString() + " OR TA.UserId=" + usr.lngUId.ToString() + ")";
                        }
                        else
                        {
                            _query += " AND (tkt.user_id=" + usr.lngUId.ToString() + " OR tkt.technician_id=" +
                                      usr.lngUId.ToString() + ")";
                        }
                        break;
                    case SortMode.TechTickets:
                        _query += " AND tkt.technician_id = " + qfilter.TechnicianId.ToString();
                        break;
                    case SortMode.MyTicketsAsAlternateTech:
                        _query += " AND TA.UserId = " + usr.lngUId.ToString() + " AND TA.UserId <> tkt.technician_id";
                        break;
                    case SortMode.TicketsAsUserNotTech:
                        if (usr.IsInRole(UserAuth.UserRole.StandardUser, UserAuth.UserRole.SuperUser))
                        {
                            _query += "  AND (tkt.user_id=" + qfilter.UserId.ToString() + " OR TA.UserId=" + usr.lngUId.ToString()
                                + ") AND tkt.technician_id<>" + qfilter.UserId.ToString();
                        }
                        else
                        {
                            _query += "  AND tkt.user_id=" + qfilter.UserId.ToString() + " AND tkt.technician_id<>" + qfilter.UserId.ToString();
                        }
                        break;
                    case SortMode.SupportGroupTickets:
                        _query += " AND tlj2.SupGroupID=(SELECT SupGroupId FROM tbl_LoginCompanyJunc WHERE company_id=" + usr.lngDId.ToString() + " AND id=" + (qfilter.UserId != 0 ? qfilter.UserId.ToString() : usr.lngUId.ToString()) + ")";
                        break;
                    default:
                        if (qfilter.Sort == SortMode.SuperUserTickets || qfilter.Sort == SortMode.TicketsAsSuperUserNotTech)
                        {
                            string accQuery = " (tkt.intAcctId IN (SELECT AccountId FROM UserAccounts WHERE DepartmentId="
                                + usr.lngDId.ToString() + " AND UserId=" +
                                usr.lngUId.ToString() + ") OR (tkt.intAcctId IS NULL AND ((SELECT COUNT(*) FROM UserAccounts WHERE DepartmentId="
                                + usr.lngDId.ToString() + " AND UserId=" + usr.lngUId.ToString() + " AND AccountId IS NULL) > 0)))";
                            string locQuery = " tkt.LocationId IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList("
                                + usr.lngDId.ToString() + ", '" + usr.strGSUserRootLocationId + "'))";
                            switch (usr.sintGSUserType)
                            {
                                case 1://Account
                                    _query += " AND" + accQuery;
                                    break;
                                case 2://Loccation
                                    _query += " AND" + locQuery;
                                    break;
                                case 3://Account and Location
                                    _query += " AND (" + accQuery + " OR" + locQuery + ")";
                                    break;
                            }
                            if (qfilter.Sort == SortMode.TicketsAsSuperUserNotTech) _query += "  AND tkt.user_id=" + qfilter.UserId.ToString() + " AND tkt.technician_id<>" + qfilter.UserId.ToString();
                        }
                        else
                        {
                            if (usr.IsInRole(UserAuth.UserRole.StandardUser, UserAuth.UserRole.SuperUser)) _query += " AND tkt.user_id=" + usr.lngUId.ToString();
                        }
                        if (qfilter.AccountId != 0)
                        {
                            if (qfilter.AccountId > 0)
                                _query += " AND tkt.intAcctId=" + qfilter.AccountId.ToString();
                            else
                            {
                                _query += " AND tkt.intAcctId IS NULL AND ISNULL(tkt.btNoAccount, 0)=" + (qfilter.AccountId == -2 ? "1" : "0");
                            }
                            if (qfilter.AccountUserId != 0) _query += " AND tkt.user_id=" + qfilter.AccountUserId.ToString();
                            if (qfilter.AccountLocationId != 0) _query += " AND tkt.AccountLocationId IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + usr.lngDId.ToString() + ", " + qfilter.AccountLocationId.ToString() + "))";
                        }
                        else
                            if (qfilter.AccountLocationId != 0) // Location's tickets
                                _query += " AND tkt.intAcctId IS NULL AND tkt.AccountLocationId = " + qfilter.AccountLocationId.ToString();

                        if (qfilter.LocationId > 0)
                            _query += " AND tkt.LocationId = " + qfilter.LocationId.ToString();

                        if (qfilter.ProjectID > 0)
                        {
                            _query += " AND tkt.ProjectID=" + qfilter.ProjectID.ToString();
                        }
                        break;
                }
            }

			if (qfilter.ShowNewMessages == NewMessagesMode.UserAndTech) _query += " AND ((tkt.NewUserPost=1 AND tkt.technician_id =" + usr.lngUId.ToString() + " AND tkt.user_id<>" + usr.lngUId.ToString() + ") OR (tkt.NewTechPost=1 AND  tkt.user_id=" + usr.lngUId.ToString() + " AND technician_id<>" + usr.lngUId.ToString() + "))";
			else if (qfilter.ShowNewMessages == NewMessagesMode.User) _query += " AND tkt.newuserpost=1";
			else if (qfilter.ShowNewMessages == NewMessagesMode.Technician) _query += " AND tkt.newtechpost=1";

			if (cfg.Folders && qfilter.FolderId != 0) _query += " AND tkt.folder_id=" + qfilter.FolderId.ToString();

			if (qfilter.ShowFollowUpTicketsOnly) _query += " AND tkt.dtFollowUp IS NOT NULL";

			switch (qfilter.TicketStatus)
			{
				case TicketStatusMode.AllOpen:
					_query += "  AND tkt.status<>'Closed'";
					break;
				case TicketStatusMode.Close:
					_query += " AND tkt.status='Closed'";
					break;
				case TicketStatusMode.Open:
					_query += " AND tkt.status='Open'";
					break;
				case TicketStatusMode.OnHold:
					_query += " AND tkt.status='On Hold'";
					break;
				case TicketStatusMode.PartsOnOrder:
					_query += " AND tkt.status='Parts On Order'";
					break;
				case TicketStatusMode.OpenClosed:
					_query += " AND (tkt.status='Open' OR tkt.status='Closed')";
					break;
			}

			if (filter != null && filter.IsEnabled && !qfilter.IsUseSql && !qfilter.IgnoreFilter && (qfilter.Sort == SortMode.NotSet || qfilter.Sort == SortMode.MyTickets || qfilter.Sort == SortMode.MyTicketsAsUser) && qfilter.AccountId == 0 && qfilter.AccountLocationId == 0 && qfilter.AccountUserId == 0 && qfilter.FolderId == 0 && qfilter.TechnicianId == 0 && qfilter.UserId == 0) //begin filter
			{
				if (filter.Statuses.Length > 0) _query += " AND tkt.status IN (" + filter.Statuses + ")";
				if (cfg.LocationTracking && filter.Locations.Length > 0) _query += " AND tkt.LocationId" + (!filter.IsLocationsInclude ? " NOT" : string.Empty) + " IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList(" + usr.lngDId.ToString() + ",'" + filter.Locations + "'))";
				if (cfg.ClassTracking && filter.Classes.Length > 0) _query += " AND tkt.class_id" + (!filter.IsClasseInclude ? " NOT" : string.Empty) + " IN (SELECT Id FROM dbo.fxGetAllChildClassesFromList(" + usr.lngDId.ToString() + ",'" + filter.Classes + "'))";
				if (cfg.TktLevels && filter.Levels.Length > 0) _query += " AND tkt.tintLevel IN (" + filter.Levels + ")";
				if (cfg.PrioritiesGeneral && filter.Priority.Length > 0) _query += " AND pri.tintpriority" + filter.Priority;
				if (cfg.AccountManager && filter.Accounts.Length > 0) _query += " AND ISNULL(tkt.intAcctId,-1)" + (!filter.IsAccountsInclude ? " NOT" : string.Empty) + " IN (" + filter.Accounts + ")";
				if (cfg.Folders)
				{
					if (filter.Folders == Filter.FilterType.TicketsAssignedToItems) _query += " AND tkt.folder_id IS NOT NULL";
					else if (filter.Folders == Filter.FilterType.TicketsNOTAsignedToItems) _query += " AND tkt.folder_id IS NULL";
				}
				if (cfg.TimeTracking)
				{
					if (filter.Projects == Filter.FilterType.TicketsAssignedToItems) _query += " AND tkt.ProjectID IS NOT NULL";
					else if (filter.Projects == Filter.FilterType.TicketsNOTAsignedToItems) _query += " AND tkt.ProjectID IS NULL";
				}
			}

			//use global filters
			if (qfilter.Sort == SortMode.NotSet || qfilter.Sort == SortMode.TechTickets) _query += GlobalFilters.GlobalFiltersSqlWhere(usr, cfg, "tkt.", "tlj2.", "SupGroupID");

			if (qfilter.SortColumnIndex < 0)
			{
				string _order = string.Empty;
				for (int i = 0; i < colset.SortColumnsCount; i++)
				{
					if (colset.GetColSQLName(colset.GetSortColumn(i)).Length == 0) continue;

					switch (colset.GetSortColumn(i))
					{
						case BrowseColumn.Account:
							if (!cfg.AccountManager) continue;
							break;
						case BrowseColumn.Location:
							if (!cfg.LocationTracking) continue;
							break;
						case BrowseColumn.Class:
							if (!cfg.ClassTracking) continue;
							break;
						case BrowseColumn.Level:
							if (!cfg.TktLevels) continue;
							break;
						case BrowseColumn.Priority:
							if (!cfg.PrioritiesGeneral) continue;
							break;
						case BrowseColumn.SupportGroup:
							if (!cfg.SupportGroups) continue;
							break;
                        case BrowseColumn.Project:
                            if (!cfg.ProjectTracking || !cfg.EnableTicketToProjectRelation) continue;
                            break;
					}
					if (_order.IndexOf(colset.GetColSQLName(colset.GetSortColumn(i))) < 0)
					{
						if (colset.GetSortColumn(i) == BrowseColumn.Location) _order += " dbo.fxGetUserLocationName(" + usr.lngDId.ToString() + ", tkt.LocationId)";
						else if (colset.GetColSQLAlias(colset.GetSortColumn(i)) == "TicketNumberFull") _order += " tkt.TicketNumber";
						else _order += " " + colset.GetColSQLName(colset.GetSortColumn(i));
						if (colset.GetSortOrderDesc(i)) _order += " DESC,";
						else _order += " ASC,";
					}
				}
				if (colset.SortColumnsCount > 0 && _order.Length > 0)
				{
					_order = _order.Substring(0, _order.Length - 1);
					_query += " ORDER BY" + _order;
				}
			}
			else
			{
				_query += " ORDER BY ";
				switch (qfilter.SortColumnSQLAlias)
				{
					case "LocationName":
						_query += "dbo.fxGetUserLocationName(" + usr.lngDId.ToString() + ", tkt.LocationId)";
						break;
					case "TicketNumberFull":
						_query += "tkt.TicketNumber";
						break;
					case "CreateTime":
						_query += "tkt.CreateTime";
						break;
					default:
						_query += qfilter.SortColumnSQLAlias;
						break;
				}
				_query += qfilter.IsSortColumnDesc ? " DESC" : " ASC";
			}

            DataTable dt = SelectByQuery(_query);

            if (usr != null && dt != null && string.Compare(qfilter.SortColumnSQLAlias, "DaysOld", true) == 0)
            {
                Data.Tickets.TicketTimer m_TicketTimer = (Data.Tickets.TicketTimer)usr.tintTicketTimer;
                dt.Columns.Add(new DataColumn("DaysOldSort", typeof(long)));
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["Status"].ToString() == "Closed")
                    {
                        if (m_TicketTimer == Data.Tickets.TicketTimer.SLATimer)
                            dr["DaysOldSort"] = Data.Tickets.SelectTicketSLATime(usr.OrgID, usr.lngDId, Functions.DB2UserDateTime((DateTime)dr["CreateTime"]), Functions.DB2UserDateTime((DateTime)dr["ClosedTime"]));
                        else
                            dr["DaysOldSort"] = ((DateTime)dr["ClosedTime"] - (DateTime)dr["CreateTime"]).TotalMinutes;
                    }
                    else
                    {
                        if (m_TicketTimer == Data.Tickets.TicketTimer.SLATimer)
                            dr["DaysOldSort"] = Data.Tickets.SelectTicketSLATime(usr.lngDId, Functions.DB2UserDateTime((DateTime)dr["CreateTime"]), Functions.DB2UserDateTime(DateTime.UtcNow));
                        else
                            dr["DaysOldSort"] = (DateTime.UtcNow - (DateTime)dr["CreateTime"]).TotalMinutes;
                    }
                }

                DataView dv = dt.DefaultView;
                dv.Sort = "DaysOldSort" + (qfilter.IsSortColumnDesc ? " DESC" : " ASC");
                dt = dv.ToTable();
            }

            return dt;
		}
	}
}
