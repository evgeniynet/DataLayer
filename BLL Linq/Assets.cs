using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System;


namespace lib.bwa.bigWebDesk.LinqBll
{
	public class Assets
	{
		public static int GetStatus(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, string Name)
		{
			if (string.IsNullOrEmpty(Name)) Name = "Active";
			IQueryable<int> IDs;
			IDs = from d in dc.AssetStatus where d.VchStatus == Name select d.Id;
			foreach (int ID in IDs) return ID;

			throw new Exception("Asset Status name must be specify");
		}

		public static int? GetMakeId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name, int? AssetTypeId)
		{
			if (string.IsNullOrEmpty(Name)) return null;
			IQueryable<int> IDs;
			IDs = from d in dc.AssetMakes where d.DepartmentId == DepartmentID && d.Make == Name select d.Id;
			foreach (int ID in IDs) return ID;

			if (AssetTypeId == null) return null;

			lib.bwa.bigWebDesk.LinqBll.Context.AssetMakes data = new lib.bwa.bigWebDesk.LinqBll.Context.AssetMakes();
			data.DepartmentId = DepartmentID;
			data.Make = Name;
			data.TypeId = (int)AssetTypeId;

			dc.AssetMakes.InsertOnSubmit(data);
			dc.SubmitChanges();
			return data.Id;
		}

		public static int? GetModelId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name, int? AssetMakeId)
		{
			if (string.IsNullOrEmpty(Name)) return null;
			IQueryable<int> IDs;
			IDs = from d in dc.AssetModels where d.DepartmentId == DepartmentID && d.Model == Name select d.Id;
			foreach (int ID in IDs) return ID;

			if (AssetMakeId == null) return null;

			lib.bwa.bigWebDesk.LinqBll.Context.AssetModels data = new lib.bwa.bigWebDesk.LinqBll.Context.AssetModels();
			data.DepartmentId = DepartmentID;
			data.Model = Name;
			data.MakeId = (int)AssetMakeId;

			dc.AssetModels.InsertOnSubmit(data);
			dc.SubmitChanges();
			return data.Id;
		}

		public static int? GetAccountId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name)
		{
			if (string.IsNullOrEmpty(Name)) return null;
			IQueryable<int> IDs;
			IDs = from d in dc.Accounts where d.DId == DepartmentID && d.VchName == Name select d.Id;
			foreach (int ID in IDs) return ID;

			return null;
		}

		public static int? GetLoginCompanyJuncId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name)
		{
			if (string.IsNullOrEmpty(Name)) return null;
			IQueryable<int> IDs;
			IDs =
				from j in dc.Tbl_LoginCompanyJunc
				join l in dc.Tbl_Logins on j.Login_id equals l.Id into jl
				join t in dc.Tbl_UserType on j.UserType_Id equals t.Id into jt

				from sl in jl.DefaultIfEmpty()
				from st in jt.DefaultIfEmpty()

				where j.Company_id == DepartmentID &
					(((sl.FirstName + " " + sl.LastName + ", " + st.Name) == Name) |
					((sl.FirstName + " " + sl.LastName) == Name) |
					((sl.FirstName + " " + sl.LastName + ", " + sl.Email) == Name))
				select j.Id;

			foreach (int ID in IDs) return ID;
			return null;
		}

		public static int? GetLocationId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name, int? AccountId)
		{
			if (string.IsNullOrEmpty(Name) || AccountId == null) return null;
			IQueryable<int> IDs;
			IDs = from d in dc.Locations
				  where d.DId == DepartmentID && d.Name == Name && d.AccountId == AccountId
				  select d.Id;

			foreach (int ID in IDs) return ID;
			return null;
		}

		public static int? GetVendorId(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentID, string Name)
		{
			if (string.IsNullOrEmpty(Name)) return null;

			IQueryable<int> IDs;
			IDs = from d in dc.Tbl_vendors where d.Company_id == DepartmentID && d.Name == Name select d.Id;
			foreach (int ID in IDs) return ID;

			return null;
		}

        public static Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>>[] ListDuplicatedInfo(Guid OrgId, int DepartmentId)
		{
			lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
			dc.CommandTimeout = 333;
			lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult [] DupAssetsEnum = dc.Sp_SelectDupeAssets(DepartmentId).ToArray();


 
			Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>> DupAssets = new Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>>();
			int Count = 0;
			foreach (var a in DupAssetsEnum)
			{
				if (a.SerialNumber == string.Empty) a.SerialNumber = null;
				if (a.Unique1 == string.Empty) a.Unique1 = null;
				if (a.Unique2 == string.Empty) a.Unique2 = null;
				if (a.Unique3 == string.Empty) a.Unique3 = null;
				if (a.Unique4 == string.Empty) a.Unique4 = null;
				if (a.Unique5 == string.Empty) a.Unique5 = null;
				if (a.Unique6 == string.Empty) a.Unique6 = null;
				if (a.Unique7 == string.Empty) a.Unique7 = null;

				AddUnique(0, a.SerialNumber, DupAssets, a);
				AddUnique(1, a.Unique1, DupAssets, a);
				AddUnique(2, a.Unique2, DupAssets, a);
				AddUnique(3, a.Unique3, DupAssets, a);
				AddUnique(4, a.Unique4, DupAssets, a);
				AddUnique(5, a.Unique5, DupAssets, a);
				AddUnique(6, a.Unique6, DupAssets, a);
				AddUnique(7, a.Unique7, DupAssets, a);
				Count++;
			}

			Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>>[] EndDupAssets = new Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>>[8];
			for (int i = 0; i < 8; i++) EndDupAssets[i] = new Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>>();

			foreach (string FullName in DupAssets.Keys)
			{
				string UniqueValue = FullName.Substring(1);
				int UniqueIndex = int.Parse(FullName.Substring(0, 1));
				EndDupAssets[UniqueIndex].Add(UniqueValue, DupAssets[FullName]);
			}

			return EndDupAssets;
		}

		private static void AddUnique(int n, string uniq, Dictionary<string, List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>> DupAssets, lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult ai)
		{
			if (string.IsNullOrEmpty(uniq)) return;
			string FullName = n + uniq;
			List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult> lai;
			if (DupAssets.Keys.Contains(FullName))
			{
			   lai = DupAssets[FullName];
			}
			else
			{
				lai = new List<lib.bwa.bigWebDesk.LinqBll.Context.Sp_SelectDupeAssetsResult>();
				DupAssets[FullName] = lai;
			}
			lai.Add(ai);
		}


        public static lib.bwa.bigWebDesk.LinqBll.Context.Assets Get(Guid OrgId, int DepartmentId, int AssetId)
		{
			lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);
			return dc.Assets.Where(a => a.Id == AssetId).FirstOrNull();
		}

		private static void CopyLogicalDrives(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int OldAssetId, int NewAssetId)
		{
			var OldLogicalDrives = (from d in dc.AssetComputerLogicalDrives where d.DepartmentId == DepartmentId && d.AssetId == OldAssetId select d).ToList();
			foreach (var d in OldLogicalDrives)
			{
				var NewD = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerLogicalDrives()
				{
					DepartmentId = DepartmentId,
					AssetId = NewAssetId,
					DeviceID = d.DeviceID,
					FileSystem = d.FileSystem,
					SizeGB = d.SizeGB,
					FreeSpaceGB = d.FreeSpaceGB,
					Description = d.Description,
					VolumeName = d.VolumeName,
					VolumeSerial = d.VolumeSerial,
					Compressed = d.Compressed,
					DriveType = d.DriveType

				};
				dc.AssetComputerLogicalDrives.InsertOnSubmit(NewD);
			}
		}

		private static void CopyPrinters(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int OldAssetId, int NewAssetId)
		{
			var OldPrinters = (from d in dc.AssetComputerPrinters where d.DepartmentId == DepartmentId && d.AssetId == OldAssetId select d).ToList();
			foreach (var d in OldPrinters)
			{
				var NewD = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerPrinters()
				{
					DepartmentId = DepartmentId,
					AssetId = NewAssetId,
					PrinterDriver = d.PrinterDriver,
					PrinterName = d.PrinterName,
					PrinterPort = d.PrinterPort

				};
				dc.AssetComputerPrinters.InsertOnSubmit(NewD);
			}
		}

		private static void CopyProcessors(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int OldAssetId, int NewAssetId)
		{
			var OldProcessors = (from d in dc.AssetComputerProcessors where d.DepartmentId == DepartmentId && d.AssetId == OldAssetId select d).ToList();
			foreach (var d in OldProcessors)
			{
				var NewD = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerProcessors()
				{
					DepartmentId = DepartmentId,
					AssetId = NewAssetId,
					CPUType = d.CPUType,
					CPUSerial = d.CPUSerial,
					CPUVendor = d.CPUVendor,
					CPUSpeedMHz = d.CPUSpeedMHz,
					CPUClass = d.CPUClass,
					CurrentClockSpeed = d.CurrentClockSpeed,
					UniqueId = d.UniqueId,
					Description = d.Description,
					Version = d.Version,
					L2CachSize = d.L2CachSize,
					ExtClock = d.ExtClock,
					CurrentVoltage = d.CurrentVoltage,
					DeviceID = d.DeviceID
				};
				dc.AssetComputerProcessors.InsertOnSubmit(NewD);
			}
		}

		private static void CopySoftwares(lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, int DepartmentId, int OldAssetId, int NewAssetId)
		{
			var OldSoftwares = (from d in dc.AssetComputerSoftwares where d.DepartmentId == DepartmentId && d.AssetId == OldAssetId select d).ToList();
			foreach (var d in OldSoftwares)
			{
				var NewD = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerSoftwares()
				{
					DepartmentId = DepartmentId,
					AssetId = NewAssetId,
					SoftwareName = d.SoftwareName,
					SoftwarePublisher = d.SoftwarePublisher,
					SoftwareVersion = d.SoftwareVersion
				};
				dc.AssetComputerSoftwares.InsertOnSubmit(NewD);
			}
		}


        public static string Merge(Guid OrgId, int DepartmentId, List<lib.bwa.bigWebDesk.LinqBll.Context.Assets> OldAssets, lib.bwa.bigWebDesk.LinqBll.Context.Assets NewAsset, int? OldAssetComputerId)
		{            
			using (lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId))
			{
				int? InactiveMergedStatusID = (from s in dc.AssetStatus where s.VchStatus == "Deleted" select s.Id).FirstOrDefault();
				if (InactiveMergedStatusID == null || InactiveMergedStatusID == 0) return "Can not find 'Deleted' asset status";

				dc.Assets.InsertOnSubmit(NewAsset);
				dc.SubmitChanges();
				if (OldAssetComputerId != null)
				{
					var OldAC = (from ac in dc.AssetComputers where ac.DepartmentId == DepartmentId && ac.AssetId == OldAssetComputerId select ac).FirstOrNull();
					if (OldAC != null)
					{
						lib.bwa.bigWebDesk.LinqBll.Context.AssetComputers NewAC = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputers()
						{
							DepartmentId = DepartmentId,
							AssetId = NewAsset.Id,
							MotherboardSerial = OldAC.MotherboardSerial,
							BiosSerial = OldAC.BiosSerial,
							OSSerial = OldAC.OSSerial,
							RegisteredUser = OldAC.RegisteredUser,
							OperatingSystem = OldAC.OperatingSystem,
							RamMbytes = OldAC.RamMbytes,
							VideoDescription = OldAC.VideoDescription,
							VideoMemoryMbytes = OldAC.VideoMemoryMbytes,
							VideoHResolution = OldAC.VideoHResolution,
							VideoVResolution = OldAC.VideoVResolution,
							NetworkName = OldAC.NetworkName,
							NetworkDomain = OldAC.NetworkDomain,
							NetworkCard1IP = OldAC.NetworkCard1IP,
							NetworkCard1Mask = OldAC.NetworkCard1Mask,
							NetworkCard1Gate = OldAC.NetworkCard1Gate,
							NetworkCard1Address = OldAC.NetworkCard1Address,
							NetworkCard1Description = OldAC.NetworkCard1Description
						};

						dc.AssetComputers.InsertOnSubmit(NewAC);

						CopyLogicalDrives(dc, DepartmentId, (int)OldAssetComputerId, (int)NewAsset.Id);
						CopyPrinters(dc, DepartmentId, (int)OldAssetComputerId, (int)NewAsset.Id);
						CopyProcessors(dc, DepartmentId, (int)OldAssetComputerId, (int)NewAsset.Id);
						CopySoftwares(dc, DepartmentId, (int)OldAssetComputerId, (int)NewAsset.Id);
					}
				}

				foreach (lib.bwa.bigWebDesk.LinqBll.Context.Assets OldAsset in OldAssets)
				{
					var SchedTicketAssets = dc.SchedTicketAssets.Where(sta => sta.DId == DepartmentId && sta.AssetId == OldAsset.Id);
					foreach (var SchedTicketAsset in SchedTicketAssets) SchedTicketAsset.AssetId = NewAsset.Id;

					var TicketAssets = dc.TicketAssets.Where(ta => ta.DId == DepartmentId && ta.AssetId == OldAsset.Id);
					foreach (var TicketAsset in TicketAssets) TicketAsset.AssetId = NewAsset.Id;

					var Asset = dc.Assets.Where(a => a.Id == OldAsset.Id).FirstOrNull();
					Asset.StatusId = (int)InactiveMergedStatusID;
					Asset.MergedId = NewAsset.Id;
				}
				dc.SubmitChanges();
				return null;
			}
		}
	}
}