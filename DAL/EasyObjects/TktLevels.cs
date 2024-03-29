/*
'===============================================================================
'  Generated From - CSharp_EasyObject_BusinessEntity.vbgen
' 
'  ** IMPORTANT  ** 
'  How to Generate your stored procedures:
' 
'  SQL      = SQL_DAAB_StoredProcs.vbgen
'  
'  This object is 'abstract' which means you need to inherit from it to be able
'  to instantiate it.  This is very easily done. You can override properties and
'  methods in your derived class, this allows you to regenerate this class at any
'  time and not worry about overwriting custom code. 
'
'  NEVER EDIT THIS FILE.
'
'  public class YourObject :  _YourObject
'  {
'
'  }
'
'===============================================================================
*/

// Generated by MyGeneration Version # (1.3.0.3)

using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.IO;

using Microsoft.Practices.EnterpriseLibrary.Data;
using NCI.EasyObjects;

namespace BWA.bigWebDesk.DAL
{

	#region Schema

	public class TktLevelsSchema : NCI.EasyObjects.Schema
	{
		private static ArrayList _entries;
		public static SchemaItem DId = new SchemaItem("DId", DbType.Int32, false, false, false, true, true, false);
		public static SchemaItem TintLevel = new SchemaItem("tintLevel", DbType.Byte, false, false, false, true, true, false);
		public static SchemaItem Description = new SchemaItem("Description", DbType.AnsiString, SchemaItemJustify.None, 2000, true, false, false, false);
		public static SchemaItem BitDefault = new SchemaItem("bitDefault", DbType.Boolean, false, true, false, false, false, false);
		public static SchemaItem IntLastResortId = new SchemaItem("intLastResortId", DbType.Int32, false, false, false, false, true, false);
		public static SchemaItem TintRoutingType = new SchemaItem("tintRoutingType", DbType.Byte, false, false, false, false, false, false);
		public static SchemaItem LevelName = new SchemaItem("LevelName", DbType.AnsiString, SchemaItemJustify.None, 50, true, false, false, false);

		public override ArrayList SchemaEntries
		{
			get
			{
				if (_entries == null )
				{
					_entries = new ArrayList();
					_entries.Add(TktLevelsSchema.DId);
					_entries.Add(TktLevelsSchema.TintLevel);
					_entries.Add(TktLevelsSchema.Description);
					_entries.Add(TktLevelsSchema.BitDefault);
					_entries.Add(TktLevelsSchema.IntLastResortId);
					_entries.Add(TktLevelsSchema.TintRoutingType);
					_entries.Add(TktLevelsSchema.LevelName);
				}
				return _entries;
			}
		}
	}
	#endregion

	public abstract class TktLevels : EasyObject
	{

		public TktLevels()
		{
			TktLevelsSchema _schema = new TktLevelsSchema();
			this.SchemaEntries = _schema.SchemaEntries;
			this.SchemaGlobal = "dbo";
		}
		
		public override void FlushData() 	 
		{ 	 
			this._whereClause = null; 	 
			this._aggregateClause = null; 	 
			base.FlushData(); 	 
		}
			   
		/// <summary>
		/// Loads the business object with info from the database, based on the requested primary key.
		/// </summary>
		/// <param name="DId"></param>
		/// <param name="TintLevel"></param>
		/// <returns>A Boolean indicating success or failure of the query</returns>
		public bool LoadByPrimaryKey(int DId, byte TintLevel)
		{
			switch(this.DefaultCommandType)
			{
				case CommandType.StoredProcedure:
					ListDictionary parameters = new ListDictionary();

					// Add in parameters
					parameters.Add(TktLevelsSchema.DId.FieldName, DId);
					parameters.Add(TktLevelsSchema.TintLevel.FieldName, TintLevel);

					return base.LoadFromSql(this.SchemaStoredProcedureWithSeparator + "GetTktLevels", parameters, CommandType.StoredProcedure);

				case CommandType.Text:
					this.Query.ClearAll();
					this.Where.WhereClauseReset();
					this.Where.DId.Value = DId;
					this.Where.TintLevel.Value = TintLevel;
					return this.Query.Load();

				default:
					throw new ArgumentException("Invalid CommandType", "commandType");
			}
		}
	
		/// <summary>
		/// Loads all records from the table.
		/// </summary>
		/// <returns>A Boolean indicating success or failure of the query</returns>
		public bool LoadAll()
		{
			switch(this.DefaultCommandType)
			{
				case CommandType.StoredProcedure:
					return base.LoadFromSql(this.SchemaStoredProcedureWithSeparator + "GetAllTktLevels", null, CommandType.StoredProcedure);

				case CommandType.Text:
					this.Query.ClearAll();
					this.Where.WhereClauseReset();
					return this.Query.Load();

				default:
					throw new ArgumentException("Invalid CommandType", "commandType");
			}
		}

		/// <summary>
		/// Adds a new record to the internal table.
		/// </summary>
		public override void AddNew()
		{
			base.AddNew();
		}

		protected override DbCommand GetInsertCommand(CommandType commandType)
		{	
			DbCommand dbCommand;

			// Create the Database object, using the default database service. The
			// default database service is determined through configuration.
			Database db = GetDatabase();

			switch(commandType)
			{
				case CommandType.StoredProcedure:
					string sqlCommand = this.SchemaStoredProcedureWithSeparator + "AddTktLevels";
					dbCommand = db.GetStoredProcCommand(sqlCommand);

					CreateParameters(db, dbCommand);
					
					return dbCommand;

				case CommandType.Text:
					this.Query.ClearAll();
					this.Where.WhereClauseReset();
					foreach(SchemaItem item in this.SchemaEntries)
					{
						if (!(item.IsAutoKey || item.IsComputed))
						{
							this.Query.AddInsertColumn(item);
						}
					}
					dbCommand = this.Query.GetInsertCommandWrapper();

					dbCommand.Parameters.Clear();
					CreateParameters(db, dbCommand);
					
					return dbCommand;

				default:
					throw new ArgumentException("Invalid CommandType", "commandType");
			}
		}

		protected override DbCommand GetUpdateCommand(CommandType commandType)
		{
            DbCommand dbCommand;

			// Create the Database object, using the default database service. The
			// default database service is determined through configuration.
			Database db = GetDatabase();

			switch(commandType)
			{
				case CommandType.StoredProcedure:
					string sqlCommand = this.SchemaStoredProcedureWithSeparator + "UpdateTktLevels";
					dbCommand = db.GetStoredProcCommand(sqlCommand);

					CreateParameters(db, dbCommand);
					
					return dbCommand;

				case CommandType.Text:
					this.Query.ClearAll();
					foreach(SchemaItem item in this.SchemaEntries)
					{
						if (!(item.IsAutoKey || item.IsComputed))
						{
							this.Query.AddUpdateColumn(item);
						}
					}

					this.Where.WhereClauseReset();
					dbCommand = this.Query.GetUpdateCommandWrapper();

					dbCommand.Parameters.Clear();
					CreateParameters(db, dbCommand);
					
					return dbCommand;

				default:
					throw new ArgumentException("Invalid CommandType", "commandType");
			}
		}

		protected override DbCommand GetDeleteCommand(CommandType commandType)
		{
            DbCommand dbCommand;

			// Create the Database object, using the default database service. The
			// default database service is determined through configuration.
			Database db = GetDatabase();

			switch(commandType)
			{
				case CommandType.StoredProcedure:
					string sqlCommand = this.SchemaStoredProcedureWithSeparator + "DeleteTktLevels";
					dbCommand = db.GetStoredProcCommand(sqlCommand);
					db.AddInParameter(dbCommand, "DId", DbType.Int32, "DId", DataRowVersion.Current);
					db.AddInParameter(dbCommand, "tintLevel", DbType.Byte, "tintLevel", DataRowVersion.Current);
					
					return dbCommand;

				case CommandType.Text:
					this.Query.ClearAll();
					this.Where.WhereClauseReset();
					this.Where.DId.Operator = WhereParameter.Operand.Equal;
					this.Where.TintLevel.Operator = WhereParameter.Operand.Equal;
					dbCommand = this.Query.GetDeleteCommandWrapper();

					dbCommand.Parameters.Clear();
					db.AddInParameter(dbCommand, "DId", DbType.Int32, "DId", DataRowVersion.Current);
					db.AddInParameter(dbCommand, "tintLevel", DbType.Byte, "tintLevel", DataRowVersion.Current);
					
					return dbCommand;

				default:
					throw new ArgumentException("Invalid CommandType", "commandType");
			}
		}

		private void CreateParameters(Database db, DbCommand dbCommand)
		{
			db.AddInParameter(dbCommand, "DId", DbType.Int32, "DId", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "tintLevel", DbType.Byte, "tintLevel", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "Description", DbType.AnsiString, "Description", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "bitDefault", DbType.Boolean, "bitDefault", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "intLastResortId", DbType.Int32, "intLastResortId", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "tintRoutingType", DbType.Byte, "tintRoutingType", DataRowVersion.Current);
			db.AddInParameter(dbCommand, "LevelName", DbType.AnsiString, "LevelName", DataRowVersion.Current);
		}
		
		#region Properties
		public virtual int DId
		{
			get
			{
				return this.GetInteger(TktLevelsSchema.DId.FieldName);
	    	}
			set
			{
				this.SetInteger(TktLevelsSchema.DId.FieldName, value);
			}
		}
		public virtual byte TintLevel
		{
			get
			{
				return this.GetByte(TktLevelsSchema.TintLevel.FieldName);
	    	}
			set
			{
				this.SetByte(TktLevelsSchema.TintLevel.FieldName, value);
			}
		}
		public virtual string Description
		{
			get
			{
				return this.GetString(TktLevelsSchema.Description.FieldName);
	    	}
			set
			{
				this.SetString(TktLevelsSchema.Description.FieldName, value);
			}
		}
		public virtual bool BitDefault
		{
			get
			{
				return this.GetBoolean(TktLevelsSchema.BitDefault.FieldName);
	    	}
			set
			{
				this.SetBoolean(TktLevelsSchema.BitDefault.FieldName, value);
			}
		}
		public virtual int IntLastResortId
		{
			get
			{
				return this.GetInteger(TktLevelsSchema.IntLastResortId.FieldName);
	    	}
			set
			{
				this.SetInteger(TktLevelsSchema.IntLastResortId.FieldName, value);
			}
		}
		public virtual byte TintRoutingType
		{
			get
			{
				return this.GetByte(TktLevelsSchema.TintRoutingType.FieldName);
	    	}
			set
			{
				this.SetByte(TktLevelsSchema.TintRoutingType.FieldName, value);
			}
		}
		public virtual string LevelName
		{
			get
			{
				return this.GetString(TktLevelsSchema.LevelName.FieldName);
	    	}
			set
			{
				this.SetString(TktLevelsSchema.LevelName.FieldName, value);
			}
		}

		public override string TableName
		{
			get { return "TktLevels"; }
		}
		
		#endregion		
		
		#region String Properties
	
		public virtual string s_DId
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.DId.FieldName) ? string.Empty : base.GetIntegerAsString(TktLevelsSchema.DId.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.DId.FieldName);
				else
					this.DId = base.SetIntegerAsString(TktLevelsSchema.DId.FieldName, value);
			}
		}

		public virtual string s_TintLevel
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.TintLevel.FieldName) ? string.Empty : base.GetByteAsString(TktLevelsSchema.TintLevel.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.TintLevel.FieldName);
				else
					this.TintLevel = base.SetByteAsString(TktLevelsSchema.TintLevel.FieldName, value);
			}
		}

		public virtual string s_Description
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.Description.FieldName) ? string.Empty : base.GetStringAsString(TktLevelsSchema.Description.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.Description.FieldName);
				else
					this.Description = base.SetStringAsString(TktLevelsSchema.Description.FieldName, value);
			}
		}

		public virtual string s_BitDefault
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.BitDefault.FieldName) ? string.Empty : base.GetBooleanAsString(TktLevelsSchema.BitDefault.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.BitDefault.FieldName);
				else
					this.BitDefault = base.SetBooleanAsString(TktLevelsSchema.BitDefault.FieldName, value);
			}
		}

		public virtual string s_IntLastResortId
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.IntLastResortId.FieldName) ? string.Empty : base.GetIntegerAsString(TktLevelsSchema.IntLastResortId.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.IntLastResortId.FieldName);
				else
					this.IntLastResortId = base.SetIntegerAsString(TktLevelsSchema.IntLastResortId.FieldName, value);
			}
		}

		public virtual string s_TintRoutingType
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.TintRoutingType.FieldName) ? string.Empty : base.GetByteAsString(TktLevelsSchema.TintRoutingType.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.TintRoutingType.FieldName);
				else
					this.TintRoutingType = base.SetByteAsString(TktLevelsSchema.TintRoutingType.FieldName, value);
			}
		}

		public virtual string s_LevelName
	    {
			get
	        {
				return this.IsColumnNull(TktLevelsSchema.LevelName.FieldName) ? string.Empty : base.GetStringAsString(TktLevelsSchema.LevelName.FieldName);
			}
			set
	        {
				if(string.Empty == value)
					this.SetColumnNull(TktLevelsSchema.LevelName.FieldName);
				else
					this.LevelName = base.SetStringAsString(TktLevelsSchema.LevelName.FieldName, value);
			}
		}


		#endregion		
	
		#region Where Clause
		public class WhereClause
		{
			public WhereClause(EasyObject entity)
			{
				this._entity = entity;
			}
			
			public TearOffWhereParameter TearOff
			{
				get
				{
					if(_tearOff == null)
					{
						_tearOff = new TearOffWhereParameter(this);
					}

					return _tearOff;
				}
			}

			#region TearOff's
			public class TearOffWhereParameter
			{
				public TearOffWhereParameter(WhereClause clause)
				{
					this._clause = clause;
				}
				
				
				public WhereParameter DId
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.DId);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter TintLevel
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.TintLevel);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter Description
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.Description);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter BitDefault
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.BitDefault);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter IntLastResortId
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.IntLastResortId);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter TintRoutingType
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.TintRoutingType);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}

				public WhereParameter LevelName
				{
					get
					{
							WhereParameter wp = new WhereParameter(TktLevelsSchema.LevelName);
							this._clause._entity.Query.AddWhereParameter(wp);
							return wp;
					}
				}


				private WhereClause _clause;
			}
			#endregion
		
			public WhereParameter DId
		    {
				get
		        {
					if(_DId_W == null)
	        	    {
						_DId_W = TearOff.DId;
					}
					return _DId_W;
				}
			}

			public WhereParameter TintLevel
		    {
				get
		        {
					if(_TintLevel_W == null)
	        	    {
						_TintLevel_W = TearOff.TintLevel;
					}
					return _TintLevel_W;
				}
			}

			public WhereParameter Description
		    {
				get
		        {
					if(_Description_W == null)
	        	    {
						_Description_W = TearOff.Description;
					}
					return _Description_W;
				}
			}

			public WhereParameter BitDefault
		    {
				get
		        {
					if(_BitDefault_W == null)
	        	    {
						_BitDefault_W = TearOff.BitDefault;
					}
					return _BitDefault_W;
				}
			}

			public WhereParameter IntLastResortId
		    {
				get
		        {
					if(_IntLastResortId_W == null)
	        	    {
						_IntLastResortId_W = TearOff.IntLastResortId;
					}
					return _IntLastResortId_W;
				}
			}

			public WhereParameter TintRoutingType
		    {
				get
		        {
					if(_TintRoutingType_W == null)
	        	    {
						_TintRoutingType_W = TearOff.TintRoutingType;
					}
					return _TintRoutingType_W;
				}
			}

			public WhereParameter LevelName
		    {
				get
		        {
					if(_LevelName_W == null)
	        	    {
						_LevelName_W = TearOff.LevelName;
					}
					return _LevelName_W;
				}
			}

			private WhereParameter _DId_W = null;
			private WhereParameter _TintLevel_W = null;
			private WhereParameter _Description_W = null;
			private WhereParameter _BitDefault_W = null;
			private WhereParameter _IntLastResortId_W = null;
			private WhereParameter _TintRoutingType_W = null;
			private WhereParameter _LevelName_W = null;

			public void WhereClauseReset()
			{
				_DId_W = null;
				_TintLevel_W = null;
				_Description_W = null;
				_BitDefault_W = null;
				_IntLastResortId_W = null;
				_TintRoutingType_W = null;
				_LevelName_W = null;

				this._entity.Query.FlushWhereParameters();

			}
	
			private EasyObject _entity;
			private TearOffWhereParameter _tearOff;
			
		}
	
		public WhereClause Where
		{
			get
			{
				if(_whereClause == null)
				{
					_whereClause = new WhereClause(this);
				}
		
				return _whereClause;
			}
		}
		
		private WhereClause _whereClause = null;	
		#endregion
		
		#region Aggregate Clause
		public class AggregateClause
		{
			public AggregateClause(EasyObject entity)
			{
				this._entity = entity;
			}
			
			public TearOffAggregateParameter TearOff
			{
				get
				{
					if(_tearOff == null)
					{
						_tearOff = new TearOffAggregateParameter(this);
					}

					return _tearOff;
				}
			}

			#region TearOff's
			public class TearOffAggregateParameter
			{
				public TearOffAggregateParameter(AggregateClause clause)
				{
					this._clause = clause;
				}
				
				
				public AggregateParameter DId
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.DId);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter TintLevel
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.TintLevel);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter Description
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.Description);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter BitDefault
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.BitDefault);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter IntLastResortId
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.IntLastResortId);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter TintRoutingType
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.TintRoutingType);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}

				public AggregateParameter LevelName
				{
					get
					{
							AggregateParameter ap = new AggregateParameter(TktLevelsSchema.LevelName);
							this._clause._entity.Query.AddAggregateParameter(ap);
							return ap;
					}
				}


				private AggregateClause _clause;
			}
			#endregion
		
			public AggregateParameter DId
		    {
				get
		        {
					if(_DId_W == null)
	        	    {
						_DId_W = TearOff.DId;
					}
					return _DId_W;
				}
			}

			public AggregateParameter TintLevel
		    {
				get
		        {
					if(_TintLevel_W == null)
	        	    {
						_TintLevel_W = TearOff.TintLevel;
					}
					return _TintLevel_W;
				}
			}

			public AggregateParameter Description
		    {
				get
		        {
					if(_Description_W == null)
	        	    {
						_Description_W = TearOff.Description;
					}
					return _Description_W;
				}
			}

			public AggregateParameter BitDefault
		    {
				get
		        {
					if(_BitDefault_W == null)
	        	    {
						_BitDefault_W = TearOff.BitDefault;
					}
					return _BitDefault_W;
				}
			}

			public AggregateParameter IntLastResortId
		    {
				get
		        {
					if(_IntLastResortId_W == null)
	        	    {
						_IntLastResortId_W = TearOff.IntLastResortId;
					}
					return _IntLastResortId_W;
				}
			}

			public AggregateParameter TintRoutingType
		    {
				get
		        {
					if(_TintRoutingType_W == null)
	        	    {
						_TintRoutingType_W = TearOff.TintRoutingType;
					}
					return _TintRoutingType_W;
				}
			}

			public AggregateParameter LevelName
		    {
				get
		        {
					if(_LevelName_W == null)
	        	    {
						_LevelName_W = TearOff.LevelName;
					}
					return _LevelName_W;
				}
			}

			private AggregateParameter _DId_W = null;
			private AggregateParameter _TintLevel_W = null;
			private AggregateParameter _Description_W = null;
			private AggregateParameter _BitDefault_W = null;
			private AggregateParameter _IntLastResortId_W = null;
			private AggregateParameter _TintRoutingType_W = null;
			private AggregateParameter _LevelName_W = null;

			public void AggregateClauseReset()
			{
				_DId_W = null;
				_TintLevel_W = null;
				_Description_W = null;
				_BitDefault_W = null;
				_IntLastResortId_W = null;
				_TintRoutingType_W = null;
				_LevelName_W = null;

				this._entity.Query.FlushAggregateParameters();

			}
	
			private EasyObject _entity;
			private TearOffAggregateParameter _tearOff;
			
		}
	
		public AggregateClause Aggregate
		{
			get
			{
				if(_aggregateClause == null)
				{
					_aggregateClause = new AggregateClause(this);
				}
		
				return _aggregateClause;
			}
		}
		
		private AggregateClause _aggregateClause = null;	
		#endregion
	}
}
