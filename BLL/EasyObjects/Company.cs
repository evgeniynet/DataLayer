using System;
using System.Data;
using System.Data.Common;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;

using BWA.bigWebDesk.DAL;

namespace BWA.bigWebDesk.BLL
{
    public class Company : BWA.bigWebDesk.DAL.tbl_company
    {
        public enum Status
        {
            TRIAL = 1,
            CLIENT = 2,
            LOST = 3
        }

        public Company(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static bool CompanyExists(Guid organizationId, int BWAAccountID,
                                         string name)
        {
            Company comp = new Company(organizationId);
            comp.Where.Company_name.Value = name;
            comp.Where.Company_name.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Where.BWAAccountId.Value = BWAAccountID;
            comp.Where.BWAAccountId.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Query.Load();
            return (comp.RowCount == 0 ? false : true);
        }

        public static bool CompanyExists(Guid organizationId, int BWAAccountID,
                                         int companyID,
                                         string name)
        {

            Company comp = new Company(organizationId);
            comp.Where.Company_id.Value = companyID;
            comp.Where.Company_id.Operator = WhereParameter.Operand.Equal;
            comp.Where.Company_name.Value = name;
            comp.Where.Company_name.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Where.BWAAccountId.Value = BWAAccountID;
            comp.Where.BWAAccountId.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Query.Load();
            return (comp.RowCount == 0 ? false : true);
        }

        public static int GetCompanyID(Guid organizationId, int BWAAccountID,
                                       int BWADepartmentID,
                                       string name)
        {
            Company comp = new Company(organizationId);
            comp.Where.Company_name.Value = name;
            comp.Where.Company_name.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Where.BWAAccountId.Value = BWAAccountID;
            comp.Where.BWAAccountId.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Where.BWADepartmentId.Value = BWADepartmentID;
            comp.Where.BWADepartmentId.Operator = NCI.EasyObjects.WhereParameter.Operand.Equal;
            comp.Query.Load();
            return (comp.RowCount == 0 ? 0 : comp.Company_id);
        }

        // for external use - web service
        public static DataSet GetCompanyData(Guid organizationId, int companyID)
        {
            Company comp = new Company(organizationId);
            comp.Where.Company_id.Value = companyID;
            comp.Where.Company_id.Operator = WhereParameter.Operand.Equal;
            comp.Query.AddResultColumn(tbl_companySchema.BWAAccountId);
            comp.Query.AddResultColumn(tbl_companySchema.BWADepartmentId);
            comp.Query.AddResultColumn(tbl_companySchema.Company_name);
            comp.Query.Load();
            if (comp.DefaultView.Table.Rows.Count > 0)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(comp.DefaultView.Table);
                return ds;
            }
            return null;
        }

        public virtual string SelectCompanyName(int department_id)
        {
            string _result = string.Empty;

            Database db = GetDatabase();

            string sqlCommand = string.Empty;

            sqlCommand = "SELECT company_name FROM tbl_company Where company_id = " + department_id.ToString();

            DbCommand dbCommand = db.GetSqlStringCommand(sqlCommand);

            IDataReader _reader = base.LoadFromSqlReader(dbCommand);
            if (_reader != null)
            {
                while (_reader.Read())
                {
                    if (!_reader.IsDBNull(0))
                        _result = _reader[0].ToString();

                    break;
                };

                _reader.Close();
            };

            return _result;
        }
    }
}
