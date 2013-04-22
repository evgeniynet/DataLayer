using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for CreationCategories.
    /// </summary>
    public class ResolutionCategories : DBAccess
    {
        public static DataRow Select(int DeptID, int ID)
        {
            return SelectRecord("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", ID) });
        }

        public static DataTable SelectAll(int DeptID)
        {
            return SelectAll(Guid.Empty, DeptID);
        }

        public static DataTable SelectAll(Guid OrgId, int DeptID)
        {
            return SelectRecords("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID) }, OrgId);
        }

        public static DataTable SelectAllActive(int DeptID)
        {
            return SelectRecords("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DBNull.Value), new SqlParameter("@btResolved", DBNull.Value), new SqlParameter("@btInactive", false) });
        }
        public static DataTable SelectAllActive(int DeptID, bool IsResolved)
        {
            return SelectRecords("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DBNull.Value), new SqlParameter("@btResolved", IsResolved), new SqlParameter("@btInactive", false) });
        }

        public static DataTable SelectByInactiveStatus(int DeptID, InactiveStatus inactiveStatus)
        {
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            if (inactiveStatus == InactiveStatus.DoesntMatter)
                _pInactive.Value = DBNull.Value;
            else
                _pInactive.Value = inactiveStatus;

            return SelectRecords("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@Id", DBNull.Value), new SqlParameter("@btResolved", DBNull.Value), _pInactive });
        }

        public static DataTable SelectByInactiveStatusAndResolved(int DeptID, InactiveStatus inactiveStatus, bool isResolved)
        {
            SqlParameter _pInactive = new SqlParameter("@btInactive", SqlDbType.Bit);
            if (inactiveStatus == InactiveStatus.DoesntMatter)
                _pInactive.Value = DBNull.Value;
            else
                _pInactive.Value = inactiveStatus;
            return SelectRecords("sp_SelectResolutions", new SqlParameter[] { new SqlParameter("@DId", DeptID), new SqlParameter("@btResolved", isResolved), _pInactive });
        }

        public static void Insert(int DeptID, int UserID, string resolutionName, bool isResolved)
        {
            Insert(DeptID, UserID, resolutionName, false, isResolved);
        }

        public static void Insert(int DeptID, int UserID, string resolutionName, bool isInactive, bool isResolved)
        {
            Insert(Guid.Empty, DeptID, UserID, resolutionName, isInactive, isResolved);
        }

        public static void Insert(Guid OrgId, int DeptID, int UserID, string resolutionName, bool isInactive, bool isResolved)
        {
            UpdateData("sp_InsertResolution",
                        new SqlParameter[] {
                                            new SqlParameter("@DepartmentId", DeptID),
                                            new SqlParameter("@UId", UserID),
                                            new SqlParameter("@ResolutionName", resolutionName),
                                            new SqlParameter("@btInactive", isInactive),
                                            new SqlParameter("@btResolved", isResolved)
                                            }, OrgId);
        }

        public static int Update(int DeptID, int ID, int UserID, string resolutionName, bool isInactive, bool isResolved)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateResolution",
                        new SqlParameter[] {
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@Id", ID),
                                            new SqlParameter("@UId", UserID),
                                            new SqlParameter("@ResolutionName", resolutionName),
                                            new SqlParameter("@btInactive", isInactive),
                                            new SqlParameter("@btResolved", isResolved),
                                            pReturnValue
                                            });
            return (int)pReturnValue.Value;
        }

        public static int Inactivate(int DeptID, int CategoryId, int UserID)
        {
            return Inactivate(DeptID, CategoryId, UserID, false);
        }

        public static int Inactivate(int DeptID, int CategoryId, int UserID, bool dontTransferTickets)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateResolution",
                        new SqlParameter[] {
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@Id", CategoryId),
                                            new SqlParameter("@UId", UserID),
                                            new SqlParameter("@ResolutionName", DBNull.Value),
                                            new SqlParameter("@btResolved", DBNull.Value),
                                            new SqlParameter("@btInactive", true),
                                            new SqlParameter("@btDontTransferTickets", dontTransferTickets),
                                            pReturnValue
                                            });
            return (int)pReturnValue.Value;
        }

        public static void Transfer(int DeptID, int fromCategoryId, int toCategoryId, bool transferAll)
        {
            UpdateData("sp_TransferResolution",
                        new SqlParameter[] {
                                            new SqlParameter("@DepartmentId", DeptID),
                                            new SqlParameter("@OldResolutionId", fromCategoryId),
                                            new SqlParameter("@NewResolutionId", toCategoryId),
                                            new SqlParameter("@btTransferAll", transferAll)
                                            });
        }


        public static DataTable GroupByResolved(DataTable source)
        {
            source.DefaultView.Sort = "btResolved ASC";
            DataTable result = source.DefaultView.ToTable();

            if (result == null)
                return result;

            DataRow newRow;

            bool isResolvedInserted = false, isUnresolvedInserted = false;

            foreach (DataRow currentRow in result.Rows)
                if (currentRow["btResolved"] != DBNull.Value && !Convert.ToBoolean(currentRow["btResolved"]))
                {
                    newRow = result.NewRow();
                    newRow["id"] = -1;
                    newRow["Name"] = "Unresolved";
                    result.Rows.InsertAt(newRow, result.Rows.IndexOf(currentRow));
                    isUnresolvedInserted = true;
                    break;
                }

            foreach (DataRow currentRow in result.Rows)
                if (currentRow["btResolved"] != DBNull.Value && Convert.ToBoolean(currentRow["btResolved"]))
                {
                    newRow = result.NewRow();
                    newRow["id"] = -1;
                    newRow["Name"] = "Resolved";
                    result.Rows.InsertAt(newRow, result.Rows.IndexOf(currentRow));
                    isResolvedInserted = true;
                    break;
                }

            if (!isUnresolvedInserted)
            {
                newRow = result.NewRow();
                newRow["id"] = -1;
                newRow["Name"] = "Unresolved (no sub-categories)";
                result.Rows.InsertAt(newRow, 0);
            }

            if (!isResolvedInserted)
            {
                newRow = result.NewRow();
                newRow["id"] = -1;
                newRow["Name"] = "Resolved (no sub-categories)";
                result.Rows.InsertAt(newRow, result.Rows.Count);
            }

            return result;
        }
    }
}
