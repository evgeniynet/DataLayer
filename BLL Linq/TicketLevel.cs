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
    public class TicketLevel
    {
        public class TicketLevelData : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public int level {get; set;}
            [DataMember] public string description {get; set;}
            [DataMember] public bool? is_default {get; set;}
            [DataMember] public string name {get; set;}
        }

        static public Dictionary<string, string> GetFieldAlias()
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            a.Add("level", "tintLevel");
            a.Add("description", "Description");
            a.Add("is_default", "bitDefault");
            a.Add("name", "LevelName");
            return a;
        }

        static void FillTicket(TicketLevelData s, Context.TktLevels d)
        {
            if (s.IsAdded("level")) d.TintLevel = (byte)s.level;
            if (s.IsAdded("description")) d.Description = s.description;
            if (s.IsAdded("is_default")) d.BitDefault = s.is_default;
            if (s.IsAdded("name")) d.LevelName = s.name;
        }

        public static IEnumerable<TicketLevelData> SelectLevels(Guid OrganizationId, int DepartmentId, string where)
        {
            //new TicketDataType();
            if (where == string.Empty) where = null;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            var lvs = dc.Sp_SelectTicketLevelsDynamicWhere(DepartmentId, where).ToList();
            var lds = lvs.Select<Context.Sp_SelectTicketLevelsDynamicWhereResult, TicketLevelData>(l => new TicketLevelData {
                level = l.TintLevel==null?0:(byte)l.TintLevel,
                name = l.LevelName,
                is_default = l.BitDefault,
                description = l.Description
            });
            return lds;
        }
    }
}
