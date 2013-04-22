using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Folder.
    /// </summary>
    public class Folders : DBAccess
    {
        public static DataTable SelectChildFolders(int DeptID, int ParentFolderId)
        {
            string _sql_query = string.Empty;

            if (ParentFolderId == 0)
            {
                _sql_query = "select *, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id and Status<>'Closed') as TicketAllOpen, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id) as TicketAll, CASE (SELECT Count(*) FROM Folders WHERE DId=" + DeptID.ToString() + " AND ParentId=F.Id) WHEN 0 THEN Cast(1 As bit)  ELSE Cast(0 As bit) END AS IsLastChildNode from Folders as F where DId=" + DeptID.ToString() + " And (ParentId=0 or ParentId IS NULL) ORDER BY F.vchName";
            }
            else
            {
                _sql_query = "select *, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id and Status<>'Closed') as TicketAllOpen, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id) as TicketAll, CASE (SELECT Count(*) FROM Folders WHERE DId=" + DeptID.ToString() + " AND ParentId=F.Id) WHEN 0 THEN Cast(1 As bit)  ELSE Cast(0 As bit) END AS IsLastChildNode from Folders as F where DID=" + DeptID.ToString() + " and ParentId=" + ParentFolderId.ToString() + " ORDER BY F.vchName";                
            };

            return SelectByQuery(_sql_query);
        }

        public static DataTable Select(int DeptID)
        {
            return SelectByQuery("select id, CASE WHEN ParentId=0 THEN NULL ELSE ParentId END as ParentId, vchName  from Folders WHERE DId=" + DeptID + "ORDER BY vchName ");
        }

        public static string GetFolderFullName(int DeptID, int FolderID)
        {
            string _result = "";
            string _sql_query = "";

            _sql_query = "Select dbo.fxGetUserFolderName(" + DeptID.ToString();
            _sql_query += ", " + FolderID.ToString() + ") as Name";


            DataTable _dt = SelectByQuery(_sql_query);
            if (_dt != null)
            {
                if (_dt.Rows.Count == 1)
                {
                    if (_dt.Rows[0]["Name"] != null)
                        _result = _dt.Rows[0]["Name"].ToString();
                };
            };

            return _result;
        }

        public static DataRow SelectOne(int DeptID, int FolderId)
        {
            DataRow _result=null;

            string _sql_query = string.Empty;
            _sql_query = "select *, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id and Status<>'Closed') as TicketAllOpen, (select count(*) from tbl_ticket where company_id=" + DeptID.ToString() + " and folder_id=F.id) as TicketAll, CASE (SELECT Count(*) FROM Folders WHERE DId=" + DeptID.ToString() + " AND ParentId=F.Id) WHEN 0 THEN Cast(1 As bit)  ELSE Cast(0 As bit) END AS IsLastChildNode from Folders as F where DID=" + DeptID.ToString() + " and Id=" + FolderId.ToString();
            DataTable _table=SelectByQuery(_sql_query);
            if (_table != null)
            {
                if (_table.Rows.Count==1)
                    _result = _table.Rows[0];
            };

            return _result;
        }

        public static int InsertFolder(int DeptID, int ParentFolderId, string NewFolderName)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_InsertFolder",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),
                                            new SqlParameter("@parentid", ParentFolderId),
                                            new SqlParameter("@vchName", NewFolderName)
                                            });
            return (int)_pRVAL.Value;
        }

        public static int UpdateFolder(int DeptID, int FolderId, string FolderName, int ParentFolderId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateFolder",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),                                            
                                            new SqlParameter("@vchName", FolderName),
                                            new SqlParameter("@Id", FolderId),
                                            new SqlParameter("@ParentId", ParentFolderId),
                                            });
            return (int)_pRVAL.Value;
        }

        public static int MoveFolder(int DeptID, int DestFolderId, int SourceFolderId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateMoveFolder",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),                                            
                                            new SqlParameter("@ParentId", DestFolderId),
                                            new SqlParameter("@FolderId", SourceFolderId)                                            
                                            });
            return (int)_pRVAL.Value;
        }

        public static int AssignTicketToFolder(int DeptID, int? FolderId, int TicketId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateTicketFolder",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DepartmentId", DeptID),                                            
                                            new SqlParameter("@TicketId", TicketId),
                                            new SqlParameter("@FolderId", FolderId)                                            
                                            });
            return (int)_pRVAL.Value;
        }

        public static int DeleteFolder(int DeptID, int FolderId)
        {
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_DeleteFolder",
                        new SqlParameter[] {_pRVAL,
                                            new SqlParameter("@DId", DeptID),                                            
                                            new SqlParameter("@FolderId", FolderId)
                                            });
            return (int)_pRVAL.Value;
        }

        public static void TransferFolder(int DepartmentId, int OldFolderId, int NewFolderId)
        {

            UpdateData("sp_TransferFolder",
                        new SqlParameter[] {
                                            new SqlParameter("@DepartmentId", DepartmentId),                                            
                                            new SqlParameter("@OldFolderId", OldFolderId),
                                            new SqlParameter("@NewFolderId", NewFolderId)
                                            });
        }

        public static DataTable SelectFolders(int dID, string searchString)
        {
            return SelectRecords("sp_SelectFolders", new SqlParameter[]
                   {
                    new SqlParameter("@DId", dID),
                    new SqlParameter("@SearchString", searchString)
                   });
        }
    }
}
