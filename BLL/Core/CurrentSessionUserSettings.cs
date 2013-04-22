using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace bigWebApps.bigWebDesk
{
    [Serializable]
    public class UserSetting : StringDictionary
    {
        private string m_UserSettingName = string.Empty;

        UserSetting(string UserSettingName)
        {
            m_UserSettingName = UserSettingName;
        }

        public bool IsDefined
        {
            get { return (base.Count > 0); }
        }

        public string Value
        {
            get { return this[m_UserSettingName]; }
            set { this[m_UserSettingName] = value; }
        }

        public string QueryString
        {
            get
            {
                StringBuilder _sb = new StringBuilder();
                foreach (DictionaryEntry _de in this) _sb.Append("&" + _de.Key.ToString() + "=" + _de.Value.ToString());
                return _sb.ToString().TrimStart('&');
            }
            set
            {
                this.Clear();
                string[] _arr = value.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string _pair in _arr)
                {
                    string[] _de = _pair.Split('=');
                    if (_de.Length > 1) this[_de[0]] = _de[1];
                }
            }
        }

        public static UserSetting GetSettings(string ValuesType)
        {
            if (HttpContext.Current.Session["UserSettingVals_" + ValuesType] == null) HttpContext.Current.Session["UserSettingVals_" + ValuesType] = new UserSetting("UserSettingVals_" + ValuesType);
            return (UserSetting)HttpContext.Current.Session["UserSettingVals_" + ValuesType];
        }

        public static void ClearSettings()
        {
            foreach (string _key in HttpContext.Current.Session.Keys)
                if (_key.IndexOf("UserSettingVals_") == 0 && HttpContext.Current.Session[_key] != null) ((UserSetting)HttpContext.Current.Session[_key]).Clear();
        }

        public static void RemoveSettings(string ValuesType)
        {
            HttpContext.Current.Session.Remove("UserSettingVals_" + ValuesType);
        }
    }
}
