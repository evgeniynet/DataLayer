using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class EmailDropBoxes : DBAccess
    {

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

        public static DataTable SelectAll(Guid OrgId, int DeptID)
        {
            return SelectRecords("sp_SelectEmailDropBoxes", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
        }

        public static DataTable SelectOne(int DeptID, int DropBoxId)
        {
            return SelectRecords("sp_SelectEmailDropBoxes", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DropBoxId) });
        }

        public static DataRow SelectOne(Guid OrgId, int DeptID, string DropBoxPseudoId)
        {
            SqlParameter _pId=new SqlParameter("@Id", SqlDbType.Int);
            _pId.Value = DBNull.Value;
            SqlParameter _pPseudoId = new SqlParameter("@PseudoId", SqlDbType.Char, 6);
            _pPseudoId.Value = DropBoxPseudoId;
            return SelectRecord("sp_SelectEmailDropBoxes", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pId, _pPseudoId }, OrgId);
        }

        public static void UpdateExternalEmail(Guid OrgId, int DeptId, string PseudoId, string ExternalEmail)
        {
            UpdateData("sp_UpdateEmailDropBoxExternalEmail", new SqlParameter[] { new SqlParameter("@DId", DeptId), new SqlParameter("@PseudoId", PseudoId), new SqlParameter("@ExternalEmail", ExternalEmail) }, OrgId);
        }

        public static int Update(int DeptID, int DropBoxId, string PseudoId, string DropBoxName, int TechnicianId, int ClassId, byte Level, int NormalPriorityId, int HighPriorityId, int LowPriorityId, string ExternalEmail)
        {
            return Update(Guid.Empty, DeptID, DropBoxId, PseudoId, DropBoxName, TechnicianId, ClassId, Level, NormalPriorityId, HighPriorityId, LowPriorityId, ExternalEmail);
        }

        public static int Update(Guid OrgId, int DeptID, int DropBoxId, string PseudoId, string DropBoxName, int TechnicianId, int ClassId, byte Level, int NormalPriorityId, int HighPriorityId, int LowPriorityId, string ExternalEmail)
        {
            SqlParameter _pId=new SqlParameter("@Id", SqlDbType.Int);
            _pId.Direction=ParameterDirection.InputOutput;
            _pId.Value=DropBoxId;
            SqlParameter _pTechnicianId = new SqlParameter("@TechnicianId", SqlDbType.Int);
            if (TechnicianId != 0) _pTechnicianId.Value = TechnicianId;
            else _pTechnicianId.Value = DBNull.Value;
            SqlParameter _pClassId=new SqlParameter("@ClassId", SqlDbType.Int);
            if (ClassId != 0) _pClassId.Value = ClassId;
            else _pClassId.Value = DBNull.Value;
            SqlParameter _pLevel = new SqlParameter("@tintLevel", SqlDbType.TinyInt);
            if (Level != 0) _pLevel.Value = Level;
            else _pLevel.Value = DBNull.Value;
            SqlParameter _pNormalPriorityId = new SqlParameter("@NormalPriorityId", SqlDbType.Int);
            if (NormalPriorityId != 0) _pNormalPriorityId.Value = NormalPriorityId;
            else _pNormalPriorityId.Value = DBNull.Value;
            SqlParameter _pHighPriorityId = new SqlParameter("@HighPriorityId", SqlDbType.Int);
            if (HighPriorityId != 0) _pHighPriorityId.Value = HighPriorityId;
            else _pHighPriorityId.Value = DBNull.Value;
            SqlParameter _pLowPriorityId = new SqlParameter("@LowPriorityId", SqlDbType.Int);
            if (LowPriorityId != 0) _pLowPriorityId.Value = LowPriorityId;
            else _pLowPriorityId.Value = DBNull.Value;
            SqlParameter _pExternalEmail = new SqlParameter("@ExternalEmail", SqlDbType.NVarChar, 255);
            if (!string.IsNullOrEmpty(ExternalEmail)) _pExternalEmail.Value = ExternalEmail;
            else _pExternalEmail.Value = DBNull.Value;
            UpdateData("sp_UpdateEmailDropBoxes", new SqlParameter[] { new SqlParameter("@DId", DeptID), _pId, new SqlParameter("@PseudoId", PseudoId), new SqlParameter("@DropBoxName", DropBoxName), _pTechnicianId, _pClassId, _pLevel, _pNormalPriorityId, _pHighPriorityId, _pLowPriorityId, new SqlParameter("@IsDefault", false), _pExternalEmail}, OrgId);
            return (int)_pId.Value;
        }

        public static void Delete(int DeptID, int DropBoxId)
        {
            UpdateData("sp_DeleteEmailDropBoxes", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DropBoxId) });
        }
    }
}
