using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class SupportPortal : DBAccess
    {
        public static DataRow Select(int dId, int accountID, Guid orgID)
        {
            return SelectRecord("sp_SelectSupportPortal", new SqlParameter[] { new SqlParameter("@DId", dId),
            new SqlParameter("@AccountID", accountID)}, orgID);
        }

        public static void Update(int dId, int accountID, string header, string footer, string css)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountID != 0)
            {
                _params[1].Value = accountID;
            }
            else
            {
                _params[1].Value = DBNull.Value;
            }
            _params[2] = new SqlParameter("@Header", header);
            _params[3] = new SqlParameter("@Footer", footer);
            _params[4] = new SqlParameter("@Css", css);

            UpdateData("sp_UpdateSupportPortal", _params);
        }

        public static void UpdateAccountSettings(int dId, int accountID, bool btCfgSupportPortal,
            bool btSPLimitNewUsersToKnownEmailSuffixes, string supportPhone, string supportEmail, string title,
            string logoBackLinkURL, bool disableCSS, string facebook, string twitter1, string twitter2)
        {
            SqlParameter[] _params = new SqlParameter[12];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@AccountID", accountID);
            _params[2] = new SqlParameter("@btCfgSupportPortal", btCfgSupportPortal);
            _params[3] = new SqlParameter("@btSPLimitNewUsersToKnownEmailSuffixes", btSPLimitNewUsersToKnownEmailSuffixes);
            _params[4] = new SqlParameter("@SupportPhone", supportPhone);
            _params[5] = new SqlParameter("@SupportEmail", supportEmail);
            _params[6] = new SqlParameter("@SPTitle", title);
            _params[7] = new SqlParameter("@LogoBackLinkURL", logoBackLinkURL);
            _params[8] = new SqlParameter("@SPDisableCSS", disableCSS);
            _params[9] = new SqlParameter("@SPFacebook", facebook);
            _params[10] = new SqlParameter("@SPTwitter1", twitter1);
            _params[11] = new SqlParameter("@SPTwitter2", twitter2);

            UpdateData("sp_UpdateAccountSupportPortalSettings", _params);
        }

        public static DataTable SelectSupportPortalLinks(int dId, int accountID, Guid orgID)
        {
            return SelectRecords("sp_SelectSupportPortalLinks", new SqlParameter[] { new SqlParameter("@DId", dId),
                new SqlParameter("@AccountID", accountID)}, orgID);
        }

        public static void InsertSupportPortalLink(int dId, int accountID, string title,
            string url, int order)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@AccountID", SqlDbType.Int);
            if (accountID != 0)
            {
                _params[1].Value = accountID;
            }
            else
            {
                _params[1].Value = DBNull.Value;
            }
            _params[2] = new SqlParameter("@Title", title);
            _params[3] = new SqlParameter("@Url", url);
            _params[4] = new SqlParameter("@OrderIndex", order);

            UpdateData("sp_InsertSupportPortalLink", _params);
        }

        public static void UpdateSupportPortalLink(int dId, int supportPortalLinkID, string title,
            string url, int order)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@SupportPortalLinkID", supportPortalLinkID);
            _params[2] = new SqlParameter("@Title", title);
            _params[3] = new SqlParameter("@Url", url);
            _params[4] = new SqlParameter("@OrderIndex", order);

            UpdateData("sp_UpdateSupportPortalLink", _params);
        }

        public static void DeleteSupportPortalLink(int dId, int supportPortalLinkID)
        {
            UpdateData("sp_DeleteSupportPortalLink", new SqlParameter[]
			{
				 new SqlParameter("@DId", dId),
				 new SqlParameter("@SupportPortalLinkID", supportPortalLinkID),
			});
        }

        public static DataRow SelectOneLink(int dId, int supportPortalLinkID)
        {
            return SelectRecord("sp_SelectSupportPortalLink", new SqlParameter[] { new SqlParameter("@DId", dId),
            new SqlParameter("@SupportPortalLinkID", supportPortalLinkID)});
        }
    }
}
