using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Folder.
    /// </summary>
    public class CustomEmails : DBAccess
    {
        public enum FieldType
        {
            Custom = 0,
            AccountLocation = 1,
            Class = 2,
            AltPhone = 3,
            Email = 4,
            FullName = 5,
            MobileEmail = 6,
            MobilePhone = 7,
            Pager = 8,
            Phone = 9,
            Title = 10,
            Level = 11,
            Priority = 12,
            ServiceRep = 13,
            Subject = 14,
            TicketNumber = 15,
            InternalLocation = 16,
            CustomTicketField = 17,
            DepartmentName=18,
            RequestCompletionDate=19,
            ExpectedResponseDate=20,
            ExpectedCompletionDate=21,
            FollowUpDate = 22,
            Project = 23,
            TotalTime = 24,
            RemainingTime = 25,
            NextStep = 26,
            RelatedTktCount = 27,
            EstimatedTime = 28
        }
        public enum CustomEmailTextType
        {
            NewlyCreatedTickets,
            EmailTktCreationSuccess,
            EmailTktCreationFailure,
            EmailNonMatchDomainSuffix,
            AfterHoursAlertToNewTicketsViaEmail,
            EmailTktClosureMessage,
            TktCloseEmail,
            MessagesSent,
            ForgotPasswordEmailSubject,
            ForgotPasswordTextTop,
            ForgotPasswordTextBottom
        }

        public static DataTable GetChkMailOptions(int departmentId)
        {
            return SelectByQuery("SELECT dbo.fxGetConfigValueBit(" + departmentId.ToString() + ", 'btMailHyperLinksStatus') as btMailHyperLinksStatus, dbo.fxGetConfigValueBit(" + departmentId.ToString() + ", 'btCfgLWP') as btCfgLWP; ");                        
        }

        public static void SetChkMailOptions(int departmentId, bool status, bool pwdStatus)
        {
            UpdateByQuery("UPDATE tbl_company SET btMailHyperLinksStatus=" + (status ? "1" : "0") + ", btCfgLWP=" + (pwdStatus ? "1" : "0") + " WHERE company_id=" + departmentId.ToString());
        }

        public static List<CustomEmailFieldItem> SelectAllCustomEmailFieldItems(UserAuth currentUser, int departmentId)
        {
            List<CustomEmailFieldItem> allCustomEmailFieldItems = new List<CustomEmailFieldItem>();
            DataTable dtCustomEmailFieldItems = SelectByQuery("SELECT FieldType, IsChecked, CustomFieldId FROM CustomEmailFields WHERE DepartmentId=" + departmentId.ToString());
            if (dtCustomEmailFieldItems != null)
                foreach (DataRow drCustomEmailFieldItem in dtCustomEmailFieldItems.Rows)
                    allCustomEmailFieldItems.Add(new CustomEmailFieldItem(currentUser, (FieldType)((int)drCustomEmailFieldItem["FieldType"]), 0, (bool)drCustomEmailFieldItem["IsChecked"], drCustomEmailFieldItem.IsNull("CustomFieldId") ? 0 : (int)drCustomEmailFieldItem["CustomFieldId"]));
            return allCustomEmailFieldItems;
        }

        public static void UpdateCustomEmailFieldItems(int departmentId, List<CustomEmailFieldItem> customEmailFieldItems)
        {
            ClearCustomEmailFieldItems(departmentId);
            foreach (CustomEmailFieldItem customEmailFieldItem in customEmailFieldItems)
                if (customEmailFieldItem.Field != FieldType.Custom)
                    InsertCustomEmailFieldItem(departmentId, customEmailFieldItem);
        }

        static void InsertCustomEmailFieldItem(int departmentId, CustomEmailFieldItem customEmailFieldItem)
        {
            UpdateByQuery("INSERT INTO CustomEmailFields(DepartmentId,FieldType, IsChecked, CustomFieldId) VALUES(" + departmentId + "," + (int)customEmailFieldItem.Field + "," + (customEmailFieldItem.IsChecked ? "1" : "0") + "," + (customEmailFieldItem.CustomFieldValue>0?customEmailFieldItem.CustomFieldValue.ToString():"NULL") + ")");
        }

        static void ClearCustomEmailFieldItems(int departmentId)
        {
            UpdateByQuery("DELETE FROM CustomEmailFields WHERE DepartmentId=" + departmentId.ToString());
        }

        public static string GetCustomEmailText(int departmentId, CustomEmailTextType textType)
        {
            return CustomTexts.GetCustomText(departmentId, textType.ToString());
        }
    }

    public class CustomEmailFieldItem
    {
        Data.CustomEmails.FieldType field;

        public Data.CustomEmails.FieldType Field
        {
            get { return field; }
        }

        int outdent = 0;
        string text = string.Empty;
        string uniqueId = string.Empty;
        bool isChecked = false;
        bool defaultValue = false;
        int customFieldValue = 0;

        public int CustomFieldValue
        {
            get { return customFieldValue; }
            set { customFieldValue = value; }
        }

        public int Outdent
        {
            get { return outdent; }
        }

        public string Text
        {
            get { return text; }
        }


        public string UniqueId
        {
            get { return uniqueId; }
        }


        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        public bool DefaultValue
        {
            get { return defaultValue; }
        }

        public CustomEmailFieldItem(string text, string uniqueId)
        {
            field = bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Custom;
            this.text = text;
            this.uniqueId = uniqueId;
        }
        public CustomEmailFieldItem(string text, string uniqueId, int outdent)
            : this(text, uniqueId)
        {
            this.outdent = outdent;
        }

        public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field)
        {
            this.field = field;

            if (field!=Data.CustomEmails.FieldType.CustomTicketField) this.defaultValue = true;

            switch (field)
            {
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.DepartmentName:
                    this.text = "Department Name";
                    break;
                case Data.CustomEmails.FieldType.AccountLocation:
                    this.text = (user == null ? "Account" : user.customNames.Account.FullSingular) + " " + (user == null ? "Location " : user.customNames.Location.FullSingular);
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.AltPhone:
                    this.text = "Alt Phone";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Class:
                    this.text = "Class";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Email:
                    this.text = "Email";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.FullName:
                    this.text = "Full Name";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Level:
                    this.text = "Level";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.MobileEmail:
                    this.text = "Mobile Email";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.MobilePhone:
                    this.text = "Mobile Phone";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Pager:
                    this.text = "Pager";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Phone:
                    this.text = "Phone";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Priority:
                    this.text = "Priority";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.ExpectedCompletionDate:
                    this.text = "Expected Completion Date";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.ExpectedResponseDate:
                    this.text = "Expected Response Date";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.RequestCompletionDate:
                    this.text = "Scheduled Date";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.FollowUpDate:
                    this.text = "Follow-Up Date";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.ServiceRep:
                    this.text = user == null ? "Technician" : user.customNames.Technician.FullSingular;
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Subject:
                    this.text = "Subject";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.NextStep:
                    this.text = "Next Step";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.TicketNumber:
                    this.text = (user == null ? "Ticket" : user.customNames.Ticket.FullSingular) + " #";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Title:
                    this.text = "Title";
                    break;
                case Data.CustomEmails.FieldType.InternalLocation:
                    this.text = "Internal " + (user == null ? "Location " : user.customNames.Location.FullSingular);
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.Project:
                    this.text = "Project";                    
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.TotalTime:
                    this.text = "Total Time";                    
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.RemainingTime:
                    this.text = "Remaining Time";                    
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.EstimatedTime:
                    this.text = "Estimated Time";
                    break;
                case bigWebApps.bigWebDesk.Data.CustomEmails.FieldType.RelatedTktCount:
                    this.text = "Related "+(user == null ? "Tickets" : user.customNames.Ticket.FullPlural) + " Count";
                    break;
            }
        }
        public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field, int outdent)
            : this(user, field)
        {
            this.outdent = outdent;
        }

        public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field, int outdent, int customFieldValue)
            : this(user, field, outdent)
        {
            this.customFieldValue = customFieldValue;
        }
public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field, int outdent, int customFieldValue, string text)
    : this(user, field, outdent, customFieldValue)
        {
            this.text = text;
        }

        public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field, int outdent, bool isChecked)
            : this(user, field, outdent)
        {
            this.isChecked = isChecked;
        }

        public CustomEmailFieldItem(UserAuth user, Data.CustomEmails.FieldType field, int outdent, bool isChecked, int customFieldValue)
            : this(user, field, outdent, isChecked)
        {
            this.customFieldValue = customFieldValue;
        }

    }


}
