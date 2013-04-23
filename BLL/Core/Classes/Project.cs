using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace bigWebApps.bigWebDesk.Data
{
	/// <summary>
	/// Summary description for CreationCategories.
	/// </summary>
	public class Project: DBAccess
	{
		public enum ReportType
		{
			ByTechs = 0,
			ByAccounts
		}

		public enum GroupByType
		{
			ByTechs = 1,
			ByAccounts,
			ByAccountProjects,
			ByTasks,
            ByTickets,
            ByDay
		}

        public static int SelectProjectsCount(Guid OrgId, Guid InstId)
        {
            int DId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM Project WHERE CompanyID=" + DId.ToString() + " AND Active=1", OrgId);
            return (int)_dt.Rows[0][0];
        }

		public static DataRow Select(int companyID, int projectID)
		{
			return SelectRecord("sp_SelectProject", new SqlParameter[] { new SqlParameter("@CompanyID", companyID),
			new SqlParameter("@ProjectID", projectID)});
		}

		public static void Update(int projectID, int companyID, int accountID, int parentID,
            string name, string description, int internalPMID, int clientPMID, int active, int estimatedHours,
            decimal estimatedCost, decimal estimatedInvoicedAmount, string[] CustFields, DateTime[] CustDates, int supGroupId,
            int priorityId)
		{
            System.Collections.Generic.List<SqlParameter> p = new System.Collections.Generic.List<SqlParameter>();
			p.Add(new SqlParameter("@ProjectID", projectID));
			p.Add(new SqlParameter("@CompanyID", companyID));
			p.Add(new SqlParameter("@AccountID", accountID));
            SqlParameter spParentID = new SqlParameter("@ParentID", SqlDbType.Int);
			if (parentID != 0)
			{
                spParentID.Value = parentID;
			}
			else
			{
                spParentID.Value = DBNull.Value;
			}
            p.Add(spParentID);
			p.Add(new SqlParameter("@Name", name));
			p.Add(new SqlParameter("@Description", description));
            SqlParameter spInternalPMID = new SqlParameter("@InternalPMID", SqlDbType.Int);
			if (internalPMID != 0)
			{
                spInternalPMID.Value = internalPMID;
			}
			else
			{
                spInternalPMID.Value = DBNull.Value;
			}
            p.Add(spInternalPMID);
            SqlParameter spClientPMID = new SqlParameter("@ClientPMID", SqlDbType.Int);
			if (clientPMID != 0)
			{
				spClientPMID.Value = clientPMID;
			}
			else
			{
				spClientPMID.Value = DBNull.Value;
			}
            p.Add(spClientPMID);
            SqlParameter spActive = new SqlParameter("@Active", SqlDbType.Bit);
            if (active < 0)
            {
                spActive.Value = DBNull.Value;
            }
            else
            {
                spActive.Value = active;
            }
            p.Add(spActive);
            SqlParameter spEstimatedHours = new SqlParameter("@EstimatedHours", SqlDbType.Int);
            if (estimatedHours != 0)
            {
                spEstimatedHours.Value = estimatedHours;
            }
            else
            {
                spEstimatedHours.Value = DBNull.Value;
            }
            p.Add(spEstimatedHours);
            SqlParameter spEstimatedCost = new SqlParameter("@EstimatedCost", SqlDbType.Decimal);
            if (estimatedCost != 0)
            {
                spEstimatedCost.Value = estimatedCost;
            }
            else
            {
                spEstimatedCost.Value = DBNull.Value;
            }
            p.Add(spEstimatedCost);
            SqlParameter spEstimatedInvoicedAmount = new SqlParameter("@EstimatedInvoicedAmount", SqlDbType.Decimal);
            if (estimatedInvoicedAmount != 0)
            {
                spEstimatedInvoicedAmount.Value = estimatedInvoicedAmount;
            }
            else
            {
                spEstimatedInvoicedAmount.Value = DBNull.Value;
            }
            p.Add(spEstimatedInvoicedAmount);
            if (CustFields != null)
            {
                for (int i = 0; i < CustFields.Length; i++)
                {
                    string _cust = CustFields[i];
                    if (!string.IsNullOrEmpty(_cust)) p.Add(new SqlParameter("@vchCust" + (i + 1), _cust));
                }
            }
            if (CustDates != null)
            {
                for (int i = 0; i < CustDates.Length; i++)
                {
                    DateTime _dtcust = CustDates[i];
                    if (_dtcust != null && _dtcust > DateTime.MinValue) p.Add(new SqlParameter("@dtCust" + (i + 1), _dtcust));
                }
            }
            SqlParameter spSupGroupId = new SqlParameter("@SupGroupId", SqlDbType.Int);
            if (supGroupId != 0)
            {
                spSupGroupId.Value = supGroupId;
            }
            else
            {
                spSupGroupId.Value = DBNull.Value;
            }
            p.Add(spSupGroupId);
            SqlParameter spPriorityId = new SqlParameter("@PriorityId", SqlDbType.Int);
            if (priorityId != 0)
            {
                spPriorityId.Value = priorityId;
            }
            else
            {
                spPriorityId.Value = DBNull.Value;
            }
            p.Add(spPriorityId);

            UpdateData("sp_UpdateProject", p.ToArray());
		}

        public static int Insert(int companyID, int accountID, int parentID,
            string name, string description, int internalPMID, int clientPMID, int active, int estimatedHours,
            decimal estimatedCost, decimal estimatedInvoicedAmount, string[] CustFields, DateTime[] CustDates, int supGroupId,
            int priorityId)
		{
            System.Collections.Generic.List<SqlParameter> p = new System.Collections.Generic.List<SqlParameter>();
            SqlParameter _pRVAL = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            _pRVAL.Direction = ParameterDirection.ReturnValue;
            p.Add(_pRVAL);
			p.Add(new SqlParameter("@CompanyID", companyID));
			p.Add(new SqlParameter("@AccountID", accountID));
            SqlParameter spParentID = new SqlParameter("@ParentID", SqlDbType.Int);
			if (parentID != 0)
			{
                spParentID.Value = parentID;
			}
			else
			{
                spParentID.Value = DBNull.Value;
			}
            p.Add(spParentID);
			p.Add(new SqlParameter("@Name", name));
			p.Add(new SqlParameter("@Description", description));
            SqlParameter spInternalPMID = new SqlParameter("@InternalPMID", SqlDbType.Int);
			if (internalPMID != 0)
			{
                spInternalPMID.Value = internalPMID;
			}
			else
			{
                spInternalPMID.Value = DBNull.Value;
			}
            p.Add(spInternalPMID);
            SqlParameter spClientPMID = new SqlParameter("@ClientPMID", SqlDbType.Int);
			if (clientPMID != 0)
			{
                spClientPMID.Value = clientPMID;
			}
			else
			{
                spClientPMID.Value = DBNull.Value;
			}
            p.Add(spClientPMID);
            SqlParameter spActive = new SqlParameter("@Active", SqlDbType.Bit);
            if (active < 0)
            {
                spActive.Value = DBNull.Value;
            }
            else
            {
                spActive.Value = active;
            }
            p.Add(spActive);
            SqlParameter spEstimatedHours = new SqlParameter("@EstimatedHours", SqlDbType.Int);
            if (estimatedHours != 0)
            {
                spEstimatedHours.Value = estimatedHours;
            }
            else
            {
                spEstimatedHours.Value = DBNull.Value;
            }
            p.Add(spEstimatedHours);
            SqlParameter spEstimatedCost = new SqlParameter("@EstimatedCost", SqlDbType.Decimal);
            if (estimatedCost != 0)
            {
                spEstimatedCost.Value = estimatedCost;
            }
            else
            {
                spEstimatedCost.Value = DBNull.Value;
            }
            p.Add(spEstimatedCost);
            SqlParameter spEstimatedInvoicedAmount = new SqlParameter("@EstimatedInvoicedAmount", SqlDbType.Decimal);
            if (estimatedInvoicedAmount != 0)
            {
                spEstimatedInvoicedAmount.Value = estimatedInvoicedAmount;
            }
            else
            {
                spEstimatedInvoicedAmount.Value = DBNull.Value;
            }
            p.Add(spEstimatedInvoicedAmount);
            if (CustFields != null)
            {
                for (int i = 0; i < CustFields.Length; i++)
                {
                    string _cust = CustFields[i];
                    if (!string.IsNullOrEmpty(_cust)) p.Add(new SqlParameter("@vchCust" + (i + 1), _cust));
                }
            }
            if (CustDates != null)
            {
                for (int i = 0; i < CustDates.Length; i++)
                {
                    DateTime _dtcust = CustDates[i];
                    if (_dtcust != null && _dtcust > DateTime.MinValue) p.Add(new SqlParameter("@dtCust" + (i + 1), _dtcust));
                }
            }
            SqlParameter spSupGroupId = new SqlParameter("@SupGroupId", SqlDbType.Int);
            if (supGroupId != 0)
            {
                spSupGroupId.Value = supGroupId;
            }
            else
            {
                spSupGroupId.Value = DBNull.Value;
            }
            p.Add(spSupGroupId);
            SqlParameter spPriorityId = new SqlParameter("@PriorityId", SqlDbType.Int);
            if (priorityId != 0)
            {
                spPriorityId.Value = priorityId;
            }
            else
            {
                spPriorityId.Value = DBNull.Value;
            }
            p.Add(spPriorityId);

            UpdateData("sp_InsertProject", p.ToArray());
            return (int)p[0].Value;
		}

        public static DataTable SelectList(int companyID, int accountID, int activeStatus, int userID, bool btCfgAcctMngr)
        {
            return SelectList(companyID, accountID, activeStatus, userID, false, btCfgAcctMngr);
        }

        public static DataTable SelectList(int companyID, int accountID, int activeStatus, int userID, bool onlyMyProjects, bool btCfgAcctMngr)
		{
            SqlParameter _paramsActive = new SqlParameter("@Active", SqlDbType.Bit);
            if (activeStatus < 0)
            {
                _paramsActive.Value = DBNull.Value;
            }
            else
            {
                _paramsActive.Value = activeStatus;
            }
			return SelectRecords("sp_SelectProjectList", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					_paramsActive,
					new SqlParameter("@UserID", userID),
					new SqlParameter("@MyProjects", onlyMyProjects ? 1 : 0),
					new SqlParameter("@btCfgAcctMngr", btCfgAcctMngr ? 1 : 0)                   
				   });
		}

		// --- ProjectTime chapter ---
		public static void InsertTimeRecord(int DepartmentID, int ProjectID, int UserID, int TaskTypeID,
			DateTime Date, decimal Hours, string Note, DateTime StartTime, DateTime StopTime, DateTime CreatedTime, int CreatedBy, int TimeOffset, decimal HourlyRate,
			int accountID)
		{
			SqlParameter[] _params = new SqlParameter[14];
			_params[0] = new SqlParameter("@DepartmentID", DepartmentID);
			_params[1] = new SqlParameter("@ProjectID", SqlDbType.Int);
			if (ProjectID <= 0) _params[1].Value = DBNull.Value;
			else _params[1].Value = ProjectID;
			_params[2] = new SqlParameter("@UserID", SqlDbType.Int);
			if (UserID != 0) _params[2].Value = UserID;
			else _params[2].Value = DBNull.Value;
			_params[3] = new SqlParameter("@TaskTypeID", SqlDbType.Int);
			if (TaskTypeID != 0) _params[3].Value = TaskTypeID;
			else _params[3].Value = DBNull.Value;
			_params[4] = new SqlParameter("@Date", Date);
			_params[5] = new SqlParameter("@Hours", Hours);
			_params[6] = new SqlParameter("@Note", SqlDbType.NVarChar);
			if (!String.IsNullOrEmpty(Note)) _params[6].Value = Note;
			else _params[6].Value = DBNull.Value;
			_params[7] = new SqlParameter("@StartTime", SqlDbType.SmallDateTime);
            if (StartTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[7].Value = StartTime;
			else _params[7].Value = DBNull.Value;
			_params[8] = new SqlParameter("@StopTime", SqlDbType.SmallDateTime);
            if (StopTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[8].Value = StopTime;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@CreatedTime", SqlDbType.SmallDateTime);
			if (CreatedTime != DateTime.MinValue) _params[9].Value = CreatedTime;
			else _params[9].Value = DBNull.Value;
			_params[10] = new SqlParameter("@CreatedBy", SqlDbType.Int);
			if (CreatedBy != 0) _params[10].Value = CreatedBy;
			else _params[10].Value = DBNull.Value;
			_params[11] = new SqlParameter("@TimeOffset", TimeOffset);
			_params[12] = new SqlParameter("@HourlyRate", HourlyRate);
			_params[13] = new SqlParameter("@AccountID", SqlDbType.Int);
			if (accountID <= 0) _params[13].Value = DBNull.Value;
			else _params[13].Value = accountID;
			UpdateData("sp_InsertProjectTime", _params);            
		}

		public static void UpdateTimeRecord(int ProjectTimeId, int DepartmentID, int ProjectID, int UserID, int TaskTypeID,
			DateTime Date, decimal Hours, string Note, DateTime StartTime, DateTime StopTime, DateTime UpdatedTime,
			int UpdatedBy, int TimeOffset, decimal HourlyRate, int accountID)
		{ // this method was used for update project time records
			SqlParameter[] _params = new SqlParameter[15];
			_params[0] = new SqlParameter("@ProjectTimeId", ProjectTimeId);
			_params[1] = new SqlParameter("@DepartmentID", DepartmentID);
			_params[2] = new SqlParameter("@ProjectID", ProjectID);
			if (ProjectID <= 0) _params[2].Value = DBNull.Value;
			else _params[2].Value = ProjectID;
			_params[3] = new SqlParameter("@UserID", SqlDbType.Int);
			if (UserID != 0) _params[3].Value = UserID;
			else _params[3].Value = DBNull.Value;
			_params[4] = new SqlParameter("@TaskTypeID", SqlDbType.Int);
			if (TaskTypeID != 0) _params[4].Value = TaskTypeID;
			else _params[4].Value = DBNull.Value;
			_params[5] = new SqlParameter("@Date", Date);
			_params[6] = new SqlParameter("@Hours", Hours);
			_params[7] = new SqlParameter("@Note", SqlDbType.NVarChar);
			if (!String.IsNullOrEmpty(Note)) _params[7].Value = Note;
			else _params[7].Value = DBNull.Value;
			_params[8] = new SqlParameter("@StartTime", SqlDbType.SmallDateTime);
            if (StartTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[8].Value = StartTime;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@StopTime", SqlDbType.SmallDateTime);
            if (StopTime > System.Data.SqlTypes.SqlDateTime.MinValue.Value) _params[9].Value = StopTime;
			else _params[9].Value = DBNull.Value;
			_params[10] = new SqlParameter("@UpdatedTime", SqlDbType.SmallDateTime);
			if (UpdatedTime != DateTime.MinValue) _params[10].Value = UpdatedTime;
			else _params[10].Value = DBNull.Value;
			_params[11] = new SqlParameter("@UpdatedBy", SqlDbType.Int);
			if (UpdatedBy != 0) _params[11].Value = UpdatedBy;
			else _params[11].Value = DBNull.Value;
			_params[12] = new SqlParameter("@TimeOffset", TimeOffset);
			_params[13] = new SqlParameter("@HourlyRate", HourlyRate);
			_params[14] = new SqlParameter("@AccountID", SqlDbType.Int);
			if (accountID <= 0) _params[14].Value = DBNull.Value;
			else _params[14].Value = accountID;
			UpdateData("sp_UpdateProjectTime", _params);
		}

        public static DataTable SelectProjectTimeList(int DepartmentID, int ProjectID, DateTime Date, int TechID,
			int accountID, int taskTypeId)
        {
            return SelectProjectTimeList(Guid.Empty, DepartmentID, ProjectID, Date, TechID, accountID, taskTypeId);
        }

		public static DataTable SelectProjectTimeList(Guid orgId, int DepartmentID, int ProjectID, DateTime Date, int TechID,
			int accountID, int taskTypeId)
		{
            SqlParameter spDate = new SqlParameter("@Date", SqlDbType.SmallDateTime);
            if (Date == DateTime.MinValue)
            {
                spDate.Value = DBNull.Value;
            }
            else
            {
                spDate.Value = Date;
            }
			return SelectRecords("sp_SelectProjectTimeList", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentID),
					new SqlParameter("@ProjectID", ProjectID),
					spDate,
					new SqlParameter("@TechID", TechID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@TaskTypeId", taskTypeId)
				   },orgId);
		}

		public static DataTable SelectProjectsDayTime(int companyID, int accountID, int userID, DateTime SelectedDate)
		{
			return SelectRecords("sp_SelectProjectsDayTime", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@Date", SelectedDate)
				   });
		}

		public static DataRow SelectProjectTime(int DepartmentID, int ProjectID, int ProjectTimeId)
		{
			return SelectRecord("sp_SelectProjectTime", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentID),
					new SqlParameter("@ProjectID", ProjectID),
					new SqlParameter("@ProjectTimeId", ProjectTimeId)
				   });
		}

		public static DataTable SelectProjectTotalTime(int DepartmentID, int ProjectID, DateTime Time, int TechID,
			int accountID, int taskTypeId, int ticketAccountID)
		{
			return SelectRecords("sp_SelectProjectTotalTime", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentID),
					new SqlParameter("@ProjectID", ProjectID),
					new SqlParameter("@Time", Time),
					new SqlParameter("@TechID", TechID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@TaskTypeId", taskTypeId),
					new SqlParameter("@TicketAccountID", ticketAccountID),
				   });
		}

		public static void InsertDayTime(int DepartmentID, int UserID, DateTime Date, DateTime StartTime, DateTime StopTime, 
				decimal TimeOut, decimal TotalDayTime, DateTime CreatedTime, int CreatedBy, int TimeOffset)
		{
			SqlParameter[] _params = new SqlParameter[10];
			_params[0] = new SqlParameter("@DepartmentID", DepartmentID);
			_params[1] = new SqlParameter("@UserID", UserID);
			_params[2] = new SqlParameter("@Date", Date);
			_params[3] = new SqlParameter("@StartTime", StartTime);
			_params[4] = new SqlParameter("@StopTime", StopTime);
			_params[5] = new SqlParameter("@TimeOut", TimeOut);
			_params[6] = new SqlParameter("@TotalDayTime", TotalDayTime);
			_params[7] = new SqlParameter("@CreatedTime", SqlDbType.SmallDateTime);
			if (CreatedTime != DateTime.MinValue) _params[7].Value = CreatedTime;
			else _params[7].Value = DBNull.Value;
			_params[8] = new SqlParameter("@CreatedBy", SqlDbType.Int);
			if (CreatedBy != 0) _params[8].Value = CreatedBy;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@TimeOffset", TimeOffset);
			UpdateData("sp_InsertDaySummaryTime", _params);
		}

		public static void UpdateDayTime(int Id, int DepartmentID, int UserID, DateTime Date, DateTime StartTime, DateTime StopTime,
				decimal TimeOut, decimal TotalDayTime, DateTime UpdatedTime, int UpdatedBy, int TimeOffset)
		{
			SqlParameter[] _params = new SqlParameter[11];
			_params[0] = new SqlParameter("@Id", Id);
			_params[1] = new SqlParameter("@DepartmentID", DepartmentID);
			_params[2] = new SqlParameter("@UserID", UserID);
			_params[3] = new SqlParameter("@Date", Date);
			_params[4] = new SqlParameter("@StartTime", StartTime);
			_params[5] = new SqlParameter("@StopTime", StopTime);
			_params[6] = new SqlParameter("@TimeOut", TimeOut);
			_params[7] = new SqlParameter("@TotalDayTime", TotalDayTime);
			_params[8] = new SqlParameter("@UpdatedTime", SqlDbType.SmallDateTime);
			if (UpdatedTime != DateTime.MinValue) _params[8].Value = UpdatedTime;
			else _params[8].Value = DBNull.Value;
			_params[9] = new SqlParameter("@UpdatedBy", SqlDbType.Int);
			if (UpdatedBy != 0) _params[9].Value = UpdatedBy;
			else _params[9].Value = DBNull.Value;
			_params[10] = new SqlParameter("@TimeOffset", TimeOffset);
			UpdateData("sp_UpdateDaySummaryTime", _params);
		}

		public static DataTable SelectDayReports(int DepartmentID, int UserID, DateTime CurCalendarView, DateTime SelectedDate)
		{
			return SelectRecords("sp_SelectDayReports", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentID),
					new SqlParameter("@UserID", UserID),
					new SqlParameter("@CalendarView", CurCalendarView),
					new SqlParameter("@SelectedDate", SelectedDate)

				   });
		}
		// ---------------------------

		public static DataTable SelectBillingMethods()
		{
			return SelectRecords("sp_SelectBillingMethods");
		}

		public static void UpdateProjectBillableRates(int projectID, int billingMethodID, decimal flatFee,
			decimal hourlyBlendedRate, int ratePlanID, int companyID, int flatFeeMode, DateTime dtNextDate, string sQBAccount, string sQBItem)
		{
			SqlParameter[] _params = new SqlParameter[10];
			_params[0] = new SqlParameter("@ProjectID", projectID);
			_params[1] = new SqlParameter("@BillingMethodID", billingMethodID);
			_params[2] = new SqlParameter("@FlatFee", SqlDbType.Money);
			if (flatFee > 0)
			{
				_params[2].Value = flatFee;
			}
			else
			{
				_params[2].Value = DBNull.Value;
			}
			_params[3] = new SqlParameter("@HourlyBlendedRate", SqlDbType.SmallMoney);
			if (hourlyBlendedRate > 0)
			{
				_params[3].Value = hourlyBlendedRate;
			}
			else
			{
				_params[3].Value = DBNull.Value;
			}
			_params[4] = new SqlParameter("@RatePlanID", SqlDbType.Int);
			if (ratePlanID > 0)
			{
				_params[4].Value = ratePlanID;
			}
			else
			{
				_params[4].Value = DBNull.Value;
			}
			_params[5] = new SqlParameter("@CompanyID", companyID);
			_params[6] = new SqlParameter("@FlatFeeMode", SqlDbType.Int);
			if (flatFeeMode > -1) _params[6].Value = flatFeeMode;
			else _params[6].Value = DBNull.Value;
			_params[7] = new SqlParameter("@FlatFeeNextDate", SqlDbType.SmallDateTime);
			if (dtNextDate > DateTime.MinValue) _params[7].Value = dtNextDate;
			else _params[7].Value = DBNull.Value;

			_params[8] = new SqlParameter("@QBAccountAlias", SqlDbType.NVarChar);
			if (String.IsNullOrEmpty(sQBAccount)) _params[8].Value = DBNull.Value;
			else _params[8].Value = sQBAccount;

			_params[9] = new SqlParameter("@QBItemAlias", SqlDbType.NVarChar);
			if (String.IsNullOrEmpty(sQBItem)) _params[9].Value = DBNull.Value;
			else _params[9].Value = sQBItem;
			UpdateData("sp_UpdateProjectBillableRates", _params);
		}

		public static DataTable SelectProjectTechs(int projectID, int companyID)
		{
			return SelectRecords("sp_SelectProjectTechs", new SqlParameter[]
				   {
					new SqlParameter("@ProjectID", projectID),
					new SqlParameter("@CompanyID", companyID)
				   });
		}

		public static void InsertProjectTech(int companyID, int projectID, int techID)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@TechID", techID);
			UpdateData("sp_InsertProjectTech", _params);
		}

		public static void DeleteProjectTech(int projectTechID, int companyID)
		{
			UpdateData("sp_DeleteProjectTech", new SqlParameter[]
			{
				 new SqlParameter("@ProjectTechID", projectTechID),
				 new SqlParameter("@CompanyID", companyID),
			});
		}

		public static DataTable SelectProjectTaskTypeRates(int projectID, int companyID,
			int ratePlanID)
		{
			return SelectRecords("sp_SelectProjectTaskTypeRates", new SqlParameter[]
				   {
					new SqlParameter("@ProjectID", projectID),
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@RatePlanID", ratePlanID)
				   });
		}

		public static void InsertProjectTaskTypeRate(int companyID, int projectID, int taskTypeID,
			decimal hourlyRate)
		{
			SqlParameter[] _params = new SqlParameter[4];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@TaskTypeID", taskTypeID);
			_params[3] = new SqlParameter("@HourlyRate", hourlyRate);
			UpdateData("sp_InsertProjectTaskTypeRate", _params);
		}

		public static void DeleteProjectTaskTypeRate(int projectTaskTypeRateID, int companyID)
		{
			UpdateData("sp_DeleteProjectTaskTypeRate", new SqlParameter[]
			{
				 new SqlParameter("@ProjectTaskTypeRateID", projectTaskTypeRateID),
				 new SqlParameter("@CompanyID", companyID)
			});
		}

		public static void UpdateProjectTaskTypeRate(int projectTaskTypeRateID, decimal hourlyRate,
			int companyID)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@ProjectTaskTypeRateID", projectTaskTypeRateID);
			_params[1] = new SqlParameter("@HourlyRate", hourlyRate);
			_params[2] = new SqlParameter("@CompanyID", companyID);

			UpdateData("sp_UpdateProjectTaskTypeRate", _params);
		}

		public static void ClearProjectTaskTypeRates(int companyID, int projectID)
		{
			SqlParameter[] _params = new SqlParameter[2];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);

			UpdateData("sp_ClearProjectTaskTypeRates", _params);
		}

		public static void DeleteProjectRate(int projectID, int companyID)
		{
			UpdateData("sp_DeleteProjectRate", new SqlParameter[]
			{
				new SqlParameter("@ProjectID", projectID),
				new SqlParameter("@CompanyID", companyID)
			});
		}

		public static void DeleteAllProjectTechs(int companyID, int projectID)
		{
			UpdateData("sp_DeleteAllProjectTechs", new SqlParameter[]
			{
				new SqlParameter("@CompanyID", companyID),
				new SqlParameter("@ProjectID", projectID)
			});
		}

        public static DataTable SelectAllParent(int DeptID, int ProjectId)
        {
            return SelectAllParent(Guid.Empty, DeptID, ProjectId);
        }

	    // Project to Ticket Chapter
		public static DataTable SelectAllParent(Guid OrgId, int DeptID, int ProjectId)
		{
            return SelectByQuery("SELECT P.*, PP.Level, PP.IsLastChild FROM dbo.fxGetAllParentProjects(" + DeptID.ToString() + "," + ProjectId.ToString() + ") PP INNER JOIN Project P ON P.CompanyID = " + DeptID.ToString() + " AND P.ProjectID=PP.Id ORDER BY Level", OrgId);
		}

        public static DataTable SelectProjectHierarchy(int CompanyID, int AccountID, int ParentID, bool Active)
        {
            return SelectProjectHierarchy(Guid.Empty, CompanyID, AccountID, ParentID, Active);
        }

	    public static DataTable SelectProjectHierarchy(Guid OrgId, int CompanyID, int AccountID, int ParentID, bool Active)
		{
			SqlParameter _pParentId = new SqlParameter("@ParentID", SqlDbType.Int);
			if (ParentID < 0) _pParentId.Value = DBNull.Value;
			else _pParentId.Value = ParentID;
			SqlParameter _pActive = new SqlParameter("@Active", SqlDbType.Bit);
			_pActive.Value = (Active) ? 1 : 0;
			return SelectRecords("sp_SelectProjectHierarchy", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", CompanyID),
					new SqlParameter("@AccountID", AccountID),
					_pParentId,
					_pActive
				   }, OrgId);
		}

		public static int ProjectCount(int companyID, int accountID)
		{
			int retValue = 0;
			DataTable dtCount = SelectByQuery(String.Format("SELECT COUNT(Project.ProjectID) AS ProjectCount FROM Project WHERE Project.Active = 1 AND Project.ParentID IS NULL AND Project.CompanyID = {0} AND (({1}=-1 AND Project.AccountID IS null) OR Project.AccountID={1} OR {1}=0)", companyID.ToString(), accountID.ToString()));
			if (dtCount != null)
				if (dtCount.Rows.Count > 0) retValue = (int)dtCount.Rows[0]["ProjectCount"];
			return retValue;
		}

		public static DataTable SelectUserTimeReport(int companyID, int accountID, string userIDs,
			DateTime dateStart, DateTime dateEnd)
		{
			return SelectRecords("sp_SelectUserTimeReport", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@UserIDs", userIDs),
					new SqlParameter("@DateStart", dateStart),
					new SqlParameter("@DateEnd", dateEnd)
				   });
		}

		public static DataTable SelectUserTimeReport(int companyID, string accountIDs,
			DateTime dateStart, DateTime dateEnd)
		{
			return SelectRecords("sp_SelectAccountTimeReport", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),                    
					new SqlParameter("@AccountIDs", accountIDs),
					new SqlParameter("@DateStart", dateStart),
					new SqlParameter("@DateEnd", dateEnd)
				   });
		}
		//--------------------------
		public static DataRow SelectProjectFlatFee(int CompanyID, int ProjectID)
		{
			return SelectRecord("sp_SelectProjectFlatFee", new SqlParameter[] { new SqlParameter("@DId", CompanyID), new SqlParameter("@ProjectID", ProjectID) });
		}

		public static DataRow SelectProjectBlendedRate(int CompanyID, int ProjectID)
		{
			return SelectRecord("sp_SelectProjectBlendedRate", new SqlParameter[] { new SqlParameter("@DId", CompanyID), new SqlParameter("@ProjectID", ProjectID) });
		}

		public static DataTable SelectProjectRatesHierarchy(int companyID, int ProjectTaskTypeRateID)
		{
			SqlParameter[] _params = new SqlParameter[2];
			_params[0] = new SqlParameter("@ProjectTaskTypeRateID", ProjectTaskTypeRateID);
			_params[1] = new SqlParameter("@CompanyID", companyID);
			return SelectRecords("sp_SelectProjectRatesHierarchy", _params);
		}

		public static DataTable SelectProjectAvailableTechs(int CompanyID, int ProjectID)
		{
			return SelectRecords("sp_SelectAccountProjectTechs", new SqlParameter[] { new SqlParameter("@DepartmentId", CompanyID), new SqlParameter("@ProjectID", ProjectID) });
		}

		public static DataTable SelectProjectAvailableTaskTypes(int CompanyID, int ProjectID)
		{
			return SelectRecords("sp_SelectProjectTaskTypes", new SqlParameter[] { new SqlParameter("@DepartmentID", CompanyID), new SqlParameter("@ProjectID", ProjectID) });
		}

		public static bool HasProjectTimeSettings(int CompanyID, int ProjectID)
		{
			DataRow _row = Select(CompanyID, ProjectID);
			if (_row != null) if (!_row.IsNull("BillingMethodID")) return true;
			DataTable dt = SelectProjectTaskTypeRates(ProjectID, CompanyID, 0);
			if (dt.Rows.Count > 0) return true;
			dt = SelectProjectTechs(ProjectID, CompanyID);
			if (dt.Rows.Count > 0) return true;
			return false;
		}

		// Retainers

		public static DataTable SelectProjectRetainers(int companyID, int projectID)
		{
			return SelectRecords("sp_SelectProjectRetainers", new SqlParameter[]
				   {
					   new SqlParameter("@CompanyID", companyID),
					   new SqlParameter("@ProjectID", projectID)                    
				   });
		}

		public static void InsertProjectRetainer(int companyID, int projectID, int techID)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@TechID", techID);
			UpdateData("sp_InsertProjectRetainer", _params);
		}

		public static void DeleteProjectRetainer(int companyID, int projectRetainerID)
		{
			UpdateData("sp_DeleteProjectRetainer", new SqlParameter[]
			{
				new SqlParameter("@CompanyID", companyID),
				new SqlParameter("@ProjectRetainerID", projectRetainerID)                 
			});
		}

		public static void UpdateProjectRetainerAmount(int companyID, int itemId, decimal newValue, DateTime newStartDate, DateTime newEndDate)
		{
			SqlParameter[] _params = new SqlParameter[5];
			_params[0] = new SqlParameter("@DId", companyID);
			_params[1] = new SqlParameter("@Id", itemId);
			_params[2] = new SqlParameter("@NewAmount", newValue);
			_params[3] = new SqlParameter("@NewStartDate", newStartDate);
			_params[4] = new SqlParameter("@NewEndDate", SqlDbType.SmallDateTime);
			if (newEndDate == DateTime.MinValue) _params[4].Value = DBNull.Value;
			else _params[4].Value = newEndDate;
			UpdateData("sp_UpdateProjectRetainerAmount", _params);
		}

		public static DataTable SelectUserTimeReportDetails(int companyID, int accountID, int userID,
			DateTime dateStart, DateTime dateEnd, int taskTypeId, int projectID)
		{
			return SelectRecords("sp_SelectUserTimeReportDetails", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@DateStart", dateStart),
					new SqlParameter("@DateEnd", dateEnd),
					new SqlParameter("@TaskTypeId", taskTypeId),
					new SqlParameter("@ProjectID", projectID)
				   });
		}

		public static DataTable SelectDetailTimeReport(int DepartmentId, DateTime StartDate, DateTime EndDate, GroupByType groupByType, string AccountsList, string AccountsOnlyList, string ProjectList, string TechniciansList)
		{
			return SelectRecords("sp_SelectDetailTimeReport", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentId),
					new SqlParameter("@DateStart", StartDate),
					new SqlParameter("@DateEnd", EndDate),
					new SqlParameter("@GroupByField", (int)groupByType),
					new SqlParameter("@AccountIDs", AccountsList),
					new SqlParameter("@AccountOnlyIDs", AccountsOnlyList),
					new SqlParameter("@ProjectIDs", ProjectList),
					new SqlParameter("@TechsIDs", TechniciansList)
				   });
		}

		public static void DeleteProjectTime(int projectTimeId, int departmentID)
		{
			UpdateData("sp_DeleteProjectTime", new SqlParameter[]
			{
				 new SqlParameter("@ProjectTimeId", projectTimeId),
				 new SqlParameter("@DepartmentID", departmentID),
			});
		}

		public static DataTable SelectExportTimeLogs(int companyID, DateTime dateStart, DateTime dateEnd)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@DateStart", dateStart);
			_params[2] = new SqlParameter("@DateEnd", dateEnd);
			return SelectRecords("sp_SelectExportTimeLogs", _params);
		}

		public static int SelectTimeLogCountByDate(int companyID, DateTime startDate, DateTime endDate)
		{
			SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
			pReturnValue.Direction = ParameterDirection.ReturnValue;

			UpdateData("sp_SelectTimeLogCountByDate",
						new[] {pReturnValue,
							   new SqlParameter("@CompanyID", companyID),
							   new SqlParameter("@DateStart", startDate),
							   new SqlParameter("@DateEnd", endDate)});

			return (int)pReturnValue.Value;
		}

		public static DataTable SelectAccountsProjects(int companyID)
		{
			SqlParameter[] _params = new SqlParameter[1];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			return SelectRecords("sp_SelectAccountsProjects", _params);
		}

		public static DataTable SelectProjectsTree(int companyID, int accountID)
		{
			SqlParameter[] _params = new SqlParameter[2];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@AccountID", accountID);
			return SelectRecords("sp_SelectProjectsTree", _params);
		}

		public static void Rename(int companyID, int projectID, 
			string name)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@Name", name);
			
			UpdateData("sp_RenameProject", _params);
		}

		public static void Inactivate(int companyID, int projectID, bool active)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@Active", active);

			UpdateData("sp_InactivateProject", _params);
		}

		public static void Move(int companyID, int projectID, int parentID)
		{
			SqlParameter[] _params = new SqlParameter[3];
			_params[0] = new SqlParameter("@CompanyID", companyID);
			_params[1] = new SqlParameter("@ProjectID", projectID);
			_params[2] = new SqlParameter("@ParentID", SqlDbType.Int);
			if (parentID != 0)
			{
				_params[2].Value = parentID;
			}
			else
			{
				_params[2].Value = DBNull.Value;
			}

			UpdateData("sp_MoveProject", _params);
		}

		public static DataTable SelectUserDayTime(int companyID, int userID, DateTime selectedDate)
		{
			return SelectRecords("sp_SelectUserDayTime", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@Date", selectedDate)
				   });
		}

		public static void UpdateDayTimeProjectNotes(int departmentID, DateTime date, int userID,
			int accountId, int projectId, string completedNotes, string nextStepsNotes)
		{
			SqlParameter[] _params = new SqlParameter[7];
			_params[0] = new SqlParameter("@DId", departmentID);
			_params[1] = new SqlParameter("@Date", date);
			_params[2] = new SqlParameter("@UserId", userID);
			_params[3] = new SqlParameter("@AccountId", SqlDbType.Int);
			if (accountId > 0) _params[3].Value = accountId;
			else _params[3].Value = DBNull.Value;
			_params[4] = new SqlParameter("@ProjectId", SqlDbType.Int);
			if (projectId > 0) _params[4].Value = projectId;
			else _params[4].Value = DBNull.Value;
			_params[5] = new SqlParameter("@CompletedNotes", completedNotes);
			_params[6] = new SqlParameter("@NextStepsNotes", nextStepsNotes);
			UpdateData("sp_UpdateDayTimeProjectNotes", _params);
		}

		public static DataTable SelectTicketsDayTimeByProject(int companyID, int userID, DateTime selectedDate,
			int accountID, int projectID)
		{
			return SelectRecords("sp_SelectTicketsDayTimeByProject", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@Date", selectedDate),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@ProjectID", projectID)
				   });
		}

		public static DataTable SelectProjectDayTimeByProject(int companyID, int userID, DateTime selectedDate,
			int accountID, int projectID)
		{
			return SelectRecords("sp_SelectProjectDayTimeByProject", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@UserID", userID),
					new SqlParameter("@Date", selectedDate),
					new SqlParameter("@AccountID", accountID),
					new SqlParameter("@ProjectID", projectID)
				   });
		}

        public static DataTable SelectProjectUsers(int projectID, int companyID)
        {
            return SelectRecords("sp_SelectProjectUsers", new SqlParameter[]
                   {
                    new SqlParameter("@ProjectID", projectID),
                    new SqlParameter("@CompanyID", companyID)
                   });
        }

        public static void DeleteProjectUser(int projectTechID, int companyID)
        {
            UpdateData("sp_DeleteProjectUser", new SqlParameter[]
            {
                 new SqlParameter("@ProjectTechID", projectTechID),
                 new SqlParameter("@CompanyID", companyID),
            });
        }

        public static void InsertProjectUser(int companyID, int projectID, int userID, bool subscribeEmail)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@CompanyID", companyID);
            _params[1] = new SqlParameter("@ProjectID", projectID);
            _params[2] = new SqlParameter("@UserID", userID);
            _params[3] = new SqlParameter("@SubscribeEmail", subscribeEmail);
            UpdateData("sp_InsertProjectUser", _params);
        }

        public static void UpdateProjectUser(int departmentId, int userProjectsID,
            bool subscribeEmail)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DepartmentId", departmentId);
            _params[1] = new SqlParameter("@UserProjectsID", userProjectsID);
            _params[2] = new SqlParameter("@SubscribeEmail", subscribeEmail);

            UpdateData("sp_UpdateProjectUser", _params);
        }

        public static DataTable SelectDayTimeProjectNotes(int companyID, int accountID, int projectID, DateTime date, int userID)
        {
            return SelectRecords("sp_SelectDayTimeProjectNotes", new SqlParameter[]
                   {
                    new SqlParameter("@CompanyID", companyID),
                    new SqlParameter("@AccountID", accountID),
                    new SqlParameter("@ProjectID", projectID),
                    new SqlParameter("@Date", date),
                    new SqlParameter("@UserId", userID)
                   });
        }        
        
        
        public static DataRow SelectDetail(Guid orgId, int companyID, int projectID)
        {
            return SelectRecord("sp_SelectProjectDetail", new SqlParameter[] { new SqlParameter("@CompanyID", companyID),
			new SqlParameter("@ProjectID", projectID)}, orgId);
        }

        public static DataRow SelectDetail(int companyID, int projectID)
        {
            return SelectDetail(Guid.Empty, companyID, projectID);
        }

        public static DataTable SelectProjectRecipients(int projectID, int companyID)
        {
            return SelectRecords("sp_SelectProjectRecipients", new SqlParameter[]
                   {
                    new SqlParameter("@ProjectID", projectID),
                    new SqlParameter("@CompanyID", companyID)
                   });
        }

        public static DataTable SelectListWithHours(int companyID, int accountID, int activeStatus, int userID, bool onlyMyProjects, bool btCfgAcctMngr)
        {
            return SelectListWithHours(Guid.Empty, companyID, accountID, activeStatus, userID, onlyMyProjects, btCfgAcctMngr);
        }

        public static DataTable SelectListWithHours(Guid OrgId, int companyID, int accountID, int activeStatus, int userID, bool onlyMyProjects, bool btCfgAcctMngr)
        {
            SqlParameter _paramsActive = new SqlParameter("@Active", SqlDbType.Bit);
            if (activeStatus < 0)
            {
                _paramsActive.Value = DBNull.Value;
            }
            else
            {
                _paramsActive.Value = activeStatus;
            }
            return SelectRecords("sp_SelectProjectListWithHours", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", companyID),
					new SqlParameter("@AccountID", accountID),
					_paramsActive,
					new SqlParameter("@UserID", userID),
					new SqlParameter("@MyProjects", onlyMyProjects ? 1 : 0),
					new SqlParameter("@btCfgAcctMngr", btCfgAcctMngr ? 1 : 0)                   
				   }, OrgId);
        }

        public static DataTable SelectProjectPriorities(int deptID)
        {
            return SelectRecords("sp_SelectProjectPriorities", new SqlParameter[] { new SqlParameter("@DId", deptID) });
        }

        public static DataRow SelectProjectPriority(int deptID, int projectPriorityID)
        {
            return SelectRecord("sp_SelectProjectPriority", new SqlParameter[] { new SqlParameter("@DId", deptID), new SqlParameter("@ProjectPriorityID", projectPriorityID) });
        }

        public static void InsertProjectPriority(int dId, string name, string description)
        {
            SqlParameter[] _params = new SqlParameter[3];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@Name", name);
            _params[2] = new SqlParameter("@Description", description);
            UpdateData("sp_InsertProjectPriority", _params);
        }

        public static void UpdateProjectPriority(int dId, int projectPriorityID, string name, string description)
        {
            SqlParameter[] _params = new SqlParameter[4];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@ProjectPriorityID", projectPriorityID);
            _params[2] = new SqlParameter("@Name", name);
            _params[3] = new SqlParameter("@Description", description);
            UpdateData("sp_UpdateProjectPriority", _params);
        }

        public static void UpdateProjectPriorityDefault(int dId, int projectPriorityID)
        {
            SqlParameter[] _params = new SqlParameter[2];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@ProjectPriorityID", projectPriorityID);
            UpdateData("sp_UpdateProjectPriorityDefault", _params);
        }

        public static void DeleteProjectPriority(int dId, int projectPriorityID)
        {
            UpdateData("sp_DeleteProjectPriority", new SqlParameter[]
			{
				 new SqlParameter("@DId", dId),
				 new SqlParameter("@ProjectPriorityID", projectPriorityID),
			});
        }

        public static void UpdateProjectPriorityOrder(int DeptID, int priorityId, bool moveUp)
        {
            UpdateData("sp_UpdateProjectPriorityOrder", new SqlParameter[]{
				new SqlParameter("@DepartmentId", DeptID),
				new SqlParameter("@PriorityId", priorityId),
				new SqlParameter("@MoveUp", moveUp)
			});
        }

        public static DataRow SelectProjectTimeByID(Guid OrgId, int DepartmentID, int ProjectTimeId)
        {
            return SelectRecord("sp_SelectProjectTimeByID", new SqlParameter[]
				   {
					new SqlParameter("@CompanyID", DepartmentID),
					new SqlParameter("@ProjectTimeId", ProjectTimeId)
				   }, OrgId);
        }
	}
}
