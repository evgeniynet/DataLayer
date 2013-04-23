using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;

namespace bigWebApps.bigWebDesk.Data
{
	[DataContract(Name = "Ticket")]
	public class Ticket : DBAccess
	{
		public enum Status
		{
			Open,
			Closed,
			OnHold,
			PartsOnOrder
		}
		private int m_ID = 0;
		private int m_DeptID = 0;
		private int m_UserId = 0;
		private string m_PseudoID = string.Empty;
		private string m_UserFirstName = string.Empty;
		private string m_UserLastName = string.Empty;
		private string m_UserEmail = string.Empty;
		private string m_UserMobileEmail = string.Empty;
		private Data.Logins.EmailType m_UserMobileEmailType = Data.Logins.EmailType.HTML;
		private string m_UserPhone = string.Empty;
		private string m_UserMobilePhone = string.Empty;

		private string m_UserTitle = string.Empty;

		private int m_TechId = 0;
		private string m_TechFirstName = string.Empty;
		private string m_TechLastName = string.Empty;
		private string m_TechEmail = string.Empty;
		private string m_TechMobileEmail = string.Empty;
		private Data.Logins.EmailType m_TechMobileEmailType = Data.Logins.EmailType.HTML;
		private string m_TechPhone = string.Empty;
		private string m_TechMobilePhone = string.Empty;
		private int m_Priority = 0;
		private int m_PriorityId = 0;
		private string m_PriorityName = string.Empty;
		private int m_UserCreatedId = 0;
		private string m_UserCreatedFirstName = string.Empty;
		private string m_UserCreatedLastName = string.Empty;
		private string m_UserCreatedEmail = string.Empty;
		private string m_UserCreatedMobileEmail = string.Empty;
		private Data.Logins.EmailType m_UserCreatedMobileEmailType = Data.Logins.EmailType.HTML;
		private string m_UserCreatedPhone = string.Empty;
		private string m_UserCreatedMobilePhone = string.Empty;
		private Status m_Status = Status.Open;
		private DateTime m_CreateTime = DateTime.MinValue;
		private DateTime m_ClosedTime = DateTime.MinValue;
		private int m_LocationId = 0;
		private string m_LocationName = string.Empty;
		private int m_ClassId = 0;
		private string m_ClassName = string.Empty;
		private int m_ProjectId = 0;
		private string m_ProjectName = string.Empty;
		private string m_SerialNumber = string.Empty;
		private string m_FolderPath = string.Empty;
		private int m_FolderId = 0;
		private int m_CreationCategoryId = 0;
		private string m_CreationCategoryName = string.Empty;
		private string m_Subject = string.Empty;
		private Logins.UserType m_TechType = Logins.UserType.NotSet;
		private string m_Note = string.Empty;
		private string m_TicketNumber = string.Empty;
		private string m_TicketNumberPrefix = string.Empty;
		private string m_CustomFieldsXML = string.Empty;
		private decimal m_PartsCost = 0;
		private decimal m_LaborCost = 0;
		private decimal m_MiscCost = 0;
		private decimal m_TravelCost = 0;
		private DateTime m_RequestCompletionDate = DateTime.MinValue;
		private string m_RequestCompletionNote = string.Empty;
		private DateTime m_FollowUpDate = DateTime.MinValue;
		private string m_FollowUpNote = string.Empty;
		private DateTime m_SLAComplete = DateTime.MinValue;
		private DateTime m_SLAResponse = DateTime.MinValue;
		private bool m_InitResponse = false;
		private int m_SLACompleteUsed = 0;
		private int m_SLAResponseUsed = 0;
		private int m_Level = 0;
		private string m_LevelName = string.Empty;
		private bool m_IsViaEmailParser = false;
		private int m_AccountId = 0;
		private string m_AccountName = string.Empty;
		private int m_AccountLocationId = 0;
		private string m_AccountLocationName = string.Empty;
		private int m_ResolutionCategoryId = 0;
		private string m_ResolutionCategoryName = string.Empty;
		private bool m_IsResolved = false;
		private string m_ConfirmedBy = string.Empty;
		private bool m_IsConfirmed = false;
		private DateTime m_ConfirmedDate = DateTime.MinValue;
		private string m_ConfirmedNote = string.Empty;
		private int m_SupportGroupId = 0;
		private string m_SupportGroupName = string.Empty;
		private string m_IdMethod = string.Empty;
		private bool m_IsHandledByCallCentre = false;
		private string m_SubmissionCategory = string.Empty;
		private bool m_IsUserInactive = false;
		private bool m_IsTicketDeleted = false;
		private int m_TotalTimeMinutes = 0;
		private string m_InitialPost = string.Empty;
		protected LogCollection m_TicketLogs = null;
		private bool m_IsSendNotificationEmail = true;
		private string m_EmailCC = string.Empty;
		private string m_NextStep = string.Empty;
		private decimal m_TotalHours = 0;
		private decimal m_RemainingHours = decimal.MinValue;
		private decimal m_EstimatedTime = 0;
		private string m_Workpad = String.Empty;
		private DateTime m_NextStepDate = DateTime.MinValue;
		private int m_SchedTicketID = 0;
		private Guid m_OrgId = Guid.Empty;

		protected TicketAssignees m_users = null;
		protected TicketAssignees m_technicians = null;

		private int m_RelatedTicketsCount = 0;
		private DateTime m_UpdatedTime = DateTime.MinValue;
		private int m_DaysOld = 0;
		private bool m_KB = false;
		private int m_KBType = 0;
		private int m_KBPublishLevel = 0;
		private string m_KBSearchDesc = string.Empty;
		private string m_KBAlternateId = string.Empty;
		private int m_KBHelpfulCount = 0;
		private string m_KBPortalAlias = string.Empty;
		private int m_OpenTodosCount = 0;
        private bool m_btNoAccount = false;

		public TicketAssignees Users
		{
			get {return m_users;}
			set { m_users = value; }
		}

		public TicketAssignees Technicians
		{
			get { return m_technicians; }
			set { m_technicians = value; }
		}

		public Ticket()
		{
		}

		public Ticket (DataRow tktRow)
		{
			InitTicket(tktRow);
		}

		public Ticket(int DeptID, int TktID)
			: this(Guid.Empty, DeptID, TktID)
		{
		}

		public Ticket(int DeptID, string PseudoTktID) : this(Guid.Empty, DeptID, PseudoTktID)
		{
		}

		public Ticket(Guid OrgID, int DeptID, int TktID)
		{
			m_DeptID = DeptID;
			m_OrgId = OrgID;
			DataRow _row = Tickets.SelectOne(OrgID, DeptID, TktID);
			if (_row != null && string.IsNullOrEmpty(_row["PseudoId"].ToString()))
			{
				UpdatePseudoId(OrgID, DeptID, TktID, Micajah.Common.Bll.Support.GeneratePseudoUnique());
			}
			InitTicket(_row);
		}

		public Ticket(Guid OrgID, int DeptID, string PseudoTktID)
			: this(OrgID, DeptID, PseudoTktID, false)
		{

		}

		public Ticket(Guid OrgID, int DeptID, string PseudoTktID, bool preloadCollections)
		{
			m_DeptID = DeptID;
			m_OrgId = OrgID;
			DataRow _row = Tickets.SelectOne(OrgID, DeptID, PseudoTktID);
			InitTicket(_row);
			if (_row != null)
			{
				LoadCollections(OrgID, DeptID, int.Parse(_row["Id"].ToString()), preloadCollections);
			}
		}

		public Ticket(int DeptID, int TktID, bool preloadCollections)
			: this(Guid.Empty, DeptID, TktID, preloadCollections)
		{
		}

		public Ticket(Guid OrgID, int DeptID, int TktID, bool preloadCollections)
			: this(OrgID, DeptID, TktID)
		{
			LoadCollections(OrgID, DeptID, TktID, preloadCollections);
		}

		private void LoadCollections(Guid OrgID, int DeptID, int TktID, bool preloadCollections)
		{
			if (preloadCollections)
			{
				this.m_users = new TicketAssignees(OrgID, DeptID, TktID, TicketAssignmentType.User, m_UserId);
				this.m_technicians = new TicketAssignees(OrgID, DeptID, TktID, TicketAssignmentType.Technician, m_TechId);
				m_TicketLogs = new LogCollection(OrgID, DeptID, TktID);
			}
			else m_TicketLogs = new LogCollection();
		}

		protected void InitTicket(DataRow TktRow)
		{
			if (TktRow == null)
			{
				m_IsTicketDeleted = true;
				return;
			}
			m_ID = (int)TktRow["Id"];
			m_PseudoID = TktRow["PseudoId"].ToString();
			m_UserId = (int)TktRow["user_id"];
			m_UserFirstName = TktRow["user_firstname"].ToString();
			m_UserLastName = TktRow["user_lastname"].ToString();
			m_UserEmail = TktRow["user_email"].ToString();
			m_UserMobileEmail = TktRow["user_mobileemail"].ToString();
			m_UserMobileEmailType = (Data.Logins.EmailType)(byte)TktRow["user_mobileemailtype"];
			m_IsUserInactive = (bool)TktRow["user_inactive"];
			m_UserTitle = TktRow["user_title"].ToString();
			m_UserPhone = TktRow["user_phone"].ToString();
			m_UserMobilePhone = TktRow["user_mobilephone"].ToString();
			if (!TktRow.IsNull("tintPriority")) m_Priority = (byte)TktRow["tintPriority"];
			if (!TktRow.IsNull("PriorityId")) m_PriorityId = (int)TktRow["PriorityId"];
			m_PriorityName = TktRow["PriName"].ToString();
			m_TechId = (int)TktRow["technician_id"];
			m_TechFirstName = TktRow["technician_firstname"].ToString();
			m_TechLastName = TktRow["technician_lastname"].ToString();
			m_TechMobileEmail = TktRow["technician_mobileemail"].ToString();
			m_TechMobileEmailType = (Data.Logins.EmailType)(byte)TktRow["technician_mobileemailtype"];
			m_TechPhone = TktRow["technician_phone"].ToString();
			m_TechMobilePhone = TktRow["technician_mobilephone"].ToString();
			if (!TktRow.IsNull("Created_id")) m_UserCreatedId = (int)TktRow["Created_id"];
			m_UserCreatedFirstName = TktRow["created_firstname"].ToString();
			m_UserCreatedLastName = TktRow["created_lastname"].ToString();
			m_UserCreatedEmail = TktRow["created_email"].ToString();
			m_UserCreatedMobileEmail = TktRow["created_mobileemail"].ToString();
			if (!TktRow.IsNull("Created_id")) m_UserCreatedMobileEmailType = (Data.Logins.EmailType)(byte)TktRow["created_mobileemailtype"];
			m_UserCreatedPhone = TktRow["created_phone"].ToString();
			m_UserCreatedMobilePhone = TktRow["created_mobilephone"].ToString();
			switch (TktRow["Status"].ToString())
			{
				case "Open":
					m_Status = Status.Open;
					break;
				case "Closed":
					m_Status = Status.Closed;
					break;
				case "On Hold":
					m_Status = Status.OnHold;
					break;
				case "Parts On Order":
					m_Status = Status.PartsOnOrder;
					break;
			}
			m_CreateTime = (DateTime)TktRow["CreateTime"];
			if (!TktRow.IsNull("ClosedTime")) m_ClosedTime = (DateTime)TktRow["ClosedTime"];
			if (!TktRow.IsNull("UpdatedTime")) m_UpdatedTime = (DateTime)TktRow["UpdatedTime"];
			if (!TktRow.IsNull("LocationId")) m_LocationId = (int)TktRow["LocationId"];
			m_LocationName = TktRow["LocationName"].ToString();
			if (!TktRow.IsNull("class_id")) m_ClassId = (int)TktRow["class_id"];
			m_ClassName = TktRow["class_name"].ToString();
			if (!TktRow.IsNull("ProjectID")) m_ProjectId = (int)TktRow["ProjectID"];
			m_ProjectName = TktRow["ProjectName"].ToString();
			m_SerialNumber = TktRow["SerialNumber"].ToString();
			if (!TktRow.IsNull("FolderId")) m_FolderId = (int)TktRow["FolderId"];
			m_FolderPath = TktRow["FolderPath"].ToString();
			if (!TktRow.IsNull("CreationCatsId")) m_CreationCategoryId = (int)TktRow["CreationCatsId"];
			m_CreationCategoryName = TktRow["CategoryName"].ToString();
			m_Subject = TktRow["Subject"].ToString();
			if (!TktRow.IsNull("NextStep")) m_NextStep = TktRow["NextStep"].ToString();
			m_TechType = (Logins.UserType)TktRow["technician_TypeId"];
			if (m_TechType == Logins.UserType.Queue) m_TechEmail = TktRow["vchQueEmailAddress"].ToString();
			else m_TechEmail = TktRow["technician_email"].ToString();
			m_Note = TktRow["note"].ToString();
			m_TicketNumber = TktRow["TicketNumber"].ToString();
			m_TicketNumberPrefix = TktRow["TicketNumberPrefix"].ToString();
			m_CustomFieldsXML = TktRow["CustomXML"].ToString();
			if (!TktRow.IsNull("PartsCost")) m_PartsCost = (decimal)TktRow["PartsCost"];
			if (!TktRow.IsNull("LaborCost")) m_LaborCost = (decimal)TktRow["LaborCost"];
			if (!TktRow.IsNull("MiscCost")) m_MiscCost = (decimal)TktRow["MiscCost"];
			if (!TktRow.IsNull("TravelCost")) m_TravelCost = (decimal)TktRow["TravelCost"];
			if (!TktRow.IsNull("dtReqComp")) m_RequestCompletionDate = (DateTime)TktRow["dtReqComp"];
			m_RequestCompletionNote = TktRow["ReqCompNote"].ToString();
			if (!TktRow.IsNull("dtFollowUp")) m_FollowUpDate = (DateTime)TktRow["dtFollowUp"];
			m_FollowUpNote = TktRow["FollowUpNote"].ToString();
			if (!TktRow.IsNull("dtSLAComplete")) m_SLAComplete = (DateTime)TktRow["dtSLAComplete"];
			if (!TktRow.IsNull("dtSLAResponse")) m_SLAResponse = (DateTime)TktRow["dtSLAResponse"];
			if (!TktRow.IsNull("btInitResponse")) m_InitResponse = (bool)TktRow["btInitResponse"];
			if (!TktRow.IsNull("intSLACompleteUsed")) m_SLACompleteUsed = (int)TktRow["intSLACompleteUsed"];
			if (!TktRow.IsNull("intSLAResponseUsed")) m_SLAResponseUsed = (int)TktRow["intSLAResponseUsed"];
			if (!TktRow.IsNull("tintLevel")) m_Level = (byte)TktRow["tintLevel"];
			m_LevelName = TktRow["LevelName"].ToString();
			if (!TktRow.IsNull("btViaEmailParser")) m_IsViaEmailParser = (bool)TktRow["btViaEmailParser"];
			if (!TktRow.IsNull("intAcctId")) m_AccountId = (int)TktRow["intAcctId"];
			if (!TktRow.IsNull("AccountLocationId")) m_AccountLocationId = (int)TktRow["AccountLocationId"];
			m_AccountName = TktRow["vchAcctName"].ToString();
			m_AccountLocationName = TktRow["AccountLocationName"].ToString();
			if (!TktRow.IsNull("ResolutionCatsId")) m_ResolutionCategoryId = (int)TktRow["ResolutionCatsId"];
			m_ResolutionCategoryName = TktRow["ResolutionName"].ToString();
			if (!TktRow.IsNull("btResolved")) m_IsResolved = (bool)TktRow["btResolved"];
			m_ConfirmedBy = TktRow["ConfirmedBy"].ToString();
			if (!TktRow.IsNull("btConfirmed")) m_IsConfirmed = (bool)TktRow["btConfirmed"];
			if (!TktRow.IsNull("dtConfirmed")) m_ConfirmedDate = (DateTime)TktRow["dtConfirmed"];
			m_ConfirmedNote = TktRow["vchConfirmedNote"].ToString();
			if (!TktRow.IsNull("SupportGroupId")) m_SupportGroupId = (int)TktRow["SupportGroupId"];
			m_SupportGroupName = TktRow["SupportGroupName"].ToString();
			m_IdMethod = TktRow["TicketIdMethod"].ToString();
			if (!TktRow.IsNull("btHandledByCallCentre")) m_IsHandledByCallCentre = (bool)TktRow["btHandledByCallCentre"];
			m_SubmissionCategory = TktRow["SubmissionCategory"].ToString();
			if (!TktRow.IsNull("intTktTimeMin")) m_TotalTimeMinutes = (int)TktRow["intTktTimeMin"];
			m_EmailCC = TktRow["EmailCC"].ToString();
			m_RelatedTicketsCount = (int)TktRow["RelatedTktsCount"];
			if (!TktRow.IsNull("TotalHours")) m_TotalHours = (decimal)TktRow["TotalHours"];
			if (!TktRow.IsNull("RemainingHours")) m_RemainingHours = (decimal)TktRow["RemainingHours"];
			if (!TktRow.IsNull("EstimatedTime")) m_EstimatedTime = (decimal)TktRow["EstimatedTime"];
			m_Workpad = TktRow["Workpad"].ToString();
			if (!TktRow.IsNull("NextStepDate")) m_NextStepDate = (DateTime)TktRow["NextStepDate"];
			if (!TktRow.IsNull("SchedTicketID")) m_SchedTicketID = (int)TktRow["SchedTicketID"];
			m_InitialPost = TktRow["InitPost"].ToString();
			if (TktRow.Table.Columns.Contains("DaysOld") && !TktRow.IsNull("DaysOld")) m_DaysOld = (int)TktRow["DaysOld"];
			if (!TktRow.IsNull("KB")) m_KB = (bool)TktRow["KB"];
			if (!TktRow.IsNull("KBType")) m_KBType = (byte)TktRow["KBType"];
			if (!TktRow.IsNull("KBPublishLevel")) m_KBPublishLevel = (byte)TktRow["KBPublishLevel"];
			if (!TktRow.IsNull("KBSearchDesc")) m_KBSearchDesc = TktRow["KBSearchDesc"].ToString();
			if (!TktRow.IsNull("KBAlternateId")) m_KBAlternateId = TktRow["KBAlternateId"].ToString();
			if (!TktRow.IsNull("KBHelpfulCount")) m_KBHelpfulCount = (int)TktRow["KBHelpfulCount"];
            if (!TktRow.IsNull("btNoAccount")) m_btNoAccount = (bool)TktRow["btNoAccount"];
			m_KBPortalAlias = TktRow["KBPortalAlias"].ToString();
			m_OpenTodosCount = (int)TktRow["OpenTodosCount"];
		}

		public int ID
		{
			get { return m_ID; }
			set { m_ID = value; }
		}

		[DataMember]
		public string PseudoID
		{
			get { return m_PseudoID; }
			set { m_PseudoID = value; }
		}

		[DataMember]
		public Guid OrganizationId
		{
			get { return m_OrgId; }
			set { m_OrgId = value; }
		}

		[DataMember]
		public int DepartmentID
		{
			get { return m_DeptID; }
			set { m_DeptID = value; }
		}

		[DataMember]
		public bool IsDeleted
		{
			get { return m_IsTicketDeleted; }
			set { m_IsTicketDeleted = value; }
		}

		[DataMember]
		public int UserId
		{
			get { return m_UserId; }
			set { m_UserId = value; }
		}

		[DataMember]
		public string UserTitle
		{
			get { return m_UserTitle; }
			set { m_UserTitle = value; }
		}

		[DataMember]
		public string UserFirstName
		{
			get { return m_UserFirstName; }
			set { m_UserFirstName = value; }
		}

		[DataMember]
		public string UserLastName
		{
			get { return m_UserLastName; }
			set { m_UserLastName = value; }
		}

		[DataMember]
		public string UserEmail
		{
			get { return m_UserEmail; }
			set { m_UserEmail = value; }
		}

		[DataMember]
		public string UserMobileEmail
		{
			get { return m_UserMobileEmail; }
			set { m_UserMobileEmail = value; }
		}

		[DataMember]
		public Data.Logins.EmailType UserMobileEmailType
		{
			get { return m_UserMobileEmailType; }
			set { m_UserMobileEmailType = value; }
		}

		[DataMember]
		public string UserPhone
		{
			get { return m_UserPhone; }
			set { m_UserPhone = value; }
		}

		[DataMember]
		public string UserMobilePhone
		{
			get { return m_UserMobilePhone; }
			set { m_UserMobilePhone = value; }
		}

		[DataMember]
		public int TechnicianId
		{
			get { return m_TechId; }
			set { m_TechId = value; }
		}

		[DataMember]
		public string TechnicianFirstName
		{
			get { return m_TechFirstName; }
			set { m_TechFirstName = value; }
		}

		[DataMember]
		public string TechnicianLastName
		{
			get { return m_TechLastName; }
			set { m_TechLastName = value; }
		}

		[DataMember]
		public string TechnicianEmail
		{
			get { return m_TechEmail; }
			set { m_TechEmail = value; }
		}

		[DataMember]
		public string TechnicianMobileEmail
		{
			get { return m_TechMobileEmail; }
			set { m_TechMobileEmail = value; }
		}

		[DataMember]
		public Data.Logins.EmailType TechnicianMobileEmailType
		{
			get { return m_TechMobileEmailType; }
			set { m_TechMobileEmailType = value; }
		}

		[DataMember]
		public string TechnicianPhone
		{
			get { return m_TechPhone; }
			set { m_TechPhone = value; }
		}

		[DataMember]
		public string TechnicianMobilePhone
		{
			get { return m_TechMobilePhone; }
			set { m_TechMobilePhone = value; }
		}

		[DataMember]
		public int Priority
		{
			get { return m_Priority; }
			set { m_Priority = value; }
		}

		public int PriorityId
		{
			get { return m_PriorityId; }
			set { m_PriorityId = value; }
		}

		[DataMember]
		public string PriorityName
		{
			get { return m_PriorityName; }
			set { m_PriorityName = value; }
		}

		[DataMember]
		public int UserCreatedId
		{
			get { return m_UserCreatedId; }
			set { m_UserCreatedId = value; }
		}

		[DataMember]
		public string UserCreatedFirstName
		{
			get { return m_UserCreatedFirstName; }
			set { m_UserCreatedFirstName = value; }
		}

		[DataMember]
		public string UserCreatedLastName
		{
			get { return m_UserCreatedLastName; }
			set { m_UserCreatedLastName = value; }
		}

		[DataMember]
		public string UserCreatedEmail
		{
			get { return m_UserCreatedEmail; }
			set { m_UserCreatedEmail = value; }
		}

		[DataMember]
		public string UserCreatedMobileEmail
		{
			get { return m_UserCreatedMobileEmail; }
			set { m_UserCreatedMobileEmail = value; }
		}

		[DataMember]
		public Data.Logins.EmailType UserCreatedMobileEmailType
		{
			get { return m_UserCreatedMobileEmailType; }
			set { m_UserCreatedMobileEmailType = value; }
		}

		[DataMember]
		public string UserCreatedPhone
		{
			get { return m_UserCreatedPhone; }
			set { m_UserCreatedPhone = value; }
		}

		[DataMember]
		public string UserCreatedMobilePhone
		{
			get { return m_UserCreatedMobilePhone; }
			set { m_UserCreatedMobilePhone = value; }
		}

		[DataMember]
		public Status TicketStatus
		{
			get { return m_Status; }
			set { m_Status = value; }
		}

		public DateTime CreateTime
		{
			get { return m_CreateTime; }
			set { m_CreateTime = value; }
		}

		public DateTime ClosedTime
		{
			get { return m_ClosedTime; }
			set { m_ClosedTime = value; }
		}

		[DataMember]
		public int LocationId
		{
			get { return m_LocationId; }
			set { m_LocationId = value; }
		}

		[DataMember]
		public string LocationName
		{
			get { return m_LocationName; }
			set { m_LocationName = value; }
		}

		[DataMember]
		public int ClassId
		{
			get { return m_ClassId; }
			set { m_ClassId = value; }
		}

		[DataMember]
		public string ClassName
		{
			get { return m_ClassName; }
			set { m_ClassName = value; }
		}

		[DataMember]
		public int ProjectId
		{
			get { return m_ProjectId; }
			set { m_ProjectId = value; }
		}

		[DataMember]
		public string ProjectName
		{
			get { return m_ProjectName; }
			set { m_ProjectName = value; }
		}

		[DataMember]
		public string SerialNumber
		{
			get { return m_SerialNumber; }
			set { m_SerialNumber = value; }
		}

		[DataMember]
		public int FolderId
		{
			get { return m_FolderId; }
			set { m_FolderId = value; }
		}

		[DataMember]
		public string FolderPath
		{
			get { return m_FolderPath; }
			set { m_FolderPath = value; }
		}

		[DataMember]
		public int CreationCategoryId
		{
			get { return m_CreationCategoryId; }
			set { m_CreationCategoryId = value; }
		}

		[DataMember]
		public string CreationCategoryName
		{
			get { return m_CreationCategoryName; }
			set { m_CreationCategoryName = value; }
		}

		[DataMember]
		public string Subject
		{
			get { return m_Subject; }
			set { m_Subject = value; }
		}

		[DataMember]
		public Logins.UserType TechnicianType
		{
			get { return m_TechType; }
			set { m_TechType = value; }
		}

		[DataMember]
		public string Note
		{
			get { return m_Note; }
			set { m_Note = value; }
		}

		[DataMember]
		public string TicketNumber
		{
			get { return m_TicketNumber; }
			set { m_TicketNumber = value; }
		}

		[DataMember]
		public string TicketNumberPrefix
		{
			get { return m_TicketNumberPrefix; }
			set { m_TicketNumberPrefix = value; }
		}

		[DataMember]
		public string CustomFieldsXML
		{
			get { return m_CustomFieldsXML; }
			set { m_CustomFieldsXML = value; }
		}

		[DataMember]
		public decimal PartsCost
		{
			get { return m_PartsCost; }
			set { m_PartsCost = value; }
		}

		[DataMember]
		public decimal LaborCost
		{
			get { return m_LaborCost; }
			set { m_LaborCost = value; }
		}

		[DataMember]
		public int TotalTimeMinutes
		{
			get { return m_TotalTimeMinutes; }
			set { m_TotalTimeMinutes = value; }
		}

		[DataMember]
		public decimal MiscCost
		{
			get { return m_MiscCost; }
			set { m_MiscCost = value; }
		}

		[DataMember]
		public decimal TravelCost
		{
			get { return m_TravelCost; }
			set { m_TravelCost = value; }
		}

		public DateTime RequestCompletionDate
		{
			get { return m_RequestCompletionDate; }
			set { m_RequestCompletionDate = value; }
		}

		[DataMember]
		public string RequestCompletionNote
		{
			get { return m_RequestCompletionNote; }
			set { m_RequestCompletionNote = value; }
		}

		public DateTime FollowUpDate
		{
			get { return m_FollowUpDate; }
			set { m_FollowUpDate = value; }
		}

		[DataMember]
		public string FollowUpNote
		{
			get { return m_FollowUpNote; }
			set { m_FollowUpNote = value; }
		}

		public DateTime SLAComplete
		{
			get { return m_SLAComplete; }
			set { m_SLAComplete = value; }
		}

		public DateTime SLARespose
		{
			get { return m_SLAResponse; }
			set { m_SLAResponse = value; }
		}

		[DataMember]
		public bool InitResponse
		{
			get { return m_InitResponse; }
			set { m_InitResponse = value; }
		}

		[DataMember]
		public int SLACompleteUsed
		{
			get { return m_SLACompleteUsed; }
			set { m_SLACompleteUsed = value; }
		}

		[DataMember]
		public int SLAResponseUsed
		{
			get { return m_SLAResponseUsed; }
			set { m_SLAResponseUsed = value; }
		}

		[DataMember]
		public int Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		[DataMember]
		public string LevelName
		{
			get { return m_LevelName; }
			set { m_LevelName = value; }
		}

		[DataMember]
		public bool IsViaEmailParser
		{
			get { return m_IsViaEmailParser; }
			set { m_IsViaEmailParser = value; }
		}

		[DataMember]
		public int AccountId
		{
			get { return m_AccountId; }
			set { m_AccountId = value; }
		}

		[DataMember]
		public string AccountName
		{
			get { return m_AccountName; }
			set { m_AccountName = value; }
		}

		[DataMember]
		public int AccountLocationId
		{
			get { return m_AccountLocationId; }
			set { m_AccountLocationId = value; }
		}

		[DataMember]
		public string AccountLocationName
		{
			get { return m_AccountLocationName; }
			set { m_AccountLocationName = value; }
		}

		[DataMember]
		public int ResolutionCategoryId
		{
			get { return m_ResolutionCategoryId; }
			set { m_ResolutionCategoryId = value; }
		}

		[DataMember]
		public string ResolutionCategoryName
		{
			get { return m_ResolutionCategoryName; }
			set { m_ResolutionCategoryName = value; }
		}

		[DataMember]
		public bool IsResolved
		{
			get { return m_IsResolved; }
			set { m_IsResolved = value; }
		}

		[DataMember]
		public string ConfirmedBy
		{
			get { return m_ConfirmedBy; }
			set { m_ConfirmedBy = value; }
		}

		[DataMember]
		public bool IsConfirmed
		{
			get { return m_IsConfirmed; }
			set { m_IsConfirmed = value; }
		}

		public DateTime ConfirmedDate
		{
			get { return m_ConfirmedDate; }
			set { m_ConfirmedDate = value; }
		}

		[DataMember]
		public string ConfirmedNote
		{
			get { return m_ConfirmedNote; }
			set { m_ConfirmedNote = value; }
		}

		[DataMember]
		public int SupportGroupId
		{
			get { return m_SupportGroupId; }
			set { m_SupportGroupId = value; }
		}

		[DataMember]
		public string SupportGroupName
		{
			get { return m_SupportGroupName; }
			set { m_SupportGroupName = value; }
		}

		[DataMember]
		public string IDMethod
		{
			get { return m_IdMethod; }
			set { m_IdMethod = value; }
		}

		[DataMember]
		public bool IsHandleByCallCentre
		{
			get { return m_IsHandledByCallCentre; }
			set { m_IsHandledByCallCentre = value; }
		}

		[DataMember]
		public string SubmissionCategory
		{
			get { return m_SubmissionCategory; }
			set { m_SubmissionCategory = value; }
		}

		[DataMember]
		public bool IsUserInactive
		{
			get { return m_IsUserInactive; }
			set { m_IsUserInactive = value; }
		}

		[DataMember]
		public string NextStep
		{
			get { return m_NextStep; }
			set { m_NextStep = value; }
		}

		[DataMember]
		public decimal TotalHours
		{
			get { return m_TotalHours; }
			set { m_TotalHours = value; }
		}

		[DataMember]
		public decimal RemainingHours
		{
			get { return m_RemainingHours; }
			set { m_RemainingHours = value; }
		}

		[DataMember]
		public decimal EstimatedTime
		{
			get { return m_EstimatedTime; }
			set { m_EstimatedTime = value; }
		}

		[DataMember]
		public string Workpad
		{
			get { return m_Workpad; }
			set { m_Workpad = value; }
		}

		public DateTime NextStepDate
		{
			get { return m_NextStepDate; }
			set { m_NextStepDate = value; }
		}

		[DataMember]
		public int SchedTicketID
		{
			get { return m_SchedTicketID; }
			set { m_SchedTicketID = value; }
		}

		public DateTime UpdatedTime
		{
			get { return m_UpdatedTime; }
			set { m_UpdatedTime = value; }
		}

		[DataMember]
		public bool KB
		{
			get { return m_KB; }
			set { m_KB = value; }
		}

		[DataMember]
		public int KBType
		{
			get { return m_KBType; }
			set { m_KBType = value; }
		}

		[DataMember]
		public int KBPublishLevel
		{
			get { return m_KBPublishLevel; }
			set { m_KBPublishLevel = value; }
		}

		[DataMember]
		public string KBSearchDesc
		{
			get { return m_KBSearchDesc; }
			set { m_KBSearchDesc = value; }
		}

		[DataMember]
		public string KBAlternateId
		{
			get { return m_KBAlternateId; }
			set { m_KBAlternateId = value; }
		}

		[DataMember]
		public int KBHelpfulCount
		{
			get { return m_KBHelpfulCount; }
			set { m_KBHelpfulCount = value; }
		}

		[DataMember]
		public string KBPortalAlias
		{
			get { return m_KBPortalAlias; }
			set { m_KBPortalAlias = value; }
		}

        [DataMember]
        public bool NoAccount
        {
            get { return m_btNoAccount; }
            set { m_btNoAccount = value; }
        }

		[DataMember]
		public string InitialPost
		{
			get { return m_InitialPost; }
			set { m_InitialPost = value; }
		}

		[DataMember]
		public bool IsSendNotificationEmail
		{
			get { return m_IsSendNotificationEmail; }
			set { m_IsSendNotificationEmail = value; }
		}

		[DataMember]
		public string EmailCC
		{
			get { return m_EmailCC; }
			set { m_EmailCC = value; }
		}

		[DataMember]
		public int RelatedTicketsCount
		{
			get { return m_RelatedTicketsCount; }
			set { m_RelatedTicketsCount = value; }
		}

		[DataMember]
		public int DaysOldInMinutes
		{
			get { return m_DaysOld; }
			set { m_DaysOld = value; }
		}

		public int OpenTodosCount
		{
			get { return m_OpenTodosCount; }
			set { m_OpenTodosCount = value; }
		}

		public LogCollection TicketLogs
		{
			get { return m_TicketLogs; }
			set { m_TicketLogs = value; }
		}

		public static void UpdateSubject(Guid OrgId, int DeptID, int TktID, string Subject, string nextStep)
		{
			if (Subject.Length > 100) Subject = Subject.Substring(0, 100);
			if (nextStep.Length > 100) nextStep = nextStep.Substring(0, 100);

			UpdateData("sp_UpdateTicketSubject", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@Subject", Subject), new SqlParameter("@NextStep", nextStep) }, OrgId);
		}

		public static void UpdateSubject(int DeptID, int TktID, string Subject, string nextStep)
		{
			UpdateSubject(Guid.Empty, DeptID, TktID, Subject, nextStep);
		}

		public static void UpdateCreationTime(int DeptID, int TktID, DateTime TktDateTime)
		{
			UpdateData("sp_UpdateTktCreationTime", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), new SqlParameter("@dtCreationTime", TktDateTime) });
		}

		public static void UpdateFollowUpDate(int DeptID, int TktID, DateTime FollowUpDateTime, string FollowUpNote)
		{
			SqlParameter _pFollowUpDate = new SqlParameter("@dtFollowUp", SqlDbType.SmallDateTime);
			if (FollowUpDateTime == DateTime.MinValue) _pFollowUpDate.Value = DBNull.Value;
			else _pFollowUpDate.Value = FollowUpDateTime;
			SqlParameter _pFollowupNote = new SqlParameter("@vchFollowUpNote", SqlDbType.NVarChar, 50);
			if (FollowUpNote.Length > 0) _pFollowupNote.Value = FollowUpNote;
			else _pFollowupNote.Value = DBNull.Value;
			UpdateData("sp_UpdateTktFUD", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), _pFollowUpDate, _pFollowupNote });
		}

		public static void UpdateClass(int DeptID, int TktID, int ClassID)
		{
			UpdateByQuery("UPDATE tbl_ticket SET class_id = " + ClassID.ToString() + ", UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateProject(int DeptID, int TktID, int ProjectID)
		{
			if (ProjectID == 0) UpdateByQuery("UPDATE tbl_ticket SET ProjectID = NULL, UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
			else UpdateByQuery("UPDATE tbl_ticket SET ProjectID = " + ProjectID.ToString() + ", UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateCreationCategory(int DeptID, int TktID, int CreationCategoryID)
		{
			SqlParameter _pCategoryId = new SqlParameter("@CategoryId", SqlDbType.Int);
			if (CreationCategoryID != 0) _pCategoryId.Value = CreationCategoryID;
			else _pCategoryId.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketCategory", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), _pCategoryId });
		}

		public static void UpdateSerialNumber(int DeptID, int TktID, string SerialNumber)
		{
			if (SerialNumber.Length > 0) UpdateByQuery("UPDATE tbl_ticket SET SerialNumber = '" + Security.SQLInjectionBlock(SerialNumber.Replace("'", "''")) + "', UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
			else UpdateByQuery("UPDATE tbl_ticket SET SerialNumber = NULL, UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateResolution(int DeptID, int TktID, int ResolutionId, bool IsResolved)
		{
			UpdateResolution(Guid.Empty, DeptID, TktID, ResolutionId, IsResolved);
		}

		public static void UpdateResolution(Guid OrgId, int DeptID, int TktID, int ResolutionId, bool IsResolved)
		{
			SqlParameter _pResolutionId = new SqlParameter("@ResolutionId", SqlDbType.Int);
			if (ResolutionId != 0) _pResolutionId.Value = ResolutionId;
			else _pResolutionId.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketResolution", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), _pResolutionId, new SqlParameter("@btResolved", IsResolved) }, OrgId);
		}


		public static void UpdateConfirmation(Guid OrgId, int DeptID, int TktID, int UserID, bool IsConfirmed, string ConfirmationNote)
		{
			SqlParameter _pConfirmationNote = new SqlParameter("@vchConfirmedNote", SqlDbType.NVarChar, 254);
			if (ConfirmationNote.Length > 0) _pConfirmationNote.Value = ConfirmationNote;
			else _pConfirmationNote.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketConfirmation", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@UId", UserID), new SqlParameter("@btConfirmed", IsConfirmed), _pConfirmationNote }, OrgId);
		}

		public static void UpdateConfirmation(int DeptID, int TktID, int UserID, bool IsConfirmed, string ConfirmationNote)
		{
			UpdateConfirmation(Guid.Empty, DeptID, TktID, UserID, IsConfirmed, ConfirmationNote);
		}

		public static void UpdateIDMethod(int DeptID, int TktID, string IDMethod)
		{
			SqlParameter _pIDMethod = new SqlParameter("@vchIdMethod", SqlDbType.NVarChar, 255);
			if (IDMethod.Length > 0) _pIDMethod.Value = IDMethod;
			else _pIDMethod.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketIDMethod", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TktID), _pIDMethod });
		}

		public static void UpdateFolder(int DeptID, int TktID, int FolderId)
		{
			SqlParameter _pFolderId = new SqlParameter("@FolderId", SqlDbType.Int);
			if (FolderId != 0) _pFolderId.Value = FolderId;
			else _pFolderId.Value = DBNull.Value;
			UpdateData("sp_UpdateTicketFolder", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), _pFolderId });
		}

		public static void UpdateEndUser(Guid OrgId, int DeptID, int TktID, int UserId)
		{
			UpdateData("sp_UpdateTicketEndUser", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@UserId", UserId) }, OrgId);
		}

		public static void UpdateEndUser(int DeptID, int TktID, int UserId)
		{
			UpdateEndUser(Guid.Empty, DeptID, TktID, UserId);
		}

		public static void UpdateHandledByCallCentre(int DeptID, int TktID, bool IsHandledByCallCentre)
		{
			UpdateData("sp_UpdateTicketHandledCC", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@btHandledByCallCentre", IsHandledByCallCentre) });
		}

		public static void UpdateSubmissionCategory(int DeptID, int TktID, string SubmissionCategory)
		{
			UpdateData("sp_UpdateTicketSubmissionCat", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@vchSubmissionCat", SubmissionCategory) });
		}

		public static void UpdateRequestCompletionDate(int DeptID, int TktID, DateTime RequestCompletionDate, string RequestCompletionNote)
		{
			SqlParameter _pRequestCompletionDate = new SqlParameter("@dtReqComp", SqlDbType.SmallDateTime);
			if (RequestCompletionDate == DateTime.MinValue) _pRequestCompletionDate.Value = DBNull.Value;
			else _pRequestCompletionDate.Value = RequestCompletionDate;
			SqlParameter _pRequestCompletionNote = new SqlParameter("@vchReqComp", SqlDbType.NVarChar, 50);
			if (RequestCompletionNote.Length > 0) _pRequestCompletionNote.Value = RequestCompletionNote;
			else _pRequestCompletionNote.Value = DBNull.Value;
			UpdateData("sp_UpdateTktRCD", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), _pRequestCompletionDate, _pRequestCompletionNote });
		}

		public static void InsertResponse(Guid orgId, int departmentId, int ticketId, int userId, bool pickUp, bool userTypeUser, bool placeOnHold, string logText, string systemGeneratedText, int timeLogId, string[] uploadedFileNames, string to, bool reopenClosedOnHold)
		{
            if (uploadedFileNames != null && uploadedFileNames.Length > 0)
                systemGeneratedText = (string.IsNullOrEmpty(systemGeneratedText) ? string.Empty : systemGeneratedText + Environment.NewLine) + GetFilesUpdatedLogEntry(uploadedFileNames).Message;
			InsertResponse(orgId, departmentId, ticketId, userId, pickUp, userTypeUser, placeOnHold, logText, systemGeneratedText, timeLogId, to, reopenClosedOnHold);
		}

        public static string InsertResponse(Guid OrgId, int DeptID, int TktID, int UserId, bool PickUp, bool UserTypeUser, bool PlaceOnHold, string LogText, string systemGeneratedText = "", int TimeLogID = 0, string To = null, bool reopenClosedOnHold = true)
		{
			SqlParameter _pLogText = new SqlParameter("@LogText", SqlDbType.NVarChar, -1);
			_pLogText.Direction = ParameterDirection.InputOutput;
			if (LogText.Length > 4899) LogText = "--Text truncated at 5000 characters--<br><br>" + LogText.Substring(0, 4800) + "<br><br>--Text truncated at 5000 characters--";
			if (LogText.Length > 0) _pLogText.Value = LogText;
			else _pLogText.Value = DBNull.Value;
			SqlParameter _pEmailSubject = new SqlParameter("@EmailSubject", SqlDbType.NVarChar, 100);
			_pEmailSubject.Direction = ParameterDirection.InputOutput;
			SqlParameter _pUserFullName = new SqlParameter("@UserFullName", SqlDbType.NVarChar, 100);
			_pUserFullName.Direction = ParameterDirection.InputOutput;

			SqlParameter pSystemGeneratedText = new SqlParameter("@SystemGeneratedText", SqlDbType.NVarChar, -1);
			if (string.IsNullOrEmpty(systemGeneratedText))
				pSystemGeneratedText.Value = DBNull.Value;
			else
				pSystemGeneratedText.Value = systemGeneratedText;

			SqlParameter _pTo = new SqlParameter("@To", SqlDbType.NVarChar, 1000);
			if (string.IsNullOrEmpty(To))
				_pTo.Value = DBNull.Value;
			else
				_pTo.Value = To;
			SqlParameter _TimeEntryID = new SqlParameter("@TimeEntryID", SqlDbType.Int);
			if (TimeLogID > 0) _TimeEntryID.Value = TimeLogID;
			else _TimeEntryID.Value = DBNull.Value;
			UpdateData("sp_UpdateTktResponse", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), new SqlParameter("@UId", UserId), new SqlParameter("@btPickup", PickUp), new SqlParameter("@btUserTypeUser", UserTypeUser), new SqlParameter("@btPlaceOnHold", PlaceOnHold), _pLogText, pSystemGeneratedText, _pEmailSubject, _pUserFullName, _TimeEntryID, _pTo, new SqlParameter("@ReopenClosedOnHold", reopenClosedOnHold) }, OrgId);
			return _pLogText.Value.ToString();
		}

		public enum UpdateMode : int
		{
			Append = 1,
			Prepend = 2,
			Replace = 3
		}

		private struct FilesUploadedLogEntry
		{
			private string title;
			private string message;
			public FilesUploadedLogEntry(string title, string message)
			{
				this.title = title;
				this.message = message;
			}

			public string Title
			{
				get { return title; }
			}

			public string Message
			{
				get { return message; }
			}
		}

		private static FilesUploadedLogEntry GetFilesUpdatedLogEntry(string[] uploadedFileNames)
		{
			return GetFilesUpdatedLogEntry(uploadedFileNames, null);
		}

		private static FilesUploadedLogEntry GetFilesUpdatedLogEntry(string[] uploadedFileNames, string[] deletedFileNames)
		{
			int deletedFilesCount = deletedFileNames != null ? deletedFileNames.Length : 0;
			int uploadedFilesCount = uploadedFileNames != null ? uploadedFileNames.Length : 0;

			string title = "File";
			title += (deletedFilesCount + uploadedFilesCount > 1) ? "s were " : " was ";

			string message = string.Empty;

			if (uploadedFilesCount > 0)
			{
				title += "uploaded ";
				message += "Following file" + (uploadedFilesCount > 1 ? "s were " : " was ") + " uploaded:";
				foreach (string fileName in uploadedFileNames)
					message += " " + fileName + ",";
				message = message.Remove(message.Length - 1, 1) + ".";
			}

			if (deletedFilesCount > 0 && uploadedFilesCount > 0)
			{
				title += "& ";
				message += Environment.NewLine;
			}

			if (deletedFilesCount > 0)
			{
				title += "deleted";
				message += "Following file" + (deletedFilesCount > 1 ? "s were " : " was ") + " deleted:";
				foreach (string fileName in deletedFileNames)
					message += " " + fileName + ",";
				message = message.Remove(message.Length - 1, 1) + ".";
			}

			return new FilesUploadedLogEntry(title, message);
		}

		public static void UpdateLogMessageWithFileChanges(int departmentId, int logEntryId, string[] fileNames)
		{
			UpdateLogMessageWithFileChanges(Guid.Empty, departmentId,  logEntryId, fileNames);
		}

		public static void UpdateLogMessageWithFileChanges(Guid OrgID, int departmentId, int logEntryId, string[] fileNames)
		{
			if (fileNames.Length > 0)
				UpdateLogMessage(OrgID, departmentId, logEntryId, GetFilesUpdatedLogEntry(fileNames).Message,
								 UpdateMode.Append);
		}

		public static void UpdateLogMessage(Guid OrgID, int departmentId, int logEntryId, string logEntryText, UpdateMode updateMode)
		{
			if (logEntryId > 0 && !string.IsNullOrEmpty(logEntryText))
				UpdateData("sp_UpdateTicketLogEntry", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@LogEntryId", logEntryId), new SqlParameter("@LogEntryText", logEntryText), new SqlParameter("@UpdateMode", (int)updateMode) }, OrgID);
		}

		public static void UpdateRemoteSession(Guid OrgID, int DeptID, int TktID, string SID, bool IsStart)
		{
			if (IsStart)
			{
				if (TktID != 0) UpdateByQuery("INSERT INTO RemoteSessions(DId, TId, SId, SStart) VALUES (" + DeptID.ToString() + ", " + TktID.ToString() + ", '" + SID.ToString().Replace("'", "''") + "', GETUTCDATE())", OrgID);
				else UpdateByQuery("INSERT INTO RemoteSessions(DId, TId, SId, SStart) VALUES (" + DeptID.ToString() + ", NULL, '" + SID.ToString().Replace("'", "''") + "', GETUTCDATE())", OrgID);
			}
			else
			{
				UpdateByQuery("UPDATE RemoteSessions SET SIsCompleted=1, SEnd=GETUTCDATE() WHERE SId='" + SID.ToString().Replace("'", "''") + "'", OrgID);
			}
		}

		public static void InsertLogMessage(int departmentId, int userId, int ticketId, string[] uploadedFileNames)
		{
			InsertLogMessage(departmentId, userId, ticketId, uploadedFileNames, null);
		}

		public static void InsertLogMessage(int departmentId, int userId, int ticketId, string[] uploadedFileNames, string[] deletedFileNames)
		{
			if ((uploadedFileNames != null && uploadedFileNames.Length > 0) || (deletedFileNames != null && deletedFileNames.Length > 0))
			{
				FilesUploadedLogEntry filesUploadedLogEntry = GetFilesUpdatedLogEntry(uploadedFileNames, deletedFileNames);
				InsertLogMessage(departmentId, ticketId, userId, filesUploadedLogEntry.Title, string.Empty, filesUploadedLogEntry.Message);
			}
		}

		public static void InsertLogMessage(int DeptID, int TktID, int UserId, string LogType, string LogText, string systemGeneratedText)
		{
			InsertLogMessage(Guid.Empty, DeptID, TktID, UserId, LogType, LogText, systemGeneratedText);
		}

		public static void InsertLogMessage(Guid OrgID, int DeptID, int TktID, int UserId, string LogType, string LogText, string systemGeneratedMessage)
		{
			InsertLogMessage(OrgID, DeptID, TktID, UserId, LogType, LogText, systemGeneratedMessage, 0);
		}

		public static void InsertLogMessage(int departmentId, int ticketId, int userId, string logType, string logText, int timeLogId, string[] uploadedFileNames)
		{
			string systemGeneratedText = string.Empty;
			if (uploadedFileNames != null && uploadedFileNames.Length > 0)
				systemGeneratedText = GetFilesUpdatedLogEntry(uploadedFileNames).Message;

			InsertLogMessage(departmentId, ticketId, userId, logType, logText, systemGeneratedText, timeLogId);
		}

		public static void InsertLogMessage(int DeptID, int TktID, int UserId, string LogType, string LogText, string  systemGeneratedText, int TimeLogID)
		{
			InsertLogMessage(Guid.Empty, DeptID, TktID, UserId, LogType, LogText, systemGeneratedText, TimeLogID);
		}

		private static void InsertLogMessage(Guid OrgID, int DeptID, int TktID, int UserId, string LogType, string LogText, string systemGeneratedText, int TimeLogID)
		{
			if (LogText.Length > 4999) LogText = "--Text truncated at 5000 characters--<br><br>" + LogText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
			SqlParameter _pNote = new SqlParameter("@vchNote", SqlDbType.NVarChar, -1);
			if (LogText.Length > 0) _pNote.Value = LogText;
			else _pNote.Value = DBNull.Value;
			
			SqlParameter pSystemGeneratedText = new SqlParameter("@SystemGeneratedText", SqlDbType.NVarChar, -1);
			if (!string.IsNullOrEmpty(systemGeneratedText)) pSystemGeneratedText.Value = systemGeneratedText;
			else pSystemGeneratedText.Value = DBNull.Value;

			SqlParameter _TimeEntryID = new SqlParameter("@TimeEntryID", SqlDbType.Int);
			if (TimeLogID > 0) _TimeEntryID.Value = TimeLogID;
			else _TimeEntryID.Value = DBNull.Value;
			UpdateData("sp_InsertTktLog", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TktID), new SqlParameter("@UId", UserId), new SqlParameter("@vchType", LogType), _pNote, pSystemGeneratedText, _TimeEntryID }, OrgID);
		}


		public enum TicketAssignmentType
		{
			User = 1,
			Technician = 2
		}

		public static DataTable SelectTicketAssignees(Guid OrgID, int departmentId, int ticketId, TicketAssignmentType ticketAssignmentType, bool distinctUsers, bool onlyActiveAssignments)
		{
			return SelectRecords("sp_SelectTicketAssignees", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TicketId", ticketId), new SqlParameter("@AssignmentType", ticketAssignmentType), new SqlParameter("@DistinctUsers", distinctUsers), new SqlParameter("@OnlyActiveAssignments", onlyActiveAssignments) }, OrgID);
		}

		public static DataTable SelectTicketAssignees(Guid OrgID, int departmentId, int ticketId, TicketAssignmentType ticketAssignmentType)
		{
			return SelectTicketAssignees(OrgID, departmentId, ticketId, ticketAssignmentType, false, false);
		}

		public static void UpdateTechNote(int TktID, string NoteText)
		{
			UpdateData("sp_UpdateTicketTechNote", new SqlParameter[] { new SqlParameter("@TicketId", TktID), new SqlParameter("@Note", NoteText) });
		}

		public static void UpdateTicketWorkpad(int TktID, string Content)
		{
			UpdateData("sp_UpdateTicketWorkpad", new SqlParameter[] { new SqlParameter("@TicketId", TktID), new SqlParameter("@Workpad", Content) });
		}

		public static void UpdatePriority(int DeptID, int TicketId, int PriorityId)
		{
			SqlParameter _pOldPriName = new SqlParameter("@OldPriName", SqlDbType.NVarChar, 50);
			_pOldPriName.Direction = ParameterDirection.Output;
			SqlParameter _pOldPriInt = new SqlParameter("@OldPriInt", SqlDbType.TinyInt);
			_pOldPriInt.Direction = ParameterDirection.Output;
			SqlParameter _pNewPriName = new SqlParameter("@NewPriName", SqlDbType.NVarChar, 50);
			_pNewPriName.Direction = ParameterDirection.Output;
			SqlParameter _pNewPriInt = new SqlParameter("@NewPriInt", SqlDbType.TinyInt);
			_pNewPriInt.Direction = ParameterDirection.Output;
			UpdateData("sp_UpdateTktPriority", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@PId", PriorityId), _pOldPriName, _pOldPriInt, _pNewPriName, _pNewPriInt });
		}

		public static void UpdatePriorityWhereNull(int DeptID, int PriorityId)
		{
			UpdateByQuery("UPDATE tbl_ticket SET PriorityId = " + PriorityId.ToString() + ", UpdatedTime=GETUTCDATE() WHERE PriorityId is null AND company_id = " + DeptID.ToString());
		}

		public static void UpdateLevel(int DeptID, int TicketId, int tintLevel, int UserId, string Note)
		{
			UpdateData("sp_UpdateTktLevel", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@UId", UserId), new SqlParameter("@TId", TicketId), new SqlParameter("@tintLevel", tintLevel), new SqlParameter("@vchTktNote", Note) });
		}

		private string IncreaseCopyOf(string subject)
		{
			if (subject.StartsWith("Copy("))
			{
				try
				{
					int number = 0;
					int position = subject.IndexOf("Copy(") + 5;
					int position2 = subject.IndexOf(")", position + 1);
					if (int.TryParse(subject.Substring(position, position2 - position), out number))
						return "Copy(" + ++number + ") of " + subject.Substring(position2+4);
				}
				catch
				{
					return "Copy of " + m_Subject;
				}
			}
			else if (subject.StartsWith("Copy of "))
				return "Copy(2) of " + subject.Substring(8);
			else
				return "Copy of " + m_Subject;
			return subject;
		}

		public string Clone(Guid OrgID, int DeptID, int TicketId, int UserId)
		{
			DataRow TktRow = Tickets.SelectOne(OrgID, DeptID, TicketId);
			if (TktRow == null)
			{
				//Ticket not exsist
				return "Source ticket not exists";
			}
			InitTicket(TktRow);
			int tktDeptId = (int)TktRow["company_id"];
            int userCreatedId = m_UserCreatedId;
            DataRow _user_details = Data.Logins.SelectUserDetails(tktDeptId, m_UserCreatedId);
            if (_user_details != null)
            {
                userCreatedId = (bool)_user_details["btUserInactive"] ? UserId : m_UserId;
            }
			int initialPostId = 0;
            string initPost = m_InitialPost;
            if (m_InitialPost.Contains("This ticket was entered by"))
                initPost = m_InitialPost.Substring(0, m_InitialPost.IndexOf("<br><br>This ticket was entered by"));
            int _TktId = Data.Tickets.CreateNew(OrgID, tktDeptId, UserId, m_TechId, userCreatedId, Functions.User2DBDateTime(DateTime.UtcNow), m_AccountId, m_AccountLocationId, 
				false, m_LocationId, m_ClassId, m_Level, m_SubmissionCategory, IsHandleByCallCentre, m_CreationCategoryId, false,
                m_PriorityId, m_RequestCompletionDate, m_RequestCompletionNote, m_SerialNumber, Tickets.SelectAssetsToArray(tktDeptId, TicketId), m_IdMethod, m_CustomFieldsXML, IncreaseCopyOf(m_Subject), initPost,
                null, TktRow["Status"].ToString() == "Closed" ? "Open" : TktRow["Status"].ToString(), out initialPostId, m_ProjectId, m_FolderId, m_SchedTicketID, m_EstimatedTime);

			if (_TktId < 0)
			{
				string errMsg = "<b>ERROR. Ticket Not Saved.</b><br>";
				switch (_TktId)
				{
					case -1:
						errMsg += "Routing Error: Input level is not setup for this class.";
						break;
					case -2:
						errMsg += "Routing Error: No routing options are enabled. No route found. Must choose Technician specifically.";
						break;
					case -3:
						errMsg += "Routing Error: No Route Found. Routing configuration must be modified.";
						break;
					case -4:
						errMsg += "Routing Error: Level does not exists.";
						break;
					case -5:
						errMsg += "Routing Error: Route found but Technician could not be returned. Please check routing order for errors.";
						break;
				}
				return errMsg;
			}
			else if (_TktId > 0)
			{

				DataTable dt = Data.Ticket.SelectTicketAssignees(OrgID, tktDeptId, TicketId, Data.Ticket.TicketAssignmentType.User, true, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						int userId = (int)dr["UserId"];
						if (userId == userCreatedId)
							continue;
						Data.Ticket.AttachAlternateAssignee(tktDeptId, _TktId, userId, Data.Ticket.TicketAssignmentType.User);
					}
				}

				dt = Data.Ticket.SelectTicketAssignees(OrgID, tktDeptId, TicketId, Data.Ticket.TicketAssignmentType.Technician, true, true);

				if (dt != null && dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						int techId = (int)dr["UserId"];
						if (techId == m_TechId)
							continue;
						Data.Ticket.AttachAlternateAssignee(tktDeptId, _TktId, techId, Data.Ticket.TicketAssignmentType.Technician);
					}
				}

				dt = Data.ToDo.SelectToDoListAndItems(tktDeptId, TicketId, m_ProjectId);
				if (dt != null && dt.Rows.Count > 0)
				{
                    string todoListId = Guid.NewGuid().ToString();
                    foreach (DataRow _r in dt.Rows)
                    {
                        switch (_r["ItemType"].ToString())
                        {
                            case "1": //ToDoList
                                todoListId = Guid.NewGuid().ToString();
                                Data.ToDo.InsertToDoList(todoListId, tktDeptId, _r["Name"].ToString(), "", _TktId, m_ProjectId);
                                break;
                            case "2": //ToDoItem
                                Data.ToDo.InsertToDoItem(tktDeptId, _r["Text"].ToString(), todoListId, m_UserCreatedId, _r.IsNull("AssignedId") ? 0 : (int)_r["AssignedId"], _r["HoursEstimatedRemaining"].ToString().Trim().Length > 0 ? (decimal)_r["HoursEstimatedRemaining"] : 0,
                                    _r["Due"].ToString().Trim().Length > 0 ? (DateTime)_r["Due"] : DateTime.MinValue);
                                break;
                        }
                    }
				}

                Data.Ticket _newTkt = new Data.Ticket(tktDeptId, _TktId, true);
                foreach (Data.Ticket.TicketAssignee _ta in _newTkt.Users) _ta.SendResponse = true;
                foreach (Data.Ticket.TicketAssignee _ta in _newTkt.Technicians) _ta.SendResponse = true;
				Data.NotificationRules.RaiseNotificationEvent(tktDeptId, m_UserCreatedId, Data.NotificationRules.TicketEvent.NewTicket, _newTkt, this, null);
			}
			return _TktId.ToString();
		}

		public static void UpdateLocation(int DeptID, int TicketId, int LocationId)
		{
			UpdateByQuery("UPDATE tbl_ticket SET LocationId  = " + LocationId.ToString() + ", UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TicketId.ToString());
		}

		public static void UpdateTechnician(Guid OrgId, int DeptID, int TicketId, int TechnicianId, bool keepTechnicianAssigned)
		{
			UpdateData("sp_UpdateTicketTechnician", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@NewTechnicianId", TechnicianId), new SqlParameter("@KeepTechnicianAssigned", keepTechnicianAssigned) }, OrgId);
		}

		public static void UpdateTechnician(int DeptID, int TicketId, int TechnicianId, bool keepTechnicianAssigned)
		{
			UpdateTechnician(Guid.Empty, DeptID, TicketId, TechnicianId, keepTechnicianAssigned);
		}

		public static void AttachAlternateAssignee(int departmentId, int ticketId, int assigneeId, Ticket.TicketAssignmentType assignmentType)
		{
			AttachAlternateAssignee(departmentId, ticketId, assigneeId, assignmentType, true);
		}

		public static void AttachAlternateAssignee(int departmentId, int ticketId, int assigneeId, Ticket.TicketAssignmentType assignmentType, bool reassignIfAssigned)
		{
			UpdateData("sp_InsertTicketAssignment", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TicketId", ticketId), new SqlParameter("@UserId", assigneeId), new SqlParameter("@AssignmentType", (int)assignmentType), new SqlParameter("@ReassignIfAssigned", reassignIfAssigned) });
		}

		public static void AttachAlternateAssignee(Guid OrgId, int departmentId, int ticketId, int assigneeId, Ticket.TicketAssignmentType assignmentType, bool reassignIfAssigned)
		{
			UpdateData("sp_InsertTicketAssignment", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TicketId", ticketId), new SqlParameter("@UserId", assigneeId), new SqlParameter("@AssignmentType", (int)assignmentType), new SqlParameter("@ReassignIfAssigned", reassignIfAssigned) }, OrgId);
		}

		public static void DetachAlternateAssignee(int departmentId, int ticketId, int assignmentId)
		{
			UpdateData("sp_UpdateTicketAssignmentDetach", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TicketId", ticketId), new SqlParameter("@AssignmentId", assignmentId) });
		}

		public static void UpdateTravelCost(int DeptID, int TicketId, decimal TravelCost)
		{
			UpdateData("sp_UpdateTravelCost", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@TravelCost", TravelCost) });
		}

		public static void UpdateStatus(Guid OrgId, int DeptID, int TicketId, Status TicketStatus)
		{
			switch (TicketStatus)
			{
				case Status.Open:
					UpdateData("sp_UpdateTicketStatus", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@Status", 3) }, OrgId);
					break;
				case Status.OnHold:
					UpdateData("sp_UpdateTicketStatus", new SqlParameter[] { new SqlParameter("@CompanyId", DeptID), new SqlParameter("@TicketId", TicketId), new SqlParameter("@Status", 2) }, OrgId);
					break;
			}
		}

		public static void UpdateStatus(int DeptID, int TicketId, Status TicketStatus)
		{
			UpdateStatus(Guid.Empty, DeptID, TicketId, TicketStatus);
		}

		public static void UpdateAccount(int DeptID, int TicketId, int AccountId, int AccountLocationId, int TransferMode, bool IsUserTransfer)
		{
			SqlParameter _pAccLocationId = new SqlParameter("@AcctLocId", SqlDbType.Int);
			if (AccountLocationId != 0) _pAccLocationId.Value = AccountLocationId;
			else _pAccLocationId.Value = DBNull.Value;
			UpdateData("sp_UpdateTktAccount", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@AcctId", AccountId), _pAccLocationId, new SqlParameter("@intTransfer", TransferMode), new SqlParameter("@btTransferUser", IsUserTransfer) });
		}

		public static void UpdateTransferByClass(Guid OrgId, int DeptID, int TicketId, int ClassId, bool KeepPriority, bool KeepLevel, bool keepTechnicianAssigned)
		{
			SqlParameter _pOldTechName = new SqlParameter("@OldTechName", SqlDbType.NVarChar, 100);
			_pOldTechName.Direction = ParameterDirection.Output;
			SqlParameter _pNewTechName = new SqlParameter("@NewTechName", SqlDbType.NVarChar, 100);
			_pNewTechName.Direction = ParameterDirection.Output;
			UpdateData("sp_UpdateTktTransferByClass", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@ClassId", ClassId), _pOldTechName, _pNewTechName, new SqlParameter("@btNoPriorityChange", KeepPriority), new SqlParameter("@btKeepCurrentLevel", KeepLevel), new SqlParameter("@KeepTechnicianAssigned", keepTechnicianAssigned) }, OrgId);
		}

		public static void UpdateTransferByClass(int DeptID, int TicketId, int ClassId, bool KeepPriority, bool KeepLevel, bool keepTechnicianAssigned)
		{
			UpdateTransferByClass(Guid.Empty, DeptID, TicketId, ClassId, KeepPriority, KeepLevel, keepTechnicianAssigned);
		}

		public static int UpdateEscalateByLevel(int DeptID, int TicketId, int UserId, bool UpDirection, string NoteText, bool keepTechnicianAssigned)
		{
			SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			_pRVAL.Direction = ParameterDirection.ReturnValue;
			SqlParameter _pDirection = new SqlParameter("@vchDirection", SqlDbType.VarChar, 4);
			if (UpDirection) _pDirection.Value = "up";
			else _pDirection.Value = "down";
			SqlParameter _pText = new SqlParameter("@vchText", SqlDbType.NVarChar, -1);
			_pText.Direction = ParameterDirection.InputOutput;
			if (NoteText.Length > 4999) NoteText = "--Text truncated at 5000 characters--<br><br>" + NoteText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
			if (NoteText.Length > 0) _pText.Value = NoteText;
			else _pText.Value = DBNull.Value;
			SqlParameter _pNoteType = new SqlParameter("@vchNoteType", SqlDbType.NVarChar, 50);
			_pNoteType.Direction = ParameterDirection.Output;
			UpdateData("sp_EscalateTicket", new SqlParameter[] { _pRVAL, new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@UId", UserId), _pDirection, _pText, _pNoteType, new SqlParameter("@KeepTechnicianAssigned", keepTechnicianAssigned) });
			return (int)_pRVAL.Value;
		}

		public static void CloseTicket(Guid OrgId, int DeptID, int TicketId, int UserId, string LogNote, string  systemGeneratedText, DateTime ClosureDate, string EmailCC)
		{
			SqlParameter _pTId = new SqlParameter("@TId", SqlDbType.Int);
			_pTId.Value = DBNull.Value;
			SqlParameter _pLogNote = new SqlParameter("@vchNote", SqlDbType.NVarChar, -1);
			if (LogNote.Length > 4999) LogNote = "--Text truncated at 5000 characters--<br><br>" + LogNote.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
			if (LogNote.Length > 0) _pLogNote.Value = LogNote;
			else _pLogNote.Value = DBNull.Value;

			SqlParameter pSystemGeneratedText = new SqlParameter("@SystemGeneratedText", SqlDbType.NVarChar, -1);
			if (!string.IsNullOrEmpty(systemGeneratedText)) pSystemGeneratedText.Value = systemGeneratedText;
			else pSystemGeneratedText.Value = DBNull.Value;

			//            UpdateData("sp_DeleteTktFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pTId });
			UpdateData("sp_UpdateTktClose", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@UId", UserId), _pLogNote, pSystemGeneratedText, new SqlParameter("@dtClosureDate", ClosureDate), new SqlParameter("@EmailCC", EmailCC) }, OrgId);
		}

		public static void CloseTicket(int DeptID, int TicketId, int UserId, string LogNote, string systemGeneratedText, DateTime ClosureDate, string EmailCC)
		{
			CloseTicket(Guid.Empty, DeptID, TicketId, UserId, LogNote, systemGeneratedText, ClosureDate, EmailCC);
		}

		public static void UpdateFiles(Guid OrgID, int DeptID, int TicketId, System.IO.FileInfo[] files, FileItem[] savedfiles)
		{
			FileItem[] _fArr = Tickets.SelectFilesToArray(DeptID, TicketId);
			foreach (FileItem _f1 in _fArr)
			{
				bool _toDelete = true;
				foreach (FileItem _f2 in savedfiles)
				{
					if (_f1.Name == _f2.Name)
					{
						_toDelete = false;
						break;
					}
				}
				if (!_toDelete) continue;
                Tickets.DeleteFile(OrgID, DeptID, TicketId, _f1.ID);
			}
			foreach (System.IO.FileInfo _file in files)
			{
				System.IO.FileStream _fstream = _file.OpenRead();
				byte[] _data = new byte[Convert.ToInt32(_fstream.Length)];
				_fstream.Read(_data, 0, _data.Length);
				Tickets.InsertFile(DeptID, TicketId, _file.Name, Convert.ToInt32(_file.Length), _data);
				_fstream.Close();
			}
		}

		public static void UpdateNewPostIcon(int DeptID, int TicketId, int UserId, bool PostOnOff)
		{
			UpdateData("sp_UpdateNewPostIcon", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@TId", TicketId), new SqlParameter("@PostOnOff", PostOnOff), new SqlParameter("@UId", UserId) });
		}

		public static void UpdateNextStep(int DeptID, int TktID, string nextStep)
		{
			if (nextStep.Length > 100) nextStep = nextStep.Substring(0, 100);

			UpdateData("sp_UpdateTicketNextStep", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@NextStep", nextStep) });
		}

		public static void UpdatePseudoId(Guid OrgId, int DeptID, int TktID, string pseudoId)
		{
			UpdateData("sp_UpdateTicketPseudoId", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@TicketId", TktID), new SqlParameter("@PseudoId", pseudoId) }, OrgId);
		}

		public static void UpdateKBPublishLevel(int DeptID, int TktID, int kbPublishLevel)
		{
			UpdateByQuery("UPDATE tbl_ticket SET KBPublishLevel = " + kbPublishLevel.ToString() + ", UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateKBSearchDesc(int DeptID, int TktID, string kbSearchDesc)
		{
			UpdateByQuery("UPDATE tbl_ticket SET KBSearchDesc = '" + kbSearchDesc + "', UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateKBAlternateId(int DeptID, int TktID, string kbAlternateId)
		{
			UpdateByQuery("UPDATE tbl_ticket SET KBAlternateId = '" + kbAlternateId + "', UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public static void UpdateTicketLog(Guid OrgId, int deptID, int id, string vchNote, bool hidden, int editedBy)
		{
			UpdateData("sp_UpdateTicketLog", new SqlParameter[] { 
				new SqlParameter("@DId", deptID), 
				new SqlParameter("@Id", id), 
				new SqlParameter("@vchNote", vchNote),
				new SqlParameter("@Hidden", hidden),
				new SqlParameter("@EditedBy", editedBy) }, OrgId);
		}

		public static void ConvertToKBArticle(int DeptID, int TktID)
		{
			UpdateByQuery("UPDATE tbl_ticket SET KB = 1, KBType = 1, KBPublishLevel = 1, UpdatedTime=GETUTCDATE() WHERE company_id = " + DeptID.ToString() + " AND id = " + TktID.ToString());
		}

		public class LogCollection : System.Collections.CollectionBase
		{
			public LogCollection()
			{
			}

			public LogCollection(Guid OrgID, int DeptID, int TicketID)
			{
				DataTable _dt = Tickets.SelectTicketLog(OrgID, DeptID, TicketID);
				foreach (DataRow _row in _dt.Rows)
					List.Add(new LogEntry(_row.IsNull("UId") ? 0 : (int)_row["UId"], _row["FirstName"].ToString(), _row["LastName"].ToString(), (DateTime)_row["dtDate"], _row["vchType"].ToString(), _row["vchNote"].ToString(), _row["To"].ToString()));
			}

			public LogCollection(int DeptID, int TicketID)
				: this(Guid.Empty, DeptID, TicketID)
			{
			}

			public LogEntry this[int index]
			{
				get { return (LogEntry)List[index]; }
				set { List[index] = value; }
			}

			public int Add(LogEntry value)
			{
				return List.Add(value);
			}

			public int IndexOf(LogEntry value)
			{
				return List.IndexOf(value);
			}

			public void Insert(int index, LogEntry value)
			{
				List.Insert(index, value);
			}

			public void Remove(LogEntry value)
			{
				List.Remove(value);
			}

			public bool Contains(LogEntry value)
			{
				return List.Contains(value);
			}
		}

		public class LogEntry
		{
			int m_UserId = 0;
			string m_UserFirstName = string.Empty;
			string m_UserLastName = string.Empty;
			DateTime m_CreatedDate = DateTime.MinValue;
			string m_LogType = string.Empty;
			string m_LogNote = string.Empty;
			private string m_To = string.Empty;

			public LogEntry()
			{
			}

			public LogEntry(int UserId, string UserFirstName, string UserLastName, DateTime CreatedDate, string LogType, string LogNote, string To)
				: this(UserId, UserFirstName, UserLastName, CreatedDate, LogType, LogNote)
			{
				m_To = To;
			}

			public LogEntry(int UserId, string UserFirstName, string UserLastName, DateTime CreatedDate, string LogType, string LogNote)
			{
				m_UserId = UserId;
				m_UserFirstName = UserFirstName;
				m_UserLastName = UserLastName;
				m_CreatedDate = CreatedDate;
				m_LogType = LogType;
				m_LogNote = LogNote;
			}

			[DataMember]
			public int UserId
			{
				get { return m_UserId; }
				set { m_UserId = value; }
			}

			public string UserFirstName
			{
				get { return m_UserFirstName; }
				set { m_UserFirstName = value; }
			}

			public string UserLastName
			{
				get { return m_UserLastName; }
				set { m_UserLastName = value; }
			}

			public DateTime CreatedDate
			{
				get { return m_CreatedDate; }
				set { m_CreatedDate = value; }
			}

			public string LogType
			{
				get { return m_LogType; }
				set { m_LogType = value; }
			}

			public string LogNote
			{
				get { return m_LogNote; }
				set { m_LogNote = value; }
			}

			public string To
			{
				get { return m_To; }
				set { m_To = value; }
			}
		}

		public class TicketAssignee
		{
			int userId;
			private string name;
			bool sendResponse;
			bool isPrimary;

			[Bindable(true)]
			public int UserId
			{
				get { return this.userId; }
				set { this.userId = value; }
			}

			[Bindable(true)]
			public string Name
			{
				get { return this.name; }
				set { this.name = value; }
			}

			public bool SendResponse
			{
				get { return this.sendResponse; }
				set { this.sendResponse = value; }
			}

			public bool IsPrimary
			{
				get { return this.isPrimary; }
				set { this.isPrimary = value; }
			}

			public TicketAssignee()
			{
			}

			public TicketAssignee(int userId, string name)
			{
				this.userId = userId;
				this.name = name;
			}
		}

		public class TicketAssignees : CollectionBase
		{
			public TicketAssignees()
			{
			}

			public TicketAssignees(int departmentId, int ticketId, Ticket.TicketAssignmentType assignmentType)
				: this(Guid.Empty, departmentId, ticketId, assignmentType)
			{
			}

			public TicketAssignees(Guid OrgID, int departmentId, int ticketId, Ticket.TicketAssignmentType assignmentType)
			{
				DataTable dtAssignees = Ticket.SelectTicketAssignees(OrgID, departmentId, ticketId, assignmentType, true, true);
				foreach (DataRow drAssignee in dtAssignees.Rows)
					List.Add(new TicketAssignee(drAssignee.IsNull("UserId") ? 0 : (int)drAssignee["UserId"], drAssignee["name"].ToString()));
			}

			public TicketAssignees(Guid OrgID, int departmentId, int ticketId, Ticket.TicketAssignmentType assignmentType, int primaryUserId)
				: this(OrgID, departmentId, ticketId, assignmentType)
			{
				if (primaryUserId > 0)
					this.SetPrimary(primaryUserId);
			}

			public TicketAssignees(int departmentId, int ticketId, Ticket.TicketAssignmentType assignmentType, int primaryUserId)
				: this(Guid.Empty, departmentId, ticketId, assignmentType, primaryUserId)
			{
			}

			public TicketAssignee this[int index]
			{
				get { return (TicketAssignee)List[index]; }
				set { List[index] = value; }
			}

			public void SetPrimary(int userId)
			{
				foreach (TicketAssignee ticketAssignee in List)
					ticketAssignee.IsPrimary = ticketAssignee.UserId == userId;
			}

			public bool SetSendResponse(int userId)
			{
				foreach (TicketAssignee ticketAssignee in List)
					if (ticketAssignee.UserId == userId)
					{
						ticketAssignee.SendResponse = true;
						return true;
					}
				return false;
			}

			public string GetName(int userId)
			{
				TicketAssignee ticketAssignee = GetByUserId(userId);
				return ticketAssignee == null ? string.Empty : ticketAssignee.Name;
			}

			public TicketAssignee GetByUserId(int userId)
			{
				foreach (TicketAssignee ticketAssignee in List)
					if (ticketAssignee.UserId == userId)
						return ticketAssignee;
				return null;
			}

			public int Add(TicketAssignee value)
			{
				return List.Add(value);
			}

			public int IndexOf(TicketAssignee value)
			{
				return List.IndexOf(value);
			}

			public void Insert(int index, TicketAssignee value)
			{
				List.Insert(index, value);
			}

			public void Remove(TicketAssignee value)
			{
				List.Remove(value);
			}

			public bool Contains(TicketAssignee value)
			{
				return List.Contains(value);
			}
		}
	}
}
