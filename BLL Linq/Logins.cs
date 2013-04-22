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
    public static class Logins
    {
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

        public class LoginData : lib.bwa.bigWebDesk.LinqBll.Context.AccountedData
        {
            [DataMember] public string key {get; set;}
            [DataMember] public string email { get; set; }
            [DataMember] public string first_name {get; set;}
            [DataMember] public string last_name {get; set;}
            [DataMember] public string title {get; set;}
            [DataMember] public string organization {get; set;}
            [DataMember] public string phone {get; set;}
            [DataMember] public string mobile_phone {get; set;}
            [DataMember] public string location {get; set;}
            [DataMember] public string location_id {get; set;}
            [DataMember] public string user_type_id {get; set;}
            [DataMember] public string user_type {get; set;}
            [DataMember] public int? global_level_setting {get; set;}
        }

        static string GetString(object o)
        {
            if (o == null) return null;
            return o.ToString();
        }

        static public Dictionary<string, string> GetFieldAlias()
        {
            Dictionary<string, string> a = new Dictionary<string, string>();
            a.Add("key", "cast(l.id as varchar(MAX))");

            a.Add("email", "l.Email");
            a.Add("first_name", "l.FirstName");
            a.Add("last_name", "l.LastName");
            a.Add("title", "l.Title");
            a.Add("organization", "j.vchOrganization");
            a.Add("phone", "l.Phone");
            a.Add("mobile_phone", "l.MobilePhone");
            a.Add("location", "lv.LocationFullName");
            a.Add("location_id", "j.location_id");
            a.Add("user_type_id", "j.UserType_Id");
            a.Add("user_type", "t.Name");
            a.Add("global_level_setting", "j.tintLevel");
            return a;
        }

        static void FillLogin(LoginData s, Context.Tbl_Logins d)
        {
            if (s.IsAdded("email")) d.Email = s.email;
            if (s.IsAdded("first_name")) d.FirstName = s.first_name;
            if (s.IsAdded("last_name")) d.LastName = s.last_name;
            if (s.IsAdded("title")) d.Title = s.title;
            if (s.IsAdded("phone")) d.Phone = s.phone;
            if (s.IsAdded("mobile_phone")) d.MobilePhone = s.mobile_phone;
        }
        static void FillJunk(LoginData s, Context.Tbl_LoginCompanyJunc d)
        {
            if (s.IsAdded("organization")) d.VchOrganization = s.organization;
            if (s.IsAdded("user_type_id") && s.user_type_id!=null) d.UserType_Id = GetId(s.user_type_id);
            if (s.IsAdded("global_level_setting")) d.TintLevel = (s.global_level_setting == null) ? null : (byte?)((byte)((int)s.global_level_setting));
        }

        public static IEnumerable<LoginData> SelectLogins(Guid OrganizationId, int DepartmentId, string where)
        {
            if (where == string.Empty) where = null;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);

            var l = dc.Sp_SelectLoginsDynamicWhere(DepartmentId, where);

            var ld = l.Select<Context.Sp_SelectLoginsDynamicWhereResult, LoginData>(r => new LoginData()
            {
                key = GetString(r.Id),
                email = r.Email,
                first_name = r.FirstName,
                last_name = r.LastName,
                title = r.Title,
                organization = r.Organization,
                phone = r.Phone,
                mobile_phone = r.MobilePhone,
                location = r.LocationFullName,
                location_id = GetString(r.Location_id),
                user_type_id = GetString(r.UserType_Id),
                user_type = r.UserTypeName,
                global_level_setting = r.TintLevel
            });

            return ld;
        }

        public static void InsertLogin(Guid OrganizationId, int DepartmentId, LoginData ld)
        {
            if (string.IsNullOrEmpty(ld.email)) return;
            ld.email=ld.email.ToLower();
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            var l = (from ll in dc.Tbl_Logins where ll.Email.ToLower() == ld.email select ll).FirstOrNull();
            if (l == null)
            {
                l = new Context.Tbl_Logins();
                FillLogin(ld, l);
                dc.Tbl_Logins.InsertOnSubmit(l);
                dc.SubmitChanges();
            }
            else
            {
                FillLogin(ld, l);
            }
            var j = (from jj in dc.Tbl_LoginCompanyJunc where jj.Company_id==DepartmentId && jj.Login_id==l.Id select jj).FirstOrNull();
            if (j == null)
            {
                j = new Context.Tbl_LoginCompanyJunc();
                j.Company_id = DepartmentId;
                j.Login_id = l.Id;
                FillJunk(ld, j);
                dc.Tbl_LoginCompanyJunc.InsertOnSubmit(j);
            }
            else
            {
                FillJunk(ld, j);
            }
            
            dc.SubmitChanges();
        }

        public static void UpdateLogin(Guid OrganizationId, int DepartmentId, int LoginCompanyJuncId, LoginData ld)
        {
            if(LoginCompanyJuncId<1) return;

            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrganizationId, DepartmentId);
            var j = (from jj in dc.Tbl_LoginCompanyJunc where jj.Company_id == DepartmentId && jj.Id == LoginCompanyJuncId select jj).FirstOrNull();
            if (j == null) return;

            var l = (from ll in dc.Tbl_Logins where ll.Id == j.Login_id select ll).FirstOrNull();
            if (l == null) return;
            
            FillJunk(ld, j);
            FillLogin(ld, l);
            dc.SubmitChanges();
       }
    }
}
