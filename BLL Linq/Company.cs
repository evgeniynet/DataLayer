using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lib.bwa.bigWebDesk.LinqBll
{
    static public class Company
    {
        public struct CompanyInfo
        {
            public int DepartmentId;
            public Guid InstanceId;
            public Guid OrganizationId;
        }

        public static CompanyInfo Get(string OrgAlias, string InstAlias, Guid OrgGuid, Guid InstGuid)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext.Create(OrgAlias, InstAlias, OrgGuid, InstGuid);
            CompanyInfo ret = new CompanyInfo();
            if (dc == null || dc.OrganizationId==null || dc.OrganizationId==Guid.Empty) return ret;
            if (dc.InstaceId != null) ret.InstanceId = (Guid)dc.InstaceId;
            ret.OrganizationId = (Guid)dc.OrganizationId;
            if (dc.DepartmentId!=null) ret.DepartmentId = (int)dc.DepartmentId;
            return ret;
        }
    }
}
