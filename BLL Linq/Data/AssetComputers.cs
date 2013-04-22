using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace bigWebApps.HelpDesk.WebApi.Soap.v1
{
    [DataContract] public class AssetComputerData
    {
        [DataMember] [DataIgnoreAttrib] [DataNameAttrib("Id")] [DataReadonlyAttrib] public int AssetNumber;
        [DataMember] public string MotherboardSerialNumber;
        [DataMember] public string BiosSeriaNumber;
        [DataMember] public string PCSerialNumber;

        [DataMember] public string RegisteredUser;
        [DataMember] public string OperatingSystem;
        [DataMember] public string OSSerial;
        [DataMember] public int? RamMbytes;
        [DataMember] public string VideoDescription;
        [DataMember] public int? VideoMemoryMbytes;
        [DataMember] public int? VideoHResolution;
        [DataMember] public int? VideoVResolution;
        [DataMember] public string NetworkName;
        [DataMember] public string NetworkDomain;
        [DataMember] public string NetworkCard1IP;
        [DataMember] public string NetworkCard1Mask;
        [DataMember] public string NetworkCard1Gate;
        [DataMember] public string NetworkCard1Address;
        [DataMember] public string NetworkCard1Description;



        [DataMember] public AssetComputerLogicalDriveData[] LogicalDrives;
        [DataMember] public AssetComputerProcessorData[] Processors;
        [DataMember] public AssetComputerPrinterData[] Printers;
        [DataMember] public AssetComputerSoftwareData[] Softwares;



        [DataMember] [DataIgnoreAttrib] public string Category;
        [DataMember] [DataIgnoreAttrib] public string Type;
        [DataMember] [DataIgnoreAttrib] public string Status;
        [DataMember] [DataIgnoreAttrib] public string Make;
        [DataMember] [DataIgnoreAttrib] public string Model;

        [DataMember] [DataNameAttrib("Name")] public string AssetName;
        [DataMember] public string PONumber;
        [DataMember] public string FundingSource;
        [DataMember] public string FundingCode;

        [DataMember] [DataIgnoreAttrib] public string Account;
        //[DataMember] [DataNameAttrib("location_id")] public int? AccountLocation_id;
        [DataMember] [DataIgnoreAttrib] public string AccountLocation;
        [DataMember] public string Description;
        [DataMember] public string Notes;

        [DataMember] public System.DateTime? DateAquired;
        [DataMember] public System.DateTime? DatePurchased;
        [DataMember] public System.DateTime? DateReceived;
        [DataMember] public System.DateTime? DateEntered;
        [DataMember] public System.DateTime? DateDeployed;
        [DataMember] public System.DateTime? DateOutOfService;
        [DataMember] public System.DateTime? DateDisposed;
        [DataMember] [DataNameAttrib("dtUpdated")] public System.DateTime? DateUpdated;

        [DataMember] [DataIgnoreAttrib] public string UpdatedByFirstName;
        [DataMember] [DataIgnoreAttrib] public string UpdatedByLastName;
        [DataMember] [DataIgnoreAttrib] public string UpdatedByEmail;

        [DataMember] [DataIgnoreAttrib] public string CheckedOutFirstName;
        [DataMember] [DataIgnoreAttrib] public string CheckedOutLastName;
        [DataMember] [DataIgnoreAttrib] public string CheckedOutEmail;

        [DataMember] [DataIgnoreAttrib] public string OwnerFirstName;
        [DataMember] [DataIgnoreAttrib] public string OwnerLastName;
        [DataMember] [DataIgnoreAttrib] public string OwnerEmail;

        [DataMember] public decimal? ValueCurrent;
        [DataMember] public decimal? ValueReplacement;
        [DataMember] public decimal? ValueDepreciated;
        [DataMember] public decimal? ValueSalvage;
        [DataMember] [DataNameAttrib("DisposalCost")] public decimal? ValueDisposalCost;
        [DataMember] [DataNameAttrib("Value")] public decimal? ValuePurchaseCost;

        [DataMember] [DataIgnoreAttrib] public string PurchaseVendor;
        [DataMember] [DataIgnoreAttrib] public string WarrantyVendor;
        [DataMember] public byte? PartsWarrantyLength;
        [DataMember] public byte? LaborWarrantyLength;

        [DataMember] public short AssetSort;
        [DataMember] public string Room;
        [DataMember] public bool Active;
    }

    [DataContract] public class AssetComputerLogicalDriveData
    {
        [DataMember] public string DeviceID;
        [DataMember] public string FileSystem;
        [DataMember] public float? SizeGB;
        [DataMember] public float? FreeSpaceGB;
        [DataMember] public string Description;
        [DataMember] public string VolumeName;
        [DataMember] public string VolumeSerial;
        [DataMember] public bool? Compressed;
        [DataMember] public string DriveType;
    }

    [DataContract] public class AssetComputerProcessorData
    {
        [DataMember] public string CPUType;
        [DataMember] public string CPUSerial;
        [DataMember] public string CPUVendor;
        [DataMember] public int? CPUSpeedMHz;
        [DataMember] public string CPUClass;
        [DataMember] public int? CurrentClockSpeed;
        [DataMember] public string UniqueId;
        [DataMember] public string Description;
        [DataMember] public string Version;
        [DataMember] public int? L2CachSize;
        [DataMember] public int? ExtClock;
        [DataMember] public float? CurrentVoltage;
        [DataMember] public string DeviceID;
    }

    [DataContract] public class AssetComputerPrinterData
    {
        [DataMember] public string PrinterName;
        [DataMember] public string PrinterDriver;
        [DataMember] public string PrinterPort;
    }

    [DataContract] public class AssetComputerSoftwareData
    {
        [DataMember] public string SoftwareName;
        [DataMember] public string SoftwarePublisher;
        [DataMember] public string SoftwareVersion;
    }
}