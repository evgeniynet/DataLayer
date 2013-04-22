using System;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using Micajah.Common.Bll;
using Micajah.Common.Security;

namespace bigWebApps.bigWebDesk
{

    /// <summary>
    /// Summary description for Functions.
    /// </summary>
    public class Functions
    {
        public delegate bool CheckDateRange(string p1, string p2);

        private static string FormatTimePart(int TimePart)
        {
            if (TimePart < 10) return "0" + TimePart.ToString();
            else return TimePart.ToString();
        }

        public static string DisplayDateDurationHtml(TimeSpan tspan)
        {
            string _result = string.Empty;
            if (tspan.Days != 0) _result += "<b>" + tspan.Days.ToString() + "</b>" + "d";
            if (tspan.Hours != 0 || tspan.Days != 0)
            {
                if (_result.Length > 0) _result += "&nbsp;";
                _result += "<b>" + tspan.Hours.ToString() + "</b>h";
            }
            if (_result.Length > 0) _result += "&nbsp;";
            _result += "<b>" + tspan.Minutes.ToString() + "</b>m";
            return _result;
        }

        public static void CreateHoursList(ListItemCollection ListItems)
        {
            CreateHoursList(ListItems, SortDirection.Ascending);
        }

        public static void CreateHoursList(ListItemCollection ListItems, SortDirection direction)
        {
            int from = 0, to = 23, step = 1;
            if (direction == SortDirection.Descending)
            {
                from = 23; to = 0; step = -1;
            }

            if (Micajah.Common.Security.UserContext.Current.TimeFormat == 0) //AM/PM time format
            {
                for (int i = from; (direction == SortDirection.Ascending && i <= to) || (direction == SortDirection.Descending && i >= to); i += step)
                    if (i < 12)
                        ListItems.Add(new ListItem(i.ToString() + "am", i.ToString()));
                    else if (i > 12)
                        ListItems.Add(new ListItem((i - 12).ToString() + "pm", i.ToString()));
                    else
                        ListItems.Add(new ListItem(i.ToString() + "noon", i.ToString()));

                ListItems.FindByValue("0").Text = "12am";
            }
            else
                for (int i = from; (direction == SortDirection.Ascending && i <= to) || (direction == SortDirection.Descending && i >= to); i += step) ListItems.Add(new ListItem(FormatTimePart(i), i.ToString()));
        }

        public static void CreateMinutesList(ListItemCollection ListItems, int Interval)
        {
            CreateMinutesList(ListItems, Interval, SortDirection.Ascending);
        }

        public static void CreateMinutesList(ListItemCollection ListItems, int Interval, SortDirection direction)
        {
            int from = 0, to = 59, step = Interval;
            if (direction == SortDirection.Descending)
            {
                from = 59; to = 0; step = -Interval;
            }

            for (int i = from; (direction == SortDirection.Ascending && i <= to) || (direction == SortDirection.Descending && i >= to); i += step)
                ListItems.Add(new ListItem(FormatTimePart(i), i.ToString()));
        }

        public static string DisplayDateDurationHtml(int TotalMinutes, int BusinessDayLength)
        {
            string _result = string.Empty;

            if (BusinessDayLength <= 0)
                return _result;

            int days = TotalMinutes / BusinessDayLength;
            int hours = (TotalMinutes % BusinessDayLength) / 60;
            int minutes = (TotalMinutes % BusinessDayLength) % 60;

            if (days != 0) _result += "<b>" + days.ToString() + "</b>" + "d";
            if (hours != 0 || days != 0)
            {
                if (_result.Length > 0) _result += "&nbsp;";
                _result += "<b>" + ((days < 0) ? Math.Abs(hours) : hours).ToString() + "</b>h";
            }

            if (_result.Length > 0) _result += "&nbsp;";

            _result += "<b>" + (((days < 0) || hours < 0) ? Math.Abs(minutes) : minutes).ToString() + "</b>m";

            return _result;
        }

        public static string DisplayDateDuration(int TotalMinutes)
        {
            return DisplayDateDuration(TotalMinutes, 0);
        }

        public static string DisplayDateDuration(long TotalMinutes, int BusinessDayLength)
        {
            if (BusinessDayLength == 0) return DisplayDateDuration(TimeSpan.FromMinutes(Convert.ToDouble(TotalMinutes)));

            string _result = string.Empty;

            if (BusinessDayLength <= 0)
                return _result;

            long days = TotalMinutes / BusinessDayLength;
            long hours = (TotalMinutes % BusinessDayLength) / 60;
            long minutes = (TotalMinutes % BusinessDayLength) % 60;

            if (days != 0) _result += days.ToString() + "d";
            if (hours != 0 || days != 0)
            {
                if (_result.Length > 0) _result += " ";
                _result += hours.ToString() + "h";
            }

            if (_result.Length > 0) _result += " ";

            _result += minutes.ToString() + "m";

            return _result;
        }

        public static string DisplayDateDuration(TimeSpan tspan)
        {
            return DisplayDateDuration(tspan, false);
        }

        private static string GetLargerAttribute(string Text, bool HighLight)
        {
            if (HighLight) return "<span style=\"font-size:11pt;\">" + Text + "</span>";
            else return Text;
        }

        public static string DisplayDateDuration(TimeSpan tspan, bool HighLight)
        {
            string _result = string.Empty;

            if (tspan.Days != 0) _result += GetLargerAttribute(tspan.Days.ToString(), HighLight) + "d";//tspan.Days.ToString() + "d";
            if (tspan.Hours != 0 || tspan.Days != 0)
            {
                if (_result.Length > 0) _result += " ";
                _result += GetLargerAttribute(tspan.Hours.ToString(), HighLight) + "h";//tspan.Hours.ToString() + "h";
            }
            if (_result.Length > 0) _result += " ";
            _result += GetLargerAttribute(tspan.Minutes.ToString(), HighLight) + "m"; //tspan.Minutes.ToString() + "m";
            return _result;
        }

        public static string FormatSQLDateTime(DateTime date)
        {
            //Universal SQL format - yyyy/mm/dd hh:nn
            return date.Year.ToString() + (date.Month < 10 ? "0" + date.Month.ToString() : date.Month.ToString()) + (date.Day < 10 ? "0" + date.Day.ToString() : date.Day.ToString()) + " " + date.Hour.ToString() + ":" + date.Minute.ToString();
        }

        public static DateTime User2DBDateTime(DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc)
            {
                return date;
            }
            return TimeZoneInfo.ConvertTimeToUtc(date, UserContext.Current.TimeZone);
        }

        public static DateTime DB2UserDateTime(DateTime date)
        {
            return DB2UserDateTime(date, UserContext.Current.TimeZone);
        }

        public static DateTime DB2UserDateTime(DateTime date, TimeZoneInfo timezone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(date, timezone);
        }

        public static string FormatSQLShortDateTime(DateTime date)
        {
            //Universal SQL format - yyyy/mm/dd hh:nn
            return date.Year.ToString() + "/" + date.Month.ToString() + "/" + date.Day.ToString();
        }

        public static string SqlStr(object RowCol)
        {
            if (string.IsNullOrEmpty(RowCol.ToString())) return "NULL";
            else return "'" + RowCol.ToString().Replace("'", "''") + "'";
        }

        public static string SqlBit(object RowCol)
        {
            if (string.IsNullOrEmpty(RowCol.ToString())) return "NULL";
            else
            {
                bool _boolValue = false;
                if (!bool.TryParse(RowCol.ToString(), out _boolValue)) return "0";
                if (_boolValue) return "1";
                else return "0";
            }
        }

        public static string DisplayDate(DateTime date, int hoursOffset, bool omitUTC)
        {
            return string.Format(GetDisplayDateFormat(omitUTC), date.AddHours(hoursOffset));
        }

        public static string DisplayDate(DateTime date, bool omitUTC)
        {
            return string.Format(GetDisplayDateFormat(omitUTC), DB2UserDateTime(date));
        }

        public static string DisplayDate(DateTime date, TimeZoneInfo timezone, bool omitUTC)
        {
            return string.Format(GetDisplayDateFormat(timezone.BaseUtcOffset.Hours, omitUTC), DB2UserDateTime(date, timezone));
        }

        public static string GetDisplayDateFormat(bool OmitUTC)
        {
            return GetDisplayDateFormat(UserContext.Current.TimeZone.BaseUtcOffset.Hours, OmitUTC);
        }

        public static string GetDisplayDateFormat(int UTCOffset, bool OmitUTC)
        {
            string sResult = @"{0:d-MMM-yyyy}";

            if (OmitUTC) sResult += " (UTC" + Convert.ToString(UTCOffset) + ")";

            return sResult;
        }

        public static string GetDisplayDateTimeFormat(bool OmitUTC)
        {
            return GetDisplayDateTimeFormat(UserContext.Current.TimeFormat,
                                            UserContext.Current.TimeZone.BaseUtcOffset.Hours, OmitUTC);
        }

        public static string GetDisplayDateTimeFormat(int timeformat, int utcoffset, bool OmitUTC)
        {
            string sResult = @"{0:d-MMM-yyyy";

            if (timeformat == 0) sResult += " hh:mm tt}";
            else sResult += " HH:mm}";
            if (OmitUTC) sResult += " (UTC" + Convert.ToString(utcoffset) + ")";

            return sResult;
        }

        public static System.Globalization.CultureInfo GetCultureFromDateFormat()
        {
            return GetCultureFromDateFormat(UserContext.Current.TimeFormat, UserContext.Current.DateFormat);
        }

        public static System.Globalization.CultureInfo GetCultureFromDateFormat(int timeformat, int dateformat)
        {
            System.Globalization.CultureInfo _ci = new System.Globalization.CultureInfo("en-US", false);

            System.Globalization.DateTimeFormatInfo _dtfi = _ci.DateTimeFormat;

            if (timeformat == 0) _dtfi.ShortTimePattern = "hh:mm";
            else _dtfi.ShortTimePattern = "HH:mm";
            if (dateformat == 0) _dtfi.ShortDatePattern = "MM/dd/yyyy";
            else _dtfi.ShortDatePattern = "dd/MM/yyyy";
            _dtfi.LongDatePattern = "d-MMM-yyyy";
            return _ci;
        }

        public static DateTime ParseDateTime(string datetime, int hoursOffset)
        {
            return DateTime.Parse(datetime, GetCultureFromDateFormat().DateTimeFormat).AddHours(hoursOffset);
        }

        public static DateTime ParseDateTime(string datetime)
        {
            return User2DBDateTime(DateTime.Parse(datetime, GetCultureFromDateFormat().DateTimeFormat));
        }

        public static string DisplayTime(DateTime date, int hoursOffset, bool omitUTC)
        {
            if (UserContext.Current != null && UserContext.Current.TimeZone != null && UserContext.Current.TimeZone.IsDaylightSavingTime(date))
                hoursOffset++;
            return DisplayTimeFormat(date.AddHours(hoursOffset), omitUTC);
        }

        public static string DisplayTime(DateTime date, bool omitUTC)
        {
            return DisplayTimeFormat(DB2UserDateTime(date), omitUTC);
        }

        private static string DisplayTimeFormat(DateTime date, bool omitUTC)
        {
            string sResult = string.Empty;
            //12:00AM/PM
            if (UserContext.Current.TimeFormat == 0)
            {
                if (date.Hour > 12) sResult = FormatTimePart(date.Hour - 12) + ":" + FormatTimePart(date.Minute) + "PM";
                else if (date.Hour == 12) sResult = FormatTimePart(date.Hour) + ":" + FormatTimePart(date.Minute) + "PM";
                else if ((date.Hour < 12) && (date.Hour != 0)) sResult = FormatTimePart(date.Hour) + ":" + FormatTimePart(date.Minute) + "AM";
                else if (date.Hour == 0) sResult = "12:" + FormatTimePart(date.Minute) + "AM";
            }
            //24:00 (UTC)
            else sResult = FormatTimePart(date.Hour) + ":" + FormatTimePart(date.Minute);
            if (omitUTC) sResult += " (UTC" + Convert.ToString(UserContext.Current.TimeZone.BaseUtcOffset.Hours) + ")";
            return sResult;
        }

        public static string DisplayDateTime(DateTime date, int hoursOffset, bool OmitUTC)
        {
            if (UserContext.Current != null && UserContext.Current.TimeZone != null && UserContext.Current.TimeZone.IsDaylightSavingTime(date))
                hoursOffset++;
            return string.Format(GetDisplayDateTimeFormat(OmitUTC), date.AddHours(hoursOffset));
        }

        public static string DisplayDateTime(DateTime date, bool OmitUTC)
        {
            return string.Format(GetDisplayDateTimeFormat(OmitUTC), DB2UserDateTime(date));
        }

        public static string DisplayDateTime(DateTime date, TimeZoneInfo timezone, int timeformat, bool omitutc)
        {
            return string.Format(GetDisplayDateTimeFormat(timeformat, timezone.BaseUtcOffset.Hours, omitutc), DB2UserDateTime(date, timezone));
        }

        public static string DisplayDateMask()
        {
            if (UserContext.Current.DateFormat == 0) return "MM/dd/yyyy";
            return "dd/MM/yyyy";
        }

        public static string DisplayTimeMask()
        {
            //12:00AM/PM
            if (UserContext.Current.TimeFormat == 0) return "hh:mm";
            //24:00 (UTC)
            return "HH:mm";
        }

        public static string DisplayDateTimeMask()
        {
            return DisplayDateMask() + " " + DisplayTimeMask();
        }

        public static bool IsReqAfterHoursAlert(int DeptID, Config DeptConfig, DateTime CheckDateTime, string _wDays)
        {
            CheckDateTime = DB2UserDateTime(CheckDateTime);
            DateTime _timeStart = new DateTime(CheckDateTime.Year, CheckDateTime.Month, CheckDateTime.Day, DeptConfig.BusHourStart, DeptConfig.BusMinStart, 0);
            DateTime _timeStop = new DateTime(CheckDateTime.Year, CheckDateTime.Month, CheckDateTime.Day, DeptConfig.BusHourStop, DeptConfig.BusMinStop, 59);
            if (CheckDateTime < _timeStart || CheckDateTime > _timeStop) return true;
            if (_wDays.Length > 6)
            {
                if ((int)CheckDateTime.DayOfWeek > 0)
                {
                    if (_wDays.Substring((int)CheckDateTime.DayOfWeek - 1, 1) == "0") return true;
                }
                else
                {
                    if (_wDays.Substring(6, 1) == "0") return true;
                }
            }
            System.Data.DataTable _hdays = Data.Companies.SelectHolidays(DeptID, CheckDateTime.Year);
            foreach (System.Data.DataRow _hday in _hdays.Rows)
            {
                if (CheckDateTime >= (DateTime)_hday["dtStart"] && CheckDateTime <= (DateTime)_hday["dtStop"]) return true;
            }
            return false;
        }

        public static int SendEmail(string FromEmail, string FromName, string ToEmail, string ToName, string CC, string Subject, string Body, Data.FileItem[] Files)
        {
            return SendEmail(FromEmail, FromName, ToEmail, ToName, CC, Subject, Body, Files, true);
        }

        public static int SendEmail(string FromEmail, string FromName, string ToEmail, string ToName, string CC, string Subject, string Body, Data.FileItem[] Files, bool IsHtmlEmail)
        {
            try
            {
                string SMTPServer = System.Configuration.ConfigurationManager.AppSettings["SMTPServer"];
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient(SMTPServer);
                smtp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;

                System.Net.Mail.MailMessage mess = new System.Net.Mail.MailMessage();
                mess.From = new System.Net.Mail.MailAddress(FromEmail, FromName);
                if (ToEmail != null && ToEmail.Length > 0)
                {
                    string[] _toArr = ToEmail.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string _to in _toArr) mess.To.Add(new System.Net.Mail.MailAddress(_to.Trim(), ToName));
                }
                mess.IsBodyHtml = IsHtmlEmail;
                mess.Subject = Subject;
                mess.Body = Body;
                if (CC != null && CC.Length > 0)
                {
                    string[] _ccArr = CC.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string _cc in _ccArr) mess.CC.Add(_cc);
                }
                //              mess.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", 2);
                if (Files != null)
                {
                    foreach (Data.FileItem _file in Files)
                    {
                        System.Net.Mail.Attachment _data = new System.Net.Mail.Attachment(new System.IO.MemoryStream(_file.Data, true), _file.Name);
                        _data.ContentDisposition.CreationDate = _file.Updated;
                        mess.Attachments.Add(_data);
                    }
                }
                smtp.Send(mess);
                return 0;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string GetWeekdayName(int weekDay)
        {
            switch (weekDay)
            {
                case 0:
                    return "Monday";
                case 1:
                    return "Tuesday";
                case 2:
                    return "Wednesday";
                case 3:
                    return "Thursday";
                case 4:
                    return "Friday";
                case 5:
                    return "Saturday";
                case 6:
                    return "Sunday";
            }
            return "";
        }

        public static int GetWeekdayIndex(DateTime inDate)
        {
            switch (inDate.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 2;
                case DayOfWeek.Tuesday:
                    return 3;
                case DayOfWeek.Wednesday:
                    return 4;
                case DayOfWeek.Thursday:
                    return 5;
                case DayOfWeek.Friday:
                    return 6;
                case DayOfWeek.Saturday:
                    return 7;
                case DayOfWeek.Sunday:
                    return 1;
            }
            return -1;
        }

        public static string GetMonthName(int Month)
        {
            switch (Month)
            {
                case 1:
                    return "January";
                case 2:
                    return "February";
                case 3:
                    return "March";
                case 4:
                    return "April";
                case 5:
                    return "May";
                case 6:
                    return "June";
                case 7:
                    return "July";
                case 8:
                    return "August";
                case 9:
                    return "September";
                case 10:
                    return "October";
                case 11:
                    return "November";
                case 12:
                    return "December";
                default:
                    return String.Empty;
            }
        }

        public static string GetShortMonthName(int Month)
        {
            switch (Month)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sep";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                default:
                    return String.Empty;
            }
        }

        public static bool ParseHours(string HoursInput, out decimal dHours)
        {
            dHours = 0;
            if ((HoursInput.IndexOf('.') != HoursInput.LastIndexOf('.')) || (HoursInput.IndexOf(':') != HoursInput.LastIndexOf(':')) || (HoursInput.Contains(":") && HoursInput.Contains("."))) return false;
            if (HoursInput.Contains(".")) return decimal.TryParse(HoursInput, out dHours);
            string sHours = String.Empty;
            if (HoursInput.Contains(":")) sHours = (HoursInput.Split(':'))[0];
            else sHours = HoursInput;
            if (String.IsNullOrEmpty(sHours)) sHours = "0";
            int iHours = 0;
            if (!int.TryParse(sHours, out iHours)) return false;
            string sMinutes = String.Empty;
            int iMinutes = 0;
            if (HoursInput.Contains(":"))
            {
                sMinutes = (HoursInput.Split(':'))[1];
                if (String.IsNullOrEmpty(sMinutes)) sMinutes = "0";
                if (!int.TryParse(sMinutes, out iMinutes)) return false;
            }
            if ((iMinutes > 59) || (iMinutes < 0)) return false;
            dHours = ((decimal)iHours) + Math.Truncate((decimal)(((iMinutes / 60) * 100) / 100));
            return true;
        }

        public static string RemoveHTML(string in_HTML)
        {
            string _CrLf = Environment.NewLine;
            in_HTML = Regex.Replace(in_HTML, "<br>|<BR>|<br/>|<BR/>|<br />|<BR />|&lt;br&gt;|&lt;BR&gt;|&lt;br/&gt;|&lt;BR/&gt;|&lt;br /&gt;|&lt;BR /&gt;", _CrLf);
            in_HTML = Regex.Replace(in_HTML, "<head>(.|\n)*?</head>|<style(.|\n)*?>(.|\n)*?</style>|<script(.|\n)*?>(.|\n)*?</script>|<(.|\n)*?>", string.Empty);
            in_HTML = Regex.Replace(in_HTML, "&lt;(.|\n)*?&gt;", string.Empty);
            in_HTML = Regex.Replace(in_HTML, @"([\r\n]\s*){3,}", _CrLf + _CrLf);
            in_HTML = Regex.Replace(in_HTML, @"([\n]\s*){3,}", _CrLf + _CrLf);
            in_HTML = in_HTML.Trim(_CrLf.ToCharArray());
            return in_HTML;
        }

        public static DateTime ParseTimeToDate(DateTime SelectedDate, string sTime, string TimePart)
        {
            if ((sTime.IndexOf('.') != sTime.LastIndexOf('.')) || (sTime.IndexOf(':') != sTime.LastIndexOf(':')) || (sTime.Contains(":") && sTime.Contains("."))) return DateTime.MinValue;
            string sHours = String.Empty;
            string HourString = sTime.Replace(".", ":");
            if (HourString.Contains(":")) sHours = (HourString.Split(':'))[0];
            else sHours = HourString;
            if (String.IsNullOrEmpty(sHours)) sHours = "0";
            int iHours = 0;
            if (!int.TryParse(sHours, out iHours)) return DateTime.MinValue;
            if ((iHours > 23) || (iHours < 0)) return DateTime.MinValue;
            if (UserContext.Current.TimeFormat == 0 && (iHours > 12))
            {
                iHours -= 12;
                TimePart = "pm";
            }
            string sMinutes = String.Empty;
            int iMinutes = 0;
            string preMinutes = String.Empty;
            if (sTime.Contains("."))
            {
                preMinutes = (sTime.Split('.'))[1];
                if (String.IsNullOrEmpty(preMinutes)) preMinutes = "0";
                sMinutes = "0." + preMinutes;
                double dMinutes = 0;
                if (!double.TryParse(sMinutes, out dMinutes)) return DateTime.MinValue;
                iMinutes = (int)Math.Truncate(dMinutes * 60);
            }
            else if (sTime.Contains(":"))
            {
                preMinutes = (sTime.Split(':'))[1];
                if (String.IsNullOrEmpty(preMinutes)) preMinutes = "0";
                sMinutes = preMinutes;
                if (!int.TryParse(sMinutes, out iMinutes)) return DateTime.MinValue;
            }
            if ((iMinutes > 59) || (iMinutes < 0)) return DateTime.MinValue;
            int Hours = 0;
            int Minutes = iMinutes;
            if (UserContext.Current.TimeFormat == 0)
            {
                if (TimePart == "am") Hours = iHours == 12 ? 0 : iHours;
                else Hours = iHours == 12 ? iHours : (iHours + 12);
            }
            else Hours = iHours;
            return new DateTime(SelectedDate.Year, SelectedDate.Month, SelectedDate.Day, Hours, Minutes, 0);
        }

        public static DateTime GetDatePickerValue(string sDate, CheckDateRange checkFunction)
        {
            if (!checkFunction(sDate, Functions.DisplayDateMask())) return DateTime.MinValue;
            if (String.IsNullOrEmpty(sDate)) return DateTime.MinValue;
            else return Functions.ParseDateTime(sDate);
        }



        public static DateTime ParseDate(string datestring, string dateformat)
        {
            if (datestring.Length == 0) return DateTime.MinValue;
            System.Globalization.DateTimeFormatInfo _dtfi = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
            _dtfi.ShortDatePattern = dateformat;
            DateTime _date = DateTime.Parse(datestring, _dtfi);
            return _date;
        }

        static protected int _maxYear = 2075;

        public static bool IsValidDate(string datestring, string dateformat)
        {
            try
            {
                DateTime _date = ParseDate(datestring, dateformat);

                if (_date.Year > _maxYear)
                {
                    return false;
                };

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidEmail(string strIn)
        {
            // Return true if strIn is in valid e-mail format.
            return System.Text.RegularExpressions.Regex.IsMatch(strIn, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
        }

        public static string CreateMD5hash(string data)
        {
            string _result = string.Empty;

            MD5 _md5 = new MD5CryptoServiceProvider();

            if (_md5 != null)
            {
                System.Text.ASCIIEncoding _encoding = new System.Text.ASCIIEncoding();
                if (_encoding != null)
                {
                    byte[] _byte_array = _encoding.GetBytes(data);

                    byte[] _result_byte_array = _md5.ComputeHash(_byte_array);
                    if (_result_byte_array != null)
                    {
                        _result = ToHexString(_result_byte_array);
                        _result = _result.ToLower();
                    }
                }
            }

            return _result;
        }

        public static string ToHexString(byte[] bytes)
        {
            char[] hexDigits = {
                                '0', '1', '2', '3', '4', '5', '6', '7',
                                '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

            char[] chars = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                int b = bytes[i];
                chars[i * 2] = hexDigits[b >> 4];
                chars[i * 2 + 1] = hexDigits[b & 0xF];
            }
            return new string(chars);
        }

        public static bool IsIE9(System.Web.HttpBrowserCapabilities browser)
        {
            // Returns the version of Internet Explorer or a -1
            // (indicating the use of another browser).
            float rv = -1;
            if (browser.Browser == "IE")
                rv = (float)(browser.MajorVersion + browser.MinorVersion);
            return rv >= 9.0;
        }

        public static string GetDeptWebAddress(Data.Companies.Department dept, string UserEmail, string UserPassword, bool IsForTech)
        {
            string _server = System.Web.HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            _server = _server.ToLower();
            if (_server.IndexOf("http://") < 0 && _server.IndexOf("https://") < 0) _server = "https://" + _server;
            if (_server.LastIndexOf("/") < (_server.Length - 1)) _server += "/";

            string _url = string.Empty;
            bool _isUseHash = false;
            if (dept.Config.AllowUserLoginWithoutPassword)
            {
                _url = _server;
                _isUseHash = true;
            }
            else if (dept.Config.EnableLDAPGlobal && dept.Config.EnableLDAP && dept.Config.EnableLDAPEmailExtUrl && dept.Config.LDAPLocalUrl.Length > 0)
            {
                _url = dept.Config.LDAPLocalUrl.ToLower();
                if (_url.IndexOf("http://") < 0 && _url.IndexOf("https://") < 0) _url = "https://" + _url;
                if (_url.LastIndexOf("/") < (_url.Length - 1)) _url += "/";
            }
            else _url = _server;
            _url += "?dept=" + dept.PseudoID + "&org=" + dept.OrganizationPseudoId + (_isUseHash && !IsForTech ? "&login=" + System.Web.HttpUtility.UrlEncode(UserEmail) + "&hash=" + Functions.CreateMD5hash(UserPassword.Length > 0 ? UserPassword : UserEmail) : string.Empty);
            return _url;
        }
    }
}
