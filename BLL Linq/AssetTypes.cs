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
    public class AssetTypes
    {
        public static string Merge(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int SourceTypeID, int DestTypeID)
        {

            //AssetMakes
            var MergeInfo =
                    from SourceMake in dc.AssetMakes
                    join DestMake in dc.AssetMakes on new
                    {
                        SDID = SourceMake.DepartmentId,
                        DDID = DepartmentId,
                        SourceTypeID = SourceMake.TypeId,
                        DestTypeID,
                        SourceMake.Make
                    } equals new
                    {
                        SDID = DepartmentId,
                        DDID = DestMake.DepartmentId,
                        SourceTypeID,
                        DestTypeID = DestMake.TypeId,
                        DestMake.Make
                    }

                    orderby SourceMake.Make, SourceMake.Id, DestMake.Make, DestMake.Id
                    select new { SourceMake, DestMake };


            foreach (var mi in MergeInfo)
            {
                AssetMakes.Merge(dc, DepartmentId, mi.SourceMake.Id, mi.DestMake.Id);
            }


            //AssetMakes
            var Makes = from m in dc.AssetMakes where m.DepartmentId == DepartmentId && m.TypeId == SourceTypeID select m;
            foreach (var Make in Makes) Make.TypeId = DestTypeID;


            //Assets
            var Assets = from a in dc.Assets where a.DepartmentId == DepartmentId && a.TypeId == SourceTypeID select a;
            foreach (var Asset in Assets) Asset.TypeId = DestTypeID;

            //DupeAssetTypeProperties
            var DupeTypeProperties = from sp in dc.AssetTypeProperties
                                     join dp in dc.AssetTypeProperties on new
                                     {
                                         sDepartmentId = sp.DId,
                                         dDepartmentId = DepartmentId,
                                         SourceTypeID = sp.AssetTypeId,
                                         DestTypeID,
                                         sp.Name
                                     } equals new
                                     {
                                         sDepartmentId = DepartmentId,
                                         dDepartmentId = dp.DId,
                                         SourceTypeID,
                                         DestTypeID=dp.AssetTypeId,
                                         dp.Name
                                     }
                                     select new { sp, dp};
            foreach (var DupeTypeProperty in DupeTypeProperties)
            {
                var TypePropValues = from v in dc.AssetPropertyValues
                                     where v.AssetTypePropertyId == DupeTypeProperty.sp.Id
                                     select v;
                foreach (var TypePropValue in TypePropValues)
                {
                    TypePropValue.AssetTypePropertyId = DupeTypeProperty.dp.Id;
                    dc.AssetTypeProperties.DeleteOnSubmit(DupeTypeProperty.sp);
                }
            }
            dc.SubmitChanges();

            //AssetTypeProperties
            var TypeProperties = from p in dc.AssetTypeProperties where p.DId == DepartmentId && p.AssetTypeId == SourceTypeID select p;
            foreach (var TypeProperty in TypeProperties) TypeProperty.AssetTypeId = DestTypeID;

            //AssetTypeCustCap
            var TypeCustCaps = from c in dc.AssetTypeCustCap where c.TypeId == SourceTypeID select c;
            foreach (var TypeCustCap in TypeCustCaps) TypeCustCap.TypeId = DestTypeID;


            //AssetTypes
            var Types = from t in dc.AssetTypes where t.DepartmentId == DepartmentId && t.Id == SourceTypeID select t;
            dc.AssetTypes.DeleteAllOnSubmit(Types);

            dc.SubmitChanges();
            return null;
        }
        public static string Merge(Guid OrgId, int DepartmentId, int SourceTypeID, int DestTypeID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                return Merge(dc, DepartmentId, SourceTypeID, DestTypeID);
            }
        }

        public static lib.bwa.bigWebDesk.LinqBll.Context.AssetTypeProperties Select(Guid OrgId, int DepartmentId, int TypeID)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
            return (from p in dc.AssetTypeProperties where p.Id==TypeID select p).FirstOrNull();
        }

        public static lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectTypePropertyMergeConflictResult[] GetTypePropertyMergeConflict(Guid OrgId, int DepartmentId, int SourceTypeID, int DestTypeID, bool MergeTypeConflict)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
            var ConflictInfo = dc.Sp_SelectTypePropertyMergeConflict(DepartmentId, SourceTypeID, DestTypeID, MergeTypeConflict);
            lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectTypePropertyMergeConflictResult[] ret = ConflictInfo.ToArray();
            if (ret == null || ret.Length < 1) return null;
            return ret;
        }

        public static void GetNames(Guid OrgId, int DepartmentId, int TypeID, out string TypeName, out string CategoryName)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
            var r = (
                from t in dc.AssetTypes
                join c in dc.AssetCategories on new { t.DepartmentId, t.CategoryId } equals new { c.DepartmentId, CategoryId = c.Id }
                where t.DepartmentId == DepartmentId && t.Id == TypeID
                select new { TypeName = t.Name, CategoryName = c.Name }
                ).FirstOrNull();

            if (r == null)
            {
                TypeName = null;
                CategoryName = null;
            }
            else
            {
                TypeName = r.TypeName;
                CategoryName = r.CategoryName;
            }
        }

        public static string Delete(Guid OrgId, int DepartmentId, int TypeID)
        {
            try
            {
                using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
                {
                    int RetCode;
                    var Makes = from m in dc.AssetMakes where m.TypeId == TypeID && m.DepartmentId == DepartmentId select m;
                    foreach (var Make in Makes)
                    {
                        var Models = from m in dc.AssetModels where m.MakeId == Make.Id && m.DepartmentId == DepartmentId select m;
                        foreach (var m in Models)
                        {
                            RetCode = dc.Sp_DeleteAssetModel(DepartmentId, m.Id);
                            if (RetCode != 1) return "Can not delete '" + m.Model + "' asset model. Assets live under this model.";
                        }
                        RetCode = dc.Sp_DeleteAssetMake(DepartmentId, Make.Id);
                        if (RetCode != 1) return "Can not delete '" + Make.Make + "' asset make. Assets live under this make.";
                    }

                    RetCode = dc.Sp_DeleteAssetType(DepartmentId, TypeID);
                    if (RetCode == 3) return "Can not delete this asset type. Custom Fields Setup that must be removed first.";
                    if (RetCode != 1) return "Can not delete this asset type. Assets live under this type.";

                    dc.SubmitChanges();
                    return null;
                }
            }
            catch (System.Data.SqlClient.SqlException  sqlEx)
            {
                return "Can not delete this. Assets live under it.";
            }
            catch (Exception ex)
            {
                return "Can not delete this. Unexpected error. Contact system admnistrator.";
            }
            return null;
        }

        public static string Copy(Guid OrgId, int DepartmentId, int TypeID, int CategoryID)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Types = from t in dc.AssetTypes where t.Id == TypeID && t.DepartmentId == DepartmentId select t;
                var Type = Types.FirstOrNull();
                if (Type == null) return "Can not find specified asset type.";

                Types = from t in dc.AssetTypes where t.DepartmentId == DepartmentId && t.CategoryId == CategoryID && t.Name == Type.Name select t;
                var ExistType = Types.FirstOrNull();
                if (ExistType != null) return "This asset type exist in destination asset category.";

                lib.bwa.bigWebDesk.LinqBll.Context.AssetTypes NewType = new lib.bwa.bigWebDesk.LinqBll.Context.AssetTypes();
                NewType.CategoryId = CategoryID;
                NewType.DepartmentId = DepartmentId;
                NewType.ConfigCustFields = Type.ConfigCustFields;
                NewType.Name = Type.Name;

                dc.AssetTypes.InsertOnSubmit(NewType);
                dc.SubmitChanges();
                return null;
            }
        }

        public static string Move(Guid OrgId, int DepartmentId, int TypeID, int CategoryID, out bool IsMerge)
        {
            IsMerge = false;
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Types = from t in dc.AssetTypes where t.Id == TypeID && t.DepartmentId == DepartmentId select t;
                var Type = Types.FirstOrNull();
                if (Type == null) return "Can not find spesified asset type.";

                Types = from t in dc.AssetTypes where t.DepartmentId == DepartmentId && t.CategoryId == CategoryID && t.Name == Type.Name select t;
                Type = Types.FirstOrNull();
                if (Type == null)
                {
                    ISingleResult<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectMoveAssetTypeResult> rez = (dc.Sp_SelectMoveAssetType(DepartmentId, 0, TypeID, CategoryID)).ImmediateLoad();
                }
                else
                {
                    string rez = Merge(dc, DepartmentId, TypeID, Type.Id);
                    if (rez != null) return rez;
                    IsMerge = true;
                }
                dc.SubmitChanges();
                return null;
            }
        }

        public static void Rename(Guid OrgId, int DepartmentId, int TypeID, string Name)
        {
            using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.TransactionMode.ImmediateOpenConnection))
            {
                var Types = from c in dc.AssetTypes where c.Id == TypeID && c.DepartmentId == DepartmentId select c;
                var Type = Types.FirstOrNull();
                if (Type == null) return;

                Type.Name = Name;
                dc.SubmitChanges();
            }
        }
    }
}