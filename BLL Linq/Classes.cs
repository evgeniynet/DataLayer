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
    public static class Classes
    {
        static int? GetNullId(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int n;
            if (!int.TryParse(id, out n)) return null;
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

        static byte? GetNullByte(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int n;
            if (!int.TryParse(id, out n)) return null;
            return (byte)n;
        }

        static byte GetByte(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            int n;
            if (!int.TryParse(id, out n)) return 0;
            return (byte)n;
        }
        static string GetString(object o)
        {
            if (o == null) return null;
            return o.ToString();
        }

        public class ClassData : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public string parent_class_id {get; set;}
            [DataMember] public int hierarchy_level {get; set;}
            [DataMember] public string name { get; set; }
            [DataMember] public string description { get; set; }
            [DataMember] public bool? active { get; set; }

            [DataMember] public string priority_id { get; set; }
            [DataMember] public string priority { get; set; }

            [DataMember] public int? level { get; set; }
            [DataMember] public string level_name { get; set; }

            [DataMember] public string last_resort_tech_userid {get; set;}
            [DataMember] public string last_resort_tech {get; set;}

            [DataMember] public string class_type_id {get; set;}
            [DataMember] public string class_type {get; set;}

            [DataMember] public string routing_type_id {get; set;}
            [DataMember] public string routing_type {get; set;}
        }

        static public Dictionary<string, string> GetFieldAlias()
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            a.Add("key", "cast(l.id as nvarchar(MAX))");
            a.Add("parent_class_id", "cast(l.ParentId as nvarchar(MAX))");
            a.Add("hierarchy_level", "t.Treelevel");
            a.Add("name", "Name");
            a.Add("description", "txtDesc");
            a.Add("active", "( not l.btInactive)");
            a.Add("priority_id", "cast(l.intPriorityId as nvarchar(MAX))");
            a.Add("level", "tintLevelOverride");
            a.Add("last_resort_tech_userid", "cast(l.LastResortTechId as nvarchar(MAX))");
            a.Add("class_type_id", "cast(l.tintClassType as nvarchar(MAX))");
            a.Add("routing_type_id", "cast(l.ConfigDistributedRouting as nvarchar(MAX))");
            return a;
        }

        static void FillClass(ClassData s, Context.Tbl_class d)
        {
            if (s.IsAdded("parent_class_id")) d.ParentId = GetNullId(s.parent_class_id);
            if (s.IsAdded("name")) d.Name = s.name;
            if (s.IsAdded("description")) d.TxtDesc = s.description;
            if (s.IsAdded("active") && s.active!=null) d.BtInactive = !(bool)s.active;
            if (s.IsAdded("priority_id")) d.IntPriorityId = GetNullId(s.priority_id);
            if (s.IsAdded("level")) d.TintLevelOverride = (s.level == null ? null : (byte?)(byte)((int)s.level));
            if (s.IsAdded("last_resort_tech_userid")) d.LastResortTechId = GetId(s.last_resort_tech_userid);
            if (s.IsAdded("class_type_id")) d.TintClassType = GetByte(s.class_type_id);
            if (s.IsAdded("routing_type_id")) d.ConfigDistributedRouting = GetByte(s.routing_type_id);
        }

        public static IEnumerable<ClassData> SelectClasses(Guid OrganizationId, int DepartmentId, string where)
        {
            if (where == string.Empty) where = null;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            var c = dc.Sp_SelectClassesDynamicWhere(DepartmentId, where);
            var ld = c.Select<Context.Sp_SelectClassesDynamicWhereResult, ClassData>(r => new ClassData()
            {
                key = GetString(r.Id),
                parent_class_id = GetString(r.ParentId),
                name = r.Name,
                description = r.TxtDesc,
                active = r.BtInactive==null?null:(bool?)(!r.BtInactive),
                priority_id = GetString(r.IntPriorityId),
                priority = r.PriorityName,
                level = r.TintLevelOverride,
                hierarchy_level = r.HierarchyLevel == null ? 0 : (int)r.HierarchyLevel,
                last_resort_tech_userid = GetString(r.LastResortTechId),
                last_resort_tech = r.LastResortTechName,
                class_type_id = GetString(r.TintClassType),
                routing_type_id = GetString(r.ConfigDistributedRouting)
            });

            return ld;
        }
        public static void InsertClass(Guid OrganizationId, int DepartmentId, ClassData cd)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            Context.Tbl_class c = new Context.Tbl_class();
            FillClass(cd, c);
            dc.Tbl_class.InsertOnSubmit(c);
            dc.SubmitChanges();
        }

        public static void UpdateClass(Guid OrganizationId, int DepartmentId, int ClassId, ClassData cd)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            var c = dc.Tbl_class.Where(cc => cc.Id == ClassId && cc.Company_id == DepartmentId).FirstOrNull();
            if (c == null) return;
            FillClass(cd, c);
            dc.SubmitChanges();
        }
    }
}
