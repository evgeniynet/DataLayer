using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.IO;
using System.Net.Mail;
using System.Xml.Serialization;

namespace bigWebApps.bigWebDesk.Data
{
	[Serializable]
	public class MailNotification
	{
		public enum UseSendMailEngine
		{
			SystemWebMail = 0,
			NotificationService = 1
		}

		private Guid _org_id = Guid.Empty;
		private int _department_id = -1;
		private int _user_id = -1;

		private string _from = string.Empty;
		private string _to = string.Empty;
		private string _subject = string.Empty;
		private string _body = string.Empty;
		private FileItem[] _files=new FileItem[0];
				
		//internal

		//Constructors
		public MailNotification()
		{
		}

		public MailNotification(Guid OrgID, int x_department_id, int x_user_id, string x_from, string x_to, string x_subject, string x_body)
		{
			_org_id = OrgID;
			_department_id = x_department_id;
			_user_id = x_user_id;

			_from = x_from;
			_to = x_to;
			_subject = x_subject;
			_body = x_body;
		}

		public MailNotification(int x_department_id, int x_user_id, string x_from, string x_to, string x_subject, string x_body) : this (Guid.Empty, x_department_id, x_user_id, x_from, x_to, x_subject, x_body)
		{
		}

		//Properties
		public int DepartmentId
		{
			set { _department_id = value; }
			get { return _department_id; }
		}

		public int UserId
		{
			set { _user_id = value; }
			get { return _user_id; }
		}

		public string From
		{
			set { _from = value; }
			get { return _from; }
		}

		public string To
		{
			set { _to = value; }
			get { return _to; }
		}

		public string Subject
		{
			set { _subject = value; }
			get { return _subject; }
		}

		public string Body
		{
			set { _body = value; }
			get { return _body; }
		}

		public FileItem[] AttachedFiles
		{
			set { _files = value; }
			get { return _files; }
		}

		public string Commit(UseSendMailEngine mail_engine, bool IsBodyHtml)
		{
			string _result = string.Empty;

			try
			{
				switch (mail_engine)
				{
					case UseSendMailEngine.SystemWebMail:
						Functions.SendEmail(_from, string.Empty, _to, string.Empty, null, _subject, _body, _files, IsBodyHtml);
						break;
					case UseSendMailEngine.NotificationService:
						XmlSerializer _serializer = new XmlSerializer(typeof(MailNotification));
						TextWriter _stream = new StringWriter();
						_serializer.Serialize(_stream, this);
						string _object_state = _stream.ToString();
						_stream.Close();
						NotificationRules.TicketEvent _event = IsBodyHtml ? NotificationRules.TicketEvent.DirectMailAsHTML : NotificationRules.TicketEvent.DirectMail;
						int _operation_result = NotificationEventsQueue.InsertEvent(_org_id, _department_id, _user_id, _event, _object_state, _files);
						if (_operation_result <= 0)
							_result = "Can't add mass mailes to NotificationEventsQueue.";

						break;
				};
			}
			catch (Exception ex)
			{
				_result = ex.Message;
			};

			return _result;
		}

	}

}
