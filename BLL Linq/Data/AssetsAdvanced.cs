using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace bigWebApps.HelpDesk.WebApi.SOAP.v1.Advanced
{
    [DataContract] public class AssetData
    {
        [DataMember] [DataNameAttrib("Id")] [DataReadonlyAttrib] public int AssetNumber;
        [DataMember] public string SerialNumber;
        [DataMember] [DataIgnoreAttrib] public string Unique1;
        [DataMember] [DataIgnoreAttrib] public string Unique3;
        [DataMember] [DataIgnoreAttrib] public string Unique4;
        [DataMember] [DataIgnoreAttrib] public string Unique5;
        [DataMember] [DataIgnoreAttrib] public string Unique6;
        [DataMember] [DataIgnoreAttrib] public string Unique7;
        [DataMember] [DataIgnoreAttrib] public string Unique8;

        [DataMember] public int CategoryId;
        [DataMember] public int TypeId;
        [DataMember] public int StatusId;
        [DataMember] public int? MakeId;
        [DataMember] public int? ModelId;

        [DataMember] [DataNameAttrib("Name")] public string AssetName;
        [DataMember] public string PONumber;
        [DataMember] public string FundingSource;
        [DataMember] public string FundingCode;

        [DataMember] public int? AccountId;
        //[DataMember] [DataNameAttrib("location_id")] public int? AccountLocation_id;
        [DataMember] [DataNameAttrib("LocationId")] public int? AccountLocationId;
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

        [DataMember] [DataNameAttrib("intUpdatedBy")] public int? UpdatedById;
        [DataMember] public int? CheckedOutId;
        [DataMember] public int? OwnerId;

        [DataMember] public decimal? ValueCurrent;
        [DataMember] public decimal? ValueReplacement;
        [DataMember] public decimal? ValueDepreciated;
        [DataMember] public decimal? ValueSalvage;
        [DataMember] [DataNameAttrib("DisposalCost")]
        public decimal? ValueDisposalCost;
        [DataMember] [DataNameAttrib("Value")]
        public decimal? ValuePurchaseCost;

        [DataMember] [DataNameAttrib("VendorId")] public int? PurchaseVendorId;
        [DataMember] [DataNameAttrib("WarrantyVendor")] public int? WarrantyVendorId;
        [DataMember] public byte? PartsWarrantyLength;
        [DataMember]
        public byte? LaborWarrantyLength;

        [DataMember]
        public string AssetTag;
        [DataMember]
        public short AssetSort;
        [DataMember]
        public string Room;
        [DataMember]
        public bool Active;
    }

    [DataContract]
    public class AssetExtendData : AssetData
    {
        [DataMember]
        public string Category;
        [DataMember]
        public string Type;
        [DataMember]
        public string Status;
        [DataMember]
        public string Make;
        [DataMember]
        public string Model;
        [DataMember]
        public string Account;
        [DataMember]
        public string UpdatedBy;
        [DataMember]
        public string CheckedOut;
        [DataMember]
        public string Owner;
        [DataMember]
        public string PurchaseVendor;
        [DataMember]
        public string WarrantyVendor;
    }
    [DataContract]
    public class AssetExtendResult
    {
        [DataMember]
        public bool NewAsset;
        [DataMember]
        public bool NewCategory;
        [DataMember]
        public bool NewType;
        [DataMember]
        public bool NewMake;
        [DataMember]
        public bool NewModel;
        [DataMember]
        public bool NewUpdatedBy;
        [DataMember]
        public bool NewCheckedOut;
        [DataMember]
        public bool NewOwner;
        [DataMember]
        public bool NewPurchaseVendor;
        [DataMember]
        public bool NewWarrantyVendor;
    }


    [DataContract]
    public class AssetPropertyValueData
    {
        [DataMember]
        public int Id;
        [DataMember]
        public int AssetId;
        [DataMember]
        public int AssetTypePropertyId;
        [DataMember]
        public string PropertyValue;
    }

    [DataContract]
    public class AssetTypePropertyData
    {
        [DataMember]
        public int Id;
        [DataMember]
        public int AssetTypeId;
        [DataMember]
        public string Name;
        [DataMember]
        public int DataType;
        [DataMember]
        public string Enumeration;
        [DataMember]
        public string Description;
    }

    [DataContract]
    public class AssetCategoryData
    {
        [DataMember]
        public int id;
        [DataMember]
        public string Name;
    }

    [DataContract]
    public class AssetMakeData
    {
        [DataMember]
        public int id;
        [DataMember]
        public int TypeId;
        [DataMember]
        public string Make;
    }

    [DataContract]
    public class AssetModelData
    {
        [DataMember]
        public int id;
        [DataMember]
        public int MakeId;
        [DataMember]
        public string Model;
        [DataMember]
        public string Links;
    }

    [DataContract]
    public class AssetTypeData
    {
        [DataMember]
        public int id;
        [DataMember]
        public string Name;
        [DataMember]
        public int CategoryId;
        [DataMember]
        public bool configCustFields;
    }

    [DataContract]
    public class VendorData
    {
        [DataMember]
        public int id;
        [DataMember]
        public string name;
        [DataMember]
        public string phone;
        [DataMember]
        public string fax;
        [DataMember]
        public string AccountNumber;
        [DataMember]
        public string notes;
    }

    [DataContract]
    public class AssetSubAssetData
    {
        [DataMember]
        public int id;
        [DataMember]
        public int AssetId;
        [DataMember]
        public int AssetChildId;
        [DataMember]
        public string Description;
    }



    //Read Only Datas

    [DataContract]
    public class AssetStatusData
    {
        [DataMember]
        public int id;
        [DataMember]
        public string vchStatus;
    }

    [DataContract]
    public class LocationData
    {
        [DataMember]
        public int Id;
        [DataMember]
        public int? ParentId;
        [DataMember]
        public int? AccountId;
        [DataMember]
        public string LocationType;
        [DataMember]
        public string Name;
        [DataMember]
        public bool Inactive;
        [DataMember]
        public string Country;
        [DataMember]
        public string State;
        [DataMember]
        public string City;
        [DataMember]
        public string Address1;
        [DataMember]
        public string Address2;
        [DataMember]
        public string ZipCode;
        [DataMember]
        public string Phone1;
        [DataMember]
        public string Phone2;
        [DataMember]
        public int? WorkPlaces;
        [DataMember]
        public string RoomNumber;
        [DataMember]
        public decimal? RoomSize;
        [DataMember]
        public string Description;
        [DataMember]
        public bool IsDefault;
    }

    [DataContract]
    public class AccountData
    {
        [DataMember]
        public int Id;
        [DataMember]
        public string location_id;
        [DataMember]
        public string LocationId;
        [DataMember]
        public string txtNote;
        [DataMember]
        public string vchName;
    }

    [DataContract]
    public class CompanyLoginData
    {
        [DataMember]
        public int Id;
        [DataMember]
        public string UserType;
        [DataMember]
        public string Email;
        [DataMember]
        public string FirstName;
        [DataMember]
        public string LastName;
    }
}