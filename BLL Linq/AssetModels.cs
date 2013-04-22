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
    public class AssetModels
    {
        public static string Merge(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int SourceModelID, int DestModelID)
        {
            var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.ModelId == SourceModelID select a;
            foreach (var Asset in Assets) Asset.ModelId = DestModelID;

            var Models = from m in dc.AssetModels where m.DepartmentId == DepartmentId && m.Id == SourceModelID select m;
            dc.AssetModels.DeleteAllOnSubmit(Models);
            return null;
        }

        public static string Merge(Guid OrgId, int DepartmentId, int SourceModelID, int DestModelID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                Merge(dc, DepartmentId, SourceModelID, DestModelID);
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Delete(Guid OrgId, int DepartmentId, int ModelID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                int RetCode = dc.Sp_DeleteAssetModel(DepartmentId, ModelID);
                if (RetCode != 1) return "Can not delete this asset model. Assets are exist for this model.";
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Copy(Guid OrgId, int DepartmentId, int ModelID, int MakeID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Models = from t in dc.AssetModels where t.Id == ModelID && t.DepartmentId == DepartmentId select t;
                var Model = Models.FirstOrNull();
                if (Model == null) return "Can not find specified asset model.";

                string ModelName = Model.Model;
                string ModelLinks = Model.Links;

                Models = from t in dc.AssetModels where t.DepartmentId == DepartmentId && t.MakeId == MakeID && t.Model == ModelName select t;
                Model = Models.FirstOrNull();
                if (Model != null) return "'" + ModelName + "' asset model already exists in destination asset make. Copy operation is not allowed.";

                lib.bwa.bigWebDesk.LinqBll.Context.AssetModels NewModel = new lib.bwa.bigWebDesk.LinqBll.Context.AssetModels();
                NewModel.MakeId = MakeID;
                NewModel.DepartmentId = DepartmentId;
                NewModel.Model = ModelName;
                NewModel.Links = ModelLinks;

                dc.AssetModels.InsertOnSubmit(NewModel);
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Move(Guid OrgId, int DepartmentId, int ModelID, int MakeID, out bool IsMerge)
        {
            IsMerge = false;
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Models = from t in dc.AssetModels where t.Id == ModelID && t.DepartmentId == DepartmentId select t;
                var Model = Models.FirstOrNull();
                if (Model == null) return "Can not find specified asset model.";

                Models = from m in dc.AssetModels where m.DepartmentId == DepartmentId && m.MakeId == MakeID && m.Model == Model.Model select m;
                var MergeModel = Models.FirstOrNull();
                if (MergeModel == null)
                {
                    Model.MakeId = MakeID;
                    var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.ModelId == ModelID select a;
                    foreach (var Asset in Assets)
                    {
                        Asset.MakeId = MakeID;
                    }
                }
                else
                {
                    string ret = Merge(dc, DepartmentId, ModelID, MergeModel.Id);
                    if (ret != null) return ret;
                    IsMerge = true;
                }
                dc.SubmitChanges();
                return null;
            }
        }

        public static void Rename(Guid OrgId, int DepartmentId, int ModelID, string Name)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Models = from c in dc.AssetModels where c.Id == ModelID && c.DepartmentId == DepartmentId select c;
                var Model = Models.FirstOrNull();
                if (Model == null) return;

                Model.Model = Name;
                dc.SubmitChanges();
            }
        }
    }
}