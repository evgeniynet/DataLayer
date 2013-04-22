using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System;


namespace lib.bwa.bigWebDesk.LinqBll
{
    public class Folders
    {
        public class stFolderInfo
        {
            public int Id {get; set;}
            public int DepartmentId { get; set; }
            public int? ParentId { get; set; }
            public string VchName { get; set; }
            public int TicketOpen { get; set; }
        }

        public static IQueryable<stFolderInfo> List(Guid OrgId, int DepartmentId, out string DepartmentName)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);

            DepartmentName = (from c in dc.Tbl_company where c.Company_id == DepartmentId select c.Company_name).FirstOrNull();

            IQueryable<stFolderInfo> ret = 
                from f in dc.Folders 
                where f.DId == DepartmentId 
                orderby f.VchName
                select new stFolderInfo { 
                    Id = f.Id, 
                    DepartmentId = f.DId, 
                    ParentId = f.ParentId ==null ? 0 : f.ParentId, 
                    VchName = f.VchName,
                    TicketOpen = (dc.Tbl_ticket.Where(t => t.Company_id == DepartmentId && t.Folder_id == f.Id && t.Status != "Closed").Count()),
                };

            return ret;
        }

        public static string Rename(Guid OrgId, int DepartmentId, int FolderId, string NewName)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var folders = from f in dc.Folders where f.DId == DepartmentId && f.Id == FolderId select f;
                var folder = folders.FirstOrNull();
                if (folder == null) return "Can not find specified folder.";

                var PrentChild = (from f in dc.Folders where f.DId == DepartmentId && f.ParentId == folder.ParentId && f.VchName == NewName && f.Id != folder.Id select f).FirstOrNull();
                if (PrentChild == null)
                {
                    folder.VchName = NewName.Length>50 ? NewName.Substring(0, 50) : NewName;
                }
                else
                {
                    string rez = Merge(dc, DepartmentId, FolderId, PrentChild.Id);
                    if (rez != null) return rez;
                }
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Delete(Guid OrgId, int DepartmentId, int FolderId)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var folders = from f in dc.Folders where f.DId == DepartmentId && f.Id == FolderId select f;
                var folder = folders.FirstOrNull();
                if (folder == null) return "Can not find the folder.";

                int SubFolderCount = dc.Folders.Count(f=>f.DId == DepartmentId && f.ParentId == FolderId);
                if (SubFolderCount > 0) return "The folder has " + SubFolderCount + " subfolders. Please delete all subfolders first.";

                int TicketsCount = dc.Tbl_ticket.Count(t=>t.Company_id==DepartmentId && t.Folder_id==FolderId);
                if(TicketsCount>1) return "The folder has "+TicketsCount+" tickets. Please merge this folder with another one or delete all folder tickets first.";

                dc.Folders.DeleteOnSubmit(folder);
                dc.SubmitChanges();
                return null;
            }
        }

        public static int? AddChild(Guid OrgId, int DepartmentId, int FolderId, string ChildName)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                lib.bwa.bigWebDesk.LinqBll.Context.Folders folder;
                if (FolderId != 0)
                {
                    var folders = from f in dc.Folders where f.DId == DepartmentId && f.Id == FolderId select f;
                    folder = folders.FirstOrNull();
                    if (folder == null) return null;
                }

                folder = new lib.bwa.bigWebDesk.LinqBll.Context.Folders();
                folder.DId = DepartmentId;
                folder.ParentId = FolderId;
                folder.VchName = ChildName.Length>50 ? ChildName.Substring(0,50) : ChildName;
                dc.Folders.InsertOnSubmit(folder);

                dc.SubmitChanges();
                return folder.Id;
            }
        }

        public static string Merge(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int SourceFolderId, int DestFolderId)
        {
            var SourceFolders = from f in dc.Folders where f.DId == DepartmentId && f.Id == SourceFolderId select f;
            var Source = SourceFolders.FirstOrNull();
            if (Source == null) return "Can not find the source folder.";

            var DestFolders = from f in dc.Folders where f.DId == DepartmentId && f.Id == DestFolderId select f;
            var Dest = DestFolders.FirstOrNull();
            if (Dest == null) return "Can not find the destination folder.";

            if (Dest.ParentId == SourceFolderId)
            {
                Dest.ParentId = Source.ParentId;
            }

            var SourceSubFolders = from f in dc.Folders where f.DId == DepartmentId && f.ParentId == SourceFolderId select f;
            foreach (var sub in SourceSubFolders) sub.ParentId = DestFolderId;

            var SourceTickets = from t in dc.Tbl_ticket where t.Company_id == DepartmentId && t.Folder_id == SourceFolderId select t;
            foreach (var t in SourceTickets) t.Folder_id = DestFolderId;

            dc.Folders.DeleteOnSubmit(Source);
            dc.SubmitChanges();
            return null;
        }

        public static string Merge(Guid OrgId, int DepartmentId, int SourceFolderId, int DestFolderId)
        {
            if (SourceFolderId == 0) return "Can not merge 'bigWebApps & Support' root folder.";
            if (DestFolderId == 0) return "Can not merge to 'bigWebApps & Support' root folder.";

            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                return Merge(dc, DepartmentId, SourceFolderId, DestFolderId);
            }
        }

        public static string Move(Guid OrgId, int DepartmentId, int SourceFolderId, int DestFolderId)
        {
            if (SourceFolderId == 0) return "Can not move 'bigWebApps & Support' root folder.";

            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var SourceFolders = from f in dc.Folders where f.DId == DepartmentId && f.Id == SourceFolderId select f;
                var Source = SourceFolders.FirstOrNull();
                if (Source == null) return "Can not find the source folder.";

                if (Source.ParentId == DestFolderId) return null;

                var DestFolderChild = (from f in dc.Folders where f.DId == DepartmentId && f.ParentId == DestFolderId && f.VchName == Source.VchName && f.Id != Source.Id select f).FirstOrNull();
                if (DestFolderChild == null)
                {

                    if (DestFolderId != 0)
                    {
                        var DestFolders = from f in dc.Folders where f.DId == DepartmentId && f.Id == DestFolderId select f;
                        var Dest = DestFolders.FirstOrNull();
                        if (Dest == null) return "Can not find the destination folder.";

                        if (Dest.ParentId == SourceFolderId)
                        {
                            Dest.ParentId = Source.ParentId;
                        }
                    }
                    Source.ParentId = DestFolderId;
                }
                else
                {
                    string rez = Merge(dc, DepartmentId, SourceFolderId, DestFolderChild.Id);
                    if (rez != null) return rez;
                }
                dc.SubmitChanges();
                return null;
            }
        }

        public static string AssignTicket(Guid OrgId, int DepartmentId, int FolderId, int TicketId)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Folder = (from f in dc.Folders where f.DId == DepartmentId && f.Id == FolderId select f).FirstOrNull();
                if (Folder == null) return "Can not find the folder. Maybe other user delete this folder. ";

                var Tiket = (from t in dc.Tbl_ticket where t.Company_id == DepartmentId && t.Id == TicketId select t).FirstOrNull();
                if (Tiket == null) return "Can not find the ticket. Maybe other user delete this ticket. ";
                Tiket.Folder_id = FolderId;
                Tiket.UpdatedTime = DateTime.UtcNow;
                dc.SubmitChanges();
                return null;
            }
        }
    }
}