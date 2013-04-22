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
    public class AssetCategories
    {
        public struct CategoryTypeMakeModel
        {
            public int? CategoryID;
            public string CategoryName;
            public int? TypeID;
            public string TypeName;
            public bool? TypeConfigCustFields;
            public int? MakeID;
            public string MakeName;
            public int? ModelID;
            public string ModelName;
            public string ModelLinks;
        }
        public static IQueryable<CategoryTypeMakeModel> GetCategoryTypeMakeModel(Guid OrgId, int DepartmentId)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
               
            IQueryable<CategoryTypeMakeModel> ret =

                from category in dc.AssetCategories
                join jType in dc.AssetTypes on new { DepartmentId, CategoryId = category.Id } equals new { jType.DepartmentId, jType.CategoryId } into iType

                from type in iType.DefaultIfEmpty()
                join jMake in dc.AssetMakes on new { DepartmentId = (int)DepartmentId, TypeId = (int)type.Id } equals new { DepartmentId = (int)jMake.DepartmentId, TypeId = (int)jMake.TypeId } into iMake

                from make in iMake.DefaultIfEmpty()
                join jModel in dc.AssetModels on new { DepartmentId, MakeId = make.Id } equals new { DepartmentId = jModel.DepartmentId, jModel.MakeId } into iModel

                from model in iModel.DefaultIfEmpty()

                where category.DepartmentId == DepartmentId

                orderby category.Name, category.Id, type.Name, type.Id, make.Make, make.Id, model.Model, model.Id

                select new CategoryTypeMakeModel
                {
                    CategoryID = category.Id,
                    CategoryName = category.Name,
                    TypeID = type.Id,
                    TypeName = type.Name,
                    TypeConfigCustFields = type.ConfigCustFields,
                    MakeID = make.Id,
                    MakeName = make.Make,
                    ModelID = model.Id,
                    ModelName = model.Model,
                    ModelLinks = model.Links
                };

            return ret;            
        }

        public static string Merge(Guid OrgId, int DepartmentId, int SourceCategoryID, int DestCategoryID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var MergeInfo =
                        from SourceType in dc.AssetTypes
                        join DestType in dc.AssetTypes on new
                        {
                            SDID = SourceType.DepartmentId,
                            DDID = DepartmentId,
                            SourceCategoryID = SourceType.CategoryId,
                            DestCategoryID,
                            SourceType.Name
                        } equals new
                        {
                            SDID = DepartmentId,
                            DDID = DestType.DepartmentId,
                            SourceCategoryID,
                            DestCategoryID = DestType.CategoryId,
                            DestType.Name
                        }

                        orderby SourceType.Name, SourceType.Id, DestType.Name, DestType.Id
                        select new { SourceType, DestType };


                foreach (var mi in MergeInfo)
                {
                    AssetTypes.Merge(dc, DepartmentId, mi.SourceType.Id, mi.DestType.Id);
                }

                var Types = from t in dc.AssetTypes where t.DepartmentId == DepartmentId && t.CategoryId == SourceCategoryID select t;
                foreach (var type in Types) type.CategoryId = DestCategoryID;

                var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.CategoryId == SourceCategoryID select a;
                foreach (var Asset in Assets) Asset.CategoryId = DestCategoryID;

                var Categories = from c in dc.AssetCategories where c.DepartmentId == DepartmentId && c.Id == SourceCategoryID select c;
                dc.AssetCategories.DeleteAllOnSubmit(Categories);

                dc.SubmitChanges();
                return null;
            }
        }

        public static string GetName(Guid OrgId, int DepartmentId, int CategoryID)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
            string Name = (from c in dc.AssetCategories where c.DepartmentId == DepartmentId && c.Id == CategoryID select c.Name).FirstOrNull();
            return Name;
        }

        public static string Delete(Guid OrgId, int DepartmentId, int CategoryID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                int RetCode;

                var Types = from t in dc.AssetTypes where t.CategoryId == CategoryID && t.DepartmentId == DepartmentId select t;
                foreach (var Type in Types)
                {
                    var Makes = from m in dc.AssetMakes where m.TypeId == Type.Id && m.DepartmentId == DepartmentId select m;
                    foreach (var Make in Makes)
                    {
                        var Models = from m in dc.AssetModels where m.MakeId == Make.Id && m.DepartmentId == DepartmentId select m;
                        foreach (var m in Models)
                        {
                            RetCode = dc.Sp_DeleteAssetModel(DepartmentId, m.Id);
                            if (RetCode != 1) return "Can not delete '" + m.Model + "' model. Assets live under this model.";
                        }
                        RetCode = dc.Sp_DeleteAssetMake(DepartmentId, Make.Id);
                        if (RetCode != 1) return "Can not delete '" + Make.Make + "' make. Assets live under this make.";
                    }
                    RetCode = dc.Sp_DeleteAssetType(DepartmentId, Type.Id);
                    if (RetCode == 3) return "Can not delete '" + Type.Name + "' type. Custom Fields Setup that must be removed first.";
                    if (RetCode != 1) return "Can not delete '" + Type.Name + "' type. Assets live under this type.";
                }

                RetCode = dc.Sp_DeleteAssetCategory(DepartmentId, CategoryID);
                if (RetCode != 1)
                {
                    string CatName = (from c in dc.AssetCategories where c.DepartmentId==DepartmentId && c.Id==CategoryID select c.Name).FirstOrNull();
                    if(!string.IsNullOrEmpty(CatName)) CatName="'"+CatName+"'";
                    else CatName="the";
                    return "Can not delete "+CatName+" category. Assets live under this category.";
                }

                dc.SubmitChanges();
                return null;
            }
         }

        public static void Rename(Guid OrgId, int DepartmentId, int CategoryID, string Name)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Categories = from c in dc.AssetCategories where c.Id == CategoryID && c.DepartmentId == DepartmentId select c;
                var Category = Categories.FirstOrNull();
                if (Category == null) return;

                Category.Name = Name;
                dc.SubmitChanges();
            }
        }
    }
}