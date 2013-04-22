using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class AccessKey : DBAccess
    {
        public static DataTable SelectAccessKeysTypes(int CompanyID)
        {
            return SelectRecords("sp_SelectAccessKeysTypes", new SqlParameter[] { new SqlParameter("@CompanyID", CompanyID) });
        }

        public static int InsertAccessKey(int CompanyID, int UserID, int AccessKeyTypeID, bool IsPrivateKey)
        {
            return UpdateByQuery(String.Format("INSERT INTO AccessKey (CompanyID, UserID, AccessKeyTypeID, PublicAccessKey, PrivateAccessKey) VALUES ({0}, {1}, {2}, '{3}', '{4}')", CompanyID, UserID, AccessKeyTypeID, GenerateGuid(), IsPrivateKey ? GenerateGuid() : String.Empty));                
        }

        public static int DeleteAccessKey(int CompanyID, int idAccessKey)
        {
            return UpdateByQuery(String.Format("DELETE FROM AccessKey WHERE CompanyID = {0} AND AccessKeyID = {1}", CompanyID, idAccessKey));                
        }

        public static int RegenerateGuides(int CompanyID, int aid, bool IsPrivateKey)
        {
            return UpdateByQuery(String.Format("UPDATE AccessKey SET PublicAccessKey = '{2}', PrivateAccessKey = '{3}' WHERE CompanyID = {0} AND AccessKeyID = {1}", CompanyID, aid, GenerateGuid(), IsPrivateKey ? GenerateGuid() : String.Empty));                
        }

        private static string GenerateGuid()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
