using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class Configuration : DBAccess
    {
        public enum Turn
        {
            On,
            Off
        }

        public static void EscalationAndLevels(int DepartmentId, int LastResortTechId, Turn turn)
        {
            SqlParameter pTurn = new SqlParameter();
            pTurn.ParameterName = "@vchOnOff";
            if (turn == Turn.On)
                pTurn.Value = "on";
            else if (turn == Turn.Off)
                pTurn.Value = "off";

            SqlParameter pLastResortTechId = new SqlParameter();
            pLastResortTechId.ParameterName = "@intLastResortId";
            if (LastResortTechId > 0)
                pLastResortTechId.Value = LastResortTechId;
            else
                pLastResortTechId.Value = DBNull.Value;

            Companies.UpdateData(DepartmentId, "sp_UpdateConfigLevel", new SqlParameter[]{
                new SqlParameter("@DId", DepartmentId),
                pLastResortTechId,
                pTurn
            });
        }
    }
}
