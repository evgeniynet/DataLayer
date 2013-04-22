using System;
using System.Data;
using System.Data.SqlClient;

namespace bigWebApps.bigWebDesk.Data
{
    /// <summary>
    /// Summary description for Classes.
    /// </summary>
    public class AssetCategories : DBAccess
    {
        public static DataTable SelectAssetCategories(int departmentId)
        {
            return SelectAssetCategories(Guid.Empty, departmentId);
        }

        public static DataTable SelectAssetCategories(Guid OrgId, int departmentId)
        {
            return SelectRecords("sp_SelectAssetCategoriesWithCount", new[] { new SqlParameter("@DepartmentId", departmentId) }, OrgId);
        }

        public static DataTable SelectAssetCategories(int departmentId, int AssetProfileId)
        {
            return SelectRecords("sp_SelectAssetProfileCategories", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetProfileId", AssetProfileId) });
        }

        public static DataTable SelectAssetTypes(int departmentId, int AssetProfileId, int AssetCategoryId)
        {
            return SelectRecords("sp_SelectAssetProfileTypes", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@AssetProfileId", AssetProfileId), new SqlParameter("@AssetCategoryId", AssetCategoryId) });
        }

        public static DataTable SelectAssetCategoryTypes(int DepartmentId, int? AssetTypeId, bool? PortableTypes)
        {
            SqlParameter pPortable = new SqlParameter("@Portable", DBNull.Value);
            if (PortableTypes != null)
                pPortable.Value = PortableTypes;
            return SelectRecords("sp_SelectAssetCategoryTypes", new[] { new SqlParameter("@DepartmentId", DepartmentId), new SqlParameter("@TypeId", AssetTypeId), pPortable });
        }

        public static int AddAssetCategory(Guid organizatioId, int departmentId, string assetName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetCategory", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@Name", assetName) }, organizatioId);
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetCategory(int departmentId, int categoryId, string assetName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetCategory", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@Name", assetName), new SqlParameter("@AssetCategoryId", categoryId) });
            return (int)pReturnValue.Value;
        }

        public static int DeleteAssetCategory(int departmentId, int categoryId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_DeleteAssetCategory", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("ID", categoryId) });
            return (int)pReturnValue.Value;
        }

        public static DataRow GetAssetCategory(Guid organizationId, int departmentId, int categoryId)
        {
            return SelectRecord("sp_SelectAssetCategory", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@CategoryId", categoryId) }, organizationId);
        }

        //Asset types
        public static DataTable SelectAllAssetTypes(int departmentId)
        {
            return SelectRecords("sp_SelectAssetTypes", new[] { new SqlParameter("@DepartmentId", departmentId) });
        }

        public static DataTable SelectAssetTypes(int departmentId, int categoryId)
        {
            return SelectAssetTypes(Guid.Empty, departmentId, categoryId);
        }

        public static DataTable SelectAssetTypes(Guid OrgId, int departmentId, int categoryId)
        {
            return SelectRecords("sp_SelectAssetTypesWithCount", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@CategoryId", categoryId) }, OrgId);
        }

        public static DataTable SelectOneAssetType(int departmentId, int typeId)
        {
            return SelectAssetType(Guid.Empty, departmentId, typeId).Table;
        }

        public static DataRow SelectAssetType(Guid organizationId, int departmentId, int typeId)
        {
            return SelectRecord("sp_SelectAssetType", new[] { new SqlParameter("@DepartmentID", departmentId), new SqlParameter("@TypeId", typeId) }, organizationId);
        }

        public static int AddAssetType(Guid organizationId, int departmentId, int categoryId, string assetName, int? AssetProfileId)
        {
            return AddAssetType(organizationId, departmentId, categoryId, assetName, AssetProfileId, true, false);
        }

        public static int AddAssetType(Guid organizationId, int departmentId, int categoryId, string assetName, int? AssetProfileId, bool AuditEnable, bool IsPortable)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetType", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Name", assetName), new SqlParameter("@CategoryId", categoryId), new SqlParameter("@AssetProfileId", AssetProfileId), new SqlParameter("@AuditEnable", AuditEnable), new SqlParameter("@IsPortable", IsPortable) }, organizationId);
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetType(int departmentId, int categoryId, int typeId, string assetName, int? AssetProfileId, bool AuditEnable, bool IsPortable)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetType", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Name", assetName), new SqlParameter("@CategoryId", categoryId), new SqlParameter("@TypeId", typeId), new SqlParameter("@AssetProfileId", AssetProfileId), new SqlParameter("@AuditEnable", AuditEnable), new SqlParameter("@IsPortable", IsPortable) });
            return (int)pReturnValue.Value;
        }

        public static int DeleteAssetType(int departmentId, int typeId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_DeleteAssetType", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("ID", typeId) });
            return (int)pReturnValue.Value;
        }

        public static DataRow GetAssetType(int departmentId, int typeId)
        {
            return SelectRecord("sp_SelectAssetType", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TypeId", typeId) });
        }

        public static int MoveAssetType(int departmentId, int categoryId, int typeId, int newCategoryId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_SelectMoveAssetType", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@CategoryId", categoryId), new SqlParameter("@TypeId", typeId), new SqlParameter("@NewCategoryId", newCategoryId) });
            return (int)pReturnValue.Value;
        }

        public static int MergeAssetType(int departmentId, int categoryId, int typeId, string assetName, int mergeTypeId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;

            UpdateData("sp_UpdateAssetType", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Name", assetName), new SqlParameter("@CategoryId", categoryId), new SqlParameter("@TypeId", typeId), new SqlParameter("@MergeFlag", 1), new SqlParameter("@MergeTypeId", mergeTypeId) });
            return (int)pReturnValue.Value;
        }

        //Asset makes
        public static DataTable SelectAssetMakes(int departmentId, int typeId)
        {
            return SelectAssetMakes(Guid.Empty, departmentId, typeId);
        }

        public static DataTable SelectAssetMakes(Guid OrgId, int departmentId, int typeId)
        {
            return SelectRecords("sp_SelectAssetMakesWithCount", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@TypeId", typeId) }, OrgId);
        }

        public static int AddAssetMake(Guid organizationId, int departmentId, int typeId, string assetName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetMake", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Make", assetName), new SqlParameter("@TypeId", typeId) }, organizationId);
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetMake(int departmentId, int typeId, int makeId, string assetName)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetMake", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Make", assetName), new SqlParameter("@TypeId", typeId), new SqlParameter("@MakeId", makeId) });
            return (int)pReturnValue.Value;
        }

        public static int DeleteAssetMake(int departmentId, int makeId)
        {
            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_DeleteAssetMake", new[] { pReturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("ID", makeId) });
            return (int)pReturnValue.Value;
        }

        public static DataRow GetAssetMake(int departmentId, int makeId)
        {
            return SelectRecord("sp_SelectAssetMake", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@MakeId", makeId) });
        }

        //Asset models
        public static DataTable SelectAssetModels(int departmentId, int makeId)
        {
            return SelectAssetModels(Guid.Empty, departmentId, makeId);
        }

        public static DataTable SelectAssetModels(Guid OrgId, int departmentId, int makeId)
        {
            return SelectRecords("sp_SelectAssetModels", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@MakeId", makeId) }, OrgId);
        }

        public static int AddAssetModel(Guid organizationId, int departmentId, int makeId, string assetName, string links)
        {
            SqlParameter pLinks = new SqlParameter("@Links", DBNull.Value);
            if (links.Length > 0)
                pLinks.Value = links;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetModel", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Model", assetName), new SqlParameter("@MakeId", makeId), new SqlParameter("@ModelId", DBNull.Value), pLinks }, organizationId);
            return (int)pReturnValue.Value;
        }

        public static int UpdateAssetModel(int departmentId, int makeId, int modelId, string assetName, string links)
        {
            SqlParameter pLinks = new SqlParameter("@Links", DBNull.Value);
            if (links.Length > 0)
                pLinks.Value = links;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetModel", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@Model", assetName), new SqlParameter("@MakeId", makeId), new SqlParameter("@ModelId", modelId), pLinks });
            return (int)pReturnValue.Value;
        }

        public static int DeleteAssetModel(int departmentId, int modelId)
        {
            SqlParameter preturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            preturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_DeleteAssetModel", new[] { preturnValue, new SqlParameter("@DepartmentId", departmentId), new SqlParameter("ID", modelId) });
            return (int)preturnValue.Value;
        }

        public static DataRow GetAssetModel(int departmentId, int modelId)
        {
            return SelectRecord("sp_SelectAssetModel", new[] { new SqlParameter("@DepartmentId", departmentId), new SqlParameter("@ModelId", modelId) });
        }

        public static int UpdateAssetVendor(Guid organizationId, int departmentId, int vendorId, int assetId, string vendorType, string vendorName, string vendorPhone, string vendorFax, string vendorAccountNumber, string vendorNotes)
        {
            SqlParameter pVendorType = new SqlParameter("@vchVendorType", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorType))
                pVendorType.Value = vendorType;

            SqlParameter pVendorName = new SqlParameter("@vchName", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorName))
                pVendorName.Value = vendorName;

            SqlParameter pVendorPhone = new SqlParameter("@vchPhone", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorPhone))
                pVendorPhone.Value = vendorPhone;

            SqlParameter pVendorFax = new SqlParameter("@vchFax", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorFax))
                pVendorFax.Value = vendorFax;

            SqlParameter pVendorAccountNumber = new SqlParameter("@vchAccountNumber", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorAccountNumber))
                pVendorAccountNumber.Value = vendorAccountNumber;

            SqlParameter pVendorNotes = new SqlParameter("@vchNotes", DBNull.Value);
            if (!string.IsNullOrEmpty(vendorNotes))
                pVendorNotes.Value = vendorNotes;

            SqlParameter pReturnValue = new SqlParameter("@RETURN_VALUE", SqlDbType.Int);
            pReturnValue.Direction = ParameterDirection.ReturnValue;
            UpdateData("sp_UpdateAssetVendor", new[] { pReturnValue, new SqlParameter("@DId", departmentId), new SqlParameter("@VendorId", vendorId), new SqlParameter("@intAssetId", assetId), pVendorType, pVendorName, pVendorPhone, pVendorFax, pVendorAccountNumber, pVendorNotes }, organizationId);
            return (int)pReturnValue.Value;
        }

        public static int GetModelIndex(int departmentId, int makeId, int modelId)
        {
            DataTable dtModels = SelectAssetModels(departmentId, makeId);
            if (dtModels != null)
                for (int i = 0; i < dtModels.Rows.Count; i++)
                    if (((int)dtModels.Rows[i]["ModelId"]) == modelId)
                        return i;

            return 0;
        }

        public static int GetMakeDataId(int departmentId, int modelId)
        {
            int makeId = 0;

            DataRow drModel = GetAssetModel(departmentId, modelId);
            if (drModel != null)
                makeId = Int32.Parse(drModel["MakeId"].ToString());

            return makeId;
        }

        public static int GetMakeIndex(int departmentId, int typeId, int makeId)
        {
            DataTable dtMakes = SelectAssetMakes(departmentId, typeId);
            if (dtMakes != null)
                for (int i = 0; i < dtMakes.Rows.Count; i++)
                    if (((int)dtMakes.Rows[i]["MakeId"]) == makeId)
                        return i;

            return 0;
        }

        public static int GetTypeDataId(int departmentId, int makeId)
        {
            int typeId = 0;

            DataRow drMake = GetAssetMake(departmentId, makeId);
            if (drMake != null)
                typeId = Int32.Parse(drMake["TypeId"].ToString());

            return typeId;
        }

        public static int GetTypeIndex(int departmentId, int categoryId, int typeId)
        {
            DataTable dtTypes = SelectAssetTypes(departmentId, categoryId);
            if (dtTypes != null)
                for (int i = 0; i < dtTypes.Rows.Count; i++)
                    if (((int)dtTypes.Rows[i]["TypeId"]) == typeId)
                        return i;

            return 0;
        }

        public static int GetCategoryDataId(int departmentId, int typeId)
        {
            int categoryId = 0;

            DataRow drCategory = GetAssetType(departmentId, typeId);
            if (drCategory != null)
                categoryId = Int32.Parse(drCategory["CategoryId"].ToString());

            return categoryId;
        }

        public static int GetCategoryIndex(int departmentId, int categoryId)
        {
            DataTable dtCategory = SelectAssetCategories(departmentId);
            if (dtCategory != null)
                for (int i = 0; i < dtCategory.Rows.Count; i++)
                    if (((int)dtCategory.Rows[i]["CategoryId"]) == categoryId)
                        return i;

            return 0;
        }

        public static int[] GetSelectedIndexes(int departmentId, int categoryId, int typeId, int makeId, int modelId)
        {
            int[] indexes = null;

            if (categoryId > 0)
            {
                indexes = new int[1];
                indexes[0] = GetCategoryIndex(departmentId, categoryId);
            }

            if (typeId > 0)
            {
                indexes = new int[3];
                int categoryDataId = GetCategoryDataId(departmentId, typeId);
                indexes[0] = GetCategoryIndex(departmentId, categoryDataId);
                indexes[1] = 0;
                indexes[2] = GetTypeIndex(departmentId, categoryDataId, typeId);
            }

            if (makeId > 0)
            {
                indexes = new int[5];
                int typeDataId = GetTypeDataId(departmentId, makeId);
                int categoryDataId = GetCategoryDataId(departmentId, typeDataId);

                indexes[0] = GetCategoryIndex(departmentId, categoryDataId);
                indexes[1] = 0;
                indexes[2] = GetTypeIndex(departmentId, categoryDataId, typeDataId);
                indexes[3] = 0;
                indexes[4] = GetMakeIndex(departmentId, typeDataId, makeId);
            }

            if (modelId > 0)
            {
                indexes = new int[7];
                int makeDataId = GetMakeDataId(departmentId, modelId);
                int typeDataId = GetTypeDataId(departmentId, makeDataId);
                int categoryDataId = GetCategoryDataId(departmentId, typeDataId);

                indexes[0] = GetCategoryIndex(departmentId, categoryDataId);
                indexes[1] = 0;
                indexes[2] = GetTypeIndex(departmentId, categoryDataId, typeDataId);
                indexes[3] = 0;
                indexes[4] = GetMakeIndex(departmentId, typeDataId, makeDataId);
                indexes[5] = 0;
                indexes[6] = GetModelIndex(departmentId, makeDataId, modelId);
            }

            return indexes;
        }
    }
}
