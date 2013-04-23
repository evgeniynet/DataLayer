using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Web;
using System.Data.SqlClient;
using System.Collections.Specialized;
using System.Collections;
//using Micajah.Common;

namespace bigWebApps.bigWebDesk.Data
{
	public enum StandardDateRange
	{
		Custom = 0,
		Today = 1,
		ThisWeek = 2,
		ThisMonth = 3,
		ThisYear = 4,
		ThisFiscalYear = 5,
		LastWeek = 6,
		LastMonth = 7,
		LastYear = 8,
		LastFiscalYear = 9,
		Rolling30Days = 10,
		Rolling90Days = 11,
		Rolling365Days = 12,
	}

	public class Assets : DBAccess
	{
		public struct AssetItem
		{
			public int ID;
			public string Description;
			public string SerialTagNumber;
			public AssetItem(int id, string description)
			{
				ID = id;
				Description = description;
				SerialTagNumber = string.Empty;
			}

			public AssetItem(string serialtagnumber, string description)
			{
				SerialTagNumber = serialtagnumber;
				Description = description;
				ID = 0;
			}
		}

		public enum BrowseColumn
		{
			Blank,
			Location,
			Owner,
			Vendor,
			WarrantyVendor,
			AssetName,
			PONumber,
			FundingCode,
			DateAquired,
			DatePurchased,
			DateReceived,
			DateEntered,
			DateDeployed,
			DateOutOfService,
			DateDisposed,
			ValueCurrent,
			ValueReplacement,
			ValueDepreciated,
			ValueSalvage,
			DisposalCost,
			SerialNumber,
			Status,
			CheckedOutTo,
			DateUpdated,
			AuditNote
		}

		public enum ColumnTypeInfo
		{
			Table,
			Field,
			Header
		}

		public class ColumnsSetting
		{
			private int m_Id = 0;
			private string m_Name = string.Empty;
			private BrowseColumn m_Col1 = BrowseColumn.Blank;
			private BrowseColumn m_Col2 = BrowseColumn.Blank;
			private BrowseColumn m_Col3 = BrowseColumn.Blank;
			private BrowseColumn m_Col4 = BrowseColumn.Blank;
			private BrowseColumn m_Col5 = BrowseColumn.Blank;
			private BrowseColumn m_Col6 = BrowseColumn.Blank;
			private BrowseColumn m_Col7 = BrowseColumn.Blank;
			private BrowseColumn m_Col8 = BrowseColumn.Blank;

			private BrowseColumn m_SortCol1 = BrowseColumn.Blank;
			private BrowseColumn m_SortCol2 = BrowseColumn.Blank;
			private BrowseColumn m_SortCol3 = BrowseColumn.Blank;

			private bool m_SortOrdDesc1 = false;
			private bool m_SortOrdDesc2 = false;
			private bool m_SortOrdDesc3 = false;

			public ColumnsSetting()
			{
			}

			public ColumnsSetting(int mode)
			{
				//mode=1 get object from Session
				//mode=2 get object from query string
				switch (mode)
				{
					case 1:
						UserSetting _us = UserSetting.GetSettings("AssetCS");
						if (!_us.IsDefined) return;
						InitObjectFromQueryString(_us.QueryString);
						return;
					case 2:
						InitObjectFromQueryString(HttpContext.Current.Request.QueryString.ToString());
						return;
				}
			}

			public ColumnsSetting(int departmentId, int settingId)
			{
				if (settingId == 0) return;
				SqlParameter _pUId = new SqlParameter("@UId", SqlDbType.Int);
				_pUId.Value = DBNull.Value;
				DataRow _row = Data.DBAccess.SelectRecord("sp_SelectAssetColumnSettings", new SqlParameter[] { new SqlParameter("@DId", departmentId), _pUId, new SqlParameter("@Id", settingId) });
				if (_row == null) return;
				InitObjectFromQueryString(_row["State"].ToString());
				m_Id = settingId;
			}

			private void InitObjectFromQueryString(string querystr)
			{
				string[] _state = querystr.ToString().Split('&');
				System.Collections.Specialized.StringDictionary _sd = new System.Collections.Specialized.StringDictionary();
				for (int i = 0; i < _state.Length; i++)
				{
					if (_state[i].Length == 0) continue;
					string[] _item = _state[i].Split('=');
					if (_item.Length > 1) _sd.Add(_item[0], _item[1]);
					else _sd.Add(_item[0], string.Empty);
				}
				if (_sd.ContainsKey("ci") && _sd["ci"].Length > 0) m_Id = int.Parse(_sd["ci"]);
				if (_sd.ContainsKey("cn")) m_Name = HttpUtility.UrlDecode(_sd["cn"]);
				if (_sd.ContainsKey("c1") && _sd["c1"].Length > 0) m_Col1 = (BrowseColumn)int.Parse(_sd["c1"]);
				if (_sd.ContainsKey("c2") && _sd["c2"].Length > 0) m_Col2 = (BrowseColumn)int.Parse(_sd["c2"]);
				if (_sd.ContainsKey("c3") && _sd["c3"].Length > 0) m_Col3 = (BrowseColumn)int.Parse(_sd["c3"]);
				if (_sd.ContainsKey("c4") && _sd["c4"].Length > 0) m_Col4 = (BrowseColumn)int.Parse(_sd["c4"]);
				if (_sd.ContainsKey("c5") && _sd["c5"].Length > 0) m_Col5 = (BrowseColumn)int.Parse(_sd["c5"]);
				if (_sd.ContainsKey("c6") && _sd["c6"].Length > 0) m_Col6 = (BrowseColumn)int.Parse(_sd["c6"]);
				if (_sd.ContainsKey("c7") && _sd["c7"].Length > 0) m_Col7 = (BrowseColumn)int.Parse(_sd["c7"]);
				if (_sd.ContainsKey("c8") && _sd["c8"].Length > 0) m_Col8 = (BrowseColumn)int.Parse(_sd["c8"]);
				if (_sd.ContainsKey("sc1") && _sd["sc1"].Length > 0) m_SortCol1 = (BrowseColumn)int.Parse(_sd["sc1"]);
				if (_sd.ContainsKey("sc2") && _sd["sc2"].Length > 0) m_SortCol2 = (BrowseColumn)int.Parse(_sd["sc2"]);
				if (_sd.ContainsKey("sc3") && _sd["sc3"].Length > 0) m_SortCol3 = (BrowseColumn)int.Parse(_sd["sc3"]);
				if (_sd.ContainsKey("oc1") && _sd["oc1"] == "1") m_SortOrdDesc1 = true;
				if (_sd.ContainsKey("oc2") && _sd["oc2"] == "1") m_SortOrdDesc2 = true;
				if (_sd.ContainsKey("oc3") && _sd["oc3"] == "1") m_SortOrdDesc3 = true;
			}

			#region Public Methods
			public string GetQueryString()
			{
				string _st = string.Empty;
				_st += "&ci=" + m_Id.ToString();
				if (m_Name.Length > 0) _st += "&cn=" + HttpUtility.UrlEncode(m_Name);
				if (m_Col1 != BrowseColumn.Blank) _st += "&c1=" + ((int)m_Col1).ToString();
				if (m_Col2 != BrowseColumn.Blank) _st += "&c2=" + ((int)m_Col2).ToString();
				if (m_Col3 != BrowseColumn.Blank) _st += "&c3=" + ((int)m_Col3).ToString();
				if (m_Col4 != BrowseColumn.Blank) _st += "&c4=" + ((int)m_Col4).ToString();
				if (m_Col5 != BrowseColumn.Blank) _st += "&c5=" + ((int)m_Col5).ToString();
				if (m_Col6 != BrowseColumn.Blank) _st += "&c6=" + ((int)m_Col6).ToString();
				if (m_Col7 != BrowseColumn.Blank) _st += "&c7=" + ((int)m_Col7).ToString();
				if (m_Col8 != BrowseColumn.Blank) _st += "&c8=" + ((int)m_Col8).ToString();
				if (m_SortCol1 != BrowseColumn.Blank) _st += "&sc1=" + ((int)m_SortCol1).ToString();
				if (m_SortOrdDesc1) _st += "&oc1=1";
				if (m_SortCol2 != BrowseColumn.Blank) _st += "&sc2=" + ((int)m_SortCol2).ToString();
				if (m_SortOrdDesc2) _st += "&oc2=1";
				if (m_SortCol3 != BrowseColumn.Blank) _st += "&sc3=" + ((int)m_SortCol3).ToString();
				if (m_SortOrdDesc3) _st += "&oc3=1";
				_st = _st.TrimStart('&');
				return _st;
			}

			public void SaveToSession()
			{
				UserSetting _us = UserSetting.GetSettings("AssetCS");
				_us.QueryString = GetQueryString();
			}

			public int SaveToDatabase(int DeptID, int SettingID, int UserID)
			{
				SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				_pRVAL.Direction = ParameterDirection.ReturnValue;
				SqlParameter _pId = new SqlParameter("@Id", SettingID);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateAssetColumnSetting", new SqlParameter[]{_pRVAL, _pId,
					new SqlParameter("@DId", DeptID),
					new SqlParameter("@UId", UserID),
					new SqlParameter("@Name", m_Name),
					new SqlParameter("@State", GetQueryString())});
				if ((int)_pRVAL.Value > 0) return -(int)_pRVAL.Value;
				else
				{
					m_Id = (int)_pId.Value;
					return m_Id;
				}
			}

			public static DataTable SelectSettings(int DeptID, int UserID)
			{
				return SelectRecords("sp_SelectAssetColumnSettings", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID) });
			}

			public static void DeleteSetting(int DeptID, int SettingID)
			{
				UpdateData("sp_DeleteAssetColumnSetting", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", SettingID) });
			}

			#endregion
			#region Public Properties
			public int ID
			{
				get { return m_Id; }
				set { m_Id = value; }
			}
			public string Name
			{
				set { m_Name = value; }
				get { return m_Name; }
			}

			public BrowseColumn Column1
			{
				set { m_Col1 = value; }
				get { return m_Col1; }
			}

			public BrowseColumn Column2
			{
				set { m_Col2 = value; }
				get { return m_Col2; }
			}

			public BrowseColumn Column3
			{
				set { m_Col3 = value; }
				get { return m_Col3; }
			}

			public BrowseColumn Column4
			{
				set { m_Col4 = value; }
				get { return m_Col4; }
			}

			public BrowseColumn Column5
			{
				set { m_Col5 = value; }
				get { return m_Col5; }
			}

			public BrowseColumn Column6
			{
				set { m_Col6 = value; }
				get { return m_Col6; }
			}

			public BrowseColumn Column7
			{
				set { m_Col7 = value; }
				get { return m_Col7; }
			}

			public BrowseColumn Column8
			{
				set { m_Col8 = value; }
				get { return m_Col8; }
			}

			public BrowseColumn SortColumn1
			{
				set { m_SortCol1 = value; }
				get { return m_SortCol1; }
			}

			public BrowseColumn SortColumn2
			{
				set { m_SortCol2 = value; }
				get { return m_SortCol2; }
			}

			public BrowseColumn SortColumn3
			{
				set { m_SortCol3 = value; }
				get { return m_SortCol3; }
			}

			public bool SortColumnDesc1
			{
				set { m_SortOrdDesc1 = value; }
				get { return m_SortOrdDesc1; }
			}

			public bool SortColumnDesc2
			{
				set { m_SortOrdDesc2 = value; }
				get { return m_SortOrdDesc2; }
			}

			public bool SortColumnDesc3
			{
				set { m_SortOrdDesc3 = value; }
				get { return m_SortOrdDesc3; }
			}
			#endregion
		}

		public class Filter
		{
			private int _departmentId = 0;
			private int m_Id = 0;
			private string m_FilterName = string.Empty;
			private int m_CategoryID = 0;
			private int m_TypeID = 0;
			private int m_MakeID = 0;
			private int m_ModelID = 0;
			private int m_StatusID = 0;
			private int m_AccountID = 0;
            private int m_TicketAccountID = 0;
			private int m_LocationID = 0;
			private string m_LocationName = string.Empty;
			private int m_OwnerID = 0;
			private int m_CheckedOutID = 0;
			private int m_VendorID = 0;
			private int m_WarrantyVendorID = 0;
			private string m_SerialNumber = string.Empty;
			private string m_Name = string.Empty;
			private string m_PONumber = string.Empty;
			private string m_FundingCode = string.Empty;
			private DateTime m_DateStart = DateTime.MinValue;
			private DateTime m_DateEnd = DateTime.MinValue;
			private StandardDateRange m_DateRange = StandardDateRange.Custom;
			private bool m_DateAquired = false;
			private bool m_DatePurchased = false;
			private bool m_DateReceived = false;
			private bool m_DateEntered = false;
			private bool m_DateDeployed = false;
			private bool m_DateOutOfService = false;
			private bool m_DateDisposed = false;
			private bool m_DateUpdated = false;
			private string m_ValueCurrent = string.Empty;
			private string m_ValueReplacement = string.Empty;
			private string m_ValueDepreciated = string.Empty;
			private string m_ValueSalvage = string.Empty;
			private string m_DisposalCost = string.Empty;
			private AssetTypeProperty[] m_CustomProperties = new AssetTypeProperty[] { };
			private string m_CustomWhere;

			public Filter(int departmentId, int filterId)
			{
				_departmentId = departmentId;
				m_Id = filterId;
				if (filterId == 0) return;
				SqlParameter _pUId = new SqlParameter("@UId", SqlDbType.Int);
				_pUId.Value = DBNull.Value;
				DataRow _row = Data.DBAccess.SelectRecord("sp_SelectAssetFilters", new[] { new SqlParameter("@DId", departmentId), _pUId, new SqlParameter("@Id", filterId) });
				if (_row == null) return;
				string[] _state = _row["FilterState"].ToString().Split('&');
				System.Collections.Specialized.StringDictionary _sd = new System.Collections.Specialized.StringDictionary();
				for (int i = 0; i < _state.Length; i++)
				{
					if (_state[i].Length == 0) continue;
					string[] _item = _state[i].Split('=');
					if (_item.Length > 1) _sd.Add(_item[0], _item[1]);
					else _sd.Add(_item[0], string.Empty);
				}
				if (_sd.ContainsKey("fn")) m_FilterName = HttpUtility.UrlDecode(_sd["fn"]);
				if (_sd.ContainsKey("ca") && _sd["ca"].Length > 0) m_CategoryID = int.Parse(_sd["ca"]);
				if (_sd.ContainsKey("ty") && _sd["ty"].Length > 0) TypeID = int.Parse(_sd["ty"]);
				if (_sd.ContainsKey("ma") && _sd["ma"].Length > 0) m_MakeID = int.Parse(_sd["ma"]);
				if (_sd.ContainsKey("mo") && _sd["mo"].Length > 0) m_ModelID = int.Parse(_sd["mo"]);
				if (_sd.ContainsKey("st") && _sd["st"].Length > 0) m_StatusID = int.Parse(_sd["st"]);
				if (_sd.ContainsKey("ac") && _sd["ac"].Length > 0) m_AccountID = int.Parse(_sd["ac"]);
				if (_sd.ContainsKey("lo") && _sd["lo"].Length > 0) m_LocationID = int.Parse(_sd["lo"]);
				if (_sd.ContainsKey("ln")) m_LocationName = HttpUtility.UrlDecode(_sd["ln"]);
				if (_sd.ContainsKey("ow") && _sd["ow"].Length > 0) m_OwnerID = int.Parse(_sd["ow"]);
				if (_sd.ContainsKey("co") && _sd["co"].Length > 0) m_CheckedOutID = int.Parse(_sd["co"]);
				if (_sd.ContainsKey("vn") && _sd["vn"].Length > 0) m_VendorID = int.Parse(_sd["vn"]);
				if (_sd.ContainsKey("wv") && _sd["wv"].Length > 0) m_WarrantyVendorID = int.Parse(_sd["wv"]);
				if (_sd.ContainsKey("sn")) m_SerialNumber = HttpUtility.UrlDecode(_sd["sn"]);
				if (_sd.ContainsKey("an")) m_Name = HttpUtility.UrlDecode(_sd["an"]);
				if (_sd.ContainsKey("po")) m_PONumber = HttpUtility.UrlDecode(_sd["po"]);
				if (_sd.ContainsKey("fc")) m_FundingCode = HttpUtility.UrlDecode(_sd["fc"]);
				if (_sd.ContainsKey("ds") && _sd["ds"].Length > 0) m_DateStart = DateTime.Parse(HttpUtility.UrlDecode(_sd["ds"]), new System.Globalization.CultureInfo("en-US"));
				if (_sd.ContainsKey("de") && _sd["de"].Length > 0) m_DateEnd = DateTime.Parse(HttpUtility.UrlDecode(_sd["de"]), new System.Globalization.CultureInfo("en-US"));
				if (_sd.ContainsKey("dr") && _sd["dr"].Length > 0) m_DateRange = (StandardDateRange)Enum.Parse(typeof(StandardDateRange), _sd["dr"], true);
				if (_sd.ContainsKey("ba") && _sd["ba"] == "1") m_DateAquired = true;
				if (_sd.ContainsKey("bp") && _sd["bp"] == "1") m_DatePurchased = true;
				if (_sd.ContainsKey("br") && _sd["br"] == "1") m_DateReceived = true;
				if (_sd.ContainsKey("be") && _sd["be"] == "1") m_DateEntered = true;
				if (_sd.ContainsKey("bd") && _sd["bd"] == "1") m_DateDeployed = true;
				if (_sd.ContainsKey("bo") && _sd["bo"] == "1") m_DateOutOfService = true;
				if (_sd.ContainsKey("bs") && _sd["bs"] == "1") m_DateDisposed = true;
				if (_sd.ContainsKey("bu") && _sd["bu"] == "1") m_DateUpdated = true;
				if (_sd.ContainsKey("vc")) m_ValueCurrent = HttpUtility.UrlDecode(_sd["vc"]);
				if (_sd.ContainsKey("vr")) m_ValueReplacement = HttpUtility.UrlDecode(_sd["vr"]);
				if (_sd.ContainsKey("vd")) m_ValueDepreciated = HttpUtility.UrlDecode(_sd["vd"]);
				if (_sd.ContainsKey("vs")) m_ValueSalvage = HttpUtility.UrlDecode(_sd["vs"]);
				if (_sd.ContainsKey("dc")) m_DisposalCost = HttpUtility.UrlDecode(_sd["dc"]);
				foreach (AssetTypeProperty _prop in m_CustomProperties)
					if (_sd.ContainsKey("cp" + _prop.ID.ToString())) _prop.Value = HttpUtility.UrlDecode(_sd["cp" + _prop.ID.ToString()]);
			}

			public Filter(int departmentId, bool isNew)
			{
				_departmentId = departmentId;
				if (isNew) return;

				UserSetting _c = UserSetting.GetSettings("AssetS");
				if (!_c.IsDefined) return;
				if (_c["FI"] != null && _c["FI"].Length > 0) m_Id = int.Parse(_c["FI"]);
				if (_c["FN"] != null) m_FilterName = _c["FN"];
				if (_c["CA"] != null && _c["CA"].Length > 0) m_CategoryID = int.Parse(_c["CA"]);
				if (_c["TY"] != null && _c["TY"].Length > 0) TypeID = int.Parse(_c["TY"]);
				if (_c["MA"] != null && _c["MA"].Length > 0) m_MakeID = int.Parse(_c["MA"]);
				if (_c["MO"] != null && _c["MO"].Length > 0) m_ModelID = int.Parse(_c["MO"]);
				if (_c["ST"] != null && _c["ST"].Length > 0) m_StatusID = int.Parse(_c["ST"]);
				if (_c["AC"] != null && _c["AC"].Length > 0) m_AccountID = int.Parse(_c["AC"]);
				if (_c["LO"] != null && _c["LO"].Length > 0) m_LocationID = int.Parse(_c["LO"]);
				if (_c["LN"] != null) m_LocationName = HttpUtility.UrlDecode(_c["LN"]);
				if (_c["OW"] != null && _c["OW"].Length > 0) m_OwnerID = int.Parse(_c["OW"]);
				if (_c["CO"] != null && _c["CO"].Length > 0) m_CheckedOutID = int.Parse(_c["CO"]);
				if (_c["VN"] != null && _c["VN"].Length > 0) m_VendorID = int.Parse(_c["VN"]);
				if (_c["WV"] != null && _c["WV"].Length > 0) m_WarrantyVendorID = int.Parse(_c["WV"]);
				if (_c["SN"] != null) m_SerialNumber = HttpUtility.UrlDecode(_c["SN"]);
				if (_c["AN"] != null) m_Name = HttpUtility.UrlDecode(_c["AN"]);
				if (_c["PO"] != null) m_PONumber = HttpUtility.UrlDecode(_c["PO"]);
				if (_c["FC"] != null) m_FundingCode = HttpUtility.UrlDecode(_c["FC"]);
				if (_c["DS"] != null && _c["DS"].Length > 0) m_DateStart = DateTime.Parse(HttpUtility.UrlDecode(_c["DS"]), new System.Globalization.CultureInfo("en-US"));
				if (_c["DE"] != null && _c["DE"].Length > 0) m_DateEnd = DateTime.Parse(HttpUtility.UrlDecode(_c["DE"]), new System.Globalization.CultureInfo("en-US"));
				if (_c["DR"] != null && _c["DR"].Length > 0) m_DateRange = (StandardDateRange)Enum.Parse(typeof(StandardDateRange), _c["DR"], true);
				if (_c["BA"] != null && _c["BA"].Length > 0) m_DateAquired = bool.Parse(_c["BA"]);
				if (_c["BP"] != null && _c["BP"].Length > 0) m_DatePurchased = bool.Parse(_c["BP"]);
				if (_c["BR"] != null && _c["BR"].Length > 0) m_DateReceived = bool.Parse(_c["BR"]);
				if (_c["BE"] != null && _c["BE"].Length > 0) m_DateEntered = bool.Parse(_c["BE"]);
				if (_c["BD"] != null && _c["BD"].Length > 0) m_DateDeployed = bool.Parse(_c["BD"]);
				if (_c["BO"] != null && _c["BO"].Length > 0) m_DateOutOfService = bool.Parse(_c["BO"]);
				if (_c["BS"] != null && _c["BS"].Length > 0) m_DateDisposed = bool.Parse(_c["BS"]);
				if (_c["BU"] != null && _c["BU"].Length > 0) m_DateUpdated = bool.Parse(_c["BU"]);
				if (_c["VC"] != null) m_ValueCurrent = HttpUtility.UrlDecode(_c["VC"]);
				if (_c["VR"] != null) m_ValueReplacement = HttpUtility.UrlDecode(_c["VR"]);
				if (_c["VD"] != null) m_ValueDepreciated = HttpUtility.UrlDecode(_c["VD"]);
				if (_c["VS"] != null) m_ValueSalvage = HttpUtility.UrlDecode(_c["VS"]);
				if (_c["DC"] != null) m_DisposalCost = HttpUtility.UrlDecode(_c["DC"]);
				foreach (AssetTypeProperty _prop in m_CustomProperties)
					if (_c["CP" + _prop.ID.ToString()] != null) _prop.Value = HttpUtility.UrlDecode(_c["CP" + _prop.ID.ToString()]);
			}

			#region Public Methods

			public void SaveToSession()
			{
				UserSetting _c = UserSetting.GetSettings("AssetS");
				_c["FI"] = m_Id.ToString();
				_c["FN"] = HttpUtility.UrlEncode(m_FilterName);
				_c["CA"] = m_CategoryID.ToString();
				_c["TY"] = m_TypeID.ToString();
				_c["MA"] = m_MakeID.ToString();
				_c["MO"] = m_ModelID.ToString();
				_c["ST"] = m_StatusID.ToString();
				_c["AC"] = m_AccountID.ToString();
				_c["LO"] = m_LocationID.ToString();
				_c["LN"] = HttpUtility.UrlEncode(m_LocationName);
				_c["OW"] = m_OwnerID.ToString();
				_c["CO"] = m_CheckedOutID.ToString();
				_c["VN"] = m_VendorID.ToString();
				_c["WV"] = m_WarrantyVendorID.ToString();
				_c["SN"] = HttpUtility.UrlEncode(m_SerialNumber);
				_c["AN"] = HttpUtility.UrlEncode(m_Name);
				_c["PO"] = HttpUtility.UrlEncode(m_PONumber);
				_c["FC"] = HttpUtility.UrlEncode(m_FundingCode);
				_c["DS"] = HttpUtility.UrlEncode(m_DateStart.ToString(new System.Globalization.CultureInfo("en-US")));
				_c["DE"] = HttpUtility.UrlEncode(m_DateEnd.ToString(new System.Globalization.CultureInfo("en-US")));
				_c["DR"] = m_DateRange.ToString();
				_c["BA"] = m_DateAquired.ToString();
				_c["BP"] = m_DatePurchased.ToString();
				_c["BR"] = m_DateReceived.ToString();
				_c["BE"] = m_DateEntered.ToString();
				_c["BD"] = m_DateDeployed.ToString();
				_c["BO"] = m_DateOutOfService.ToString();
				_c["BS"] = m_DateDisposed.ToString();
				_c["BU"] = m_DateUpdated.ToString();
				_c["VC"] = HttpUtility.UrlEncode(m_ValueCurrent);
				_c["VR"] = HttpUtility.UrlEncode(m_ValueReplacement);
				_c["VD"] = HttpUtility.UrlEncode(m_ValueDepreciated);
				_c["VS"] = HttpUtility.UrlEncode(m_ValueSalvage);
				_c["DC"] = HttpUtility.UrlEncode(m_DisposalCost);
				foreach (AssetTypeProperty _prop in m_CustomProperties) _c["CP" + _prop.ID.ToString()] = HttpUtility.UrlEncode(_prop.Value);
			}

			public int SaveToDatabase(int UserID)
			{
				string _fs = string.Empty;
				if (m_FilterName.Length > 0) _fs += "&fn=" + HttpUtility.UrlEncode(m_FilterName);
				if (m_CategoryID != 0) _fs += "&ca=" + m_CategoryID.ToString();
				if (m_TypeID != 0) _fs += "&ty=" + m_TypeID.ToString();
				if (m_MakeID != 0) _fs += "&ma=" + m_MakeID.ToString();
				if (m_ModelID != 0) _fs += "&mo=" + m_ModelID.ToString();
				if (m_StatusID != 0) _fs += "&st=" + m_StatusID.ToString();
				if (m_AccountID != 0) _fs += "&ac=" + m_AccountID.ToString();
				if (m_LocationID != 0) _fs += "&lo=" + m_LocationID.ToString();
				if (m_LocationName.Length > 0) _fs += "&ln=" + HttpUtility.UrlEncode(m_LocationName);
				if (m_OwnerID != 0) _fs += "&ow=" + m_OwnerID.ToString();
				if (m_CheckedOutID != 0) _fs += "&co=" + m_CheckedOutID.ToString();
				if (m_VendorID != 0) _fs += "&vn=" + m_VendorID.ToString();
				if (m_WarrantyVendorID != 0) _fs += "&wv=" + m_WarrantyVendorID.ToString();
				if (m_SerialNumber.Length > 0) _fs += "&sn=" + HttpUtility.UrlEncode(m_SerialNumber);
				if (m_Name.Length > 0) _fs += "&an=" + HttpUtility.UrlEncode(m_Name);
				if (m_PONumber.Length > 0) _fs += "&po=" + HttpUtility.UrlEncode(m_PONumber);
				if (m_FundingCode.Length > 0) _fs += "&fc=" + HttpUtility.UrlEncode(m_FundingCode);
				if (m_DateStart > DateTime.MinValue) _fs += "&ds=" + HttpUtility.UrlEncode(m_DateStart.ToString(new System.Globalization.CultureInfo("en-US")));
				if (m_DateEnd > DateTime.MinValue) _fs += "&de=" + HttpUtility.UrlEncode(m_DateEnd.ToString(new System.Globalization.CultureInfo("en-US")));
				if (m_DateRange != StandardDateRange.Custom) _fs += "&dr=" + m_DateRange.ToString();
				if (m_DateAquired) _fs += "&ba=1";
				if (m_DatePurchased) _fs += "&bp=1";
				if (m_DateReceived) _fs += "&br=1";
				if (m_DateEntered) _fs += "&be=1";
				if (m_DateDeployed) _fs += "&bd=1";
				if (m_DateOutOfService) _fs += "&bo=1";
				if (m_DateDisposed) _fs += "&bs=1";
				if (m_DateUpdated) _fs += "&bu=1";
				if (m_ValueCurrent.Length > 0) _fs += "&vc=" + HttpUtility.UrlEncode(m_ValueCurrent);
				if (m_ValueReplacement.Length > 0) _fs += "&vr=" + HttpUtility.UrlEncode(m_ValueReplacement);
				if (m_ValueDepreciated.Length > 0) _fs += "&vd=" + HttpUtility.UrlEncode(m_ValueDepreciated);
				if (m_ValueSalvage.Length > 0) _fs += "&vs=" + HttpUtility.UrlEncode(m_ValueSalvage);
				if (m_DisposalCost.Length > 0) _fs += "&dc=" + HttpUtility.UrlEncode(m_DisposalCost);
				_fs = _fs.TrimStart('&');
				foreach (AssetTypeProperty _prop in m_CustomProperties)
					if (_prop.Value.Length > 0) _fs += "&cp" + _prop.ID.ToString() + "=" + HttpUtility.UrlEncode(_prop.Value);
				SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				_pRVAL.Direction = ParameterDirection.ReturnValue;
				SqlParameter _pId = new SqlParameter("@Id", m_Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateAssetFilter", new SqlParameter[]{_pRVAL, _pId,
					new SqlParameter("@DId", _departmentId),
					new SqlParameter("@UId", UserID),
					new SqlParameter("@Name", m_FilterName),
					new SqlParameter("@FilterState", _fs)});
				if ((int)_pRVAL.Value > 0) return -(int)_pRVAL.Value;
				else
				{
					m_Id = (int)_pId.Value;
					return m_Id;
				}
			}

			public static int GetFilterID(int departmentId, int userId, string FilterName)
			{
				DataTable t = SelectRecords("sp_SelectAssetFilters", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@UId", userId), new SqlParameter("@FilterName", FilterName) });
				if (t == null || t.Rows.Count <= 0) return 0;
				int id = (int)t.Rows[0]["Id"];
				return id;
			}

			public static DataTable SelectFilters(int departmentId, int userId)
			{
				return SelectRecords("sp_SelectAssetFilters", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@UId", userId) });
			}

			public static void Delete(int departmentId, int filterId)
			{
				UpdateData("sp_DeleteAssetFilter", new SqlParameter[] { new SqlParameter("@DId", departmentId), new SqlParameter("@Id", filterId) });
			}

			public static void UpdateColumnSetting(int departmentId, int filterId, int columnSettingId)
			{
				if (columnSettingId == 0) UpdateByQuery("UPDATE AssetFilters SET AssetColumnSettingId=NULL WHERE DId=" + departmentId.ToString() + " AND Id=" + filterId.ToString());
				else UpdateByQuery("UPDATE AssetFilters SET AssetColumnSettingId=" + columnSettingId.ToString() + " WHERE DId=" + departmentId.ToString() + " AND Id=" + filterId.ToString());
			}

			#endregion
			#region Public Properties
			public int ID
			{
				set { m_Id = value; }
				get { return m_Id; }
			}

			public string FilterName
			{
				set { m_FilterName = value; }
				get { return m_FilterName; }
			}
			public int CategoryID
			{
				set { m_CategoryID = value; }
				get { return m_CategoryID; }
			}
			public int TypeID
			{
				set
				{
					if (value == 0) m_CustomProperties = new AssetTypeProperty[] { };
					else if (m_TypeID != value) m_CustomProperties = AssetTypeProperties.SelectAllToPropertiesArray(_departmentId, value);
					m_TypeID = value;
				}
				get { return m_TypeID; }
			}
			public int MakeID
			{
				set { m_MakeID = value; }
				get { return m_MakeID; }
			}
			public int ModelID
			{
				set { m_ModelID = value; }
				get { return m_ModelID; }
			}
			public int StatusID
			{
				set { m_StatusID = value; }
				get { return m_StatusID; }
			}
			public int AccountID
			{
				set { m_AccountID = value; }
				get { return m_AccountID; }
			}
            public int TicketAccountID
            {
                set { m_TicketAccountID= value; }
                get { return m_TicketAccountID; }
            }
			public int LocationID
			{
				set { m_LocationID = value; }
				get { return m_LocationID; }
			}
			public string LocationName
			{
				set { m_LocationName = value; }
				get { return m_LocationName; }
			}
			public int OwnerID
			{
				set { m_OwnerID = value; }
				get { return m_OwnerID; }
			}
			public int CheckedOutID
			{
				set { m_CheckedOutID = value; }
				get { return m_CheckedOutID; }
			}
			public int VendorID
			{
				set { m_VendorID = value; }
				get { return m_VendorID; }
			}
			public int WarrantyVendorID
			{
				set { m_WarrantyVendorID = value; }
				get { return m_WarrantyVendorID; }
			}
			public string SerialNumber
			{
				set { m_SerialNumber = value; }
				get { return m_SerialNumber; }
			}
			public string AssetName
			{
				set { m_Name = value; }
				get { return m_Name; }
			}
			public string PONumber
			{
				set { m_PONumber = value; }
				get { return m_PONumber; }
			}
			public string FundingCode
			{
				set { m_FundingCode = value; }
				get { return m_FundingCode; }
			}
			public DateTime AssetDateStart
			{
				set { m_DateStart = value; }
				get { return m_DateStart; }
			}
			public DateTime AssetDateEnd
			{
				set { m_DateEnd = value; }
				get { return m_DateEnd; }
			}
			public StandardDateRange AssetDateRange
			{
				set { m_DateRange = value; }
				get { return m_DateRange; }
			}
			public bool CheckDateAquired
			{
				set { m_DateAquired = value; }
				get { return m_DateAquired; }
			}
			public bool CheckDatePurchased
			{
				set { m_DatePurchased = value; }
				get { return m_DatePurchased; }
			}
			public bool CheckDateReceived
			{
				set { m_DateReceived = value; }
				get { return m_DateReceived; }
			}
			public bool CheckDateEntered
			{
				set { m_DateEntered = value; }
				get { return m_DateEntered; }
			}
			public bool CheckDateDeployed
			{
				set { m_DateDeployed = value; }
				get { return m_DateDeployed; }
			}
			public bool CheckDateOutOfService
			{
				set { m_DateOutOfService = value; }
				get { return m_DateOutOfService; }
			}
			public bool CheckDateDisposed
			{
				set { m_DateDisposed = value; }
				get { return m_DateDisposed; }
			}
			public bool CheckDateUpdated
			{
				set { m_DateUpdated = value; }
				get { return m_DateUpdated; }
			}
			public string ValueCurrentExpression
			{
				set { m_ValueCurrent = value; }
				get { return m_ValueCurrent; }
			}
			public string ValueReplacementExpression
			{
				set { m_ValueReplacement = value; }
				get { return m_ValueReplacement; }
			}
			public string ValueDepreciatedExpression
			{
				set { m_ValueDepreciated = value; }
				get { return m_ValueDepreciated; }
			}
			public string ValueSalvageExpression
			{
				set { m_ValueSalvage = value; }
				get { return m_ValueSalvage; }
			}
			public string DisposalCostExpression
			{
				set { m_DisposalCost = value; }
				get { return m_DisposalCost; }
			}

			public AssetTypeProperty[] CustomProperties
			{
				get { return m_CustomProperties; }
			}

			public string CustomWhere
			{
				get { return m_CustomWhere; }
				set { m_CustomWhere = value; }
			}

			#endregion
		}

		public static string GetAssetCustomPropertiesByFilterSQLQuery(ref Filter filter, ref int custom_count)
		{
			string _custom_property = "";

			custom_count = 0;

			AssetTypeProperty[] _properties = (filter != null ? filter.CustomProperties : null);
			if (_properties != null)
			{
				int _new_counter = 0;

				foreach (AssetTypeProperty _property in _properties)
				{
					if (_property != null)
					{
						if ((_property.ID > 0) && (_property.Value.Length > 0))
						{
							if (_new_counter > 0)
							{
								_custom_property += " OR ";
							}
							else
							{
								_custom_property += " AND (";
							};

							_new_counter++;
							switch (_property.DataType)
							{
								case PropertyType.DateTime:
									_custom_property += "(";
									_custom_property += "AssetTypePropertyID=";
									_custom_property += _property.ID.ToString();

									string[] dvalues = _property.Value.Split(new char[] { ' ' });

									if ((dvalues != null) && (dvalues.Length == 2))
									{
										_custom_property += " AND ";

										_custom_property += "(CASE WHEN IsDATE(PropertyValue)=1 THEN CONVERT(datetime, PropertyValue, 120) ELSE NULL END)";
										_custom_property += dvalues[0].ToString();
										_custom_property += "'";

										try
										{
											DateTime _convert = DateTime.Parse(dvalues[1].ToString());
											_custom_property += Functions.FormatSQLShortDateTime(_convert);
										}
										catch
										{
										};

										_custom_property += "' ";
									};

									_custom_property += ")";

									break;
								case PropertyType.Numeric:
									_custom_property += "(";
									_custom_property += "AssetTypePropertyID=";
									_custom_property += _property.ID.ToString();

									string[] values = _property.Value.Split(new char[] { ' ' });

									if ((values != null) && (values.Length == 2))
									{
										_custom_property += " AND ";
										_custom_property += "(CASE WHEN IsNumeric(PropertyValue)=1 THEN CONVERT(numeric(18,4), replace(PropertyValue,',','.')) ELSE NULL END)";
										_custom_property += values[0].ToString();
										_custom_property += "CONVERT(numeric(18,4), replace('";
										_custom_property += values[1].ToString();
										_custom_property += "',',','.')) ";
									};

									_custom_property += ")";

									break;
								case PropertyType.Integer:
									_custom_property += "(";
									_custom_property += "AssetTypePropertyID=";
									_custom_property += _property.ID.ToString();
									_custom_property += " AND ";
									_custom_property += "(CASE WHEN IsNumeric(PropertyValue)=1 AND LEN(PropertyValue)<19 THEN CONVERT(numeric(18,4), PropertyValue) ELSE NULL END)";
									_custom_property += _property.Value;
									_custom_property += ")";
									break;
								case PropertyType.Text:
									_custom_property += "(";
									_custom_property += "AssetTypePropertyID=";
									_custom_property += _property.ID.ToString();
									_custom_property += " AND ";
									_custom_property += "PropertyValue";
									_custom_property += " Like '";
									string _string_property_value = Security.SQLInjectionBlock(_property.Value.Replace("'", "''"));
									_custom_property += _string_property_value;
									_custom_property += "%";
									_custom_property += "'";
									_custom_property += ")";
									break;
								case PropertyType.Enumeration:
									_custom_property += "(";
									_custom_property += "AssetTypePropertyID=";
									_custom_property += _property.ID.ToString();
									_custom_property += " AND ";
									_custom_property += "PropertyValue";
									_custom_property += "='";
									string _enum_property_value = Security.SQLInjectionBlock(_property.Value.Replace("'", "''"));
									_custom_property += _enum_property_value;
									_custom_property += "'";
									_custom_property += ")";
									break;
							}
						}
					}
				}

				if (_new_counter > 0)
				{
					_custom_property += ")";
					custom_count = _new_counter;
				}
			}

			return _custom_property;
		}


		private static string GetWildcardSearch(string dbColumn, string pattern, bool useLike)
		{
			bool searchForNulls = pattern.Contains("__NULL__");
			if (searchForNulls) pattern = pattern.Substring(8, pattern.Length - 8);
			pattern = Security.SQLInjectionBlock(pattern.Replace("'", "''"));

			string Result = string.Empty;
			if (searchForNulls) Result = "( " + dbColumn + " IS NULL OR " + dbColumn + " = '' OR ";

			if (pattern.Contains("*") || useLike)
			{
				pattern = pattern.Replace("%", "[%]").Replace("_", "[_]").Replace("[", "'[[]'").Replace("*", "%");
				Result += dbColumn + " LIKE '" + pattern + "'";
			}
			else
			{
				Result += dbColumn + " = '" + pattern + "'";
			}

			if (searchForNulls) Result += " )";

			return Result;
		}
		public static string GetAssetsByFilterSQLQuery(int departmentId, ref Filter filter, ref ColumnsSetting colsetting, bool isRealDates, bool isForExport, string customSort, string customGroupSort, int? UserId, Config cfg, bool? Active)
		{
			return GetAssetsByFilterSQLQuery(Guid.Empty, departmentId, ref filter, ref colsetting, isRealDates, isForExport, customSort, customGroupSort, UserId, cfg, Active)
			;
		}

		public static string GetAssetsByFilterSQLQuery(Guid organizationId, int departmentId, ref Filter filter, ref ColumnsSetting colsetting, bool isRealDates, bool isForExport, string customSort, string customGroupSort, int? UserId, Config cfg, bool? Active)
		{
			string sqlQuery = "";

			string _sql = string.Empty;
			string _static_order = string.Empty;
			string _having = string.Empty;
			string _groupby = string.Empty;
			int _custom_property_count = 0;
			string _select_list = string.Empty;

			AssetsConfig assetsConfig;
			if (filter != null && !string.IsNullOrEmpty(filter.SerialNumber))
				assetsConfig = Companies.SelectAssetUniqueCaptions(organizationId, departmentId);
			else
				assetsConfig = new AssetsConfig();

			bool accountManager = true;
			if (cfg != null && !cfg.AccountManager)
			{
				accountManager = false;
			}

			string default_sort = String.Empty;
			if (!isForExport)
			{
				if (filter == null) _sql += "SELECT TOP 0 ";
				else _sql += "SELECT ";

				_select_list = "Assets.Id, ";

				_select_list += " Assets.CategoryId as AssetCategoryId, AssetCategories.Name as AssetCategoryName, ";
				_select_list += " Assets.TypeId as AssetTypeId, AssetTypes.Name as AssetTypeName, ";
				_select_list += " Assets.MakeId as AssetMakeId, AssetMakes.Make as AssetMakeName, ";
				_select_list += " Assets.ModelId as AssetModelId, AssetModels.Model as AssetModelName, ";
				_select_list += " Assets.AccountId as AccountId, ";
				if (accountManager)
				{
					_select_list += " ISNULL(Accounts.vchName,'(Internal)') AccountName, ";
				}
				_select_list += " Assets.AssetGUID, Assets.SerialNumber, Assets.Unique1, Assets.Unique2, Assets.Unique3, Assets.Unique4, Assets.Unique5, Assets.Unique6, Assets.Unique7, CASE WHEN LEN(Assets.SerialNumber)>0 THEN Assets.SerialNumber END AS AssetIdentifyCode, ";

				if (colsetting != null)
				{
					string _sql_custom = string.Empty;

					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column1, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column2, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column3, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column4, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column5, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column6, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column7, true, isRealDates);
					_sql_custom += GetColumnName(departmentId, ColumnTypeInfo.Table, colsetting.Column8, true, isRealDates);

					_select_list += _sql_custom;
				}

                _select_list += " dbo.fxGetUserName(lo_checkout.FirstName, lo_checkout.LastName, lo_checkout.Email) as CheckoutName, AssetStatus.vchStatus as StatusName, Assets.dtUpdated AS UpdatedDate, dbo.fxGetUserName2(lo_updated.FirstName, lo_updated.LastName, lo_updated.Email) AS UpdatedByName";

				_static_order = " ORDER BY ";

				if (String.IsNullOrEmpty(customGroupSort))
				{
					bool FirstOrder = true;
					string[] DefGroupSort = new string[] { "AssetCategoryName", "AssetTypeName", "AssetMakeName", "AssetModelName" };
					foreach (string s in DefGroupSort)
					{
						if (!customSort.Contains(s))
						{
							default_sort += (FirstOrder ? "" : ",") + s;
							FirstOrder = false;
						}
					}
				}
			}
			else
			{
				_sql += "SELECT ";
				_select_list += " 0 as GroupId, 0 as AssetId, (CASE WHEN EXISTS(select TOP 1 * from AssetTypeProperties where AssetTypeProperties.DId=Assets.DepartmentId and AssetTypeProperties.AssetTypeId=Assets.TypeId) THEN 1 ELSE 0 END) AS IsHaveCustomPropertiesId, Assets.DepartmentId, Assets.Id, ";
				_select_list += " Assets.CategoryId as AssetCategoryId, AssetCategories.Name as AssetCategoryName, ";
				_select_list += " Assets.TypeId as AssetTypeId, AssetTypes.Name as AssetTypeName, ";
				_select_list += " Assets.MakeId as AssetMakeId, AssetMakes.Make as AssetMakeName, ";
				_select_list += " Assets.ModelId as AssetModelId, AssetModels.Model as AssetModelName, ";
				_select_list += " Assets.AssetGUID, Assets.SerialNumber, Assets.Unique1, Assets.Unique2, Assets.Unique3, Assets.Unique4, Assets.Unique5, Assets.Unique6, Assets.Unique7, Assets.StatusID as StatusId, AssetStatus.vchStatus as StatusName, ";
				_select_list += " Assets.Name as AssetName, Assets.Description as AssetDescription";
				_select_list += ", Assets.AccountId as AccountId";
				if (accountManager)
				{
					_select_list += ", ISNULL(Accounts.vchName,'(Internal)') AccountName";
				}
				_select_list += ", Assets.LocationId as LocationId, dbo.fxGetUserLocationName(" + departmentId.ToString() + ", Assets.LocationId) as LocationName";
                _select_list += ", Assets.OwnerId as OwnerId, lo_owner.FirstName as OwnerFirstName, lo_owner.LastName as OwnerLastName, lo_owner.EMail as OwnerMail, Assets.CheckedOutId as CheckedOutId, lo_checkout.FirstName as CheckedOutFirstName, lo_checkout.LastName as CheckedOutLastName, lo_checkout.EMail as CheckoutMail, Assets.VendorID as VendorId, Vendors.Name as VendorName, Assets.WarrantyVendor as WarrantyVendorId, WarrantyVendors.Name as WarrantyVendorName, Assets.PONumber, Assets.DateAquired, Assets.DateDeployed, Assets.DateDisposed, Assets.DateEntered, Assets.DateOutOfService, Assets.DatePurchased, Assets.DateReceived, Assets.LaborWarrantyLength, Assets.PartsWarrantyLength, Assets.Value, Assets.ValueCurrent, Assets.ValueDepreciated, Assets.ValueReplacement, Assets.ValueSalvage, Assets.DisposalCost, Assets.AssetSort, Assets.FundingCode, Assets.FundingSource, CONVERT(nvarchar(4000), Assets.Notes) as Notes, Assets.dtUpdated AS UpdatedDate, dbo.fxGetUserName2(lo_updated.FirstName, lo_updated.LastName, lo_updated.Email) AS UpdatedByName ";

				_static_order = " ORDER BY IsHaveCustomPropertiesId asc, AssetCategories.Name asc, AssetTypes.Name asc, AssetMakes.Make asc, AssetModels.Model asc ";
			};

			if (colsetting != null)
			{
				string _dynamic_order = "";

				string _sort_1 = GetColumnName(departmentId, ColumnTypeInfo.Field, colsetting.SortColumn1, false, isRealDates);
				string _sort_2 = GetColumnName(departmentId, ColumnTypeInfo.Field, colsetting.SortColumn2, false, isRealDates);
				string _sort_3 = GetColumnName(departmentId, ColumnTypeInfo.Field, colsetting.SortColumn3, false, isRealDates);

				if (_sort_1.Length > 0 && (String.IsNullOrEmpty(customGroupSort) || _sort_1.ToLower() != customGroupSort.ToLower()))
				{
					if (colsetting.SortColumnDesc1)
						_dynamic_order = _dynamic_order + _sort_1 + " DESC";
					else
						_dynamic_order = _dynamic_order + _sort_1 + " ASC";
				}

				if (_sort_2.Length > 0 && (String.IsNullOrEmpty(customGroupSort) || _sort_2.ToLower() != customGroupSort.ToLower()))
				{
					if (_dynamic_order.Length > 0)
						_dynamic_order += ",";

					if (colsetting.SortColumnDesc2)
						_dynamic_order = _dynamic_order + _sort_2 + " DESC";
					else
						_dynamic_order = _dynamic_order + _sort_2 + " ASC";
				}

				if (_sort_3.Length > 0 && (String.IsNullOrEmpty(customGroupSort) || _sort_3.ToLower() != customGroupSort.ToLower()))
				{
					if (_dynamic_order.Length > 0)
						_dynamic_order += ",";

					if (colsetting.SortColumnDesc3)
						_dynamic_order = _dynamic_order + _sort_3 + " DESC";
					else
						_dynamic_order = _dynamic_order + _sort_3 + " ASC";
				}
				
				
				if (!isForExport)
				{   
					if (!String.IsNullOrEmpty(customGroupSort))
						_static_order += customGroupSort;

					if (!String.IsNullOrEmpty(customSort) && (String.IsNullOrEmpty(customGroupSort) || customGroupSort.ToLower() != customSort.ToLower()))
						_static_order +=  (!String.IsNullOrEmpty(customGroupSort) ? ", " : String.Empty) + customSort;
					else if (!String.IsNullOrEmpty(_dynamic_order))
						_static_order += (!String.IsNullOrEmpty(customGroupSort) ? ", " : String.Empty) + _dynamic_order;                        
					else if (!String.IsNullOrEmpty(default_sort))
						_static_order += (!String.IsNullOrEmpty(customGroupSort) ? ", " : String.Empty) + default_sort;
				}
			}

			_sql += _select_list;

			_sql += " FROM Assets ";
			_sql = _sql + " INNER JOIN AssetCategories ON AssetCategories.DepartmentId=" + departmentId.ToString() + " and Assets.CategoryId = AssetCategories.Id ";
			_sql = _sql + " INNER JOIN AssetTypes ON AssetTypes.DepartmentId=" + departmentId.ToString() + " and Assets.TypeId = AssetTypes.Id ";
			_sql = _sql + " INNER JOIN AssetMakes ON AssetMakes.DepartmentId=" + departmentId.ToString() + " and Assets.MakeId = AssetMakes.Id ";
			_sql = _sql + " INNER JOIN AssetModels ON AssetModels.DepartmentId=" + departmentId.ToString() + " and Assets.ModelId = AssetModels.Id ";
			_sql = _sql + " INNER JOIN AssetStatus ON (AssetStatus.DId is NULL OR AssetStatus.DId=" + departmentId.ToString() + ") and Assets.StatusId = AssetStatus.Id ";//now statuses use for all departments
			_sql = _sql + " LEFT OUTER JOIN Accounts ON Accounts.DId=" + departmentId.ToString() + " and Assets.AccountId = Accounts.Id ";
			_sql = _sql + " LEFT OUTER JOIN Locations ON Locations.DId=" + departmentId.ToString() + " and Assets.LocationId = Locations.Id ";
			_sql = _sql + " LEFT OUTER JOIN tbl_vendors Vendors ON Vendors.company_id=" + departmentId.ToString() + " and Assets.VendorId = Vendors.Id ";
			_sql = _sql + " LEFT OUTER JOIN tbl_vendors WarrantyVendors ON WarrantyVendors.company_id=" + departmentId.ToString() + " and Assets.WarrantyVendor = WarrantyVendors.Id ";
			_sql = _sql + " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj_owner ON tlj_owner.company_id=" + departmentId.ToString() + " and Assets.OwnerId=tlj_owner.id LEFT OUTER JOIN tbl_Logins lo_owner ON lo_owner.id=tlj_owner.login_id ";
			_sql = _sql + " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj_checkout ON tlj_checkout.company_id=" + departmentId.ToString() + " and Assets.CheckedOutId=tlj_checkout.id LEFT OUTER JOIN tbl_Logins lo_checkout ON lo_checkout.id=tlj_checkout.login_id ";
			_sql = _sql + " LEFT OUTER JOIN tbl_LoginCompanyJunc tlj_updated ON tlj_updated.company_id=" + departmentId.ToString() + " and Assets.intUpdatedBy=tlj_updated.id LEFT OUTER JOIN tbl_Logins lo_updated ON lo_updated.id=tlj_updated.login_id ";
			_sql = _sql + " LEFT OUTER JOIN AssetStatusCompany ON AssetStatusCompany.DId=" + departmentId.ToString() + " and AssetStatusCompany.AssetStatusID=Assets.StatusId ";


			if (filter == null)
			{                
				string _DefWhere = " WHERE Assets.StatusId<>17 AND Assets.DepartmentId=" + departmentId.ToString();

				if (Active == true) _DefWhere += " AND (AssetStatusCompany.NonActive=0 OR (AssetStatusCompany.NonActive IS NULL)) ";
				else if (Active == false) _DefWhere += " AND AssetStatusCompany.NonActive=1 ";

				return _sql + _DefWhere + _static_order;
			}

			string DeptCriteria = "Assets.DepartmentId = " + departmentId.ToString();
			string AssetStatusCriteria = String.Empty;
			if (filter.StatusID > 0)
				AssetStatusCriteria = " AND Assets.StatusID=" + filter.StatusID.ToString();
			else if (filter.StatusID == 0)
			{
				AssetStatusCriteria = " AND Assets.StatusId<>17";
				if (Active == true)
					AssetStatusCriteria += " AND (AssetStatusCompany.NonActive=0 OR (AssetStatusCompany.NonActive IS NULL)) ";
				else if (Active == false)
					AssetStatusCriteria += " AND AssetStatusCompany.NonActive=1 ";
			}
			else if (filter.StatusID == -1)
				AssetStatusCriteria = " AND Assets.StatusId<>17"; // All except deleted
			else if (filter.StatusID == -2)
				AssetStatusCriteria = String.Empty; // All + deleted
			else if (filter.StatusID == -3)
				AssetStatusCriteria = " AND Assets.StatusId<>17 AND (AssetStatusCompany.NonActive=0 OR (AssetStatusCompany.NonActive IS NULL))"; // All Active statuses
			else if (filter.StatusID == -4)
				AssetStatusCriteria = " AND Assets.StatusId<>17 AND AssetStatusCompany.NonActive=1 ";

			string _where = " WHERE " + DeptCriteria + AssetStatusCriteria;

			if (filter.CategoryID != 0) _where += " AND Assets.CategoryID=" + filter.CategoryID.ToString();
			if (filter.TypeID != 0) _where += " AND Assets.TypeID=" + filter.TypeID.ToString();
			if (filter.MakeID != 0) _where += " AND Assets.MakeID=" + filter.MakeID.ToString();
			if (filter.ModelID != 0) _where += " AND Assets.ModelID=" + filter.ModelID.ToString();


			//Check Global Filters
			if (UserId != null && cfg != null)
			{
				GlobalFilters _gf = new GlobalFilters(organizationId, departmentId, (int)UserId);
				string _gfAccWhere = string.Empty;
				if (cfg.AccountManager && _gf.IsFilterEnabled(GlobalFilters.FilterState.EnabledGlobalFilters) && _gf.IsFilterEnabled(GlobalFilters.FilterType.Accounts))
				{
					string _strAcc = GlobalFilters.SelectFilterByTypeToString(organizationId, departmentId, (int)UserId, GlobalFilters.FilterType.Accounts);
					if (_strAcc.Length > 0)
						_gfAccWhere = " ISNULL(Assets.AccountId, -1) IN (" + _strAcc + ")";
				}
				string _gfLocWhere = string.Empty;
				if (cfg.LocationTracking && _gf.IsFilterEnabled(GlobalFilters.FilterState.EnabledGlobalFilters) && _gf.IsFilterEnabled(GlobalFilters.FilterType.Locations))
				{
					string _strLoc = GlobalFilters.SelectFilterByTypeToString(organizationId, departmentId, (int)UserId, GlobalFilters.FilterType.Locations);
					if (_strLoc.Length > 0)
						_gfLocWhere = " Assets.LocationID IN (SELECT Id FROM dbo.fxGetAllChildLocationsFromList(" + departmentId.ToString() + ", '" + _strLoc + "'))";
					else
						_gfLocWhere = " Assets.LocationID IS NULL";
				}

				if (!String.IsNullOrEmpty(_gfAccWhere))
					 _where += " AND " + _gfAccWhere;
				if (!String.IsNullOrEmpty(_gfLocWhere))
					 _where += " AND " + _gfLocWhere;
			}


			if (filter.AccountID != 0)
			{
				if ((filter.AccountID == -1) || (filter.AccountID == int.MinValue))
					_where += " AND Assets.AccountId IS NULL";
				else
					_where += " AND Assets.AccountId = " + filter.AccountID.ToString();
			}

			if (!string.IsNullOrEmpty(filter.CustomWhere)) _where += " AND (" + filter.CustomWhere + ")";

			int CurLocationID = filter.LocationID;
			if (CurLocationID > 0) _where += " AND Assets.LocationID IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + departmentId.ToString() + ", " + CurLocationID.ToString() + "))";
			else if (CurLocationID == int.MinValue) _where += " AND Assets.LocationID IS NULL";
			else if (CurLocationID < 0)
			{
				CurLocationID = (-1) * CurLocationID;
				_where += " AND ((Assets.LocationID IN (SELECT Id FROM dbo.fxGetAllChildLocations(" + departmentId.ToString() + ", " + CurLocationID.ToString() + "))) OR Assets.LocationID IS NULL)";
			}
			if (filter.OwnerID > 0) _where += " AND Assets.OwnerID=" + filter.OwnerID.ToString();
			else if (filter.OwnerID == int.MinValue) _where += " AND Assets.OwnerID IS NULL";
			if (filter.CheckedOutID > 0) _where += " AND Assets.CheckedOutID=" + filter.CheckedOutID.ToString();
			else if (filter.CheckedOutID == int.MinValue) _where += " AND Assets.CheckedOutID IS NULL";
			if (filter.VendorID > 0) _where += " AND Assets.VendorID=" + filter.VendorID.ToString();
			else if ((filter.VendorID == int.MinValue) || (filter.VendorID == -1)) _where += " AND Assets.VendorID IS NULL";
			if (filter.WarrantyVendorID > 0) _where += " AND Assets.WarrantyVendor=" + filter.WarrantyVendorID.ToString();
			else if ((filter.WarrantyVendorID == int.MinValue) || (filter.WarrantyVendorID == -1)) _where += " AND Assets.WarrantyVendor IS NULL";
			// #1 Serial Number
			string CurValue = filter.SerialNumber;
			if (CurValue.Length > 0)
			{
				_where += " AND ( " + GetWildcardSearch("Assets.SerialNumber", CurValue, false);

				if (!string.IsNullOrEmpty(assetsConfig.Unique1Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique1", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique2Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique2", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique3Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique3", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique4Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique4", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique5Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique5", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique6Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique6", CurValue, false);
				if (!string.IsNullOrEmpty(assetsConfig.Unique7Caption))
					_where += " OR " + GetWildcardSearch("Assets.Unique7", CurValue, false);

				_where += " )";
			}

			// #3 Asset Name
			CurValue = filter.AssetName;
			if (CurValue.Length > 0)
			{
				_where += " AND " + GetWildcardSearch("Assets.Name", CurValue, true);
			}
			// #4 PO Number
			CurValue = filter.PONumber;
			if (CurValue.Length > 0)
			{
				_where += " AND " + GetWildcardSearch("Assets.PONumber", CurValue, true);
			}
			// #5 Funding Code
			CurValue = filter.FundingCode;
			if (CurValue.Length > 0)
			{
				_where += " AND " + GetWildcardSearch("Assets.FundingCode", CurValue, true);
			}

			if (filter.AssetDateStart != DateTime.MinValue && filter.AssetDateEnd != DateTime.MinValue)
			{
				if (filter.CheckDateAquired) _where += " AND (Assets.DateAquired BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateDeployed) _where += " AND (Assets.DateDeployed BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateDisposed) _where += " AND (Assets.DateDisposed BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateEntered) _where += " AND (Assets.DateEntered BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateOutOfService) _where += " AND (Assets.DateOutOfService BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDatePurchased) _where += " AND (Assets.DatePurchased BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateReceived) _where += " AND (Assets.DateReceived BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
				if (filter.CheckDateUpdated) _where += " AND (Assets.dtUpdated BETWEEN '" + Functions.FormatSQLDateTime(filter.AssetDateStart) + "' AND '" + Functions.FormatSQLDateTime(filter.AssetDateEnd) + "') ";
			}
			//#1
			CurValue = filter.ValueCurrentExpression;
			if (CurValue.Length > 0)
			{
				if (CurValue.Contains("NULL"))
				{
					if (CurValue == "NULL") _where += " AND Assets.ValueCurrent IS NULL";
					else
					{
						CurValue = CurValue.Substring(4, CurValue.Length - 4);
						_where += " AND (Assets.ValueCurrent IS NULL OR Assets.ValueCurrent" + Security.SQLInjectionBlock(CurValue.Replace("'", "''")) + ")";
					}
				}
				else
				{
					_where += " AND Assets.ValueCurrent" + Security.SQLInjectionBlock(CurValue.Replace("'", "''"));
				}
			}
			//#2
			CurValue = filter.ValueDepreciatedExpression;
			if (CurValue.Length > 0)
			{
				if (CurValue.Contains("NULL"))
				{
					if (CurValue == "NULL") _where += " AND Assets.ValueDepreciated IS NULL";
					else
					{
						CurValue = CurValue.Substring(4, CurValue.Length - 4);
						_where += " AND (Assets.ValueDepreciated IS NULL OR Assets.ValueDepreciated" + Security.SQLInjectionBlock(CurValue.Replace("'", "''")) + ")";
					}
				}
				else
				{
					_where += " AND Assets.ValueDepreciated" + Security.SQLInjectionBlock(CurValue.Replace("'", "''"));
				}
			}
			//#3
			CurValue = filter.ValueReplacementExpression;
			if (CurValue.Length > 0)
			{
				if (CurValue.Contains("NULL"))
				{
					if (CurValue == "NULL") _where += " AND Assets.ValueReplacement IS NULL";
					else
					{
						CurValue = CurValue.Substring(4, CurValue.Length - 4);
						_where += " AND (Assets.ValueReplacement IS NULL OR Assets.ValueReplacement" + Security.SQLInjectionBlock(CurValue.Replace("'", "''")) + ")";
					}
				}
				else
				{
					_where += " AND Assets.ValueReplacement" + Security.SQLInjectionBlock(CurValue.Replace("'", "''"));
				}
			}
			//#4
			CurValue = filter.ValueSalvageExpression;
			if (CurValue.Length > 0)
			{
				if (CurValue.Contains("NULL"))
				{
					if (CurValue == "NULL") _where += " AND Assets.ValueSalvage IS NULL";
					else
					{
						CurValue = CurValue.Substring(4, CurValue.Length - 4);
						_where += " AND (Assets.ValueSalvage IS NULL OR Assets.ValueSalvage" + Security.SQLInjectionBlock(CurValue.Replace("'", "''")) + ")";
					}
				}
				else
				{
					_where += " AND Assets.ValueSalvage" + Security.SQLInjectionBlock(CurValue.Replace("'", "''"));
				}
			}
			//#5
			CurValue = filter.DisposalCostExpression;
			if (CurValue.Length > 0)
			{
				if (CurValue.Contains("NULL"))
				{
					if (CurValue == "NULL") _where += " AND Assets.DisposalCost IS NULL";
					else
					{
						CurValue = CurValue.Substring(4, CurValue.Length - 4);
						_where += " AND (Assets.DisposalCost IS NULL OR Assets.DisposalCost" + Security.SQLInjectionBlock(CurValue.Replace("'", "''")) + ")";
					}
				}
				else
				{
					_where += " AND Assets.DisposalCost" + Security.SQLInjectionBlock(CurValue.Replace("'", "''"));
				}
			}

			_having = GetAssetCustomPropertiesByFilterSQLQuery(ref filter, ref _custom_property_count);
			if (!string.IsNullOrEmpty(_having))
			{
				_having = "having ((Select count(*) from AssetPropertyValues apv1 where apv1.DId = " + departmentId.ToString() + " AND apv1.AssetId=Assets.Id " +
					_having + ")=" + _custom_property_count + ") ";
				_groupby = "Assets.DepartmentId,Assets.Id,Assets.OwnerId,Assets.CheckedOutId,Assets.TypeId,Assets.MakeId,Assets.ModelId," +
					"Assets.location_id,Assets.LocationId,Assets.VendorId,Assets.WarrantyVendor,Assets.Name,Assets.SerialNumber,Assets.Description,Assets.Value," +
					"Assets.DateAquired,Assets.LaborWarrantyLength,Assets.PartsWarrantyLength,CONVERT(nvarchar(max),Assets.Notes),Assets.Room,Assets.PONumber,Assets.Active,Assets.FundingCode," +
					"Assets.CategoryId,Assets.StatusId,Assets.AssetSort,Assets.DatePurchased,Assets.DateDeployed,Assets.DateOutOfService,Assets.DateEntered," +
					"Assets.DateReceived,Assets.DateDisposed,Assets.ValueCurrent,Assets.ValueReplacement,Assets.ValueDepreciated,Assets.ValueSalvage,Assets.DisposalCost," +
					"Assets.FundingSource,Assets.dtUpdated,Assets.intUpdatedBy,Assets.AccountId,Accounts.vchName,Assets.AssetNumber,Assets.AssetGUID," +
					"Assets.Unique1,Assets.Unique2,Assets.Unique3,Assets.Unique4,Assets.Unique5,Assets.Unique6,Assets.Unique7," +
					"lo_owner.LastName,lo_owner.FirstName,Vendors.Name,WarrantyVendors.NameAssetStatus.vchStatus,lo_checkout.LastName,lo_checkout.FirstName,lo_owner.Email,lo_checkout.Email,WarrantyVendors.name,AssetStatus.vchStatus," +
                    "AssetCategories.Name,AssetTypes.Name,AssetMakes.Make,AssetModels.Model,dbo.fxGetUserName2(lo_updated.FirstName, lo_updated.LastName, lo_updated.Email),AuditNote";
				string[] Groups = _groupby.Split(',');
				_groupby = string.Empty;
				string LowSql = _sql.ToLower();
				foreach (string Group in Groups) if (LowSql.Contains(Group.ToLower())) _groupby += "," + Group;
				_groupby = " GROUP BY " + _groupby.Substring(1) + " ";
			}

			if (colsetting != null)
				sqlQuery = _sql + _where + _groupby + _having + _static_order;

			return sqlQuery;
		}

		public static DataTable AssetGroupingByStatus(ref DataTable _table)
			{
			if (_table != null)
			{
				int i = 0;
				string _StatusName = "";
				string StatusName = "";

				//string CTMMColumnName = "CategoryTypeMakeModel";
				//if (!_table.Columns.Contains(CTMMColumnName)) _table.Columns.Add(CTMMColumnName, typeof(string));

				while (i < _table.Rows.Count)
				{
					DataRow _row = _table.Rows[i];
					if (_row != null)
				{
						string AssetCategoryName = _row["AssetCategoryName"].ToString();
						string AssetTypeName = _row["AssetTypeName"].ToString();
						string AssetMakeName = _row["AssetMakeName"].ToString();
						string AssetModelName = _row["AssetModelName"].ToString();                        

						StatusName = _row["StatusName"].ToString();

						if (StatusName != _StatusName)
				{
							_StatusName = StatusName;

							DataRow _new_row = _table.NewRow();
							_new_row["Id"] = "-1";
							_new_row["StatusName"] = StatusName;

							_table.Rows.InsertAt(_new_row, i);
						}
				}

					i++;
				}

				int categoryItemsCount = 0;
				_table.Columns.Add("ItemsCount", typeof(int));
				for (i = _table.Rows.Count - 1; i >= 0; i--)
					if (_table.Rows[i]["Id"].ToString() == "-1")
				{
						_table.Rows[i]["ItemsCount"] = categoryItemsCount;
						categoryItemsCount = 0;
					}
					else
						categoryItemsCount++;
			}

			return _table;
		}

		public static DataTable AssetGroupingByType(ref DataTable _table)
		{
			if (_table != null)
			{
				int i = 0;

				string _AssetCategoryId = ""; string _AssetCategoryName = "";
				string _AssetTypeId = ""; string _AssetTypeName = "";
				string _AssetMakeId = ""; string _AssetMakeName = "";
				string _AssetModelId = ""; string _AssetModelName = "";

				string AssetCategoryId = ""; string AssetCategoryName = "";
				string AssetTypeId = ""; string AssetTypeName = "";
				string AssetMakeId = ""; string AssetMakeName = "";
				string AssetModelId = ""; string AssetModelName = "";

				while (i < _table.Rows.Count)
				{
					DataRow _row = _table.Rows[i];
					if (_row != null)
					{
						AssetCategoryId = _row["AssetCategoryId"].ToString();
						AssetTypeId = _row["AssetTypeId"].ToString();
						AssetMakeId = _row["AssetMakeId"].ToString();
						AssetModelId = _row["AssetModelId"].ToString();

						AssetCategoryName = _row["AssetCategoryName"].ToString();
						AssetTypeName = _row["AssetTypeName"].ToString();
						AssetMakeName = _row["AssetMakeName"].ToString();
						AssetModelName = _row["AssetModelName"].ToString();

						if ((AssetCategoryId != _AssetCategoryId) || (AssetTypeId != _AssetTypeId) || (AssetMakeId != _AssetMakeId) || (AssetModelId != _AssetModelId))
						{
							_AssetCategoryId = AssetCategoryId;
							_AssetTypeId = AssetTypeId;
							_AssetMakeId = AssetMakeId;
							_AssetModelId = AssetModelId;

							_AssetCategoryName = AssetCategoryName;
							_AssetTypeName = AssetTypeName;
							_AssetMakeName = AssetMakeName;
							_AssetModelName = AssetModelName;

							DataRow _new_row = _table.NewRow();
							_new_row["Id"] = "-1";
							_new_row["AssetCategoryId"] = _AssetCategoryId;
							_new_row["AssetTypeId"] = _AssetTypeId;
							_new_row["AssetMakeId"] = _AssetMakeId;
							_new_row["AssetModelId"] = _AssetModelId;

							_new_row["AssetCategoryName"] = _AssetCategoryName;
							_new_row["AssetTypeName"] = _AssetTypeName;
							_new_row["AssetMakeName"] = _AssetMakeName;
							_new_row["AssetModelName"] = _AssetModelName;

							_table.Rows.InsertAt(_new_row, i);
						}
					}

					i++;
				}

				int categoryItemsCount = 0;
				_table.Columns.Add("ItemsCount", typeof (int));
				for (i = _table.Rows.Count - 1; i >= 0; i--)
					if (_table.Rows[i]["Id"].ToString() == "-1")
					{
						_table.Rows[i]["ItemsCount"] = categoryItemsCount;
						categoryItemsCount = 0;
					}
					else
						categoryItemsCount++;
			}

			return _table;
		}

		public static DataTable AssetGrouping(ref DataTable _table)
		{
			if (_table != null)
			{
				int i = 0;

				int _new_group_id = 0;

				int IsHaveCustomProperties = 0;
				int AssetTypeId = 0;
				int _AssetTypeId = 0;

				while (i < _table.Rows.Count)
				{
					DataRow _row = _table.Rows[i];
					if (_row != null)
					{
						IsHaveCustomProperties = Int32.Parse(_row["IsHaveCustomPropertiesId"].ToString());
						AssetTypeId = Int32.Parse(_row["AssetTypeId"].ToString());

						if ((AssetTypeId != _AssetTypeId) && (IsHaveCustomProperties != 0))
						{
							_new_group_id++;

							if (i == 0)
								_new_group_id = 0;

							_row["GroupId"] = _new_group_id;

							_AssetTypeId = AssetTypeId;
						}
						else
							_row["GroupId"] = _new_group_id;

					};

					i++;
				}
			};

			return _table;
		}

		public static DataTable SelectAssetsByFilter(int departmentId, Filter filter, ColumnsSetting colsetting, string customSort, out int assetsCount, string customGroupSort, int UserId, Config cfg, bool? Active)
		{
			string sqlQuery = GetAssetsByFilterSQLQuery(departmentId, ref filter, ref colsetting, false, false, customSort, customGroupSort, UserId, cfg, Active);

			DataTable _table = SelectByQuery(sqlQuery);
			assetsCount=_table.Rows.Count;
			_table = AssetGroupingByType(ref _table);

			return _table;
		}

		public static DataTable SelectNonActiveAssetsByFilter(int departmentId, Filter filter, ColumnsSetting colsetting, string customSort, out int assetsCount, string customGroupSort, int UserId, Config cfg, bool? Active)
		{
			string sqlQuery = GetAssetsByFilterSQLQuery(departmentId, ref filter, ref colsetting, false, false, customSort, customGroupSort, UserId, cfg, Active);


			DataTable _table = SelectByQuery(sqlQuery);
			assetsCount = _table.Rows.Count;
			_table = AssetGroupingByStatus(ref _table);

			return _table;
		}

		public static DataTable SelectAssetsByFilterForExport(Guid organizationId, int departmentId, Filter filter, ColumnsSetting colsetting)
		{
			return SelectAssetsByFilterForExport(organizationId, departmentId, filter, colsetting, null, null);
		}

		public static DataTable SelectAssetsByFilterForExport(Guid organizationId, int departmentId, Filter filter, ColumnsSetting colsetting, int? UserId, Config cfg)
		{
			string sqlQuery = GetAssetsByFilterSQLQuery(organizationId, departmentId, ref filter, ref colsetting, true, true, "", null, UserId, cfg, true);

			DataTable dtAssets = SelectByQuery(sqlQuery, organizationId);
			dtAssets = AssetGrouping(ref dtAssets);
			return dtAssets;
		}

		public static DataTable ImportAssetsApi(Guid organizationId, int departmentId, bool isInsertOnly, bool insertMissedValues, DataTable dtAssets, ref int insertedObjectsCount, ref int updatedObjectsCount)
		{
			return ImportAssetsApi(organizationId, departmentId, null, "Assets API", isInsertOnly, insertMissedValues, dtAssets, ref insertedObjectsCount, ref updatedObjectsCount);
		}

		public static DataTable ImportAssetsApi(Guid organizationId, int departmentId, int? userId, string userName, bool isInsertOnly, bool insertMissedValues, DataTable dtAssets, ref int insertedObjectsCount, ref int updatedObjectsCount)
		{
			ImportAssets importAssets = new ImportAssets(organizationId, departmentId);
			ImportCustomProperty importCustomProperty = new ImportCustomProperty();

			importAssets.Validate(ref importCustomProperty, dtAssets);

			if (insertMissedValues)
			{
				DataTable dtNewAssetTypes = importAssets.GetCommonNewAssetTypes(isInsertOnly);
				DataTable dtNewAssetVendors = importAssets.GetCommonNewAssetVendors(isInsertOnly);

				if ((dtNewAssetTypes != null && dtNewAssetTypes.Rows.Count > 0) || (dtNewAssetVendors != null && dtNewAssetVendors.Rows.Count > 0))
				{
					if (dtNewAssetTypes != null)
						foreach (DataRow drAssetType in dtNewAssetTypes.Rows)
							ImportAssets.AddType(organizationId, departmentId, drAssetType["AssetCategoryName"].ToString(), drAssetType["AssetTypeName"].ToString(), drAssetType["AssetMakeName"].ToString(), drAssetType["AssetModelName"].ToString());


					if (dtNewAssetVendors != null)
						foreach (DataRow drAssetVendor in dtNewAssetVendors.Rows)
							ImportAssets.AddVendor(organizationId, departmentId, drAssetVendor["VendorName"].ToString());

					importAssets.GetImportLog().Clear();
					importAssets.Validate(ref importCustomProperty, dtAssets);
				}
			}

			importAssets.GetCommonNewAssets(userId, userName, isInsertOnly, ref insertedObjectsCount, ref updatedObjectsCount);

			return importAssets.GetImportLog();
		}

		public static DataTable SelectParentAssets(int departmentId, int assetId)
		{
			string sqlQuery = "SELECT distinct AssetSubAssets.AssetId, AssetCategories.Name as AssetCategoryName, AssetTypes.Name as AssetTypeName, ";
			sqlQuery += "AssetMakes.Make as AssetMakeName, AssetModels.Model as AssetModelName, Assets.AssetNumber, Assets.SerialNumber, ";
			sqlQuery += "Assets.Unique1, Assets.Unique2, Assets.Unique3, Assets.Unique4, Assets.Unique5, Assets.Unique6, Assets.Unique7, ";
			sqlQuery += "Assets.Description ";
			sqlQuery += "FROM AssetSubAssets ";
			sqlQuery += "INNER JOIN Assets ON AssetSubAssets.DId=Assets.DepartmentId and AssetSubAssets.AssetId = Assets.Id ";
			sqlQuery += "INNER JOIN AssetCategories ON Assets.DepartmentId=AssetCategories.DepartmentId and Assets.CategoryId = AssetCategories.Id ";
			sqlQuery += "INNER JOIN AssetTypes ON Assets.DepartmentId=AssetTypes.DepartmentId and Assets.TypeId = AssetTypes.Id ";
			sqlQuery += "INNER JOIN AssetMakes ON Assets.DepartmentId=AssetMakes.DepartmentId and Assets.MakeId = AssetMakes.Id ";
			sqlQuery += "INNER JOIN AssetModels ON Assets.DepartmentId=AssetModels.DepartmentId and Assets.ModelId = AssetModels.Id ";
			sqlQuery += "INNER JOIN AssetStatus ON (AssetStatus.DId is NULL OR Assets.DepartmentId=AssetStatus.DId) and Assets.StatusId = AssetStatus.Id ";
			sqlQuery += "WHERE ";
			sqlQuery += "AssetSubAssets.DId=" + departmentId.ToString() + " AND ";
			sqlQuery += "AssetSubAssets.AssetChildId=" + assetId.ToString();

			return SelectByQuery(sqlQuery);
		}

		public static Assets.AssetItem[] SelectSubAssets(int departmentId, int assetId)
		{
			DataTable _dt = SelectRecords("sp_SelectSubAssets", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
			Data.Assets.AssetItem[] _arr = new Assets.AssetItem[_dt.Rows.Count];
			for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new Assets.AssetItem((int)_dt.Rows[i]["AssetChildId"], _dt.Rows[i]["Description"].ToString());
			return _arr;
		}

		public static int SelectCategoryId(Guid organizationId, int departmentId, string categoryName)
		{
			int result = -1;
			if (categoryName.Length > 0)
			{
				string _sql_query = "Select Id from AssetCategories where DepartmentId=" + departmentId.ToString();
				string _name = categoryName.Replace("'", "''");
				_sql_query = _sql_query + " AND LOWER(Name)='" + Security.SQLInjectionBlock(_name.ToLower()) + "'";

				DataTable _table = SelectByQuery(_sql_query, organizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectTypeId(Guid organizationId, int departmentId, string typeName, ref int categoryId)
		{
			int result = -1;
			if (!string.IsNullOrEmpty(typeName))
			{
				string sqlQuery = "Select Id, CategoryId from AssetTypes where DepartmentId=" + departmentId.ToString();
				sqlQuery = sqlQuery + " AND LOWER(Name)='" + Security.SQLInjectionBlock(typeName.Replace("'", "''").ToLower()) + "'";

				if (categoryId > 0)
					sqlQuery = sqlQuery + " AND CategoryId=" + categoryId.ToString();

				DataTable _table = SelectByQuery(sqlQuery, organizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
							categoryId = Int32.Parse(_table.Rows[0]["CategoryId"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectMakeId(Guid organizationId, int departmentId, string makeName, ref int typeId)
		{
			int result = -1;
			if (!string.IsNullOrEmpty(makeName))
			{
				string sqlQuery = "Select Id, TypeId from AssetMakes where DepartmentId=" + departmentId.ToString();
				sqlQuery = sqlQuery + " AND LOWER(Make)='" + Security.SQLInjectionBlock(makeName.Replace("'", "''").ToLower()) + "'";

				if (typeId > 0)
					sqlQuery = sqlQuery + " AND TypeId=" + typeId.ToString();

				DataTable _table = SelectByQuery(sqlQuery, organizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
							typeId = Int32.Parse(_table.Rows[0]["TypeId"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectModelId(Guid organizationId, int departmentId, string modelName, ref int makeId)
		{
			int result = -1;
			if (!string.IsNullOrEmpty(modelName))
			{
				string sqlQuery = "Select Id, MakeId from AssetModels where DepartmentId=" + departmentId.ToString();
				sqlQuery = sqlQuery + " AND LOWER(Model)='" + Security.SQLInjectionBlock(modelName.Replace("'", "''").ToLower()) + "'";

				if (makeId > 0)
					sqlQuery = sqlQuery + " AND MakeId=" + makeId.ToString();

				DataTable _table = SelectByQuery(sqlQuery, organizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
							makeId = Int32.Parse(_table.Rows[0]["MakeId"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectStatusId(Guid orgznizationId, int departmentId, string statusName)
		{
			int result = -1;
			if (statusName.Length > 0)
			{
				string sqlQuery = "SELECT Id FROM AssetStatus WHERE (DId is NULL OR DId=" + departmentId.ToString();
				sqlQuery = sqlQuery + ") AND LOWER(vchStatus)='" + Security.SQLInjectionBlock(statusName.Replace("'", "''").ToLower()) + "'";

				DataTable _table = SelectByQuery(sqlQuery, orgznizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int GetAssetLocationId(Guid organizationId, int departmentId, int accountId, string locationName)
		{
			int result = -1;
			if (locationName.Length > 0)
			{
				string sqlQuery = "SELECT dbo.fxGetLocationIdByFullName(" + departmentId.ToString() + ", " + (accountId > 0 ? accountId.ToString() : "NULL") + ", ";
				sqlQuery += "'" + Security.SQLInjectionBlock(locationName.Replace("'", "''")) + "') AS LocationId";

				DataTable dtLocations = SelectByQuery(sqlQuery, organizationId);
				if (dtLocations != null && dtLocations.Rows.Count == 1)
				{
					try
					{
						result = Int32.Parse(dtLocations.Rows[0]["LocationId"].ToString());
					}
					catch { }
				}
			}

			return result;
		}

		public static int SelectCustomPropertyId(int departmentId, int assetTypeId, string customPropertyName)
		{
			int result = -1;
			if (customPropertyName.Length > 0)
			{
				string sqlQuery = "Select Id from AssetTypeProperties where DId=" + departmentId.ToString();
				sqlQuery += " AND AssetTypeId=" + assetTypeId.ToString();
				sqlQuery += " AND LOWER(name)='" + Security.SQLInjectionBlock(customPropertyName.Replace("'", "''")) + "'";

				DataTable dtCustomProperties = SelectByQuery(sqlQuery);
				if (dtCustomProperties != null)
				{
					if (dtCustomProperties.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(dtCustomProperties.Rows[0]["Id"].ToString());
						}
						catch { }
					}
					else
					{
						if (dtCustomProperties.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectUserId(Guid organizationId, int departmentId, string email)
		{
			int result = -1;
			if (!string.IsNullOrEmpty(email))
			{
				string sqlQuery = "SELECT tlj.id, lo.Email, lo.firstname, lo.lastname,  lo.lastname +', '+lo.firstname as FullName FROM tbl_LoginCompanyJunc tlj JOIN tbl_Logins lo ON lo.id=tlj.login_id WHERE tlj.company_id=" + departmentId.ToString();
				sqlQuery += " AND tlj.UserType_Id<>4 AND tlj.btUserInactive=0";
				sqlQuery += " AND LOWER(lo.email)='" + Security.SQLInjectionBlock(email.Replace("'", "''").ToLower()) + "'";
				DataTable dtUsers = SelectByQuery(sqlQuery, organizationId);
				if (dtUsers != null)
				{
					if (dtUsers.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(dtUsers.Rows[0]["Id"].ToString());
						}
						catch { }
					}
					else
					{
						if (dtUsers.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int SelectVendorId(Guid organizationId, int departmentId, string vendorName)
		{
			int result = -1;
			if (!string.IsNullOrEmpty(vendorName))
			{
				string sqlQuery = "SELECT Id FROM tbl_vendors WHERE company_id=" + departmentId.ToString();
				sqlQuery = sqlQuery + " AND LOWER(name)='" + Security.SQLInjectionBlock(vendorName.Replace("'", "''").ToLower()) + "'";

				DataTable _table = SelectByQuery(sqlQuery, organizationId);
				if (_table != null)
				{
					if (_table.Rows.Count == 1)
					{
						try
						{
							result = Int32.Parse(_table.Rows[0]["Id"].ToString());
						}
						catch { }
					}
					else
					{
						if (_table.Rows.Count > 1)
							result = -2;
					}
				}
			}

			return result;
		}

		public static int GetAssetId(Guid organizationId, string sqlQuery, out bool updateUniqueFields)
		{
			int result = -1;
			updateUniqueFields = false;

			DataTable dtAssets = SelectByQuery(sqlQuery, organizationId);
			if (dtAssets != null)
			{
				if (dtAssets.Rows.Count == 1)
				{
					try
					{
						result = Int32.Parse(dtAssets.Rows[0]["Id"].ToString());

						if (dtAssets.Columns.Contains("UniqueFieldMatches") && !dtAssets.Rows[0].IsNull("UniqueFieldMatches") && dtAssets.Columns.Contains("NullUniqueFields") && !dtAssets.Rows[0].IsNull("NullUniqueFields"))
						{
							int uniqueFieldMatches = Convert.ToInt32(dtAssets.Rows[0]["UniqueFieldMatches"]);
							int nullUniqueFields = Convert.ToInt32(dtAssets.Rows[0]["NullUniqueFields"]);

							if (dtAssets.Columns.Contains("UpdateUniqueFields"))
							{
								if (uniqueFieldMatches > 1)
									dtAssets.Rows[0]["UpdateUniqueFields"] = true;
								else if (uniqueFieldMatches == 1)
									dtAssets.Rows[0]["UpdateUniqueFields"] = nullUniqueFields == 7;
								else if (uniqueFieldMatches == 0)
								{
									result = -1;
									dtAssets.Rows[0]["UpdateUniqueFields"] = true;
								}
							}

						}

						if (dtAssets.Columns.Contains("UpdateUniqueFields") && !dtAssets.Rows[0].IsNull("UpdateUniqueFields"))
							updateUniqueFields = Convert.ToBoolean(dtAssets.Rows[0]["UpdateUniqueFields"]);
					}
					catch { }
				}
				else if (dtAssets.Rows.Count > 1)
				{
					result = -2;
				}
			}

			return result;
		}

		public static int SelectAssetId(Guid organizationId, int departmentId, Guid? guid, string serial, string unique1, string unique2, string unique3, string unique4, string unique5, string unique6, string unique7, out bool updateUniqueFields)
		{
			int result = -1;
			bool selectByGuid = guid != null && guid != Guid.Empty;

			string sqlQuery = "SELECT TOP 1 Id, ";

			if (selectByGuid)
				sqlQuery += "1";
			else
				sqlQuery += "0";
			sqlQuery += " AS UpdateUniqueFields";

			if (!selectByGuid)
			{
				sqlQuery += ", (0";
				sqlQuery += CreateWhenConditionForSelectAssetId("SerialNumber", serial);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique1", unique1);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique2", unique2);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique3", unique3);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique4", unique4);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique5", unique5);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique6", unique6);
				sqlQuery += CreateWhenConditionForSelectAssetId("Unique7", unique7);
				sqlQuery += ") AS UniqueFieldMatches";

				sqlQuery += ", (";
				sqlQuery += "(CASE WHEN SerialNumber IS NULL THEN 1 ELSE 0 END)";
				for (int i = 1; i < 8; i++)
					sqlQuery += "+(CASE WHEN Unique" + i + " IS NULL THEN 1 ELSE 0 END)";
				sqlQuery += ") AS NullUniqueFields";
			}

			sqlQuery += " FROM Assets WHERE DepartmentId=" + departmentId;

			if (selectByGuid)
				sqlQuery += " AND AssetGUID = '" + guid + "'";
			else
			{
				sqlQuery += " AND (";
				sqlQuery += CreateWhereConditionForSelectAssetId("SerialNumber", Security.SQLInjectionBlock(serial.Replace("'", "''")), false);
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique1", Security.SQLInjectionBlock(unique1.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique2", Security.SQLInjectionBlock(unique2.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique3", Security.SQLInjectionBlock(unique3.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique4", Security.SQLInjectionBlock(unique4.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique5", Security.SQLInjectionBlock(unique5.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique6", Security.SQLInjectionBlock(unique6.Replace("'", "''")));
				sqlQuery += CreateWhereConditionForSelectAssetId("Unique7", Security.SQLInjectionBlock(unique7.Replace("'", "''")));
				sqlQuery += ") ORDER BY UniqueFieldMatches DESC";
			}

			result = GetAssetId(organizationId, sqlQuery, out updateUniqueFields);
			return result;
		}

		static string CreateWhenConditionForSelectAssetId(string identifierName, string identifierValue)
		{
			return !string.IsNullOrEmpty(identifierValue) ? "+(CASE WHEN " + identifierName + "='" + Security.SQLInjectionBlock(identifierValue.Replace("'", "''")) + "' THEN 1 ELSE 0 END)" : string.Empty;
		}

		static string CreateWhereConditionForSelectAssetId(string identifierName, string identifierValue)
		{
			return CreateWhereConditionForSelectAssetId(identifierName, identifierValue, true);
		}

		static string CreateWhereConditionForSelectAssetId(string identifierName, string identifierValue, bool prependOrClause)
		{
			return (prependOrClause ? " OR " : " ") + (string.IsNullOrEmpty(identifierValue) ? "ISNULL(" + identifierName + ",'') = ''" : identifierName + " = '" + identifierValue.ToLower() + "'");
		}

		public static string GetColumnName(int departmentId, ColumnTypeInfo columnInfo, BrowseColumn columnObject, bool addDelimiter, bool isRealDates)
		{
			string result = string.Empty;

			if (columnObject != Data.Assets.BrowseColumn.Blank)
			{
				switch (columnObject)
				{
					case BrowseColumn.AssetName:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.Name AS AssetName";
								break;
							case ColumnTypeInfo.Field:
								result += "AssetName";
								break;
							case ColumnTypeInfo.Header:
								result += "Asset Name";
								break;
						}
						break;
					case BrowseColumn.DateAquired:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateAquired";
								else
									result += "Convert(nvarchar(50),Assets.DateAquired,120) as DateAquired";
								break;
							case ColumnTypeInfo.Field:
								result += "DateAquired";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Acquired";
								break;
						}
						break;
					case BrowseColumn.DateDeployed:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateDeployed";
								else
									result += "Convert(nvarchar(50),Assets.DateDeployed,120) as DateDeployed";
								break;
							case ColumnTypeInfo.Field:
								result += "DateDeployed";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Deployed";
								break;
						}
						break;
					case BrowseColumn.DateUpdated:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.dtUpdated";
								else
									result += "Convert(nvarchar(50),Assets.dtUpdated,120) as dtUpdated";
								break;
							case ColumnTypeInfo.Field:
								result += "dtUpdated";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Updated";
								break;
						}
						break;
					case BrowseColumn.DateDisposed:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateDisposed";
								else
									result += "Convert(nvarchar(50),Assets.DateDisposed,120) as DateDisposed";
								break;
							case ColumnTypeInfo.Field:
								result += "DateDisposed";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Disposed";
								break;
						}
						break;
					case BrowseColumn.DateEntered:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateEntered";
								else
									result += "Convert(nvarchar(50),Assets.DateEntered,120) as DateEntered";
								break;
							case ColumnTypeInfo.Field:
								result += "DateEntered";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Entered";
								break;
						}
						break;
					case BrowseColumn.DateOutOfService:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateOutOfService";
								else
									result += "Convert(nvarchar(50),Assets.DateOutOfService,120) as DateOutOfService";
								break;
							case ColumnTypeInfo.Field:
								result += "DateOutOfService";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Out Of Service";
								break;
						}
						break;
					case BrowseColumn.DatePurchased:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DatePurchased";
								else
									result += "Convert(nvarchar(50),Assets.DatePurchased,120) as DatePurchased";
								break;
							case ColumnTypeInfo.Field:
								result += "DatePurchased";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Purchased";
								break;
						}
						break;
					case BrowseColumn.DateReceived:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								if (isRealDates)
									result += "Assets.DateReceived";
								else
									result += "Convert(nvarchar(50),Assets.DateReceived,120) as DateReceived";
								break;
							case ColumnTypeInfo.Field:
								result += "DateReceived";
								break;
							case ColumnTypeInfo.Header:
								result += "Date Received";
								break;
						}
						break;
					case BrowseColumn.DisposalCost:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.DisposalCost";
								break;
							case ColumnTypeInfo.Field:
								result += "DisposalCost";
								break;
							case ColumnTypeInfo.Header:
								result += "Disposal Cost";
								break;
						}
						break;
					case BrowseColumn.FundingCode:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.FundingCode";
								break;
							case ColumnTypeInfo.Field:
								result += "FundingCode";
								break;
							case ColumnTypeInfo.Header:
								result += "Funding Code";
								break;
						}
						break;
					case BrowseColumn.Location:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								//result += "Locations.Name as LocationName";
								result += "dbo.fxGetUserLocationName(" + departmentId + ", Assets.LocationId) as LocationName";
								break;
							case ColumnTypeInfo.Field:
								result += "LocationName";
								break;
							case ColumnTypeInfo.Header:
								result += "Location Name";
								break;
						}
						break;
					case BrowseColumn.Owner:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
                                result += "dbo.fxGetUserName(lo_owner.FirstName, lo_owner.LastName, lo_owner.Email) as OwnerName";
								break;
							case ColumnTypeInfo.Field:
								result += "OwnerName";
								break;
							case ColumnTypeInfo.Header:
								result += "Owner Name";
								break;
						}
						break;
					case BrowseColumn.PONumber:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.PONumber";
								break;
							case ColumnTypeInfo.Field:
								result += "PONumber";
								break;
							case ColumnTypeInfo.Header:
								result += "PO Number";
								break;
						}
						break;
					case BrowseColumn.ValueCurrent:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.ValueCurrent";
								break;
							case ColumnTypeInfo.Field:
								result += "ValueCurrent";
								break;
							case ColumnTypeInfo.Header:
								result += "Value Current";
								break;
						}
						break;
					case BrowseColumn.ValueDepreciated:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.ValueDepreciated";
								break;
							case ColumnTypeInfo.Field:
								result += "ValueDepreciated";
								break;
							case ColumnTypeInfo.Header:
								result += "Value Depreciated";
								break;
						}
						break;
					case BrowseColumn.ValueReplacement:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.ValueReplacement";
								break;
							case ColumnTypeInfo.Field:
								result += "ValueReplacement";
								break;
							case ColumnTypeInfo.Header:
								result += "Value Replacement";
								break;
						}
						break;
					case BrowseColumn.ValueSalvage:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.ValueSalvage";
								break;
							case ColumnTypeInfo.Field:
								result += "ValueSalvage";
								break;
							case ColumnTypeInfo.Header:
								result += "Value Salvage";
								break;
						}
						break;
					case BrowseColumn.Vendor:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Vendors.Name as VendorName";
								break;
							case ColumnTypeInfo.Field:
								result += "VendorName";
								break;
							case ColumnTypeInfo.Header:
								result += "Vendor Name";
								break;
						}
						break;
					case BrowseColumn.WarrantyVendor:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "WarrantyVendors.Name as WarrantyVendorName";
								break;
							case ColumnTypeInfo.Field:
								result += "WarrantyVendorName";
								break;
							case ColumnTypeInfo.Header:
								result += "Warranty Vendor";
								break;
						}
						break;

					case BrowseColumn.SerialNumber:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.SerialNumber as SerialNumber";
								break;
							case ColumnTypeInfo.Field:
								result += "SerialNumber";
								break;
							case ColumnTypeInfo.Header:
								result += "Serial Number";
								break;
						}
						break;

					case BrowseColumn.Status:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "AssetStatus.vchStatus as StatusName";
								break;
							case ColumnTypeInfo.Field:
								result += "StatusName";
								break;
							case ColumnTypeInfo.Header:
								result += "Status";
								break;
						}
						break;

					case BrowseColumn.CheckedOutTo:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "CheckoutName";
								break;
							case ColumnTypeInfo.Field:
								result += "CheckoutName";
								break;
							case ColumnTypeInfo.Header:
								result += "CheckOut To";
								break;
						}
						break;
					case BrowseColumn.AuditNote:
						switch (columnInfo)
						{
							case ColumnTypeInfo.Table:
								result += "Assets.AuditNote AS AuditNote";
								break;
							case ColumnTypeInfo.Field:
								result += "AuditNote";
								break;
							case ColumnTypeInfo.Header:
								result += "Audit Note";
								break;
						}
						break;
				}
			}

			if (addDelimiter)
			{
				if (result.Length > 0)
					result = result + ",";
			}

			return result;
		}

		public static DataTable SelectAssetsCheckedOutToUser(int departmentId, int userId)
		{
            return SelectAssetsCheckedOutToUser(Guid.Empty, departmentId, userId);
		}

        public static DataTable SelectAssetsCheckedOutToUser(Guid OrgId, int departmentId, int userId)
        {
            return SelectRecords("sp_SelectAssetsCheckedOut", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@UserId", userId) }, OrgId);
        }

		public static DataTable SelectOwners(int departmentId)
		{
			return SelectByQuery("SELECT ISNULL(A.OwnerId,-1) AS Id, ISNULL(L.LastName+', '+L.FirstName, '-Empty Owner-') AS Name FROM Assets A LEFT OUTER JOIN tbl_LoginCompanyJunc LCJ ON A.OwnerId=LCJ.id AND LCJ.company_id=" + departmentId + " INNER JOIN tbl_Logins L ON LCJ.login_id=L.id WHERE A.DepartmentId=" + departmentId + " GROUP BY A.OwnerId, ISNULL(L.LastName+', '+L.FirstName,'-Empty Owner-') ORDER BY Name");
		}

		public static DataTable SelectCheckedOutUsers(int departmentId)
		{
			return SelectByQuery("SELECT ISNULL(A.CheckedOutId,-1) AS Id, ISNULL(L.LastName+', '+L.FirstName,'-Empty CheckedOut-') AS Name FROM Assets A LEFT OUTER JOIN tbl_LoginCompanyJunc LCJ ON A.CheckedOutId=LCJ.id AND LCJ.company_id=" + departmentId.ToString() + " INNER JOIN tbl_Logins L ON LCJ.login_id=L.id WHERE A.DepartmentId=" + departmentId.ToString() + " GROUP BY A.CheckedOutId, ISNULL(L.LastName+', '+L.FirstName,'-Empty CheckedOut-') ORDER BY Name");
		}

		public static DataTable SelectVendors(int departmentId)
		{
			return SelectByQuery("SELECT A.VendorId AS Id, ISNULL(V.Name, '-Noname Vendor-')+ISNULL(', '+V.AccountNumber,'') AS Name FROM Assets A JOIN tbl_Vendors V ON V.company_id=" + departmentId + " AND A.VendorId=V.Id WHERE A.DepartmentId=" + departmentId + " GROUP BY A.VendorId, ISNULL(V.Name, '-Noname Vendor-')+ISNULL(', '+V.AccountNumber,'') ORDER BY Name");
		}

		public static DataTable SelectWarrantyVendors(int departmentId)
		{
			return SelectByQuery("SELECT A.WarrantyVendor AS Id, ISNULL(WV.Name, '-Noname Vendor-') + ISNULL(', ' + WV.AccountNumber, '') AS Name FROM Assets A JOIN tbl_Vendors WV ON WV.company_id=" + departmentId + " AND A.WarrantyVendor=WV.Id WHERE A.DepartmentId=" + departmentId + " GROUP BY A.WarrantyVendor, ISNULL(WV.Name, '-Noname Vendor-') + ISNULL(', ' + WV.AccountNumber, '') ORDER BY Name");
		}

		public static DataTable SelectLocations(int departmentId)
		{
			return SelectByQuery("SELECT L.Id AS LocationId, L.Name AS LocationName FROM Locations L WHERE L.DId=" + departmentId.ToString() + " AND L.AccountId IS NULL AND L.ParentId IS NULL ORDER BY LocationName");
		}

		public static DataTable SelectAssetByUniqueField(int departmentId, string searchString)
		{
			return SelectAssetByUniqueField(departmentId, searchString, false);
		}

		public static DataTable SelectAssetByUniqueField(int departmentId, string searchString, bool IncludeIdField)
		{
			SqlParameter searchId = null;
			if (IncludeIdField)
			{
				int Id = 0;
				if (int.TryParse(searchString, out Id))
					if (Id > 0)
						searchId = new SqlParameter("@IdValue", Id);
			}
			if (searchId == null)
				searchId = new SqlParameter("@IdValue", DBNull.Value);

			searchString = searchString.Trim();
			SqlParameter SearchPar = string.IsNullOrEmpty(searchString)?new SqlParameter("@Search", DBNull.Value):new SqlParameter("@Search", searchString);

			return SelectRecords("sp_SelectAssetByUniqueField", new[] { new SqlParameter("@DId", departmentId), SearchPar, searchId });
		}

        public static DataTable SelectAssetsSearchUnionCheckedOut(int departmentId, int userId, string uniqueField, int acctId = 0)
		{
            return SelectAssetsSearchUnionCheckedOut(Guid.Empty, departmentId, userId, uniqueField);
		}

        public static DataTable SelectAssetsSearchUnionCheckedOut(Guid OrgId, int departmentId, int userId, string uniqueField, int acctId = 0)
        {
            SqlParameter pUniqueField = new SqlParameter("@UniqueField", uniqueField);
            if (string.IsNullOrEmpty(uniqueField))
                pUniqueField.Value = DBNull.Value;

            return SelectRecords("sp_SelectAssetsSearchUnionCheckedOut", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@UserId", userId), pUniqueField, new SqlParameter("@AccountId", acctId) }, OrgId);
        }

		public class Statuses : DBAccess
		{
			public static DataTable SelectAll(int departmentId)
			{
				return SelectRecords("sp_SelectAssetStatusList", new[] { new SqlParameter("@DId", departmentId) });
			}

			public static DataRow SelectOne(int departmentId, int statusId)
			{
				return SelectRecord("sp_SelectAssetStatusList", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@Id", statusId) });
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

		public class AssetTypeProperty
		{
			private int m_ID = 0;
			private string m_Name = string.Empty;
			private PropertyType m_DataType = PropertyType.Text;
			private string m_Description = string.Empty;
			private string m_DefaultValue = string.Empty;
			private string m_Enumeration = string.Empty;
			private string[] m_EnumerationArray = new string[] { };
			private string m_Value = string.Empty;

			public AssetTypeProperty()
			{
			}

			public AssetTypeProperty(int id, string name, PropertyType datatype, string description, string defaultvalue, string enumeration, string propvalue)
			{
				m_ID = id;
				m_Name = name;
				m_DataType = datatype;
				m_Description = description;
				m_DefaultValue = defaultvalue;
				Enumeration = enumeration;
				m_Value = propvalue;
			}
			public int ID
			{
				set { m_ID = value; }
				get { return m_ID; }
			}
			public string Name
			{
				set { m_Name = value; }
				get { return m_Name; }
			}
			public PropertyType DataType
			{
				set { m_DataType = value; }
				get { return m_DataType; }
			}
			public string Description
			{
				set { m_Description = value; }
				get { return m_Description; }
			}
			public string DefaultValue
			{
				set { m_DefaultValue = value; }
				get { return m_DefaultValue; }
			}
			public string Enumeration
			{
				set
				{
					if (value.Length == 0) m_EnumerationArray = new string[] { };
					else if (m_Enumeration != value)
					{
						string _CrLf = new string(new char[] { (char)13, (char)10 });
						m_EnumerationArray = value.Split(new string[] { _CrLf }, StringSplitOptions.RemoveEmptyEntries);
					}
					m_Enumeration = value;
				}
				get { return m_Enumeration; }
			}
			public string[] EnumerationArray
			{
				get { return m_EnumerationArray; }
				set
				{
					if (value.Length > 0)
					{
						m_Enumeration = value[0];
						string _CrLf = new string(new char[] { (char)13, (char)10 });
						for (int i = 1; i < value.Length; i++) m_Enumeration += _CrLf + value[i];
					}
					else m_Enumeration = string.Empty;
					m_EnumerationArray = value;
				}
			}
			public string Value
			{
				set { m_Value = value; }
				get { return m_Value; }
			}
		}

		public class AssetTypeProperties : DBAccess
		{
			public static DataTable SelectAll(int departmentId, int assetTypeId)
			{
				return SelectAll(Guid.Empty, departmentId, assetTypeId);
			}

			public static DataTable SelectAll(Guid organizationId, int departmentId, int assetTypeId)
			{
				return SelectRecords("sp_SelectAssetTypeProperties", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@AssetTypeId", assetTypeId) }, organizationId);
			}

			public static AssetTypeProperty[] SelectAllToPropertiesArray(int departmentId, int assetTypeId)
			{
				DataTable _dt = SelectAll(departmentId, assetTypeId);
				if (_dt.Rows.Count == 0) return new AssetTypeProperty[] { };
				AssetTypeProperty[] _arr = new AssetTypeProperty[_dt.Rows.Count];
				for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new AssetTypeProperty((int)_dt.Rows[i]["Id"], _dt.Rows[i]["Name"].ToString(), (PropertyType)_dt.Rows[i]["DataType"], _dt.Rows[i]["Description"].ToString(), string.Empty, _dt.Rows[i]["Enumeration"].ToString(), string.Empty);
				return _arr;
			}

			public static DataRow SelectOne(int departmentId, int assetTypeId, int propertyId)
			{
				return SelectRecord("sp_SelectAssetTypeProperties", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@AssetTypeId", assetTypeId), new SqlParameter("@Id", propertyId) });
			}

			public static string GetValue(int departmentId, int assetId, int assetTypePropertyId)
			{
				SqlParameter pPropertyValue = new SqlParameter("@PropertyValue", SqlDbType.NVarChar, 255);
				pPropertyValue.Direction = ParameterDirection.Output;
				SqlCommand sqlCommand = CreateSqlCommand("sp_GetAssetPropertyValue", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@AssetId", assetId), new SqlParameter("@AssetTypePropertyId", assetTypePropertyId), pPropertyValue });
				if (sqlCommand.Connection.State == ConnectionState.Closed) sqlCommand.Connection.Open();
				sqlCommand.ExecuteNonQuery();
				sqlCommand.Connection.Close();
				return pPropertyValue.Value.ToString();
			}

			public static int SelectPropertyType(int departmentId, int assetTypePropertyId)
			{
				int result = -1;

				string _sql_query = "select DataType from AssetTypeProperties where DId=" + departmentId.ToString() + " and Id=" + assetTypePropertyId.ToString();

				DataTable _table = SelectByQuery(_sql_query);
				if (_table != null && _table.Rows.Count == 1)
				{
					try
					{
						result = Int32.Parse(_table.Rows[0]["DataType"].ToString());
					}
					catch { }
				}

				return result;
			}

			public static int SetValue(int departmentId, int assetId, int assetTypePropertyId, string propertyValue)
			{
				SqlParameter pId = new SqlParameter("@Id", SqlDbType.Int);
				pId.Direction = ParameterDirection.InputOutput;
				pId.Value = DBNull.Value;

				string _new_property_value = propertyValue;

				int _property_type = SelectPropertyType(departmentId, assetTypePropertyId);

				if (_property_type == (int)PropertyType.DateTime)//Date
				{
					if (_new_property_value.Length > 0)
					{
						try
						{
							DateTime _date_value = DateTime.Parse(_new_property_value);
							_new_property_value = Functions.FormatSQLShortDateTime(_date_value);
						}
						catch { }
					}
				}

				if (_property_type == (int)PropertyType.Numeric)//Numeric 
				{
					if (_new_property_value.Length > 0)
					{
						try
						{
							Decimal _decimal_value = Decimal.Parse(_new_property_value);
							_new_property_value = _decimal_value.ToString("F");
						}
						catch { }
					}
				}

				SqlParameter pPropertyValue = new SqlParameter("@PropertyValue", DBNull.Value);
				if (_new_property_value.Length > 0)
					pPropertyValue.Value = _new_property_value;

				UpdateData("sp_UpdateAssetPropertyValue", new[] { pId, new SqlParameter("@DId", departmentId), new SqlParameter("@AssetId", assetId), new SqlParameter("@AssetTypePropertyId", assetTypePropertyId), pPropertyValue });

				return (int)pId.Value;
			}

			public static bool IsNameExists(int departmentId, int assetTypeId, string name, int id)
			{
				SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
				pReturnValue.Direction = ParameterDirection.ReturnValue;
				SqlCommand _cmd = CreateSqlCommand("sp_SelectAssetTypePropertyNameExists", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@AssetTypeId", assetTypeId), new SqlParameter("@Name", name), new SqlParameter("@Id", id) });
				if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
				_cmd.ExecuteNonQuery();
				_cmd.Connection.Close();
				if ((int)pReturnValue.Value == 1) return true;
				else return false;
			}

			public static bool IsNameExists(int departmentId, int AssetTypeId, string name)
			{
				return IsNameExists( departmentId, AssetTypeId, name, 0);
			}

			public static int Update(int departmentId, int Id, int AssetTypeId, string Name, int DataType, string Enumeration, string Description)
			{
				SqlParameter _pId = new SqlParameter("@Id", Id);
				_pId.Direction = ParameterDirection.InputOutput;
				UpdateData("sp_UpdateAssetTypeProperty", new[]{_pId, 
																				  new SqlParameter("@DId", departmentId), 
																				  new SqlParameter("@AssetTypeId", AssetTypeId),
																				  new SqlParameter("@Name", Name), 
																				  new SqlParameter("@DataType", DataType), 
																				  new SqlParameter("@Enumeration", Enumeration), 
																				  new SqlParameter("@Description", Description)});
				return (int)_pId.Value;
			}

			public static void Delete(int departmentId, int id)
			{
				UpdateData("sp_DeleteAssetTypeProperty", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@Id", id) });
			}
		}

		public class ExcelProperty
		{
			private string _prefix;
			private string _name;
			private string _value;
			private bool _visible;

			public string Prefix
			{
				set { _prefix = value; }
				get { return _prefix; }
			}

			public string Name
			{
				set { _name = value; }
				get { return _name; }
			}

			public string Value
			{
				set { _value = value; }
				get { return _value; }
			}

			public bool Visible
			{
				set { _visible = value; }
				get { return _visible; }
			}

			public ExcelProperty(string property_prefix, string property_name, string property_value)
			{
				_prefix = property_prefix;
				_name = property_name;
				_value = property_value;
				_visible = true;
			}

			override public string ToString()
			{
				string result = " ";

				if (_visible)
					result = result + _prefix + ":" + _name + "=\"" + _value + "\"";
				else
					result = "";

				return result;
			}
		}

		public class ExcelPropertyCollection
		{
			private SortedList _list;

			public ExcelPropertyCollection()
			{
				_list = new SortedList();
			}

			public bool Clear()
			{
				bool result = false;

				if (_list != null)
				{
					_list.Clear();
					result = true;
				};

				return result;
			}

			public bool Add(string property_prefix, string property_name, string property_value)
			{
				bool result = false;

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (!_list.Contains(property_name))
					{
						try
						{
							ExcelProperty _new_property = new ExcelProperty(property_prefix, property_name, property_value);
							if (_new_property != null)
							{
								int _old_size = _list.Count;
								_list.Add(property_name, _new_property);
								int _new_size = _list.Count;

								if (_old_size != _new_size)
									result = true;
							};
						}
						catch
						{

						}
					};
				};

				return result;
			}

			public bool Add(string property_prefix, string property_name, string property_value, bool visible)
			{
				bool result = false;

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (!_list.Contains(property_name))
					{
						try
						{
							ExcelProperty _new_property = new ExcelProperty(property_prefix, property_name, property_value);
							if (_new_property != null)
							{
								_new_property.Visible = visible;
								int _old_size = _list.Count;
								_list.Add(property_name, _new_property);
								int _new_size = _list.Count;

								if (_old_size != _new_size)
									result = true;
							};
						}
						catch
						{

						}
					};
				};

				return result;
			}

			public bool Remove(string property_name)
			{
				bool result = false;

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (_list.Contains(property_name))
					{
						_list.Remove(property_name);
					};
				};

				return result;
			}

			public bool IsExist(string property_name)
			{
				bool result = false;

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (_list.Contains(property_name))
					{
						result = true;
					};
				};

				return result;
			}

			public bool SetPropertyValue(string property_name, string property_value)
			{
				bool result = false;

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (_list.Contains(property_name))
					{
						ExcelProperty _object = (ExcelProperty)_list[property_name];
						if (_object != null)
						{
							_object.Value = property_value;
							result = true;
						};
					};
				};

				return result;
			}

			public string GetPropertyValue(string property_name)
			{
				string result = "";

				if (property_name.Length == 0)
					return result;

				if (_list != null)
				{
					if (_list.Contains(property_name))
					{
						ExcelProperty _object = (ExcelProperty)_list[property_name];
						if (_object != null)
						{
							result = _object.Value;
						};
					};
				};

				return result;
			}

			override public string ToString()
			{
				string result = "";

				if (_list == null)
					return result;

				for (int i = 0; i < _list.Count; i++)
				{
					ExcelProperty _object = (ExcelProperty)_list[_list.GetKey(i).ToString()];
					if (_object != null)
						result += _object.ToString();
				};

				return result;
			}
		}

		public class ExcelObject
		{
			private string _name;
			private bool _is_xml;
			private bool _is_new_line;
			private ExcelPropertyCollection _property_collection;
			private ExcelObject Parent = null;
			private SortedList _objects;

			public string Name
			{
				set { _name = value; }
				get { return _name; }
			}

			public bool IsXML
			{
				set { _is_xml = value; }
				get { return _is_xml; }
			}

			public bool IsNewLine
			{
				set { _is_new_line = value; }
				get { return _is_new_line; }
			}

			public ExcelObject(string object_name)
			{
				_property_collection = new ExcelPropertyCollection();
				_objects = new SortedList();

				_name = object_name;
				_is_xml = true;
				_is_new_line = false;
			}

			public bool ClearObjects()
			{
				bool result = false;

				if (_objects != null)
				{
					_objects.Clear();
					result = true;
				};

				return result;
			}

			public bool AddObject(int key, ref ExcelObject new_object)
			{
				bool result = false;

				int new_key = key;

				if (_objects != null)
				{
					if (new_object != null)
					{
						if (new_key == 0)
							new_key = _objects.Count;

						if (!_objects.Contains(new_key))
						{
							_objects.Add(new_key, new_object);
							new_object.Parent = this;
							result = true;
						};
					};
				};

				return result;
			}

			public bool RemoveObject(int key)
			{
				bool result = false;

				if (_objects != null)
				{
					if (_objects.Contains(key))
					{
						_objects.Remove(key);
						result = true;
					};
				};

				return result;
			}

			public bool IsObjectExist(int key)
			{
				bool result = false;

				if (_objects != null)
				{
					if (_objects.Contains(key))
						result = true;
				};

				return result;
			}

			public ExcelObject GetRootObject(ref ExcelObject existed_object)
			{
				ExcelObject result = null;

				if (existed_object != null)
				{
					result = existed_object;
					ExcelObject current_object = existed_object;
					while (current_object.Parent != null)
					{
						result = current_object;
						current_object = existed_object.Parent;
					};
				};

				return result;
			}

			public bool AddProperty(string property_prefix, string property_name, string property_value)
			{
				bool result = false;

				if (_property_collection != null)
					result = _property_collection.Add(property_prefix, property_name, property_value);

				return result;
			}

			public bool AddProperty(string property_prefix, string property_name, string property_value, bool visible)
			{
				bool result = false;

				if (_property_collection != null)
					result = _property_collection.Add(property_prefix, property_name, property_value, visible);

				return result;
			}

			public bool IsPropertyExist(string property_name)
			{
				bool result = false;

				if (_property_collection != null)
					result = _property_collection.IsExist(property_name);

				return result;
			}

			public bool RemoveProperty(string property_name)
			{
				bool result = false;

				if (_property_collection != null)
					result = _property_collection.Remove(property_name);

				return result;
			}

			public bool SetPropertyValue(string property_name, string property_value)
			{
				bool result = false;

				if (_property_collection != null)
					result = _property_collection.SetPropertyValue(property_name, property_value);

				return result;
			}

			public string GetPropertyValue(string property_name)
			{
				string result = "";

				if (_property_collection != null)
					result = _property_collection.GetPropertyValue(property_name);

				return result;
			}

			override public string ToString()
			{
				string result = "";

				if (_is_xml)
				{
					result = result + "<" + _name + "";

					if (_property_collection != null)
					{
						result = result + _property_collection.ToString();
						result = result + ">";
					};

					if (_objects.Count > 0)
					{
						for (int index = 0; index < _objects.Count; index++)
						{
							result = result + ((ExcelObject)_objects[_objects.GetKey(index)]).ToString();
						};
					};

					result = result + "</" + _name + ">";

					if (_is_new_line)
						result = result + Environment.NewLine;
				}
				else
					result = _name;

				return result;
			}
		}

		public class ExcelColumns
		{
			SortedList _list;

			public ExcelColumns()
			{
				_list = new SortedList();
			}

			public bool Clear()
			{
				bool result = false;

				if (_list != null)
				{
					_list.Clear();
					result = true;
				};

				return result;
			}

			public bool AddColumn(int index, string column_name, string column_alias, int width, string type, string system_type, string style_id)
			{
				bool result = false;

				if (_list != null)
				{
					if (!_list.Contains(column_name))
					{
						ExcelObject _object = new ExcelObject("Column");
						if (_object != null)
						{
							_object.AddProperty("ss", "AutoFitWidth", "0");
							_object.AddProperty("ss", "Width", width.ToString());
							_object.AddProperty("", "Type", type, false);
							_object.AddProperty("", "Name", column_name, false);
							_object.AddProperty("", "Alias", column_alias, false);
							_object.AddProperty("", "SystemType", system_type, false);
							_object.AddProperty("", "StyleId", style_id, false);
							_object.AddProperty("", "Index", index.ToString(), false);
							_list.Add(column_name, _object);
							result = true;
						};
					};
				};

				return result;
			}

			public bool IsColumnExist(string column_name)
			{
				bool result = false;

				if (_list != null)
				{
					if (_list.Contains(column_name))
						result = true;
				};

				return result;
			}

			public bool RemoveColumn(string column_name)
			{
				bool result = false;

				if (_list != null)
				{
					if (_list.Contains(column_name))
					{
						_list.Remove(column_name);
						result = true;
					};
				};

				return result;
			}

			public int Count()
			{
				int result = 0;

				if (_list != null)
				{
					result = _list.Count;
				};

				return result;
			}

			public ExcelObject GetColumnObject(int index)
			{
				ExcelObject result = null;

				if ((index >= 0) && (index < Count()))
				{
					result = (ExcelObject)_list[_list.GetKey(index).ToString()];
				};

				return result;
			}

			public ExcelObject GetColumnObject(string column_name)
			{
				ExcelObject result = null;

				if (_list != null)
				{
					if (_list.Contains(column_name))
					{
						result = (ExcelObject)_list[column_name];
					};
				};

				return result;
			}

		}

		public class ExcelRow
		{
			private ExcelObject _row;
			private SortedList _list;
			private ExcelTable _table;

			public ExcelRow(int index, ExcelTable table)
			{
				_list = new SortedList();
				_row = new ExcelObject("Row");
				_table = table;

				if ((table != null) && (_row != null))
				{
					_row.IsNewLine = true;
					ExcelObject _table_object = _table.GetTableObject();
					if (_table_object != null)
						_table_object.AddObject(index, ref _row);
				};
			}

			public ExcelRow(ExcelTable table)
			{
				_list = new SortedList();
				_row = new ExcelObject("Row");
				_table = table;

				if (_row != null)
				{
					_row.IsNewLine = true;
				};
			}

			public static string GetExcelShortDateFormat(string user_date)
			{
				string result = "";

				if (user_date.Length > 0)
				{
					try
					{
						DateTime date = new DateTime();
						date = DateTime.Parse(user_date);
						//Universal Excel format: yyyy-mm-dd 
						result = date.Year.ToString() + "-";
						if (date.Month < 10)
							result = result + "0" + date.Month.ToString();
						else
							result = result + date.Month.ToString();

						result += "-";

						if (date.Day < 10)
							result = result + "0" + date.Day.ToString();
						else
							result = result + date.Day.ToString();
					}
					catch
					{
					};
				};

				return result;
			}

			public static string GetExcelDoubleFormat(string user_double)
			{
				string result = "";

				if (user_double.Length > 0)
				{
					try
					{
						Decimal _double = Decimal.Parse(user_double);
						Int32 _first = Decimal.ToInt32(_double);
						_double = (_double - _first) * 10000;
						Int32 _second = Decimal.ToInt32(_double);
						result = _first + "." + _second;
					}
					catch
					{
					};
				};

				return result;
			}

			public bool SetValue(string column_name, string value, bool is_white)
			{
				bool result = false;

				if (_list != null)
				{
					if (!_list.Contains(column_name))
					{
						ExcelObject _cell = new ExcelObject("Cell");
						if (_cell != null)
						{
							ExcelObject _data = new ExcelObject("Data");
							if (_data != null)
							{
								string column_type = "";
								string column_system_type = "";
								string style_id = "";
								int column_index = 0;
								ExcelColumns _columns = _table.GetColumns();
								if (_columns != null)
								{
									ExcelObject _column = _columns.GetColumnObject(column_name);
									if (_column != null)
									{
										column_type = _column.GetPropertyValue("Type");
										column_system_type = _column.GetPropertyValue("SystemType");
										style_id = _column.GetPropertyValue("StyleId");
										column_index = Int32.Parse(_column.GetPropertyValue("Index"));
										if (column_index < 0)
											column_index = -column_index;
										_column.IsNewLine = true;
									};
								};

								int style_int = 0;

								try
								{
									style_int = Int32.Parse(style_id);
									if (!is_white)
										style_int = style_int + 4;
								}
								catch
								{
								}

								_cell.AddProperty("ss", "StyleID", "s" + style_int.ToString());

								_row.AddObject(column_index, ref _cell);

								if (value.Length > 0)
								{
									_cell.AddObject(0, ref _data);
									_data.AddProperty("ss", "Type", column_type);

									if (column_system_type == "System.DateTime")
										value = GetExcelShortDateFormat(value);

									if (column_system_type == "System.Decimal")
										value = GetExcelDoubleFormat(value);

									ExcelObject _value = new ExcelObject(value);
									if (_value != null)
									{
										_data.AddObject(0, ref _value);
										_value.IsXML = false;

										_list.Add(column_name, _value);
										result = true;
									};
								};
							};
						};
					}
					else
					{
						if (value.Length > 0)
							((ExcelObject)_list[column_name]).Name = value;
					};
				};

				return result;
			}

			public bool SetHeaderValue(string column_name, string column_alias)
			{
				bool result = false;

				//string value = column_name;
				string value = column_alias;

				if (_list != null)
				{
					if (!_list.Contains(column_name))
					{
						ExcelObject _cell = new ExcelObject("Cell");
						if (_cell != null)
						{
							ExcelObject _data = new ExcelObject("Data");
							if (_data != null)
							{
								string column_type = "";
								string column_system_type = "";
								string style_id = "";
								int column_index = 0;
								ExcelColumns _columns = _table.GetColumns();
								if (_columns != null)
								{
									ExcelObject _column = _columns.GetColumnObject(column_name);
									if (_column != null)
									{
										column_type = _column.GetPropertyValue("Type");
										column_system_type = _column.GetPropertyValue("SystemType");
										style_id = _column.GetPropertyValue("StyleId");
										column_index = Int32.Parse(_column.GetPropertyValue("Index"));
										if (column_index < 0)
											column_index = -column_index;
										_column.IsNewLine = true;
									};
								};

								_cell.AddProperty("ss", "StyleID", "s21");
								_row.AddObject(column_index, ref _cell);

								if (value.Length > 0)
								{
									_cell.AddObject(0, ref _data);
									_data.AddProperty("ss", "Type", "String");

									ExcelObject _value = new ExcelObject(value);
									if (_value != null)
									{
										_data.AddObject(0, ref _value);
										_value.IsXML = false;

										_list.Add(column_name, _value);
										result = true;
									};
								};
							};
						};
					}
					else
						((ExcelObject)_list[column_name]).Name = value;
				};

				return result;
			}

			public string GetValue(string column_name)
			{
				string result = "";

				if (_list != null)
				{
					if (_list.Contains(column_name))
					{
						result = ((ExcelObject)_list[column_name]).Name.ToString();
					};
				};

				return result;
			}

			public bool IsExist(string column_name)
			{
				bool result = false;

				if (_list != null)
				{
					if (_list.Contains(column_name))
					{
						result = true;
					};
				};

				return result;
			}

			override public string ToString()
			{
				string result = "";

				if (_row != null)
				{
					result = _row.ToString();
				};

				return result;
			}
		}

		public class ExcelTable
		{
			private ExcelObject _table;
			private SortedList _rows;
			private ExcelColumns _columns;

			public ExcelTable(ref ExcelColumns columns)
			{
				_columns = columns;
				_table = new ExcelObject("Table");

				if (_table != null)
				{
					int _column_count = columns.Count();
					for (int index = 0; index < _column_count; index++)
					{
						ExcelObject _value = columns.GetColumnObject(index);
						_table.AddObject(-(index + 1), ref _value);
					};
				};

				_rows = new SortedList();
			}

			public ExcelColumns GetColumns()
			{
				return _columns;
			}

			public ExcelObject GetTableObject()
			{
				return _table;
			}

			public ExcelRow AddRow(int index)
			{
				ExcelRow result = null;
				if (_rows != null)
				{
					if (!_rows.Contains(index))
					{
						ExcelRow _new_row = new ExcelRow(index, this);
						if (_new_row != null)
						{
							_rows.Add(index, _new_row);
							result = _new_row;
						};
					};
				};

				return result;
			}

			public string ToHeader()
			{
				string result = "";

				if (_table != null)
				{
					result = _table.ToString();
				};

				if (result.Length > "</Table>".Length)
				{
					result = result.Remove(result.Length - "</Table>".Length - 1, "</Table>".Length);
				};

				return result;
			}

			public string ToFooter()
			{
				return "</Table>";
			}
		}

		public class ExcelDocument
		{
			private string _column_filter;
			private string _header;
			private ExcelObject _root;
			private System.Web.UI.HtmlTextWriter _writer = null;

			public string ColumnFilter //filter columns which contains filter value in any case
			{
				set { _column_filter = value; }
				get { return _column_filter; }
			}

			public void SetWriter(ref System.Web.UI.HtmlTextWriter writer)
			{
				_writer = writer;
			}

			public System.Web.UI.HtmlTextWriter GetWriter()
			{
				return _writer;
			}

			public ExcelDocument()
			{
				_column_filter = "";

				_header = "<?xml version=\"1.0\"?>";
				_header += "<?mso-application progid=\"Excel.Sheet\"?>";

				//_footer = "";

				_root = new ExcelObject("Workbook");
				if (_root != null)
				{
					_root.AddProperty("", "xmlns", "urn:schemas-microsoft-com:office:spreadsheet");
					_root.AddProperty("xmlns", "o", "urn:schemas-microsoft-com:office:office");
					_root.AddProperty("xmlns", "x", "urn:schemas-microsoft-com:office:excel");
					_root.AddProperty("xmlns", "ss", "urn:schemas-microsoft-com:office:spreadsheet");
					_root.AddProperty("xmlns", "html", "http://www.w3.org/TR/REC-html40");

					ExcelObject _styles = new ExcelObject("Styles");
					if (_styles != null)
					{
						ExcelObject _style_default = new ExcelObject("Style");
						if (_style_default != null)
						{
							_style_default.AddProperty("ss", "ID", "Default");
							_style_default.AddProperty("ss", "Name", "Normal");
							_styles.AddObject(0, ref _style_default);
						};

						ExcelObject _header_default = new ExcelObject("Style");
						if (_header_default != null)
						{
							ExcelObject _header_align = new ExcelObject("Alignment");
							if (_header_align != null)
							{
								_header_align.AddProperty("ss", "Vertical", "Bottom");
								_header_align.AddProperty("ss", "WrapText", "0");
								_header_default.AddObject(0, ref _header_align);
							};

							ExcelObject _header_font = new ExcelObject("Font");
							if (_header_font != null)
							{
								_header_font.AddProperty("ss", "Color", "#FFFFFF");
								_header_font.AddProperty("ss", "Bold", "1");
								_header_default.AddObject(0, ref _header_font);
							};

							ExcelObject _header_interior = new ExcelObject("Interior");
							if (_header_interior != null)
							{
								_header_interior.AddProperty("ss", "Color", "#333333");
								_header_interior.AddProperty("ss", "Pattern", "Solid");
								_header_default.AddObject(0, ref _header_interior);
							};

							_header_default.AddProperty("ss", "ID", "s21");
							_styles.AddObject(0, ref _header_default);
						};


						ExcelObject _number_default = new ExcelObject("Style");
						if (_number_default != null)
						{
							ExcelObject _number_format_default = new ExcelObject("NumberFormat");
							if (_number_format_default != null)
							{
								_number_default.AddObject(0, ref _number_format_default);
							};

							ExcelObject _number_default_interior = new ExcelObject("Interior");
							if (_number_default_interior != null)
							{
								_number_default_interior.AddProperty("ss", "Color", "#FFFFFF");
								_number_default_interior.AddProperty("ss", "Pattern", "Solid");
								_number_default.AddObject(0, ref _number_default_interior);
							};

							_number_default.AddProperty("ss", "ID", "s22");
							_styles.AddObject(0, ref _number_default);
						};

						ExcelObject _number_zero_precision = new ExcelObject("Style");
						if (_number_zero_precision != null)
						{
							ExcelObject _number_format_zero_precision = new ExcelObject("NumberFormat");
							if (_number_format_zero_precision != null)
							{
								_number_format_zero_precision.AddProperty("ss", "Format", "0");
								_number_zero_precision.AddObject(0, ref _number_format_zero_precision);
							};

							ExcelObject _number_zero_precision_interior = new ExcelObject("Interior");
							if (_number_zero_precision_interior != null)
							{
								_number_zero_precision_interior.AddProperty("ss", "Color", "#FFFFFF");
								_number_zero_precision_interior.AddProperty("ss", "Pattern", "Solid");
								_number_zero_precision.AddObject(0, ref _number_zero_precision_interior);
							};

							_number_zero_precision.AddProperty("ss", "ID", "s23");
							_styles.AddObject(0, ref _number_zero_precision);
						};

						ExcelObject _number_four_precision = new ExcelObject("Style");
						if (_number_four_precision != null)
						{
							ExcelObject _number_format_four_precision = new ExcelObject("NumberFormat");
							if (_number_format_four_precision != null)
							{
								_number_format_four_precision.AddProperty("ss", "Format", "0.0000");
								_number_four_precision.AddObject(0, ref _number_format_four_precision);
							};

							ExcelObject _number_four_precision_interior = new ExcelObject("Interior");
							if (_number_four_precision_interior != null)
							{
								_number_four_precision_interior.AddProperty("ss", "Color", "#FFFFFF");
								_number_four_precision_interior.AddProperty("ss", "Pattern", "Solid");
								_number_four_precision.AddObject(0, ref _number_four_precision_interior);
							};

							_number_four_precision.AddProperty("ss", "ID", "s24");
							_styles.AddObject(0, ref _number_four_precision);
						};

						ExcelObject _date_style = new ExcelObject("Style");
						if (_date_style != null)
						{
							ExcelObject _date_format = new ExcelObject("NumberFormat");
							if (_date_format != null)
							{
								_date_format.AddProperty("ss", "Format", "Short Date");
								_date_style.AddObject(0, ref _date_format);
							};

							ExcelObject _date_style_interior = new ExcelObject("Interior");
							if (_date_style_interior != null)
							{
								_date_style_interior.AddProperty("ss", "Color", "#FFFFFF");
								_date_style_interior.AddProperty("ss", "Pattern", "Solid");
								_date_style.AddObject(0, ref _date_style_interior);
							};

							_date_style.AddProperty("ss", "ID", "s25");
							_styles.AddObject(0, ref _date_style);
						};


						//Other color
						ExcelObject _number_default_ = new ExcelObject("Style");
						if (_number_default_ != null)
						{
							ExcelObject _number_format_default_ = new ExcelObject("NumberFormat");
							if (_number_format_default_ != null)
							{
								_number_default_.AddObject(0, ref _number_format_default_);
							};

							ExcelObject _number_default_interior_ = new ExcelObject("Interior");
							if (_number_default_interior_ != null)
							{
								_number_default_interior_.AddProperty("ss", "Color", "#C0C0C0");
								_number_default_interior_.AddProperty("ss", "Pattern", "Solid");
								_number_default_.AddObject(0, ref _number_default_interior_);
							};

							_number_default_.AddProperty("ss", "ID", "s26");
							_styles.AddObject(0, ref _number_default_);
						};

						ExcelObject _number_zero_precision_ = new ExcelObject("Style");
						if (_number_zero_precision_ != null)
						{
							ExcelObject _number_format_zero_precision_ = new ExcelObject("NumberFormat");
							if (_number_format_zero_precision_ != null)
							{
								_number_format_zero_precision_.AddProperty("ss", "Format", "0");
								_number_zero_precision_.AddObject(0, ref _number_format_zero_precision_);
							};

							ExcelObject _number_zero_precision_interior_ = new ExcelObject("Interior");
							if (_number_zero_precision_interior_ != null)
							{
								_number_zero_precision_interior_.AddProperty("ss", "Color", "#C0C0C0");
								_number_zero_precision_interior_.AddProperty("ss", "Pattern", "Solid");
								_number_zero_precision_.AddObject(0, ref _number_zero_precision_interior_);
							};

							_number_zero_precision_.AddProperty("ss", "ID", "s27");
							_styles.AddObject(0, ref _number_zero_precision_);
						};

						ExcelObject _number_four_precision_ = new ExcelObject("Style");
						if (_number_four_precision_ != null)
						{
							ExcelObject _number_format_four_precision_ = new ExcelObject("NumberFormat");
							if (_number_format_four_precision_ != null)
							{
								_number_format_four_precision_.AddProperty("ss", "Format", "0.0000");
								_number_four_precision_.AddObject(0, ref _number_format_four_precision_);
							};

							ExcelObject _number_four_precision_interior_ = new ExcelObject("Interior");
							if (_number_four_precision_interior_ != null)
							{
								_number_four_precision_interior_.AddProperty("ss", "Color", "#C0C0C0");
								_number_four_precision_interior_.AddProperty("ss", "Pattern", "Solid");
								_number_four_precision_.AddObject(0, ref _number_four_precision_interior_);
							};

							_number_four_precision_.AddProperty("ss", "ID", "s28");
							_styles.AddObject(0, ref _number_four_precision_);
						};

						ExcelObject _date_style_ = new ExcelObject("Style");
						if (_date_style_ != null)
						{
							ExcelObject _date_format_ = new ExcelObject("NumberFormat");
							if (_date_format_ != null)
							{
								_date_format_.AddProperty("ss", "Format", "Short Date");
								_date_style_.AddObject(0, ref _date_format_);
							};

							ExcelObject _date_style_interior_ = new ExcelObject("Interior");
							if (_date_style_interior_ != null)
							{
								_date_style_interior_.AddProperty("ss", "Color", "#C0C0C0");
								_date_style_interior_.AddProperty("ss", "Pattern", "Solid");
								_date_style_.AddObject(0, ref _date_style_interior_);
							};

							_date_style_.AddProperty("ss", "ID", "s29");
							_styles.AddObject(0, ref _date_style_);
						};

						_root.AddObject(0, ref _styles);
					};
				};
			}

			public ExcelTable CreateWorkSheet(string name, ref ExcelColumns columns)
			{
				ExcelTable result = null;

				if (_root != null)
				{
					ExcelObject _worksheet = new ExcelObject("Worksheet");
					if (_worksheet != null)
					{
						_worksheet.AddProperty("ss", "Name", name);

						ExcelTable _table = new ExcelTable(ref columns);
						if (_table != null)
						{
							ExcelObject _table_object = _table.GetTableObject();
							if (_table_object != null)
							{
								_worksheet.AddObject(0, ref _table_object);
								_table_object.IsNewLine = true;
							};

							result = _table;
						};

						//***_root.AddObject(0, ref _worksheet);
					};
				};

				return result;
			}

			public string ConvertSystemTypeToExcelType(string system_type_name)
			{
				string result = "String";

				switch (system_type_name)
				{
					case "System.Int16":
						result = "Number";
						break;
					case "System.Int32":
						result = "Number";
						break;
					case "System.Byte":
						result = "Number";
						break;
					case "System.Decimal":
						result = "Number";
						break;
					case "System.String":
						result = "String";
						break;
					case "System.DateTime":
						result = "DateTime";
						break;
				};

				return result;
			}

			public string ConvertSystemTypeToExcelStyle(string system_type_name)
			{
				string result = "s22";

				switch (system_type_name)
				{
					case "System.Int16":
						result = "23";
						break;
					case "System.Int32":
						result = "23";
						break;
					case "System.Byte":
						result = "23";
						break;
					case "System.Decimal":
						result = "24";
						break;
					case "System.Guid":
						result = "22";
						break;
					case "System.String":
						result = "22";
						break;
					case "System.DateTime":
						result = "25";
						break;
				};

				return result;
			}

			public int ConvertSystemTypeToCellWidth(string system_type_name)
			{
				int result = 100;

				switch (system_type_name)
				{
					case "System.Int16":
						result = 50;
						break;
					case "System.Int32":
						result = 50;
						break;
					case "System.Byte":
						result = 25;
						break;
					case "System.Decimal":
						result = 50;
						break;
					case "System.Guid":
						result = 260;
						break;
					case "System.String":
						result = 200;
						break;
					case "System.DateTime":
						result = 75;
						break;
				};

				return result;
			}

			public ExcelRow AddHeader(ref ExcelTable excel_table, ref ExcelColumns exist_columns)
			{
				ExcelRow result = null;

				if ((excel_table != null) && (exist_columns != null))
				{
					int column_count = exist_columns.Count();
					if (column_count > 0)
					{
						result = AddRow(0, ref excel_table);
						if (result != null)
						{
							string column_name = string.Empty;
							string column_alias = string.Empty;
							for (int index = 0; index < column_count; index++)
							{
								ExcelObject _object = exist_columns.GetColumnObject(index);
								if (_object != null)
								{
									column_name = _object.GetPropertyValue("Name");
									column_alias = _object.GetPropertyValue("Alias");
								};

								if (column_name.Length > 0)
									result.SetHeaderValue(column_name, column_alias);
							};
						};
					};
				};

				return result;
			}

			public ExcelColumns CreateColumns(ref ExcelColumns exist_columns, ref DataTable data_table)
			{
				ExcelColumns result = null;
				int column_count = 0;

				if (exist_columns == null)
					result = new ExcelColumns();
				else
					result = exist_columns;

				if ((result != null) && (data_table != null))
				{
					column_count = result.Count();
					int global_index = 0;
					for (int index = 0; index < data_table.Columns.Count; index++)
					{
						string _name = data_table.Columns[index].ColumnName.ToString();
						string _alias = _name;

						if (_name.ToLower() == "dateaquired")
						{
							_alias = "DateAcquired";
						};

						if (_column_filter.Length > 0)
						{
							if (_name.ToLower().Contains(_column_filter.ToLower()) && _name != "AssetGUID")
								continue;
						};

						string _type = ConvertSystemTypeToExcelType(data_table.Columns[index].DataType.ToString());
						string _system_type = data_table.Columns[index].DataType.ToString();
						string _style_id = ConvertSystemTypeToExcelStyle(data_table.Columns[index].DataType.ToString());

						int _width = ConvertSystemTypeToCellWidth(data_table.Columns[index].DataType.ToString());

						//columns and rows stored on common table object, need set not collise indexes
						result.AddColumn(column_count + global_index + 1, _name, _alias, _width, _type, _system_type, _style_id);
						global_index++;
					};
				};

				return result;
			}

			public ExcelColumns AddColumn(ref ExcelColumns exist_columns, ref DataColumn data_column)
			{
				ExcelColumns result = null;
				int column_count = 0;

				if (exist_columns == null)
					result = new ExcelColumns();
				else
					result = exist_columns;

				if ((result != null) && (data_column != null))
				{
					column_count = result.Count();

					string _name = data_column.ColumnName.ToString();

					if (_column_filter.Length > 0)
					{
						if (_name.ToLower().Contains(_column_filter.ToLower()) && _name != "AssetGUID")
						{
							string __type = ConvertSystemTypeToExcelType(data_column.DataType.ToString());
							string __system_type = data_column.DataType.ToString();
							string __style_id = ConvertSystemTypeToExcelStyle(data_column.DataType.ToString());

							int __width = ConvertSystemTypeToCellWidth(data_column.DataType.ToString());

							string _alias = _name;
							if (_name.ToLower() == "dateaquired")
								_alias = "DateAcquired";

							//columns and rows stored on common table object, need set not collise indexes
							result.AddColumn(column_count + 1, _name, _alias, __width, __type, __system_type, __style_id);
						};
					}
					else
					{
						string _type = ConvertSystemTypeToExcelType(data_column.DataType.ToString());
						string _system_type = data_column.DataType.ToString();
						string _style_id = ConvertSystemTypeToExcelStyle(data_column.DataType.ToString());

						int _width = ConvertSystemTypeToCellWidth(data_column.DataType.ToString());

						string _alias = _name;
						if (_name.ToLower() == "dateaquired")
							_alias = "DateAcquired";

						//columns and rows stored on common table object, need set not collise indexes
						result.AddColumn(column_count + 1, _name, _alias, _width, _type, _system_type, _style_id);
					};
				};

				return result;
			}

			//Warning: this function not filter columns by filter;
			public ExcelColumns AddColumn(ref ExcelColumns exist_columns, string column_name, string column_system_type)
			{
				ExcelColumns result = null;
				int column_count = 0;

				if (exist_columns == null)
					result = new ExcelColumns();
				else
					result = exist_columns;

				if ((result != null) && (column_name.Length > 0))
				{
					column_count = result.Count();

					string _name = column_name;

					string __type = ConvertSystemTypeToExcelType(column_system_type);
					string __system_type = column_system_type;
					string __style_id = ConvertSystemTypeToExcelStyle(column_system_type);

					int __width = ConvertSystemTypeToCellWidth(column_system_type);

					string _alias = _name;
					if (_name.ToLower() == "dateaquired")
						_alias = "DateAcquired";

					result.AddColumn(column_count + 1, _name, _alias, __width, __type, __system_type, __style_id);
				};

				return result;
			}

			public ExcelRow AddRow(int index, ref ExcelTable excel_table)
			{
				ExcelRow result = null;

				if (excel_table != null)
				{
					result = excel_table.AddRow(index + 1);
				};

				return result;
			}

			public void UpdateRow(ref ExcelRow excel_row, ref DataTable data_table, ref DataRow data_row, bool is_white)
			{
				if ((excel_row != null) && (data_table != null) && (data_row != null))
				{
					int column_count = data_table.Columns.Count;

					for (int column_index = 0; column_index < column_count; column_index++)
					{
						string _column_name = data_table.Columns[column_index].ColumnName;

						if (_column_filter.Length > 0)
						{
							if (_column_name.ToLower().Contains(_column_filter.ToLower()) && _column_name != "AssetGUID")
								continue;
						};

						excel_row.SetValue(_column_name, data_row[column_index].ToString(), is_white);
					};
				};
			}

			public void UpdateRow(ref ExcelRow excel_row, ref DataTable all_properties, ref DataTable data_table, string column_name_field, string column_value_field, bool is_white)
			{
				if ((excel_row != null) && (all_properties != null) && (data_table != null) && (column_name_field.Length > 0) && (column_value_field.Length > 0))
				{
					foreach (DataRow _row in data_table.Rows)
					{
						if (_row != null)
						{
							string _column_name = _row[column_name_field].ToString();
							excel_row.SetValue(_column_name, _row[column_value_field].ToString(), is_white);
						};
					};

					foreach (DataRow _allrow in all_properties.Rows)
					{
						if (_allrow != null)
						{
							string _property_name = _allrow[column_name_field].ToString();
							excel_row.SetValue(_property_name, "", is_white);
						};
					};
				};
			}

			public bool CreateSheet(string name, ref DataTable data_table)
			{
				bool result = false;

				if (data_table != null)
				{
					ExcelColumns columns = null;
					columns = CreateColumns(ref columns, ref data_table);

					int column_count = 0;

					if (columns != null)
						column_count = data_table.Columns.Count;

					ExcelTable _table = this.CreateWorkSheet(name, ref columns);
					if (_table != null)
					{
						AddHeader(ref _table, ref columns);
						bool is_white = true;
						for (int row_index = 0; row_index < data_table.Rows.Count; row_index++)
						{
							DataRow _row = data_table.Rows[row_index];
							if (_row != null)
							{
								ExcelRow excel_row = AddRow(row_index + 1, ref _table);
								UpdateRow(ref excel_row, ref data_table, ref _row, is_white);

								if (is_white)
									is_white = false;
								else
									is_white = true;
							};
						};
					};

					result = true;
				};

				return result;
			}

			override public string ToString()
			{
				string result = _header;

				if (_root != null)
				{
					result = result + _root.ToString();
				}

				return result;
			}
		}

		public class ExportAssets : ExcelDocument
		{
			public ExportAssets(ref System.Web.UI.HtmlTextWriter writer)
			{
				ColumnFilter = "id";
				SetWriter(ref writer);
			}

			private DataTable GetAssetProperties(int departmentId, int assetTypeId)
			{
				string sqlQuery = "SELECT * FROM AssetTypeProperties WHERE DId=" + departmentId + " AND AssetTypeId=" + assetTypeId;
				return SelectByQuery(sqlQuery);
			}

			private DataTable GetAssetPropertyValues(int departmentId, int assetId, int assetTypeId)
			{
				string sqlQuery = "SELECT AssetPropertyValues.*, AssetTypeProperties.AssetTypeId, AssetTypeProperties.Name, AssetTypeProperties.DataType, AssetTypeProperties.Enumeration, AssetTypeProperties.Description FROM AssetPropertyValues INNER JOIN AssetTypeProperties ON AssetPropertyValues.DId = AssetTypeProperties.DId AND AssetPropertyValues.AssetTypePropertyId = AssetTypeProperties.Id ";
				sqlQuery += "where AssetPropertyValues.DId=" + departmentId.ToString();
				sqlQuery += " and AssetPropertyValues.AssetId=" + assetId.ToString();
				sqlQuery += " and AssetTypeProperties.AssetTypeId=" + assetTypeId.ToString();

				return SelectByQuery(sqlQuery);
			}

			private bool CreateAssetSheet(int group_index, ref DataTable table)
			{
				bool result = false;

				System.Web.UI.HtmlTextWriter _writer = GetWriter();

				if ((table != null) && (_writer != null))
				{
					string expression = "GroupId=" + group_index.ToString();
					DataRow[] foundRows = null;
					foundRows = table.Select(expression);

					ExcelColumns columns = null;

					if (foundRows != null)
					{
						if (foundRows.Length > 0)
						{
							result = true;
							DataRow _init = foundRows[0];
							if (_init != null)
							{
								int _asset_type_id = Int32.Parse(_init["AssetTypeId"].ToString());
								int _department_id = Int32.Parse(_init["DepartmentId"].ToString());

								columns = CreateColumns(ref columns, ref table);
								if (columns != null)
								{
									DataTable _custom_properties_table = GetAssetProperties(_department_id, _asset_type_id);
									if (_custom_properties_table != null)
									{
										foreach (DataRow _row in _custom_properties_table.Rows)
										{
											if (_row != null)
											{
												string _property_name = _row["Name"].ToString();
												int _property_type = Int32.Parse(_row["DataType"].ToString());
												string _system_type = "System.String";

												switch (_property_type)
												{
													case 0:
														_system_type = "System.String";
														break;
													case 1:
														_system_type = "System.Int32";
														break;
													case 2:
														_system_type = "System.Decimal";
														break;
													case 3:
														_system_type = "System.DateTime";
														break;
													case 4:
														_system_type = "System.String";
														break;
												}

												columns = AddColumn(ref columns, _property_name, _system_type);
											}
										}
									}
								}
							}
						}
						else
							return false;
					}
					else
						return false;

					int _sheet_id = group_index + 1;
					string _sheet_name = "AssetsGroup" + _sheet_id.ToString();

					string _start_worksheet = "<Worksheet ss:Name=\"" + _sheet_name + "\">";
					string _end_worksheet = "</Worksheet>";
					_writer.Write(_start_worksheet);

					ExcelTable _table = this.CreateWorkSheet(_sheet_name, ref columns);
					if (_table != null)
					{
						AddHeader(ref _table, ref columns);

						//***new
						_writer.Write(_table.ToHeader());

						bool is_white = true;
						DataTable _custom_properties_full = null;
						for (int row_index = 0; row_index < foundRows.Length; row_index++)
						{
							DataRow _row = foundRows[row_index];
							if (_row != null)
							{
								//***ExcelRow excel_row = AddRow(row_index + 1, ref _table);
								ExcelRow excel_row = new ExcelRow(_table);
								if (excel_row != null)
								{
									UpdateRow(ref excel_row, ref table, ref _row, is_white);

									int _department = Int32.Parse(_row["DepartmentId"].ToString());
									int _asset_id = Int32.Parse(_row["Id"].ToString());
									int _asset_type_id = Int32.Parse(_row["AssetTypeId"].ToString());

									if (_custom_properties_full == null)
										_custom_properties_full = GetAssetProperties(_department, _asset_type_id);

									DataTable _custom_properties = GetAssetPropertyValues(_department, _asset_id, _asset_type_id);
									UpdateRow(ref excel_row, ref _custom_properties_full, ref _custom_properties, "Name", "PropertyValue", is_white);

									if (is_white)
										is_white = false;
									else
										is_white = true;

									//***new
									_writer.Write(excel_row.ToString());
									_writer.Flush();
								}
							}
						}

						//***new
						_writer.Write(_table.ToFooter());
					}

					_writer.Write(_end_worksheet);
				}

				return result;
			}

			public string Export(ref DataTable table)
			{
				string result = "";

				int group_index = 0;
				bool group_result = true;

				System.Web.UI.HtmlTextWriter _writer = GetWriter();

				if (_writer != null)
				{
					string _document = ToString();

					if (_document.Length > "</Workbook>".Length)
					{
						_document = _document.Remove(_document.Length - "</Workbook>".Length - 1, "</Workbook>".Length);
					}

					_writer.Write(_document);
					_writer.Flush();

					while (group_result)
					{
						group_result = CreateAssetSheet(group_index, ref table);
						group_index++;
					}

					_writer.Write("</Workbook>");
					_writer.Flush();
				}

				return result;
			}
		}

		public class ImportColumn
		{
			private int _column_index;
			private string _column_name;
			private string _column_system_type;
			private bool _column_require;
			private string _column_enumeration;

			public int ColumnIndex
			{
				set { _column_index = value; }
				get { return _column_index; }
			}

			public bool IsColumnRequire
			{
				set { _column_require = value; }
				get { return _column_require; }
			}

			public string ColumnName
			{
				set { _column_name = value; }
				get { return _column_name; }
			}

			public string ColumnEnumeration
			{
				set { _column_enumeration = value; }
				get { return _column_enumeration; }
			}

			public string ColumnSystemType
			{
				set { _column_system_type = value; }
				get { return _column_system_type; }
			}

			public ImportColumn(int column_index, string column_name, string column_system_type)
			{
				_column_index = column_index;
				_column_name = column_name;
				_column_system_type = column_system_type;
				_column_require = false;
				_column_enumeration = "";
			}

			public ImportColumn(string column_name, string column_system_type)
			{
				_column_index = -1;
				_column_name = column_name;
				_column_system_type = column_system_type;
				_column_require = false;
				_column_enumeration = "";
			}
		}

		public class ImportColumns
		{
			private SortedList _columns;

			public ImportColumns()
			{
				_columns = new SortedList();
			}

			public void Clear()
			{
				if (_columns != null)
					_columns.Clear();
			}

			public void CreateColumns(ref DataTable data_table, string filter)
			{
				if (data_table != null)
				{
					for (int index = 0; index < data_table.Columns.Count; index++)
					{
						string _name = data_table.Columns[index].ColumnName.ToString();

						if (filter.Length > 0)
						{
							if (_name.ToLower().Contains(filter.ToLower()) && _name != "AssetGUID")
								continue;
						}

						string _system_type = data_table.Columns[index].DataType.ToString();

						AddColumn(-1, _name, _system_type, false);
					}
				}

				return;
			}

			public void CreateColumnsInTable(ref DataTable etalon_data_table, ref DataTable data_table, string filter)
			{
				if ((etalon_data_table != null) && (data_table != null))
				{
					for (int index = 0; index < etalon_data_table.Columns.Count; index++)
					{
						string _name = etalon_data_table.Columns[index].ColumnName.ToString();

						if (filter.Length > 0)
						{
							if (_name.ToLower().Contains(filter.ToLower()) && _name != "AssetGUID")
								continue;
						}

						string _system_type = etalon_data_table.Columns[index].DataType.ToString();

						DataColumn data_column = new DataColumn();
						if (data_column != null)
						{
							data_column.DataType = System.Type.GetType(_system_type);
							data_column.AllowDBNull = true;
							data_column.Caption = _name;
							data_column.ColumnName = _name;

							// Add the column to the table. 
							data_table.Columns.Add(data_column);
						}
					}
				}

				return;
			}

			public bool AddColumn(int column_index, string column_name, string column_system_type, bool column_require)
			{
				bool result = false;

				if (_columns != null)
				{
					if (!_columns.Contains(column_name))
					{
						ImportColumn _column = new ImportColumn(column_index, column_name, column_system_type);
						if (_column != null)
						{
							_column.IsColumnRequire = column_require;
							_columns.Add(column_name, _column);
							result = true;
						}
					}
				}
				else
				{
					ImportColumn _column = ((ImportColumn)_columns[column_name]);
					if (_column != null)
					{
						_column.ColumnIndex = column_index;
						_column.ColumnName = column_name;
						_column.ColumnSystemType = column_system_type;
						result = true;
					}
				}

				return result;
			}

			public ImportColumn GetColumn(string column_name)
			{
				ImportColumn result = null;

				if (_columns != null)
				{
					if (_columns.Contains(column_name))
					{
						object _key = (object)column_name;
						result = (ImportColumn)_columns[_key];
					}
				}

				return result;
			}

			public ImportColumn GetColumn(int index)
			{
				ImportColumn result = null;

				if (_columns != null)
				{
					if ((index >= 0) && (index < GetColumnCount()))
					{
						result = (ImportColumn)_columns[_columns.GetKey(index).ToString()];
					}
				}

				return result;
			}

			public int GetColumnCount()
			{
				int result = 0;

				if (_columns != null)
				{
					result = _columns.Count;
				}

				return result;
			}

			public bool IsColumnExist(string column_name)
			{
				bool result = false;

				if (_columns != null)
				{
					if (_columns.Contains(column_name))
					{
						result = true;
					};
				}

				return result;
			}

			public bool Remove(string column_name)
			{
				bool result = false;

				if (_columns != null)
				{
					if (_columns.Contains(column_name))
					{
						_columns.Remove(column_name);
						result = true;
					}
				}

				return result;
			}

		};

		public class ImportCustomProperty
		{
			public DataTable _table = null;

			public ImportCustomProperty()
			{
				if (_table == null)
					CreateStructure();
				else
					Clear();
			}

			public void Clear()
			{
				if (_table != null)
				{
					_table.Clear();
					_table = null;
				}
			}

			private void AddNewRow(int custom_property_id, int asset_type_id, ref string name, int data_type, ref string enumeration, ref string new_enumeration_value, ref string group_name)
			{
				if (_table != null)
				{
					DataRow _new_row = _table.NewRow();
					if (_new_row != null)
					{
						string _CrLf = new string(new char[] { (char)13, (char)10 });

						enumeration = enumeration.Replace(_CrLf, "|");

						string _enumeration = "|" + enumeration + "|";
						string _search_value = "|" + new_enumeration_value + "|";

						if (!_enumeration.Contains(_search_value))
						{
							_new_row["Id"] = custom_property_id;
							_new_row["AssetTypeId"] = asset_type_id;
							_new_row["Name"] = name;
							_new_row["DataType"] = data_type;
							_new_row["NewEnumeration"] = new_enumeration_value;
							_new_row["Enumeration"] = enumeration + "|" + new_enumeration_value;
							_new_row["GroupName"] = group_name;

							_table.Rows.Add(_new_row);
						}
					}
				}
			}

			public void Add(int custom_property_id, int asset_type_id, string name, int data_type, string enumeration, string new_enumeration_value, string group_name)
			{
				if (_table != null)
				{
					DataRow[] _result = null;
					string _expression;
					_expression = "Id=" + custom_property_id.ToString() + " AND AssetTypeId=" + asset_type_id.ToString();
					_result = _table.Select(_expression);

					if (_result == null)
					{
						AddNewRow(custom_property_id, asset_type_id, ref name, data_type, ref enumeration, ref new_enumeration_value, ref group_name);
					}
					else
					{
						if (_result.Length == 0)
							AddNewRow(custom_property_id, asset_type_id, ref name, data_type, ref enumeration, ref new_enumeration_value, ref group_name);

						if (_result.Length == 1)
						{
							DataRow _exist_row = _result[0];
							if (_exist_row != null)
							{
								string _enumeration = _exist_row["Enumeration"].ToString();
								string _new_enumeration = _exist_row["NewEnumeration"].ToString();

								string __enumeration = "|" + _enumeration + "|";
								string __search_value = "|" + new_enumeration_value + "|";

								if (!__enumeration.Contains(__search_value))
								{
									_new_enumeration = _new_enumeration + "; " + new_enumeration_value;
									_enumeration = _enumeration + "|" + new_enumeration_value;
									_exist_row["Enumeration"] = _enumeration;
									_exist_row["NewEnumeration"] = _new_enumeration;
								}
							}
						}
					}
				}
			}

			public DataTable GetImportCustomProperty()
			{
				return _table;
			}

			private void CreateStructure()
			{
				_table = new DataTable();
				if (_table != null)
				{
					_table.TableName = "ImportCustomProperty";

					//CustomPropertyId
					DataColumn column1 = new DataColumn();
					column1.DataType = System.Type.GetType("System.Int32");
					column1.AllowDBNull = false;
					column1.Caption = "Id";
					column1.ColumnName = "Id";
					column1.DefaultValue = 0;

					DataColumn column2 = new DataColumn();
					column2.DataType = System.Type.GetType("System.Int32");
					column2.AllowDBNull = false;
					column2.Caption = "AssetTypeId";
					column2.ColumnName = "AssetTypeId";
					column2.DefaultValue = 0;

					DataColumn column3 = new DataColumn();
					column3.DataType = System.Type.GetType("System.String");
					column3.AllowDBNull = false;
					column3.Caption = "Name";
					column3.ColumnName = "Name";
					column3.DefaultValue = "";

					DataColumn column4 = new DataColumn();
					column4.DataType = System.Type.GetType("System.Int32");
					column4.AllowDBNull = false;
					column4.Caption = "DataType";
					column4.ColumnName = "DataType";
					column4.DefaultValue = 0;

					DataColumn column5 = new DataColumn();
					column5.DataType = System.Type.GetType("System.String");
					column5.AllowDBNull = false;
					column5.Caption = "NewEnumeration";
					column5.ColumnName = "NewEnumeration";
					column5.DefaultValue = "";

					DataColumn column6 = new DataColumn();
					column6.DataType = System.Type.GetType("System.String");
					column6.AllowDBNull = false;
					column6.Caption = "Enumeration";
					column6.ColumnName = "Enumeration";
					column6.DefaultValue = "";

					DataColumn column7 = new DataColumn();
					column7.DataType = System.Type.GetType("System.String");
					column7.AllowDBNull = false;
					column7.Caption = "GroupName";
					column7.ColumnName = "GroupName";
					column7.DefaultValue = "";

					// Add the column to the table. 
					_table.Columns.Add(column1);
					_table.Columns.Add(column2);
					_table.Columns.Add(column3);
					_table.Columns.Add(column4);
					_table.Columns.Add(column5);
					_table.Columns.Add(column6);
					_table.Columns.Add(column7);
				}
			}
		}

		public class ImportLog
		{
			private int current_errors = 0;
			private int current_warnings = 0;

			private int MAX_ERRORS = 32;
			private int MAX_WARNINGS = 32;

			private SortedList _list = null;
			private bool _is_critical_errors;


			public enum ErrorType
			{
				Critical = 1,
				Error = 2,
				Warning = 3,
				Info = 4
			};

			public DataTable _log = null;

			public ImportLog()
			{
				_is_critical_errors = false;
			}

			public void Clear()
			{
				if (_log != null)
				{
					_log.Clear();
					_log = null;
				}
			}

			public DataTable GetLog()
			{
				return _log;
			}

			private bool CheckNewLevel(string id)
			{
				bool result = false;

				if (_list != null)
				{
					if (_list.Contains(id))
						result = false;
					else
					{
						_list.Add(id, id);
						result = true;
					};
				};

				return result;
			}

			public bool IsHaveCriticalErrors()
			{
				return _is_critical_errors;
			}

			public void Add(ref DataRow SourceDataRow, ErrorType error_type, string sheet_name, string asset_path, string asset_serial, string asset_field_name, string asset_field_value, string description)
			{
				if (_log == null)
					CreateStructure();

				DataRow _row = null;

				string _error = "";
				int _error_type = 0;
				switch (error_type)
				{
					case ErrorType.Info:
						_error_type = 4;
						_error = "Info";
						break;
					case ErrorType.Warning:
						_error_type = 3;
						_error = "Warning(s) - This value(s) can't import, if need this value(s) then resolve problem(s) and try again.";
						current_warnings++;
						break;
					case ErrorType.Error:
						_error_type = 2;
						_error = "Error(s) - This asset(s) can't import, if need this asset(s) then resolve problem(s) and try again.";
						current_errors++;
						break;
					case ErrorType.Critical:
						_error_type = 1;

						if (!_is_critical_errors)
							_is_critical_errors = true;

						_error = "Critical Error(s) - File can't import, please resolve problem(s) and try again.";
						break;
				}

				if ((_error_type == 2) && (current_errors > MAX_ERRORS))
					return;

				if ((_error_type == 3) && (current_warnings > MAX_WARNINGS))
					return;

				if (SourceDataRow != null)
				{
					if ((_error_type == 1) || (_error_type == 2))
						SourceDataRow["GroupId"] = -1;
				}

				if (_log != null)
				{
					string _unique_group_name = sheet_name + "_" + _error_type.ToString();
					if (CheckNewLevel(_unique_group_name))
					{
						DataRow _new_row = _log.NewRow();
						if (_new_row != null)
						{
							_new_row["Id"] = -1;
							_new_row["ErrorType"] = _error_type;
							_new_row["ErrorTypeDesc"] = _error;
							_new_row["SheetName"] = sheet_name;
							_new_row["AssetPath"] = asset_path;
							_new_row["AssetSerial"] = asset_serial;
							_new_row["FieldName"] = asset_field_name;
							_new_row["FieldValue"] = asset_field_value;
							_new_row["Description"] = description;

							_log.Rows.Add(_new_row);
						}
					}

					_row = _log.NewRow();

					if (_row != null)
					{
						_row["Id"] = 0;
						_row["ErrorType"] = _error_type;
						_row["ErrorTypeDesc"] = _error;
						_row["SheetName"] = sheet_name;
						_row["AssetPath"] = asset_path;
						_row["AssetSerial"] = asset_serial;
						_row["FieldName"] = asset_field_name;
						_row["FieldValue"] = asset_field_value;
						_row["Description"] = description;

						_log.Rows.Add(_row);
					}
				}
			}

			public void Closure()
			{
				if (_log != null)
				{
					if (current_errors > MAX_ERRORS)
					{
						DataRow _error_row = _log.NewRow();
						if (_error_row != null)
						{
							_error_row["Id"] = -1;
							_error_row["ErrorType"] = 2; //Error
							_error_row["ErrorTypeDesc"] = "Total Errors: " + current_errors.ToString();
							_error_row["SheetName"] = "";
							_error_row["AssetPath"] = "";
							_error_row["AssetSerial"] = "";
							_error_row["FieldName"] = "";
							_error_row["FieldValue"] = "";
							_error_row["Description"] = "";

							_log.Rows.Add(_error_row);
						}
					}

					if (current_warnings > MAX_WARNINGS)
					{
						DataRow _warning_row = _log.NewRow();
						if (_warning_row != null)
						{
							_warning_row["Id"] = -1;
							_warning_row["ErrorType"] = 3; //Error
							_warning_row["ErrorTypeDesc"] = "Total Warnings: " + current_warnings.ToString();
							_warning_row["SheetName"] = "";
							_warning_row["AssetPath"] = "";
							_warning_row["AssetSerial"] = "";
							_warning_row["FieldName"] = "";
							_warning_row["FieldValue"] = "";
							_warning_row["Description"] = "";

							_log.Rows.Add(_warning_row);
						};
					};
				};
			}

			private void CreateStructure()
			{
				_list = new SortedList();
				_log = new DataTable();
				if (_log != null)
				{
					_log.TableName = "ImportLog";

					DataColumn column1 = new DataColumn();
					column1.DataType = System.Type.GetType("System.Int32");
					column1.AllowDBNull = false;
					column1.Caption = "Id";
					column1.ColumnName = "Id";
					column1.DefaultValue = 0;

					DataColumn column2 = new DataColumn();
					column2.DataType = System.Type.GetType("System.Int32");
					column2.AllowDBNull = false;
					column2.Caption = "ErrorType";
					column2.ColumnName = "ErrorType";
					column2.DefaultValue = 0;

					DataColumn column3 = new DataColumn();
					column3.DataType = System.Type.GetType("System.String");
					column3.AllowDBNull = false;
					column3.Caption = "ErrorTypeDesc";
					column3.ColumnName = "ErrorTypeDesc";
					column3.DefaultValue = "";

					DataColumn column4 = new DataColumn();
					column4.DataType = System.Type.GetType("System.String");
					column4.AllowDBNull = false;
					column4.Caption = "SheetName";
					column4.ColumnName = "SheetName";
					column4.DefaultValue = "";

					DataColumn column5 = new DataColumn();
					column5.DataType = System.Type.GetType("System.String");
					column5.AllowDBNull = false;
					column5.Caption = "AssetPath";
					column5.ColumnName = "AssetPath";
					column5.DefaultValue = "";

					DataColumn column6 = new DataColumn();
					column6.DataType = System.Type.GetType("System.String");
					column6.AllowDBNull = false;
					column6.Caption = "AssetSerial";
					column6.ColumnName = "AssetSerial";
					column6.DefaultValue = "";

					DataColumn column8 = new DataColumn();
					column8.DataType = System.Type.GetType("System.String");
					column8.AllowDBNull = false;
					column8.Caption = "FieldName";
					column8.ColumnName = "FieldName";
					column8.DefaultValue = "";

					DataColumn column9 = new DataColumn();
					column9.DataType = System.Type.GetType("System.String");
					column9.AllowDBNull = false;
					column9.Caption = "FieldValue";
					column9.ColumnName = "FieldValue";
					column9.DefaultValue = "";

					DataColumn column10 = new DataColumn();
					column10.DataType = System.Type.GetType("System.String");
					column10.AllowDBNull = false;
					column10.Caption = "Description";
					column10.ColumnName = "Description";
					column10.DefaultValue = "";

					// Add the column to the table. 
					_log.Columns.Add(column1);
					_log.Columns.Add(column2);
					_log.Columns.Add(column3);
					_log.Columns.Add(column4);
					_log.Columns.Add(column5);
					_log.Columns.Add(column6);
					_log.Columns.Add(column8);
					_log.Columns.Add(column9);
					_log.Columns.Add(column10);
				};
			}
		}

		public class ImportRows
		{
			private ImportTable _import_table = null;

			public ImportRows(ImportTable import_table)
			{
				_import_table = import_table;
			}

			public bool Validate()
			{
				bool result = true;

				if (_import_table != null)
				{
					IDataReaderAdapter _reader = _import_table.GetReader();
					if (_reader != null)
					{
						int _old_asset_type_id = 0;
						int _asset_type_id = 0;
						//int __asset_category_id = 0;
						DataTable _custom_property_table = null;

						while (_reader.ReadLine())
						{
							DataRow _row = null;
							DataTable _data_table = _import_table.GetDataTable();
							if (_data_table != null)
							{
								_row = _data_table.NewRow();
								if (_row != null)
									_row["GroupId"] = 0;
							};

							result = ValidateMain(ref _row);

							_asset_type_id = _row["AssetTypeId"].ToString().Length > 0 ? Int32.Parse(_row["AssetTypeId"].ToString()) : 0; //Data.Assets.SelectTypeId(_import_table.GetDepartmentId(), GetAssetValue("AssetCategoryName"), ref __asset_category_id);

							if (_asset_type_id != 0 && _old_asset_type_id != _asset_type_id)
							{
								_custom_property_table = AssetTypeProperties.SelectAll(_import_table.OrganizationId, _import_table.GetDepartmentId(), _asset_type_id);
								_old_asset_type_id = _asset_type_id;
							};

							if (_asset_type_id != 0 && _custom_property_table.Rows.Count > 0)
							{
								CheckCustom(ref _custom_property_table, _asset_type_id, ref _row);
								ValidateCustom(ref _custom_property_table, _asset_type_id, ref _row);
							}

							if ((_data_table != null) && (_row != null))
								_data_table.Rows.Add(_row);
						};
					};

					ImportLog _log = _import_table.GetLog();
					if (_log != null)
						_log.Closure();
				};

				return result;
			}

			private DataRow GetCustomPropertyRow(ref DataTable custom_table, string property_name)
			{
				DataRow result = null;

				if (custom_table != null)
				{
					string expression = "Name='" + property_name.Replace("'", "''") + "'";
					DataRow[] foundRows;

					foundRows = custom_table.Select(expression);

					if (foundRows.Length > 0)
						result = foundRows[0];
				};

				return result;
			}

			private string GetValue(ref DataRow row, string field_name)
			{
				string result = "";

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							if (row[field_name].ToString().Length > 0)
								result = row[field_name].ToString();
						};
					};
				};

				return result;
			}

			private int GetIntValue(ref DataRow row, string field_name)
			{
				int result = -1;

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							try
							{
								if (row[field_name].ToString().Length > 0)
									result = Int32.Parse(row[field_name].ToString());
							}
							catch
							{
								result = -1;
							}
						};
					};
				};

				return result;
			}

			private DateTime GetDateTimeValue(ref DataRow row, string field_name)
			{
				DateTime result = DateTime.MinValue;

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							try
							{
								if (row[field_name].ToString().Length > 0)
									result = DateTime.Parse(row[field_name].ToString());
							}
							catch
							{
								result = DateTime.MinValue;
							}
						};
					};
				};

				return result;
			}


			private double GetDoubleValue(ref DataRow row, string field_name)
			{
				double result = -1;

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							try
							{
								if (row[field_name].ToString().Length > 0)
								{
									Decimal _res = Decimal.Parse(row[field_name].ToString());
									result = Decimal.ToDouble(_res);
								};
							}
							catch
							{
								result = -1;
							}
						};
					};
				};

				return result;
			}

			private bool CheckCustom(ref DataTable custom_table, int asset_type_id, ref DataRow data_row)
			{
				bool result = true;

				if (data_row != null)
				{
					IDataReaderAdapter _reader = _import_table.GetReader();
					if (_reader != null)
					{
						ImportColumns _custom = _import_table.GetCustomColumnList();
						int _custom_column_count = _custom.GetColumnCount();
						for (int _index = 0; _index < _custom_column_count; _index++)
						{
							ImportColumn _custom_column = _custom.GetColumn(_index);
							if (_custom_column != null)
							{
								string _custom_value = GetAssetCustomValue(_custom_column.ColumnName);
								if (_custom_value.Length > 0)
								{
									DataRow _row = GetCustomPropertyRow(ref custom_table, _custom_column.ColumnName);
									if (_row != null)
									{
										int _custom_property_id = GetIntValue(ref _row, "Id");
										int _custom_property_datatype = GetIntValue(ref _row, "DataType");

										string _system_type = "System.String";
										string _system_enums = "";

										switch (_custom_property_datatype)
										{
											case 0:
												_system_type = "System.String";
												break;
											case 1:
												_system_type = "System.Int32";
												break;
											case 2:
												_system_type = "System.Decimal";
												break;
											case 3:
												_system_type = "System.DateTime";
												break;
											case 4:
												_system_type = "System.Enum";
												_system_enums = GetValue(ref _row, "Enumeration");
												break;
										};

										_custom_column.ColumnSystemType = _system_type;
										_custom_column.ColumnEnumeration = _system_enums;

										string _custom_property_name_id = _custom_column.ColumnName + "Id";
										data_row[_custom_property_name_id] = AnalizeAssetBaseId(ref data_row, _custom_property_id, 0, 0, _custom_column.ColumnName, false, true);
									};
								};
							};
						};
					};
				};

				return result;
			}

			private bool ValidateCustom(ref DataTable custom_table, int asset_type, ref DataRow data_row)
			{
				bool result = true;

				if (data_row != null)
				{
					IDataReaderAdapter _reader = _import_table.GetReader();
					if (_reader != null)
					{
						ImportColumns _custom = _import_table.GetCustomColumnList();
						int _custom_column_count = _custom.GetColumnCount();
						for (int _index = 0; _index < _custom_column_count; _index++)
						{
							ImportColumn _custom_column = _custom.GetColumn(_index);
							if (_custom_column != null)
							{
								string custom_value = _reader.GetValue(_custom_column.ColumnIndex);

								string _error_desc = string.Empty;
								string _custom_valid_value = CheckValue(ref data_row, ref _custom_column, custom_value, ref _error_desc);

								if (_error_desc == "value not exist in enumeration set.")
								{
									DataRow _row = GetCustomPropertyRow(ref custom_table, _custom_column.ColumnName);
									if (_row != null)
									{
										int _custom_property_id = GetIntValue(ref _row, "Id");
										int _custom_property_datatype = GetIntValue(ref _row, "DataType");
										string _custom_property_enum = GetValue(ref _row, "Enumeration");

										string _group_name = string.Empty;

										DataRow _asset_type_row = AssetCategories.SelectAssetType(_import_table.OrganizationId, _import_table.GetDepartmentId(), asset_type);
										if (_asset_type_row != null)
										{
											int _category_id = Int32.Parse(_asset_type_row["CategoryId"].ToString());
											DataRow _asset_category = AssetCategories.GetAssetCategory(_import_table.OrganizationId, _import_table.GetDepartmentId(), _category_id);
											if (_asset_category != null)
											{
												_group_name = _group_name + _asset_category["Name"].ToString() + " \\ ";
											};

											_group_name = _group_name + _asset_type_row["Name"].ToString();
										};

										_import_table.GetImportCustomProperty().Add(_custom_property_id, asset_type, _custom_column.ColumnName, _custom_property_datatype, _custom_property_enum, custom_value, _group_name);
									};
								};

								data_row[_custom_column.ColumnName] = _custom_valid_value;
							};
						};
					};
				};

				return result;
			}

			private bool ValidateMain(ref DataRow data_row)
			{
				bool result = true;

				if (data_row != null)
				{
					ImportColumns _available = _import_table.GetAvailableColumnList();
					IDataReaderAdapter _reader = _import_table.GetReader();
					if ((_available != null) && (_reader != null))
					{
						int _column_count = _available.GetColumnCount();
						for (int _index = 0; _index < _column_count; _index++)
						{
							ImportColumn _available_column = _available.GetColumn(_index);
							if (_available_column != null)
							{
								string value = _reader.GetValue(_available_column.ColumnIndex);
								string _error_desc = string.Empty;
								string _valid_value = CheckValue(ref data_row, ref _available_column, value, ref _error_desc);
								if (_valid_value.Length > 0)
								{
									data_row[_available_column.ColumnName] = _valid_value;
								}
							}
						}

						string _asset_serial = GetAssetValue("SerialNumber");
						Guid? guid = null;
						try
						{
							guid = new Guid(GetAssetValue("AssetGUID"));
						}
						catch { }

						ImportLog _log = _import_table.GetLog();
						bool updateUniqueFields = false;

						int assetId = Data.Assets.SelectAssetId(_import_table.OrganizationId, _import_table.GetDepartmentId(), guid, GetAssetValue("SerialNumber"), GetAssetValue("Unique1"), GetAssetValue("Unique2"), GetAssetValue("Unique3"), GetAssetValue("Unique4"), GetAssetValue("Unique5"), GetAssetValue("Unique6"), GetAssetValue("Unique7"), out updateUniqueFields);
						data_row["AssetId"] = AnalizeAssetBaseId(ref data_row, assetId, 0, 0, "SerialNumber", false, false);
						data_row["UpdateUniqueFields"] = updateUniqueFields;
						if (guid != null && guid != Guid.Empty)
							data_row["AssetGUID"] = guid;
						else
							data_row["AssetGUID"] = DBNull.Value;

						int _asset_category_id = Data.Assets.SelectCategoryId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("AssetCategoryName"));
						data_row["AssetCategoryId"] = AnalizeAssetBaseId(ref data_row, _asset_category_id, 0, 0, "AssetCategoryName", true, false);
						int __asset_category_id = _asset_category_id;

						DataRow _null_row = null;

						if (_asset_category_id != -1)
						{
							int _asset_type_id = Data.Assets.SelectTypeId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("AssetTypeName"), ref __asset_category_id);
							data_row["AssetTypeId"] = AnalizeAssetBaseId(ref data_row, _asset_type_id, _asset_category_id, __asset_category_id, "AssetTypeName", true, false);
							int __asset_type_id = _asset_type_id;

							if (_asset_type_id != -1)
							{
								int _asset_make_id = Data.Assets.SelectMakeId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("AssetMakeName"), ref __asset_type_id);
								data_row["AssetMakeId"] = AnalizeAssetBaseId(ref data_row, _asset_make_id, _asset_type_id, __asset_type_id, "AssetMakeName", true, false);
								int __asset_make_id = _asset_make_id;

								if (_asset_make_id != -1)
								{
									int _asset_model_id = Data.Assets.SelectModelId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("AssetModelName"), ref __asset_make_id);
									data_row["AssetModelId"] = AnalizeAssetBaseId(ref data_row, _asset_model_id, _asset_make_id, __asset_make_id, "AssetModelName", true, false);
									if (_asset_model_id == -1)
									{
										if (_log != null)
											_log.Add(ref _null_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), "AssetModel", GetAssetValue("AssetModelName"), "Value not related in current database.");
									}
								}
								else
								{
									if (_log != null)
										_log.Add(ref _null_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), "AssetMake", GetAssetValue("AssetMakeName"), "Value not related in current database.");
								}
							}
							else
							{
								if (_log != null)
									_log.Add(ref _null_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), "AssetType", GetAssetValue("AssetTypeName"), "Value not related in current database.");
							}
						}
						else
						{
							if (_log != null)
								_log.Add(ref _null_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), "AssetCategory", GetAssetValue("AssetCategoryName"), "Value not related in current database.");
						}

						int _asset_status_id = Data.Assets.SelectStatusId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("StatusName"));
						data_row["StatusId"] = AnalizeAssetBaseId(ref data_row, _asset_status_id, 0, 0, "StatusName", true, true);

						int accountId = Accounts.GetAccountIdByName(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("AccountName"));
						data_row["AccountId"] = accountId;

						int _location_id = GetAssetLocationId(_import_table.OrganizationId, _import_table.GetDepartmentId(), accountId, GetAssetValue("LocationName"));
						data_row["LocationId"] = AnalizeAssetBaseId(ref data_row, _location_id, 0, 0, "LocationName", false, true);

						int _asset_owner_user_id = Data.Assets.SelectUserId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("OwnerMail"));
						data_row["OwnerId"] = AnalizeAssetBaseId(ref data_row, _asset_owner_user_id, 0, 0, "OwnerMail", true, true);
						int _asset_checkout_user_id = Data.Assets.SelectUserId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("CheckoutMail"));
						data_row["CheckedOutId"] = AnalizeAssetBaseId(ref data_row, _asset_checkout_user_id, 0, 0, "CheckoutMail", true, true);

						int _asset_vendor_id = Data.Assets.SelectVendorId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("VendorName"));
						data_row["VendorId"] = AnalizeAssetBaseId(ref data_row, _asset_vendor_id, 0, 0, "VendorName", false, false);

						int _asset_warranty_vendor_id = Data.Assets.SelectVendorId(_import_table.OrganizationId, _import_table.GetDepartmentId(), GetAssetValue("WarrantyVendorName"));
						data_row["WarrantyVendorId"] = AnalizeAssetBaseId(ref data_row, _asset_warranty_vendor_id, 0, 0, "WarrantyVendorName", false, false);
					};
				};

				return result;
			}

			private int AnalizeAssetBaseId(ref DataRow data_row, int id, int first, int second, string name, bool is_require, bool is_warning)
			{
				int result = -1;

				if (_import_table != null)
				{
					ImportLog _log = _import_table.GetLog();

					if (id == -2)
					{
						if (_log != null)
							_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), name, GetAssetValue(name), "Value ambiguous in current database.");

						result = 0;
					};

					if (id > 0)
						result = id;

					if (is_warning)
					{
						if (id == -1)
						{
							if (!is_require)
							{
								if (GetAssetValue(name).Length > 0)
									_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), name, GetAssetValue(name), "Value not found in current database.");
							}
							else
								_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), name, GetAssetValue(name), "Value not found in current database.");
						};
					};

					if (first != second)
					{
						if (first != -1)
						{
							if (_log != null)
								_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), name, GetAssetValue(name), "Value not related in current database.");
						}
						else
						{
							if (_log != null)
								_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), name, GetAssetValue(name), "Value not related in current database, need create parent object before.");
						};

						result = 0;
					};
				};

				return result;
			}

			private string GetAssetValue(string column_name)
			{
				string result = "";
				if (_import_table != null)
				{
					ImportColumns _available = _import_table.GetAvailableColumnList();
					IDataReaderAdapter _reader = _import_table.GetReader();
					if ((_available != null) && (_reader != null))
					{
						ImportColumn _asset_value = _available.GetColumn(column_name);
						if (_asset_value != null)
							result = _reader.GetValue(_asset_value.ColumnIndex);
					}
				}

				return result;
			}

			private string GetAssetCustomValue(string column_name)
			{
				string result = "";
				if (_import_table != null)
				{
					ImportColumns _custom = _import_table.GetCustomColumnList();
					IDataReaderAdapter _reader = _import_table.GetReader();
					if ((_custom != null) && (_reader != null))
					{
						ImportColumn _asset_value = _custom.GetColumn(column_name);
						if (_asset_value != null)
							result = _reader.GetValue(_asset_value.ColumnIndex);
					}
				}

				return result;
			}

			private string GetAssetPath()
			{
				string result = "";
				string _tmp = "";
				_tmp = GetAssetValue("AssetCategoryName");
				if (_tmp.Length > 0)
				{
					result += _tmp;
					result += " \\ ";
				};

				_tmp = GetAssetValue("AssetTypeName");
				if (_tmp.Length > 0)
				{
					result += _tmp;
					result += " \\ ";
				};

				_tmp = GetAssetValue("AssetMakeName");
				if (_tmp.Length > 0)
				{
					result += _tmp;
					result += " \\ ";
				};

				_tmp = GetAssetValue("AssetModelName");
				if (_tmp.Length > 0)
					result += _tmp;

				return result;
			}

			private string CheckValue(ref DataRow data_row, ref ImportColumn import_column, string value, ref string error_desc)
			{
				error_desc = string.Empty;

				string result = "";
				if (_import_table != null)
				{
					ImportLog _log = _import_table.GetLog();
					if ((import_column != null) && (_log != null))
					{
						if ((value.Length == 0) && (import_column.IsColumnRequire))
						{
							_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, "", "Value can't be empty.");
						}
						else
						{
							string _system_type = import_column.ColumnSystemType;

							if (value.Length > 0)
							{
								switch (_system_type)
								{
									case "System.Int16":
										try
										{
											Int16 _int16_value = Int16.Parse(value);
											result = value;
										}
										catch
										{
											if (import_column.IsColumnRequire)
												_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Required value can't convert to integer(Int16) value.");
											else
												_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value can't convert to integer(Int16) value.");
										};
										break;
									case "System.Int32":
										try
										{
											Int32 _int32_value = Int32.Parse(value);
											result = value;
										}
										catch
										{
											if (import_column.IsColumnRequire)
												_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Required value can't convert to integer(Int32) value.");
											else
												_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value can't convert to integer(Int32) value.");
										};
										break;
									case "System.Byte":
										try
										{
											Byte _int8_value = Byte.Parse(value);
											result = value;
										}
										catch
										{
											if (import_column.IsColumnRequire)
												_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Required value can't convert to integer(Int8) value.");
											else
												_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value can't convert to integer(Int8) value.");
										};
										break;
									case "System.Decimal":
										try
										{
											Decimal _decimal_value = Decimal.Parse(value);
											result = value;
										}
										catch
										{
											if (import_column.IsColumnRequire)
												_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Required value can't convert to decimal value.");
											else
												_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value can't convert to decimal value.");
										};
										break;
									case "System.String":
										result = value;
										break;
									case "System.Enum":
										result = value;
										if (value.Length > 0)
										{
											string _enums = import_column.ColumnEnumeration;
											if (_enums.Length > 0)
											{
												AssetTypeProperty _custom_property = new AssetTypeProperty();
												if (_custom_property != null)
												{
													_custom_property.Enumeration = _enums;
													string[] _array = _custom_property.EnumerationArray;
													if (_array != null)
													{
														bool _is_exist = false;
														for (int _array_index = 0; _array_index < _array.Length; _array_index++)
														{
															if (value == _array[_array_index])
															{
																_is_exist = true;
																break;
															};
														};

														if (!_is_exist)
														{
															error_desc = "value not exist in enumeration set.";
															result = "";
															//Now, supported auto creation undefined enum values.
															//_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value not exist in enumeration set.");
														};
													};
												};
											};
										};
										break;
									case "System.DateTime":
										try
										{
											DateTime _datetime_value = DateTime.Parse(value);
											result = Functions.FormatSQLShortDateTime(_datetime_value);
											//result = value;
										}
										catch
										{
											if (import_column.IsColumnRequire)
												_log.Add(ref data_row, ImportLog.ErrorType.Error, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Required value can't convert to datetime value.");
											else
												_log.Add(ref data_row, ImportLog.ErrorType.Warning, _import_table.GetSheetName(), GetAssetPath(), GetAssetValue("SerialNumber"), import_column.ColumnName, value, "Value can't convert to datetime value.");
										};
										break;
								};
							};
						};
					};
				};

				return result;
			}
		}

		public interface IDataReaderAdapter
		{
			int ColumnCount();
			bool ReadLine();
			string GetValue(int index);
		}

		public class ApiDataReaderAdapter : IDataReaderAdapter
		{
			private DataTable dtDataSource;
			private int _currentRowIndex;

			public ApiDataReaderAdapter(ref DataTable dtDataSource)
			{
				this.dtDataSource = dtDataSource;

				DataRow drCaptions = this.dtDataSource.NewRow();
				List<string> list = new List<string>();
				foreach (DataColumn dcColumn in this.dtDataSource.Columns)
					list.Add(dcColumn.ColumnName);
				drCaptions.ItemArray = list.ToArray();
				this.dtDataSource.Rows.InsertAt(drCaptions, 0);
				_currentRowIndex = -1;
			}

			public int ColumnCount()
			{
				return dtDataSource != null ? dtDataSource.Columns.Count : 0;
			}

			public bool ReadLine()
			{
				bool returnValue = dtDataSource != null && _currentRowIndex < dtDataSource.Rows.Count - 1;
				if (returnValue)
					_currentRowIndex++;
				return returnValue;
			}

			public string GetValue(int columnIndex)
			{
				string returnValue = string.Empty;

				if (dtDataSource != null)
				{
					try
					{
						if (!dtDataSource.Rows[_currentRowIndex].IsNull(columnIndex))
							returnValue = dtDataSource.Rows[_currentRowIndex][columnIndex].ToString().Trim();
					}
					catch
					{
					}
				}
				return returnValue;
			}
		}


		public class ExcelDataReaderAdapter : IDataReaderAdapter
		{
			private OleDbDataReader _r = null;

			public ExcelDataReaderAdapter(ref OleDbDataReader oledbReader)
			{
				_r = oledbReader;
			}

			public int ColumnCount()
			{
				int result = 0;
				if (_r != null)
					result = _r.FieldCount;
				return result;
			}

			public bool ReadLine()
			{
				bool result = false;

				if (_r != null)
					result = _r.Read();

				return result;
			}

			public string GetValue(int index)
			{
				string result = "";

				if (_r != null)
				{
					try
					{
						if (!_r.IsDBNull(index))
							result = _r.GetValue(index).ToString().Trim();
					}
					catch { }
				}
				return result;
			}
		}

		public class ImportTable
		{
			private Guid _organizationId = Guid.Empty;
			private int _departmentId = 0;

			private string _sheet_name = "";
			private IDataReaderAdapter _reader = null;

			private ImportLog _log = null;

			private ImportCustomProperty _import_custom_property = null;

			private DataTable _etalon_table = null;
			private DataTable _table = null;

			private ImportColumns _require_column_list = null;
			private ImportColumns _require_column_list_all = null;
			private ImportColumns _available_column_list = null;
			private ImportColumns _custom_column_list = null;

			public ImportTable(string sheet_name, Guid organizationId,  int departmentId, ref ImportLog import_log, ref ImportCustomProperty import_custom_property, IDataReaderAdapter dataReaderAdapter)
			{
				_organizationId = organizationId;
				_departmentId = departmentId;

				_require_column_list = new ImportColumns();
				_require_column_list_all = new ImportColumns();
				_available_column_list = new ImportColumns();
				_custom_column_list = new ImportColumns();

				_reader = dataReaderAdapter;
				_log = import_log;

				_import_custom_property = import_custom_property;

				_sheet_name = sheet_name;

				_table = new DataTable();
			}

			public Guid OrganizationId { get { return _organizationId; } }

			public int GetDepartmentId()
			{
				return _departmentId;
			}

			public ImportColumns GetRequiredColumnList()
			{
				return _require_column_list;
			}

			public ImportColumns GetAllRequiredColumnList()
			{
				return _require_column_list_all;
			}

			public ImportColumns GetAvailableColumnList()
			{
				return _available_column_list;
			}

			public ImportColumns GetCustomColumnList()
			{
				return _custom_column_list;
			}

			public DataTable GetDataTable()
			{
				return _table;
			}

			public IDataReaderAdapter GetReader()
			{
				return _reader;
			}

			public string GetSheetName()
			{
				return _sheet_name;
			}

			public ImportLog GetLog()
			{
				return _log;
			}

			public ImportCustomProperty GetImportCustomProperty()
			{
				return _import_custom_property;
			}

			public bool Validate(Config cfg)
			{
				if (_reader != null)
				{
					_etalon_table = Data.Assets.SelectAssetsByFilterForExport(_organizationId, _departmentId, null, null, null, cfg);
					_table.Columns.Add(new DataColumn("UpdateUniqueFields", typeof(bool)));

					if (_etalon_table != null)
					{
						_require_column_list.CreateColumns(ref _etalon_table, "Id");
						_require_column_list_all.CreateColumns(ref _etalon_table, "");
						_require_column_list.CreateColumnsInTable(ref _etalon_table, ref _table, "");

						string[] requiredColumnNames = {
													 "AssetCategoryName", "AssetTypeName", "AssetMakeName",
													 "AssetModelName", "StatusName", "OwnerMail", "CheckoutMail"
												 };

						foreach (string requiredColumnName in requiredColumnNames)
						{
							ImportColumn requiredColumn = _require_column_list.GetColumn(requiredColumnName);
							if (requiredColumn != null)
								requiredColumn.IsColumnRequire = true;
						}

						bool isContinue = true;
						if (_reader.ReadLine())
						{
							string columnValue = "";
							int _column_count = _reader.ColumnCount();
							for (int _index = 0; _index < _column_count; _index++)
							{
								columnValue = _reader.GetValue(_index);

								if (columnValue.ToLower() == "dateacquired")
									columnValue = "DateAquired";

								if (columnValue.Length > 0)
								{
									string _system_type = "System.String";
									bool _is_require = false;

									ImportColumn _asset_column = _require_column_list.GetColumn(columnValue);
									if (_asset_column != null)
									{
										_system_type = _asset_column.ColumnSystemType;
										_is_require = _asset_column.IsColumnRequire;
										_available_column_list.AddColumn(_index, columnValue, _system_type, _is_require);
									}
									else
									{
										_custom_column_list.AddColumn(_index, columnValue, _system_type, _is_require);

										if (_table != null)
										{
											DataColumn data_id = new DataColumn();
											if (data_id != null)
											{
												data_id.DataType = System.Type.GetType("System.Int32");
												data_id.AllowDBNull = false;
												data_id.Caption = columnValue + "Id";
												data_id.ColumnName = columnValue + "Id";
												data_id.DefaultValue = 0;

												// Add the column to the table. 
												_table.Columns.Add(data_id);
											};

											DataColumn data_column = new DataColumn();
											if (data_column != null)
											{
												data_column.DataType = System.Type.GetType(_system_type);
												data_column.AllowDBNull = false;
												data_column.Caption = columnValue;
												data_column.ColumnName = columnValue;
												data_column.DefaultValue = "";

												// Add the column to the table. 
												_table.Columns.Add(data_column);
											}
										}
									}
								}
							}

							//Check columns
							if (_available_column_list.GetColumnCount() != _require_column_list.GetColumnCount())
							{
								int _require_size = _require_column_list.GetColumnCount();
								for (int _require_index = 0; _require_index < _require_size; _require_index++)
								{
									ImportColumn _require_column = _require_column_list.GetColumn(_require_index);
									if (_require_column != null)
									{
										string _require_column_name = _require_column.ColumnName;
										bool _require_is_column = _require_column.IsColumnRequire;

										if (_require_column_name.Length > 0)
										{
											ImportColumn _available_column = _available_column_list.GetColumn(_require_column_name);
											if ((_available_column == null) && (_log != null))
											{
												ImportLog.ErrorType _error_type = ImportLog.ErrorType.Info;
												DataRow _empty = null;
												if (_require_is_column)
												{
													isContinue = false;
													_error_type = ImportLog.ErrorType.Critical;
													_log.Add(ref _empty, _error_type, _sheet_name, "", "", _require_column_name, "", "This column is require for import assets. Please, create and fill this column and retry import again.");
												}
												else
												{
													_error_type = ImportLog.ErrorType.Warning;
													_log.Add(ref _empty, _error_type, _sheet_name, "", "", _require_column_name, "", "This column undefined and will skip by import assets.");
												}
											}
										}
									}
								}
							}
						}

						if (isContinue)
						{
							ImportRows _import_rows = new ImportRows(this);
							if (_import_rows != null)
							{
								_import_rows.Validate();
							}
						}
					}
				}

				return true;
			}

			public bool Validate()
			{
				return Validate(null);
			}
		}

		public class ImportTables
		{
			private SortedList _tables;

			public ImportTables()
			{
				_tables = new SortedList();
			}

			public void Clear()
			{
				if (_tables != null)
					_tables.Clear();
			}

			public bool AddTable(Guid organizationId, int departmentId, string tableName, ref ImportLog importLog, ref ImportCustomProperty importCustomProperty, IDataReaderAdapter tableReader)
			{
				bool result = false;

				if (_tables != null)
				{
					if (!_tables.Contains(tableName))
					{
						ImportTable _table = new ImportTable(tableName, organizationId, departmentId, ref importLog, ref importCustomProperty, tableReader);
						if (_table != null)
						{
							_tables.Add(tableName, _table);
							result = true;
						}
					}
				}

				return result;
			}

			public int GetTableCount()
			{
				int result = 0;

				if (_tables != null)
				{
					result = _tables.Count;
				}

				return result;
			}

			public ImportTable GetTable(int index)
			{
				ImportTable result = null;

				if (_tables != null)
				{
					if ((index >= 0) && (index < GetTableCount()))
					{
						result = (ImportTable)_tables[_tables.GetKey(index).ToString()];
					}
				}

				return result;
			}

			public ImportTable GetTable(string table_name)
			{
				ImportTable result = null;

				if (_tables != null)
				{
					if (_tables.Contains(table_name))
					{
						result = (ImportTable)_tables[(object)table_name];
					}
				}

				return result;
			}

			public bool IsTableExist(string table_name)
			{
				bool result = false;

				if (_tables != null)
				{
					if (_tables.Contains(table_name))
					{
						result = true;
					}
				}

				return result;
			}

			public bool Remove(string table_name)
			{
				bool result = false;

				if (_tables != null)
				{
					if (_tables.Contains(table_name))
					{
						_tables.Remove(table_name);
						result = true;
					}
				}

				return result;
			}

		};

		public class ImportAssets
		{
			private Guid _organizationId = Guid.Empty;
			private int _departmentId = 0;

			private string _file_name = "";
			private OleDbConnection _oledb = null;

			private ImportLog import_log = null;

			private ImportTables _importTables = null;

			public ImportAssets(Guid organizationId, int departmentId)
			{
				_organizationId = organizationId;
				_departmentId = departmentId;
				_file_name = string.Empty;
				_oledb = null;
				_importTables = new ImportTables();
				import_log = new ImportLog();
			}

			public ImportAssets(Guid organizationId, int departmentId, string fileName)
				: this(organizationId, departmentId)
			{
				_file_name = fileName;

				_oledb = null;

				if (File.Exists(_file_name))
					_oledb = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _file_name + ";Extended Properties=\"Excel 12.0;HDR=No;IMEX=1\"");

				_importTables = new ImportTables();
				import_log = new ImportLog();

			}

			public bool IsHaveCriticalErrors()
			{
				bool result = false;

				if (import_log != null)
					result = import_log.IsHaveCriticalErrors();

				return result;
			}

			public DataTable GetImportLog()
			{
				DataTable result = null;

				if (import_log != null)
					return import_log.GetLog();

				return result;
			}

			public string Validate(ref ImportCustomProperty _import_custom_property, DataTable dtImportAssets)
			{
				string result = "";

				try
				{
					_importTables.AddTable(_organizationId, _departmentId, dtImportAssets.TableName, ref import_log, ref _import_custom_property, new ApiDataReaderAdapter(ref dtImportAssets));

					ImportTable importTable = _importTables.GetTable(dtImportAssets.TableName);
					if (importTable != null)
						importTable.Validate();
				}
				catch (Exception ex)
				{
					result = "Error Description: " + ex.Message.ToString();
				}
				return result;
			}

			public string Validate(ref ImportCustomProperty _import_custom_property, Config cfg)
			{
				string result = "";

				try
				{
					if (_oledb != null)
					{
						_oledb.Open();
						DataTable schemaTable = _oledb.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

						foreach (DataRow drTable in schemaTable.Rows)
						{
							string tableName = drTable["Table_Name"].ToString();

							if (!tableName.EndsWith("$")) //Skip filtering and sorting worksheets
								continue;

							OleDbCommand oleDbCommand = new OleDbCommand("SELECT * FROM [" + tableName + "]", _oledb);
							OleDbDataReader oleDbDataReader = oleDbCommand.ExecuteReader();

							if (!string.IsNullOrEmpty(tableName))
								tableName = tableName.Substring(0, tableName.Length - 1);

							_importTables.AddTable(_organizationId, _departmentId, tableName, ref import_log, ref _import_custom_property, new ExcelDataReaderAdapter(ref oleDbDataReader));

							ImportTable importTable = _importTables.GetTable(tableName);
							if (importTable != null)
								importTable.Validate(cfg);
						}
					}
				}
				catch (Exception ex)
				{
					result = "Can't process file: " + _file_name + " Error Description: " + ex.Message.ToString();
					if (_oledb != null)
					{
						if (_oledb.State == ConnectionState.Open) _oledb.Close();
						_oledb = null;
					}
				}

				if (_oledb != null)
				{
					if (_oledb.State == ConnectionState.Open) _oledb.Close();
				}

				return result;
			}

			private DataTable AppendData(ref DataTable _destination, ref DataTable _source, ref ImportTable source)
			{
				if ((_destination == null) && (source != null))
				{
					_destination = new DataTable();

					if (_destination != null)
					{
						ImportColumns _require_columns = source.GetAllRequiredColumnList();
						if (_require_columns != null)
						{
							int _require_columns_count = _require_columns.GetColumnCount();

							for (int index = 0; index < _require_columns_count; index++)
							{
								ImportColumn _require_column = _require_columns.GetColumn(index);

								if (_require_column != null)
								{
									string _system_type = _require_column.ColumnSystemType;
									string _column_name = _require_column.ColumnName;
									DataColumn data_column = new DataColumn();
									if (data_column != null)
									{
										data_column.DataType = System.Type.GetType(_system_type);
										data_column.AllowDBNull = true;
										data_column.Caption = _column_name;
										data_column.ColumnName = _column_name;

										_destination.Columns.Add(data_column);
									}
								}
							}
						}
					}
				}

				if ((source != null) && (_destination != null))
				{
					ImportColumns _row_require_columns = source.GetRequiredColumnList();
					if (_row_require_columns != null)
					{
						int _row_require_columns_count = _row_require_columns.GetColumnCount();

						if (_source != null)
						{
							foreach (DataRow _row in _source.Rows)
							{
								DataRow _new_row = _destination.NewRow();
								for (int index = 0; index < _row_require_columns_count; index++)
								{
									ImportColumn _row_require_column = _row_require_columns.GetColumn(index);
									if (_row_require_column != null)
										_new_row[_row_require_column.ColumnName] = _row[_row_require_column.ColumnName];
								}
								_destination.Rows.Add(_new_row);
							}
						}
					}
				}

				return _destination;
			}

			private DataTable AppendVendorData(ref DataTable _destination, ref DataTable _source, ref ImportTable source)
			{
				if ((_destination == null) && (source != null))
				{
					_destination = new DataTable();

					if (_destination != null)
					{
						string _system_type = "System.String";
						string _column_name = "AssetVendorName";
						DataColumn data_column = new DataColumn();
						if (data_column != null)
						{
							data_column.DataType = System.Type.GetType(_system_type);
							data_column.AllowDBNull = false;
							data_column.Caption = _column_name;
							data_column.ColumnName = _column_name;
							data_column.DefaultValue = "";
							_destination.Columns.Add(data_column);
						}
					}
				}

				if ((source != null) && (_destination != null))
				{
					if (_source != null)
					{
						foreach (DataRow _row in _source.Rows)
						{
							if (_row != null)
							{
								string _vendor_name = "";
								string _warranty_vendor_name = "";

								string _vendor_id = _row["VendorId"].ToString();
								string _warranty_vendor_id = _row["WarrantyVendorId"].ToString();

								if (_row["VendorName"] != null)
									_vendor_name = _row["VendorName"].ToString();

								if (_row["WarrantyVendorName"] != null)
									_warranty_vendor_name = _row["WarrantyVendorName"].ToString();

								if ((_vendor_id == "-1") && (_vendor_name.Length > 0))
								{
									DataRow _new_row = _destination.NewRow();
									if (_new_row != null)
									{
										_new_row["AssetVendorName"] = _vendor_name;
										_destination.Rows.Add(_new_row);
									}
								}

								if ((_warranty_vendor_id == "-1") && (_warranty_vendor_name.Length > 0))
								{
									DataRow _new_row2 = _destination.NewRow();
									if (_new_row2 != null)
									{
										_new_row2["AssetVendorName"] = _warranty_vendor_name;
										_destination.Rows.Add(_new_row2);
									}
								}
							}
						}
					}
				}

				return _destination;
			}

			DataTable RemoveDuplicateTypes(ref DataTable table)
			{
				if (table != null)
				{
					string _asset_category = "";
					string _asset_type = "";
					string _asset_make = "";
					string _asset_model = "";

					string _old_asset_category = "";
					string _old_asset_type = "";
					string _old_asset_make = "";
					string _old_asset_model = "";

					for (int i = 0; i < table.Rows.Count; i++)
					{
						DataRow drAsset = table.Rows[i];
						if (drAsset != null)
						{
							_asset_category = drAsset["AssetCategoryName"].ToString();
							_asset_type = drAsset["AssetTypeName"].ToString();
							_asset_make = drAsset["AssetMakeName"].ToString();
							_asset_model = drAsset["AssetModelName"].ToString();

							if ((_asset_category == _old_asset_category) && (_asset_type == _old_asset_type) && (_asset_make == _old_asset_make) && (_asset_model == _old_asset_model))
							{
								table.Rows.Remove(drAsset);
								i = -1;
								_old_asset_category = "";
								_old_asset_type = "";
								_old_asset_make = "";
								_old_asset_model = "";
							}
							else
							{
								_old_asset_category = _asset_category;
								_old_asset_type = _asset_type;
								_old_asset_make = _asset_make;
								_old_asset_model = _asset_model;
							}
						}
					}
				}

				return table;
			}

			DataTable RemoveDuplicateVendors(ref DataTable table)
			{
				if (table != null)
				{
					string _asset_vendor = "";
					string _old_asset_vendor = "";

					for (int i = 0; i < table.Rows.Count; i++)
					{
						DataRow drAsset = table.Rows[i];
						if (drAsset != null)
						{
							_asset_vendor = drAsset["AssetVendorName"].ToString();
							if (_asset_vendor == _old_asset_vendor)
							{
								table.Rows.Remove(drAsset);
								i = -1;
								_old_asset_vendor = "";
							}
							else
							{
								_old_asset_vendor = _asset_vendor;
							}
						}
					}
				}

				return table;
			}

			public DataTable GetCommonNewAssetTypes(bool isInsertOnly)
			{
				DataTable result = null;

				if (_importTables != null)
				{
					int importTablesCount = _importTables.GetTableCount();

					for (int i = 0; i < importTablesCount; i++)
					{
						ImportTable importTable = _importTables.GetTable(i);
						if (importTable != null)
						{
							DataTable _table = importTable.GetDataTable();
							if (_table != null)
							{
								_table.TableName = "AssetGroup" + i.ToString();
								DataView view = new DataView();
								view.Table = _table;

								if (isInsertOnly)
									view.RowFilter = "GroupId=0 AND (AssetCategoryId=-1 OR AssetTypeId=-1 OR AssetMakeId=-1 OR AssetModelId=-1) AND AssetId=-1";
								else
									view.RowFilter = "GroupId=0 AND (AssetCategoryId=-1 OR AssetTypeId=-1 OR AssetMakeId=-1 OR AssetModelId=-1) AND (AssetId=-1 OR AssetId>0)";

								view.Sort = "AssetCategoryName ASC, AssetTypeName ASC, AssetMakeName ASC, AssetModelName ASC";

								DataTable _view_table = view.ToTable();
								result = AppendData(ref result, ref _view_table, ref importTable);
							}
						}
					}
				}

				DataView result_view = new DataView();

				if ((result != null) && (result_view != null))
				{
					result.TableName = "Assets";

					result_view.Table = result;
					result_view.Sort = "AssetCategoryName ASC, AssetTypeName ASC, AssetMakeName ASC, AssetModelName ASC";

					DataTable _result_view_table = result_view.ToTable();
					result = RemoveDuplicateTypes(ref _result_view_table);
				};

				return result;
			}

			public DataTable GetCommonNewAssetVendors(bool isInsertOnly)
			{
				DataTable result = null;

				if (_importTables != null)
				{
					int importTablesCount = _importTables.GetTableCount();

					for (int i = 0; i < importTablesCount; i++)
					{
						ImportTable importTable = _importTables.GetTable(i);
						if (importTable != null)
						{
							DataTable _table = importTable.GetDataTable();
							if (_table != null)
							{
								_table.TableName = "AssetGroup" + i.ToString();
								DataView view = new DataView();
								view.Table = _table;

								if (isInsertOnly)
									view.RowFilter = "GroupId=0 AND (VendorId=-1 OR WarrantyVendorId=-1) AND AssetId=-1";
								else
									view.RowFilter = "GroupId=0 AND (VendorId=-1 OR WarrantyVendorId=-1) AND (AssetId=-1 OR AssetId>0)";

								DataTable _view_table = view.ToTable();
								result = AppendVendorData(ref result, ref _view_table, ref importTable);
							}
						}
					}
				}

				DataView result_view = new DataView();

				if ((result != null) && (result_view != null))
				{
					result.TableName = "Vendors";

					result_view.Table = result;
					result_view.Sort = "AssetVendorName ASC";

					DataTable _result_view_table = result_view.ToTable();
					result = RemoveDuplicateVendors(ref _result_view_table);
				};

				return result;
			}

			private string GetValue(ref DataRow row, string field_name)
			{
				string result = "";

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							if (row[field_name].ToString().Length > 0)
								result = row[field_name].ToString();
						}
					}
				}

				return result;
			}

			private bool GetBoolValue(ref DataRow row, string fieldName)
			{
				bool result = false;


				if (row != null && row.Table.Columns.Contains(fieldName) && row[fieldName] != null && !string.IsNullOrEmpty(row[fieldName].ToString()))
				{
					try
					{
						result = bool.Parse(row[fieldName].ToString());
					}
					catch
					{ }
				}

				return result;
			}

			private Guid? GetNullableGuidValue(ref DataRow row, string fieldName)
			{
				Guid? result = null;


				if (row != null && row.Table.Columns.Contains(fieldName) && row[fieldName] != null && !string.IsNullOrEmpty(row[fieldName].ToString()))
				{
					try
					{
						result = new Guid(row[fieldName].ToString());
					}
					catch
					{ }
				}

				return result;
			}

			private int GetIntValue(ref DataRow row, string field_name)
			{
				int result = -1;

				if (row != null && row.Table.Columns.Contains(field_name) && !row.IsNull(field_name) && !string.IsNullOrEmpty(row[field_name].ToString()))
				{
					try
					{
						result = Int32.Parse(row[field_name].ToString());
					}
					catch { }
				}

				return result;
			}

			private DateTime? GetDateTimeValue(ref DataRow row, string fieldName)
			{
				DateTime? result = null;

				if (row != null && row.Table.Columns.Contains(fieldName) && !row.IsNull(fieldName) && !string.IsNullOrEmpty(row[fieldName].ToString()))
				{
					try
					{
						result = DateTime.Parse(row[fieldName].ToString());
					}
					catch
					{
					}
				}

				return result;
			}


			private double GetDoubleValue(ref DataRow row, string field_name)
			{
				double result = -1;

				if (row != null)
				{
					if (row.Table.Columns.Contains(field_name))
					{
						if (row[field_name] != null)
						{
							try
							{
								if (row[field_name].ToString().Length > 0)
								{
									Decimal _res = Decimal.Parse(row[field_name].ToString());
									result = Decimal.ToDouble(_res);
								}
							}
							catch
							{
								result = -1;
							}
						}
					}
				}

				return result;
			}

			private bool AddAsset(int? userId, ref DataRow row)
			{
				bool result = false;

				if (row != null)
				{
					int assetCategoryId = GetIntValue(ref row, "AssetCategoryId");
					int assetTypeId = GetIntValue(ref row, "AssetTypeId");
					int assetMakeId = GetIntValue(ref row, "AssetMakeId");
					int assetModelId = GetIntValue(ref row, "AssetModelId");

					Guid? guid = GetNullableGuidValue(ref row, "AssetGUID");

					if ((assetCategoryId != -1) && (assetTypeId != -1) && (assetMakeId != -1) && (assetModelId != -1))
					{
						int assetId = Asset.InsertAsset(_departmentId, userId, guid, GetValue(ref row, "SerialNumber"), assetCategoryId, assetTypeId, assetMakeId, assetModelId, GetValue(ref row, "Unique1"), GetValue(ref row, "Unique2"), GetValue(ref row, "Unique3"), GetValue(ref row, "Unique4"), GetValue(ref row, "Unique5"), GetValue(ref row, "Unique6"), GetValue(ref row, "Unique7"), GetValue(ref row, "MotherboardSerial"), GetValue(ref row, "BiosSerial"));

						if (assetId > 0)
						{
							result = true;
							row["AssetId"] = assetId;
						}
					}
				}

				return result;
			}

			private bool UpdateCustom(ref ImportTable importTable, ref DataRow dataRow)
			{
				bool result = false;

				if ((importTable != null) && (dataRow != null))
				{
					int assetId = GetIntValue(ref dataRow, "AssetId");

					if (assetId > 0)
					{
						ImportColumns customColumnList = importTable.GetCustomColumnList();
						int customColumnCount = customColumnList.GetColumnCount();
						for (int i = 0; i < customColumnCount; i++)
						{
							ImportColumn customColumn = customColumnList.GetColumn(i);
							if (customColumn != null)
							{
								string customColumnNameId = customColumn.ColumnName + "Id";
								int customPropertyId = GetIntValue(ref dataRow, customColumnNameId);
								string customValue = GetValue(ref dataRow, customColumn.ColumnName);

								if (customPropertyId > 0)
								{
									AssetTypeProperties.SetValue(_departmentId, assetId, customPropertyId, customValue);
									result = true;
								}
							}
						}
					}
				}

				return result;
			}

			private bool UpdateAsset(ref ImportTable importTable, int? userId, string userName, ref DataRow row)
			{
				bool result = false;

				if (row != null)
				{
					int AssetCategoryID = GetIntValue(ref row, "AssetCategoryId");
					int AssetTypeID = GetIntValue(ref row, "AssetTypeId");
					int AssetMakeID = GetIntValue(ref row, "AssetMakeId");
					int AssetModelID = GetIntValue(ref row, "AssetModelId");

					int DeptID = _departmentId;
					int? UserID = userId;
					int AssetID = GetIntValue(ref row, "AssetId");

					bool updateUniqeFields = GetBoolValue(ref row, "UpdateUniqueFields");

					int OldStatusID = GetIntValue(ref row, "StatusId");
					DataRow _dr = Asset.GetAsset(_departmentId, AssetID);
					if (_dr != null)
					{
						OldStatusID = GetIntValue(ref _dr, "StatusId");
					}

					int PurchaseVendorID = GetIntValue(ref row, "VendorId");
					int WarrantyVendorID = GetIntValue(ref row, "WarrantyVendorId");
					int AccountId = GetIntValue(ref row, "AccountId");
					int LocationID = GetIntValue(ref row, "LocationId");
					string AssetName = GetValue(ref row, "AssetName");
					string AssetDescription = GetValue(ref row, "AssetDescription");
					double AssetValue = GetDoubleValue(ref row, "Value");
					double ValueCurrent = GetDoubleValue(ref row, "ValueCurrent");
					double ValueReplacement = GetDoubleValue(ref row, "ValueReplacement");
					double ValueDepreciated = GetDoubleValue(ref row, "ValueDepreciated");
					double ValueSalvage = GetDoubleValue(ref row, "ValueSalvage");
					double DisposalCost = GetDoubleValue(ref row, "DisposalCost");
					int AssetSort = GetIntValue(ref row, "AssetSort");
					string FundingSource = GetValue(ref row, "FundingSource");

					DateTime? DateAcquired = null;

					if (row.Table.Columns.Contains("DateAquired"))
						DateAcquired = GetDateTimeValue(ref row, "DateAquired");
					else
					{
						if (row.Table.Columns.Contains("DateAcquired"))
							DateAcquired = GetDateTimeValue(ref row, "DateAcquired");
					}

					DateTime? DatePurchased = GetDateTimeValue(ref row, "DatePurchased");
					DateTime? DateDeployed = GetDateTimeValue(ref row, "DateDeployed");
					DateTime? DateOutOfService = GetDateTimeValue(ref row, "DateOutOfService");
					DateTime? DateEntered = GetDateTimeValue(ref row, "DateEntered");
					DateTime? DateReceived = GetDateTimeValue(ref row, "DateReceived");
					DateTime? DateDisposed = GetDateTimeValue(ref row, "DateDisposed");
					int WarrantyLabor = GetIntValue(ref row, "LaborWarrantyLength");
					int WarrantyPart = GetIntValue(ref row, "PartsWarrantyLength");
					string PONumber = GetValue(ref row, "PONumber");
					string FindingCode = GetValue(ref row, "FundingCode");
					string SerialNumber = GetValue(ref row, "SerialNumber");
					string Unique1 = GetValue(ref row, "Unique1");
					string Unique2 = GetValue(ref row, "Unique2");
					string Unique3 = GetValue(ref row, "Unique3");
					string Unique4 = GetValue(ref row, "Unique4");
					string Unique5 = GetValue(ref row, "Unique5");
					string Unique6 = GetValue(ref row, "Unique6");
					string Unique7 = GetValue(ref row, "Unique7");
					int NewStatusID = GetIntValue(ref row, "StatusId");

					bool isValidDateAquired = false;

					if (row.Table.Columns.Contains("DateAquired"))
						isValidDateAquired = (GetDateTimeValue(ref row, "DateAquired") != DateTime.MinValue);
					else if (row.Table.Columns.Contains("DateAcquired"))
						isValidDateAquired = (GetDateTimeValue(ref row, "DateAcquired") != DateTime.MinValue);

					int updateResult = Asset.UpdateAsset(
									  DeptID,
									  UserID,
									  AssetID,
									  PurchaseVendorID,
									  WarrantyVendorID,
									  AccountId,
									  LocationID,
									  AssetName,
									  AssetDescription,
									  AssetValue,
									  ValueCurrent,
									  ValueReplacement,
									  ValueDepreciated,
									  ValueSalvage,
									  DisposalCost,
									  AssetSort,
									  FundingSource,
									  DateAcquired,
									  DatePurchased,
									  DateDeployed,
									  DateOutOfService,
									  DateEntered,
									  DateReceived,
									  DateDisposed,
									  WarrantyLabor,
									  WarrantyPart,
									  PONumber,
									  FindingCode,
									  SerialNumber,
									  Unique1,
									  Unique2,
									  Unique3,
									  Unique4,
									  Unique5,
									  Unique6,
									  Unique7,
									  NewStatusID,
									  OldStatusID,
									  updateUniqeFields
					);

					if (updateResult == 0)
					{

						int oldOwnerId = 0;
						int oldCheckoutId = 0;
						int ownerId = GetIntValue(ref row, "OwnerId");
						int checkoutId = GetIntValue(ref row, "CheckedOutId");

						if (_dr != null)
						{
							oldOwnerId = GetIntValue(ref _dr, "OwnerId");
							oldCheckoutId = GetIntValue(ref _dr, "CheckedOutId");
						}

						if ((ownerId > 0) && (ownerId != oldOwnerId))
							Asset.UpdateAssetOwner(_departmentId, ownerId, AssetID, userName);

						if ((checkoutId > 0) && (checkoutId != oldCheckoutId))
							Asset.UpdateAssetCheckout(_departmentId, checkoutId, AssetID, userName);

						if ((AssetID > 0) && (AssetCategoryID > 0) && (AssetTypeID > 0) && (AssetMakeID > 0) && (AssetModelID > 0))
							Asset.UpdateAssetTypeMakeModel(_departmentId, AssetID, AssetCategoryID, AssetTypeID, AssetMakeID, AssetModelID);

						string notes = GetValue(ref row, "Notes");
						Asset.UpdateAssetNotes(_departmentId, AssetID, notes);

						UpdateCustom(ref importTable, ref row);

						result = true;
					}
				}

				return result;
			}

			private void ImportData(ref ImportTable importTable, int? userId, string userName, ref DataTable table, ref int newObjectsCount, ref int updatedObjectsCount)
			{
				if (table != null)
				{
					for (int i = 0; i < table.Rows.Count; i++)
					{
						DataRow drAsset = table.Rows[i];
						if (drAsset != null)
						{
							int assetId = 0;
							int.TryParse(drAsset["AssetId"].ToString(), out assetId);
							if (assetId == -1)
							{
								if (AddAsset(userId, ref drAsset))
								{
									newObjectsCount++;
									UpdateAsset(ref importTable, userId, userName, ref drAsset);
								}
							}
							else if (assetId > 0)
							{
								if (UpdateAsset(ref importTable, userId, userName, ref drAsset))
									updatedObjectsCount++;
							}
						}
					}
				}
			}

			public void GetCommonNewAssets(int? userId, string userName, bool isInsertOnly, ref int insertedObjects, ref int updatedObjects)
			{
				if (_importTables != null)
				{
					int tableCount = _importTables.GetTableCount();

					for (int i = 0; i < tableCount; i++)
					{
						ImportTable importTable = _importTables.GetTable(i);
						if (importTable != null)
						{
							DataTable _table = importTable.GetDataTable();
							if (_table != null)
							{
								_table.TableName = "AssetGroup" + i.ToString();
								DataView view = new DataView();
								view.Table = _table;

								if (isInsertOnly)
									view.RowFilter = "GroupId=0 AND AssetId=-1";
								else
									view.RowFilter = "GroupId=0 AND (AssetId=-1 OR AssetId>0)";

								DataTable _view_table = view.ToTable();
								ImportData(ref importTable, userId, userName, ref _view_table, ref insertedObjects, ref updatedObjects);
							}
						}
					}
				}

				if (_oledb != null)
				{
					if (_oledb.State == ConnectionState.Open) _oledb.Close();
				}
			}

			static public void AddVendor(Guid organizationId, int departmentId, string vendorName)
			{
				vendorName = vendorName.Replace("&amp;", "&");

				if (vendorName.Length > 0)
				{
					int assetVendorId = Data.Assets.SelectVendorId(organizationId, departmentId, vendorName);
					if (assetVendorId == -1)
					{
						assetVendorId = Data.AssetCategories.UpdateAssetVendor(organizationId, departmentId, 0, -1, "", vendorName, "", "", "", "");
					}
				}
			}

			static public void AddType(Guid organizationId, int departmentId, string assetCategoryName, string assetTypeName, string assetMakeName, string assetModelName)
			{
				assetCategoryName = assetCategoryName.Replace("&amp;", "&");
				assetTypeName = assetTypeName.Replace("&amp;", "&");
				assetMakeName = assetMakeName.Replace("&amp;", "&");
				assetModelName = assetModelName.Replace("&amp;", "&");

				if ((assetCategoryName.Length > 0) && (assetTypeName.Length > 0) && (assetMakeName.Length > 0) && (assetModelName.Length > 0))
				{
					int assetCategoryId = Data.Assets.SelectCategoryId(organizationId, departmentId, assetCategoryName);
					if (assetCategoryId == -1)
					{
						//Add category
						assetCategoryId = Data.AssetCategories.AddAssetCategory(organizationId, departmentId, assetCategoryName);
					}

					int assetTypeId = Data.Assets.SelectTypeId(organizationId, departmentId, assetTypeName, ref assetCategoryId);
					if (assetTypeId == -1)
					{
						//Add type
						assetTypeId = Data.AssetCategories.AddAssetType(organizationId, departmentId, assetCategoryId, assetTypeName, null);
					}

					int assetMakeId = Data.Assets.SelectMakeId(organizationId, departmentId, assetMakeName, ref assetTypeId);
					if (assetMakeId == -1)
					{
						//Add make
						assetMakeId = Data.AssetCategories.AddAssetMake(organizationId, departmentId, assetTypeId, assetMakeName);
					}

					int assetModelId = Data.Assets.SelectModelId(organizationId, departmentId, assetModelName, ref assetMakeId);
					if (assetModelId == -1)
					{
						//Add model
						assetModelId = Data.AssetCategories.AddAssetModel(organizationId, departmentId, assetMakeId, assetModelName, "");
					}
				}
			}
		}

		public static class ImportedAssets
		{
			private static Hashtable _imports = new Hashtable();

			public static object GetResult(Guid id)
			{
				if (_imports.Contains(id)) return _imports[id];
				else return null;
			}

			public static void Add(Guid id, object value)
			{
				_imports[id] = value;
			}

			public static void Remove(Guid id)
			{
				_imports.Remove(id);
			}
		}
	}
}