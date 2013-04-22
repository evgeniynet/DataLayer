using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for ScheduledTickets.
    /// </summary>
    public class ScheduledTickets : DBAccess
    {

        /// <summary>
        /// Forces creation of new ticket from Scheduled ticket
        /// </summary>
        /// <returns>Id of the created ticket</returns>
        public static int RunScheduledTicket(int DepartmentId, int ScheduledTicketId, bool IsManualLaunch, out int initialPostId)
        {
            int error = 0;
            initialPostId = 0;

            CustomNames customNames = CustomNames.GetCustomNames(DepartmentId);

            DataRow rScheduledTicket = SelectOne(DepartmentId, ScheduledTicketId);

            int ownerId = 0;
            int userId = 0;
            int technicianId = 0;
            int locationId = 0;
            int classId = 0;
            int priorityId = 0;
            int levelId = 0;
            int creationCategoryId = 0;
            string assetSerial = "";
            string idMethod = "";
            string customXML = "";
            DateTime now = DateTime.UtcNow;
            string rTicketSubjsect = "";
            string rTicketText = "";
            byte rTicketEndCount = 0;
            bool rTicketEnabled = false;
            DateTime rTicketNext = DateTime.MinValue;
            string rTicketRecurringOn = "";
            int rTicketRecurringFrequency = 0;
            string rTicketEndMethod = "";
            DateTime rTicketStop = DateTime.MinValue;
            int rTicketAccountId = 0;
            int rTicketAccountLocationId = 0;
            int rProjectId = 0;
            int rFolderId = 0;


            if (rScheduledTicket["intOwnerId"] != DBNull.Value)
                ownerId = (int)rScheduledTicket["intOwnerId"];

            if (rScheduledTicket["intUserId"] != DBNull.Value)
                userId = (int)rScheduledTicket["intUserId"];

            if (rScheduledTicket["intTechId"] != DBNull.Value)
                technicianId = (int)rScheduledTicket["intTechId"];

            if (rScheduledTicket["LocationId"] != DBNull.Value)
                locationId = (int)rScheduledTicket["LocationId"];

            if (rScheduledTicket["intClassId"] != DBNull.Value)
                classId = (int)rScheduledTicket["intClassId"];

            if (rScheduledTicket["intPriorityId"] != DBNull.Value)
                priorityId = (int)rScheduledTicket["intPriorityId"];

            if (rScheduledTicket["tintLevel"] != DBNull.Value)
                levelId = (byte)rScheduledTicket["tintLevel"];

            if (rScheduledTicket["CreationCatsId"] != DBNull.Value)
                creationCategoryId = (int)rScheduledTicket["CreationCatsId"];

            if (rScheduledTicket["vchAssetSerial"] != DBNull.Value)
                assetSerial = (string)rScheduledTicket["vchAssetSerial"];

            if (rScheduledTicket["vchIdMethod"] != DBNull.Value)
                idMethod = (string)rScheduledTicket["vchIdMethod"];

            if (rScheduledTicket["CustomXML"] != DBNull.Value)
                customXML = (string)rScheduledTicket["CustomXML"];

            if (rScheduledTicket["vchSubject"] != DBNull.Value)
                rTicketSubjsect = (string)rScheduledTicket["vchSubject"];

            if (rScheduledTicket["vchText"] != DBNull.Value)
                rTicketText = (string)rScheduledTicket["vchText"];
            rTicketText += "<br><br>This " + customNames.Ticket.FullSingular + " was created via Scheduled " + customNames.Ticket.FullPlural + ".";

            if (rScheduledTicket["tintEndCount"] != DBNull.Value)
                rTicketEndCount = (byte)rScheduledTicket["tintEndCount"];

            if (rScheduledTicket["btEnabled"] != DBNull.Value)
                rTicketEnabled = (bool)rScheduledTicket["btEnabled"];

            if (rScheduledTicket["dtNext"] != DBNull.Value)
                rTicketNext = (DateTime)rScheduledTicket["dtNext"];

            if (rScheduledTicket["vchRecurringOn"] != DBNull.Value)
                rTicketRecurringOn = (string)rScheduledTicket["vchRecurringOn"];

            if (rScheduledTicket["tintRecurringFeq"] != DBNull.Value)
                rTicketRecurringFrequency = (byte)rScheduledTicket["tintRecurringFeq"];

            if (rScheduledTicket["vchEndMethod"] != DBNull.Value)
                rTicketEndMethod = (string)rScheduledTicket["vchEndMethod"];

            if (rScheduledTicket["dtStop"] != DBNull.Value)
                rTicketStop = (DateTime)rScheduledTicket["dtStop"];

            if (rScheduledTicket["IntAcctId"] != DBNull.Value)
                rTicketAccountId = (int)rScheduledTicket["IntAcctId"];

            if (rScheduledTicket["AccountLocationId"] != DBNull.Value)
                rTicketAccountLocationId = (int)rScheduledTicket["AccountLocationId"];

            if (!rScheduledTicket.IsNull("ProjectID")) rProjectId = (int)rScheduledTicket["ProjectID"];

            if (!rScheduledTicket.IsNull("FolderID")) rFolderId = (int)rScheduledTicket["FolderID"];


            switch (rTicketEndMethod)
            {
                case "Date":
                    if (IsManualLaunch && rTicketStop < rTicketNext) error = -1;
                    else if (!IsManualLaunch && rTicketStop < now) error = -1;
                    break;
                case "Times":
                    if (rTicketEndCount > 0)
                        rTicketEndCount--;
                    else
                        error = -2;
                    rTicketEnabled = rTicketEndCount > 0;
                    break;
            }

            if (rTicketRecurringOn != "no")
            {
                string daysOfWeek = rTicketRecurringOn.Substring(1);
                int totalDifference = daysOfWeek.Length > 0 ? Functions.GetWeekdayIndex(rTicketNext) - int.Parse(daysOfWeek[0].ToString()) : 0;

                if (IsManualLaunch)
                {
                    switch (rTicketRecurringOn[0])
                    {
                        case 'd':
                            rTicketNext = rTicketNext.AddDays(rTicketRecurringFrequency);
                            break;
                        case 'w':
                            int differenceToNextDay = DifferenceToNextDayOfWeek(daysOfWeek, Functions.GetWeekdayIndex(rTicketNext));

                            if (differenceToNextDay == -1)
                            {
                                rTicketNext = rTicketNext.AddDays(rTicketRecurringFrequency * 7 - totalDifference);
                                totalDifference = 0;
                            }
                            else
                            {
                                rTicketNext = rTicketNext.AddDays(differenceToNextDay);
                                totalDifference += differenceToNextDay;
                            }
                            break;
                        case 'm':
                            rTicketNext = rTicketNext.AddMonths(rTicketRecurringFrequency);
                            break;
                        default:
                            error = -3;
                            rTicketEnabled = false;
                            break;
                    }
                }
                else
                {
                    while (rTicketNext <= now)
                    {
                        switch (rTicketRecurringOn[0])
                        {
                            case 'd':
                                rTicketNext = rTicketNext.AddDays(rTicketRecurringFrequency);
                                break;
                            case 'w':
                                int differenceToNextDay = DifferenceToNextDayOfWeek(daysOfWeek, Functions.GetWeekdayIndex(rTicketNext));

                                if (differenceToNextDay == -1)
                                {
                                    rTicketNext = rTicketNext.AddDays(rTicketRecurringFrequency * 7 - totalDifference);
                                    totalDifference = 0;
                                }
                                else
                                {
                                    rTicketNext = rTicketNext.AddDays(differenceToNextDay);
                                    totalDifference += differenceToNextDay;
                                }
                                break;
                            case 'm':
                                rTicketNext = rTicketNext.AddMonths(rTicketRecurringFrequency);
                                break;
                            default:
                                error = -3;
                                rTicketEnabled = false;
                                break;
                        }
                    }
                }
            }
            else
                rTicketEnabled = false;

            rTicketEnabled = error == 0;

            if (error < 0)
            {
                UpdateDates(DepartmentId, ScheduledTicketId, rTicketEnabled, rTicketNext, rTicketEndCount);
                if (!IsManualLaunch && !rTicketEnabled) Data.NotificationEventsQueue.DeleteEvents(DepartmentId, ScheduledTicketId);
                return error;
            }

            int newTicketId = Data.Tickets.CreateNew(Guid.Empty, DepartmentId, ownerId, technicianId, userId, DateTime.MinValue, rTicketAccountId, rTicketAccountLocationId, false, locationId,
                classId, levelId, "Scheduled/Recurring Ticket", false, creationCategoryId, true, priorityId, DateTime.MinValue, "", assetSerial, SelectTicketAssets(DepartmentId, ScheduledTicketId), idMethod, customXML,
                rTicketSubjsect, rTicketText, null, "Open", out initialPostId, rProjectId, rFolderId, ScheduledTicketId, -1);

            if (newTicketId > 0)
            {
                UpdateDates(DepartmentId, ScheduledTicketId, rTicketEnabled, rTicketNext, rTicketEndCount);
                if (!IsManualLaunch)
                {
                    FileItem[] fileItems = NotificationEventsQueue.SelectNotificationEventFilesToArray(DepartmentId, ScheduledTicketId);
                    if (rTicketEnabled) NotificationRules.RaiseNotificationEvent(DepartmentId, userId, ScheduledTicketId, rTicketNext, fileItems);
                    else NotificationEventsQueue.DeleteEvents(DepartmentId, ScheduledTicketId);
                }
            }

            return newTicketId;
        }

        private static int DifferenceToNextDayOfWeek(string daysOfWeek, int currentDayOfWeek)
        {
            int tmpDayOfWeek = currentDayOfWeek;
            int currentDayOfWeekPosition = -1;
            do
            {
                currentDayOfWeekPosition = daysOfWeek.IndexOf(tmpDayOfWeek.ToString());
                tmpDayOfWeek++;
            } while (currentDayOfWeekPosition == -1 && tmpDayOfWeek <= 7);

            if (currentDayOfWeekPosition == -1)
                return -1;

            if (Convert.ToInt32(daysOfWeek[currentDayOfWeekPosition].ToString()) == currentDayOfWeek)
                currentDayOfWeekPosition++;

            if (currentDayOfWeekPosition < daysOfWeek.Length)
                return Convert.ToInt32(daysOfWeek[currentDayOfWeekPosition].ToString()) - currentDayOfWeek;

            return -1;
        }

        public static DataTable SelectByAccount(int DeptID, int AccountID, int UserId)
        {
            return SelectRecords("sp_SelectSchedTkts", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountID), UserId == 0 ? new SqlParameter("@UId", DBNull.Value) : new SqlParameter("@UId", UserId) });
        }

        public static void SelectAccountInfo(int DeptID, int AccountID, out string AccountName, out int LocationId, out string LocationName)
        {
            SqlParameter _pAccName = new SqlParameter("@vchAcctName", SqlDbType.VarChar, 100);
            _pAccName.Direction = ParameterDirection.Output;
            SqlParameter _pLocID = new SqlParameter("@LocationId", SqlDbType.Int);
            _pLocID.Direction = ParameterDirection.Output;
            SqlParameter _pLocName = new SqlParameter("@LocationName", SqlDbType.VarChar, 2000);
            _pLocName.Direction = ParameterDirection.Output;
            SqlCommand _cmd = CreateSqlCommand("sp_SelectAcctSchedTkt", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@AcctId", AccountID), _pAccName, _pLocID, _pLocName });
            if (_cmd.Connection.State == ConnectionState.Closed) _cmd.Connection.Open();
            _cmd.ExecuteNonQuery();
            AccountName = _pAccName.Value.ToString();
            if (_pLocID.Value == DBNull.Value) LocationId = 0;
            else LocationId = (int)_pLocID.Value;
            LocationName = _pLocName.Value.ToString();
            _cmd.Connection.Close();
        }

        public static Data.Assets.AssetItem[] SelectTicketAssets(int DeptID, int SchedTicketId)
        {
            DataTable _dt = SelectRecords("sp_SelectSchedTicketAssets", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SchedTicketId", SchedTicketId) });
            Data.Assets.AssetItem[] _arr = new Assets.AssetItem[_dt.Rows.Count];
            for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new Assets.AssetItem((int)_dt.Rows[i]["AssetId"], _dt.Rows[i]["Description"].ToString());
            return _arr;
        }

        public static Data.FileItem[] SelectTicketFiles(int DeptID, int SchedTicketId)
        {
            DataTable _dt = SelectRecords("sp_SelectSchedTicketFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SchedTicketId", SchedTicketId) });
            Data.FileItem[] _arr = new Data.FileItem[_dt.Rows.Count];
            for (int i = 0; i < _dt.Rows.Count; i++) _arr[i] = new Data.FileItem((int)_dt.Rows[i]["Id"], _dt.Rows[i]["FileName"].ToString(), (int)_dt.Rows[i]["FileSize"], (DateTime)_dt.Rows[i]["dtUpdated"], (byte[])_dt.Rows[i]["FileData"]);
            return _arr;
        }

        public static DataRow SelectTicketFile(int DeptID, int SchedTicketId, int FileId)
        {
            return SelectRecord("sp_SelectSchedTicketFiles", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SchedTicketId", SchedTicketId), new SqlParameter("@Id", FileId) });
        }

        public static DataRow SelectOne(int DeptID, int Id)
        {
            return SelectRecord("sp_SelectSchedTkt", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", Id) });
        }

        public static void DeleteAsset(int DeptID, int SchedTicketId, int AssetId)
        {
            UpdateData("sp_DeleteSchedTicketAsset", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@SchedTicketId", SchedTicketId), new SqlParameter("@AssetId", AssetId) });
        }

        public static void Delete(int DeptID, int SchedTicketId)
        {
            Data.NotificationEventsQueue.DeleteEvents(DeptID, SchedTicketId);
            UpdateData("sp_DeleteSchedTkt", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", SchedTicketId) });
        }

        public static int UpdateAsset(int DeptID, int Id, int SchedTicketId, int AssetId, string Description)
        {
            SqlParameter _pId = new SqlParameter("@Id", SqlDbType.Int);
            _pId.Direction = ParameterDirection.InputOutput;
            if (Id != 0) _pId.Value = Id;
            else _pId.Value = DBNull.Value;
            UpdateData("sp_UpdateSchedTicketAsset", new SqlParameter[] { _pId, new SqlParameter("@DId", DeptID), new SqlParameter("@SchedTicketId", SchedTicketId), new SqlParameter("@AssetId", AssetId), new SqlParameter("@Description", Description) });
            return (int)_pId.Value;
        }

        public static void DeleteFile(int DeptID, int SchedTicketId, int FileId)
        {
            UpdateData("sp_DeleteSchedTicketFile", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@STId", SchedTicketId), new SqlParameter("@FId", FileId) });
        }

        public static int InsertAsset(int DeptID, int SchedTicketId, int AssetId, string Description)
        {
            return UpdateAsset(DeptID, 0, SchedTicketId, AssetId, Description);
        }

        public static int InsertFile(int DeptID, int SchedTicketId, string FileName, int FileSize, byte[] FileData)
        {
            return UpdateFile(DeptID, 0, SchedTicketId, FileName, FileSize, FileData);
        }

        public static int UpdateFile(int DeptID, int Id, int SchedTicketId, string FileName, int FileSize, byte[] FileData)
        {
            SqlParameter _pFileData = new SqlParameter("@FileData", SqlDbType.Image);
            _pFileData.Value = FileData;
            UpdateData("sp_UpdateSchedTktFile", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@STId", SchedTicketId), new SqlParameter("@FileName", FileName), new SqlParameter("@FileSize", FileSize), _pFileData });
            return 0;
        }

        public static void UpdateDates(Guid orgID, int DeptID, int SchedTicketId, bool isEnabled, DateTime nextRunDate, byte endCount)
        {

            UpdateData("sp_UpdateSchedTktDates", new SqlParameter[] { 
                new SqlParameter("@DId", DeptID), 
                new SqlParameter("@Id", SchedTicketId), 
                new SqlParameter("@btEnabled", isEnabled),
                new SqlParameter("@dtNext", nextRunDate),
                new SqlParameter("@tintEndCount", endCount)
            }, orgID);
        }

        public static void UpdateDates(int DeptID, int SchedTicketId, bool isEnabled, DateTime nextRunDate, byte endCount)
        {
            UpdateDates(Guid.Empty, DeptID, SchedTicketId, isEnabled, nextRunDate, endCount);
        }

        public static int Update(int DeptID,
            int SchedTktID,
            int UserId,
            int OwnerId,
            int TechId,
            int LocationId,
            int PriorityId,
            int ClassId,
            int CategoryId,
            int LevelId,
            string TktSubject,
            string TktText,
            DateTime DateNext,
            string ReccuringOn,
            string MethodEnd,
            int EndCount,
            DateTime DateStop,
            int ReccuringFeq,
            bool Enabled,
            string UserEmail,
            int AccountId,
            int AccountLocationId,
            string CustomFields,
            string IdMethod,
            int projectID,
            int folderID,
            bool repeatFromCompletion,
            Assets.AssetItem[] assets)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pSchedTktId = new SqlParameter("@Id", SqlDbType.Int);
            if (SchedTktID != 0) _pSchedTktId.Value = SchedTktID;
            else _pSchedTktId.Value = DBNull.Value;
            SqlParameter _pUserId = new SqlParameter("@intUserId", SqlDbType.Int);
            if (UserId != 0) _pUserId.Value = UserId;
            else _pUserId.Value = DBNull.Value;
            SqlParameter _pTechId = new SqlParameter("@intTechId", SqlDbType.Int);
            if (TechId != 0) _pTechId.Value = TechId;
            else _pTechId.Value = DBNull.Value;
            SqlParameter _pLocationId = new SqlParameter("@LocationId", SqlDbType.Int);
            if (LocationId != 0) _pLocationId.Value = LocationId;
            else _pLocationId.Value = DBNull.Value;
            SqlParameter _pPriorityId = new SqlParameter("@intPriorityId", SqlDbType.Int);
            if (PriorityId != 0) _pPriorityId.Value = PriorityId;
            else _pPriorityId.Value = DBNull.Value;
            SqlParameter _pClassId = new SqlParameter("@intClassId", SqlDbType.Int);
            if (ClassId != 0) _pClassId.Value = ClassId;
            else _pClassId.Value = DBNull.Value;
            SqlParameter _pCategoryId = new SqlParameter("@intCategoryId", SqlDbType.Int);
            if (CategoryId != 0) _pCategoryId.Value = CategoryId;
            else _pCategoryId.Value = DBNull.Value;
            SqlParameter _pLevelId = new SqlParameter("@tintLevel", SqlDbType.Int);
            if (LevelId != 0) _pLevelId.Value = LevelId;
            else _pLevelId.Value = DBNull.Value;
            //Depreciated Parameter
            SqlParameter _pSerialNumber = new SqlParameter("@vchAssetSerial", SqlDbType.VarChar, 50);
            _pSerialNumber.Value = DBNull.Value;
            //----
            SqlParameter _pSubject = new SqlParameter("@vchSubject", SqlDbType.VarChar, 100);
            if (TktSubject.Length > 0) _pSubject.Value = TktSubject;
            else _pSubject.Value = DBNull.Value;
            SqlParameter _pText = new SqlParameter("@vchText", SqlDbType.VarChar, 5000);
            if (TktText.Length > 0)
            {
                if (TktText.Length > 4999) TktText = "--Text truncated at 5000 characters--<br><br>" + TktText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
                _pText.Value = TktText;
            }
            else _pText.Value = "(Initial post was blank.)";
            SqlParameter _pDateNext = new SqlParameter("@dtNext", SqlDbType.SmallDateTime);
            if (DateNext != null && DateNext != DateTime.MinValue) _pDateNext.Value = DateNext;
            else _pDateNext.Value = DBNull.Value;
            SqlParameter _pMethodEnd = new SqlParameter("@vchEndMethod", SqlDbType.VarChar, 5);
            if (MethodEnd.Length > 0) _pMethodEnd.Value = MethodEnd;
            else _pMethodEnd.Value = DBNull.Value;
            SqlParameter _pDateStop = new SqlParameter("@dtStop", SqlDbType.SmallDateTime);
            if (DateStop != null && DateStop != DateTime.MinValue) _pDateStop.Value = DateStop;
            else _pDateStop.Value = DBNull.Value;
            SqlParameter _pUserEmail = new SqlParameter("@vchUserEmail", SqlDbType.VarChar, 50);
            if (UserEmail.Length > 0) _pUserEmail.Value = UserEmail;
            else _pUserEmail.Value = DBNull.Value;
            SqlParameter _pAccountId = new SqlParameter("@intAcctId", SqlDbType.Int);
            if (AccountId > 0) _pAccountId.Value = AccountId;
            else _pAccountId.Value = DBNull.Value;
            SqlParameter _pAccountLocationId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
            if (AccountLocationId != 0) _pAccountLocationId.Value = AccountLocationId;
            else _pAccountLocationId.Value = DBNull.Value;
            SqlParameter _pCustomXML = new SqlParameter("@CustomXML", SqlDbType.Text);
            if (CustomFields.Length > 0) _pCustomXML.Value = CustomFields;
            else _pCustomXML.Value = DBNull.Value;
            SqlParameter _pIdMethod = new SqlParameter("@vchIdMethod", SqlDbType.VarChar);
            if (IdMethod.Length > 0) _pIdMethod.Value = IdMethod.Length > 255 ? IdMethod.Substring(0, 255) : IdMethod;
            else _pIdMethod.Value = DBNull.Value;
            SqlParameter _pProjectID = new SqlParameter("@ProjectID", SqlDbType.Int);
            if (projectID > 0) _pProjectID.Value = projectID;
            else _pProjectID.Value = DBNull.Value;
            SqlParameter _pFolderID = new SqlParameter("@FolderID", SqlDbType.Int);
            if (folderID > 0) _pFolderID.Value = folderID;
            else _pFolderID.Value = DBNull.Value;
            UpdateData("sp_UpdateSchedTkt", new SqlParameter[]{_pRVAL,
                new SqlParameter("@DId", DeptID),
                _pSchedTktId,
                _pUserId,
                new SqlParameter("@intOwnerId", OwnerId),
                _pTechId,
                _pLocationId,
                _pPriorityId,
                _pClassId,
                _pCategoryId,
                _pLevelId,
                _pSerialNumber,
                _pSubject,
                _pText,
                _pDateNext,
                new SqlParameter("@vchRecurringOn", ReccuringOn),
                _pMethodEnd,
                new SqlParameter("@tintEndCount", EndCount),
                _pDateStop,
                new SqlParameter("@tintRecurringFeq", ReccuringFeq),
                new SqlParameter("@btEnabled", Enabled),
                _pUserEmail,
                _pAccountId,
                _pAccountLocationId,
                _pIdMethod,
                _pCustomXML,
                _pProjectID,
                _pFolderID,
                new SqlParameter("@RepeatFromCompletion", repeatFromCompletion)});
            int _SchedTktId = (int)_pRVAL.Value;
            if (_SchedTktId < 0) return _SchedTktId;
            if (SchedTktID != 0)
            {
                Assets.AssetItem[] _assArr = SelectTicketAssets(DeptID, SchedTktID);
                bool _toDelete = true;
                foreach (Assets.AssetItem _ass1 in _assArr)
                {
                    _toDelete = true;
                    foreach (Assets.AssetItem _ass2 in assets)
                    {
                        if (_ass1.ID == _ass2.ID)
                        {
                            _toDelete = false;
                            break;
                        }
                    }
                    if (!_toDelete) continue;
                    DeleteAsset(DeptID, SchedTktID, _ass1.ID);
                }
            }
            foreach (Assets.AssetItem _asset in assets) InsertAsset(DeptID, _SchedTktId, _asset.ID, _asset.Description);
            return _SchedTktId;
        }

        public static DataTable SelectSchedTicketsLost(Guid orgId, int deptID)
        {
            return SelectRecords("sp_SelectSchedTicketsLost",
                new SqlParameter[] { new SqlParameter("@DepartmentId", deptID) }, orgId);
        }

        public static int SelectSchedGeneratedOpenTktsCount(Guid orgId, int departmentId, int schedTicketId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_SelectSchedGeneratedOpenTktsCount", new[] { pReturnValue, 
                new SqlParameter("@DId", departmentId), 
                new SqlParameter("@SchedTicketID", schedTicketId)}, orgId);
            return (int)pReturnValue.Value;
        }

        public static DateTime GetLastClosedDate(Guid orgId, int departmentId, int schedTicketId)
        {
            DataRow row = SelectRecord("sp_SelectSchedGeneratedLastClosedTkt", new SqlParameter[]
                {new SqlParameter("@DId", departmentId), 
                    new SqlParameter("@SchedTicketID", schedTicketId)}, orgId);
            if (row != null)
            {
                if (!row.IsNull("ClosedTime"))
                {
                    return (DateTime)row["ClosedTime"];
                }
            }
            return DateTime.MinValue;
        }
    }
}
