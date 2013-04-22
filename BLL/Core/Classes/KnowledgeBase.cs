using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Priorities.
    /// </summary>
    public class KnowledgeBase : DBAccess
    {
        /*
        public static DataTable SelectAllArticles(int DepartmentId)
        {
            return SelectRecords("", new SqlParameter[] { new SqlParameter("", ) });
        }
        */

        public static int SelectArticlesCount(Guid OrgId, Guid InstId)
        {
            int DId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM tbl_ticket WHERE tbl_ticket.company_id=" + DId.ToString() + " AND tbl_ticket.KB = 1", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static DataTable BrowseKnowledgeBase(int DepartmentId, bool onlyPublished, int kbCategoryID, int kbType)
        {
            return BrowseKnowledgeBase(Guid.Empty, DepartmentId, onlyPublished, kbCategoryID, kbType);
        }

        public static DataTable BrowseKnowledgeBase(Guid OrgId, int DepartmentId, bool onlyPublished, int kbCategoryID, int kbType)
        {
            return SelectRecords("sp_SelectBrowseKB", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId),
                new SqlParameter("@OnlyPublished", onlyPublished),
                new SqlParameter("@KBCategoryID", kbCategoryID),
                new SqlParameter("@KBType", kbType)
            }, OrgId);
        }

        public static DataTable SelectArticlesFreetext(int DepartmentId, string searchString, int kbCategoryID, int kbType)
        {
            return SelectRecords("sp_SelectFreeTextKnowledgebase", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DepartmentId), 
                new SqlParameter("@text", searchString),
                new SqlParameter("@KBCategoryID", kbCategoryID),
                new SqlParameter("@KBType", kbType)
            });
        }

        public static DataTable SelectAllComments(int DepartmentId, int ArticleId)
        {
            return SelectRecords("sp_SelectKnowledgebaseResponse", new SqlParameter[] { new SqlParameter("@Id", ArticleId), new SqlParameter("@DepartmentId", DepartmentId) });
        }

        public static DataRow SelectOneArticle(Guid OrgId, int DepartmentId, int ArticleId)
        {
            return SelectRecord("sp_SelectKBDetail", new SqlParameter[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@KBId", ArticleId) }, OrgId);
        }

        public static DataRow SelectOneArticle(int DepartmentId, int ArticleId)
        {
            return SelectOneArticle(Guid.Empty, DepartmentId, ArticleId);
        }

        public static DataRow SelectEmail(int DepartmentId, int ArticleId)
        {
            return SelectRecord("sp_SelectEmailForKnowledgebaseResponse", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@KBId", ArticleId) });
        }

        public static void DeleteArticle(int DepartmentId, int ArticleId)
        {
            UpdateData("sp_DeleteKBArticle", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@KBid", ArticleId)});
        }

        public static int InsertOrUpdateArticle(int DepartmentId, int ArticleId, int UserId, string FullName, bool GlobalKB, string Subject, string Symptom, string Cause, string Resolution)        
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateKBArticle", new SqlParameter[] {pReturnValue, 
                new SqlParameter("@Departmentid", DepartmentId),
                new SqlParameter("@KBId", ArticleId), // if 0 then insert
                new SqlParameter("@UserId", UserId),
                new SqlParameter("@FullName", FullName),
                new SqlParameter("@GlobalKB", GlobalKB),
                new SqlParameter("@Subject", Subject),
                new SqlParameter("@Symptom", Symptom),
                new SqlParameter("@Cause", Cause),
                new SqlParameter("@Resolution", Resolution)
            });
            return (int)pReturnValue.Value;
        }

        public static void InsertArticleCommentary(int DepartmentId, int UserId, int ArticleId, string Commentary, string Name)
        {
            UpdateData("sp_InsertKNowledgebaseResponse", new SqlParameter[] { 
                new SqlParameter("@Departmentid", DepartmentId),
                new SqlParameter("@UserId", UserId),
                new SqlParameter("@KBId", ArticleId),
                new SqlParameter("@Comment", Commentary),
                new SqlParameter("@vchFullName", Name)
            });
        }

        public static int SelectKBCountByDate(int DepartmentId, DateTime StartDate, DateTime EndDate)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_SelectKBCountByDate", new SqlParameter[] { pReturnValue, new SqlParameter("@DId", DepartmentId), new SqlParameter("@StartDate", StartDate), new SqlParameter("@EndDate", EndDate) });

            return pReturnValue.Value != DBNull.Value ? (int)pReturnValue.Value : 0;
        }

        public static DataTable SelectKBByDate(int DepartmentId, DateTime StartDate, DateTime EndDate)
        {
            return SelectRecords("sp_SelectKBByDate", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), 
                new SqlParameter("@StartDate", StartDate), 
                new SqlParameter("@EndDate", EndDate) });
        }

        public static DataTable SelectKBCategory(int departmentId)
        {
            return SelectRecords("sp_SelectKBCategoryList", new SqlParameter[] { new SqlParameter("@DepartmentId", departmentId) });
        }

        public static int InsertKBArticle(Guid OrgID, int departmentId, int userId, string subject, int kbPublishLevel,
            int locationId, int classId, DateTime dtCreatedTime, string workpad, int technicianId, int accountId, int accountLocationId,
            string submissionCategoryName, int creationCategoryId, string kbSearchDesc, string kbAlternateId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            SqlParameter _pLocationId = new SqlParameter("@location", SqlDbType.Int);
            if (locationId != 0) _pLocationId.Value = locationId;
            else _pLocationId.Value = DBNull.Value;
            SqlParameter _pClassId = new SqlParameter("@classID", SqlDbType.Int);
            if (classId != 0) _pClassId.Value = classId;
            else _pClassId.Value = DBNull.Value;
            SqlParameter _pTechnicianId = new SqlParameter("@techId", SqlDbType.Int);
            if (technicianId != 0) _pTechnicianId.Value = technicianId;
            else _pTechnicianId.Value = DBNull.Value;
            SqlParameter _pAccountId = new SqlParameter("@intAcctId", SqlDbType.Int);
            if (accountId > 0) _pAccountId.Value = accountId;
            else _pAccountId.Value = DBNull.Value;
            SqlParameter _pbtNoAccount = new SqlParameter("@btNoAccount", SqlDbType.Bit);
            if (accountId == -2) _pbtNoAccount.Value = true;
            else _pbtNoAccount.Value = false;
            SqlParameter _pAccountLocationId = new SqlParameter("@AccountLocationId", SqlDbType.Int);
            if (accountLocationId > 0) _pAccountLocationId.Value = accountLocationId;
            else _pAccountLocationId.Value = DBNull.Value;
            SqlParameter _pSubmissionCategoryName = new SqlParameter("@vchSubmissionCat", SqlDbType.VarChar, 50);
            if (submissionCategoryName.Length > 0) _pSubmissionCategoryName.Value = submissionCategoryName;
            else _pSubmissionCategoryName.Value = DBNull.Value;
            SqlParameter _pCreationCategoryId = new SqlParameter("@intCategoryId", SqlDbType.Int);
            if (creationCategoryId != 0) _pCreationCategoryId.Value = creationCategoryId;
            else _pCreationCategoryId.Value = DBNull.Value;
            UpdateData("sp_InsertKBArticle", new SqlParameter[] {_pRVAL, 
                new SqlParameter("@DId", departmentId),
                new SqlParameter("@PseudoId", Micajah.Common.Bll.Support.GeneratePseudoUnique()),
                new SqlParameter("@user_id", userId),
                new SqlParameter("@subject", subject),
                new SqlParameter("@KBPublishLevel", kbPublishLevel),
                _pLocationId,
                _pClassId,
                new SqlParameter("@dtCreatedTime", dtCreatedTime),
                new SqlParameter("@Workpad", workpad),
                new SqlParameter("@KBSearchDesc", kbSearchDesc),
                new SqlParameter("@KBAlternateId", kbAlternateId),
                _pTechnicianId,
                _pAccountId,
                _pAccountLocationId,
                _pSubmissionCategoryName,
                _pCreationCategoryId,
                _pbtNoAccount
            }, OrgID);
            int _TktId = (int)_pRVAL.Value;
            return _TktId;
        }

        public static DataTable SelectKBPortalSearch(Guid OrgID, int dId, string searchString, int accID, int classID, bool pageHelp)
        {
            SqlParameter _pClassID = new SqlParameter("@ClassID", SqlDbType.Int);
            if (classID >= 0) _pClassID.Value = classID;
            else _pClassID.Value = DBNull.Value;
            return SelectRecords("sp_SelectKBPortslSearch", new SqlParameter[] { 
                new SqlParameter("@DId", dId), 
                new SqlParameter("@text", searchString), 
                new SqlParameter("@AccID", accID), 
                _pClassID, 
                new SqlParameter("@PageHelp", pageHelp)
            }, OrgID);
        }

        public static DataTable SelectKBRelatedArticles(Guid OrgID, int dId, int accID, int tktID)
        {
            return SelectRecords("sp_SelectKBRelatedArticles", new SqlParameter[] { 
                new SqlParameter("@DId", dId), 
                new SqlParameter("@AccID", accID), 
                new SqlParameter("@TktID", tktID)
            }, OrgID);
        }

        public static DataTable SelectKBResponses(Guid OrgID, int dId, int tktID)
        {
            return SelectRecords("sp_SelectKBResponses", new SqlParameter[] { 
                new SqlParameter("@DId", dId), 
                new SqlParameter("@TktID", tktID)
            }, OrgID);
        }

        public static void InsertKBResponse(Guid OrgID, int DeptID, int TktID, string LogText, string to, string userName,
            string userEmail)
        {
            if (LogText.Length > 4999) LogText = "--Text truncated at 5000 characters--<br><br>" + LogText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
            SqlParameter _pNote = new SqlParameter("@vchNote", SqlDbType.VarChar, 5000);
            if (LogText.Length > 0) _pNote.Value = LogText;
            else _pNote.Value = DBNull.Value;
            UpdateData("sp_InsertKBResponse", new SqlParameter[] { 
                new SqlParameter("@DId", DeptID), 
                new SqlParameter("@TId", TktID), 
                _pNote, 
                new SqlParameter("@To", to), 
                new SqlParameter("@UserName", userName), 
                new SqlParameter("@UserEmail", userEmail)
            }, OrgID);
        }

        public static void IncreaseKBHelpfulCount(Guid OrgID, int DeptID, int TktID)
        {
            UpdateData("sp_IncreaseKBHelpfulCount", new SqlParameter[] { 
                new SqlParameter("@DId", DeptID), 
                new SqlParameter("@TId", TktID)
            }, OrgID);
        }

        public static void UpdateKBRateSum(Guid OrgID, int DeptID, int tktLogID, int increaseVal)
        {
            UpdateData("sp_UpdateKBRateSum", new SqlParameter[] { 
                new SqlParameter("@DId", DeptID), 
                new SqlParameter("@TktLogID", tktLogID), 
                new SqlParameter("@IncreaseVal", increaseVal)
            }, OrgID);
        }

        public static DataRow SelectKBBestAnswer(Guid OrgID, int DeptID, int tktID)
        {
            return SelectRecord("sp_SelectKBBestAnswer", new SqlParameter[] { 
                new SqlParameter("@DId", DeptID), 
                new SqlParameter("@TktID", tktID) 
            }, OrgID);
        }

        public static DataTable SelectKBByAlternateId(Guid OrgID, int dId, string alternateId, int kbType)
        {
            return SelectRecords("sp_SelectKBByAlternateId", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", dId), 
                new SqlParameter("@KBAlternateId", alternateId), 
                new SqlParameter("@KBType", kbType)
            }, OrgID);
        }

        public static void InsertKBQuestion(Guid OrgID, int departmentId, int tktID, string kbAlternateId)
        {
            UpdateData("sp_InsertKBQuestion", new SqlParameter[] { 
                new SqlParameter("@DId", departmentId), 
                new SqlParameter("@TktID", tktID), 
                new SqlParameter("@KBAlternateId", kbAlternateId)
            }, OrgID);
        }

        public static DataTable SelectKBAutocomplete(Guid OrgID, int dId, string searchString, int accID)
        {
            return SelectRecords("sp_SelectKBAutocomplete", new SqlParameter[] { 
                new SqlParameter("@DId", dId), 
                new SqlParameter("@text", searchString), 
                new SqlParameter("@AccID", accID)
            }, OrgID);
        }

        public static void CopyFileServiceFiles(int departmentId, string oldLocalObjectId, string newLocalObjectId, string localObjectType)
        {
            UpdateData("sp_FileService_CopyFiles", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", departmentId), 
                new SqlParameter("@OldLocalObjectId", oldLocalObjectId), 
                new SqlParameter("@NewLocalObjectId", newLocalObjectId), 
                new SqlParameter("@LocalObjectType", localObjectType) });
        }

        public static int SelectKBArticleCount(Guid OrgId, int dID)
        {
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM tbl_ticket WHERE tbl_ticket.company_id=" + dID.ToString() + " AND tbl_ticket.KB = 1 AND tbl_ticket.KBPublishLevel = 1 AND tbl_ticket.KBType = 1", OrgId);
            return (int)_dt.Rows[0][0];
        }
    }
}

