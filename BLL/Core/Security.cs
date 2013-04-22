using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

/// <summary>
/// Summary description for Security
/// </summary>
namespace bigWebApps.bigWebDesk
{
    public class Security
    {
        public static void XSSBlock(HttpRequest request)
        {
            for (int i = 0; i < request.Form.Count; i++)
                request.Form[request.Form.GetKey(i)] = request.Form[request.Form.GetKey(i)].Replace("<", "&lt;").Replace(">", "&gt;");
            for (int i = 0; i < request.Cookies.Count; i++)
                request.Cookies[request.Cookies.GetKey(i)].Value = request.Cookies[request.Cookies.GetKey(i)].Value.Replace("<", "&lt;").Replace(">", "&gt;");
        }

        public static string SQLInjectionBlock(string query)
        {
//            string[] _badChars = new string[] {"select", "drop", ";", "--", "insert", "delete", "xp_" };
            string[] _badChars = new string[] { "drop ", ";", "--", "insert ", "delete ", "xp_" };
            string _qLower = query.ToLower();
            foreach(string _bad in _badChars)
            {
                int i=_qLower.IndexOf(_bad);
                while (i >= 0)
                {
                    _qLower=_qLower.Remove(i, _bad.Length);
                    query=query.Remove(i, _bad.Length);
                    i = _qLower.IndexOf(_bad);
                }
            }
            return query;
        }
    }
}