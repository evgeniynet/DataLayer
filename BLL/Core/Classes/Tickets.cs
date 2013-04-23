using System;
using System.Web;
using System.Data;
using System.Collections;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for Tickets.
	/// </summary>
	
	public struct FileItem
	{
		public int ID;
		public string Name;
		public string ContentType;
		public int Size;
		public DateTime Updated;
		public byte[] Data;
		public FileItem(int id, string name, int size, DateTime updated)
		{
			ID = id;
			Name=name;
			Size = size;
			Updated = updated;
			Data = null;
			ContentType = GetContentType(name);
		}

		public FileItem(int id, string name, int size, DateTime updated, byte[] data)
		{
			ID = id;
			Name = name;
			Size = size;
			Updated = updated;
			Data = data;
			ContentType = GetContentType(name);
		}
		private static string GetContentType(string name)
		{
			if (name == null || name.Length == 0 || name.LastIndexOf('.') < 0) return "application/octet-stream";
			string _fext = name.Substring(name.LastIndexOf('.')).ToLower();
			if (name.IndexOf(".doc")>=0) return "application/msword";
			else if (name.IndexOf(".xls")>=0) return "application/x-msexcel";
			else if (name.IndexOf(".mdb")>=0) return "application/msaccess";
			else if (name.IndexOf(".pdf")>=0) return "application/pdf";
			else if (name.IndexOf(".rtf")>=0) return "application/rtf";
			else if (name.IndexOf(".ppt")>=0) return "application/x-mspowerpoint";
			else if (name.IndexOf(".pot")>=0) return "application/x-mspowerpoint";
			else if (name.IndexOf(".pps")>=0) return "application/x-mspowerpoint";
			else if (name.IndexOf(".ppz")>=0) return "application/x-mspowerpoint";
			else if (name.IndexOf(".zip")>=0) return "application/zip";
			else if (name.IndexOf(".wav")>=0) return "audio/x-wav";
			else if (name.IndexOf(".gif")>=0) return "image/gif";
			else if (name.IndexOf(".jpg")>=0) return "image/jpeg";
			else if (name.IndexOf(".tif")>=0) return "image/tiff";
			else if (name.IndexOf(".htm")>=0) return "text/html";
			else if (name.IndexOf(".html")>=0) return "text/html";
			else if (name.IndexOf(".txt")>=0) return "text/plain";
			else if (name.IndexOf(".xml")>=0) return "text/xml";
			else if (name.IndexOf(".mpeg")>=0) return "video/mpeg";
			else if (name.IndexOf(".mpe")>=0) return "video/mpeg";
			else if (name.IndexOf(".mpg")>=0) return "video/mpeg";
			else if (name.IndexOf(".mov")>=0) return "video/quicktime";
			else if (name.IndexOf(".avi")>=0) return "video/x-msvideo";
			else if (name.IndexOf(".dvi")>=0) return "application/x-dvi";
			else if (name.IndexOf(".ps")>=0) return "application/postscript";
			else if (name.IndexOf(".xpm")>=0) return "image/x-xpixmap";
			else if (name.IndexOf(".mp3")>=0) return "audio/mp3";
			else if (name.IndexOf(".au")>=0) return "audio/ulaw";
			else if (name.IndexOf(".mi")>=0) return "audio/midi";
			else if (name==".ra") return "audio/x-pn-realaudio";
			else if (name.IndexOf(".ram")>=0) return "audio/x-pn-realaudio";
			else if (name.IndexOf(".pdb")>=0) return "checmical/x-pdb";
			else if (name.IndexOf(".xyz")>=0) return "chemical/x-xyz";
			else if (name.IndexOf(".swf")>=0) return "application/x-shockwave-flash";
			else return "application/octet-stream";
		}
	}

	public class Tickets : DBAccess
	{
		public struct CreateTicketInfo
		{
			public int UserId, AccountId, AccountLocationId, AccountDeptLocationId, PrimaryLocationId, DeptAccCount, DeptLocationCount;
			public string AccountName, AccountLocationName, AccountDeptLocationName, UserFirstName, UserLastName, UserEmail;
			public bool HandledByCallCentre;

			public CreateTicketInfo(int UsrId, int AccId, int AccLocId, int AccDeptLocId, int PrimLocId, string AccName, string AccLocName, string AccDeptLocName, string UsrFirstName, string UsrLastName, string UsrEmail, bool HndCallCentre, int iDeptAccCount, int iDeptLocationCount)
			{
				UserId = UsrId;
				AccountId = AccId;
				AccountLocationId = AccLocId;
				AccountDeptLocationId = AccDeptLocId;
				PrimaryLocationId = PrimLocId;
				AccountName = AccName;
				AccountLocationName = AccLocName;
				AccountDeptLocationName = AccDeptLocName;
				UserFirstName = UsrFirstName;
				UserLastName = UsrLastName;
				UserEmail = UsrEmail;
				HandledByCallCentre = HndCallCentre;
				DeptAccCount = iDeptAccCount;
				DeptLocationCount = iDeptLocationCount;
			}
		}

		public enum TicketTimer 
		{
			DaysOldTimer=0, SLATimer=1
		}

		public static DataTable LookUpDepartmentsByTicketId(int TktID)
		{
			DataTable _res = SelectByQuery("SELECT 0 AS DbNumber, C.company_id, C.company_name FROM tbl_ticket T INNER JOIN tbl_company C ON C.company_id=T.company_id WHERE T.Id=" + TktID.ToString());
			//foreach (int _db in GetDbNumbers())
			//{
			//    if (_db == 0) continue;
			//    DataTable _dt = SelectByQuery("SELECT "+_db.ToString()+" AS DbNumber, C.company_id, C.company_name FROM tbl_ticket T INNER JOIN tbl_company C ON C.company_id=T.company_id WHERE T.Id=" + TktID.ToString(), _db);
			//    foreach (DataRow _row in _dt.Rows) _res.Rows.Add(_row.ItemArray);
			//}
			return _res;
		}

		public static CreateTicketInfo GetCreateTicketInfo(int DeptID, int UserID, int AccID)
		{
			return GetCreateTicketInfo(DeptID, UserID, AccID, Guid.Empty);
		}

		public static CreateTicketInfo GetCreateTicketInfo(int DeptID, int UserID, int AccID, Guid orgId)
		{
			SqlParameter _pAccId = new SqlParameter("@AcctId", SqlDbType.Int);
			_pAccId.Direction = ParameterDirection.InputOutput;
			if (AccID != 0 || AccID == -1) _pAccId.Value = AccID;
			SqlParameter _pAccName = new SqlParameter("@vchAcctName", SqlDbType.NVarChar, 100);
			_pAccName.Direction = ParameterDirection.Output;
			SqlParameter _pAccLocId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
			_pAccLocId.Direction = ParameterDirection.Output;
			SqlParameter _pAccLocName = new SqlParameter("@AccountLocationName", SqlDbType.NVarChar, 50);
			_pAccLocName.Direction = ParameterDirection.Output;
			SqlParameter _pUserDeptLocId = new SqlParameter("@UserDeptLocationId", SqlDbType.Int);
			_pUserDeptLocId.Direction = ParameterDirection.Output;
			SqlParameter _pAccDeptLocId = new SqlParameter("@AcctDeptLocationId", SqlDbType.Int);
			_pAccDeptLocId.Direction = ParameterDirection.Output;
			SqlParameter _pAccDeptLocName = new SqlParameter("@AcctDeptLocationName", SqlDbType.NVarChar, 50);
			_pAccDeptLocName.Direction = ParameterDirection.Output;
			SqlParameter _pUserFirstName = new SqlParameter("@vchUserFirstName", SqlDbType.NVarChar, 100);
			_pUserFirstName.Direction = ParameterDirection.Output;
			SqlParameter _pUserLastName = new SqlParameter("@vchUserLastName", SqlDbType.NVarChar, 100);
			_pUserLastName.Direction = ParameterDirection.Output;
			SqlParameter _pHandleCallCentre = new SqlParameter("@btCfgCCRep", SqlDbType.Bit);
			_pHandleCallCentre.Direction = ParameterDirection.Output;
			SqlParameter _pUserEmail = new SqlParameter("@vchUserEmail", SqlDbType.NVarChar, 100);
			_pUserEmail.Direction = ParameterDirection.Output;
			SqlParameter _pDeptAccCount = new SqlParameter("@DeptAccCount", SqlDbType.Int);
			_pDeptAccCount.Direction = ParameterDirection.Output;
			SqlParameter _pDeptLocationCount = new SqlParameter("@DeptLocationCount", SqlDbType.Int);
			_pDeptLocationCount.Direction = ParameterDirection.Output;
			UpdateData("sp_SelectAcctCreateTkt", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserID), _pAccId, _pAccName, _pAccLocId, _pAccLocName, _pUserDeptLocId, _pAccDeptLocId, _pAccDeptLocName, _pUserFirstName, _pUserLastName, _pHandleCallCentre, _pUserEmail, _pDeptAccCount, _pDeptLocationCount }, orgId);
			return new CreateTicketInfo(UserID, 
				_pAccId.Value!=DBNull.Value ? (AccID == -2 ? -2 : (int)_pAccId.Value) : 0,
                _pAccLocId.Value != DBNull.Value && AccID != -2 ? (int)_pAccLocId.Value : 0,
                _pAccDeptLocId.Value != DBNull.Value && AccID != -2 ? (int)_pAccDeptLocId.Value : 0, 
				_pUserDeptLocId.Value!=DBNull.Value ? (int)_pUserDeptLocId.Value : 0, 
				AccID == -2 ? "" : _pAccName.Value.ToString(),
                AccID == -2 ? "" : _pAccLocName.Value.ToString(),
                AccID == -2 ? "" : _pAccDeptLocName.Value.ToString(), 
				_pUserFirstName.Value.ToString(),
				_pUserLastName.Value.ToString(),
				_pUserEmail.Value.ToString(), 
				_pHandleCallCentre.Value!=DBNull.Value ? (bool)_pHandleCallCentre.Value : false,
				_pDeptAccCount.Value!=DBNull.Value ? (int)_pDeptAccCount.Value : 0, 
				_pDeptLocationCount.Value!=DBNull.Value ? (int)_pDeptLocationCount.Value : 0);
		}

		public static DataRow SelectOne(int DeptID, int TktID)
		{
			return SelectOne(Guid.Empty, DeptID, TktID);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID, string TktPseudoID)
		{
			SqlParameter _pTId = new SqlParameter("@TId", SqlDbType.Int);
			_pTId.Value = 0;
			return SelectRecord("sp_SelectTicketDetail", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pTId, new SqlParameter("@TPId", TktPseudoID)}, OrgID);
		}

		public static DataRow SelectOne(Guid OrgID, int DeptID, int TktID)
		{
			return SelectRecord("sp_SelectTicketDetail", new SqlParameter[]{new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID)}, OrgID);
		}

		public static int GetTicketIDByNumber(Guid OrgID,int DeptID, string TktNumberFull)
		{
            DataRow _row = SelectRecord("sp_SelectTicketId", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketNumber", TktNumberFull) }, OrgID);
			if (_row == null) return 0;
			else return (int)_row["id"];
		}

        public static int GetTicketIDByNumber(int DeptID, string TktNumberFull)
        {
            return GetTicketIDByNumber(Guid.Empty, DeptID, TktNumberFull);
        }

        public static int GetTicketIDByPseudoID(Guid OrgID, int DeptID, string PseudoID)
        {
            DataRow _row = SelectRecord("sp_SelectTicketIdByPseudoID", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@PseudoID", PseudoID) }, OrgID);
            if (_row == null) return 0;
            else return (int)_row["id"];
        }

        public static int GetTicketIDByPseudoID(int DeptID, string PseudoID)
        {
            return GetTicketIDByPseudoID(Guid.Empty, DeptID, PseudoID);
        }

        public static int SelectTicketsCount(Guid orgId, Guid instId)
        {
            int DId = Companies.SelectDepartmentId(orgId, instId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM tbl_ticket WHERE company_id=" + DId.ToString(), orgId);
            return (int)_dt.Rows[0][0];
        }

	    public static DataRow SelectTicketCounts(Guid orgId, int DeptID, int newUser, int TechId)
        {
            return SelectRecord("sp_SelectTicketcounts", new SqlParameter[] { new SqlParameter("@companyID", DeptID), new SqlParameter("@newUser", newUser), new SqlParameter("@techId", TechId) });
        }

		public static DataRow SelectTicketCounts(int DeptID, int newUser, int TechId)
		{
			return SelectTicketCounts(Guid.Empty, DeptID, newUser, TechId);
		}

		public static DataRow SelectUserTicketCounts(int DeptID, int UserId)
		{
			return SelectRecord("sp_SelectTicketCountsUser", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@UserId", UserId) });
		}

		public static DataTable SelectUnassignedQueuesCount(int DeptID)
		{
			return SelectRecords("sp_SelectUnassignedQuesCount", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID) });
		}

		public static DataTable SelectQueuesCount(UserAuth User, Config Cnf)
		{
			return SelectByQuery(@"SELECT QueueTkt.QueId, QueueTkt.QueName, QueueTkt.TicketCount, ISNULL(qtodo.ToDoCount, 0) AS ToDoCount FROM
								(SELECT J.id AS QueId, Max(L.FirstName) AS QueName, SUM(CASE WHEN T.Status<>'Closed' THEN 1 ELSE 0 END) as TicketCount 
								FROM tbl_LoginCompanyJunc J
								INNER JOIN tbl_Logins L ON J.login_id = L.id
								LEFT OUTER JOIN tbl_ticket T ON T.company_id=" + User.lngDId + @" AND T.Technician_Id=J.id " + GlobalFilters.GlobalFiltersSqlWhere(User, Cnf, "T.", "J.", "SupGroupID") + @"
								WHERE J.company_id = " + User.lngDId + @" AND J.UserType_id = 4
								GROUP BY J.id) AS QueueTkt LEFT OUTER JOIN 
								(SELECT ToDoItem.AssignedId AS TechID, SUM(CASE WHEN ToDoItem.Id IS NULL THEN 0 ELSE 1 END) AS ToDoCount FROM ToDoItem WHERE DId=" + User.lngDId
								+ @" AND Completed=0 GROUP BY ToDoItem.AssignedId) qtodo ON qtodo.TechID = QueueTkt.QueId ORDER BY QueName");
		}


		public static TicketTimer SelectTicketTimer(int DeptID, int UserID)
		{
			TicketTimer result=TicketTimer.DaysOldTimer;
			
			DataRow _user_setting=SelectRecord("sp_SelectUserDetails", new SqlParameter[]{new SqlParameter("@Did", DeptID), new SqlParameter("@Id", UserID)});

			if (_user_setting!=null)
			{
				if (!_user_setting.IsNull("tintTicketTimer"))
				{
					if ((byte)_user_setting["tintTicketTimer"]>0) 
						result=TicketTimer.SLATimer;
				}
				else
				{
					DataRow _global_setting=SelectRecord("sp_SelectCompany", new SqlParameter[]{new SqlParameter("@CompanyId", DeptID)});
					
					if(_global_setting!=null)
					{
						if (!_global_setting.IsNull("tintTicketTimer"))
						{
							if ((byte)_global_setting["tintTicketTimer"]>0) 
								result=TicketTimer.SLATimer;
						};
					};
				};
			};

			return result;
		}

		public static void DeleteTicket(int DeptID, int TicketID)
		{
			DeleteTicket(Guid.Empty, DeptID, TicketID);
		}

		public static void DeleteTicket(Guid OrgId, int DeptID, int TicketID)
		{
			UpdateData("sp_DeleteTicket", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketID) }, OrgId);
		}

		public static bool SelectJunkMail(int DeptID, string Email, string Subject)
		{
			return SelectJunkMail(Guid.Empty, DeptID, Email, Subject);
		}

		public static bool SelectJunkMail(Guid OrgID, int DeptID, string Email, string Subject)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;
			UpdateData("sp_SelectJunkEmail", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@vchEmail", Email), new SqlParameter("@vchSubject", Subject) }, OrgID);
			if (_pRVAL.Value != DBNull.Value && (int)_pRVAL.Value == 1) return true;
			else return false;
		}

        public static string GetInitThreadMessageId(Guid OrgID, int DeptId, int TktId, string Email, string newMessageId)
        {
            SqlParameter _pInitMessageId = new SqlParameter("@InitMessageId", SqlDbType.NVarChar, 255);
            _pInitMessageId.Direction = ParameterDirection.Output;
            SqlParameter _pMessageId = new SqlParameter("@MessageId", SqlDbType.NVarChar, 255);
            if (!string.IsNullOrEmpty(newMessageId)) _pMessageId.Value = newMessageId;
            else _pMessageId.Value = DBNull.Value;
            UpdateData("sp_GetTicketInitThreadMessageId", new SqlParameter[] { new SqlParameter("@DId", DeptId), new SqlParameter("@TicketId", TktId), new SqlParameter("@EMail", Email), _pMessageId, _pInitMessageId }, OrgID);
            return _pInitMessageId.Value.ToString();
        }

        public static void DeleteJunkMail(int DeptID, int TicketID, int UserId)
		{
			UpdateData("sp_DeleteJunkMail", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketID), new SqlParameter("@intUId", UserId) });
		}

		public static DataTable SelectTicketLog(int DeptID, int TicketId)
		{
			return SelectTicketLog(Guid.Empty, DeptID, TicketId);
		}

		public static DataTable SelectTicketLog(Guid OrgID, int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectTktLogs", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) }, OrgID);
		}

		public static DataTable SelectPartsByMode(int DeptID, int TicketId, int Mode)
		{
			return SelectRecords("sp_SelectPartsByMode", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@Mode", Mode) });
		}

		public static DataTable SelectPartsOther(int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectPartsOther", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) });
		}

		public static DataRow SelectPartInfo(int DeptID, int PartId)
		{
			return SelectRecord("sp_Selectpart", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@PartId", PartId) });
		}

		public static void DeletePart(int DeptID, int TicketId, int UserId, int PartId, string TicketLog)
		{
			UpdateData("sp_DeletePart", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@UId", UserId), new SqlParameter("@PartId", PartId), new SqlParameter("@vchTktLog", TicketLog) });
		}

		public static void InsertPart(int DeptID, int TicketId, int UserId, int Mode, string PartDescription, int PartQty, decimal PartCost, string TicketLog)
		{
			UpdateData("sp_InsertPart", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@UId", UserId), new SqlParameter("@Mode", Mode), new SqlParameter("@PartDesc", PartDescription), new SqlParameter("@Quantity", PartQty), new SqlParameter("@TicketLogNote", TicketLog), new SqlParameter("@Cost", PartCost) });
		}

		public static void InsertPart(int DeptID, int TicketId, int UserId, int Mode, string PartDescription, int PartQty, decimal PartCost, string TicketLog, Guid ExternalPartId, Guid ExternalOrderId)
		{
			UpdateData("sp_InsertPart", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@UId", UserId), new SqlParameter("@Mode", Mode), new SqlParameter("@PartDesc", PartDescription), new SqlParameter("@Quantity", PartQty), new SqlParameter("@TicketLogNote", TicketLog), new SqlParameter("@Cost", PartCost), new SqlParameter("@ExternalPartId", ExternalPartId), new SqlParameter("@ExternalOrderId", ExternalOrderId) });
		}

		public static void UpdatePart(int DeptID, int PartId, int Action, int UserId, string TicketLog)
		{
			SqlParameter _pLogNote = new SqlParameter("@vchTktLogNote", SqlDbType.NVarChar, -1);
			if (TicketLog.Length > 4999) TicketLog = "--Text truncated at 5000 characters--<br><br>" + TicketLog.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
			if (TicketLog.Length > 0) _pLogNote.Value = TicketLog;
			else _pLogNote.Value = DBNull.Value;
			UpdateData("sp_UpdatePartStatus", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@PartId", PartId), new SqlParameter("@Action", Action), new SqlParameter("@UId", UserId), _pLogNote });
		}

		public static void UpdatePart(int DeptID, int PartId, int Action, int UserId, string TicketLog, string VendorDescription, decimal PartCost, int PartQty)
		{
			SqlParameter _pVendorDescription = new SqlParameter("@VendorDescription", SqlDbType.NVarChar, 150);
			if (VendorDescription.Length > 0) _pVendorDescription.Value = VendorDescription;
			else _pVendorDescription.Value = DBNull.Value;
			SqlParameter _pLogNote = new SqlParameter("@vchTktLogNote", SqlDbType.NVarChar, -1);
			if (TicketLog.Length > 4999) TicketLog = "--Text truncated at 5000 characters--<br><br>" + TicketLog.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
			if (TicketLog.Length > 0) _pLogNote.Value = TicketLog;
			else _pLogNote.Value = DBNull.Value;
			UpdateData("sp_UpdatePartStatus", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@PartId", PartId), new SqlParameter("@Action", Action), new SqlParameter("@UId", UserId), _pLogNote, _pVendorDescription, new SqlParameter("@Cost", PartCost), new SqlParameter("@Qty4Tkt", PartQty) });
		}

		public static DataTable SelectMiscCosts(int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectTicketMiscCost", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) });
		}

		public static void InsertMiscCost(int DeptID, int TicketId, int UserId, decimal Amount, string Note)
		{
			UpdateData("sp_InsertMiscCost", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), new SqlParameter("@TId", TicketId), new SqlParameter("@Amount", Amount), new SqlParameter("@Note", Note) });
		}

		public static void UpdateMiscCost(int DeptID, int TicketId, int MiscCostId, decimal Amount, string Note)
		{
			UpdateData("sp_UpdateMiscCost", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", MiscCostId), new SqlParameter("@TId", TicketId), new SqlParameter("@Amount", Amount), new SqlParameter("@Note", Note) });
		}

		public static DataTable DeleteMiscCosts(int DeptID, int TicketId, int MiscCostId)
		{
			return SelectRecords("sp_DeleteMiscCost", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@Id", MiscCostId) });
		}

		public static DataTable SelectTimes(int DeptID, int TicketId)
		{
            return SelectTimes(Guid.Empty, DeptID, TicketId);
		}

        public static DataTable SelectTimes(Guid OrgId, int DeptID, int TicketId)
        {
            return SelectRecords("sp_SelectTicketTimes", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) }, OrgId);
        }

		public static DataTable SelectTicketsDayTime(int companyID, int accountID, int userID, DateTime SelectedDate)
		{
			return SelectRecords("sp_SelectTicketsDayTime", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@Date", SelectedDate)
				   });
		}

		public static int InsertTime(int DeptID, int TicketID, int UserID, DateTime Date,
			decimal Hours, string Note, decimal HourlyRate, DateTime StartTime, DateTime StopTime,
			int TaskTypeID, decimal RemainHours, DateTime CreatedTime, int CreatedBy, int TimeOffset, bool timeEntryOnDetail)
		{
			return InsertTime(Guid.Empty, DeptID, TicketID, UserID, Date, Hours, Note, HourlyRate, StartTime, StopTime, TaskTypeID, RemainHours, CreatedTime, CreatedBy, TimeOffset, timeEntryOnDetail);
		}

		public static int InsertTime(Guid OrgID, int DeptID, int TicketID, int UserID, DateTime Date, 
			decimal Hours, string Note, decimal HourlyRate, DateTime StartTime, DateTime StopTime,
			int TaskTypeID, decimal RemainHours, DateTime CreatedTime, int CreatedBy, int TimeOffset, bool timeEntryOnDetail)
		{
			SqlParameter[] _params = new SqlParameter[16];
			_params[0] = new SqlParameter("@DepartmentID", DeptID);
			_params[1] = new SqlParameter("@TicketID", TicketID);
			_params[2] = new SqlParameter("@UserID", SqlDbType.Int);
			if (UserID != 0) _params[2].Value = UserID;
			else _params[2].Value = DBNull.Value;
			_params[3] = new SqlParameter("@Date", Date);
			_params[4] = new SqlParameter("@Hours", Hours);
			_params[5] = new SqlParameter("@HourlyRate", HourlyRate);
			_params[6] = new SqlParameter("@RemainHours", SqlDbType.Decimal);
			if (RemainHours >= 0) _params[6].Value = RemainHours;
			else _params[6].Value = DBNull.Value;
			_params[7] = new SqlParameter("@Note", SqlDbType.NVarChar);
			if (!String.IsNullOrEmpty(Note)) _params[7].Value = Note;
			else _params[7].Value = DBNull.Value;
			_params[8] = new SqlParameter("@StartTime", SqlDbType.SmallDateTime);
			if (StartTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[8].Value = StartTime;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@StopTime", SqlDbType.SmallDateTime);
			if (StopTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[9].Value = StopTime;
			else _params[9].Value = DBNull.Value;
			_params[10] = new SqlParameter("@CreatedTime", SqlDbType.SmallDateTime);
			if (CreatedTime != DateTime.MinValue) _params[10].Value = CreatedTime;
			else _params[10].Value = DBNull.Value;
			_params[11] = new SqlParameter("@CreatedBy", SqlDbType.Int);
			if (CreatedBy != 0) _params[11].Value = CreatedBy;
			else _params[11].Value = DBNull.Value;
			_params[12] = new SqlParameter("@TimeOffset", TimeOffset);
			_params[13] = new SqlParameter("@TaskTypeID", SqlDbType.Int);
			if (TaskTypeID != 0) _params[13].Value = TaskTypeID;
			else _params[13].Value = DBNull.Value;
			_params[14] = new SqlParameter("@TimeEntryOnDetail", timeEntryOnDetail);
			_params[15] = new SqlParameter("@TimeLogID", SqlDbType.Int);
			_params[15].Direction = ParameterDirection.Output;
			UpdateData("sp_InsertTicketTime", _params, OrgID);
			return (int)_params[15].Value; 
		}

		public static int SelectTicketSLATime(int DeptID, DateTime StartTime, DateTime EndTime)
		{
			return SelectTicketSLATime(Guid.Empty, DeptID, StartTime, EndTime);
		}

		public static int SelectTicketSLATime(Guid OrgId, int DeptID, DateTime StartTime, DateTime EndTime)
		{
			int result=0;

			int sign = 1;
			if (StartTime > EndTime) 
			{
				sign = -1;
				DateTime _temp = StartTime;
				StartTime = EndTime;
				EndTime = _temp;
			}
			string _query = "DECLARE @WorkDays char(7); ";
			_query += "DECLARE @StartBusinnessTime int; ";
			_query += "DECLARE @EndBusinnessTime int; ";
			_query += "SELECT @WorkDays = I.WorkingDays, @StartBusinnessTime = dbo.fxGetConfigValueStr("
				+ DeptID.ToString() + ", 'tinyBusHourStart')*60 + dbo.fxGetConfigValueStr("
				+ DeptID.ToString() + ", 'tinyBusMinStart'), @EndBusinnessTime = dbo.fxGetConfigValueStr("
				+ DeptID.ToString() + ", 'tinyBusHourStop')*60 + dbo.fxGetConfigValueStr("
				+ DeptID.ToString() + ", 'tinyBusMinStop') FROM tbl_company C INNER JOIN Mc_Instance I ON I.InstanceId = C.company_guid WHERE C.company_id = "
				+ DeptID.ToString() + "; ";
			_query += "IF @WorkDays IS NULL SET @WorkDays = '1111100'; ";
			_query += "IF @StartBusinnessTime IS NULL SET @StartBusinnessTime = 0; ";
			_query += "IF @EndBusinnessTime IS NULL SET @EndBusinnessTime = 1440; ";
			_query += "select dbo.fxGetOperationalMinutes";
			_query=_query+"(";
			_query=_query+DeptID.ToString()+", ";
			_query=_query+"'"+Functions.FormatSQLDateTime(StartTime)+"', ";
			_query = _query + "'" + Functions.FormatSQLDateTime(EndTime) + "', @WorkDays, @StartBusinnessTime, @EndBusinnessTime) as DaysOld";

			DataTable _table = SelectByQuery(_query, OrgId);
			if(_table!=null)
			{
				if (_table.Rows.Count>0)
				{
					DataRow _row=_table.Rows[0];
					if(_row!=null)
					{
						result=(int)_row["DaysOld"];
					};
				};
			};

			return sign * result;
		}

		public static DataTable SelectAssets(int DeptID, int TicketId)
		{
			return SelectAssets(Guid.Empty, DeptID, TicketId);
		}

		public static DataTable SelectAssets(Guid OrgId, int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectTicketAssets", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TicketId) }, OrgId);
		}

		public static Assets.AssetItem[] SelectAssetsToArray(int DeptID, int TicketId)
		{
			DataTable _dt = SelectAssets(DeptID, TicketId);
			Data.Assets.AssetItem[] _arr = new Assets.AssetItem[_dt.Rows.Count];
			for (int i = 0; i < _dt.Rows.Count; i++)
			{
				if (_dt.Rows[i].IsNull("AssetId")) _arr[i] = new Assets.AssetItem(_dt.Rows[i]["SerialTagNumber"].ToString(), _dt.Rows[i]["Description"].ToString());
				else _arr[i] = new Assets.AssetItem((int)_dt.Rows[i]["AssetId"], _dt.Rows[i]["Description"].ToString());
			}
			return _arr;
		}

		public static void DeleteAsset(int DeptID, int TicketId, int AssetId, string SerialTagNumber)
		{
			UpdateData("sp_DeleteTicketAsset", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@AssetId", AssetId), new SqlParameter("@SerialTagNumber", SerialTagNumber) });
		}

		public static void UpdateAssets(int DeptID, int TicketId, Assets.AssetItem[] assets)
		{
			Assets.AssetItem[] _assArr = SelectAssetsToArray(DeptID, TicketId);
			bool _toDelete = true;
			foreach (Assets.AssetItem _ass1 in _assArr)
			{
				_toDelete = true;
				foreach (Assets.AssetItem _ass2 in assets)
				{
					if (_ass2.ID!=0 && _ass1.ID == _ass2.ID)
					{
						_toDelete = false;
						break;
					}
					else if (_ass2.ID == 0 && _ass1.SerialTagNumber == _ass2.SerialTagNumber)
					{
						_toDelete = false;
						break;
					}
				}
				if (!_toDelete) continue;
				DeleteAsset(DeptID, TicketId, _ass1.ID, _ass1.SerialTagNumber);
			}
			foreach (Assets.AssetItem _asset in assets) InsertAsset(DeptID, TicketId, _asset.ID, _asset.SerialTagNumber, _asset.Description);
		}

		public static int UpdateAsset(int DeptID, int Id, int TicketId, int AssetId, string SerialTagNumber, string Description)
		{
			return UpdateAsset(Guid.Empty, DeptID, Id, TicketId, AssetId, SerialTagNumber, Description);
		}

		public static int UpdateAsset(Guid OrgID, int DeptID, int Id, int TicketId, int AssetId, string SerialTagNumber, string Description)
		{
			SqlParameter _pId=new SqlParameter("@Id", SqlDbType.Int);
			_pId.Direction = ParameterDirection.InputOutput;
			if (Id != 0) _pId.Value = Id;
			else _pId.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketAsset", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@AssetId", AssetId), new SqlParameter("@SerialTagNumber", SerialTagNumber), new SqlParameter("@Description", Description) }, OrgID);
			return (int)_pId.Value;
		}

		public static int InsertAsset(int DeptID, int TicketId, int AssetId, string SerialTagNumber, string Description)
		{
			return UpdateAsset(DeptID, 0, TicketId, AssetId, SerialTagNumber, Description);
		}

		public static int InsertAsset(Guid OrgID, int DeptID, int TicketId, int AssetId, string SerialTagNumber, string Description)
		{
			return UpdateAsset(OrgID, DeptID, 0, TicketId, AssetId, SerialTagNumber, Description);
		}

		public static DataTable SelectFiles(int DeptID, int TicketId, int FileId)
		{
			return SelectRecords("sp_SelectTktFile", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@FId", FileId)});
		}

		public static DataTable SelectFiles(int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectTktFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) });
		}

		public static Data.FileItem[] SelectFilesToArray(int DeptID, int TicketId)
		{
			DataTable _dt = SelectFiles(DeptID, TicketId);
			Data.FileItem[] _arr = new Data.FileItem[_dt.Rows.Count];
			for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new Data.FileItem((int)_dt.Rows[i]["Id"], _dt.Rows[i]["FileName"].ToString(), (int)_dt.Rows[i]["FileSize"], (DateTime)_dt.Rows[i]["dtUpdated"], !_dt.Rows[i].IsNull("FileData") ? (byte[])_dt.Rows[i]["FileData"] : null);
			return _arr;
		}

		public static int InsertFile(int DeptID, int TicketId, string FileName, int FileSize, byte[] FileData)
		{
			return InsertFile(Guid.Empty, DeptID, TicketId, FileName, FileSize, FileData);
		}

		public static int InsertFile(Guid OrgID, int DeptID, int TicketId, string FileName, int FileSize, byte[] FileData)
		{
			return UpdateFile(OrgID, DeptID, 0, TicketId, FileName, FileSize, FileData);
		}

		public static int UpdateFile(Guid OrgID, int DeptID, int Id, int TicketId, string FileName, int FileSize, byte[] FileData)
		{
			SqlParameter _pFileData=new SqlParameter("@FileData", SqlDbType.Image);
			_pFileData.Value = FileData;
			UpdateData("sp_UpdateTktFile", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@FileName", FileName), new SqlParameter("@FileSize", FileSize), _pFileData }, OrgID);
			return 0;
		}

		public static void DeleteFile(Guid OrgID, int DeptID, int TktID, int FileId)
		{
            UpdateData("sp_DeleteTktFile", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), new SqlParameter("@FId", FileId) }, OrgID);
		}

		public static void UpdateCustomFields(int departmentId, int ticketId, string customFieldsXML)
		{
			UpdateData("sp_UpdateCustomFieldsXML", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TicketId", ticketId), new SqlParameter("@CustXML", customFieldsXML) });
		}

		public static int CreateNew(int DeptID,
			int UserId,
			int TechnicianId,
			int TktUserId,
			DateTime CreationDate,
			int AccountId,
			int AccountLocationId,
			bool LookUpAccount,
			int LocationId,
			int ClassId,
			int LevelId,
			string SubmissionCategoryName,
			bool HandledByCallCenter,
			int CreationCategoryId,
			bool Preventive,
			int PriorityId,
			DateTime RequestComplDate,
			string RequestComplNote,
			string SerialNumber,
			Assets.AssetItem[] assets,
			string IDMethod,
			string CustomFields,
			string Subject,
			string Text,
			System.IO.FileInfo[] files,
			string Status,
			out int initialPostId)
		{
			return CreateNew(Guid.Empty,
					DeptID,
					UserId,
					TechnicianId,
					TktUserId,
					CreationDate,
					AccountId,
					AccountLocationId,
					LookUpAccount,
					LocationId,
					ClassId,
					LevelId,
					SubmissionCategoryName,
					HandledByCallCenter,
					CreationCategoryId,
					Preventive,
					 PriorityId,
					RequestComplDate,
					RequestComplNote,
					 SerialNumber,
					assets,
					IDMethod,
					CustomFields,
					Subject,
					Text,
					files,
					Status,
					out initialPostId);
		}

		public static int CreateNew(Guid OrgID,
			int DeptID,
			int UserId,
			int TechnicianId,
			int TktUserId,
			DateTime CreationDate,
			int AccountId,
			int AccountLocationId,
			bool LookUpAccount,
			int LocationId,
			int ClassId,
			int LevelId,
			string SubmissionCategoryName,
			bool HandledByCallCenter,
			int CreationCategoryId,
			bool Preventive,
			int PriorityId,
			DateTime RequestComplDate,
			string RequestComplNote,
			string SerialNumber,
			Assets.AssetItem[] assets,
			string IDMethod,
			string CustomFields,
			string Subject,
			string Text,
			System.IO.FileInfo[] files,
			string Status,
			out int initialPostId)
		{
			return CreateNew(OrgID,
			DeptID,
			UserId,
			TechnicianId,
			TktUserId,
			CreationDate,
			AccountId,
			AccountLocationId,
			LookUpAccount,
			LocationId,
			ClassId,
			LevelId,
			SubmissionCategoryName,
			HandledByCallCenter,
			CreationCategoryId,
			Preventive,
			 PriorityId,
			RequestComplDate,
			RequestComplNote,
			 SerialNumber,
			assets,
			IDMethod,
			CustomFields,
			Subject,
			Text,
			files,
			Status,
			out initialPostId,
			0,
			0,
			0,
			-1);
		}

		public static int CreateNew(Guid OrgID, 
			int DeptID,
			int UserId,
			int TechnicianId,
			int TktUserId,
			DateTime CreationDate,
			int AccountId,
			int AccountLocationId,
			bool LookUpAccount,
			int LocationId,
			int ClassId,
			int LevelId,
			string SubmissionCategoryName,
			bool HandledByCallCenter,
			int CreationCategoryId,
			bool Preventive,
			int PriorityId,
			DateTime RequestComplDate,
			string RequestComplNote,
			string SerialNumber,
			Assets.AssetItem[] assets,
			string IDMethod,
			string CustomFields,
			string Subject,
			string Text,
			System.IO.FileInfo[] files,
			string Status,
			out int initialPostId,
			int ProjectId,
			int folderID,
			int schedTicketID,
			decimal EstimatedTime)
		{

			bool _newUserPost=TechnicianId==UserId ? false : true;
			bool _newTechPost=TktUserId==UserId ? false : true;

			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;
			SqlParameter _pTechnicianId = new SqlParameter("@techId", SqlDbType.Int);
			if (TechnicianId != 0) _pTechnicianId.Value = TechnicianId;
			else _pTechnicianId.Value = DBNull.Value;
			SqlParameter _pCreationDate = new SqlParameter("@dtCreatedTime", SqlDbType.SmallDateTime);
			if (CreationDate!=null && CreationDate!=DateTime.MinValue) _pCreationDate.Value = CreationDate;
			else _pCreationDate.Value=DBNull.Value;
			SqlParameter _pAccountId = new SqlParameter("@intAcctId", SqlDbType.Int);
			if (AccountId > 0) _pAccountId.Value = AccountId;
			else _pAccountId.Value = DBNull.Value;
            SqlParameter _pbtNoAccount = new SqlParameter("@btNoAccount", SqlDbType.Bit);
            if (AccountId == -2) _pbtNoAccount.Value = true;
            else _pbtNoAccount.Value = false;
			SqlParameter _pAccountLocationId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
			if (AccountLocationId > 0) _pAccountLocationId.Value = AccountLocationId;
			else _pAccountLocationId.Value = DBNull.Value;
			SqlParameter _pLocationId = new SqlParameter("@location", SqlDbType.Int);
			if (LocationId != 0) _pLocationId.Value = LocationId;
			else _pLocationId.Value = DBNull.Value;
			SqlParameter _pClassId = new SqlParameter("@class", SqlDbType.Int);
			if (ClassId != 0) _pClassId.Value = ClassId;
			else _pClassId.Value = DBNull.Value;
			SqlParameter _pLevelId = new SqlParameter("@tintLevel", SqlDbType.TinyInt);
			if (LevelId != 0) _pLevelId.Value = LevelId;
			else _pLevelId.Value = DBNull.Value;
			SqlParameter _pSubmissionCategoryName = new SqlParameter("@vchSubmissionCat", SqlDbType.NVarChar, 50);
			if (!string.IsNullOrEmpty(SubmissionCategoryName)) _pSubmissionCategoryName.Value = SubmissionCategoryName;
			else _pSubmissionCategoryName.Value = DBNull.Value;
			SqlParameter _pCreationCategoryId = new SqlParameter("@intCategoryId", SqlDbType.Int);
			if (CreationCategoryId != 0) _pCreationCategoryId.Value = CreationCategoryId;
			else _pCreationCategoryId.Value = DBNull.Value;
			SqlParameter _pPriorityId = new SqlParameter("@priority", SqlDbType.Int);
			if (PriorityId != 0) _pPriorityId.Value = PriorityId;
			else _pPriorityId.Value = DBNull.Value;
			SqlParameter _pRequestComplDate = new SqlParameter("@dtReqComp", SqlDbType.SmallDateTime);
			if (RequestComplDate!=DateTime.MinValue) _pRequestComplDate.Value = RequestComplDate;
			else _pRequestComplDate.Value = DBNull.Value;
			SqlParameter _pRequestComplNote = new SqlParameter("@vchReqComp", SqlDbType.NVarChar, 50);
			if (!string.IsNullOrEmpty(RequestComplNote)) _pRequestComplNote.Value = RequestComplNote;
			else _pRequestComplNote.Value = DBNull.Value;
			SqlParameter _pIDMethod = new SqlParameter("@vchIdMethod", SqlDbType.NVarChar, 255);
			if (!string.IsNullOrEmpty(IDMethod)) _pIDMethod.Value = IDMethod.Length > 255 ? IDMethod.Substring(0, 255) : IDMethod;
			else _pIDMethod.Value = DBNull.Value;
			SqlParameter _pCustomFields = new SqlParameter("@CustomXML", SqlDbType.NText);
			if (!string.IsNullOrEmpty(CustomFields)) _pCustomFields.Value = CustomFields;
			else _pCustomFields.Value = DBNull.Value;
			SqlParameter _pSubject = new SqlParameter("@subject", SqlDbType.NVarChar, 100);
			if (Subject.Length > 0) _pSubject.Value = Subject;
			else _pSubject.Value = DBNull.Value;
			SqlParameter _pText = new SqlParameter("@vchInitPost", SqlDbType.NVarChar, -1);
			if (!string.IsNullOrEmpty(Text))
			{
				if (Text.Length > 4899) Text = "--Text truncated at 5000 characters--<br><br>" + Text.Substring(0, 4800) + "<br><br>--Text truncated at 5000 characters--";
				_pText.Value = Text;
			}
			else _pText.Value = "(Initial post was blank.)";

			//Depreciated Parameter
			SqlParameter _pSerialNumber=new SqlParameter("@serialnumber", SqlDbType.NVarChar, 50);
			if (!string.IsNullOrEmpty(SerialNumber)) _pSerialNumber.Value = SerialNumber;
			else _pSerialNumber.Value=DBNull.Value;
			//----
			SqlParameter _pViaEmailParser=new SqlParameter("@btViaEmailParser", SqlDbType.Bit);
			_pViaEmailParser.Value=DBNull.Value;

			SqlParameter _pTktId = new SqlParameter("@TId", SqlDbType.Int);
			_pTktId.Direction = ParameterDirection.Output;
			SqlParameter _pTktNumber = new SqlParameter("@intTktNumber", SqlDbType.Int);
			_pTktNumber.Direction = ParameterDirection.Output;

			SqlParameter pInitialPostId = new SqlParameter("@InitialPostId", SqlDbType.Int);
			pInitialPostId.Direction = ParameterDirection.Output;

			SqlParameter _pProjectId = new SqlParameter("@ProjectID", SqlDbType.Int);
			if (ProjectId != 0) _pProjectId.Value = ProjectId;
			else _pProjectId.Value = DBNull.Value;

			SqlParameter _pFolderID = new SqlParameter("@FolderID", SqlDbType.Int);
			if (folderID != 0) _pFolderID.Value = folderID;
			else _pFolderID.Value = DBNull.Value;

			SqlParameter _pSchedTicketID = new SqlParameter("@SchedTicketID", SqlDbType.Int);
			if (schedTicketID != 0) _pSchedTicketID.Value = schedTicketID;
			else _pSchedTicketID.Value = DBNull.Value;

			SqlParameter _pEstTime = new SqlParameter("@EstimatedTime", SqlDbType.Decimal);
			if (EstimatedTime >= 0) _pEstTime.Value = EstimatedTime;
			else _pEstTime.Value = DBNull.Value;

			UpdateData("sp_InsertNewTicket", new SqlParameter[]{_pRVAL,
				new SqlParameter("@DId", DeptID),
				new SqlParameter("@PseudoId", Micajah.Common.Bll.Support.GeneratePseudoUnique()),
				_pPriorityId,
				new SqlParameter("@user_id", TktUserId),
				new SqlParameter("@status", Status),
				_pSubject,
				_pText,
				new SqlParameter("@newuserpost", _newUserPost),
				new SqlParameter("@newtechpost", _newTechPost),
				new SqlParameter("@createdId", UserId),
				_pTechnicianId,
				_pLocationId,
				_pClassId,
				_pSerialNumber,
				_pCustomFields,
				_pTktId,
				_pRequestComplDate,
				_pRequestComplNote,
				_pViaEmailParser,
				new SqlParameter("@btPreventive", Preventive),
				_pTktNumber,
				_pCreationCategoryId,
				_pAccountId,
				_pAccountLocationId,
				new SqlParameter("@btLookUpAcct", LookUpAccount),
				_pCreationDate,
				_pIDMethod,
				new SqlParameter("@btHandledByCallCentre", HandledByCallCenter),
				_pSubmissionCategoryName,
				_pLevelId,
				pInitialPostId,
				_pProjectId,
				_pFolderID,
				_pSchedTicketID,
				_pEstTime,
                _pbtNoAccount}, OrgID);

			initialPostId = pInitialPostId.Value != DBNull.Value ? (int) pInitialPostId.Value : 0;
			int _TktId = (int)_pRVAL.Value;
			if (_TktId < 0) return _TktId;
			if (assets != null)
				foreach (Assets.AssetItem _asset in assets) InsertAsset(OrgID, DeptID, _TktId, _asset.ID, _asset.SerialTagNumber, _asset.Description);
			if (files != null)
			foreach (System.IO.FileInfo _file in files)
			{
				System.IO.FileStream _fstream = _file.OpenRead();
				byte[] _data = new byte[Convert.ToInt32(_fstream.Length)];
				_fstream.Read(_data, 0, _data.Length);
				InsertFile(OrgID, DeptID, _TktId, _file.Name, Convert.ToInt32(_file.Length), _data);
				_fstream.Close();
			}
			return _TktId;
		}

		public enum TicketAction
		{
			New,
			Response,
			Confirm,
			Transfer,
			Escalate,
			ReOpen,
			Close,
			Delete
		}

		public enum SendEmailTo
		{
			User,
			Tech,
			Both
		}

		public static int MailTicketCheckCreateAccess(Guid OrgID, int department_id, string email, ref string first_name, ref string last_name, string class_or_queue, int class_or_queue_id, string email_dropbox_id, bool is_service_email, out string pin, out string custom_failure_text)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;

		    SqlParameter _pFirstName = new SqlParameter("@FirstName", SqlDbType.NVarChar, 50);
            _pFirstName.Direction = ParameterDirection.InputOutput;
		    _pFirstName.Value = !string.IsNullOrEmpty(first_name) ? first_name : string.Empty;

            SqlParameter _pLastName = new SqlParameter("@LastName", SqlDbType.NVarChar, 50);
            _pLastName.Direction=ParameterDirection.InputOutput;
            _pLastName.Value = !string.IsNullOrEmpty(last_name) ? last_name : string.Empty;

            pin=Logins.GenerateRandomPassword();
            SqlParameter _pPin = new SqlParameter("@Pin", SqlDbType.NVarChar, 4);
		    _pPin.Value = pin;

            SqlParameter _pFailureMsg = new SqlParameter("@FailureMsg", SqlDbType.NVarChar, -1);
			_pFailureMsg.Direction = ParameterDirection.Output;

			SqlParameter _pNewGlobalAcct = new SqlParameter("@NewGlobalAcct", SqlDbType.Bit);
			_pNewGlobalAcct.Direction = ParameterDirection.Output;

			SqlParameter _pSendEmailToPref = new SqlParameter("@SendEmailToPref", SqlDbType.Bit);
			_pSendEmailToPref.Direction = ParameterDirection.Output;

		    SqlParameter _pIsServiceEmail = new SqlParameter("@IsServiceEmail", SqlDbType.Bit);
		    _pIsServiceEmail.Value = is_service_email;

			UpdateData("sp_InsertEmailParseLogin", new SqlParameter[]{_pRVAL,
			new SqlParameter("@DId", department_id),
			_pFirstName,
			_pLastName,
			new SqlParameter("@EMail", email),
			_pPin,
			new SqlParameter("@ClassorQue", class_or_queue),
			new SqlParameter("@CQId", class_or_queue_id),
			new SqlParameter("@EDropBoxId", email_dropbox_id),
            _pIsServiceEmail,
			_pFailureMsg,
			_pNewGlobalAcct,
			_pSendEmailToPref
			}, OrgID);

			custom_failure_text = _pFailureMsg.Value.ToString();
            if (string.IsNullOrEmpty(first_name)) first_name = _pFirstName.Value.ToString();
            if (string.IsNullOrEmpty(last_name)) last_name = _pLastName.Value.ToString();
			return (int)_pRVAL.Value;
		}

		public static int MailTicketCreate(int department_id, int user_id, int created_user_id, int tech_id, string class_or_queue, int class_or_queue_id, string email_dropbox_id, int email_priority, string subject, string text, string emailsuffix_from, string emailsuffix_to)
		{
			return MailTicketCreate(Guid.Empty, department_id, user_id, created_user_id, tech_id, class_or_queue, class_or_queue_id, email_dropbox_id, email_priority, subject, text, emailsuffix_from, emailsuffix_to);
		}

		public static int MailTicketCreate(Guid OrgID, int department_id, int user_id, int created_user_id, int tech_id, string class_or_queue, int class_or_queue_id, string email_dropbox_id, int email_priority, string subject, string text, string emailsuffix_from, string emailsuffix_to)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;

			SqlParameter _pTId = new SqlParameter("@TId", SqlDbType.Int);
			_pTId.Direction = ParameterDirection.Output;

            SqlParameter _pTechId = new SqlParameter("@TktTechId", SqlDbType.Int);
            if (tech_id != 0) _pTechId.Value = tech_id;
            else _pTechId.Value = DBNull.Value;

			SqlParameter _pTechEmailPref = new SqlParameter("@TechEmailPref", SqlDbType.Bit);
			_pTechEmailPref.Direction = ParameterDirection.Output;

			SqlParameter _pTechName = new SqlParameter("@TechName", SqlDbType.NVarChar, 100);
			_pTechName.Direction = ParameterDirection.Output;

			SqlParameter _pUserName = new SqlParameter("@UserName", SqlDbType.NVarChar, 100);
			_pUserName.Direction = ParameterDirection.Output;

			SqlParameter _pTechEmail = new SqlParameter("@TechEmail", SqlDbType.NVarChar, 100);
			_pTechEmail.Direction = ParameterDirection.Output;

			SqlParameter _pconfigPriorities = new SqlParameter("@configPriorities", SqlDbType.TinyInt);
			_pconfigPriorities.Direction = ParameterDirection.Output;

			SqlParameter _pconfigUserPriorities = new SqlParameter("@configUserPriorities", SqlDbType.TinyInt);
			_pconfigUserPriorities.Direction = ParameterDirection.Output;

			SqlParameter _pconfigLocationTracking = new SqlParameter("@configLocationTracking", SqlDbType.TinyInt);
			_pconfigLocationTracking.Direction = ParameterDirection.Output;

			SqlParameter _pconfigClassTracking = new SqlParameter("@configClassTracking", SqlDbType.TinyInt);
			_pconfigClassTracking.Direction = ParameterDirection.Output;

			SqlParameter _pDepartmentName = new SqlParameter("@DepartmentName", SqlDbType.NVarChar, 150);
			_pDepartmentName.Direction = ParameterDirection.Output;

			SqlParameter _pLogoName = new SqlParameter("@LogoName", SqlDbType.NVarChar, 50);
			_pLogoName.Direction = ParameterDirection.Output;

			SqlParameter _pTktNumber = new SqlParameter("@TktNumber", SqlDbType.Int);
			_pTktNumber.Direction = ParameterDirection.Output;

			SqlParameter _pClassName = new SqlParameter("@ClassName", SqlDbType.NVarChar, 50);
			_pClassName.Direction = ParameterDirection.Output;

			SqlParameter _pLocationName = new SqlParameter("@LocationName", SqlDbType.NVarChar, 50);
			_pLocationName.Direction = ParameterDirection.Output;

			SqlParameter _pSuccessMsg = new SqlParameter("@SuccessMsg", SqlDbType.NVarChar, -1);
			_pSuccessMsg.Direction = ParameterDirection.Output;

			SqlParameter _pdtSLAComplete = new SqlParameter("@dtSLAComplete", SqlDbType.DateTime);
			_pdtSLAComplete.Direction = ParameterDirection.Output;

			SqlParameter _pdtSLAResponse = new SqlParameter("@dtSLAResponse", SqlDbType.DateTime);
			_pdtSLAResponse.Direction = ParameterDirection.Output;

			SqlParameter _ptintPriority = new SqlParameter("@tintPriority", SqlDbType.TinyInt);
			_ptintPriority.Direction = ParameterDirection.Output;

			SqlParameter _pvchPriority = new SqlParameter("@vchPriority", SqlDbType.NVarChar, 50);
			_pvchPriority.Direction = ParameterDirection.Output;

			SqlParameter _pbitConfigLVL = new SqlParameter("@bitConfigLVL", SqlDbType.Bit);
			_pbitConfigLVL.Direction = ParameterDirection.Output;

			SqlParameter _pbitConfigLVLUser = new SqlParameter("@bitConfigLVLUser", SqlDbType.Bit);
			_pbitConfigLVLUser.Direction = ParameterDirection.Output;

			SqlParameter _ptintLevel = new SqlParameter("@tintLevel", SqlDbType.TinyInt);
			_ptintLevel.Direction = ParameterDirection.Output;

			SqlParameter _pbtCfgAcctMngr = new SqlParameter("@btCfgAcctMngr", SqlDbType.Bit);
			_pbtCfgAcctMngr.Direction = ParameterDirection.Output;

			SqlParameter _pvchAcctName = new SqlParameter("@vchAcctName", SqlDbType.NVarChar, 100);
			_pvchAcctName.Direction = ParameterDirection.Output;

			SqlParameter _pvchAcctLocName = new SqlParameter("@vchAcctLocName", SqlDbType.NVarChar, 25);
			_pvchAcctLocName.Direction = ParameterDirection.Output;

			SqlParameter _pvchErrMsg = new SqlParameter("@vchErrMsg", SqlDbType.NVarChar, 1000);
			_pvchErrMsg.Direction = ParameterDirection.Output;

			SqlParameter _pbtCfgSuppressBWALogos = new SqlParameter("@btCfgSuppressBWALogos", SqlDbType.Bit);
			_pbtCfgSuppressBWALogos.Direction = ParameterDirection.Output;

			SqlParameter _pNotificationEventsQueueId = new SqlParameter("@NotificationEventsQueueId", SqlDbType.Int);
			_pNotificationEventsQueueId.Direction = ParameterDirection.Output;

			UpdateData("sp_InsertEmailNewTkt2", new SqlParameter[]{_pRVAL,
			new SqlParameter("@DId", department_id),
			new SqlParameter("@PseudoId",  Micajah.Common.Bll.Support.GeneratePseudoUnique()),
			new SqlParameter("@CQId", class_or_queue_id),
			new SqlParameter("@ClassorQue", class_or_queue),
			new SqlParameter("@EDropBoxId", email_dropbox_id),
			new SqlParameter("@EPriority", email_priority),
			new SqlParameter("@TktUserId", user_id),
            new SqlParameter("@TktCreatedUserId", created_user_id),
            _pTechId,
			new SqlParameter("@TktSubject", subject),
			new SqlParameter("@TktText", text),
			new SqlParameter("@vchUserEmailSuffixFrom", emailsuffix_from), 
			new SqlParameter("@vchUserEmailSuffixTo", emailsuffix_to),
			_pTId,
			_pTechEmailPref,
			_pTechName,
			_pUserName,
			_pTechEmail,
			_pconfigPriorities,
			_pconfigUserPriorities,
			_pconfigLocationTracking, 
			_pconfigClassTracking, 
			_pDepartmentName, 
			_pLogoName,
			_pTktNumber,
			_pClassName,
			_pLocationName,
			_pSuccessMsg,
			_pdtSLAComplete,
			_pdtSLAResponse,
			_ptintPriority,
			_pvchPriority,
			_pbitConfigLVL,
			_pbitConfigLVLUser,
			_ptintLevel,
			_pbtCfgAcctMngr,            
			_pvchAcctName,
			_pvchAcctLocName,
			_pvchErrMsg,
			_pbtCfgSuppressBWALogos,
			_pNotificationEventsQueueId
			}, OrgID);

			if ((int)_pRVAL.Value != 0) return (int)_pRVAL.Value;
			return (int)_pTId.Value;
		}

		public static int MailTicketResponse(Guid OrgID, int department_id, int ticket_id, string email, string text, bool is_service_email)
		{

			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;

			SqlParameter _pTktNumber = new SqlParameter("@TktNumber", SqlDbType.Int);
			_pTktNumber.Direction = ParameterDirection.Output;

			UpdateData("sp_InsertEmailResponse", new SqlParameter[]{_pRVAL,
			new SqlParameter("@DId", department_id),
			new SqlParameter("@TId", ticket_id),
			new SqlParameter("@Text", text),
			new SqlParameter("@FromEmail", email),
            new SqlParameter("@IsServiceEmail", is_service_email), 
			_pTktNumber
			}, OrgID);

			int _return_value = (int)_pRVAL.Value;
            if (_return_value == 0 && _pTktNumber.Value!=DBNull.Value)
				return (int)_pTktNumber.Value;
			return _return_value;
		}

        public static DataTable SelectProjectTimeLogs(int companyID, DateTime selectedDate, int projectID,
                    int techID, int accountID, int taskTypeId)
        {
            return SelectProjectTimeLogs(Guid.Empty, companyID, selectedDate, projectID, techID, accountID, taskTypeId);
        }

		public static DataTable SelectProjectTimeLogs(Guid orgId, int companyID, DateTime selectedDate, int projectID,
			int techID, int accountID, int taskTypeId)
		{
			SqlParameter spDate = new SqlParameter("@Date", SqlDbType.SmallDateTime);
			if (selectedDate == DateTime.MinValue)
			{
				spDate.Value = DBNull.Value;
			}
			else
			{
				spDate.Value = selectedDate;
			}
			return SelectRecords("sp_SelectProjectTicketTimes", new SqlParameter[] 
			{ 
				new SqlParameter("@DId", companyID),
				spDate,
				new SqlParameter("@ProjectID", projectID),
				new SqlParameter("@TechID", techID),
				new SqlParameter("@AccountID", accountID),
				new SqlParameter("@TaskTypeId", taskTypeId)
			}, orgId);
		}

		public static void DeleteTicketTime(int ticketTimeId, int departmentID, int ticketID)
		{
			UpdateData("sp_DeleteTicketTime", new SqlParameter[]
			{
				 new SqlParameter("@TicketTimeId", ticketTimeId),
				 new SqlParameter("@DepartmentID", departmentID),
				 new SqlParameter("@TicketID", ticketID)
			});
		}

        public static DataRow SelectTicketTimeByID(int companyID, int ticketTimeID)
        {
            return SelectTicketTimeByID(Guid.Empty, companyID, ticketTimeID);
        }

        public static DataRow SelectTicketTimeByID(Guid OrgId, int companyID, int ticketTimeID)
        {
            return SelectRecord("sp_SelectTicketTimeByID", new SqlParameter[] 
			{ 
				new SqlParameter("@CompanyID", companyID),
				new SqlParameter("@TicketTimeID", ticketTimeID)
			}, OrgId);
        }

        public static void UpdateTicketTime(Guid OrgId, int ticketTimeId, int DeptID, int TicketID, int UserID, DateTime Date,
			decimal Hours, string Note, decimal HourlyRate, DateTime StartTime, DateTime StopTime,
			int TaskTypeID, decimal RemainHours, DateTime updatedTime, int updatedBy, int TimeOffset)
		{
			SqlParameter[] _params = new SqlParameter[15];
			_params[0] = new SqlParameter("@TicketTimeId", ticketTimeId);
			_params[1] = new SqlParameter("@DepartmentID", DeptID);
			_params[2] = new SqlParameter("@TicketID", TicketID);
			_params[3] = new SqlParameter("@UserID", SqlDbType.Int);
			if (UserID != 0) _params[3].Value = UserID;
			else _params[3].Value = DBNull.Value;
			_params[4] = new SqlParameter("@Date", Date);
			_params[5] = new SqlParameter("@Hours", Hours);
			_params[6] = new SqlParameter("@HourlyRate", HourlyRate);
			_params[7] = new SqlParameter("@RemainHours", SqlDbType.Decimal);
			if (RemainHours >= 0) _params[7].Value = RemainHours;
			else _params[7].Value = DBNull.Value;
			_params[8] = new SqlParameter("@Note", SqlDbType.NVarChar);
			if (!String.IsNullOrEmpty(Note)) _params[8].Value = Note;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@StartTime", SqlDbType.SmallDateTime);
			if (StartTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[9].Value = StartTime;
			else _params[9].Value = DBNull.Value;
			_params[10] = new SqlParameter("@StopTime", SqlDbType.SmallDateTime);
			if (StopTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[10].Value = StopTime;
			else _params[10].Value = DBNull.Value;
			_params[11] = new SqlParameter("@UpdatedTime", SqlDbType.SmallDateTime);
			if (updatedTime != DateTime.MinValue) _params[11].Value = updatedTime;
			else _params[11].Value = DBNull.Value;
			_params[12] = new SqlParameter("@UpdatedBy", SqlDbType.Int);
			if (updatedBy != 0) _params[12].Value = updatedBy;
			else _params[12].Value = DBNull.Value;
			_params[13] = new SqlParameter("@TimeOffset", TimeOffset);
			_params[14] = new SqlParameter("@TaskTypeID", SqlDbType.Int);
			if (TaskTypeID != 0) _params[14].Value = TaskTypeID;
			else _params[14].Value = DBNull.Value;
			UpdateData("sp_UpdateTicketTime", _params, OrgId);
		}

        public static void UpdateTicketTime(int ticketTimeId, int DeptID, int TicketID, int UserID, DateTime Date,
            decimal Hours, string Note, decimal HourlyRate, DateTime StartTime, DateTime StopTime,
            int TaskTypeID, decimal RemainHours, DateTime updatedTime, int updatedBy, int TimeOffset)
        {
            UpdateTicketTime(Guid.Empty, ticketTimeId, DeptID, TicketID, UserID, Date,
             Hours, Note, HourlyRate, StartTime, StopTime,
             TaskTypeID, RemainHours, updatedTime, updatedBy, TimeOffset);
        }

		public static DataTable SelectTicketsSearch(int companyID, int userId, int accountID, int projectID)
		{
			return SelectRecords("sp_SelectTicketsSearch", new SqlParameter[] 
			{ 
				new SqlParameter("@DId", companyID),
				new SqlParameter("@UserId", userId),
				new SqlParameter("@AccountID", accountID),
				new SqlParameter("@ProjectID", projectID)
			});
		}

		// Ticket Estimated Time Section
		public static decimal SelectTicketEstimatedTime(int companyID, int ticketID)
		{
			decimal EstTime = -1;
			DataRow dr = SelectRecord("sp_SelectTicketEstimatedTime", new SqlParameter[] 
			{ 
				new SqlParameter("@CompanyID", companyID),
				new SqlParameter("@TicketID", ticketID)
			});
			if (dr != null)
				if (!dr.IsNull("EstimatedTime"))
					EstTime = Convert.ToDecimal(dr["EstimatedTime"].ToString());
			return EstTime;
		}

		public static void UpdateTicketTime(int DeptID, int TicketID, int UserID, decimal Budget)
		{
			SqlParameter[] _params = new SqlParameter[4];
			_params[0] = new SqlParameter("@DepartmentID", DeptID);
			_params[1] = new SqlParameter("@TicketID", TicketID);
			_params[2] = new SqlParameter("@UserID", UserID);
			_params[3] = new SqlParameter("@Budget", SqlDbType.Decimal);
			if (Budget >= 0)
				_params[3].Value = Budget;
			else
				_params[3].Value = DBNull.Value;
			UpdateData("sp_UpdateTicketEstimatedTime", _params);
		}

		public static DataTable SelectBudgetHistory(int DeptID, int TicketId)
		{
			return SelectRecords("sp_SelectTicketEstimatedTimes", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId) });
		}

        public static DataTable SelectTop100MostTimeLogs(Guid OrgId, int dID, int linkedFB, int invoiced, int accountID, int projectID, int techID)
        {
            SqlParameter[] _params = new SqlParameter[6];
            _params[0] = new SqlParameter("@DId", dID);
            _params[1] = new SqlParameter("@LinkedFB", SqlDbType.Bit);
            if (linkedFB >= 0)
                _params[1].Value = linkedFB;
            else
                _params[1].Value = DBNull.Value;
            _params[2] = new SqlParameter("@Invoiced", SqlDbType.Bit);
            if (invoiced >= 0)
                _params[2].Value = invoiced;
            else
                _params[2].Value = DBNull.Value;
            _params[3] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountID == 0)
                _params[3].Value = DBNull.Value;
            else
                _params[3].Value = accountID;
            _params[4] = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectID > 0)
                _params[4].Value = projectID;
            else
                _params[4].Value = DBNull.Value;
            _params[5] = new SqlParameter("@TechID", SqlDbType.Int);
            if (techID > 0)
                _params[5].Value = techID;
            else
                _params[5].Value = DBNull.Value;

            return SelectRecords("sp_SelectTop100MostTimeLogs", _params, OrgId);
        }
	}
}
