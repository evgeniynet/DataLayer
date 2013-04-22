using System;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace bigWebApps.bigWebDesk.Data
{
    public enum InactiveStatus : int
    {
        DoesntMatter = -1,
        Active = 0,
        Inactive = 1
    }

    public class Accounts : DBAccess
    {
        public enum ActiveStatus
        {
            Active = 1,
            Inactive = 0,
            NoFilter = -1
        }

        public enum ViewMode
        {
            MyAccounts = 0,
            SupportGroupAccounts = 1,
            AllAccounts = 2
        }

        public enum BrowseColumn
        {
            Blank = -1,
            AccName = 0,
            BWDAccRef = 1,
            OpenTickets = 2,
            DeptLocation = 3,
            AccRepName = 4,
            AccNumber = 5,
            EmailSuffix = 32,
            Ref1Number = 6,
            Ref2Number = 7,
            LocCity = 25,
            LocState = 26,
            LocPostalCode = 27,
            LocCountry = 28,
            LocTimeZone = 29,
            LocPhone1 = 30,
            LocPhone2 = 31,
            Custom1 = 8,
            Custom2 = 9,
            Custom3 = 10,
            Custom4 = 11,
            Custom5 = 12,
            Custom6 = 13,
            Custom7 = 14,
            Custom8 = 15,
            Custom9 = 16,
            Custom10 = 17,
            Custom11 = 18,
            Custom12 = 19,
            Custom13 = 20,
            Custom14 = 21,
            Custom15 = 22,
            CustomDate1 = 23,
            CustomDate2 = 24
        }

        public class ColumnsSetting
        {
            private BrowseColumn[] m_Col = new BrowseColumn[] { BrowseColumn.AccName, BrowseColumn.OpenTickets, BrowseColumn.DeptLocation, BrowseColumn.AccRepName, BrowseColumn.BWDAccRef };
            private BrowseColumn[] m_SortCol = new BrowseColumn[] { BrowseColumn.AccName };
            private string[] m_CustomCup = new string[17];
            private bool[] m_CustomOn = new bool[17];
            private ViewMode m_ViewMode = ViewMode.AllAccounts;

            private bool[] m_SortOrder = new bool[] { false };

            private string[] m_SQLColName;
            private void InitSQLColName()
            {
                m_SQLColName = new string[]{"a.vchName",
                "a.intBWDAcctNum",
                "at.OpenTickets",
                "dbo.fxGetUserLocationName(0, a.LocationId)",                
                "dbo.fxGetUserName(lg.FirstName, lg.LastName, lg.Email)",
                "a.vchAcctNum",
                "a.vchRef1Num",
                "a.vchRef2Num",
                "a.vchCust1",
                "a.vchCust2",
                "a.vchCust3",
                "a.vchCust4",
                "a.vchCust5",
                "a.vchCust6",
                "a.vchCust7",
                "a.vchCust8",
                "a.vchCust9",
                "a.vchCust10",
                "a.vchCust11",
                "a.vchCust12",
                "a.vchCust13",
                "a.vchCust14",
                "a.vchCust15",
                "a.dtCust1",
                "a.dtCust2",
                "a.City",
                "a.State",
                "a.ZipCode",
                "a.Country",
                "'UTC '+CONVERT(nvarchar(100),a.TimeZone)",
                "a.Phone1",
                "a.Phone2",
                "a.vchEmailSuffix"};
            }

        /*    (select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='City') as LocationCity,
		(select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='State') as LocationState,
		(select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='Zip Code') as LocationZipCode,
		(select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='Country') as LocationCountry,
		(select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='Phone1') as LocationPhone1,
		(select top 1 v.PropertyValue from LocationTypeProperties p join LocationPropertyValues v on v.DId=@DId and v.LocationId=l.Id and v.LocationTypePropertyId=p.Id where p.DId=@DId and p.LocationTypeId=t.Id and p.Name='Phone2') as LocationPhone2,*/
            

            private string[] m_SQLColAlias = new string[]{"AccName",
                "BWDAccNum",
                "OpenTickets",
                "DeptLocation",                
                "AccRep",
                "AccNum",
                "AccRef1Num",
                "AccRef2Num",
                "Cust1",
                "Cust2",
                "Cust3",
                "Cust4",
                "Cust5",
                "Cust6",
                "Cust7",
                "Cust8",
                "Cust9",
                "Cust10",
                "Cust11",
                "Cust12",
                "Cust13",
                "Cust14",
                "Cust15",
                "CustDate1",
                "CustDate2",
                "LocCity",
                "LocState",
                "LocZipCode",
                "LocCountry",
                "LocTimeZone",
                "LocPhone1",
                "LocPhone2",
                "EmailSuffix"};

            private bool _IsInit = false;

            public ColumnsSetting()
            {
                InitSQLColName();
                UserSetting _c =  UserSetting.GetSettings("AcctList");
                if (!_c.IsDefined) return;
                InitObject(HttpUtility.UrlDecode(_c["FD"]), HttpUtility.UrlDecode(_c["OB"]));
                if (!_IsInit) return;
                if (!string.IsNullOrEmpty(_c["VW"])) m_ViewMode = (ViewMode)int.Parse(_c["VW"]);
                for (int i = 1; i < 16; i++)
                {
                    string _cust = "vCap" + i.ToString();
                    string _custon = "vCapOn" + i.ToString();
                    if (!string.IsNullOrEmpty(_c[_cust]))
                    {
                        m_CustomCup[i - 1] = HttpUtility.UrlDecode(_c[_cust]);
                        if (_c[_custon] != null)
                        {
                            if (_c[_custon] == "1") m_CustomOn[i - 1] = true;
                            else m_CustomOn[i - 1] = false;
                        }
                        else m_CustomOn[i - 1] = true;
                    }
                    else
                    {
                        m_CustomCup[i - 1] = "Cust" + i.ToString();
                        m_CustomOn[i - 1] = false;
                    }
                }
                if (!string.IsNullOrEmpty(_c["dCap1"]))
                {
                    m_CustomCup[15] = HttpUtility.UrlDecode(_c["dCap1"]);
                    if (_c["dCapOn1"] != null)
                    {
                        if (_c["dCapOn1"] == "1") m_CustomOn[15] = true;
                        else m_CustomOn[15] = false;
                    }
                    else m_CustomOn[15] = true;
                }
                else
                {
                    m_CustomCup[15] = "CustDate1";
                    m_CustomOn[15] = false;
                }
                if (!string.IsNullOrEmpty(_c["dCap2"]))
                {
                    m_CustomCup[16] = HttpUtility.UrlDecode(_c["dCap2"]);
                    if (_c["dCapOn2"] != null)
                    {
                        if (_c["dCapOn2"] == "1") m_CustomOn[16] = true;
                        else m_CustomOn[16] = false;
                    }
                    else m_CustomOn[16] = true;
                }
                else
                {
                    m_CustomCup[16] = "CustDate2";
                    m_CustomOn[16] = false;
                }
                ReduceCustomColList();
            }

            public ColumnsSetting(int DeptID, int UserID)
                : this()
            {
                if (_IsInit) return;
                string[] _arr = SelectBrowseColumns(DeptID, UserID);
                _arr = DeleteOldColumns(_arr, DeptID, UserID);
                InitObject(_arr[0], _arr[1]);
                DataRow _row = SelectAccountCustomFields(DeptID, false);
                if (_row == null) return;
                for (int i = 1; i < 16; i++)
                {
                    if (_row.IsNull("vchCust" + i.ToString() + "Cap")) m_CustomCup[i - 1] = "Cust" + i.ToString();
                    else m_CustomCup[i - 1] = _row["vchCust" + i.ToString() + "Cap"].ToString();
                    m_CustomOn[i - 1] = (bool)_row["btCust" + i.ToString() +"On"];
                }
                if (_row.IsNull("vchDateCust1Cap")) m_CustomCup[15] = "CustDate1";
                else m_CustomCup[15] = _row["vchDateCust1Cap"].ToString();
                m_CustomOn[15] = (bool)_row["btDateCust1On"];
                if (_row.IsNull("vchDateCust2Cap")) m_CustomCup[16] = "CustDate2";
                else m_CustomCup[16] = _row["vchDateCust2Cap"].ToString();
                m_CustomOn[16] = (bool)_row["btDateCust2On"];
                ReduceCustomColList();
            }

            private string[] DeleteOldColumns(string[] _arr, int DeptID, int UserID)
            {
                string[] oldColumns = { "6", "7" };
                string fields = DeleteOldColumns(_arr[0], oldColumns);
                string sorts = DeleteOldColumnsSort(_arr[1], oldColumns);
                if (fields != _arr[0] || sorts != _arr[1])
                {
                    //Update
                    UpdateBrowseColumns(DeptID, UserID, fields, sorts);
                    _arr[0] = fields;
                    _arr[1] = sorts;
                }
                return _arr;
            }

            private string DeleteOldColumns(string columnList, string[] oldColumns)
            {
                string[] _arr = columnList.Split(',');
                columnList = "";
                for (int i = 0; i < _arr.Length; i++)
                {
                    if (!ColumnIsOld(oldColumns, _arr[i]))
                    {
                        columnList += _arr[i] + ",";
                    }
                }
                return columnList.TrimEnd(',');
            }

            private string DeleteOldColumnsSort(string columnList, string[] oldColumns)
            {
                string[] _arr = columnList.Split(',');
                columnList = "";
                for (int i = 0; i < _arr.Length; i++)
                {
                    string[] _arrTemp = _arr[i].Split('-');
                    if (!ColumnIsOld(oldColumns, _arrTemp[0]))
                    {
                        columnList += _arr[i] + ",";
                    }
                }
                return columnList.TrimEnd(',');
            }

            private bool ColumnIsOld(string[] oldColumns, string columnIndex)
            {
                for (int j = 0; j < oldColumns.Length; j++)
                {
                    if (columnIndex == oldColumns[j])
                    {
                        return true;
                    }
                }
                return false;
            }

            protected void InitObject(string fields, string sort)
            {
                if (string.IsNullOrEmpty(fields)) return;
                string[] _arr = fields.Split(',');
                m_Col = new BrowseColumn[_arr.Length];
                for (int i = 0; i < _arr.Length; i++) m_Col[i] = (BrowseColumn)int.Parse(_arr[i]);
                if (!string.IsNullOrEmpty(sort))
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

            protected void ReduceCustomColList()
            {
                ArrayList _list = new ArrayList();
                for (int i = 0; i < m_Col.Length; i++)
                {
                    if (IsCustomColumn(m_Col[i]))
                    {
                        if (IsCustomColOn(m_Col[i])) _list.Add(m_Col[i]);
                    }
                    else _list.Add(m_Col[i]);
                }
                m_Col = (BrowseColumn[])_list.ToArray(typeof(BrowseColumn));
                _list = new ArrayList();
                ArrayList _list2 = new ArrayList();
                for (int i = 0; i < m_SortCol.Length; i++)
                {
                    if (IsCustomColumn(m_SortCol[i]))
                    {
                        if (IsCustomColOn(m_SortCol[i]))
                        {
                            _list.Add(m_SortCol[i]);
                            _list2.Add(m_SortOrder[i]);
                        }
                    }
                    else
                    {
                        _list.Add(m_SortCol[i]);
                        _list2.Add(m_SortOrder[i]);
                    }
                }
                m_SortCol = (BrowseColumn[])_list.ToArray(typeof(BrowseColumn));
                m_SortOrder = (bool[])_list2.ToArray(typeof(bool));
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
                UserSetting _c = UserSetting.GetSettings("AcctList");
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
                _c["VW"] = ((int)m_ViewMode).ToString();
                _c["FD"] = _fields;
                _c["OB"] = _sort;
                for (int i = 1; i < 16; i++)
                {
                    _c["vCap" + i.ToString()] = HttpUtility.UrlEncode(m_CustomCup[i - 1]);
                    if (m_CustomOn[i - 1]) _c["vCapOn" + i.ToString()] = "1";
                    else _c["vCapOn" + i.ToString()] = "0";
                }
                _c["dCap1"] = HttpUtility.UrlEncode(m_CustomCup[15]);
                if (m_CustomOn[15]) _c["dCapOn1"] = "1";
                else _c["dCapOn1"] = "0";
                _c["dCap2"] = HttpUtility.UrlEncode(m_CustomCup[16]);
                if (m_CustomOn[16]) _c["dCapOn2"] = "1";
                else _c["dCapOn2"] = "0";


                if (DeptID == 0 || UserId == 0) return;

                UpdateBrowseColumns(DeptID, UserId, _fields, _sort);
            }

            public BrowseColumn GetBrowseColumn(int index)
            {
                if (index < m_Col.Length) return m_Col[index];
                else return BrowseColumn.Blank;
            }

            public void SetBrowseColumn(int index, BrowseColumn col)
            {
                if (index >= m_Col.Length) RedimColList(index);
                m_Col[index] = col;
            }

            public BrowseColumn GetSortColumn(int index)
            {
                if (index < m_SortCol.Length) return m_SortCol[index];
                else return BrowseColumn.Blank;
            }

            public void SetSortColumn(int index, BrowseColumn col)
            {
                if (index >= m_SortCol.Length) RedimSortColList(index);
                m_SortCol[index] = col;
            }

            public bool GetSortOrderDesc(int index)
            {
                if (index < m_SortOrder.Length) return m_SortOrder[index];
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
                    case BrowseColumn.LocCity:
                        return "City";
                    case BrowseColumn.LocState:
                        return "State/Prov";
                    case BrowseColumn.LocPostalCode:
                        return "Postal Code";
                    case BrowseColumn.LocCountry:
                        return "Country";
                    case BrowseColumn.LocTimeZone:
                        return "Time Zone";
                    case BrowseColumn.LocPhone1:
                        return "Phone1";
                    case BrowseColumn.LocPhone2:
                        return "Phone2";
                    case BrowseColumn.OpenTickets:
                        return "Open Tickets";
                    default:
                        if (!IsCustomColumn(col)) return string.Empty;
                        else return m_CustomCup[(int)col - 8];
                }
            }

            public string GetColSQLName(BrowseColumn col)
            {
                return m_SQLColName[(int)col];
            }

            public string GetColSQLAlias(BrowseColumn col)
            {
                return m_SQLColAlias[(int)col];
            }

            public bool IsCustomColumn(BrowseColumn col)
            {
                int _index = (int)col;
                if (_index < 8 || _index > 24) return false;
                else return true;
            }

            public bool IsCustomColOn(BrowseColumn col)
            {
                if (!IsCustomColumn(col)) return false;
                else return m_CustomOn[(int)col - 8];
            }

            public ViewMode ListViewMode
            {
                get { return m_ViewMode; }
                set { m_ViewMode = value; }
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

        public class Filter
        {
            ActiveStatus m_ActiveStatus = ActiveStatus.Active;
            string m_AccName = string.Empty;
            int m_UserId = 0;
            int m_SupportGroupId = 0;
            bool m_FilterLocation = true;

            public Filter()
            {
                UserSetting _c = UserSetting.GetSettings("AcctList");
                if (!_c.IsDefined) return;
                if (!string.IsNullOrEmpty(_c["AF"]))
                {
                    if (_c["AF"] == "1") m_ActiveStatus = ActiveStatus.Active;
                    else if (_c["AF"] == "0") m_ActiveStatus = ActiveStatus.Inactive;
                    else m_ActiveStatus = ActiveStatus.NoFilter;
                }
                if (!string.IsNullOrEmpty(_c["SrANa"])) m_AccName = HttpUtility.UrlDecode(_c["SrANa"]);
            }

            public Filter(ActiveStatus ActiveStatus, string AccName)
            {
                m_ActiveStatus = ActiveStatus;
                m_AccName = AccName;
            }

            public void SaveToSession()
            {
                UserSetting _c = UserSetting.GetSettings("AcctList");
                switch (m_ActiveStatus)
                {
                    case ActiveStatus.Active:
                        _c["AF"] = "1";
                        break;
                    case ActiveStatus.Inactive:
                        _c["AF"] = "0";
                        break;
                    case ActiveStatus.NoFilter:
                        _c["AF"] = "-1";
                        break;
                }
                _c["SrANa"] = HttpUtility.UrlEncode(m_AccName);
            }

            public int UserId
            {
                get { return m_UserId; }
                set { m_UserId = value; }
            }

            public int SupportGroupId
            {
                get { return m_SupportGroupId; }
                set { m_SupportGroupId = value; }
            }

            public ActiveStatus AccActive
            {
                get { return m_ActiveStatus; }
                set { m_ActiveStatus = value; }
            }

            public string AccName
            {
                get { return m_AccName; }
                set { m_AccName = value; }
            }

            public bool FilterLocation
            {
                get
                {
                    return m_FilterLocation;
                }
                set
                {
                    m_FilterLocation = value;
                }
            }
        }

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

        public static DataTable SelectAll(Guid OrgId, int DeptID)
        {
            return SelectRecords("sp_SelectAccounts", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
        }

        public static DataTable SelectUserPager(int DepartmentId, int AccountId)
        {
            return SelectRecords("sp_SelectAcctUsersPager", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@AccountId", AccountId) });
        }

        public static int SelectAccountIdByEmailSuffix(int DepartmentId, string emailSuffix)
        {
            return SelectAccountIdByEmailSuffix(Guid.Empty, DepartmentId, emailSuffix);
        }

        public static int SelectAccountIdByEmailSuffix(Guid OrgID, int DepartmentId, string emailSuffix)
        {
            SqlParameter accountId = new SqlParameter("@AcctId", SqlDbType.Int);
            accountId.Direction = ParameterDirection.Output;
            accountId.IsNullable = true;
            accountId.Value = 0;

            UpdateData("sp_SelectAcctEmailSuffix", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@vchEmailSuffix", emailSuffix), accountId }, OrgID);

            return (int)accountId.Value;
        }

        public static DataRow SelectOne(int DeptID, int AccountId)
        {
            return SelectOne(DeptID, AccountId, Guid.Empty);
        }

        public static DataRow SelectOne(int DeptID, int AccountId, Guid orgId)
        {
            return SelectRecord("sp_SelectAccountDetail", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", AccountId) }, orgId);
        }

        public static int SelectAccountsCount(Guid OrgId, Guid InstId)
        {
            int deptId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM Accounts WHERE DId=" + deptId.ToString() + " AND btActive=1", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static string SelectAccountName(int DepartmentId, int AccountId)
        {
            SqlParameter accountName = new SqlParameter("@vchAcctName", SqlDbType.VarChar, 100);
            accountName.Direction = ParameterDirection.Output;
            accountName.IsNullable = true;
            accountName.Value = 0;

            UpdateData("sp_SelectAcctName", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@AcctId", AccountId), accountName });

            return accountName.Value == DBNull.Value ? "" : (string)accountName.Value;
        }

        public static DataTable SelectAll(int DeptID, int UserID)
        {
            return SelectAll(Guid.Empty, DeptID, UserID);
        }

        public static DataTable SelectAll(Guid OrgId, int DeptID, int UserID)
        {
            return GlobalFilters.SetFilter(OrgId, DeptID, UserID, SelectAll(OrgId, DeptID), "Id", GlobalFilters.FilterType.Accounts);
        }

        public static DataTable SelectLocations(int DepartmentId, int AccountId)
        {
            return SelectRecords("sp_SelectAcctLocations", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@AccountId", AccountId) });
        }

        public static DataTable SelectAllChildLocations(int DepartmentId, int AccountId)
        {
             return SelectRecords("sp_SelectAcctAllChildLocations", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@AccountId", AccountId) });
        }

        public static DataTable SelectUsers(int DeptID, int AccountID, string Search, InactiveStatus inactiveStatus)
        {
            return SelectUsers(DeptID, AccountID, Search, inactiveStatus, 0);
        }

        public static DataTable SelectUsers(int DeptID, int AccountID, string Search, InactiveStatus inactiveStatus, int MaxCount)
        {
            SqlParameter _pSearch = new SqlParameter("@strSearch", SqlDbType.VarChar, 25);
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            SqlParameter _pMaxCount = new SqlParameter("@Top", SqlDbType.Int);

            if (MaxCount > 0) _pMaxCount.Value = MaxCount;
            else _pMaxCount.Value = DBNull.Value;

            if (!string.IsNullOrEmpty(Search)) _pSearch.Value = Search;
            else _pSearch.Value = DBNull.Value;

            if (inactiveStatus == InactiveStatus.DoesntMatter)
                _pInactive.Value = DBNull.Value;
            else
                _pInactive.Value = inactiveStatus;
            return SelectRecords("sp_SelectAcctUsers", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", AccountID), _pSearch, _pInactive, _pMaxCount });
        }

        public static void UpdateBrowseColumns(int DeptID, int UserId, string fields, string sort)
        {
            SqlParameter _pAccFields = new SqlParameter("@vchAcctFields", SqlDbType.VarChar, 30);
            _pAccFields.Direction = ParameterDirection.InputOutput;
            if (fields.Length > 0) _pAccFields.Value = fields;
            else _pAccFields.Value = DBNull.Value;
            SqlParameter _pAccSort = new SqlParameter("@vchAcctSort", SqlDbType.VarChar, 25);
            _pAccSort.Direction = ParameterDirection.InputOutput;
            if (sort.Length > 0) _pAccSort.Value = sort;
            else _pAccSort.Value = DBNull.Value;
            UpdateData("sp_SelectAcctFields", new SqlParameter[] { new SqlParameter("@Mode", "U"), new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pAccFields, _pAccSort });
        }

        public static string[] SelectBrowseColumns(int DeptID, int UserId)
        {
            SqlParameter _pAccFields = new SqlParameter("@vchAcctFields", SqlDbType.VarChar, 30);
            _pAccFields.Direction = ParameterDirection.Output;
            SqlParameter _pAccSort = new SqlParameter("@vchAcctSort", SqlDbType.VarChar, 25);
            _pAccSort.Direction = ParameterDirection.Output;
            UpdateData("sp_SelectAcctFields", new SqlParameter[] { new SqlParameter("@Mode", "S"), new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), _pAccFields, _pAccSort });
            return new string[] { _pAccFields.Value.ToString(), _pAccSort.Value.ToString() };
        }

        public static bool IsUserInAccount(int departmentId, int accountId, int userId)
        {
            DataTable userAccount = Accounts.SelectByQuery(string.Format("SELECT 1 FROM UserAccounts WHERE DepartmentId ={0} AND AccountId ={1} AND UserId ={2}", departmentId, accountId, userId));
            return (userAccount != null && userAccount.Rows.Count > 0);
        }

        public static DataTable SelectByFilter(UserAuth userAuth, ColumnsSetting colset, Filter filter)
        {
            return SelectByFilter(userAuth, colset, filter, false);
        }

        public static DataTable SelectByFilter(UserAuth userAuth, ColumnsSetting colset, Filter filter, bool addNoAccountRow)
        {
            string _query = "SELECT 0 AS ArtificialSortingField, a.id AS Id, a.AcctRepId";
            System.Collections.Generic.List<string> columnAliases = new System.Collections.Generic.List<string>();
            for (int i = 0; i < colset.BrowseColumnsCount; i++)
            {
                string columnAlias = colset.GetColSQLAlias(colset.GetBrowseColumn(i));
                if (colset.GetBrowseColumn(i) == BrowseColumn.DeptLocation)
                {
                    _query += ", dbo.fxGetUserLocationName(" + userAuth.lngDId.ToString() + ", a.LocationId) AS " + columnAlias;
                }
                else if (colset.GetBrowseColumn(i) == BrowseColumn.BWDAccRef)
                {
                    _query += ", " + colset.GetColSQLName(colset.GetBrowseColumn(i)) + " AS " + columnAlias;
                    _query += ", " + colset.GetColSQLName(Data.Accounts.BrowseColumn.Ref1Number) + " AS " + colset.GetColSQLAlias(Data.Accounts.BrowseColumn.Ref1Number);
                    _query += ", " + colset.GetColSQLName(Data.Accounts.BrowseColumn.Ref2Number) + " AS " + colset.GetColSQLAlias(Data.Accounts.BrowseColumn.Ref2Number);
                    columnAliases.Add(colset.GetColSQLAlias(Data.Accounts.BrowseColumn.Ref1Number));
                    columnAliases.Add(colset.GetColSQLAlias(Data.Accounts.BrowseColumn.Ref2Number));
                }
                else
                {
                    _query += ", " + colset.GetColSQLName(colset.GetBrowseColumn(i)) + " AS " + columnAlias;
                }

                columnAliases.Add(columnAlias);
            }
            _query += " FROM Accounts a";
            _query += " LEFT OUTER JOIN SupportGroups sp ON sp.DId=a.DId AND a.SupGroupId=sp.Id";
            _query += " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj ON tlj.company_id=a.DId AND tlj.id=a.AcctRepId JOIN tbl_Logins lg ON lg.id=tlj.login_id";
            _query += " LEFT OUTER JOIN Locations al ON al.DId=a.DId AND al.Id=a.LocationId";
            _query += " LEFT OUTER JOIN LocationTypes t on t.DId=a.DId and t.Id=al.LocationTypeId and t.Name='Building'  ";
            _query += " LEFT OUTER JOIN (SELECT t.intAcctId AS AccountId, COUNT(*) AS OpenTickets FROM tbl_ticket t WHERE t.company_id=" + userAuth.lngDId.ToString() + " AND t.Status<>'Closed' GROUP BY t.intAcctId) at ON at.AccountId=a.Id";
            _query += " WHERE a.DId=" + userAuth.lngDId;
            if (filter.AccActive != ActiveStatus.NoFilter) _query += " AND a.btActive=" + ((int)filter.AccActive).ToString();
            if (colset.ListViewMode == ViewMode.MyAccounts) _query += " AND a.AcctRepId=" + filter.UserId.ToString();
            else if (colset.ListViewMode == ViewMode.SupportGroupAccounts) _query += " AND a.SupGroupId=" + filter.SupportGroupId.ToString();
            if (filter.AccName.Length > 0)
            {
                string _fstr = Security.SQLInjectionBlock(Functions.SqlStr(filter.AccName)).Trim('\'');
                _query += " AND (a.vchName LIKE '%" + _fstr + "%'";
                if (filter.FilterLocation)
                {
                    _query += " OR (dbo.fxGetUserLocationName(" + userAuth.lngDId.ToString() + ", a.LocationId) LIKE '%" + _fstr + "%')";
                }
                _query += " OR (";
                _query += " CAST(a.intBWDAcctNum as varchar) LIKE '%" + _fstr + "%' OR a.vchRef1Num LIKE '%" + _fstr + "%' OR a.vchRef2Num LIKE '%" + _fstr + "%' OR a.vchAcctNum LIKE '%" + _fstr + "%'))";
            }

            string internalAccountName = userAuth.strGDName + " (Internal)";
            if (colset.ListViewMode == ViewMode.AllAccounts && filter.AccActive != ActiveStatus.Inactive && (string.IsNullOrEmpty(filter.AccName) || internalAccountName.ToLower().Contains(filter.AccName.ToLower())))
            {
                string union = " UNION SELECT 1 AS ArtificialSortingField, -1 AS Id, -1 AS AcctRepId";
                foreach (string columnAlias in columnAliases)
                {
                    union += ", ";
                    switch (columnAlias)
                    {
                        case "AccName":
                            union += Functions.SqlStr(internalAccountName);
                            break;
                        case "OpenTickets":
                            union += "(SELECT COUNT(*) AS OpenTickets FROM tbl_ticket t WHERE t.company_id=" + userAuth.lngDId.ToString() + " AND t.Status<>'Closed' AND t.intAcctId IS NULL AND ISNULL(t.btNoAccount, 0) = 0)";
                            break;
                        default:
                            union += "NULL";
                            break;
                    }
                    union += " AS " + columnAlias;
                }
                _query += union;
            }
            if (addNoAccountRow)
            {
                string union = " UNION SELECT -1 AS ArtificialSortingField, -2 AS Id, -2 AS AcctRepId";
                foreach (string columnAlias in columnAliases)
                {
                    union += ", ";
                    switch (columnAlias)
                    {
                        case "AccName":
                            union += Functions.SqlStr("(No Account)");
                            break;
                        case "OpenTickets":
                            union += "(SELECT COUNT(*) AS OpenTickets FROM tbl_ticket t WHERE t.company_id=" + userAuth.lngDId.ToString() + " AND t.Status<>'Closed' AND t.intAcctId IS NULL AND t.btNoAccount = 1)";
                            break;
                        default:
                            union += "NULL";
                            break;
                    }
                    union += " AS " + columnAlias;
                }
                _query += union;
            }

            string _order = " ORDER BY ArtificialSortingField DESC,";
            for (int i = 0; i < colset.SortColumnsCount; i++)
            {
                if (!columnAliases.Contains(colset.GetColSQLAlias(colset.GetSortColumn(i)))) continue;
                if (_order.IndexOf(colset.GetColSQLName(colset.GetSortColumn(i))) < 0)
                {
                    if (colset.GetBrowseColumn(i) == BrowseColumn.DeptLocation)
                    {
                        _order += " " + colset.GetColSQLAlias(colset.GetSortColumn(i));
                    }
                    else
                    {
                        _order += " " + colset.GetColSQLName(colset.GetSortColumn(i));
                    }
                    if (colset.GetSortOrderDesc(i)) _order += " DESC,";
                    else _order += " ASC,";
                }
            }
            _order = _order.Substring(0, _order.Length - 1);
            _query += _order;
            return SelectByQuery(_query);
        }

        public static DataTable SelectFiltered(UserAuth userAuth, ColumnsSetting colset, Filter filter)
        {
            return SelectFiltered(userAuth, colset, filter, false);
        }

        public static DataTable SelectFiltered(UserAuth userAuth, ColumnsSetting colset, Filter filter, bool addNoAccountRow)
        {
            return GlobalFilters.SetFilter(userAuth.lngDId, userAuth.lngUId, SelectByFilter(userAuth, colset, filter, addNoAccountRow), "Id", GlobalFilters.FilterType.Accounts);
        }

        public static DataTable SelectForExport(int DepartmentId)
        {
            return SelectRecords("sp_SelectAccountsForExport", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId) });
        }

        public static DataTable SelectUsers(int DeptID, int AccountID, string Search)
        {
            return SelectUsers(DeptID, AccountID, Search, InactiveStatus.DoesntMatter);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId)
        {
            return SelectUserAccounts(departmentId, userId, userId, true);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, Guid orgId)
        {
            return SelectUserAccounts(departmentId, userId, userId, true, orgId);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, Guid orgId, bool internalIfEmpty)
        {
            return SelectUserAccounts(departmentId, userId, userId, internalIfEmpty, orgId);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, int requestedByUserId)
        {
            return SelectUserAccounts(departmentId, userId, requestedByUserId, true);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, bool internalIfEmpty)
        {
            return SelectUserAccounts(departmentId, userId, userId, internalIfEmpty);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, int requestedByUserId, bool internalIfEmpty)
        {
            return SelectUserAccounts(departmentId, userId, userId, internalIfEmpty, Guid.Empty);
        }

        public static DataTable SelectUserAccounts(int departmentId, int userId, int requestedByUserId, bool internalIfEmpty, Guid orgId)
        {
            return GlobalFilters.SetFilter(orgId, departmentId, requestedByUserId, SelectRecords("sp_SelectUserAccounts", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@UserId", userId), new SqlParameter("@InternalIfEmpty", internalIfEmpty) }, orgId), "AccountId", GlobalFilters.FilterType.Accounts);
        }

        public static int SelectUserDefaultAccount(Guid OrgID, int departmentId, int userId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SelectRecord("sp_SelectUserDefaultAccount",
                         new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@UserId", userId), pReturnValue },
                         OrgID);

            int returnValue = 0;
            try
            {
                returnValue = Convert.ToInt32(pReturnValue.Value);
            }
            catch { }
            return returnValue;
        }

        public static DataTable SelectUsersToTransfer(int DeptID, int AccountID, int ExcludeUserID)
        {
            return SelectRecords("sp_SelectAcctUsersDelete", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountID), new SqlParameter("@Id", ExcludeUserID) });
        }

        public static DataTable SelectFiles(int DeptID, int AccountID, string Search)
        {
            SqlParameter _pSearch = new SqlParameter("@strSearch", SqlDbType.VarChar, 25);
            if (Search.Length > 0) _pSearch.Value = Search;
            else _pSearch.Value = DBNull.Value;
            return SelectRecords("sp_SelectAcctFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountID), _pSearch });
        }

        public static void SelectFile(int DepartmentId, int AccountId, int fileId, out int bwaFileId, out string fileName, out byte fileVersion, out int fileSize)
        {
            SqlParameter pBWAFileId = new SqlParameter("@intBWAFileId", SqlDbType.Int);
            pBWAFileId.Direction = ParameterDirection.Output;
            pBWAFileId.Value = DBNull.Value;

            SqlParameter pFileName = new SqlParameter("@vchName", SqlDbType.VarChar, 100);
            pFileName.Direction = ParameterDirection.Output;
            pFileName.Value = DBNull.Value;

            SqlParameter pFileVersion = new SqlParameter("@tintVersion", SqlDbType.TinyInt);
            pFileVersion.Direction = ParameterDirection.Output;
            pFileVersion.Value = DBNull.Value;

            SqlParameter pFileSize = new SqlParameter("@intSizeKB", SqlDbType.Int);
            pFileSize.Direction = ParameterDirection.Output;
            pFileSize.Value = DBNull.Value;

            UpdateData("sp_SelectAcctFile", new SqlParameter[] {
                new SqlParameter("@DId",DepartmentId),
                new SqlParameter("@AcctId",AccountId),
                new SqlParameter("@Id",fileId),
                pBWAFileId,
                pFileName,
                pFileVersion,
                pFileSize
            });

            bwaFileId = pBWAFileId.Value != DBNull.Value ? (int)pBWAFileId.Value : 0;
            fileName = pFileName.Value != DBNull.Value ? (string)pFileName.Value : string.Empty;
            fileVersion = pFileVersion.Value != DBNull.Value ? (byte)pFileVersion.Value : (byte)0;
            fileSize = pFileSize.Value != DBNull.Value ? (int)pFileSize.Value : 0;
        }

        /// <summary>
        /// Delete file
        /// <returns>BWAFileId</returns>
        /// </summary>
        public static int DeleteFile(int DepartmentId, int AccountId, int FileId)
        {
            string name = string.Empty;
            int sizeKB = 0;
            DateTime dtModified;

            return UpdateFiles("D", DepartmentId, AccountId, FileId, ref name, ref sizeKB, 0, out dtModified);
        }

        /// <summary>
        /// Update file
        /// </summary>
        public static void UpdateFiles(int DepartmentId, int AccountId, string Name, int SizeKB, int BWAFileId)
        {
            string name = Name;
            int sizeKB = SizeKB;
            DateTime dtModified;

            UpdateFiles("U", DepartmentId, AccountId, 0, ref name, ref sizeKB, BWAFileId, out dtModified);
        }

        /// <summary>
        /// Get file essential info
        /// </summary>
        public static void GetFileEssentialInfo(int DepartmentId, int AccountId, int FileId, out string Name, out int SizeKB, out DateTime Modified)
        {
            string name = string.Empty;
            int sizeKB = 0;
            DateTime dtModified = DateTime.MinValue;

            UpdateFiles("R", DepartmentId, AccountId, FileId, ref name, ref sizeKB, 0, out dtModified);

            Name = name;
            SizeKB = sizeKB;
            Modified = dtModified;
        }

        /// <summary>
        /// Common function for file deleting, updating and getting some essential info
        /// </summary>
        private static int UpdateFiles(string mode, int DepartmentId, int AccountId, int FileId, ref string Name, ref int SizeKB, int BWAFileId, out DateTime Modified)
        {
            Modified = DateTime.MinValue;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pSizeKB = new SqlParameter("@intSizeKB", SqlDbType.Int);
            pSizeKB.Direction = ParameterDirection.InputOutput;
            pSizeKB.Value = SizeKB;

            SqlParameter pModified = new SqlParameter("@dtModified", SqlDbType.SmallDateTime);
            pModified.Direction = ParameterDirection.Output;


            SqlParameter pName = new SqlParameter("@vchName", SqlDbType.VarChar, 100);
            pName.Direction = ParameterDirection.InputOutput;
            pName.Value = Name;

            UpdateData("sp_UpdateAcctFiles", new SqlParameter[] {
                pReturnValue,
                new SqlParameter("@Mode", mode),
                new SqlParameter("@DId", DepartmentId),
                new SqlParameter("@AcctId", AccountId),
                new SqlParameter("@Id", FileId),
                pName,
                pSizeKB,
                new SqlParameter("@intBWAFileId", BWAFileId),
                pModified
                });

            Name = pName.Value != DBNull.Value ? (string)pName.Value : string.Empty;
            SizeKB = pSizeKB.Value != DBNull.Value ? (int)pSizeKB.Value : 0;
            Modified = pModified.Value != DBNull.Value ? (DateTime)pModified.Value : DateTime.MinValue;

            return pReturnValue.Value != DBNull.Value ? (int)pReturnValue.Value : 0;
        }

        public static int GetAccountIdByName(int departmentId, string accountName)
        {
            return GetAccountIdByName(Guid.Empty, departmentId, accountName);
        }

        public static int GetAccountIdByName(Guid OrgID, int departmentId, string accountName)
        {
            int accountId = -1;
            if (!string.IsNullOrEmpty(accountName))
            {
                string sqlQuery = "SELECT Id AS AccountId FROM Accounts WHERE DId =" + departmentId + " AND vchName = '" + Security.SQLInjectionBlock(accountName.Replace("'", "''")) + "'";

                DataTable dtAccounts = SelectByQuery(sqlQuery, OrgID);
                if (dtAccounts != null && dtAccounts.Rows.Count == 1)
                {
                    try
                    {
                        accountId = Convert.ToInt32(dtAccounts.Rows[0]["AccountId"]);
                    }
                    catch { }
                }
            }

            return accountId;
        }

        public static DataRow SelectAccountCustomFields(int DeptID, bool isProject)
        {
            return SelectRecord("sp_SelectAcctCfg", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@isProject", isProject) });
        }

        public static void UpdateAccountAutoIncrement(int DeptID, int NewAccNumber)
        {
            UpdateData("sp_UpdateAcctAutoInc", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@intNewAcctNum", NewAccNumber) });
        }

        public static void UpdateAcctCustCharField(int DeptID, bool IsActive, bool IsDropDown, string Caption, string Options, string Default, bool IsRequired, int Number, bool isProject)
        {
            UpdateData("sp_UpdateAcctCustCharFlds", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@btCustOn", IsActive), new SqlParameter("@btCustType", IsDropDown), new SqlParameter("@vchCustCap", Caption), new SqlParameter("@vchCustOptions", Options), new SqlParameter("@vchCustDefault", Default), new SqlParameter("@btCustReq", IsRequired), new SqlParameter("@intNum", Number), new SqlParameter("@isProject", isProject) });
        }

        public static void UpdateAcctCustDateField(int DeptID, bool IsActive, string Caption, bool IsRequired, int Number, bool isProject)
        {
            UpdateData("sp_UpdateAcctCustDtFlds", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@btDateCustOn", IsActive), new SqlParameter("@vchDateCustCap", Caption), new SqlParameter("@btDateCustReq", IsRequired), new SqlParameter("@intNum", Number), new SqlParameter("@isProject", isProject) });
        }

        public static int UpdateAccount(int DeptID, int UserID, int AccountId, string AccountName, bool IsActive, bool IsOrganization, int SupportRepId, int SupportGroupId, int LocationId, string EmailSuffix, string InternalAccId, string Ref1Num, string Ref2Num, bool accLevelTimeTracking, string[] CustFields, DateTime[] CustDates,
            string Address1, string Address2, string City, string State, string ZipCode, string Country, string TimeZone, string Phone1, string Phone2)
        {
            return UpdateAccount(Guid.Empty, DeptID, UserID, AccountId, AccountName, IsActive, IsOrganization, SupportRepId, SupportGroupId, LocationId, EmailSuffix, InternalAccId, Ref1Num, Ref2Num, accLevelTimeTracking, CustFields, CustDates, Address1, Address2, City, State, ZipCode, Country, TimeZone, Phone1, Phone2);
        }

        public static int UpdateAccount(Guid OrgId, int DeptID, int UserID, int AccountId, string AccountName, bool IsActive, bool IsOrganization, int SupportRepId, int SupportGroupId, int LocationId, string EmailSuffix, string InternalAccId, string Ref1Num, string Ref2Num, bool accLevelTimeTracking, string[] CustFields, DateTime[] CustDates,
            string Address1, string Address2, string City, string State, string ZipCode, string Country, string TimeZone, string Phone1, string Phone2)
        {
            System.Collections.Generic.List<SqlParameter> p = new System.Collections.Generic.List<SqlParameter>();
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            p.Add(_pRVAL);
            p.Add(new SqlParameter("@DId", DeptID));
            p.Add(new SqlParameter("@UId", UserID));
            p.Add(new SqlParameter("@Id", AccountId));
            p.Add(new SqlParameter("@vchName", AccountName));
            p.Add(new SqlParameter("@btActive", IsActive));
            p.Add(new SqlParameter("@btOrgAcct", IsOrganization));
            p.Add(new SqlParameter("@AcctRepId", SupportRepId));
            if (SupportGroupId != 0) p.Add(new SqlParameter("@SupGroupId", SupportGroupId));
            if (LocationId != 0) p.Add(new SqlParameter("@LocationId", LocationId));
            if (!string.IsNullOrEmpty(InternalAccId)) p.Add(new SqlParameter("@vchAcctNum", InternalAccId));
            if (!string.IsNullOrEmpty(Ref1Num)) p.Add(new SqlParameter("@vchRef1Num", Ref1Num));
            if (!string.IsNullOrEmpty(Ref2Num)) p.Add(new SqlParameter("@vchRef2Num", Ref2Num));
            for(int i=0;i<CustFields.Length;i++)
            {
                string _cust = CustFields[i];
                if (!string.IsNullOrEmpty(_cust)) p.Add(new SqlParameter("@vchCust" + (i+1), _cust));
            }
            for(int i=0;i<CustDates.Length;i++)
            {
                DateTime _dtcust = CustDates[i];
                if (_dtcust != null && _dtcust > DateTime.MinValue) p.Add(new SqlParameter("@dtCust" + (i+1), _dtcust));
            }
            p.Add(new SqlParameter("@vchEmailSuffix", EmailSuffix));
            p.Add(new SqlParameter("@AccLevelTimeTracking", accLevelTimeTracking));

            p.Add(new SqlParameter("Address1", Address1));
            p.Add(new SqlParameter("Address2", Address2));
            p.Add(new SqlParameter("City", City));
            p.Add(new SqlParameter("State",State));
            p.Add(new SqlParameter("ZipCode",ZipCode));
            p.Add(new SqlParameter("Country",Country));
            p.Add(new SqlParameter("TimeZoneId",TimeZone));
            p.Add(new SqlParameter("Phone1",Phone1));
            p.Add(new SqlParameter("Phone2",Phone2));

            UpdateData("sp_UpdateAccount", p.ToArray(), OrgId);
            return (int)p[0].Value;
        }

        public static int InsertUserIntoAccount(int DeptID, int AccountId, string Email, int AccountLocationId, bool IsTransferOldTkts)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pAccLocation = new SqlParameter("@AcctLocationId", SqlDbType.Int);
            if (AccountLocationId != 0) _pAccLocation.Value = AccountLocationId;
            else _pAccLocation.Value = DBNull.Value;
            UpdateData("sp_InsertAcctUserCheck", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@vchEmail", Email), _pAccLocation, new SqlParameter("@btTransfer", IsTransferOldTkts) });
            return (int)_pRVAL.Value;
        }

        public static int InsertUserIntoAccountGlobal(int DeptID, int AccountId, string Email, int AccountLocationId, string Password, string FirstName, string LastName)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pAccLocation = new SqlParameter("@AcctLocationId", SqlDbType.Int);
            if (AccountLocationId != 0) _pAccLocation.Value = AccountLocationId;
            else _pAccLocation.Value = DBNull.Value;
            UpdateData("sp_InsertAcctUserGlobal", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@vchEmail", Email), _pAccLocation, new SqlParameter("@vchPassCode", Password), new SqlParameter("@vchFirstName", FirstName), new SqlParameter("@vchLastName", LastName) });
            return (int)_pRVAL.Value;
        }

        public static DataRow SelectAccountUser(int DeptID, int accountId, int UserID)
        {
            return SelectAccountUser(DeptID, accountId, UserID, Guid.Empty);
        }

        public static DataRow SelectAccountUser(int DeptID, int accountId, int UserID, Guid orgId)
        {
            return SelectRecord("sp_SelectAcctUser", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AccountId", accountId), new SqlParameter("@Id", UserID) }, orgId);
        }

        public static void UpdateAccountUser(int DeptID, int UserId, string FirstName, string LastName, int accountId, int accountLocationId, string Title, string Phone1, string Mobile, int UserType)
        {
            SqlParameter _pAccLocationId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
            if (accountLocationId != 0)
                _pAccLocationId.Value = accountLocationId;
            else
                _pAccLocationId.Value = DBNull.Value;

            UpdateData("sp_UpdateAcctUser", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", UserId), new SqlParameter("@vchFirstName", FirstName), new SqlParameter("@vchLastName", LastName), new SqlParameter("@AccountId", accountId), _pAccLocationId, new SqlParameter("@vchTitle", Title), new SqlParameter("@vchPhone1", Phone1), new SqlParameter("@vchMobile", Mobile), new SqlParameter("@intUserType", UserType),
            new SqlParameter("@AccountingContact", DBNull.Value),
            new SqlParameter("@AccountingContactPrimary", DBNull.Value)});
        }

        public static void UpdateAccountUser(int DeptID, int UserId, string FirstName, string LastName, int accountId, int accountLocationId, string Title, string Phone1, string Mobile, int UserType,
            bool AccountingContact, bool AccountingContactPrimary)
        {
            SqlParameter _pAccLocationId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
            if (accountLocationId != 0)
                _pAccLocationId.Value = accountLocationId;
            else
                _pAccLocationId.Value = DBNull.Value;

            UpdateData("sp_UpdateAcctUser", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", UserId), new SqlParameter("@vchFirstName", FirstName), new SqlParameter("@vchLastName", LastName), new SqlParameter("@AccountId", accountId), _pAccLocationId, new SqlParameter("@vchTitle", Title), new SqlParameter("@vchPhone1", Phone1), new SqlParameter("@vchMobile", Mobile), new SqlParameter("@intUserType", UserType),
            new SqlParameter("@AccountingContact", AccountingContact),
            new SqlParameter("@AccountingContactPrimary", AccountingContactPrimary)});
        }

        public static int SelectAccountLevel(int DeptID, int AccountId, int Level)
        {
            SqlParameter _pRoutingOption = new SqlParameter("@tintRoutingOption", SqlDbType.TinyInt);
            _pRoutingOption.Direction = ParameterDirection.Output;
            UpdateData("sp_SelectAccountLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level), _pRoutingOption });
            if (_pRoutingOption.Value != DBNull.Value) return Convert.ToInt32(_pRoutingOption.Value);
            else return -1;
        }

        public static DataTable SelectAccountLevelReps(int DeptID, int AccountId, int Level)
        {
            return SelectRecords("sp_SelectAcctLvlReps", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level) });
        }

        public static DataTable SelectAccountReps(int DeptID)
        {
            return SelectRecords("sp_SelectAcctReps", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }

        public static void UpdateAccountLevel(int DeptID, int AccountId, int Level, int RoutingOprion)
        {
            UpdateData("sp_UpdateAccountLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level), new SqlParameter("@tintRoutingOption", RoutingOprion) });
        }

        public static void DeleteAccountLevel(int DeptID, int AccountId, int Level)
        {
            UpdateData("sp_DeleteAccountLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level) });
        }

        public static void InsertAccountLevelTech(int DeptID, int AccountId, int Level, int UserId)
        {
            UpdateData("sp_InsertAccountLevelTech", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level), new SqlParameter("@UId", UserId) });
        }

        public static void DeleteAccountLevelTech(int DeptID, int AccountId, int Level, int UserId)
        {
            UpdateData("sp_DeleteAccountLevelTech", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), new SqlParameter("@tintLevel", Level), new SqlParameter("@UId", UserId) });
        }

        public static string GetAccountNote(int DeptID, int AccountId)
        {
            DataRow _row = SelectRecord("sp_SelectAcctNote", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId) });
            if (_row != null) return _row["txtNote"].ToString();
            else return string.Empty;
        }

        public static void SetAccountNote(int DeptID, int AccountId, string Note)
        {
            SqlParameter _pNote = new SqlParameter("@txtNote", SqlDbType.Text);
            if (Note.Length > 0) _pNote.Value = Note;
            else _pNote.Value = DBNull.Value;
            UpdateData("sp_UpdateAcctNote", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountId), _pNote });
        }

        public static DataTable SelectProjects(int companyID, int accountID)
        {
            return SelectRecords("sp_SelectAcctProjects", new SqlParameter[] { new SqlParameter("@CompanyID", companyID), new SqlParameter("@AccountID", accountID) });
        }

        public static DataTable SelectProjectsTree(int companyID, int accountID, bool active, int currProjectID)
        {
            return SelectRecords("sp_SelectAcctProjectsTree", new SqlParameter[]
                {
                    new SqlParameter("@CompanyID", companyID), 
                    new SqlParameter("@AccountID", accountID),
                    new SqlParameter("@Active", active),
                    new SqlParameter("@CurrProjectID", currProjectID)
                });
        }

        public static DataRow SelectAccountRate(int companyID, int accountID)
        {
            return SelectRecord("sp_SelectAccountRate", new SqlParameter[]
                   {
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@AccountID", accountID)
                   });
        }

        public static void UpdateAccountRate(int accountRateID, int companyID, int accountID,
            int billingMethodID, decimal flatFee, decimal hourlyBlendedRate, int ratePlanID, int flatFeeMode, DateTime dtNextDate, string sQBAccount, string sQBItem)
        {
            SqlParameter[] _params = new SqlParameter[11];
            _params[0] = new SqlParameter("@AccountRateID", accountRateID);
            _params[1] = new SqlParameter("@CompanyID", companyID);
            _params[2] = new SqlParameter("@AccountID", accountID);

            _params[3] = new SqlParameter("@BillingMethodID", billingMethodID);
            _params[4] = new SqlParameter("@FlatFee", SqlDbType.Money);
            if (flatFee > 0)
            {
                _params[4].Value = flatFee;
            }
            else
            {
                _params[4].Value = DBNull.Value;
            }
            _params[5] = new SqlParameter("@HourlyBlendedRate", SqlDbType.SmallMoney);
            if (hourlyBlendedRate > 0)
            {
                _params[5].Value = hourlyBlendedRate;
            }
            else
            {
                _params[5].Value = DBNull.Value;
            }
            _params[6] = new SqlParameter("@RatePlanID", SqlDbType.Int);
            if (ratePlanID > 0)
            {
                _params[6].Value = ratePlanID;
            }
            else
            {
                _params[6].Value = DBNull.Value;
            }
            _params[7] = new SqlParameter("@FlatFeeMode", SqlDbType.Int);
            if (flatFeeMode > -1) _params[7].Value = flatFeeMode;
            else _params[7].Value = DBNull.Value;
            _params[8] = new SqlParameter("@FlatFeeNextDate", SqlDbType.SmallDateTime);
            if (dtNextDate > DateTime.MinValue) _params[8].Value = dtNextDate;
            else _params[8].Value = DBNull.Value;

            _params[9] = new SqlParameter("@QBAccountAlias", SqlDbType.VarChar);
            if (String.IsNullOrEmpty(sQBAccount)) _params[9].Value = DBNull.Value;
            else _params[9].Value = sQBAccount;

            _params[10] = new SqlParameter("@QBItemAlias", SqlDbType.VarChar);
            if (String.IsNullOrEmpty(sQBItem)) _params[10].Value = DBNull.Value;
            else _params[10].Value = sQBItem;
            UpdateData("sp_UpdateAccountRate", _params);
        }

        public static void InsertAccountRate(int companyID, int accountID,
            int billingMethodID, decimal flatFee, decimal hourlyBlendedRate, int ratePlanID, int flatFeeMode, DateTime dtNextDate, string sQBAccount, string sQBItem)
        {
            SqlParameter[] _params = new SqlParameter[10];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);

            _params[2] = new SqlParameter("@BillingMethodID", billingMethodID);
            _params[3] = new SqlParameter("@FlatFee", SqlDbType.Money);
            if (flatFee > 0)
            {
                _params[3].Value = flatFee;
            }
            else
            {
                _params[3].Value = DBNull.Value;
            }
            _params[4] = new SqlParameter("@HourlyBlendedRate", SqlDbType.SmallMoney);
            if (hourlyBlendedRate > 0)
            {
                _params[4].Value = hourlyBlendedRate;
            }
            else
            {
                _params[4].Value = DBNull.Value;
            }
            _params[5] = new SqlParameter("@RatePlanID", SqlDbType.Int);
            if (ratePlanID > 0)
            {
                _params[5].Value = ratePlanID;
            }
            else
            {
                _params[5].Value = DBNull.Value;
            }
            _params[6] = new SqlParameter("@FlatFeeMode", SqlDbType.Int);
            if (flatFeeMode > -1) _params[6].Value = flatFeeMode;
            else _params[6].Value = DBNull.Value;
            _params[7] = new SqlParameter("@FlatFeeNextDate", SqlDbType.SmallDateTime);
            if (dtNextDate > DateTime.MinValue) _params[7].Value = dtNextDate;
            else _params[7].Value = DBNull.Value;

            _params[8] = new SqlParameter("@QBAccountAlias", SqlDbType.VarChar);
            if (String.IsNullOrEmpty(sQBAccount)) _params[8].Value = DBNull.Value;
            else _params[8].Value = sQBAccount;

            _params[9] = new SqlParameter("@QBItemAlias", SqlDbType.VarChar);
            if (String.IsNullOrEmpty(sQBItem)) _params[9].Value = DBNull.Value;
            else _params[9].Value = sQBItem;
            UpdateData("sp_InsertAccountRate", _params);
        }


        public static void ClearAccountTaskTypeRates(int companyID, int accountID)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);

            UpdateData("sp_ClearAccountTaskTypeRates", _params);
        }

        public static void InsertAccountTech(int companyID, int accountID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);
            _params[2] = new SqlParameter("@TechID", techID);
            UpdateData("sp_InsertAccountTech", _params);
        }

        public static DataTable SelectAccountTechs(int accountID, int companyID)
        {
            return SelectRecords("sp_SelectAccountTechs", new SqlParameter[]
                   {
                    new SqlParameter("@AccountID", accountID),
                    new SqlParameter("@CompanyID", companyID)
                   });
        }

        public static DataTable SelectAccountTaskTypeRates(int accountID, int companyID,
            int ratePlanID)
        {
            return SelectRecords("sp_SelectAccountTaskTypeRates", new SqlParameter[]
                   {
                    new SqlParameter("@AccountID", accountID),
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@RatePlanID", ratePlanID)
                   });
        }

        public static void InsertAccountTaskTypeRate(int companyID, int accountID, int taskTypeID,
            decimal hourlyRate)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);
            _params[2] = new SqlParameter("@TaskTypeID", taskTypeID);
            _params[3] = new SqlParameter("@HourlyRate", hourlyRate);
            UpdateData("sp_InsertAccountTaskTypeRate", _params);
        }

        public static void DeleteAccountTech(int accountTechID, int companyID)
        {
            UpdateData("sp_DeleteAccountTech", new SqlParameter[]
            {
                 new SqlParameter("@AccountTechID", accountTechID),
                 new SqlParameter("@CompanyID", companyID)
            });
        }

        public static void DeleteAccountTaskTypeRate(int accountTaskTypeRateID, int companyID)
        {
            UpdateData("sp_DeleteAccountTaskTypeRate", new SqlParameter[]
            {
                 new SqlParameter("@AccountTaskTypeRateID", accountTaskTypeRateID),
                 new SqlParameter("@CompanyID", companyID)
            });
        }

        public static void UpdateAccountTaskTypeRate(int companyID, int accountTaskTypeRateID, decimal hourlyRate)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountTaskTypeRateID", accountTaskTypeRateID);
            _params[2] = new SqlParameter("@HourlyRate", hourlyRate);

            UpdateData("sp_UpdateAccountTaskTypeRate", _params);
        }

        public static void DeleteAccountRate(int accountID, int companyID)
        {
            UpdateData("sp_DeleteAccountRate", new SqlParameter[]
            {
                new SqlParameter("@AccountID", accountID),
                new SqlParameter("@CompanyID", companyID)
            });
        }

        public static void DeleteAllAccountTechs(int companyID, int accountID)
        {
            UpdateData("sp_DeleteAllAccountTechs", new SqlParameter[]
            {
                new SqlParameter("@CompanyID", companyID),
                new SqlParameter("@AccountID", accountID)
            });
        }

        public static DataTable SelectAcctProjectsBillMethods(int companyID, Data.InactiveStatus entityStatus)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@Active", SqlDbType.Bit);
            switch (entityStatus)
            {
                case InactiveStatus.DoesntMatter: _params[1].Value = DBNull.Value;
                    break;
                case InactiveStatus.Active: _params[1].Value = 1;
                    break;
                case InactiveStatus.Inactive: _params[1].Value = 0;
                    break;
            }
            return SelectRecords("sp_SelectAcctProjectsBillMethods", _params);
        }

        public static DataTable SelectAccountRatesHierarchy(int companyID, int AccountTaskTypeRateID)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@AccountTaskTypeRateID", AccountTaskTypeRateID);
            _params[1] = new SqlParameter("@CompanyID", companyID);
            return SelectRecords("sp_SelectAccountRatesHierarchy", _params);
        }

        public static DataTable SelectAccProjectsMiscList(int DId, Data.InactiveStatus entityStatus, bool btCfgAcctMngr)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", DId);
            _params[1] = new SqlParameter("@Active", SqlDbType.Bit);
            switch (entityStatus)
            {
                case InactiveStatus.DoesntMatter: _params[1].Value = DBNull.Value;
                    break;
                case InactiveStatus.Active: _params[1].Value = 1;
                    break;
                case InactiveStatus.Inactive: _params[1].Value = 0;
                    break;
            }
            _params[2] = new SqlParameter("@btCfgAcctMngr", btCfgAcctMngr);
            return SelectRecords("sp_SelectAccountsProjectsMiscList", _params);
        }

        public static DataTable SelectAccountAvailableTechs(int CompanyID, int AccountID)
        {
            return SelectRecords("sp_SelectAccountProjectTechs", new SqlParameter[] { new SqlParameter("@DepartmentId", CompanyID), new SqlParameter("@AccountID", AccountID) });
        }

        public static bool HasAccountTimeSettings(int DepartmentID, int AccountID)
        {
            DataRow _row = SelectAccountRate(DepartmentID, AccountID);
            if (_row != null) if (!_row.IsNull("BillingMethodID")) return true;
            DataTable dt = SelectAccountTaskTypeRates(AccountID, DepartmentID, 0);
            if (dt.Rows.Count > 0) return true;
            dt = SelectAccountTechs(AccountID, DepartmentID);
            if (dt.Rows.Count > 0) return true;
            return false;
        }

        public static DataTable SelectAccountRetainers(int companyID, int accountID)
        {
            return SelectRecords("sp_SelectAccountRetainers", new SqlParameter[]
                   {
                       new SqlParameter("@CompanyID", companyID),
                       new SqlParameter("@AccountID", accountID)                    
                   });
        }

        public static void InsertAccountRetainer(int companyID, int accountID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);
            _params[2] = new SqlParameter("@TechID", techID);
            UpdateData("sp_InsertAccountRetainer", _params);
        }

        public static void DeleteAccountRetainer(int companyID, int accountRetainerID)
        {
            UpdateData("sp_DeleteAccountRetainer", new SqlParameter[]
            {
                new SqlParameter("@CompanyID", companyID),
                new SqlParameter("@AccountRetainerID", accountRetainerID)                 
            });
        }

        public static void UpdateRetainerAmount(int companyID, int itemId, decimal newValue, DateTime newStartDate, DateTime newEndDate)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", companyID);
            _params[1] = new SqlParameter("@Id", itemId);
            _params[2] = new SqlParameter("@NewAmount", newValue);
            _params[3] = new SqlParameter("@NewStartDate", newStartDate);
            _params[4] = new SqlParameter("@NewEndDate", SqlDbType.SmallDateTime);
            if (newEndDate == DateTime.MinValue) _params[4].Value = DBNull.Value;
            else _params[4].Value = newEndDate;
            UpdateData("sp_UpdateAccountRetainerAmount", _params);
        }

        public static DataTable SelectAccountLevelTimeTracking(int companyID, int accountID, int userID)
        {
            return SelectRecords("sp_SelectAccountLevelTimeTracking", new SqlParameter[]
            {
                 new SqlParameter("@CompanyID", companyID),
                 new SqlParameter("@AccountID", accountID),
                 new SqlParameter("@UserID", userID)
            });
        }

        public static DataTable SelectAccountsAssignedToTech(int companyID, int userID)
        {
            return SelectRecords("sp_SelectAccountsAssignedToTech", new SqlParameter[]
            {
                 new SqlParameter("@CompanyID", companyID),
                 new SqlParameter("@UserId", userID)
            });
        }

        public static void InactivateAccount(int companyID, int accountID, bool active)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@AccountID", accountID);
            _params[2] = new SqlParameter("@Active", active);

            UpdateData("sp_InactivateAccount", _params);
        }

        public static DataTable SelectAccountPrimaryContact(int departmentId, int accountID)
        {
            return SelectRecords("sp_SelectAccountPrimaryContact", new SqlParameter[]
            {
                 new SqlParameter("@DepartmentId", departmentId),
                 new SqlParameter("@AccountID", accountID)
            });
        }

        public static DataRow SelecttTicketStats(int departmentId, int accountID)
        {
            return SelectRecord("sp_SelectAccountTicketStats", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", departmentId), 
                new SqlParameter("@AccountID", accountID) 
            });
        }

        public static void UpdateAccountPrimaryContact(int deptID, int accountID, int userID)
        {
            UpdateData("sp_UpdateAccountPrimaryContact", 
                new SqlParameter[] { 
                    new SqlParameter("@DepartmentId", deptID), 
                    new SqlParameter("@AccountID", accountID), 
                    new SqlParameter("@UserID", userID)
                });
        }

        public static void UpdateUserPrimaryAccount(int deptID, int userID, int accountID)
        {
            SqlParameter _pAccountID = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountID > 0) _pAccountID.Value = accountID;
            else _pAccountID.Value = DBNull.Value;
            UpdateData("sp_UpdateUserPrimaryAccount",
                new SqlParameter[] { 
                    new SqlParameter("@DepartmentId", deptID), 
                    new SqlParameter("@UserID", userID),
                    _pAccountID
                });
        }
    }
}
