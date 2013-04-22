using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System;
using System.Runtime.Serialization;


namespace lib.bwa.bigWebDesk.LinqBll
{
    public static class Ticket
    {
        public enum enStatus { Open = 1, Closed = 2, OnHold = 3, PartsOnOrder=4 }
        public static readonly Dictionary<enStatus, string> StatusString = GetStatusString();
        private static Dictionary<enStatus, string> GetStatusString()
        {
            Dictionary<enStatus, string> ss = new Dictionary<enStatus, string>();
            ss[enStatus.Open] = "Open";
            ss[enStatus.Closed] = "Closed";
            ss[enStatus.OnHold] = "On Hold";
            ss[enStatus.PartsOnOrder] = "Parts On Order";
            return ss;
        }

        public class Transfer : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public bool? descalate {get; set;}
            [DataMember] public bool? escalate {get; set;}
            [DataMember] public bool? keep_technician_attached {get; set;}
            [DataMember] public bool? escape_level {get; set;}
            [DataMember] public string new_technician_userid {get; set;}
            [DataMember] public string new_class_id {get; set;}
            [DataMember] public bool? keep_priority {get; set;}
            [DataMember] public bool? keep_level {get; set;}
            [DataMember] public string note {get; set;}
        }

        public class Comment : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public string posted_userid {get; set;}
            [DataMember] public string posted_name {get; set;}
            [DataMember] public DateTime? posted_time {get; set;}
            [DataMember] public string type {get; set;}
            [DataMember] public string message { get; set; }
        }

        public class TimeLog : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public string tech_userid {get; set;}
            [DataMember] public string tech_name {get; set;}
            [DataMember] public decimal hours {get; set;}
            [DataMember] public DateTime? start_time {get; set;}
            [DataMember] public DateTime? stop_time {get; set;}
            [DataMember] public string tasktype_id {get; set;}
            [DataMember] public string tasktype_name {get; set;}
            [DataMember] public string note {get; set;}
        }

        public class ExtedndedTicketData : TicketData
        {
            [DataMember] public Comment [] Comments;
            [DataMember] public TimeLog [] TimeLogs;

            public static void Select(ExtedndedTicketData td, Guid OrganizationId, int DepartmentId)
            {
                int TicketId = 0;
                int.TryParse(td.key, out TicketId);
                if (TicketId < 1) return;
                lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);

                td.Comments = (from l in dc.TicketLogs
                               join jj in dc.Tbl_LoginCompanyJunc on new {Company_id=DepartmentId, l.UId} equals new {jj.Company_id, UId=(int?)jj.Id} into ij
                               
                               from j in ij.DefaultIfEmpty()
                               join ju in dc.Tbl_Logins on j.Login_id equals ju.Id into iu

                               from u in iu.DefaultIfEmpty()

                               where l.DId == DepartmentId && l.TId == TicketId && l.TicketTimeId==null
                               orderby l.DtDate descending
                               select new Comment { 
                                   key = l.Id.ToString(),
                                   posted_userid = l.UId.ToString(),
                                   posted_name = (u.FirstName == null ? "" : u.FirstName+" ") + (u.LastName==null?"":u.LastName),
                                   posted_time = l.DtDate,
                                   type = l.VchType,
                                   message = l.VchNote
                               }).ToArray();

                td.TimeLogs = (from l in dc.TicketLogs
                               join t in dc.TicketTime on new { DepartmentId, l.TicketTimeId } equals new { t.DepartmentId, TicketTimeId=(int?)t.Id }
                               join jtt in dc.TaskType on new { DepartmentID=DepartmentId, t.TaskTypeId } equals new { jtt.DepartmentID, TaskTypeId = (int?)jtt.TaskTypeId } into itt
                               join jj in dc.Tbl_LoginCompanyJunc on new { Company_id = DepartmentId, t.UserId } equals new { jj.Company_id, UserId = (int?)jj.Id } into ij

                               from j in ij.DefaultIfEmpty()
                               join ju in dc.Tbl_Logins on j.Login_id equals ju.Id into iu

                               from u in iu.DefaultIfEmpty()

                               from tt in itt.DefaultIfEmpty()

                               where l.DId == DepartmentId && l.TId == TicketId && l.TicketTimeId == null
                               orderby l.DtDate descending
                               select new TimeLog
                               {
                                   key = l.Id.ToString(),
                                   tech_userid = l.UId.ToString(),
                                   tech_name = (u.FirstName == null ? "" : u.FirstName + " ") + (u.LastName == null ? "" : u.LastName),
                                   hours = t.Hours,
                                   start_time = t.StartTime,
                                   stop_time = t.StopTime,
                                   tasktype_id = t.TaskTypeId.ToString(),
                                   tasktype_name = tt.TaskTypeName,
                                   note = t.Note
                               }).ToArray();
            }
        }


        public class TicketData : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}

            [DataMember] public string user_userid {get; set;}
            [DataMember] public string user_name {get; set;}
            [DataMember] public string user_first_name {get; set;}
            [DataMember] public string user_last_name {get; set;}
            [DataMember] public string user_email {get; set;}

            [DataMember] public string tech_userid {get; set;}
            [DataMember] public string tech_name {get; set;}
            [DataMember] public string tech_first_name {get; set;}
            [DataMember] public string tech_last_name {get; set;}
            [DataMember] public string tech_email {get; set;}

            [DataMember] public string created_userid {get; set;}
            [DataMember] public string created_name {get; set;}
            [DataMember] public DateTime? created_time {get; set;}
            [DataMember] public string created_time_str {get; set;}

            [DataMember] public string updated_userid {get; set;}
            [DataMember] public string updated_name {get; set;}
            [DataMember] public DateTime? updated_time {get; set;}

            [DataMember] public string location_name {get; set;}
            [DataMember] public string location_id {get; set;}
            [DataMember] public string class_id {get; set;}
            [DataMember] public string class_name {get; set;}

            [DataMember] public int? status {get; set;}
            [DataMember] public string status_name {get; set;}
            [DataMember] public string subject {get; set;}
            [DataMember] public string note {get; set;}

            [DataMember] public bool? is_new_user_message {get; set;}
            [DataMember] public bool? is_new_tech_message {get; set;}

            [DataMember] public string closed_userid {get; set;}
            [DataMember] public string closed_name {get; set;}
            [DataMember] public DateTime? closed_time {get; set;}
            [DataMember] public string closed_note {get; set;}

            [DataMember] public bool? is_preventive {get; set;}

            [DataMember] public string folder_id {get; set;}
            [DataMember] public string folder_name {get; set;}

            [DataMember] public string room {get; set;}
            [DataMember] public int? number {get; set;}
            [DataMember] public string custom_fields {get; set;}

            [DataMember] public decimal? amount_parts {get; set;}
            [DataMember] public decimal? amount_labor {get; set;}
            [DataMember] public decimal? amount_travel {get; set;}
            [DataMember] public decimal? amount_misc {get; set;}

            [DataMember] public string priority_id {get; set;}
            [DataMember] public string priority_name {get; set;}
            [DataMember] public int? priority_level {get; set;}


            [DataMember] public DateTime? request_completion_time {get; set;}
            [DataMember] public string request_completion_note {get; set;}
            [DataMember] public DateTime? follow_up_time {get; set;}
            [DataMember] public string follow_up_note {get; set;}
            [DataMember] public int? level {get; set;}
            [DataMember] public string level_name {get; set;}
            [DataMember] public bool? is_via_email_parser {get; set;}

            [DataMember] public string account_id {get; set;}
            [DataMember] public string account_name {get; set;}
            [DataMember] public string account_location_id {get; set;}
            [DataMember] public string account_location_name {get; set;}

            [DataMember] public bool? is_resolved {get; set;}
            [DataMember] public string resolution_category_id {get; set;}
            [DataMember] public string resolution_category_name {get; set;}

            [DataMember] public bool? is_confirmed {get; set;}
            [DataMember] public string confirmed_note {get; set;}
            [DataMember] public string confirmed_userid {get; set;}
            [DataMember] public string confirmed_name {get; set;}
            [DataMember] public DateTime? confirmed_time {get; set;}

            [DataMember] public string creation_category_id {get; set;}
            [DataMember] public string creation_category_name {get; set;}
            [DataMember] public string id_method {get; set;}

            [DataMember] public string next_step {get; set;}
            [DataMember] public DateTime? next_step_time {get; set;}
            [DataMember] public decimal? estimated_hours {get; set;}
            [DataMember] public decimal? remaining_hours {get; set;}
            [DataMember] public decimal? total_hours {get; set;}
            [DataMember] public string workpad {get; set;}

            [DataMember] public string project_id {get; set;}
            [DataMember] public string project_name {get; set;}
            [DataMember] public string submission_category_id {get; set;}
            [DataMember] public string submission_category_name {get; set;}
        }

        static string GetID(int? id)
        {
            if (id == null) return null;
            int n = (int)id;
            if (n == 0) return null;
            return n.ToString();
        }

        static private string GetName(string FirstName, string LastName)
        {
            if (FirstName != null) FirstName = FirstName.Trim();
            if (LastName != null) LastName = LastName.Trim();
            string s = " ";
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName)) s = string.Empty;
            return FirstName + s + LastName;
        }

        private static int? GetStatusNumber(string status)
        {
            if (status == null) return null;
            status = status.ToLower().Trim();
            if (status == "") return null;
            foreach (var ss in StatusString) if (ss.Value.ToLower() == status) return (int)ss.Key;
            int n = 0;
            int.TryParse(status, out n);
            if (n > 0) return n;
            return null;
        }

        public static string GetStatusName(int? n)
        {
            if (n == null) return "Open";
            enStatus s = (enStatus)n;
            string r = StatusString[s];
            if (string.IsNullOrEmpty(r)) r = "Open";
            return r;
        }

        public static string GetDateStr(DateTime? dt)
        {
            if (dt == null || dt == DateTime.MinValue) return null;
            return ((DateTime)dt).ToString("MM/dd HH:mm");
        }

        public static string GetExtDateStr(DateTime? dt)
        {
            if (dt == null || dt == DateTime.MinValue) return null;
            DateTime DT = (DateTime)dt;
            DateTime DTNow = DateTime.UtcNow;
            string ret = null;

            if (DT.Date == DTNow.Date)
            {
                ret = DT.ToString("hh:mm")+DT.ToString("tt").ToLower();
            }
            else if(DT.Date == DTNow.Date.AddDays(-1))
            {
                ret = "Yesterday";
            }
            else if ((DTNow.Date - DT.Date).Days < 7 && DTNow.Date > DT.Date && DTNow.Date.DayOfWeek > DT.Date.DayOfWeek)
            {
                ret = DT.ToString("dddd");
            }
            else
            {
                ret = DT.ToString("MM/dd/yyyy");
            }
            return ret;
        }

        public static IEnumerable<TicketDataType> SelectTickets<TicketDataType>(Guid OrganizationId, int DepartmentId, string where) where TicketDataType : TicketData, new()
        {
            //new TicketDataType();
            if (where == string.Empty) where = null;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);

            var ts = dc.Sp_SelectTicketsDynamicWhere(DepartmentId, where).ToList();
            var tt = ts.Select<Context.Sp_SelectTicketsDynamicWhereResult, TicketDataType>(t => new TicketDataType()
            {
                key = GetID(t.Id),

                user_userid = GetID(t.User_id),
                user_name = GetName(t.UserFirstName, t.UserLastName),
                user_first_name = t.UserFirstName,
                user_last_name = t.UserLastName,
                user_email = t.UserEmail,

                tech_userid = GetID(t.Technician_id),
                tech_name = GetName(t.TechnicianFirstName, t.TechnicianLastName),
                tech_first_name = t.TechnicianFirstName,
                tech_last_name = t.TechnicianLastName,
                tech_email = t.TechnicianEmail,

                created_userid = GetID(t.Created_id),
                created_name = GetName(t.CreatedUserFirstName, t.CreatedUserLastName),
                created_time = t.CreateTime,
                created_time_str = GetExtDateStr(t.CreateTime),

                //updated_userid = ,
                //updated_name = ,
                updated_time = t.UpdatedTime,

                location_name = t.LocationFullName,
                location_id = GetID(t.Location_id),
                class_id = GetID(t.Class_id),
                class_name = t.ClassFullName,

                status = GetStatusNumber(t.Status),
                status_name = t.Status,
                subject = t.Subject,
                note = t.Note,

                is_new_user_message = t.NewUserPost,
                is_new_tech_message = t.NewTechPost,

                closed_userid = GetID(t.Closed_id),
                closed_name = GetName(t.ClosedUserFirstName, t.ClosedUserLastName),
                closed_time = t.ClosedTime,
                closed_note = t.ClosureNote,

                is_preventive = t.BtPreventive,

                folder_id = GetID(t.Folder_id),
                folder_name = t.FolderFullName,

                room = t.Room,
                number = t.TicketNumber,
                custom_fields = t.CustomXML,

                amount_parts = t.PartsCost,
                amount_labor = t.LaborCost,
                amount_travel = t.TravelCost,
                amount_misc = t.MiscCost,


                request_completion_time =  t.DtReqComp,
                request_completion_note = t.ReqCompNote,
                follow_up_time = t.DtFollowUp,
                follow_up_note = t.FollowUpNote,
                level = t.TintLevel,
                level_name = t.TicketLevel,
                is_via_email_parser = t.BtViaEmailParser,

                account_id = GetID(t.IntAcctId),
                account_name = t.AccountName,
                account_location_id = GetID(t.AccountLocationId),
                account_location_name = t.AccountLocationFullName,

                is_resolved = t.BtResolved,
                resolution_category_id = GetID(t.ResolutionCatsId),
                resolution_category_name = t.ResolutionCategoryName,

                priority_id = GetID(t.PriorityId),
                priority_name = t.Priority,
                priority_level = t.PriorityLevel,

                is_confirmed = t.BtConfirmed,
                confirmed_note = t.VchConfirmedNote,
                confirmed_userid = GetID(t.IntConfirmedBy),
                confirmed_name = GetName(t.ConfirmedUserFirstName, t.ConfirmedUserLastName),
                confirmed_time = t.DtConfirmed,

                creation_category_id = GetID(t.CreationCatsId),
                creation_category_name = t.CreationCategoryName,
                id_method = t.VchIdMethod,

                next_step = t.NextStep,
                next_step_time = t.NextStepDate,
                estimated_hours = t.EstimatedTime,
                remaining_hours = t.RemainingHours,
                total_hours = t.TotalHours,
                workpad = t.Workpad,

                project_id = GetID(t.ProjectID),
                project_name = t.ProjectName,
                submission_category_id = GetID(t.IntSubmissionCatId),
                submission_category_name = t.SubmissionCategoryName,
            });
            return tt;
        }

        static int? GetNullId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int n;
            if (!int.TryParse(id, out n)) return null;
            if (n < 1) return null;
            return n;
        }

        static byte? GetNullByte(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            byte n;
            if (!byte.TryParse(id, out n)) return null;
            if (n < 1) return null;
            return n;
        }

        static int GetId(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            int n;
            if (!int.TryParse(id, out n)) return 0;
            if (n < 1) return 0;
            return n;
        }

        static public Dictionary<string, string> GetFieldAlias()
        {
            Dictionary<string, string>  a = new Dictionary<string, string>();
            a.Add("key", "cast(t.Id as varchar(MAX))");

            a.Add("user_id", "User_id");
            a.Add("user_name", "(ISNULL(l1.FirstName+' ','')+ISNULL(l1.LastName,''))");

            a.Add("Technician", "(ISNULL(l2.TechnicianFullName+' ','')+ISNULL(l2.TechnicianEmail,''))");
            a.Add("CreatedUser", "(ISNULL(l3.CreatedUserFullName+' ','')+ISNULL(l3.CreatedUserEmail,''))");
            a.Add("ClosedUser", "(ISNULL(l4.ClosedUserFullName+' ','')+ISNULL(l4.ClosedUserEmail,''))");
            a.Add("ConfirmedUser", "(ISNULL(l5.ConfirmedUserFullName+' ','')+ISNULL(l5.ConfirmedUserEmail,''))");
            a.Add("Location", "dbo.fxGetUserLocationName(t.company_id, t.LocationId)");
            a.Add("AccountLocation", "dbo.fxGetUserLocationName(t.company_id, t.AccountLocationId)");
            a.Add("Class", "dbo.fxGetFullClassName(t.company_id, t.class_id)");
            a.Add("Status", "Status");
            a.Add("Subject", "Subject");
            a.Add("CreateTime", "CreateTime");
            a.Add("Note", "Note");
            a.Add("Workpad", "Workpad");
            a.Add("ClosedTime", "ClosedTime");
            a.Add("Folder", "dbo.fxGetUserFolderName(t.company_id, t.folder_id)");
            a.Add("CreationCategory", "cc.vchName");
            a.Add("TicketNumber", "TicketNumber");
            a.Add("CustomXML", "CustomXML");
            a.Add("PartsCost", "PartsCost");
            a.Add("LaborCost", "LaborCost");
            a.Add("TravelCost", "TravelCost");
            a.Add("MiscCost", "MiscCost");
            a.Add("Priority", "(STR(p.tintPriority) + ' - ' + p.Name)");
            a.Add("PriorityLevel", "p.tintPriority");
            a.Add("SLACompleteDateTime", "SLACompleteDateTime");
            a.Add("SLAResponseDateTime", "SLAResponseDateTime");
            a.Add("SLAStartDateTime", "SLAStartDateTime");
            a.Add("RequestCompletionDateTime", "RequestCompletionDateTime");
            a.Add("RequestCompletionNote", "RequestCompletionNote");
            a.Add("FollowUpDateTime", "FollowUpDateTime");
            a.Add("FollowUpNote", "FollowUpNote");
            a.Add("TicketLevel", "(STR(tl.tintLevel) + ' - ' + tl.LevelName)");
            a.Add("IsCreatedViaEmailParser", "IsCreatedViaEmailParser");
            a.Add("AccountName", "a.vchName");
            a.Add("AccountNumber", "a.vchAcctNum");
            a.Add("ClosureNote", "ClosureNote");
            a.Add("IsResolved", "IsResolved");
            a.Add("ResolutionCategoryName", "rc.vchName");
            a.Add("IsConfirmed", "IsConfirmed");
            a.Add("ConfirmedDateTime", "ConfirmedDateTime");
            a.Add("EstimatedTime", "EstimatedTime");
            a.Add("ConfirmedNote", "ConfirmedNote");
            a.Add("IdMethod", "IdMethod");
            a.Add("IsHandledByCallCenter", "IsHandledByCallCenter");
            a.Add("SubmissionCategoryName", "sc.vchName");
            a.Add("EmailCC", "EmailCC");
            a.Add("TicketNumberPrefix", "TicketNumberPrefix");
            a.Add("RemainingHours", "RemainingHours");
            a.Add("TotalHours", "TotalHours");
            a.Add("NextStep", "NextStep");
            a.Add("IsPreventive", "IsPreventive");
            a.Add("Project", "pr.Name");
            return a;
        }
        
        static void FillTicket(TicketData s, Context.Tbl_ticket d)
        {
            if(s.IsAdded("user_userid")) d.User_id = GetId(s.user_userid);
            if(s.IsAdded("tech_userid")) d.Technician_id = GetId(s.tech_userid);
            if(s.IsAdded("created_userid"))  d.Created_id = GetNullId(s.created_userid);
            if(s.IsAdded("created_time") && s.created_time!=null) d.CreateTime = (DateTime)s.created_time;
            if(s.IsAdded("updated_time")) d.UpdatedTime = s.updated_time;
            if(s.IsAdded("location_id")) d.Location_id = GetNullId(s.location_id);
            if(s.IsAdded("class_id")) d.Class_id = GetNullId(s.class_id);

            if(s.IsAdded("status")) d.Status = GetStatusName(s.status);
            if(s.IsAdded("subject")) d.Subject = s.subject;
            if(s.IsAdded("note")) d.Note = s.note;

            if(s.IsAdded("is_new_user_message") && s.is_new_user_message!=null) d.NewUserPost = (bool)s.is_new_user_message;
            if(s.IsAdded("is_new_tech_message") && s.is_new_tech_message!=null) d.NewTechPost = (bool)s.is_new_tech_message;

            if(s.IsAdded("closed_userid")) d.Closed_id = GetId(s.closed_userid);
            if(s.IsAdded("closed_note")) d.ClosureNote =s.closed_note ;

            if(s.IsAdded("is_preventive") && s.is_preventive!=null) d.BtPreventive = (bool)s.is_preventive;

            if(s.IsAdded("folder_id")) d.Folder_id = GetId(s.folder_id);

            if(s.IsAdded("room")) d.Room = s.room;
            if(s.IsAdded("number")&& s.number!=null) d.TicketNumber = (int)s.number;
            if(s.IsAdded("custom_fields")) d.CustomXML = s.custom_fields;

            if(s.IsAdded("amount_parts") && s.amount_parts!=null) d.PartsCost = (decimal)s.amount_parts;
            if(s.IsAdded("amount_labor") && s.amount_labor!=null) d.LaborCost = (decimal)s.amount_labor;
            if(s.IsAdded("amount_travel") && s.amount_travel!=null) d.TravelCost = (decimal)s.amount_travel;
            if(s.IsAdded("amount_misc") && s.amount_misc!=null) d.MiscCost = (decimal)s.amount_misc;


            if(s.IsAdded("request_completion_time")) d.DtReqComp = s.request_completion_time;
            if(s.IsAdded("request_completion_note")) d.ReqCompNote = s.request_completion_note;
            if(s.IsAdded("follow_up_time")) d.DtFollowUp = s.follow_up_time;
            if(s.IsAdded("follow_up_note")) d.FollowUpNote = s.follow_up_note;
            if(s.IsAdded("level")) d.TintLevel = s.level==null?null:(byte?)((byte)s.level);
            if(s.IsAdded("is_via_email_parser") && s.is_via_email_parser!=null) d.BtViaEmailParser = (bool)s.is_via_email_parser;

            if(s.IsAdded("account_id")) d.IntAcctId = GetNullId(s.account_id);
            if(s.IsAdded("account_location_id")) d.AccountLocationId = GetNullId(s.account_location_id);

            if(s.IsAdded("is_resolved")) d.BtResolved = s.is_resolved;
            if(s.IsAdded("resolution_category_id")) d.ResolutionCatsId = GetNullId(s.resolution_category_id);

            if(s.IsAdded("priority_id")) d.PriorityId = GetNullId(s.priority_id);

            if(s.IsAdded("is_confirmed")) d.BtConfirmed = s.is_confirmed;
            if(s.IsAdded("confirmed_note")) d.VchConfirmedNote = s.confirmed_note;
            if(s.IsAdded("confirmed_userid")) d.IntConfirmedBy = GetNullId(s.confirmed_userid);
            if(s.IsAdded("confirmed_time")) d.DtConfirmed = s.confirmed_time;

            if(s.IsAdded("creation_category_id")) d.CreationCatsId =GetNullId(s.creation_category_id);
            if(s.IsAdded("id_method")) d.VchIdMethod = s.id_method;

            if(s.IsAdded("next_step")) d.NextStep = s.next_step;
            if(s.IsAdded("next_step_time")) d.NextStepDate = s.next_step_time;
            if(s.IsAdded("estimated_hours")) d.EstimatedTime = s.estimated_hours;
            if(s.IsAdded("remaining_hours")) d.RemainingHours = s.remaining_hours;
            if(s.IsAdded("total_hours")) d.TotalHours = s.total_hours;
            if(s.IsAdded("workpad")) d.Workpad = s.workpad;

            if(s.IsAdded("project_id")) d.ProjectID = GetNullId(s.project_id);
            if(s.IsAdded("submission_category_id")) d.IntSubmissionCatId = GetNullId(s.submission_category_id);
        }

        static public void Update(Guid OrgId, int DepartmentId, int TicketId, TicketData s)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                var t = dc.Tbl_ticket.Where(d => d.Company_id == DepartmentId && d.Id == TicketId).FirstOrNull();
                if (t == null) return;
                FillTicket(s, t);
                dc.SubmitChanges();

            }
        }

        static int GetUserId(string uid, int DefaultUserId)
        {
            if (string.IsNullOrEmpty(uid)) return DefaultUserId;
            int id = 0;
            int.TryParse(uid, out id);
            if (id < 1) return DefaultUserId;
            return id;
        }

        static int GetIntID(string id)
        {
            if(string.IsNullOrEmpty(id)) return 0;
            int n = 0;
            int.TryParse(id, out n);
            if (n < 0) n = 0;
            return n;
        }

        static string GetEmptyString(string s)
        {
            if (s == null) return string.Empty;
            return s;
        }
        static public string Insert(Guid OrgId, int DepartmentId, TicketData s, int DefaultUserId)
        {
            int initialPostId;
            int TicketId = bigWebApps.bigWebDesk.Data.Tickets.CreateNew(
                OrgId,
                DepartmentId,
                DefaultUserId,
                GetUserId(s.tech_userid, DefaultUserId),
                GetUserId(s.user_userid, DefaultUserId),
                s.created_time == null ? DateTime.UtcNow : (DateTime)s.created_time,
                GetIntID(s.account_id),
                GetIntID(s.account_location_id),
                false,
                GetIntID(s.location_id),
                GetIntID(s.class_id),
                s.level == null ? 0 : (int)s.level,
                GetEmptyString(s.submission_category_name),
                false,
                GetIntID(s.creation_category_id),
                s.is_preventive == true,
                GetIntID(s.priority_id),
                s.request_completion_time == null ? DateTime.MinValue : (DateTime)s.request_completion_time,
                GetEmptyString(s.request_completion_note),
                "",
                null,
                "",
                "",
                GetEmptyString(s.subject),
                GetEmptyString(s.note),
                null,
                GetStatusName(s.status),
                out initialPostId
            );
            return TicketId.ToString();    

            /*using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                Context.Tbl_ticket t = new Context.Tbl_ticket();
                FillTicket(s, t);
                t.Company_id = DepartmentId;

                if (t.User_id < 1) t.User_id = DefaultUserId;
                if (t.Technician_id < 1) t.Technician_id = DefaultUserId;
                if (t.Created_id == null) t.Created_id = DefaultUserId;
                if (string.IsNullOrEmpty(t.Status)) t.Status = "Open";
                t.CreateTime = DateTime.UtcNow;

                dc.Tbl_ticket.InsertOnSubmit(t);
                dc.SubmitChanges();
                return t.Id.ToString();
            }*/
        }

        static public void Delete(Guid OrgId, int DepartmentId, int TicketId)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                dc.Sp_DeleteTicket(DepartmentId, TicketId);
            }
        }

        static int GetTicketId(Context.MutiBaseDataContext dc, int DepartmentId, string TisketID)
        {
            int tid = 0;
            int.TryParse(TisketID, out tid);
            if (tid < 1) return 0;
            var tt = (from t in dc.Tbl_ticket where t.Id == tid && t.Company_id == DepartmentId && t.Status == "Open" select t).FirstOrNull();
            if (tt == null) return 0;
            return tt.Id;
        }

        static int? GetUserID(Context.MutiBaseDataContext dc, int DepartmentId, string UserID)
        {
            int uid = 0;
            int.TryParse(UserID, out uid);
            if (uid < 1) return null;
            var u = (from cl in dc.Tbl_LoginCompanyJunc where cl.Id==uid && cl.Company_id==DepartmentId select cl).FirstOrNull();
            if(u==null) return null;
            return uid;
        }

        static int? GetTaskTypeID(Context.MutiBaseDataContext dc, int DepartmentId, string TaskTypeID)
        {
            int ttid = 0;
            int.TryParse(TaskTypeID, out ttid);
            if (ttid < 1) return null;
            var u = (from tt in dc.TaskType where tt.TaskTypeId==ttid && tt.DepartmentID==DepartmentId select tt).FirstOrNull();
            if(u==null) return null;
            return ttid;
        }

        static public void AddComment(Guid OrgId, int DepartmentId, string TisketID, Comment comment, int DefaultUserId)
        {
            using(Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId))
            {

                int tid = GetTicketId(dc, DepartmentId, TisketID);
                if (tid < 1) return;
                int puid=0;
                int.TryParse(comment.posted_userid,out puid);
                if (puid < 1) puid = DefaultUserId;


                Context.TicketLogs tl = new Context.TicketLogs();
                tl.DId = DepartmentId;
                tl.TId = tid;
                tl.UId = puid;
                tl.DtDate = (comment.posted_time==null)?(DateTime.UtcNow):((DateTime)comment.posted_time);
		        tl.VchType = comment.type;
                tl.VchNote = comment.message;

                dc.TicketLogs.InsertOnSubmit(tl);
                dc.SubmitChanges();
            }
        }

        static public void AddTimeLog(Guid OrgId, int DepartmentId, string TisketID, TimeLog tl)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                int tid = GetTicketId(dc, DepartmentId, TisketID);
                if (tid < 1) return;

                Context.TicketTime tt = new Context.TicketTime();
                tt.DepartmentId = DepartmentId;
                tt.TicketId = tid;
                tt.UserId = GetUserID(dc, DepartmentId, tl.tech_userid);
                tt.Date = DateTime.UtcNow;
                tt.Hours = tl.hours;
                tt.Note = tl.note;
                tt.StartTime = tl.start_time;
                tt.StopTime = tl.stop_time;
                tt.TaskTypeId = GetTaskTypeID(dc, DepartmentId, tl.tasktype_id);

                dc.TicketTime.InsertOnSubmit(tt);
                dc.SubmitChanges();
            }
        }

        public static decimal GetTicketTotalTime(Context.MutiBaseDataContext dc, int DeptID, int TicketId, out decimal RemainHours)
        {
            var ttr = dc.Sp_SelectTicketTimes(DeptID, TicketId);
            RemainHours = 0m;
            var drs = ttr.Where(d=>d.HoursRemaining!=null).OrderBy(d=>d.DtDate).ToList();
            if (drs.Count > 0)
                if (drs[0].HoursRemaining!=null)
                    RemainHours = (decimal)(drs[0].HoursRemaining);
            decimal? TotalTime = drs.Sum(d => d.Hours);
            return TotalTime == null ? 0 : (decimal)TotalTime;
        }

        class CustomField
        {
            public int id;
            public string val;
            public string cap;
        }
        static private List<CustomField> GetCustomFieldCollection(string CustomFieldsXML, Context.MutiBaseDataContext dc, int DepartmentId, int ClassId)
        {
            List<CustomField> d = new List<CustomField>();
            System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
            try { _doc.LoadXml(CustomFieldsXML); }
            catch { return d; }

            var cfs = dc.CustomFields.Where(f => f.DepartmentId == DepartmentId && f.Class_id == ClassId).ToList();

            System.Xml.XmlNodeList _fields = _doc.SelectNodes("/root/field");
            foreach (System.Xml.XmlNode _field in _fields)
            {
                if (_field.Attributes["id"] == null) continue;
                string _id = _field.Attributes["id"].Value;
                if (_id.Length == 0) continue;

                string _value = string.Empty;
                System.Xml.XmlNode _subNode = _field.SelectSingleNode("value");
                if (_subNode != null) _value = _subNode.InnerText;

                int id;
                int.TryParse(_id, out id);
                if (id < 1) continue;
                var c = cfs.Where(f => f.Id == id).FirstOrNull();
                if (c == null) continue;

                d.Add(new CustomField(){id=id,val=_value,cap=c.Caption});
            }
            return d;
        }

        public static string BuildTicketEmailBodyHTML(Guid OrgId, int DepartmentId, int TicketId, int CompanyJuncLoginId)
        {
            bigWebApps.bigWebDesk.Data.Companies.Department dept = new bigWebApps.bigWebDesk.Data.Companies.Department(OrgId, DepartmentId);


            //bigWebApps.bigWebDesk.Data.Ticket t = new bigWebApps.bigWebDesk.Data.Ticket(OrgId, DepartmentId,TicketId);

            Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DepartmentId);
            var tkts = 
                from t in dc.Tbl_ticket
                join juj in dc.Tbl_LoginCompanyJunc on new { User_id = t.User_id, t.Company_id } equals new { User_id = juj.Id, juj.Company_id } into iuj
                join jtj in dc.Tbl_LoginCompanyJunc on new { Tech_Id = t.Technician_id, t.Company_id } equals new { Tech_Id = jtj.Id, jtj.Company_id } into itj
                join jcc in dc.CreationCats on new { t.CreationCatsId, t.Company_id } equals new { CreationCatsId = (int?)jcc.Id, Company_id = jcc.DId } into icc
                join jrc in dc.ResolutionCats on new { t.ResolutionCatsId, t.Company_id } equals new { ResolutionCatsId = (int?)jrc.Id, Company_id = jrc.DId } into irc
                join jp in dc.Project on new { t.ProjectID, t.Company_id } equals new { ProjectID = (int?)jp.ProjectID, Company_id = jp.CompanyID } into ip
                join jpr in dc.Priorities on new { t.PriorityId, t.Company_id } equals new { PriorityId = (int?)jpr.Id, Company_id = jpr.DId } into ipr
                join jl in dc.Rpt_Locations on new { t.LocationId, t.Company_id } equals new { LocationId = (int?)jl.Id, Company_id = jl.DId } into il
                join jlv in dc.TktLevels on new { t.TintLevel, t.Company_id } equals new { TintLevel = (byte?)jlv.TintLevel, Company_id = jlv.DId } into ilv
                join jc in dc.Rpt_Classes on new { t.Class_id, t.Company_id } equals new { Class_id = (int?)jc.Id, Company_id=jc.DId } into ic
                join ja in dc.Accounts on new { t.IntAcctId, t.Company_id } equals new { IntAcctId = (int?)ja.Id, Company_id = ja.DId } into ia

                from uj in iuj.DefaultIfEmpty()
                join jul in dc.Tbl_Logins on uj.Login_id equals jul.Id into iul

                from ul in iul.DefaultIfEmpty()

                from tj in itj.DefaultIfEmpty()
                join jtl in dc.Tbl_Logins on tj.Login_id equals jtl.Id into itl

                from tl in itl.DefaultIfEmpty()
                from cc in icc.DefaultIfEmpty()
                from rc in irc.DefaultIfEmpty()
                from p in ip.DefaultIfEmpty()
                from pr in ipr.DefaultIfEmpty()
                from l in il.DefaultIfEmpty()
                from lv in ilv.DefaultIfEmpty()
                from c in ic.DefaultIfEmpty()

                from a in ia.DefaultIfEmpty()
                join jal in dc.Rpt_Locations on new { a.LocationId, a.DId } equals new { LocationId = (int?)jal.Id, jal.DId } into ial

                from al in ial.DefaultIfEmpty()

                where t.Company_id == DepartmentId && t.Id == TicketId
                select new
                {
                    tkt = t,
                    user = ul,
                    tch = tl,
                    CreationCatName = cc.VchName,
                    ResolutionCatName = rc.VchName,
                    RelatedTicketsCount = (from rt in dc.RelatedTickets where rt.DId == DepartmentId && rt.TicketId == t.Id select rt.Id).Count(),
                    ProjectName = p.Name,
                    PriorityName = pr.Name,
                    PriorityLevel = pr.TintPriority,
                    LocationName = l.LocationFullName,
                    TktLevelName = lv.LevelName,
                    ClassName = c.ClassFullName,
                    AccountName = a.VchName,
                    AccountLocationName = al.LocationFullName

                };

            var tkt = tkts.FirstOrNull();
                //dc.Tbl_ticket.Where(td => td.Company_id == DepartmentId && td.Id == TicketId).FirstOrNull();
            var usr = dc.Tbl_Logins.Where(ud => ud.Id == CompanyJuncLoginId).FirstOrNull();

            if (dept == null || tkt == null || usr == null) return "Unexpexted Server Error!";
            string _CrLf = new string(new char[] { (char)13, (char)10 });

            string _body = "<html><body><table cellpadding=\"4\" cellspacing=\"0\">";



            _body += "<tr><td colspan='2' style=\"font-size:x-large;padding:10px;font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">#";
            _body += tkt.tkt.TicketNumberPrefix + tkt.tkt.TicketNumber + " " + tkt.tkt.Subject + "</td></tr>";




            //_body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">" + dept.CustomNames.Ticket.FullSingular + " #</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\"><b>";
            //_body += tkt.tkt.TicketNumberPrefix + tkt.tkt.TicketNumber;
            //_body += "&nbsp;</b></td></tr>";
            //_body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Subject</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.tkt.Subject + "&nbsp;</td></tr>";
            if (!string.IsNullOrEmpty(tkt.tkt.NextStep))
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Next Step</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.tkt.NextStep + "&nbsp;</td></tr>";
            _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Department</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + dept.Name + "&nbsp;</td></tr>";
            //if (dept.Config.AccountManager)
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">" + dept.CustomNames.Account.FullSingular + "/" + dept.CustomNames.Location.FullSingular + "</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.AccountName + (!string.IsNullOrEmpty(tkt.AccountLocationName) ? " / " + tkt.AccountLocationName : string.Empty) + "&nbsp;</td></tr>";
            _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">User</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">";
            _body += tkt.user.FirstName + " " + tkt.user.LastName;
            if (!string.IsNullOrEmpty(tkt.user.Title)) _body += "<br />" + tkt.user.Title;
            _body += "<br />" + tkt.user.Email;
            if (!string.IsNullOrEmpty(tkt.user.Phone)) _body += "<br />Phone:" + tkt.user.Phone;
            if (!string.IsNullOrEmpty(tkt.user.MobilePhone)) _body += "<br />Mobile:" + tkt.user.MobilePhone;
            _body += "</td></tr>";
            _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">" + dept.CustomNames.Technician.FullSingular + "</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.tch.FirstName + " " + tkt.tch.LastName + "&nbsp;</td></tr>";
            //if (dept.Config.TktLevels)
            //{
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Level</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.tkt.TintLevel + (!string.IsNullOrEmpty(tkt.TktLevelName) ? " - " + tkt.TktLevelName : string.Empty) + "&nbsp;</td></tr>";
            //}
            //if (dept.Config.PrioritiesGeneral)
            //{
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Priority</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.PriorityLevel + (!string.IsNullOrEmpty(tkt.PriorityName) ? " - " + tkt.PriorityName : string.Empty) + "&nbsp;</td></tr>";
                if (tkt.tkt.DtSLAResponse != null && !tkt.tkt.BtInitResponse)
                {
                    _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Expect Response By</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + bigWebApps.bigWebDesk.Functions.DisplayDateTime((DateTime)tkt.tkt.DtSLAResponse, false) + "&nbsp;</td></tr>";
                }
                if (tkt.tkt.DtSLAComplete != null)
                {
                    _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Expect Completion By</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + bigWebApps.bigWebDesk.Functions.DisplayDateTime((DateTime)tkt.tkt.DtSLAComplete, false) + "&nbsp;</td></tr>";
                }
            //}
            if (/*dept.Config.RequestCompletionDate &&*/ tkt.tkt.DtReqComp !=null && (DateTime)tkt.tkt.DtReqComp > DateTime.MinValue)
            {
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Scheduled By</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + bigWebApps.bigWebDesk.Functions.DisplayDateTime((DateTime)tkt.tkt.DtReqComp, false) + (!string.IsNullOrEmpty(tkt.tkt.ReqCompNote) ? "<br />" + tkt.tkt.ReqCompNote : string.Empty) + "&nbsp;</td></tr>";
            }
            if (tkt.tkt.DtFollowUp != null)
            {
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Follow-Up Date</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + bigWebApps.bigWebDesk.Functions.DisplayDateTime((DateTime)tkt.tkt.DtFollowUp, false) + (!string.IsNullOrEmpty(tkt.tkt.FollowUpNote)? "<br />" + tkt.tkt.FollowUpNote : string.Empty) + "&nbsp;</td></tr>";
            }
            //if (dept.Config.LocationTracking)
            //{
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">" + dept.CustomNames.Location.FullSingular + "</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.LocationName + "&nbsp;</td></tr>";
            //}
            //if (dept.Config.ClassTracking)
            //{
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Class</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.ClassName + "&nbsp;</td></tr>";
            //}
            //if (dept.Config.EnableTicketToProjectRelation)
            //{
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Project</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.ProjectName + "&nbsp;</td></tr>";
            //}
            //if (dept.Config.TimeTracking)
            //{
                decimal RemainHours = 0m;
                var ttr = dc.Sp_SelectTicketTimes(DepartmentId, TicketId);
                decimal TotalTicketTime = GetTicketTotalTime(dc, DepartmentId, TicketId, out RemainHours);
                int PercentageComplete = 100;
                string sPercentage = String.Empty;
                if (TotalTicketTime + RemainHours != 0)
                {
                    PercentageComplete = (int)Math.Round(100 * TotalTicketTime / (TotalTicketTime + RemainHours));
                    if ((TotalTicketTime != 0) || (RemainHours != 0)) sPercentage = "&nbsp;-&nbsp;" + PercentageComplete.ToString() + "%&nbsp;complete";
                }
                string sTotalTicketTime = TotalTicketTime == 0 ? TotalTicketTime.ToString("0") : TotalTicketTime.ToString("0.00").TrimEnd('0').TrimEnd('.');
                decimal intRemainHours = decimal.Truncate(RemainHours);
                string sRemainHours = intRemainHours == RemainHours ? RemainHours.ToString("0") : RemainHours.ToString("0.00").TrimEnd('0').TrimEnd('.');

                var tes = dc.Sp_SelectTicketEstimatedTime(DepartmentId, TicketId).FirstOrNull();
                decimal TktBudget = 0;
                if (tes != null && tes.EstimatedTime != null) TktBudget = (decimal)tes.EstimatedTime;
                string sEstimatedTime = String.Empty;
                decimal EstTime = TotalTicketTime + RemainHours;
                if ((TotalTicketTime == 0) && (RemainHours == 0))
                {
                    if (TktBudget < 0)
                        sEstimatedTime = "No budget";
                    else
                        sEstimatedTime = "Budget:&nbsp;" + (TktBudget == 0 ? TktBudget.ToString("0") : TktBudget.ToString("0.00").TrimEnd('0').TrimEnd('.'));
                }
                else if (PercentageComplete < 100 && TktBudget >= 0)
                {
                    decimal dr = EstTime - TktBudget;
                    string OverBudget = String.Empty;
                    if (dr > 0)
                        OverBudget = "<span style=\"color:Maroon;\">" + dr.ToString("0.00").TrimEnd('0').TrimEnd('.') + "&nbsp;Overbudget</span>";
                    else
                        if (dr == 0)
                            OverBudget = "0&nbsp;Overbudget";
                        else
                            OverBudget = (decimal.Negate(dr)).ToString("0.00").TrimEnd('0').TrimEnd('.') + "&nbsp;under&nbsp;budget";
                    sEstimatedTime = (EstTime == 0 ? EstTime.ToString("0") : EstTime.ToString("0.00").TrimEnd('0').TrimEnd('.'))
                        + "&nbsp;hours&nbsp;estimated&nbsp;|&nbsp;Budget:"
                        + (TktBudget == 0 ? TktBudget.ToString("0") : TktBudget.ToString("0.00").TrimEnd('0').TrimEnd('.'))
                        + "&nbsp;|&nbsp;" + OverBudget;
                }
                else
                    sEstimatedTime = (EstTime == 0 ? EstTime.ToString("0") : EstTime.ToString("0.00").TrimEnd('0').TrimEnd('.')) + "&nbsp;hours"
                        + (PercentageComplete != 100 ? "&nbsp;estimated" : String.Empty);
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Logged Time</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + sTotalTicketTime + "&nbsp;hours&nbsp;</td></tr>";
                string tdRemainInnerHTML = sRemainHours + "&nbsp;hours" + sPercentage;
                if (tkt.tkt.Status == StatusString[enStatus.Closed])
                {
                    tdRemainInnerHTML = "100% complete";
                }
                _body +=
                    "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Remaining Time</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" +
                    tdRemainInnerHTML + "&nbsp;</td></tr>";

                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Total Time</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" +
                    sEstimatedTime + "&nbsp;</td></tr>";

            //}
            if (/*dept.Config.RelatedTickets &&*/ tkt.RelatedTicketsCount > 0)
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Related " + dept.CustomNames.Ticket.AbbreviatedPlural + " Count</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + tkt.RelatedTicketsCount.ToString() + "</td></tr>";

            if (/*dept.Config.CustomFields &&*/ tkt.tkt.Class_id!=null)
            {
                //var _ctcf = dept.CustomEmails.CheckedTicketCustomFields;
                var cf = GetCustomFieldCollection(tkt.tkt.CustomXML, dc, DepartmentId, (int)tkt.tkt.Class_id);
                foreach (var _tcf in cf)
                {
                    //if (_ctcf.ContainsKey(_tcf.ID) && (_ctcf[_tcf.ID].IsForTech == IsForTech || !_ctcf[_tcf.ID].IsForTech) && _tcf.Value.Length > 0)
                    //{
                    _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">" + _tcf.cap + "</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + _tcf.val + "&nbsp;</td></tr>";
                    //}
                }
            }
            if (/*dept.Config.ResolutionTracking &&*/ tkt.tkt.Status == StatusString[enStatus.Closed])
            {
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Resolution</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">" + (tkt.tkt.BtResolved==true ? "Resolved" : "UnResolved") + (!string.IsNullOrEmpty(tkt.ResolutionCatName) ? " - " + tkt.ResolutionCatName : string.Empty) + "&nbsp;</td></tr>";
            }
            if (/*dept.Config.ConfirmationTracking &&*/ tkt.tkt.Status == StatusString[enStatus.Closed] && tkt.tkt.BtResolved == true && tkt.tkt.BtConfirmed == true)
            {
                _body += "<tr><td style=\"background-color: #aaaaaa; text-align: right; font-size: 10pt; color: White; border-bottom: solid 1px #555555;\">Confirmation</td><td style=\"font-family:Arial; border-bottom: solid 1px #555555; text-align: left;\">Confirmed" + (!string.IsNullOrEmpty(tkt.tkt.VchConfirmedNote) ? " - " + tkt.tkt.VchConfirmedNote : string.Empty) + "&nbsp;</td></tr>";
            }
            _body += "</table>";
            _body += "<br /><table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\">";


            var TicketLogs = from tl in dc.TicketLogs
                             join jlc in dc.Tbl_LoginCompanyJunc on tl.UId equals (int?)jlc.Id into ilc

                             from lc in ilc.DefaultIfEmpty()
                             join jl in dc.Tbl_Logins on lc.Login_id equals jl.Id into il

                             from l in il.DefaultIfEmpty()

                             where tl.DId == DepartmentId && tl.TId == TicketId
                             orderby tl.DtDate descending

                             select new { tl.DId, tl.TId, tl.Id, tl.UId, tl.DtDate, tl.VchType, tl.VchNote, tl.TicketTimeId, tl.To, l.FirstName, l.LastName, l.Email};

            foreach (var _log in TicketLogs)
            {
                _body += "<tr bgcolor=\"#3d3d8d\"><td colspan=\"2\" align=\"center\"><font size=\"2\" color=\"#ffffff\"><b>" + _log.VchType + "</b></font></td></tr>";
                _body += "<tr bgcolor=\"#cccccc\"><td>" + _log.LastName + ", " + _log.FirstName + "</td><td align=\"right\">" + bigWebApps.bigWebDesk.Functions.DisplayDateTime(_log.DtDate, false) + "</td></tr>";
                _body += "<tr><td colspan=\"2\">" + System.Text.RegularExpressions.Regex.Replace(_log.VchNote.Replace(_CrLf, "<br />"), @"((ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\=\?\,\'\/\\\+&amp;%\$#_]*)?([a-zA-Z0-9]))", "<a href=\"$1\">$1</a>") + "<br /><br /></td></tr>";
            }
            _body += "</table></body></html>";
            return _body;
        }

        static public void UpdateTicketTechnican(Guid OrgId, int DepartmentId, int TicketID, int NewTechinicanId, bool keepTechnicianAssigned)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                dc.Sp_UpdateTicketTechnician(DepartmentId, TicketID, NewTechinicanId, keepTechnicianAssigned);
            }
        }

        public static void UpdateStatus(Guid OrgId, int DepartmentId, int TicketId, int? ts)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DepartmentId))
            {
                int intTs = 0;
                switch (ts)
                {
                    case (int)enStatus.Open: intTs = 3; break;
                    case (int)enStatus.OnHold: intTs = 2; break;
                    default: return;
                }
                dc.Sp_UpdateTicketStatus(DepartmentId, TicketId, intTs);
            }
        }

        public static void InsertLogMessage(Guid OrgId,int DeptID, int TktID, int UserId, string LogType, string LogText, string systemGeneratedText, int? TimeLogID)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                if (LogText!=null && LogText.Length > 4999) LogText = "--Text truncated at 5000 characters--<br><br>" + LogText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
                dc.Sp_InsertTktLog(DeptID, TktID, UserId, LogType, LogText, systemGeneratedText, TimeLogID, null);
            }
        }

        /*public static int UpdateEscalateByLevel(Guid OrgId, int DeptID, int TicketId, int UserId, bool UpDirection, string NoteText, bool keepTechnicianAssigned)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                if (NoteText!=null && NoteText.Length > 4999) NoteText = "--Text truncated at 5000 characters--<br><br>" + NoteText.Substring(0, 4905) + "<br><br>--Text truncated at 5000 characters--";
                if(NoteText==null) NoteText=null;

                string NoteType=null;
                int? RoutingErrCode = 0;

                int r = dc.Sp_EscalateTicket(DeptID,TicketId,UserId,UpDirection?"up":"down", ref NoteText, ref NoteType, ref RoutingErrCode,keepTechnicianAssigned);
                return r;
            }
        }*/

        public static void UpdateLevel(Guid OrgId, int DeptID, int TicketId, byte NewLevel)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                var tkt = dc.Tbl_ticket.Where(t => t.Company_id == DeptID && t.Id == TicketId).FirstOrNull();
                if (tkt == null) return;
                tkt.TintLevel = NewLevel;
                dc.SubmitChanges();
            }
        }

        public static void UpdateNewPostIcon(Guid OrgId, int DeptID, int TicketId, int UserId, bool PostOnOff)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                dc.Sp_UpdateNewPostIcon(DeptID, TicketId, PostOnOff, UserId);
            }
        }

        public static void UpdateTransferByClass(Guid OrgId, int DeptID, int TicketId, int ClassId, bool KeepPriority, bool KeepLevel, bool keepTechnicianAssigned)
        {
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                string OldTechName=null;
                string NewTechName=null;
                dc.Sp_UpdateTktTransferByClass(DeptID, TicketId, ClassId, ref OldTechName, ref NewTechName, KeepPriority, KeepLevel, keepTechnicianAssigned);
            }
        }

        private static string GetFullName(string f, string l)
        {
            string n = f;
            if (!string.IsNullOrEmpty(l) && !string.IsNullOrEmpty(f)) n += " ";
            n += l;
            return n;
        }

        public static Context.Tbl_ticket Get(Guid OrgId, int DeptID, int TicketId, out string TechName)
        {
            TechName = null;
            using (Context.MutiBaseDataContext dc = new Context.MutiBaseDataContext(OrgId, DeptID))
            {
                var d = (   from t in dc.Tbl_ticket
                            join j in dc.Tbl_LoginCompanyJunc on new {t.Company_id, t.Technician_id} equals new {j.Company_id, Technician_id=j.Id}
                            join l in dc.Tbl_Logins on j.Login_id equals l.Id
                            where t.Company_id == DeptID && t.Id == TicketId
                            select new { t, TechName = GetFullName(l.FirstName, l.LastName) }
                        ).FirstOrNull();
                if (d == null) return null;
                TechName = d.TechName;
                return d.t;
            }
        }

        public static void SelectSummary(Guid OrgId, Guid InstId, int DepartmentId, int CompanyJuncLoginId, out Dictionary<string, int> UnassignedQueues, out int? UnconfirmedUserTktsCount, out int? NewMessagesCount, out int? OpenCount, out int? AsUserCount, out int? OnHoldCount, out int? OnPartsCount, out int? FollowUpCount)
        {
            UnassignedQueues = null;
            UnconfirmedUserTktsCount = null;
            NewMessagesCount = null;
            AsUserCount = null;
            OnHoldCount = null;
            OnPartsCount = null;
            FollowUpCount = null;

            bigWebApps.bigWebDesk.Config conf = new bigWebApps.bigWebDesk.Config(OrgId, InstId, DepartmentId);
            //if (conf.UnassignedQue)
            //{
                DataTable queuesCount = bigWebApps.bigWebDesk.Data.Tickets.SelectUnassignedQueuesCount(DepartmentId);
                if (queuesCount != null && queuesCount.Rows.Count > 0)
                {
                    UnassignedQueues = new Dictionary<string, int>();
                    foreach (DataRow r in queuesCount.Rows) UnassignedQueues.Add(r["QueName"].ToString(), Convert.ToInt32(r["TicketCount"]));
                }
            //}

            DataRow _row = bigWebApps.bigWebDesk.Data.Tickets.SelectTicketCounts(DepartmentId, 1, CompanyJuncLoginId);
            //if (conf.ResolutionTracking && conf.ConfirmationTracking)
            //{
                int unconfirmedUserTicketsCount = _row.IsNull("UnconfirmedUserTickets") ? 0 : (int)_row["UnconfirmedUserTickets"];
                if (unconfirmedUserTicketsCount > 0)
                {
                    UnconfirmedUserTktsCount = unconfirmedUserTicketsCount;
                }
            //}

            NewMessagesCount = Convert.ToInt32(_row["NewMessagesCount"]);
            OpenCount = Convert.ToInt32(_row["OpenTickets"]);
            AsUserCount = Convert.ToInt32(_row["userTickets"]);
            //if (conf.OnHoldStatus)
            //{
                OnHoldCount = Convert.ToInt32(_row["OnHoldTickets"]);
            //}
            //if (conf.PartsTracking)
            //{
                OnPartsCount = Convert.ToInt32(_row["PartsOnOrderTickets"]);
            //}
            FollowUpCount = Convert.ToInt32(_row["reminderTicket"]);
        }
    }
}
