using System;
using System.Data;
using System.Data.Common;
using System.Collections.Specialized;
using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;
using System.Data.SqlClient;

namespace BWA.bigWebDesk.BLL
{
	public class Locations : BWA.bigWebDesk.DAL.Locations
	{
        public enum LocationType
        {
            ORGANIZATION = 1,
            BUILDING = 2,
            ROOM = 3
        }

        public Locations(Guid organizationId)
        {
            this.ConnectionString = Micajah.Common.Bll.Providers.OrganizationProvider.GetConnectionString(organizationId);
        }

        public static int AddLocation(Guid organizationId, int deptID,
                                       string name,
                                       LocationType typeID)
        {
            Locations loc = new Locations(organizationId);
            loc.AddNew();
            loc.DId = deptID;
            loc.Name = name;
            loc.LocationTypeId = (int)typeID;
            loc.Inactive = false;
            loc.IsDefault = false;
            loc.Save();
            return loc.Id;
        }

        public virtual void sp_UpdateLocation(ref int Id,
                                                int DId,
                                                int ParentId,
                                                int AccountId,
                                                int LocationTypeId,
                                                string Name,
                                                bool Inactive,
                                                string Country,
                                                string State,
                                                string City,
                                                string Address1,
                                                string Address2,
                                                string ZipCode,
                                                string Phone1,
                                                string Phone2,
                                                int WorkPlaces,
                                                string RoomNumber,
                                                decimal RoomSize,
                                                string Description,
                                                bool IsDefault)
        {
            //  Create the Database object, using the default database service. The
            //  default database service is determined through configuration.
            Database db = GetDatabase();

            string sqlCommand = this.SchemaStoredProcedureWithSeparator + "sp_UpdateLocation";
            DbCommand dbCommand = db.GetStoredProcCommand(sqlCommand);

            // Add procedure parameters
            db.AddOutParameter(dbCommand, "Id", DbType.Int32, 0);
            db.AddInParameter(dbCommand, "DId", DbType.Int32, DId);
            db.AddInParameter(dbCommand, "ParentId", DbType.Int32, ParentId);
            db.AddInParameter(dbCommand, "AccountId", DbType.Int32, AccountId);
            db.AddInParameter(dbCommand, "LocationTypeId", DbType.Int32, LocationTypeId);
            db.AddInParameter(dbCommand, "Name", DbType.AnsiString, Name);
            db.AddInParameter(dbCommand, "Inactive", DbType.Boolean, Inactive);
            db.AddInParameter(dbCommand, "Country", DbType.AnsiString, Country);
            db.AddInParameter(dbCommand, "State", DbType.AnsiString, State);
            db.AddInParameter(dbCommand, "City", DbType.AnsiString, City);
            db.AddInParameter(dbCommand, "Address1", DbType.AnsiString, Address1);
            db.AddInParameter(dbCommand, "Address2", DbType.AnsiString, Address2);
            db.AddInParameter(dbCommand, "ZipCode", DbType.AnsiString, ZipCode);
            db.AddInParameter(dbCommand, "Phone1", DbType.AnsiString, Phone1);
            db.AddInParameter(dbCommand, "Phone2", DbType.AnsiString, Phone2);
            db.AddInParameter(dbCommand, "WorkPlaces", DbType.Int32, WorkPlaces);
            db.AddInParameter(dbCommand, "RoomNumber", DbType.AnsiString, RoomNumber);
            db.AddInParameter(dbCommand, "RoomSize", DbType.Decimal, RoomSize);
            db.AddInParameter(dbCommand, "Description", DbType.AnsiString, Description);
            db.AddInParameter(dbCommand, "IsDefault", DbType.Boolean, IsDefault);

            base.LoadFromSqlNoExec(dbCommand);

            // Get output parameter values
            db.GetParameterValue(dbCommand, "Id");
        }

        
	}   
}
