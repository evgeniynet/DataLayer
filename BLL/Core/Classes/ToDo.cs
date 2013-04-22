using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    public class ToDo : DBAccess
    {
        public static DataTable SelectToDoListTemplates(int companyID)
        {
            return SelectToDoListTemplates(Guid.Empty, companyID);
        }

        public static DataTable SelectToDoListTemplates(Guid OrgId, int companyID)
        {
            return SelectRecords("sp_SelectToDoListTemplates", 
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID)
                }, OrgId);
        }

        public static void InsertToDoListTemplate(int dId, string name)
        {
            UpdateData("sp_InsertToDoListTemplate", new SqlParameter[]
				   {
                    new SqlParameter("@Id", Guid.NewGuid()),
					new SqlParameter("@DId", dId),
					new SqlParameter("@Name", name)                 
				   });
        }

        public static DataTable SelectToDoListAndItemsTemplates(int companyID)
        {
            return SelectRecords("sp_SelectToDoListAndItemsTemplates",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", companyID)
                });
        }

        public static void InsertToDoItemTemplate(int dId, string text, string toDoListTemplateId, int assignedId, 
            decimal hoursEstimatedRemaining)
        {
            SqlParameter spAssignedID = new SqlParameter("@AssignedId", assignedId);
            if (assignedId == 0)
            {
                spAssignedID.Value = DBNull.Value;
            }
            SqlParameter spHoursEstimatedRemaining = new SqlParameter("@HoursEstimatedRemaining", hoursEstimatedRemaining);
            if (hoursEstimatedRemaining == 0)
            {
                spHoursEstimatedRemaining.Value = DBNull.Value;
            }
            UpdateData("sp_InsertToDoItemTemplate", new SqlParameter[]
				   {
                    new SqlParameter("@Id", Guid.NewGuid()),
					new SqlParameter("@DId", dId),
                    new SqlParameter("@ToDoListTemplateId", toDoListTemplateId),
					new SqlParameter("@Text", text),
					spAssignedID,
					spHoursEstimatedRemaining              
				   });
        }

        public static void UpdateToDoItemTemplate(int dId, string text, string id, int assignedId, decimal hoursEstimatedRemaining)
        {
            SqlParameter spAssignedID = new SqlParameter("@AssignedId", assignedId);
            if (assignedId == 0)
            {
                spAssignedID.Value = DBNull.Value;
            }
            SqlParameter spHoursEstimatedRemaining = new SqlParameter("@HoursEstimatedRemaining", hoursEstimatedRemaining);
            if (hoursEstimatedRemaining == 0)
            {
                spHoursEstimatedRemaining.Value = DBNull.Value;
            }
            UpdateData("sp_UpdateToDoItemTemplate", new SqlParameter[]
				   {
                    new SqlParameter("@Id", id),
					new SqlParameter("@DId", dId),
					new SqlParameter("@Text", text),
					spAssignedID,
					spHoursEstimatedRemaining               
				   });
        }

        public static void DeleteToDoItemTemplate( int dId, string id)
        {
            UpdateData("sp_DeleteToDoItemTemplate", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId)
			});
        }

        public static void UpdateToDoListTemplate(int dId, string name, string id)
        {
            UpdateData("sp_UpdateToDoListTemplate", new SqlParameter[]
				   {
                    new SqlParameter("@Id", id),
					new SqlParameter("@DId", dId),
					new SqlParameter("@Name", name)                 
				   });
        }

        public static void DeleteToDoListTemplate(int dId, string id)
        {
            UpdateData("sp_DeleteToDoListTemplate", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId)
			});
        }

        public static void MoveToDoItemTemplate(int dId, string sourceToDoListTemplateId, string sourceToDoItemTemplateId,
            string destToDoListTemplateId, string destToDoItemTemplateId)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@sourceToDoListTemplateId", sourceToDoListTemplateId);
            _params[2] = new SqlParameter("@SourceToDoItemTemplateId", sourceToDoItemTemplateId);
            _params[3] = new SqlParameter("@DestToDoListTemplateId", destToDoListTemplateId);
            _params[4] = new SqlParameter("@DestToDoItemTemplateId", destToDoItemTemplateId);
            if (destToDoItemTemplateId == "")
            {
                _params[4].Value = DBNull.Value;
            }
            UpdateData("sp_MoveToDoItemTemplate", _params);
        }

        public static void CopyToDoListFromTemplate(int dId, string toDoListId, string toDoListTemplateId,
            int ticketId, int projectId, string name)
        {
            SqlParameter[] _params = new SqlParameter[6];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@ToDoListId", toDoListId);
            _params[2] = new SqlParameter("@ToDoListTemplateId", toDoListTemplateId);
            _params[3] = new SqlParameter("@TicketId", ticketId);
            if (ticketId == 0)
            {
                _params[3].Value = DBNull.Value;
            }
            _params[4] = new SqlParameter("@ProjectId", projectId);
            if (projectId == 0)
            {
                _params[4].Value = DBNull.Value;
            }
            _params[5] = new SqlParameter("@Name", name);
            if (name.Trim() == "")
            {
                _params[5].Value = DBNull.Value;
            }
            UpdateData("sp_CopyToDoListFromTemplate", _params);
        }

        public static void CopyToDoItemFromTemplate(int dId, string toDoItemId, string toDoListId, string toDoItemTemplateId,
            int createdBy)
        {
            UpdateData("sp_CopyToDoItemFromTemplate", new SqlParameter[]
			{
				 new SqlParameter("@DId", dId),
				 new SqlParameter("@ToDoItemId", toDoItemId),
				 new SqlParameter("@ToDoListId", toDoListId),
				 new SqlParameter("@ToDoItemTemplateId", toDoItemTemplateId),
				 new SqlParameter("@CreatedBy", createdBy)
			});
        }

        public static DataTable SelectToDoItemTemplates(int dId, string toDoListTemplateId)
        {
            return SelectRecords("sp_SelectToDoItemTemplates",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", dId),
				    new SqlParameter("@ToDoListTemplateId", toDoListTemplateId)
                });
        }

        public static DataTable SelectToDoListAndItems(int dId, int ticketId, int projectId)
        {
            return SelectToDoListAndItems(Guid.Empty, dId, ticketId, projectId);
        }

        public static DataTable SelectToDoListAndItems(Guid OrgId, int dId, int ticketId, int projectId)
        {
            return SelectRecords("sp_SelectToDoListAndItems",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", dId),
				    new SqlParameter("@TicketId", ticketId),
				    new SqlParameter("@ProjectId", projectId)
                }, OrgId);
        }

        public static void InsertToDoItem(int dId, string text, string toDoListId, int createdBy, int assignedId,
            decimal hoursEstimatedRemaining, DateTime dueDate)
        {
            SqlParameter[] _params = new SqlParameter[8];
            _params[0] = new SqlParameter("@Id", Guid.NewGuid());
            _params[1] = new SqlParameter("@DId", dId);
            _params[2] = new SqlParameter("@ToDoListId", toDoListId);
            _params[3] = new SqlParameter("@Text", text);
            _params[4] = new SqlParameter("@CreatedBy", createdBy);
            _params[5] = new SqlParameter("@AssignedId", assignedId);
            if (assignedId == 0)
            {
                _params[5].Value = DBNull.Value;
            }
            _params[6] = new SqlParameter("@HoursEstimatedRemaining", hoursEstimatedRemaining);
            if (hoursEstimatedRemaining == 0)
            {
                _params[6].Value = DBNull.Value;
            }
            _params[7] = new SqlParameter("@Due", dueDate);
            if (dueDate == DateTime.MinValue)
            {
                _params[7].Value = DBNull.Value;
            }
            
            UpdateData("sp_InsertToDoItem", _params);
        }

        public static void UpdateToDoItem(string id, int dId, string text, int updatedBy, int assignedId,
            decimal hoursEstimatedRemaining, DateTime dueDate)
        {
            SqlParameter[] _params = new SqlParameter[7];
            _params[0] = new SqlParameter("@Id", id);
            _params[1] = new SqlParameter("@DId", dId);
            _params[2] = new SqlParameter("@Text", text);
            _params[3] = new SqlParameter("@UpdatedBy", updatedBy);
            _params[4] = new SqlParameter("@AssignedId", assignedId);
            if (assignedId == 0)
            {
                _params[4].Value = DBNull.Value;
            }
            _params[5] = new SqlParameter("@HoursEstimatedRemaining", hoursEstimatedRemaining);
            if (hoursEstimatedRemaining == 0)
            {
                _params[5].Value = DBNull.Value;
            }
            _params[6] = new SqlParameter("@Due", dueDate);
            if (dueDate == DateTime.MinValue)
            {
                _params[6].Value = DBNull.Value;
            }

            UpdateData("sp_UpdateToDoItem", _params);
        }

        public static void DeleteToDoItem(Guid orgId, int dId, string id)
        {
            UpdateData("sp_DeleteToDoItem", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId)
			}, orgId);
        }

        public static void DeleteToDoItem(int dId, string id)
        {
            DeleteToDoItem(Guid.Empty, dId, id);
        }

        public static void MoveToDoItem(int dId, string sourceToDoListId, string sourceToDoItemId,
            string destToDoListId, string destToDoItemId)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@SourceToDoListId", sourceToDoListId);
            _params[2] = new SqlParameter("@SourceToDoItemId", sourceToDoItemId);
            _params[3] = new SqlParameter("@DestToDoListId", destToDoListId);
            _params[4] = new SqlParameter("@DestToDoItemId", destToDoItemId);
            if (destToDoItemId == "")
            {
                _params[4].Value = DBNull.Value;
            }
            UpdateData("sp_MoveToDoItem", _params);
        }

        public static void InsertToDoList(string id, int dId, string name, string toDoListTemplateId, int ticketId, int projectId)
        {
            SqlParameter[] _params = new SqlParameter[6];
            _params[0] = new SqlParameter("@Id", id);
            _params[1] = new SqlParameter("@DId", dId);
            _params[2] = new SqlParameter("@Name", name);
            _params[3] = new SqlParameter("@ToDoListTemplateId", toDoListTemplateId);
            if (toDoListTemplateId == "")
            {
                _params[3].Value = DBNull.Value;
            }
            _params[4] = new SqlParameter("@TicketId", ticketId);
            if (ticketId == 0)
            {
                _params[4].Value = DBNull.Value;
            }
            _params[5] = new SqlParameter("@ProjectId", projectId);
            if (projectId == 0)
            {
                _params[5].Value = DBNull.Value;
            }

            UpdateData("sp_InsertToDoList", _params);
        }

        public static void InsertToDoList(int dId, string name, string toDoListTemplateId, int ticketId, int projectId)
        {
            InsertToDoList(Guid.NewGuid().ToString(), dId, name, toDoListTemplateId, ticketId, projectId);
        }

        public static void UpdateToDoList(string id, int dId, string name)
        {
            UpdateData("sp_UpdateToDoList", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId),
				 new SqlParameter("@Name", name)
			});
        }

        public static void DeleteToDoList(Guid OrgId, int dId, string id)
        {
            UpdateData("sp_DeleteToDoList", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId)
			}, OrgId);
        }

        public static void DeleteToDoList(int dId, string id)
        {
            DeleteToDoList(Guid.Empty, dId, id);
        }

        public static void CompleteToDoItem(int dId, string id, bool completed, int updatedBy)
        {
            SqlParameter paramID = new SqlParameter("@Id", SqlDbType.UniqueIdentifier);
            Guid itemID;
            if (Guid.TryParse(id, out itemID))
            {
                paramID.Value = itemID;
                UpdateData("sp_CompleteToDoItem", new SqlParameter[]
			    {
				    paramID,
				    new SqlParameter("@DId", dId),
				    new SqlParameter("@Completed", completed),
				    new SqlParameter("@UpdatedBy", updatedBy)
			    });
            }
        }

        public static DataTable SelectToDoSearch(int dId, int assignedId, DateTime startDate, DateTime endDate,
            int listType)
        {
            SqlParameter[] _params = new SqlParameter[5];
            _params[0] = new SqlParameter("@DId", dId);
            _params[1] = new SqlParameter("@AssignedId", assignedId);
            _params[2] = new SqlParameter("@StartDate", startDate);
            if (startDate == DateTime.MinValue)
            {
                _params[2].Value = DBNull.Value;
            }
            _params[3] = new SqlParameter("@EndDate", endDate);
            if (endDate == DateTime.MinValue)
            {
                _params[3].Value = DBNull.Value;
            }
            _params[4] = new SqlParameter("@ListType", listType);

            return SelectRecords("sp_SelectToDoSearch",
                _params);
        }

        public static DataRow SelectToDoList(int dId, string id)
        {
            return SelectRecord("sp_SelectToDoList", new SqlParameter[] { new SqlParameter("@DId", dId),
			new SqlParameter("@Id", id)});
        }

        public static DataTable SelectToDoByUser(int dId, int assignedId)
        {
            return SelectRecords("sp_SelectToDoByUser",
                new SqlParameter[] 
                { 
                    new SqlParameter("@DId", dId),
				    new SqlParameter("@AssignedId", assignedId)
                });
        }

        public static DataRow SelectToDoItem(int dId, string id)
        {
            return SelectRecord("sp_SelectToDoItem", new SqlParameter[] { new SqlParameter("@DId", dId),
			new SqlParameter("@Id", id)});
        }

        public static void UpdateToDoItemTicketID(string id, int dId, int updatedBy, int ticketId)
        {
            UpdateData("sp_UpdateToDoItemTicketID", new SqlParameter[]
			{
				 new SqlParameter("@Id", id),
				 new SqlParameter("@DId", dId),
                 new SqlParameter("@UpdatedBy", updatedBy),
				 new SqlParameter("@TicketId", ticketId)
			});
        }
    }
}
