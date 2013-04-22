using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Classes.
    /// </summary>
    public class Asset : DBAccess
    {
        [Flags]
        public enum AssetDuplicateType
        {
            NoDuplicates = 0,
            SerialNumber = 1,
            Unique1 = 2,
            Unique2 = 4,
            Unique3 = 8,
            Unique4 = 16,
            Unique5 = 32,
            Unique6 = 64,
            Unique7 = 128
        }

        public static int SelectAssetsCount(Guid OrgId, Guid InstId)
        {
            int deptId = Companies.SelectDepartmentId(OrgId, InstId);
            DataTable _dt = SelectByQuery("SELECT Count(*) FROM Assets WHERE DepartmentId=" + deptId.ToString() + " AND Active=1", OrgId);
            return (int)_dt.Rows[0][0];
        }

        public static DataRow GetAsset(int departmentId, int assetId)
        {
            return GetAsset(Guid.Empty, departmentId, assetId);
        }

        public static DataRow GetAsset(Guid OrgId, int departmentId, int assetId)
        {
            return SelectRecord("sp_SelectAsset", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) }, OrgId);
        }

        public static DataRow GetAssetComputer(int departmentId, int assetId)
        {
            return SelectRecord("sp_SelectAssetComputer", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetComputerPrinters(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectAssetComputerPrinters", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetComputerLogicalDrives(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectAssetComputerLogicalDrives", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetComputerProcessors(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectAssetComputerProcessors", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetComputerSoftwares(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectAssetComputerSoftwares", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetStatusList(int departmentId)
        {
            return SelectRecords("sp_SelectAssetStatusList", new[] { new SqlParameter("@DId", departmentId) });
        }


        public static DataTable SelectAssetProfilesList()
        {
            return SelectRecords("sp_SelectAssetProfilesList");
        }

        public static DataTable SelectAssetVendorList(int departmentId)
        {
            return SelectRecords("sp_SelectVendor", new[] { new SqlParameter("@companyId", departmentId), new SqlParameter("@code", 1) });
        }

        public static DataTable SelectAssetTickets(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectAssetTickets", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectSubAssets(int departmentId, int assetId)
        {
            return SelectRecords("sp_SelectSubAssets", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId) });
        }

        public static DataTable SelectAssetUserList(int departmentId)
        {
            return SelectRecords("sp_SelectUserForLikeSearch", new[] { new SqlParameter("@DId", departmentId), new SqlParameter("@FirstName", ""), new SqlParameter("@LastName", ""), new SqlParameter("@Email", "") });
        }

        public static DataTable SelectAssetLog(int assetId)
        {
            return SelectRecords("sp_SelectAssetLogs", new[] { new SqlParameter("@AssetId", assetId) });
        }

        public static int UpdateAssetNotes(int departmentId, int assetId, string notes)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetNotes", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId), new SqlParameter("@Notes", notes) });
            return (int)pReturnValue.Value;
        }

        public static int InsertAssetLog(int departmentId, int userId, int assetId, string log)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_InsertAssetLog", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@UId", userId), new SqlParameter("@AssetId", assetId), new SqlParameter("@Note", log) });
            return (int)pReturnValue.Value;
        }

        public static int InsertSubAsset(int departmentId, int assetId, int subAssetId, string description)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pAseetDescription = new SqlParameter("@Description", DBNull.Value);
            if (!string.IsNullOrEmpty(description))
                pAseetDescription.Value = description;

            UpdateData("sp_InsertSubAsset", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId), new SqlParameter("@SubAssetId", subAssetId), pAseetDescription });
            return (int)pReturnValue.Value;
        }

        public static int DeleteSubAsset(int departmentId, int assetId, int subAssetId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pSubAsset = new SqlParameter("@SubAssetId", DBNull.Value);
            if (subAssetId > 0)
                pSubAsset.Value = subAssetId;

            UpdateData("sp_DeleteSubAsset", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetId", assetId), pSubAsset });
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetOwner(int departmentId, int userId, int assetId, string userName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pOwner = new SqlParameter("@OwnerId", DBNull.Value);
            if (userId > 0)
                pOwner.Value = userId;

            UpdateData("sp_UpdateAssetOwner", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), pOwner, new SqlParameter("@AssetId", assetId), new SqlParameter("@vchUpdatedBy", userName) });
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetCheckout(int departmentId, int userId, int assetId, string userName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pOwner = new SqlParameter("@CheckedOutId", DBNull.Value);
            if (userId > 0)
                pOwner.Value = userId;

            UpdateData("sp_UpdateAssetCheckedOut", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), pOwner, new SqlParameter("@AssetId", assetId), new SqlParameter("@vchUpdatedBy", userName) });
            return (int)pReturnValue.Value;
        }

        public static DataTable SelectDuplicates(int departmentId, int assetId, string serialNumber, string unique1, string unique2, string unique3, string unique4, string unique5, string unique6, string unique7, out int duplicateFields, string MotherboardSerial, string BiosSerial)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pAssetId = new SqlParameter("@AssetId", DBNull.Value);
            if (assetId > 0)
                pAssetId.Value = assetId;

            SqlParameter pSerialNumber = new SqlParameter("@SerialNumber", DBNull.Value);
            if (!string.IsNullOrEmpty(serialNumber))
                pSerialNumber.Value = serialNumber;

            SqlParameter pUnique1 = new SqlParameter("@Unique1", DBNull.Value);
            if (!string.IsNullOrEmpty(unique1))
                pUnique1.Value = unique1;

            SqlParameter pUnique2 = new SqlParameter("@Unique2", DBNull.Value);
            if (!string.IsNullOrEmpty(unique2))
                pUnique2.Value = unique2;

            SqlParameter pUnique3 = new SqlParameter("@Unique3", DBNull.Value);
            if (!string.IsNullOrEmpty(unique3))
                pUnique3.Value = unique3;

            SqlParameter pUnique4 = new SqlParameter("@Unique4", DBNull.Value);
            if (!string.IsNullOrEmpty(unique4))
                pUnique4.Value = unique4;

            SqlParameter pUnique5 = new SqlParameter("@Unique5", DBNull.Value);
            if (!string.IsNullOrEmpty(unique5))
                pUnique5.Value = unique5;

            SqlParameter pUnique6 = new SqlParameter("@Unique6", DBNull.Value);
            if (!string.IsNullOrEmpty(unique6))
                pUnique6.Value = unique6;

            SqlParameter pUnique7 = new SqlParameter("@Unique7", DBNull.Value);
            if (!string.IsNullOrEmpty(unique7)) pUnique7.Value = unique7;

            SqlParameter pMotherboardSerial = new SqlParameter("@MotherboardSerial", DBNull.Value);
            if (!string.IsNullOrEmpty(MotherboardSerial)) pMotherboardSerial.Value = MotherboardSerial;

            SqlParameter pBiosSerial = new SqlParameter("@BiosSerial", DBNull.Value);
            if (!string.IsNullOrEmpty(BiosSerial)) pBiosSerial.Value = BiosSerial;

            DataTable dtDuplicateAssets = SelectRecords("sp_SelectAssetIsDuplicate",
                        new[] { 
                                pReturnValue,
                                pAssetId,
                                new SqlParameter("@DepartmentId", departmentId),
                                pSerialNumber,
                                pUnique1,
                                pUnique2,
                                pUnique3,
                                pUnique4,
                                pUnique5,
                                pUnique6,
                                pUnique7
                                });
            duplicateFields = (int)pReturnValue.Value;
            return dtDuplicateAssets;
        }

        public static int InsertAsset(int departmentId, int? userId, Guid? guid, string serialNumber, int categoryId, int typeId, int makeId, int modelId, string unique1, string unique2, string unique3, string unique4, string unique5, string unique6, string unique7, string MotherboardSerial, string BiosSerial)
        {
            return InsertAsset(departmentId, userId, guid, serialNumber, categoryId, typeId, makeId, modelId, unique1, unique2, unique3, unique4, unique5, unique6, unique7, MotherboardSerial, BiosSerial, null, null);
        }

        public static int InsertAsset(int departmentId, int? userId, Guid? guid, string serialNumber, int categoryId, int typeId, int makeId, int modelId, string unique1, string unique2, string unique3, string unique4, string unique5, string unique6, string unique7, string MotherboardSerial, string BiosSerial, string AssetName, string AssetDescription)
        {
            return InsertAsset(departmentId, userId, guid, serialNumber, categoryId, typeId, makeId, modelId, unique1, unique2, unique3, unique4, unique5, unique6, unique7, MotherboardSerial, BiosSerial, AssetName, AssetDescription, 0);
        }

        public static int InsertAsset(int departmentId, int? userId, Guid? guid, string serialNumber, int categoryId, int typeId, int makeId, int modelId, string unique1, string unique2, string unique3, string unique4, string unique5, string unique6, string unique7, string MotherboardSerial, string BiosSerial, string AssetName, string AssetDescription, int LocationId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pGuid = new SqlParameter("@AssetGuid", DBNull.Value);
            if (guid != null && guid != Guid.Empty)
                pGuid.Value = guid.Value;

            SqlParameter pSerialNumber = new SqlParameter("@SerialNumber", DBNull.Value);
            if (!string.IsNullOrEmpty(serialNumber))
                pSerialNumber.Value = serialNumber;

            SqlParameter pUnique1 = new SqlParameter("@Unique1", DBNull.Value);
            if (!string.IsNullOrEmpty(unique1))
                pUnique1.Value = unique1;

            SqlParameter pUnique2 = new SqlParameter("@Unique2", DBNull.Value);
            if (!string.IsNullOrEmpty(unique2))
                pUnique2.Value = unique2;

            SqlParameter pUnique3 = new SqlParameter("@Unique3", DBNull.Value);
            if (!string.IsNullOrEmpty(unique3))
                pUnique3.Value = unique3;

            SqlParameter pUnique4 = new SqlParameter("@Unique4", DBNull.Value);
            if (!string.IsNullOrEmpty(unique4))
                pUnique4.Value = unique4;

            SqlParameter pUnique5 = new SqlParameter("@Unique5", DBNull.Value);
            if (!string.IsNullOrEmpty(unique5))
                pUnique5.Value = unique5;

            SqlParameter pUnique6 = new SqlParameter("@Unique6", DBNull.Value);
            if (!string.IsNullOrEmpty(unique6))
                pUnique6.Value = unique6;

            SqlParameter pUnique7 = new SqlParameter("@Unique7", DBNull.Value);
            if (!string.IsNullOrEmpty(unique7))
                pUnique7.Value = unique7;

            SqlParameter pUserId = new SqlParameter("@UserId", DBNull.Value);
            if (userId != null)
                pUserId.Value = userId.Value;

            SqlParameter pMotherboardSerial = new SqlParameter("@MotherboardSerial", DBNull.Value);
            if (!string.IsNullOrEmpty(MotherboardSerial)) pMotherboardSerial.Value = MotherboardSerial;

            SqlParameter pBiosSerial = new SqlParameter("@BiosSerial", DBNull.Value);
            if (!string.IsNullOrEmpty(BiosSerial)) pBiosSerial.Value = BiosSerial;

            SqlParameter pAssetMake = new SqlParameter("@AssetMake", DBNull.Value);
            if (makeId > 0)
                pAssetMake.Value = makeId;            

            SqlParameter pAssetModel = new SqlParameter("@AssetModel", DBNull.Value);
            if (modelId > 0)
                pAssetModel.Value = modelId;

            SqlParameter pLocationId = new SqlParameter("@LocationId", LocationId);
            if (LocationId <= 0) pLocationId.Value = DBNull.Value;

            UpdateData("sp_InsertAsset",
                        new[] { 
                                pReturnValue,
                                new SqlParameter("@DepartmentId", departmentId),
                                pUserId,
                                pGuid,
                                pSerialNumber,
                                pUnique1,
                                pUnique2,
                                pUnique3,
                                pUnique4,
                                pUnique5,
                                pUnique6,
                                pUnique7,
                                new SqlParameter("@AssetCategory", categoryId),
                                new SqlParameter("@AssetType", typeId),
                                pAssetMake,
                                pAssetModel,
                                pMotherboardSerial,
                                pBiosSerial,
                                new SqlParameter("@Name", AssetName),
                                new SqlParameter("@Description", AssetDescription),
                                pLocationId
                                });

            return (int)pReturnValue.Value;
        }

        public static void DeleteTempAsset(int DepartmentId, int AssetId)
        {
            UpdateData("sp_DeleteTempAsset", new SqlParameter[] { new SqlParameter("@DId", DepartmentId), new SqlParameter("@AssetId", AssetId) });
        }

        public static int UpdateAsset(int departmentId,
                                      int? userId,
                                      int assetId,
                                      int purchaseVendorId,
                                      int warrantyVendorId,
                                      int accountId,
                                      int locationId,
                                      string assetName,
                                      string assetDescription,
                                      double assetValue,
                                      double valueCurrent,
                                      double valueReplacement,
                                      double valueDepreciated,
                                      double valueSalvage,
                                      double disposalCost,
                                      int assetSort,
                                      string fundingSource,
                                      DateTime? dateAcquired,
                                      DateTime? datePurchased,
                                      DateTime? dateDeployed,
                                      DateTime? dateOutOfService,
                                      DateTime? dateEntered,
                                      DateTime? dateReceived,
                                      DateTime? dateDisposed,
                                      int warrantyLabor,
                                      int warrantyPart,
                                      string poNumber,
                                      string findingCode,
                                      string serialNumber,
                                      string unique1,
                                      string unique2,
                                      string unique3,
                                      string unique4,
                                      string unique5,
                                      string unique6,
                                      string unique7,
                                      int newStatusId,
                                      int oldStatusId,
                                      bool updateUniqueFields
                                      )
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pPurchaseVendor = new SqlParameter("@PurchaseVendor", purchaseVendorId);
            if (purchaseVendorId <= 0) pPurchaseVendor.Value = DBNull.Value;

            SqlParameter pWarrantyVendor = new SqlParameter("@WarrantyVendor", warrantyVendorId);
            if (warrantyVendorId <= 0) pWarrantyVendor.Value = DBNull.Value;

            SqlParameter pValue = new SqlParameter("@Value", SqlDbType.Money, 8);
            if (assetValue < 0)
                pValue.Value = DBNull.Value;
            else
                pValue.Value = assetValue;

            SqlParameter pValueCurrent = new SqlParameter("@ValueCurrent", SqlDbType.Money, 8);
            if (valueCurrent < 0)
                pValueCurrent.Value = DBNull.Value;
            else
                pValueCurrent.Value = valueCurrent;

            SqlParameter pValueReplacement = new SqlParameter("@ValueReplacement", SqlDbType.Money, 8);
            if (valueReplacement < 0)
                pValueReplacement.Value = DBNull.Value;
            else
                pValueReplacement.Value = valueReplacement;

            SqlParameter pValueDepreciated = new SqlParameter("@ValueDepreciated", SqlDbType.Money, 8);
            if (valueDepreciated < 0)
                pValueDepreciated.Value = DBNull.Value;
            else
                pValueDepreciated.Value = valueDepreciated;

            SqlParameter pValueSalvage = new SqlParameter("@ValueSalvage", SqlDbType.Money, 8);
            if (valueSalvage < 0)
                pValueSalvage.Value = DBNull.Value;
            else
                pValueSalvage.Value = valueSalvage;

            SqlParameter pDisposalCost = new SqlParameter("@DisposalCost", SqlDbType.Money, 8);
            if (disposalCost < 0)
                pDisposalCost.Value = DBNull.Value;
            else
                pDisposalCost.Value = disposalCost;


            SqlParameter pLocationId = new SqlParameter("@Location", locationId);
            if (locationId <= 0) pLocationId.Value = DBNull.Value;

            SqlParameter pDateAquired = new SqlParameter("@DateAquired", DbType.DateTime);
            if (dateAcquired.HasValue)
                pDateAquired.Value = dateAcquired.Value;
            else
                pDateAquired.Value = DBNull.Value;

            SqlParameter pDatePurchased = new SqlParameter("@DatePurchased", DbType.DateTime);
            if (datePurchased.HasValue)
                pDatePurchased.Value = datePurchased.Value;
            else
                pDatePurchased.Value = DBNull.Value;

            SqlParameter pDateDeployed = new SqlParameter("@DateDeployed", DbType.DateTime);
            if (dateDeployed.HasValue)
                pDateDeployed.Value = dateDeployed.Value;
            else
                pDateDeployed.Value = DBNull.Value;

            SqlParameter pDateOutOfService = new SqlParameter("@DateOutOfService", DbType.DateTime);
            if (dateOutOfService.HasValue)
                pDateOutOfService.Value = dateOutOfService.Value;
            else
                pDateOutOfService.Value = DBNull.Value;

            SqlParameter pDateEntered = new SqlParameter("@DateEntered", DbType.DateTime);
            if (dateEntered.HasValue)
                pDateEntered.Value = dateEntered.Value;
            else
                pDateEntered.Value = DBNull.Value;

            SqlParameter pDateReceived = new SqlParameter("@DateReceived", DbType.DateTime);
            if (dateReceived.HasValue)
                pDateReceived.Value = dateReceived.Value;
            else
                pDateReceived.Value = DBNull.Value;

            SqlParameter pDateDisposed = new SqlParameter("@DateDisposed", DbType.DateTime);
            if (dateDisposed.HasValue)
                pDateDisposed.Value = dateDisposed.Value;
            else
                pDateDisposed.Value = DBNull.Value;

            SqlParameter pWarrantyLabor = new SqlParameter("@LaborWarrantyLength", warrantyLabor);
            if (warrantyLabor < 0) pWarrantyLabor.Value = DBNull.Value;

            SqlParameter pWarrantyPart = new SqlParameter("@PartsWarrantyLength", warrantyPart);
            if (warrantyPart < 0) pWarrantyPart.Value = DBNull.Value;

            SqlParameter pAssetSort = new SqlParameter("@AssetSort", assetSort);
            if (assetSort < 0) pAssetSort.Value = 1; //default

            SqlParameter pFundingSource = new SqlParameter("@FundingSource", fundingSource);
            if (fundingSource.Length == 0) pFundingSource.Value = DBNull.Value;

            SqlParameter pNewStatusId = new SqlParameter("@newStatusId", SqlDbType.Int, 4);
            if (newStatusId == oldStatusId || newStatusId < 0)
                pNewStatusId.Value = DBNull.Value;
            else
                pNewStatusId.Value = newStatusId;

            SqlParameter pUnique1 = new SqlParameter("@Unique1", DBNull.Value);
            if (!string.IsNullOrEmpty(unique1))
                pUnique1.Value = unique1;

            SqlParameter pUnique2 = new SqlParameter("@Unique2", DBNull.Value);
            if (!string.IsNullOrEmpty(unique2))
                pUnique2.Value = unique2;

            SqlParameter pUnique3 = new SqlParameter("@Unique3", DBNull.Value);
            if (!string.IsNullOrEmpty(unique3))
                pUnique3.Value = unique3;

            SqlParameter pUnique4 = new SqlParameter("@Unique4", DBNull.Value);
            if (!string.IsNullOrEmpty(unique4))
                pUnique4.Value = unique4;

            SqlParameter pUnique5 = new SqlParameter("@Unique5", DBNull.Value);
            if (!string.IsNullOrEmpty(unique5))
                pUnique5.Value = unique5;

            SqlParameter pUnique6 = new SqlParameter("@Unique6", DBNull.Value);
            if (!string.IsNullOrEmpty(unique6))
                pUnique6.Value = unique6;

            SqlParameter pUnique7 = new SqlParameter("@Unique7", DBNull.Value);
            if (!string.IsNullOrEmpty(unique7))
                pUnique7.Value = unique7;

            SqlParameter pUserId = new SqlParameter("@UId", DBNull.Value);
            if (userId !=  null)
                pUserId.Value = userId.Value;

            UpdateData("sp_UpdateAsset",
            new[] { pReturnValue,
                    new SqlParameter("@DepartmentId", departmentId),
                    new SqlParameter("@UId", userId),
                    new SqlParameter("@AssetId", assetId),                                                               
                    pPurchaseVendor,                
                    pWarrantyVendor,
                    new SqlParameter("@AccountId", accountId),
                    pLocationId,
                    new SqlParameter("@Name", assetName),
                    new SqlParameter("@Description", assetDescription),
                    pValue,
                    pValueCurrent,
                    pValueReplacement,
                    pValueDepreciated,
                    pValueSalvage,
                    pDisposalCost,
                    pDateAquired,
                    pDatePurchased,
                    pDateDeployed,
                    pDateOutOfService,
                    pDateEntered,
                    pDateReceived,
                    pDateDisposed,
                    pWarrantyLabor,
                    pWarrantyPart,
                    pAssetSort,
                    pFundingSource,
                    new SqlParameter("@Room", ""),
                    new SqlParameter("@PONumber", poNumber),
                    new SqlParameter("@FundingCode", findingCode),
                    new SqlParameter("@SerialNumber", serialNumber),
                    pUnique1,
                    pUnique2,
                    pUnique3,
                    pUnique4,
                    pUnique5,
                    pUnique6,
                    pUnique7,
                    pNewStatusId,
                    new SqlParameter("@oldStatusId", oldStatusId),
                    new SqlParameter("@UpdateUniqueFields", updateUniqueFields),
                    });
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetTypeMakeModel(int departmentId, int assetId, int categoryId, int typeId, int makeId, int modelId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateAssetTypeMakeModel",
                        new[] { 
                                pReturnValue,
                                new SqlParameter("@DepartmentId", departmentId),
                                new SqlParameter("@AssetId", assetId),
                                new SqlParameter("@AssetCategory", categoryId),
                                new SqlParameter("@AssetType", typeId),
                                new SqlParameter("@AssetMake", makeId),
                                new SqlParameter("@AssetModel", modelId)
                                });
            return (int)pReturnValue.Value;
        }

        public static string GetUserFullName(int departmentId, int userId)
        {
            string result = string.Empty;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            SqlParameter pFullName = new SqlParameter("@FullName", SqlDbType.VarChar, 75);
            pFullName.Direction = ParameterDirection.Output;

            UpdateData("sp_SelectUserFullName",
                        new[] { 
                                pReturnValue,
                                pFullName,                
                                new SqlParameter("@DId", departmentId),
                                new SqlParameter("@UId", userId)
                               });
            result = pFullName.Value.ToString();

            return result;
        }

        public static DataTable SelectCompanyAssetStatuses(int dID)
        {
            SqlParameter[] _params = new SqlParameter[1];
            _params[0] = new SqlParameter("@DId", dID);
            return SelectRecords("sp_SelectAssetStatusCompany", _params);
        }

        public static void InsertAssetStatusCompany(int dID, int assetStatusID, bool NonActive, bool EnableUse)
        {
            SqlParameter[] _params = new SqlParameter[]
            {
                new SqlParameter("@DId", dID),
                new SqlParameter("@AssetStatusID", assetStatusID),
                new SqlParameter("@NonActive", NonActive),
                new SqlParameter("@EnableUse", EnableUse)
            };

            UpdateData("sp_InsertAssetStatusCompany", _params);
        }

        public static void AssignAllAssetStatusesToDepartment(int departmnetId)
        {
            UpdateData("sp_InsertAssetStatusCompanyAll",  new SqlParameter[]{new SqlParameter("@DepartmentId", departmnetId)});
        }

        public static void DeleteAssetStatusCompany(int dID, int assetStatusID)
        {
            UpdateData("sp_DeleteAssetStatusCompany", new SqlParameter[]
            {
                 new SqlParameter("@DId", dID),
                 new SqlParameter("@AssetStatusID", assetStatusID),
            });
        }
    }
}
