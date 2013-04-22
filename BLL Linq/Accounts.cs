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
    public static class Accounts
    {
        public class AccountData : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public int? bwd_account_number {get; set;}
            [DataMember] public bool? organization_account {get; set;}
            [DataMember] public string internal_account_number {get; set;}
            [DataMember] public string ref_number_1 {get; set;}
            [DataMember] public string ref_number_2 {get; set;}
            [DataMember] public string support_group_id {get; set;}
            [DataMember] public string support_group_name{get; set;}
            [DataMember] public string note {get; set;}
            [DataMember] public string cust_field_1 {get; set;}
            [DataMember] public string cust_field_2 {get; set;}
            [DataMember] public string cust_field_3 {get; set;}
            [DataMember] public string cust_field_4 {get; set;}
            [DataMember] public string cust_field_5 {get; set;}
            [DataMember] public string cust_field_6 {get; set;}
            [DataMember] public string cust_field_7 {get; set;}
            [DataMember] public string cust_field_8 {get; set;}
            [DataMember] public string cust_field_9 {get; set;}
            [DataMember] public string cust_field_10 {get; set;}
            [DataMember] public string cust_field_11 {get; set;}
            [DataMember] public string cust_field_12 {get; set;}
            [DataMember] public string cust_field_13 {get; set;}
            [DataMember] public string cust_field_14 {get; set;}
            [DataMember] public string cust_field_15 {get; set;}
            [DataMember] public DateTime? cust_date_time_1 {get; set;}
            [DataMember] public DateTime? cust_date_time_2 {get; set;}
            [DataMember] public bool? active {get; set;}
            [DataMember] public string name {get; set;}
            [DataMember] public string report_tech_userid {get; set;}
            [DataMember] public string email_suffix {get; set;}
            [DataMember] public bool? time_tracking {get; set;}
        }

        static public Dictionary<string, string> GetFieldAlias()
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            a.Add("key", "cast(l.id as varchar(MAX))");
            a.Add("parent_class_id", "cast(l.ParentId as varchar(MAX))");
            a.Add("hierarchy_level", "t.Treelevel");
            a.Add("name", "Name");
            a.Add("description", "txtDesc");
            a.Add("active", "( not l.btInactive)");
            a.Add("priority_id", "cast(l.intPriorityId as varchar(MAX))");
            a.Add("level", "tintLevelOverride");
            a.Add("last_resort_tech_userid", "cast(l.LastResortTechId as varchar(MAX))");
            a.Add("class_type_id", "cast(l.tintClassType as varchar(MAX))");
            a.Add("routing_type_id", "cast(l.ConfigDistributedRouting as varchar(MAX))");
            return a;
        }
    }
}
