using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace bigWebApps.HelpDesk.WebApi.Soap.v1
{
    [DataContract] public class AssetData
    {
        [DataMember] [DataIgnoreAttrib] [DataNameAttrib("Id")] [DataReadonlyAttrib] public int AssetNumber;
        [DataMember] public string SerialNumber;
        [DataMember] public string Unique1;
        [DataMember] public string Unique2;
        [DataMember] public string Unique3;
        [DataMember] public string Unique4;
        [DataMember] public string Unique5;
        [DataMember] public string Unique6;
        [DataMember] public string Unique7;

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

    [DataContract] public class AssetPropertyValueData
    {
        [DataMember] public int Id;
        [DataMember] public int AssetId;
        [DataMember] public string AssetTypeProperty;
        [DataMember] public string PropertyValue;
    }
}