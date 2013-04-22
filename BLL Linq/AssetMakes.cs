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
    public class AssetMakes
    {
        public static string Merge(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int SourceMakeID, int DestMakeID)
        {
            var MergeInfo =
                from SourceModel in dc.AssetModels
                join DestModel in dc.AssetModels on new
                {
                    SourceDID = SourceModel.DepartmentId,
                    DestDID = DepartmentId,
                    SourceMakeID = SourceModel.MakeId,
                    DestMakeID,
                    SourceModel.Model
                } equals new
                {
                    SourceDID = DepartmentId,
                    DestDID = DestModel.DepartmentId,
                    SourceMakeID,
                    DestMakeID = DestModel.MakeId,
                    DestModel.Model
                }
                select new { SourceModel, DestModel };


            foreach (var mi in MergeInfo)
            {
                AssetModels.Merge(dc, DepartmentId, mi.SourceModel.Id, mi.DestModel.Id);
            }

            var Models = from m in dc.AssetModels where m.DepartmentId == DepartmentId && m.MakeId == SourceMakeID select m;
            foreach (var Model in Models) Model.MakeId = DestMakeID;

            var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.MakeId == SourceMakeID select a;
            foreach (var a in Assets) a.MakeId = DestMakeID;

            var Makes = from m in dc.AssetMakes where m.DepartmentId == DepartmentId && m.Id == SourceMakeID select m;
            dc.AssetMakes.DeleteAllOnSubmit(Makes);

            dc.SubmitChanges();
            return null;
        }

        public static string Merge(Guid OrgId, int DepartmentId, int SourceMakeID, int DestMakeID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                string ret = Merge(dc, DepartmentId, SourceMakeID, DestMakeID);
                if (ret != null) return ret;
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Delete(Guid OrgId, int DepartmentId, int MakeID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                int RetCode;
                var Models = from m in dc.AssetModels where m.MakeId == MakeID && m.DepartmentId == DepartmentId select m;
                foreach (var m in Models)
                {
                    RetCode = dc.Sp_DeleteAssetModel(DepartmentId, m.Id);
                    if (RetCode != 1) return "Can not delete '" + m.Model + "' asset model. Assets are exist for this model.";
                }

                RetCode = dc.Sp_DeleteAssetMake(DepartmentId, MakeID);
                if (RetCode != 1) return "Can not delete this asset make. Assets are exist for this make.";

                dc.SubmitChanges();
                return null;
            }
        }

        public static string Copy(Guid OrgId, int DepartmentId, int MakeID, int TypeID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Makes = from t in dc.AssetMakes where t.Id == MakeID && t.DepartmentId == DepartmentId select t;
                var Make = Makes.FirstOrNull();
                if (Make == null) return "Can not find specified asset make.";

                string MakeName = Make.Make;

                Makes = from t in dc.AssetMakes where t.DepartmentId == DepartmentId && t.TypeId == TypeID && t.Make == Make.Make select t;
                Make = Makes.FirstOrNull();
                if (Make != null) return "'" + MakeName + "' asset make exists in destination asset category. Copy operation is not allowed.";

                lib.bwa.bigWebDesk.LinqBll.Context.AssetMakes NewMake = new lib.bwa.bigWebDesk.LinqBll.Context.AssetMakes();
                NewMake.TypeId = TypeID;
                NewMake.DepartmentId = DepartmentId;
                NewMake.Make = MakeName;

                dc.AssetMakes.InsertOnSubmit(NewMake);
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Move(Guid OrgId, int DepartmentId, int MakeID, int TypeID, out bool IsMerge)
        {
            IsMerge = false;
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Makes = from m in dc.AssetMakes where m.Id == MakeID && m.DepartmentId == DepartmentId select m;
                var Make = Makes.FirstOrNull();
                if (Make == null) return "Can not find specified asset make.";

                Makes = from t in dc.AssetMakes where t.DepartmentId == DepartmentId && t.TypeId == TypeID && t.Make == Make.Make select t;
                var MergeMake = Makes.FirstOrNull();
                if (MergeMake == null)
                {
                    Make.TypeId = TypeID;
                    var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.MakeId == MakeID select a;
                    foreach (var Asset in Assets)
                    {
                        Asset.TypeId = TypeID;
                    }
                }
                else
                {
                    string ret = Merge(dc, DepartmentId, MakeID, MergeMake.Id);
                    if (ret != null) return ret;
                    IsMerge = true;
                }

                dc.SubmitChanges();
                return null;
            }
        }

        public static void Rename(Guid OrgId, int DepartmentId, int MakeID, string Name)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Makes = from c in dc.AssetMakes where c.Id == MakeID && c.DepartmentId == DepartmentId select c;
                var Make = Makes.FirstOrNull();
                if (Make == null) return;

                Make.Make = Name;
                dc.SubmitChanges();
            }
        }
    }
}