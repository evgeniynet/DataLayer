using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for RouteOrder.
    /// </summary>
    public class RouteOrder : DBAccess
    {
        public static DataTable SelectAll(int DeptID)
        {
            return SelectRecords("sp_SelectRoutingOrder", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }

        public static DataTable SelectLevels(int DeptID)
        {
            return SelectRecords("sp_SelectLevelsLite", new SqlParameter[] { new SqlParameter("@DId", DeptID) });
        }
     
        public static bool SelectRoutingOrderDefault(int DeptID)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            SqlParameter _pDefault = new SqlParameter("@btDefault", SqlDbType.Bit);
            _pDefault.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectRoutingOrderDefault",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),
                                            _pDefault,                                                                       
                                            });
            return bool.Parse(_pDefault.Value.ToString());
        }

        public static int UpdateRoutingOrderMode(int DeptID)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateRoutingOrderMode",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID)                                            
                                            });
            return (int)_pRVAL.Value;
        }

        //-1: Route already exists.
        public static int InsertRoute(int DeptID, int Router)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_InsertRoute",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),                 
                                            new SqlParameter("@tintRoute", Router)
                                            });
            return (int)_pRVAL.Value;
        }

        public static int UpdateRoute(int DeptID, string Direction, int Router)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateRoute",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),   
                                            new SqlParameter("@vchDirection", Direction),
                                            new SqlParameter("@tintRoute", Router)
                                            });
            return (int)_pRVAL.Value;
        }

        //-1: Cannot delete the last route.  Must add a new route first or switch to the default routing method. 
        public static int DeleteRoute(int DeptID, int Router)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DeleteRoute",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),                                          
                                            new SqlParameter("@tintRoute", Router)
                                            });
            return (int)_pRVAL.Value;
        }
    }
}
