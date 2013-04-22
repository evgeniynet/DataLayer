using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel;
using System;

using bigWebApps.HelpDesk.WebApi.Soap.v1;

namespace lib.bwa.bigWebDesk.LinqBll
{
    public class AssetComputers
    {
        static public bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData[] GetAssetComputer(Guid OrgId, int DepartmentID, string MotherboardSerialNumber, int PageIndex, out int TotalPageNumber, int AssetsPageSize)
        {
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentID);

            var DBAssets = (
                from a in dc.Assets

                join AC in dc.AssetComputers on new { a.DepartmentId, AssetId = a.Id } equals new { AC.DepartmentId, AC.AssetId }

                join _c in dc.AssetCategories on new { a.DepartmentId, a.CategoryId } equals new { _c.DepartmentId, CategoryId = _c.Id } into __c
                join _t in dc.AssetTypes on new { a.DepartmentId, a.TypeId } equals new { _t.DepartmentId, TypeId = _t.Id } into __t
                join _s in dc.AssetStatus on a.StatusId equals _s.Id into __s
                join _m in dc.AssetMakes on new { a.DepartmentId, a.MakeId } equals new { _m.DepartmentId, MakeId = (int?)_m.Id } into __m
                join _M in dc.AssetModels on new { a.DepartmentId, a.ModelId } equals new { _M.DepartmentId, ModelId = (int?)_M.Id } into __M
                join _ac in dc.Accounts on new { a.DepartmentId, a.AccountId } equals new { DepartmentId = _ac.DId, AccountId = (int?)_ac.Id } into __ac
                join _l in dc.Locations on new { a.DepartmentId, a.LocationId } equals new { DepartmentId = _l.DId, LocationId = (int?)_l.Id } into __l
                join _ju in dc.Tbl_LoginCompanyJunc on new { a.DepartmentId, UpdatedById = a.IntUpdatedBy } equals new { DepartmentId = _ju.Company_id, UpdatedById = (int?)_ju.Id } into __ju
                join _jc in dc.Tbl_LoginCompanyJunc on new { a.DepartmentId, a.CheckedOutId } equals new { DepartmentId = _jc.Company_id, CheckedOutId = (int?)_jc.Id } into __jc
                join _jo in dc.Tbl_LoginCompanyJunc on new { a.DepartmentId, a.OwnerId } equals new { DepartmentId = _jo.Company_id, OwnerId = (int?)_jo.Id } into __jo
                join _v in dc.Tbl_vendors on a.VendorId equals _v.Id into __v
                join _w in dc.Tbl_vendors on a.WarrantyVendor equals _w.Id into __w

                from c in __c.DefaultIfEmpty()
                from t in __t.DefaultIfEmpty()
                from s in __s.DefaultIfEmpty()
                from m in __m.DefaultIfEmpty()
                from M in __M.DefaultIfEmpty()
                from ac in __ac.DefaultIfEmpty()
                from l in __l.DefaultIfEmpty()
                from v in __v.DefaultIfEmpty()
                from w in __w.DefaultIfEmpty()

                from sju in __ju.DefaultIfEmpty()
                join _ulu in dc.Tbl_Logins on sju.Login_id equals _ulu.Id into __ulu
                join _utu in dc.Tbl_UserType on sju.UserType_Id equals _utu.Id into __utu
                from julu in __ulu.DefaultIfEmpty()
                from utu in __utu.DefaultIfEmpty()

                from sjc in __jc.DefaultIfEmpty()
                join _ulc in dc.Tbl_Logins on sjc.Login_id equals _ulc.Id into __ulc
                join _utc in dc.Tbl_UserType on sjc.UserType_Id equals _utc.Id into __utc
                from julc in __ulc.DefaultIfEmpty()
                from utc in __utc.DefaultIfEmpty()

                from sjo in __jo.DefaultIfEmpty()
                join _ulo in dc.Tbl_Logins on sjo.Login_id equals _ulo.Id into __ulo
                join _uto in dc.Tbl_UserType on sjo.UserType_Id equals _uto.Id into __uto
                from ulo in __ulo.DefaultIfEmpty()
                from uto in __uto.DefaultIfEmpty()

                where a.DepartmentId == DepartmentID && t.AssetProfileId == 1 && (MotherboardSerialNumber == null || AC.MotherboardSerial == MotherboardSerialNumber)

                orderby a.Name

                select new
                {
                    a,

                    Category = c.Name,
                    Type = t.Name,
                    Status = s.VchStatus,
                    Make = m.Make,
                    Model = M.Model,
                    Account = ac.VchName,
                    AccountLocation = l.Name,
                    /*UpdatedBy = julu.FirstName + " " + julu.LastName + ", " + utu.Name,
                    CheckedOut = julc.FirstName + " " + julc.LastName + ", " + utc.Name,
                    Owner = ulo.FirstName + " " + ulo.LastName + ", " + uto.Name,*/
                    PurchaseVendor = v.Name,
                    WarrantyVendor = w.Name,

                    MotherboardSerial = AC.MotherboardSerial,
                    BiosSerial = AC.BiosSerial,
                    RegisteredUser = AC.RegisteredUser,
                    OperatingSystem = AC.OperatingSystem,
                    OSSerial = AC.OSSerial,
                    RamMbytes = AC.RamMbytes,
                    VideoDescription = AC.VideoDescription,
                    VideoMemoryMbytes = AC.VideoMemoryMbytes,
                    VideoHResolution = AC.VideoHResolution,
                    VideoVResolution = AC.VideoVResolution,
                    NetworkName = AC.NetworkName,
                    NetworkDomain = AC.NetworkDomain,
                    NetworkCard1IP = AC.NetworkCard1IP,
                    NetworkCard1Mask = AC.NetworkCard1Mask,
                    NetworkCard1Gate = AC.NetworkCard1Gate,
                    NetworkCard1Address = AC.NetworkCard1Address,
                    NetworkCard1Description = AC.NetworkCard1Description
                }).Skip(PageIndex * AssetsPageSize).ToList();

            TotalPageNumber = PageIndex + (DBAssets.Count + AssetsPageSize - 1) / AssetsPageSize;
            DBAssets = DBAssets.Take(AssetsPageSize).ToList();

            //bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData[] ret = bigWebApps.HelpDesk.WebApi.Utils.CopyProperties<bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData>(DBAssets);
            List<AssetComputerData> lr = new List<AssetComputerData>();
            foreach (var a in DBAssets)
            {
                AssetComputerData r = new AssetComputerData();
                lr.Add(r);

                r.Account = a.Account;
                r.AccountLocation = a.AccountLocation;
                r.BiosSeriaNumber = a.BiosSerial;
                r.Category = a.Category;
                r.Make = a.Make;
                r.Model = a.Model;
                r.MotherboardSerialNumber = a.MotherboardSerial;
                r.NetworkCard1Address = a.NetworkCard1Address;
                r.NetworkCard1Description = a.NetworkCard1Description;
                r.NetworkCard1Gate = a.NetworkCard1Gate;
                r.NetworkCard1IP = a.NetworkCard1IP;
                r.NetworkCard1Mask = a.NetworkCard1Mask;
                r.NetworkDomain = a.NetworkDomain;
                r.NetworkName = a.NetworkName;
                r.OperatingSystem = a.OperatingSystem;
                r.OSSerial = a.OSSerial;
                r.PurchaseVendor = a.PurchaseVendor;
                r.RamMbytes = a.RamMbytes;
                r.RegisteredUser = a.RegisteredUser;
                r.Status = a.Status;
                r.Type = a.Type;
                r.VideoDescription = a.VideoDescription;
                r.VideoHResolution = a.VideoHResolution;
                r.VideoMemoryMbytes = a.VideoMemoryMbytes;
                r.VideoVResolution = a.VideoVResolution;
                r.WarrantyVendor = a.WarrantyVendor;

                r.Active = a.a.Active;
                r.AssetName = a.a.Name;
                r.AssetNumber = a.a.AssetNumber==null?0:(int)a.a.AssetNumber;
                r.AssetSort = a.a.AssetSort;
                r.DateAquired = a.a.DateAquired;
                r.DateDeployed = a.a.DateDeployed;
                r.DateDisposed = a.a.DateDisposed;
                r.DateEntered = a.a.DateEntered;
                r.DateOutOfService = a.a.DateOutOfService;
                r.DatePurchased = a.a.DatePurchased;
                r.DateReceived = a.a.DateReceived;
                r.DateUpdated = a.a.DtUpdated;
                r.Description = a.a.Description;
                r.FundingCode = a.a.FundingCode;
                r.FundingSource = a.a.FundingSource;
                r.LaborWarrantyLength = a.a.LaborWarrantyLength;
                r.Notes = a.a.Notes;
                r.PartsWarrantyLength = a.a.PartsWarrantyLength;
                r.PONumber = a.a.PONumber;
                r.Room = a.a.Room;
                r.ValueCurrent = a.a.ValueCurrent;
                r.ValueDepreciated = a.a.ValueDepreciated;
                r.ValueDisposalCost = a.a.DisposalCost;
                r.ValuePurchaseCost = a.a.Value;
                r.ValueReplacement = a.a.ValueReplacement;
                r.ValueSalvage = a.a.ValueSalvage;

                GetAssetComputerArrays(DepartmentID, dc, r);

            }
            /*for (int i = 0; i < ret.Length; i++)
            {
                bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData r = ret[i];
                var a = DBAssets[i];

                bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData d = ret[i];
                //d.MotherboardSerialNumber = DBAssets[i].MotherboardSerial;
                //d.BiosSeriaNumber = DBAssets[i].BiosSerial;
                GetAssetComputerArrays(DepartmentID, dc, d);
            }*/

            return lr.ToArray();
        }

        static void GetAssetComputerArrays(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerData d)
        {
            d.LogicalDrives = (
                            from ld in dc.AssetComputerLogicalDrives
                            where ld.DepartmentId == DepartmentID && ld.AssetId == d.AssetNumber
                            select new bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerLogicalDriveData()
                            {
                                DeviceID = ld.DeviceID,
                                FileSystem = ld.FileSystem,
                                SizeGB = (float?)ld.SizeGB,
                                FreeSpaceGB = (float?)ld.FreeSpaceGB,
                                Description = ld.Description,
                                VolumeName = ld.VolumeName,
                                VolumeSerial = ld.VolumeSerial,
                                Compressed = ld.Compressed,
                                DriveType = ld.DriveType
                            }
                        ).ToArray();

            d.Processors = (
                    from p in dc.AssetComputerProcessors
                    where p.DepartmentId == DepartmentID && p.AssetId == d.AssetNumber
                    select new bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerProcessorData()
                    {
                        CPUType = p.CPUType,
                        CPUSerial = p.CPUSerial,
                        CPUVendor = p.CPUVendor,
                        CPUSpeedMHz = p.CPUSpeedMHz,
                        CPUClass = p.CPUClass,
                        CurrentClockSpeed = p.CurrentClockSpeed,
                        UniqueId = p.UniqueId,
                        Description = p.Description,
                        Version = p.Version,
                        L2CachSize = p.L2CachSize,
                        ExtClock = p.ExtClock,
                        CurrentVoltage = (float?)p.CurrentVoltage,
                        DeviceID = p.DeviceID,
                    }
                ).ToArray();

            d.Printers = (
                   from p in dc.AssetComputerPrinters
                   where p.DepartmentId == DepartmentID && p.AssetId == d.AssetNumber
                   select new bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerPrinterData()
                   {
                       PrinterName = p.PrinterName,
                       PrinterDriver = p.PrinterDriver,
                       PrinterPort = p.PrinterPort
                   }
               ).ToArray();

            d.Softwares = (
                   from p in dc.AssetComputerSoftwares
                   where p.DepartmentId == DepartmentID && p.AssetId == d.AssetNumber
                   select new bigWebApps.HelpDesk.WebApi.Soap.v1.AssetComputerSoftwareData()
                   {
                       SoftwareName = p.SoftwareName,
                       SoftwarePublisher = p.SoftwarePublisher,
                       SoftwareVersion = p.SoftwareVersion
                   }
               ).ToArray();
        }

        static public int AddImportLog(Guid OrgId, int DepartmentId, int ImportedBy, System.Collections.Generic.List<int> ImportedAssteId)
        {
            if(ImportedAssteId==null || ImportedAssteId.Count<1) return 0;
            DateTime DTUpdated = DateTime.UtcNow;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentId);

            int? ExistId = (from i in dc.AssetImports where i.DepartmentId==DepartmentId select (int?)i.ImportId).Max();
            int ImportId = 1;
            if(ExistId!=null && (int)ExistId>0) ImportId=((int)ExistId)+1;

            foreach (int id in ImportedAssteId)
            {
                lib.bwa.bigWebDesk.LinqBll.Context.AssetImports imp = new lib.bwa.bigWebDesk.LinqBll.Context.AssetImports()
                {
                    AssetId = id,
                    DepartmentId = DepartmentId,
                    DtUpdated = DTUpdated,
                    ImportId = ImportId,
                    UpdatedBy = ImportedBy
                };
                dc.AssetImports.InsertOnSubmit(imp);
            }
            dc.SubmitChanges();
            return ImportId;
        }

        static public string LastLog;
        static public int SaveAssetComputer(Guid OrgId, int DepartmentID, AssetComputerData data, bool AddNewOnly, int? AccountId, int? LocationId, ref int? CategoryId, ref int? TypeId, DateTime? DTUpdated)
        {
            LastLog = "SaveAssetComputer ";
            if(DTUpdated==null) DTUpdated = DateTime.UtcNow;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentID);

            bool isNew = true;
            lib.bwa.bigWebDesk.LinqBll.Context.Assets A = null;

            if(data.AssetNumber>1)
            {
                A = (
                    from a in dc.Assets
                    where a.DepartmentId == DepartmentID && a.AssetNumber==data.AssetNumber
                    select a
                    ).FirstOrNull();
                isNew = A == null;
            }

            if (isNew && data.MotherboardSerialNumber != null)
            {
                A = (
                    from a in dc.Assets
                    join ac in dc.AssetComputers on new { a.DepartmentId, AssetId = a.Id } equals new { ac.DepartmentId, ac.AssetId }
                    join t in dc.AssetTypes on new { a.DepartmentId, a.TypeId } equals new { t.DepartmentId, TypeId = t.Id }
                    where a.DepartmentId == DepartmentID && t.AssetProfileId == 1 && a.Unique7 == data.MotherboardSerialNumber
                    select a
                    ).FirstOrNull();
                isNew = A == null;
            }

            if (isNew)
            {
                A = (
                    from a in dc.Assets
                    join t in dc.AssetTypes on new { a.DepartmentId, a.TypeId } equals new { t.DepartmentId, TypeId = t.Id }
                    where a.DepartmentId == DepartmentID && t.AssetProfileId == 1 && a.SerialNumber == data.PCSerialNumber
                    select a
                    ).FirstOrNull();
                isNew = A == null;
            }

            LastLog += " isNew=" + isNew;

            if (AddNewOnly && !isNew) return 0;

            if (isNew)
            {
                A = new lib.bwa.bigWebDesk.LinqBll.Context.Assets();
                A.AssetGUID = Guid.NewGuid();
                int? MaxNumber = (from a in dc.Asset where a.DId==DepartmentID select (int?)a.AssetNumber).Max();
                if (MaxNumber == null || (int)MaxNumber < 1) A.AssetNumber = 1;
                else A.AssetNumber = (int)MaxNumber + 1;
            }

            if (data.AssetSort>0) A.AssetSort = data.AssetSort;
            if (data.DateAquired != null) A.DateAquired = data.DateAquired;
            if (data.DateDeployed != null) A.DateDeployed = data.DateDeployed;
            if (data.DateDisposed != null) A.DateDisposed = data.DateDisposed;
            if (data.DateEntered != null) A.DateEntered = data.DateEntered;
            if (data.DateEntered != null) A.DateEntered = data.DateEntered;
            if (data.DateOutOfService != null) A.DateOutOfService = data.DateOutOfService;
            if (data.DatePurchased != null) A.DatePurchased = data.DatePurchased;
            if (data.DateReceived != null) A.DateReceived = data.DateReceived;
            if (data.Description != null) A.Description = data.Description;
            if (data.FundingCode != null) A.FundingCode = data.FundingCode;
            if (data.FundingSource != null) A.FundingSource = data.FundingSource;
            if (data.LaborWarrantyLength != null) A.LaborWarrantyLength = data.LaborWarrantyLength;
            if (data.Notes != null) A.Notes = data.Notes;
            if (data.PartsWarrantyLength != null) A.PartsWarrantyLength = data.PartsWarrantyLength;
            if (data.PONumber != null) A.PONumber = data.PONumber;
            if (data.Room != null) A.Room = data.Room;
            if (data.ValueCurrent != null) A.ValueCurrent = data.ValueCurrent;
            if (data.ValueDepreciated != null) A.ValueDepreciated = data.ValueDepreciated;
            if (data.ValueReplacement != null) A.ValueReplacement = data.ValueReplacement;
            if (data.ValueSalvage != null) A.ValueSalvage = data.ValueSalvage;
            
            A.Unique7 = data.MotherboardSerialNumber;
            A.SerialNumber = data.PCSerialNumber;
            A.DtUpdated = (DateTime)DTUpdated;

            if (CategoryId != null && TypeId != null)
            {
                A.CategoryId = (int)CategoryId;
                A.TypeId = (int)TypeId;
                LastLog += " Direct_TypeId";
            }
            else
            {

                if (string.IsNullOrEmpty(data.Type))
                {
                    if (isNew)
                    {
                        LastLog += " Name_TypeId";
                        var at = (from t in dc.AssetTypes where t.DepartmentId == DepartmentID && t.AssetProfileId == 1 select t).FirstOrDefault();
                        if (at == null || at.Id == 0) throw new Exception("Cann not find Asset Type with 'Computer profile'");
                        A.CategoryId = at.CategoryId;
                        A.TypeId = at.Id;
                    }
                }
                else
                {
                    LastLog += " First_TypeId";
                    var at = (from t in dc.AssetTypes where t.DepartmentId == DepartmentID && t.AssetProfileId == 1 && t.Name == data.Type select t).FirstOrDefault();
                    if (at == null || at.Id == 0)
                    {
                        if (isNew) throw new Exception("Cann not find '" + data.Type + "' Asset Type");
                    }
                    else
                    {
                        A.CategoryId = at.CategoryId;
                        A.TypeId = at.Id;
                    }
                }
            }

            CategoryId = A.CategoryId;
            TypeId = A.TypeId;

            A.StatusId = Assets.GetStatus(dc, data.Status);
            A.MakeId = Assets.GetMakeId(dc, DepartmentID, data.Make, A.TypeId);
            A.ModelId = Assets.GetModelId(dc, DepartmentID, data.Model, A.MakeId);

            if(AccountId!=null) A.AccountId = AccountId;
            else A.AccountId = Assets.GetAccountId(dc, DepartmentID, data.Account);
            if (LocationId!=null) A.LocationId = LocationId;
            else A.LocationId = Assets.GetLocationId(dc, DepartmentID, data.AccountLocation, A.AccountId);

            A.VendorId = Assets.GetVendorId(dc, DepartmentID, data.PurchaseVendor);
            A.WarrantyVendor = Assets.GetVendorId(dc, DepartmentID, data.WarrantyVendor);
            A.DepartmentId = DepartmentID;

            if (isNew) dc.Assets.InsertOnSubmit(A);
            dc.SubmitChanges();
            data.AssetNumber = A.Id;
            LastLog += " Id=" + A.Id;

            try { SaveComputers(DepartmentID, dc, data); } catch { }
            try { SaveComputerLogicalDrives(DepartmentID, dc, data); } catch { }
            try { SaveComputerProcessors(DepartmentID, dc, data); } catch { }
            try { SaveComputerPrinters(DepartmentID, dc, data); } catch { }
            try { SaveComputerSoftwares(DepartmentID, dc, data); } catch { }

            dc.SubmitChanges();
            return A.Id;
        }

        static public void SaveAssetComputer(Guid OrgId, int DepartmentID, AssetComputerData data, bool AddNewOnly)
        {
            int? CategoryId = null;
            int? TypeId = null;
            SaveAssetComputer(OrgId, DepartmentID, data, AddNewOnly, null, null, ref CategoryId, ref TypeId, null);
        }

        static void CopyComputers(AssetComputerData source, lib.bwa.bigWebDesk.LinqBll.Context.AssetComputers dest, int DepartmentID, int AssetId)
        {
            dest.AssetId = AssetId;
            dest.DepartmentId = DepartmentID;
            dest.NetworkCard1Address = source.NetworkCard1Address;
            dest.NetworkCard1Description = source.NetworkCard1Description;
            dest.NetworkCard1Gate = source.NetworkCard1Gate;
            dest.NetworkCard1IP = source.NetworkCard1IP;
            dest.NetworkCard1Mask = source.NetworkCard1Mask;
            dest.NetworkDomain = source.NetworkDomain;
            dest.NetworkName = source.NetworkName;
            dest.OperatingSystem = source.OperatingSystem;
            dest.OSSerial = source.OSSerial;
            dest.RamMbytes = source.RamMbytes;
            dest.RegisteredUser = source.RegisteredUser;
            dest.VideoDescription = source.VideoDescription;
            dest.VideoHResolution = source.VideoHResolution;
            dest.VideoMemoryMbytes = source.VideoMemoryMbytes;
            dest.VideoVResolution = source.VideoVResolution;
        }

        static void SaveComputers(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, AssetComputerData data)
        {
            var lds = (
                from d in dc.AssetComputers
                where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber
                select d
                ).FirstOrNull();
            if (lds == null)
            {
                lds = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputers();
                dc.AssetComputers.InsertOnSubmit(lds);
            }
            CopyComputers(data, lds, DepartmentID, data.AssetNumber);
        }

        static void CopyComputerLogicalDrives(AssetComputerLogicalDriveData source, lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerLogicalDrives dest, int DepartmentID, int AsetId)
        {
            dest.AssetId = AsetId;
            dest.Compressed = source.Compressed;
            dest.DepartmentId = DepartmentID;
            dest.Description = source.Description;
            dest.DeviceID = source.DeviceID;
            dest.DriveType = source.DriveType;
            dest.FileSystem = source.FileSystem;
            dest.FreeSpaceGB = source.FreeSpaceGB;
            dest.SizeGB = source.SizeGB;
            dest.VolumeName = source.VolumeName;
            dest.VolumeSerial = source.VolumeSerial;
        }

        static void SaveComputerLogicalDrives(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, AssetComputerData data)
        {
            var lds = (
                from d in dc.AssetComputerLogicalDrives
                where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber
                select d
                ).ToList();

            var NeedDel = lds.Where(d1 => !data.LogicalDrives.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            dc.AssetComputerLogicalDrives.DeleteAllOnSubmit(NeedDel);

            var NeedInsert = data.LogicalDrives.Where(d1 => !lds.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            foreach (AssetComputerLogicalDriveData i in NeedInsert)
            {
                var New = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerLogicalDrives();
                //bigWebApps.HelpDesk.WebApi.Utils.CopyProperties<AssetComputerLogicalDriveData>(i, New);
                CopyComputerLogicalDrives(i, New, DepartmentID, data.AssetNumber);
                dc.AssetComputerLogicalDrives.InsertOnSubmit(New);
            }

            var NeedUpdate = lds.Where(d1 => data.LogicalDrives.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            foreach (var i in NeedUpdate)
            {
                var source = data.LogicalDrives.Where(d => d.DeviceID == i.DeviceID).FirstOrDefault();
                if (source == null || source.DeviceID == null) continue;
                //bigWebApps.HelpDesk.WebApi.Utils.CopyProperties<AssetComputerLogicalDriveData>(source, i);
                CopyComputerLogicalDrives(source, i, DepartmentID, data.AssetNumber);
            }
        }

        static void CopyComputerProcessor(AssetComputerProcessorData source, lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerProcessors dest, int DepartmentID, int AssetID)
        {
            dest.AssetId = AssetID;
            dest.CPUClass = source.CPUClass;
            dest.CPUSerial = source.CPUSerial;
            dest.CPUSpeedMHz = source.CPUSpeedMHz;
            dest.CPUType = source.CPUType;
            dest.CPUVendor = source.CPUVendor;
            dest.CurrentClockSpeed = source.CurrentClockSpeed;
            dest.CurrentVoltage = source.CurrentVoltage;
            dest.DepartmentId = DepartmentID;
            dest.Description = source.Description;
            dest.DeviceID = source.DeviceID;
            dest.ExtClock = source.ExtClock;
            dest.L2CachSize = source.L2CachSize;
            dest.UniqueId = source.UniqueId;
            dest.Version = source.Version;
        }

        static void SaveComputerProcessors(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, AssetComputerData data)
        {
            var lds = (
                from d in dc.AssetComputerProcessors
                where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber
                select d
                ).ToList();

            var NeedDel = lds.Where(d1 => !data.Processors.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            dc.AssetComputerProcessors.DeleteAllOnSubmit(NeedDel);

            var NeedInsert = data.Processors.Where(d1 => !lds.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            foreach (AssetComputerProcessorData i in NeedInsert)
            {
                var New = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerProcessors();
                CopyComputerProcessor(i, New, DepartmentID, data.AssetNumber);
                dc.AssetComputerProcessors.InsertOnSubmit(New);
            }

            var NeedUpdate = lds.Where(d1 => data.Processors.Select(d2 => d2.DeviceID).Contains(d1.DeviceID)).ToList();
            foreach (var i in NeedUpdate)
            {
                var source = data.Processors.Where(d => d.DeviceID == i.DeviceID).FirstOrDefault();
                if (source == null || source.DeviceID == null) continue;
                CopyComputerProcessor(source, i, DepartmentID, data.AssetNumber);
            }
        }

        static void CopyComputerPrinters(AssetComputerPrinterData source, lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerPrinters dest, int DepartmentID, int AssetID)
        {
            dest.AssetId = AssetID;
            dest.DepartmentId = DepartmentID;
            dest.PrinterDriver = source.PrinterDriver;
            dest.PrinterName = source.PrinterName;
            dest.PrinterPort = source.PrinterPort;
        }

        static void SaveComputerPrinters(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, AssetComputerData data)
        {
            var lds = (
                from d in dc.AssetComputerPrinters
                where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber
                select d
                ).ToList();

            var NeedDel = lds.Where(d1 => !data.Printers.Select(d2 => d2.PrinterPort).Contains(d1.PrinterPort)).ToList();
            dc.AssetComputerPrinters.DeleteAllOnSubmit(NeedDel);

            var NeedInsert = data.Printers.Where(d1 => !lds.Select(d2 => d2.PrinterPort).Contains(d1.PrinterPort)).ToList();
            foreach (AssetComputerPrinterData i in NeedInsert)
            {
                var New = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerPrinters();
                CopyComputerPrinters(i, New, DepartmentID, data.AssetNumber);
                dc.AssetComputerPrinters.InsertOnSubmit(New);
            }

            var NeedUpdate = lds.Where(d1 => data.Printers.Select(d2 => d2.PrinterPort).Contains(d1.PrinterPort)).ToList();
            foreach (var i in NeedUpdate)
            {
                var source = data.Printers.Where(d => d.PrinterPort == i.PrinterPort).FirstOrDefault();
                if (source == null || source.PrinterPort == null) continue;
                CopyComputerPrinters(source, i, DepartmentID, data.AssetNumber);
            }
        }

        static void CopyComputerSoftware(AssetComputerSoftwareData source, lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerSoftwares dest, int DepartmentID, int AssetID)
        {
            dest.AssetId = AssetID;
            dest.DepartmentId = DepartmentID;
            dest.SoftwareName = source.SoftwareName;
            dest.SoftwarePublisher = source.SoftwarePublisher;
            dest.SoftwareVersion = source.SoftwareVersion;
        }

        static void SaveComputerSoftwares(int DepartmentID, lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc, AssetComputerData data)
        {
            var lds = (
                from d in dc.AssetComputerSoftwares
                where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber
                select d
                ).ToList();

            var ss = data.Softwares.ToList();
            foreach (var ld in lds)
            {
                bool exist = false;
                for (int i = 0; i < ss.Count; i++)
                {
                    var s = ss[i];
                    if (ld.SoftwareName == s.SoftwareName && ld.SoftwarePublisher == s.SoftwarePublisher && ld.SoftwareVersion == s.SoftwareVersion)
                    {
                        exist = true;
                        ss.Remove(s);
                        i--;
                    }
                }
                if (exist) continue;
                dc.AssetComputerSoftwares.DeleteOnSubmit(ld);
            }

            foreach (var s in ss)
            {
                var New = new lib.bwa.bigWebDesk.LinqBll.Context.AssetComputerSoftwares();
                CopyComputerSoftware(s, New, DepartmentID, data.AssetNumber);
                dc.AssetComputerSoftwares.InsertOnSubmit(New);
            }
        }

        public static void DeleteAssetComputer(Guid OrgId, int DepartmentID, AssetComputerData data)
        {
            if (data.AssetNumber == 0) return;
            lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext dc = new lib.bwa.bigWebDesk.LinqBll.Context.MutiBaseDataContext(OrgId, DepartmentID);

            var lds = from d in dc.AssetComputerLogicalDrives where d.DepartmentId == DepartmentID && d.AssetId == data.AssetNumber select d;
            dc.AssetComputerLogicalDrives.DeleteAllOnSubmit(lds);

            var ps = from p in dc.AssetComputerProcessors where p.DepartmentId == DepartmentID && p.AssetId == data.AssetNumber select p;
            dc.AssetComputerProcessors.DeleteAllOnSubmit(ps);

            var prs = from p in dc.AssetComputerPrinters where p.DepartmentId == DepartmentID && p.AssetId == data.AssetNumber select p;
            dc.AssetComputerPrinters.DeleteAllOnSubmit(prs);

            var ss = from p in dc.AssetComputerSoftwares where p.DepartmentId == DepartmentID && p.AssetId == data.AssetNumber select p;
            dc.AssetComputerSoftwares.DeleteAllOnSubmit(ss);

            var ac = from p in dc.AssetComputers where p.DepartmentId == DepartmentID && p.AssetId == data.AssetNumber select p;
            dc.AssetComputers.DeleteAllOnSubmit(ac);

            var a = from p in dc.Assets where p.DepartmentId == DepartmentID && p.Id == data.AssetNumber select p;
            dc.Assets.DeleteAllOnSubmit(a);
        }
    }
}
