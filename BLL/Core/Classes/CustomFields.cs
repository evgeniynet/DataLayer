using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for CustomFields
    /// </summary>
    public class CustomFields : DBAccess
    {

        public enum Type { TextBox = 1, TextArea = 2, DropDown = 3, Checkboxes = 4 }

        public static string GetTypeName(int type)
        {
            return GetTypeName((Type)type);
        }
        public static string GetTypeName(Type type)
        {
            switch (type)
            { 
                case Type.Checkboxes:
                    return "Checkboxes";
                case Type.DropDown:
                    return "Drop Down";
                case Type.TextArea:
                    return "Text Area";
                case Type.TextBox:
                    return "Text Box";
            }
            return "";
        }
        public static DataTable SelectTicketCustomFields(int DeptID, int classID)
        {
            return SelectTicketCustomFields(DeptID, classID, Guid.Empty);
        }

        public static DataTable SelectTicketCustomFields(int DeptID, int classID, Guid orgId)
        {
            return SelectRecords("sp_SelectCustomFields", new SqlParameter[] { 
                new SqlParameter("@DepartmentId", DeptID),
                new SqlParameter("@ClassID", classID) 
            }, orgId);
        }

        public static void Insert(int DepartmentId, string Caption, byte Type, string Choices, bool Required,
    bool DisableUserEditing, string DefaultValue, int Position, bool IsForTech, int classID)
        {
            Insert(Guid.Empty, DepartmentId, Caption, Type, Choices, Required, DisableUserEditing, DefaultValue, Position, IsForTech, classID);
        }

        public static void Insert(Guid OrgId, int DepartmentId, string Caption, byte Type, string Choices, bool Required,
            bool DisableUserEditing, string DefaultValue, int Position, bool IsForTech, int classID)
        {
            Update(OrgId, DepartmentId, 0, Caption, Type, Choices, Required, DisableUserEditing, DefaultValue, Position, IsForTech, classID);
        }

        public static void Update(int DepartmentId, int FieldId, string Caption, byte Type, string Choices, bool Required,
            bool DisableUserEditing, string DefaultValue, int Position, bool IsForTech, int classID)
        {
            Update(Guid.Empty, DepartmentId, FieldId, Caption, Type, Choices, Required, DisableUserEditing, DefaultValue, Position, IsForTech, classID);
        }

        public static void Update(Guid OrgId, int DepartmentId, int FieldId, string Caption, byte Type, string Choices, bool Required,
            bool DisableUserEditing, string DefaultValue, int Position, bool IsForTech, int classID)
        {
            SqlParameter sqlpClass = new SqlParameter("@Class_id", SqlDbType.Int);
            if (classID > 0)
            {
                sqlpClass.Value = classID;
            }
            else
            {
                sqlpClass.Value = DBNull.Value;
            }
            UpdateData("sp_UpdateCustomField", new SqlParameter[]{
                new SqlParameter("@DepartmentId", DepartmentId),
                new SqlParameter("@FieldId", FieldId),
                new SqlParameter("@Caption", Caption),
                new SqlParameter("@Type", Type),
                new SqlParameter("@Choices", Choices),
                new SqlParameter("@Required", Required),
                new SqlParameter("@DisableUserEditing", DisableUserEditing),
                new SqlParameter("@DefaultValue", DefaultValue),
                new SqlParameter("@Position", Position),
                new SqlParameter("@IsForTech", IsForTech),
                sqlpClass
            }, OrgId);
        }

        public static DataRow SelectTicketCustomField(int DeptID, int customFieldId)
        {
            return SelectRecord("sp_SelectCustomField", new SqlParameter[] { new SqlParameter("@DepartmentId", DeptID), new SqlParameter("@FieldId", customFieldId) });
        }

        public static void Delete(int departmentId, int customFieldId)
        {

            UpdateData("sp_DeleteCustomField", new SqlParameter[]{
                new SqlParameter("@DepartmentId", departmentId),
                new SqlParameter("@FieldId", customFieldId)
            });
        }

        public static void Move(int DeptID, int? sorceCustomFieldId, int? destCustomFieldId)
        {
            UpdateData("sp_MoveCustomField", new SqlParameter[]{
                    new SqlParameter("@DId", DeptID), 
                    new SqlParameter("@SorceCustomFieldId", sorceCustomFieldId), 
					new SqlParameter("@DestCustomFieldId", destCustomFieldId)
                });
        }
    }
}
