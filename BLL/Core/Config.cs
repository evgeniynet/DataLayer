using System;
using System.Data;
using System.Web;
using bigWebApps.bigWebDesk.Data;
using Micajah.Common.Bll;
using System.Configuration;
using System.Runtime.Serialization;

namespace bigWebApps.bigWebDesk
{
    /// <summary>
    /// CONFIG VALUES
    /// Below common configuration values are read from the configuration string stored in the login session and transferred to local
    /// variables to be used on each page the configuration value is needed. Only common configuration values are stored in local variables
    /// below.  If the configuration value is only used on one page it will be retrieved directly from the database on each page load. There
    /// is no set rule to determine if the value is read and transferred below or pulled directly from the db on page load. It depends on performance
    /// and how often the configuration value will be used throughout the program.
    /// </summary>
    public class Config
    {
        #region Global properties
        public bool OnHoldStatus = false;
        public bool TimeTracking = false;
        public bool PartsTracking = false;
        public bool TechCheckin = false;
        public bool LocationTracking = false;
        public bool KBTracking = false;
        public bool ProjectTracking = false;
        public bool TechChoose = false;
        public bool NewUserWizard = false;
        public bool SerialNumber = false;
        public string SerialNumberName = "Serial Number";
        public bool ScheduledTickets = true;
        public bool Folders = false;
        public bool AssetTracking = false; //Asset Tracking
        public bool ClassTracking = false; //Class Tracking
        public bool PrioritiesGeneral = false; //Priorities General
        public bool PrioritiesUser = false; //Priorities User
        public bool PrioritiesLog = false; //Save to Ticket Log when priority was changed.
        public bool ChooseTechLimitQues = false; //ChooseTechLimitQues
        public bool CategoryTracking = false; //Category Tracking
        public bool ResolutionTracking = false; //Resolution Tracking
        public bool ConfirmationTracking = false; //Confirmation Tracking
        public bool AfterHoursAlert = false; //After hours alert
        public bool CustomFields = false; //Cust Fields
        public bool MiscCosts = false; //Misc Costs
        public bool TravelCosts = false; //Travel Costs
        public bool ViewTktCosts = false; //View Tkt Costs
        public bool UnassignedQue = false; //Unassigned Que
        public bool TktLevels = false; //TktLevels
        public bool TktLevelsForUser = false; //TktLevels for Users
        public bool RequestCompletionDate = true; //Request Completion Date
        public bool AccountManager = false; //Account Manager
        public bool TktClosureDate = false; //Allow ticket closure date to be changed
        public bool TktRequireClosureNote = false; //Requires Ticket Closure Note

        public bool ForceTimeEntry = false; ////Force Time entry support
        //public bool TktCreationDate = false; //Allow ticket creation date to be changed

        public bool TktCreationTimeSupport = false; //Added Allow edit ticket creation time support
        public bool SuppresBWALogos = false; //Added ability to suppress BWA logos and branding
        public bool SupportGroups = false; //Added support groups
        public bool TktIDMethod = true; //VGOOZ: 31-Mar-2005: Added Ticket Id Method
        public bool CallCenterSupport = false; //VGOOZ: 31-Mar-2005: Added Call Center Support
        public bool SubmissionCategory = false; //VGOOZ: 31-Mar-2005: Added Enbale Submission Category
        public bool MultipleTimeEntry = false; //VGOOZ 17-APR-2005: Enable Multiple Time Entry for Technicians
        public bool SignatureBlockPrintedTkt = true; //VGOOZ 17-APR-2005: Enable Signature Block on Printed Tickets
        public bool CustomNames = false; //MRUDKOVSKI: 19-MAY-2005 - Custom Names feauture
        public bool AllowCCTktClosing = true; //VGOOZ 12-OCT-2005: Allow CC EMails on closing tickets
        public bool RemoteAssistance = true; //VGOOZ 04-MAY-2006: Enable Remote Assistance Support
        public bool AllowViewSchedTtkTech = true; //VGOOZ 11-MAY-2006: Allow technicians view all scheduled tickets
        public bool TechChooseLevel = false; //VGOOZ 30-MAY-2006: Allow technicians to choose Level during Ticket Creation
        public bool DisplayCustFieldsForTech = false; //VGOOZ 15-JUNE-2006: Display ticket custom fields for technicians only
        // YMYKYTYUK 20-JAN-2009:option DisplayTicketMsgBoxesAsLinks is obsolete and should be removed
        // Search other places by ~DTMBAL~
        //public bool DisplayTicketMsgBoxesAsLinks = false; //VGOOZ 3-SEPTEMBER-2007: Display Ticket Message Boxes as links
        public bool AllowTechReopenTicket = true; //VGOOZ: 3-SEPTEMBER-2007 - Allow Technician to reopen tickets after reopen limit
        public bool AllowTechEscalateDescalateOnly = false; //VGOOZ: 18-SEPTEMBER-2007 - Allow Technician to escalate/descalate ticket only
        public bool DeploymentLoggerEnabled = false; //DCHERNENKO: 25-SEPTEMBER-2007 - is Deployment Logger enabled
        public bool DeploymentLoggerSettedUp = false; //DCHERNENKO: 25-SEPTEMBER-2007 - is Deployment Logger setted up
        public bool EnableNotificationRulesAdminSection = true; //VGOOZ: 24-OCTOBER-2007 - Enable Notification Rules Admin Section
        public bool LimitTechAbilityInAssets = false; //DCHERNENKO: 16-OCTOBER-2007 - Limit technicians in assets management
        public bool HTTPSEnabled = false; //VGOOZ: 6-MARCH-2008 - Is use HTTPS enabled
        public bool DisableTransferTktsToCheckedOUTTechs = false; //VGOOZ: 29-MAY-2008 -Disable select checked out tech from transfer list
        public bool AllowSUserToChooseAnyLogin = false;
        public bool EnableLDAP = false; //VGOOZ 21-OCT-2008: This option enables by Department Admin
        public bool EnableLDAPGlobal = false; //VGOOZ 21-OCT-2008: This option enables from BO
        public bool EnableLDAPAutoRedirect = false;
        public bool EnableLDAPEmailExtUrl = false;
        public string LDAPLocalUrl = "http://localhost";
        public string EmailSuffixes = string.Empty;
        public bool ForceLowestClassNode = false; //VGOOZ 27-OCT-2008: Force the person entering a ticket to choose the lowest node of class that is created
        public bool AllowTransferTicketUser = false; //VGOOZ 4-NOV-2008: Allow Technicans to change ticket end user
        public bool DisplayAddNewUserLink = true; //YMykytyuk 25-NOV-2008: Allow Techs to add new user when create a ticket
        public bool AllowEnterTimeOnTicketDetail = false; //YMykytyuk 20-JAN-2009: Allow enter time on th View Ticket page
        public bool EnableTicketToProjectRelation = false; //YMykytyuk 03-FEB-2009: Allow tie Ticket to Project
        public bool AllowStartStopTimeOnTicketDetail = true; //YMykytyuk 21-FEB-2009: Show Start/Stop time on Ticket Detail page when saving time
        public bool AllowRemainHoursOnTicketDetail = true; //YMykytyuk 21-FEB-2009: Show Remaining Hours on Ticket Detail page when saving time
        public bool RequireTktInitialPost = false; //Requeire to enter ticket initial post when ticket created 
        public bool EnableGlobalAPI = false; //YMykytyuk 03-APR-2009: Enable Global API - configuration/ GlobalAPI
        public bool AllowTimeEntriesOnTktLog = true; //YMykytyuk 14-APR-2009
        public bool AllowUserLoginWithoutPassword = true; //VGOOZ 26-MAY-2009: Tkt #8142 - User Login via Email with NO Password
        public bool RelatedTickets = true; //VGOOZ 1-NOV-2009: Tkt #8898: Sub Master Related Tickets
        public bool AccLevelTimeTracking = true;
        public bool BCIntegration = false;
        public bool BetaFeaturesEnabled = false;
        public byte EMailParserV2 = 0; // 0 - Off; 1 - Legacy Parser (Old Email Parser); 2 - EMail Parser Ver 2
        public bool EnableAssetAuditor = false;
        public bool WarehouseIntegrationEnabled = false; // Artem Korzhavin 16 feb 2011
        public decimal HourIncrement = -1;
        public decimal MinimumLoggableTime = -1;
        public decimal DefaultDistanceRate = 0;
        private SettingCollection InternalSettings = null;
        private Instance InternalInstance = null;
        public bool QBIntegration = false;
        public bool SupportPortal = false;

        // new settings from tbl_company
        public int LicensedUsers = 50;
        public decimal HourlyRate = 0;
        public DateTime TktExportStart=DateTime.MinValue;
        public DateTime TktExportEnd=DateTime.MaxValue;
        public int LicensedAssets = 500;
        public bool ExpectDueDateUser = false;
        public int BusHourStart = 9;
        public int BusMinStart = 0;
        public int BusHourStop = 18;
        public int BusMinStop = 0;
        public string WorkingDays = "1111100";
        public int RoutingOrder = 0;
        public bool AcctRtOvr = true;
        public int AcctRtOvrLevel = 1;
        public int AcctRtOvrOption = 0;
        public int AcctRtOvrRouteType = 1;
        public int RemoteAssistanceSessions = 0;
        public int CreationCatId = 0;
        public int TicketTimer = 0;
        public int TicketReopenLimitUser = 400;
        public int TicketReopenLimitTechnician = 90;
        public decimal HourlyBillableRate = 0;
        public string QBAccount = string.Empty;
        public bool FBIntegration = false;
        public int FBClientId = 0;
        public string FBEndPointURL = string.Empty;
        public string FBToken = string.Empty;
        public string BCEndPointURL = string.Empty;
        public string BCToken = string.Empty;
        public string InternalQBAccount = string.Empty;
        public bool EnableBilling = true;
        public bool EnablePayments = true;
        public bool PrioritiesUserCreateTicket = false;
        public bool EnableToDo = false;
        public bool EnableContactUsWidget = false;
        public int CUWClassID = 0;
        public string FBURL = string.Empty;
        public string FBAccessToken = string.Empty;
        public string FBAccessTokenSecret = string.Empty;

        AssetsConfig assets;
        public AssetsConfig Assets
        {
            get { return assets; }
            set { assets = value; }
        }

        SupportPortalConfig support;
        public SupportPortalConfig SupportPortalSettings
        {
            get { return support; }
            set { support = value; }
        }

        string currency = "$";
        public string Currency
        {
            get
            {
                if (!string.IsNullOrEmpty(currency))
                    return currency;
                return "$";
            }
        }

        public string FBoAuthSignature
        {
            get
            {
                if (FBAccessTokenSecret != "" && !string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["FBoAuthSecret"]))
                {
                    return System.Configuration.ConfigurationManager.AppSettings["FBoAuthSecret"].ToString() 
                        + "&" + FBAccessTokenSecret;
                }
                return "";
            }
        }
        #endregion

        #region Public methods

        public static Config GetConfig(Guid OrgID, Guid InstId)
        {
            int deptId = Companies.SelectDepartmentId(OrgID, InstId);
            return GetConfig(OrgID, deptId);
        }

        public static Config GetConfig(Guid OrgID, int departmentId)
        {
            if (HttpContext.Current == null || HttpContext.Current.Cache == null) return new Config(OrgID, departmentId);
 
            Config configurationObject = (Config)HttpContext.Current.Cache.Get(GetCachedConfigObjectKey(OrgID, departmentId));

            if (configurationObject == null)
            {
                configurationObject = new Config(OrgID, departmentId);
                HttpContext.Current.Cache.Add(GetCachedConfigObjectKey(OrgID, departmentId), configurationObject, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(30), System.Web.Caching.CacheItemPriority.Default, null);
            }
            return configurationObject;
        }

        public static string GetCachedConfigObjectKey(Guid OrgID, int departmentId)
        {
            return "DepartmentSettings." + Data.DBAccess.GetCurrentOrgID(OrgID).ToString() + "," + departmentId.ToString();
        }

        public static void ForceCachedConfigObjectToExpire(Guid OrgID, int departmentId)
        {
            HttpContext.Current.Cache.Remove(GetCachedConfigObjectKey(OrgID, departmentId));
            bigWebApps.bigWebDesk.CustomNames.ClearCache(OrgID, departmentId);
        }

        public static void ForceCachedConfigObjectToExpire(Guid OrgID, Guid instanceId)
        {
            DataRow _row = Companies.SelectOneBase(OrgID, instanceId);
            if (_row != null)
                ForceCachedConfigObjectToExpire(OrgID, (int)_row["company_id"]);
        }

        public Config(Guid OrgID, int DeptID)
        {
            if (DeptID > 0)
            {
                DataRow _row = Data.Companies.SelectOneBase(OrgID, DeptID);
                if (_row == null) return;
                InitClassVariables(OrgID, (Guid)_row["company_guid"], DeptID);
            }
        }

        public Config(Guid OrgID, Guid InstanceID, int DeptID)
        {
            InitClassVariables(OrgID, InstanceID, DeptID);
        }

        public void UpdateSettings()
        {
            if (this.InternalSettings != null)
            {
                Setting setting = null;

                // need to add settings which can be edited without settings control
                setting = this.InternalSettings.FindByShortName("emailSuffixes");
                if (setting != null) setting.Value = this.EmailSuffixes;
                setting = this.InternalSettings.FindByShortName("CreationCatId");
                if (setting != null) setting.Value = this.CreationCatId.ToString();
                setting = this.InternalSettings.FindByShortName("CUWClassID");
                if (setting != null) setting.Value = this.CUWClassID.ToString();
                setting = this.InternalSettings.FindByShortName("FBURL");
                if (setting != null) setting.Value = this.FBURL.ToString();
                setting = this.InternalSettings.FindByShortName("FBAccessToken");
                if (setting != null) setting.Value = this.FBAccessToken.ToString();
                setting = this.InternalSettings.FindByShortName("FBAccessTokenSecret");
                if (setting != null) setting.Value = this.FBAccessTokenSecret.ToString();

                // Save all settings in db
                this.InternalSettings.UpdateValues(this.InternalInstance.OrganizationId, this.InternalInstance.InstanceId);
            }
        }

        private void InitClassVariables(Guid OrgId, Guid InstanceId, int DeptID)
        {
            Instance currentInstance = null;

            if (OrgId == Guid.Empty && Micajah.Common.Security.UserContext.Current != null && Micajah.Common.Security.UserContext.Current.SelectedOrganization != null) OrgId = Micajah.Common.Security.UserContext.Current.SelectedOrganization.OrganizationId;

            if (Micajah.Common.Security.UserContext.Current != null && Micajah.Common.Security.UserContext.Current.SelectedInstance != null && InstanceId == Micajah.Common.Security.UserContext.Current.SelectedInstance.InstanceId)
            {
                currentInstance = Micajah.Common.Security.UserContext.Current.SelectedInstance; // Current context
            }
            else
            {
                currentInstance = Micajah.Common.Bll.Providers.InstanceProvider.GetInstance(InstanceId, OrgId); // Looking through all dbs
            }

            if (currentInstance != null)
            {
                #region Load settings

                SettingCollection settings = currentInstance.Settings;
                InternalSettings = settings;
                InternalInstance = currentInstance;
                Setting setting = null;

                setting = settings.FindByShortName("configOnHoldStatus");
                if (setting != null) bool.TryParse(setting.Value, out OnHoldStatus); //On Hold Status
                setting = settings.FindByShortName("configTimetracking");
                if (setting != null) bool.TryParse(setting.Value, out TimeTracking); //Time Tracking
                setting = settings.FindByShortName("configPartsTracking");
                if (setting != null) bool.TryParse(setting.Value, out PartsTracking); //Parts Tracking
                setting = settings.FindByShortName("ConfigtechCheckin");
                if (setting != null) bool.TryParse(setting.Value, out TechCheckin); //Tech Checking
                setting = settings.FindByShortName("configLocationTracking");
                if (setting != null) bool.TryParse(setting.Value, out LocationTracking); //Location Tracking
                setting = settings.FindByShortName("btCfgKB");
                if (setting != null) bool.TryParse(setting.Value, out KBTracking); //Knowledgebase Tracking
                setting = settings.FindByShortName("btCfgProject");
                if (setting != null) bool.TryParse(setting.Value, out ProjectTracking); //Projects Tracking
                setting = settings.FindByShortName("configChooseTechnician");
                if (setting != null) bool.TryParse(setting.Value, out TechChoose);
                setting = settings.FindByShortName("ConfigNewUserWizard");
                if (setting != null) bool.TryParse(setting.Value, out NewUserWizard);
                setting = settings.FindByShortName("configSerialNumber");
                if (setting != null) bool.TryParse(setting.Value, out SerialNumber);
                setting = settings.FindByShortName("SerialNumberName");
                this.SerialNumberName = (setting == null) ? "Serial Number" : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("btCfgSchedTkt");
                if (setting != null) bool.TryParse(setting.Value, out ScheduledTickets);
                setting = settings.FindByShortName("configFolders");
                if (setting != null) bool.TryParse(setting.Value, out Folders);
                setting = settings.FindByShortName("configAssetTracking");
                if (setting != null) bool.TryParse(setting.Value, out AssetTracking);
                setting = settings.FindByShortName("configClassTracking");
                if (setting != null) bool.TryParse(setting.Value, out ClassTracking);
                setting = settings.FindByShortName("configPriorities");
                if (setting != null) bool.TryParse(setting.Value, out PrioritiesGeneral);
                setting = settings.FindByShortName("configUserPriorities");
                if (setting != null) bool.TryParse(setting.Value, out PrioritiesUser);
                setting = settings.FindByShortName("btCfgPRL");
                if (setting != null) bool.TryParse(setting.Value, out PrioritiesLog);
                setting = settings.FindByShortName("configChooseTechLimitQues");
                if (setting != null) bool.TryParse(setting.Value, out ChooseTechLimitQues);
                setting = settings.FindByShortName("configCategoryTracking");
                if (setting != null) bool.TryParse(setting.Value, out CategoryTracking);
                setting = settings.FindByShortName("btCfgRES");
                if (setting != null) bool.TryParse(setting.Value, out ResolutionTracking);
                setting = settings.FindByShortName("btCfgCON");
                if (setting != null) bool.TryParse(setting.Value, out ConfirmationTracking);
                setting = settings.FindByShortName("btCfgEAHA");
                if (setting != null) bool.TryParse(setting.Value, out AfterHoursAlert);
                setting = settings.FindByShortName("configCustomFields");
                if (setting != null) bool.TryParse(setting.Value, out CustomFields);
                setting = settings.FindByShortName("configMiscCost");
                if (setting != null) bool.TryParse(setting.Value, out MiscCosts);
                setting = settings.FindByShortName("configTrvlCost");
                if (setting != null) bool.TryParse(setting.Value, out TravelCosts);
                setting = settings.FindByShortName("configViewTktCost");
                if (setting != null) bool.TryParse(setting.Value, out ViewTktCosts);
                setting = settings.FindByShortName("configUAQ");
                if (setting != null) bool.TryParse(setting.Value, out UnassignedQue);
                setting = settings.FindByShortName("configLVL");
                if (setting != null) bool.TryParse(setting.Value, out TktLevels);
                setting = settings.FindByShortName("btConfigLVLUser");
                if (setting != null) bool.TryParse(setting.Value, out TktLevelsForUser);
                setting = settings.FindByShortName("configRCD");
                if (setting != null) bool.TryParse(setting.Value, out RequestCompletionDate);
                setting = settings.FindByShortName("btCfgAcctMngr");
                if (setting != null) bool.TryParse(setting.Value, out AccountManager);
                setting = settings.FindByShortName("btCfgACDC");
                if (setting != null) bool.TryParse(setting.Value, out TktClosureDate);
                setting = settings.FindByShortName("btCfgAFTE");
                if (setting != null) bool.TryParse(setting.Value, out ForceTimeEntry);
                setting = settings.FindByShortName("btCfgAECT");
                if (setting != null) bool.TryParse(setting.Value, out TktCreationTimeSupport);
                setting = settings.FindByShortName("btCfgSuppressBWALogos");
                if (setting != null) bool.TryParse(setting.Value, out SuppresBWALogos);
                setting = settings.FindByShortName("btCfgESG");
                if (setting != null) bool.TryParse(setting.Value, out SupportGroups);
                setting = settings.FindByShortName("btCfgIDM");
                if (setting != null) bool.TryParse(setting.Value, out TktIDMethod);
                setting = settings.FindByShortName("btCfgCCS");
                if (setting != null) bool.TryParse(setting.Value, out CallCenterSupport);
                setting = settings.FindByShortName("btCfgESC");
                if (setting != null) bool.TryParse(setting.Value, out SubmissionCategory);
                setting = settings.FindByShortName("btCfgEMTE");
                if (setting != null) bool.TryParse(setting.Value, out MultipleTimeEntry);
                setting = settings.FindByShortName("btCfgSBTP");
                if (setting != null) bool.TryParse(setting.Value, out SignatureBlockPrintedTkt);
                setting = settings.FindByShortName("btCfgCUSN");
                if (setting != null) bool.TryParse(setting.Value, out CustomNames);
                setting = settings.FindByShortName("btCfgACCT");
                if (setting != null) bool.TryParse(setting.Value, out AllowCCTktClosing);
                setting = settings.FindByShortName("btCfgRAST");
                if (setting != null) bool.TryParse(setting.Value, out RemoteAssistance);
                setting = settings.FindByShortName("btCfgASTT");
                if (setting != null) bool.TryParse(setting.Value, out AllowViewSchedTtkTech);
                setting = settings.FindByShortName("btCfgATCL");
                if (setting != null) bool.TryParse(setting.Value, out TechChooseLevel);
                setting = settings.FindByShortName("btCfgCFTO");
                if (setting != null) bool.TryParse(setting.Value, out DisplayCustFieldsForTech);
                setting = settings.FindByShortName("btCfgTROL");
                if (setting != null) bool.TryParse(setting.Value, out AllowTechReopenTicket);
                setting = settings.FindByShortName("btCfgTEDO");
                if (setting != null) bool.TryParse(setting.Value, out AllowTechEscalateDescalateOnly);
                setting = settings.FindByShortName("btReqClosureNote");
                if (setting != null) bool.TryParse(setting.Value, out TktRequireClosureNote);
                setting = settings.FindByShortName("btCfgLimTechAsts");
                if (setting != null) bool.TryParse(setting.Value, out LimitTechAbilityInAssets);
                setting = settings.FindByShortName("btGlobalSSL");
                if (setting != null) bool.TryParse(setting.Value, out HTTPSEnabled);
                setting = settings.FindByShortName("btCfgDTTCOT");
                if (setting != null) bool.TryParse(setting.Value, out DisableTransferTktsToCheckedOUTTechs);

                setting = settings.FindByShortName("btAllowSUserToChooseAnyLogin");
                if (setting != null) bool.TryParse(setting.Value, out AllowSUserToChooseAnyLogin);
                setting = settings.FindByShortName("btGeneralLdap");
                if (setting != null) bool.TryParse(setting.Value, out EnableLDAP);
                setting = settings.FindByShortName("btGlobalLDAP");
                if (setting != null) bool.TryParse(setting.Value, out EnableLDAPGlobal);
                setting = settings.FindByShortName("btLdapARLP");
                if (setting != null) bool.TryParse(setting.Value, out EnableLDAPAutoRedirect);
                setting = settings.FindByShortName("btLdap");
                if (setting != null) bool.TryParse(setting.Value, out EnableLDAPEmailExtUrl);
                setting = settings.FindByShortName("LdapLocalURL");
                LDAPLocalUrl = (setting == null || string.IsNullOrEmpty(setting.Value)) ? LDAPLocalUrl : setting.Value;
                setting = settings.FindByShortName("charCurrency");
                currency = (setting == null) ? String.Empty : setting.Value;
                setting = settings.FindByShortName("btCfgFLCN");
                if (setting != null) bool.TryParse(setting.Value, out ForceLowestClassNode);
                setting = settings.FindByShortName("btCfgATTU");
                if (setting != null) bool.TryParse(setting.Value, out AllowTransferTicketUser);
                setting = settings.FindByShortName("btDisplayAddUserLink");
                if (setting != null) bool.TryParse(setting.Value, out DisplayAddNewUserLink);
                setting = settings.FindByShortName("btEnterTimeOnTktDtl");
                if (setting != null) bool.TryParse(setting.Value, out AllowEnterTimeOnTicketDetail);
                setting = settings.FindByShortName("btTkt2PrjRelation");
                if (setting != null) bool.TryParse(setting.Value, out EnableTicketToProjectRelation);
                setting = settings.FindByShortName("btStartStopOnTktDtl");
                if (setting != null) bool.TryParse(setting.Value, out AllowStartStopTimeOnTicketDetail);
                setting = settings.FindByShortName("btRemHoursOnTktDtl");
                if (setting != null) bool.TryParse(setting.Value, out AllowRemainHoursOnTicketDetail);

                this.assets = new AssetsConfig((settings.FindByShortName("AssetsUnique1Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique1Caption")).Value
                    , (settings.FindByShortName("AssetsUnique2Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique2Caption")).Value
                    , (settings.FindByShortName("AssetsUnique3Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique3Caption")).Value
                    , (settings.FindByShortName("AssetsUnique4Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique4Caption")).Value
                    , (settings.FindByShortName("AssetsUnique5Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique5Caption")).Value
                    , (settings.FindByShortName("AssetsUnique6Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique6Caption")).Value
                    , (settings.FindByShortName("AssetsUnique7Caption") == null) ? string.Empty : (settings.FindByShortName("AssetsUnique7Caption")).Value
                    );

                setting = settings.FindByShortName("btCfgRTIP");
                if (setting != null) bool.TryParse(setting.Value, out RequireTktInitialPost);
                setting = settings.FindByShortName("btEnableGlobalAPI");
                if (setting != null) bool.TryParse(setting.Value, out EnableGlobalAPI);
                setting = settings.FindByShortName("btTimeEntriesOnTktLog");
                if (setting != null) bool.TryParse(setting.Value, out AllowTimeEntriesOnTktLog);
                setting = settings.FindByShortName("btCfgLWP");
                if (setting != null) bool.TryParse(setting.Value, out AllowUserLoginWithoutPassword);
                setting = settings.FindByShortName("btCfgERTS");
                if (setting != null) bool.TryParse(setting.Value, out RelatedTickets);
                
                this.BetaFeaturesEnabled = currentInstance.Beta;

                setting = settings.FindByShortName("emailSuffixes");
                EmailSuffixes = (setting == null) ? string.Empty : setting.Value;

                setting = settings.FindByShortName("AccLevelTimeTracking");
                if (setting != null) bool.TryParse(setting.Value, out AccLevelTimeTracking);
                setting = settings.FindByShortName("tintCfgEParserV2");
                if (setting != null) byte.TryParse(setting.Value, out EMailParserV2);
                setting = settings.FindByShortName("btBCIntegration");
                if (setting != null) bool.TryParse(setting.Value, out BCIntegration);
                setting = settings.FindByShortName("CfgAssetAuditor");
                if (setting != null) bool.TryParse(setting.Value, out EnableAssetAuditor);
                setting = settings.FindByShortName("WarehouseIntegrationEnabled");
                if (setting != null) bool.TryParse(setting.Value, out WarehouseIntegrationEnabled);
                setting = settings.FindByShortName("btQBIntegration");
                if (setting != null) bool.TryParse(setting.Value, out QBIntegration);
                setting = settings.FindByShortName("btCfgSupportPortal");
                if (setting != null) bool.TryParse(setting.Value, out SupportPortal);

                setting = settings.FindByShortName("hrIncrement");
                if (setting != null) decimal.TryParse(setting.Value, out HourIncrement);
                setting = settings.FindByShortName("MinLogTime");
                if (setting != null) decimal.TryParse(setting.Value, out MinimumLoggableTime);
                setting = settings.FindByShortName("DefaultDistanceRate");
                if (setting != null) decimal.TryParse(setting.Value, out DefaultDistanceRate);

                bool limitNewUsersToKnownEmailSuffixes = false;
                bool disableCSS = false;
                setting = settings.FindByShortName("btSPLimitNewUsersToKnownEmailSuffixes");
                if (setting != null) bool.TryParse(setting.Value, out limitNewUsersToKnownEmailSuffixes);
                setting = settings.FindByShortName("SPDisableCSS");
                if (setting != null) bool.TryParse(setting.Value, out disableCSS);

                support =
                    new SupportPortalConfig(limitNewUsersToKnownEmailSuffixes,
                        (settings.FindByShortName("SupportPhone") == null) ? string.Empty : (settings.FindByShortName("SupportPhone")).Value,
                        (settings.FindByShortName("SupportEmail") == null) ? string.Empty : (settings.FindByShortName("SupportEmail")).Value,
                        (settings.FindByShortName("SPTitle") == null) ? string.Empty : (settings.FindByShortName("SPTitle")).Value,
                        (settings.FindByShortName("LogoBackLinkURL") == null) ? string.Empty : (settings.FindByShortName("LogoBackLinkURL")).Value,
                        disableCSS,
                        (settings.FindByShortName("SPFacebook") == null) ? string.Empty : (settings.FindByShortName("SPFacebook")).Value,
                        (settings.FindByShortName("SPTwitter1") == null) ? string.Empty : (settings.FindByShortName("SPTwitter1")).Value,
                        (settings.FindByShortName("SPTwitter2") == null) ? string.Empty : (settings.FindByShortName("SPTwitter2")).Value);

                // New settings from tbl_company
                setting = settings.FindByShortName("intLicensedUsers");
                if (setting != null) int.TryParse(setting.Value, out LicensedUsers);
                setting = settings.FindByShortName("configHourlyRate");
                if (setting != null) decimal.TryParse(setting.Value, out HourlyRate);
                setting = settings.FindByShortName("TktExportdtStart");
                if (setting != null) DateTime.TryParse(setting.Value, out TktExportStart);
                setting = settings.FindByShortName("TktExportdtEnd");
                if (setting != null) DateTime.TryParse(setting.Value, out TktExportEnd);
                setting = settings.FindByShortName("intLicensedAssets");
                if (setting != null) int.TryParse(setting.Value, out LicensedAssets);
                setting = settings.FindByShortName("configExpectDueDateUser");
                if (setting != null) bool.TryParse(setting.Value, out ExpectDueDateUser);
                setting = settings.FindByShortName("tinyBusHourStart");
                if (setting != null) int.TryParse(setting.Value, out BusHourStart);
                setting = settings.FindByShortName("tinyBusMinStart");
                if (setting != null) int.TryParse(setting.Value, out BusMinStart);
                setting = settings.FindByShortName("tinyBusHourStop");
                if (setting != null) int.TryParse(setting.Value, out BusHourStop);
                setting = settings.FindByShortName("tinyBusMinStop");
                if (setting != null) int.TryParse(setting.Value, out BusMinStop);
                this.WorkingDays = currentInstance.WorkingDays;
                setting = settings.FindByShortName("tintRoutingOrder");
                if (setting != null) int.TryParse(setting.Value, out RoutingOrder);
                setting = settings.FindByShortName("btAcctRtOvr");
                if (setting != null) bool.TryParse(setting.Value, out AcctRtOvr);
                setting = settings.FindByShortName("tintAcctRtOvrLevel");
                if (setting != null) int.TryParse(setting.Value, out AcctRtOvrLevel);
                setting = settings.FindByShortName("tintAcctRtOvrOption");
                if (setting != null) int.TryParse(setting.Value, out AcctRtOvrOption);
                setting = settings.FindByShortName("tintAcctRtOvrRouteType");
                if (setting != null) int.TryParse(setting.Value, out AcctRtOvrRouteType);
                setting = settings.FindByShortName("RemoteAssistanceSessions");
                if (setting != null) int.TryParse(setting.Value, out RemoteAssistanceSessions);
                setting = settings.FindByShortName("CreationCatId");
                if (setting != null) int.TryParse(setting.Value, out CreationCatId);
                setting = settings.FindByShortName("tintTicketTimer");
                if (setting != null) int.TryParse(setting.Value, out TicketTimer);
                setting = settings.FindByShortName("intUTROL");
                if (setting != null) int.TryParse(setting.Value, out TicketReopenLimitUser);
                setting = settings.FindByShortName("intTTROL");
                if (setting != null) int.TryParse(setting.Value, out TicketReopenLimitTechnician);
                setting = settings.FindByShortName("configHourlyBillableRate");
                if (setting != null) decimal.TryParse(setting.Value, out HourlyBillableRate);
                setting = settings.FindByShortName("CfgQBAccount");
                QBAccount = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("btFBIntegration");
                if (setting != null) bool.TryParse(setting.Value, out FBIntegration);
                setting = settings.FindByShortName("FBClientId");
                if (setting != null) int.TryParse(setting.Value, out FBClientId);
                setting = settings.FindByShortName("FBEndPointURL");
                FBEndPointURL = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("FBToken");
                FBToken = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("BCEndPointURL");
                BCEndPointURL = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("BCToken");
                BCToken = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("CfgInternalQBAccount");
                InternalQBAccount = (setting == null) ? string.Empty : Convert.ToString(setting.Value);                
                setting = settings.FindByShortName("btCfgBilling");
                if (setting != null) bool.TryParse(setting.Value, out EnableBilling);
                setting = settings.FindByShortName("btCfgStaffPayments");
                if (setting != null) bool.TryParse(setting.Value, out EnablePayments);

                setting = settings.FindByShortName("configUCP");
                if (setting != null) bool.TryParse(setting.Value, out PrioritiesUserCreateTicket);
                setting = settings.FindByShortName("btCfgToDo");
                if (setting != null) bool.TryParse(setting.Value, out EnableToDo);
                setting = settings.FindByShortName("btCfgContactUsWidget");
                if (setting != null) bool.TryParse(setting.Value, out EnableContactUsWidget);
                setting = settings.FindByShortName("CUWClassID");
                if (setting != null) int.TryParse(setting.Value, out CUWClassID);
                setting = settings.FindByShortName("FBURL");
                FBURL = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("FBAccessToken");
                FBAccessToken = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                setting = settings.FindByShortName("FBAccessTokenSecret");
                FBAccessTokenSecret = (setting == null) ? string.Empty : Convert.ToString(setting.Value);
                #endregion
            }

            try
            {
                DataTable deploymentLoggerData = BWA.bigWebDesk.BLL.DeploymentLogger.GetCompanyData(DBAccess.GetCurrentOrgID(Guid.Empty), DeptID);
                if (deploymentLoggerData != null && deploymentLoggerData.Rows.Count > 0)
                {
                    this.DeploymentLoggerSettedUp = true;
                    this.DeploymentLoggerEnabled = (bool)deploymentLoggerData.Rows[0]["Enabled"];
                }
            }
            catch { }
        }

        public override string ToString()
        {
            string returnString = string.Empty;
            if (this.OnHoldStatus)
                returnString += "~OHS";
            if (this.TimeTracking)
                returnString += "~TIT";
            if (this.PartsTracking)
                returnString += "~PRT"; //Parts Tracking
            if (this.TechCheckin)
                returnString += "~TCH"; //TechCheckin
            if (this.LocationTracking)
                returnString += "~LTR"; //Location Tracking
            if (this.TechChoose)
                returnString += "~CTN"; //Choose Technician
            if (this.NewUserWizard)
                returnString += "~NUW"; //New User Wizard 'This one may should not be permanent
            if (this.SerialNumber)
                returnString += "~SNB"; //Enable Serial number
            if (this.ScheduledTickets)
                returnString += "~SCM"; //Scheduled Tickets
            if (this.Folders)
                returnString += "~FLD"; //Folders
            if (this.AssetTracking)
                returnString += "~ATR"; //Asset Tracking
            if (this.ClassTracking)
                returnString += "~CLT"; //Class Tracking
            if (this.PrioritiesGeneral)
                returnString += "~PRG"; //Priorities General
            if (this.PrioritiesUser)
                returnString += "~PRU"; //Priorities User
            if (this.PrioritiesLog)
                returnString += "~PRL"; //Priorities Log
            if (this.ChooseTechLimitQues)
                returnString += "~TLQ"; //ChooseTechLimitQues
            if (this.CategoryTracking)
                returnString += "~CAT"; //Category Tracking
            if (this.ResolutionTracking)
                returnString += "~RES"; //Resolution Tracking
            if (this.ConfirmationTracking)
                returnString += "~CON"; //Confirmation Tracking
            if (this.AfterHoursAlert)
                returnString += "~AHA"; //After hours alert
            if (this.CustomFields)
                returnString += "~CUF"; //Cust Fields
            if (this.MiscCosts)
                returnString += "~MCT"; //Misc Costs
            if (this.TravelCosts)
                returnString += "~TCT"; //Travel Costs
            if (this.ViewTktCosts)
                returnString += "~VCT"; //View Tkt Costs
            if (this.UnassignedQue)
                returnString += "~UAQ"; //Unassigned Que
            if (this.TktLevels)
                returnString += "~LVL"; //TktLevels
            if (this.TktLevelsForUser)
                returnString += "~LVU"; //TktLevels for Users
            if (this.RequestCompletionDate)
                returnString += "~RCD"; //Scheduled Date
            if (this.AccountManager)
                returnString += "~ACT"; //Account Manager
            if (this.TktClosureDate)
                returnString += "~CDC"; //Allow ticket closure date to be changed
            if (this.ForceTimeEntry)
                returnString += "~FTE"; //Force Time entry support
            if (this.TktCreationTimeSupport)
                returnString += "~ECT"; //Added Allow edit ticket creation time support
            if (this.SuppresBWALogos)
                returnString += "~SLG"; //Added ability to suppress BWA logos and branding
            if (this.SupportGroups)
                returnString += "~ESG"; //Added support groups
            if (this.TktIDMethod)
                returnString += "~IDM"; //Added Ticket Id Method
            if (this.CallCenterSupport)
                returnString += "~CCS"; //Added Call Center Support
            if (this.SubmissionCategory)
                returnString += "~ESC"; //Added Enable Submission Category
            if (this.MultipleTimeEntry)
                returnString += "~MTE"; //Added Enable Multiple Time Entry for Technicians
            if (this.SignatureBlockPrintedTkt)
                returnString += "~SBT"; //Added Enable Signature Block on Printed Tickets 
            if (this.CustomNames)
                returnString += "~CSN"; //Custom Names feauture
            if (this.AllowCCTktClosing)
                returnString += "~CCT"; //Added Allow CC EMails on ticket closing
            if (this.RemoteAssistance)
                returnString += "~RAS"; //Enable Remote Assistance support
            if (this.AllowViewSchedTtkTech)
                returnString += "~STT"; //Allow technicians view all scheduled tickets
            if (this.TechChooseLevel)
                returnString += "~TCL"; //Allow technicians to choose Level during Ticket Creation
            if (this.DisplayCustFieldsForTech)
                returnString += "~CFT"; //Display ticket custom fields for technicians only.
            // YMYKYTYUK 20-JAN-2009:option DisplayTicketMsgBoxesAsLinks is obsolete and should be removed
            // Search other places by ~DTMBAL~
            /*if (this.DisplayTicketMsgBoxesAsLinks)
                returnString += "~MAL"; //Display ticket message boxes as links.*/
            if (this.AllowTechReopenTicket)
                returnString += "~TRL"; //Allow Technician to reopen tickets after reopen limit
            if (this.AllowTechEscalateDescalateOnly)
                returnString += "~TED"; //Allow Technician to escalate/descalate ticket only
            if (this.TktRequireClosureNote)
                returnString += "~RCN"; //Requires Ticket Closure Note
            if (this.DeploymentLoggerEnabled)
                returnString += "~DLE"; //Is Deployment Logger enabled
            if (this.DeploymentLoggerSettedUp)
                returnString += "~DLS"; //Is Deployment Logger set up
            if (this.EnableNotificationRulesAdminSection)
                returnString += "~NRA"; //Enable Notification Rules Admin Section
            if (this.LimitTechAbilityInAssets)
                returnString += "~LTA"; //Limit technicians in assets management
            if (this.HTTPSEnabled)
                returnString += "~SSL"; //Is use HTTPS enabled

            if (this.DisableTransferTktsToCheckedOUTTechs)
                returnString += "~DTT"; //Disable transfer ticket to Checked out tech

            if (this.EnableLDAPGlobal)
                returnString += "~ADG";
            if (this.EnableLDAP)
                returnString += "~ADL";
            if (this.ForceLowestClassNode)
                returnString += "~FLC";
            if (this.AllowTransferTicketUser)
                returnString += "~TTU";
            if (this.DisplayAddNewUserLink)
                returnString += "~NUL";
            if (this.AllowEnterTimeOnTicketDetail)
                returnString += "~ETD";
            if (this.EnableTicketToProjectRelation)
                returnString += "~TPR";
            if (this.AllowStartStopTimeOnTicketDetail)
                returnString += "~SST";
            if (this.AllowRemainHoursOnTicketDetail)
                returnString += "~ARH";
            if (this.RequireTktInitialPost)
                returnString += "~RIP";
            if (this.EnableGlobalAPI)
                returnString += "~EGA";
            if (this.AllowTimeEntriesOnTktLog)
                returnString += "~TET";
            if (this.AllowUserLoginWithoutPassword)
                returnString += "~LWP";
            if (this.RelatedTickets)
                returnString += "~ERT";
            if (this.BetaFeaturesEnabled)
                returnString += "~EBT";
            if (this.EnableAssetAuditor)
                returnString += "~EAA";
            if (this.EnableBilling)
                returnString += "~BIL";
            if (this.EnablePayments)
                returnString += "~PAY";
            if (this.PrioritiesUserCreateTicket)
                returnString += "~PUC";
            if (this.EnableToDo)
                returnString += "~TOD";
            if (this.EnableContactUsWidget)
                returnString += "~CUW";
            if (returnString != string.Empty)
                returnString += "~";

            return returnString;
        }

        #endregion
    }

    [DataContract(Name = "AssetsConfig")]
    public class AssetsConfig
    {
        string unique1Caption;
        string unique2Caption;
        string unique3Caption;
        string unique4Caption;
        string unique5Caption;
        string unique6Caption;
        string unique7Caption;

        [DataMember]
        public string Unique1Caption
        {
            get { return unique1Caption; }
            set { unique1Caption = value; }
        }

        [DataMember]
        public string Unique2Caption
        {
            get { return unique2Caption; }
            set { unique2Caption = value; }
        }

        [DataMember]
        public string Unique3Caption
        {
            get { return unique3Caption; }
            set { unique3Caption = value; }
        }

        [DataMember]
        public string Unique4Caption
        {
            get { return unique4Caption; }
            set { unique4Caption = value; }
        }

        [DataMember]
        public string Unique5Caption
        {
            get { return unique5Caption; }
            set { unique5Caption = value; }
        }

        [DataMember]
        public string Unique6Caption
        {
            get { return unique6Caption; }
            set { unique6Caption = value; }
        }

        [DataMember]
        public string Unique7Caption
        {
            get { return unique7Caption; }
            set { unique7Caption = value; }
        }

        public AssetsConfig()
        {
            this.unique1Caption =
            this.unique2Caption =
            this.unique3Caption =
            this.unique4Caption =
            this.unique5Caption =
            this.unique6Caption =
            this.unique7Caption = string.Empty;
        }

        public AssetsConfig(string unique1Caption, string unique2Caption, string unique3Caption, string unique4Caption, string unique5Caption, string unique6Caption, string unique7Caption)
        {
            this.unique1Caption = unique1Caption;
            this.unique2Caption = unique2Caption;
            this.unique3Caption = unique3Caption;
            this.unique4Caption = unique4Caption;
            this.unique5Caption = unique5Caption;
            this.unique6Caption = unique6Caption;
            this.unique7Caption = unique7Caption;
        }
    }

    public class SupportPortalConfig
    {
        private bool m_LimitNewUsersToKnownEmailSuffixes = false;
        private string m_Phone = string.Empty;
        private string m_Email = string.Empty;
        private string m_Title = string.Empty;
        private string m_LogoBackLinkURL = string.Empty;
        private bool m_DisableCSS = false;
        private string m_Facebook = string.Empty;
        private string m_Twitter1 = string.Empty;
        private string m_Twitter2 = string.Empty;

        public bool LimitNewUsersToKnownEmailSuffixes
        {
            get { return m_LimitNewUsersToKnownEmailSuffixes; }
            set { m_LimitNewUsersToKnownEmailSuffixes = value; }
        }

        public string Phone
        {
            get { return m_Phone; }
            set { m_Phone = value; }
        }

        public string Email
        {
            get { return m_Email; }
            set { m_Email = value; }
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public string LogoBackLinkURL
        {
            get { return m_LogoBackLinkURL; }
            set { m_LogoBackLinkURL = value; }
        }

        public bool DisableCSS
        {
            get { return m_DisableCSS; }
            set { m_DisableCSS = value; }
        }

        public string Facebook
        {
            get { return m_Facebook; }
            set { m_Facebook = value; }
        }

        public string Twitter1
        {
            get { return m_Twitter1; }
            set { m_Twitter1 = value; }
        }

        public string Twitter2
        {
            get { return m_Twitter2; }
            set { m_Twitter2 = value; }
        }

        public SupportPortalConfig()
        {
            this.LimitNewUsersToKnownEmailSuffixes = false;
            this.Phone =
            this.Email =
            this.Title =
            this.LogoBackLinkURL =
            this.Facebook =
            this.Twitter1 =
            this.Twitter2 = string.Empty;
            this.DisableCSS = false;
        }

        public SupportPortalConfig(bool limitNewUsersToKnownEmailSuffixes, string phone, string email,
            string title, string logoBackLinkURL, bool disableCSS, string facebook, string twitter1, string twitter2)
        {
            this.LimitNewUsersToKnownEmailSuffixes = limitNewUsersToKnownEmailSuffixes;
            this.Phone = phone;
            this.Email = email;
            this.Title = title;
            this.LogoBackLinkURL = logoBackLinkURL;
            this.DisableCSS = disableCSS;
            this.Facebook = facebook;
            this.Twitter1 = twitter1;
            this.Twitter2 = twitter2;
        }
    }
}
