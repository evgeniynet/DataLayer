using System;
using System.Data;
using System.Data.OleDb;
using bigWebApps.HelpDesk.WebApi.Soap;

namespace bigWebApps.HelpDesk.WebApi.Soap.v1
{
    public class AccessJenIInventoryBll : IDisposable
    {
        string mFileName;
        OleDbConnection AccessConn;
        public AccessJenIInventoryBll(string FileName)
        {
            mFileName = FileName;
            string strAccessConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + FileName;
            AccessConn = new OleDbConnection(strAccessConn);
            AccessConn.Open();
        }

        DataSet Select(string SelectSql)
        {
            DataSet ds = new DataSet();
            OleDbCommand AccessCommand = new OleDbCommand(SelectSql, AccessConn);
            OleDbDataAdapter DataAdapter = new OleDbDataAdapter(AccessCommand);
            //AccessConn.Open();
            DataAdapter.Fill(ds);
            AccessConn.Close();
            return ds;
        }

        public void Dispose()
        {
            if ((((int)AccessConn.State) & (int)ConnectionState.Open) != 0) AccessConn.Close();
        }

        int? GetInt(object o)
        {
            if (o == null || o is DBNull) return null;
            return Convert.ToInt32(o);
        }
        float? GetFloat(object o)
        {
            if (o == null || o is DBNull) return null;
            return (float)Convert.ToDecimal(o);
        }

        bool? GetBool(object o)
        {
            if (o == null || o is DBNull) return null;
            return Convert.ToBoolean(o);
        }

        const int Gb = 1024 * 1024 * 1024;
        const int Mb = 1024 * 1024;
        float? GetValue(object o, int division)
        {
            float? f = GetFloat(o);
            if (f == null) return null;
            return (float)f / division;
        }

        void SelectWin32CPU(AssetComputerData acd, int AID)
        {
            System.Collections.Generic.List<AssetComputerProcessorData> l = new System.Collections.Generic.List<AssetComputerProcessorData>();
            DataSet ds = Select("select * from Win32_Processor where AID=" + AID);
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                AssetComputerProcessorData d = new AssetComputerProcessorData()
                {
                    CPUType = r["Name"].ToString(),
                    CPUSerial = null,
                    CPUVendor = r["Manufacturer"].ToString(),
                    CPUSpeedMHz = GetInt(r["CurrentClockSpeed"]),
                    CPUClass = r["Version"].ToString(),
                    CurrentClockSpeed = GetInt(r["CurrentClockSpeed"]),
                    UniqueId = r["UniqueId"].ToString(),
                    Description = r["Description"].ToString(),
                    Version = r["Version"].ToString(),
                    L2CachSize = GetInt(r["L2CacheSize"]),
                    ExtClock = GetInt(r["ExtClock"]),
                    CurrentVoltage = GetFloat(r["CurrentVoltage"]),
                    DeviceID = r["DeviceID"].ToString()
                };
                l.Add(d);
            }
            acd.Processors = l.ToArray();
        }

        void SelectWin32LocicalDrive(AssetComputerData acd, int AID)
        {
            System.Collections.Generic.List<AssetComputerLogicalDriveData> l = new System.Collections.Generic.List<AssetComputerLogicalDriveData>();
            DataSet ds = Select("select * from Win32_LogicalDisk where AID=" + AID);
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                AssetComputerLogicalDriveData d = new AssetComputerLogicalDriveData()
                {
                    DeviceID = r["DeviceID"].ToString(),
                    FileSystem = r["FileSystem"].ToString(),
                    SizeGB = GetValue(r["Size"], Gb),
                    FreeSpaceGB = GetValue(r["FreeSpace"], Gb),
                    Description = r["Description"].ToString(),
                    VolumeName = r["VolumeName"].ToString(),
                    VolumeSerial = r["VolumeSerialNumber"].ToString(),
                    Compressed = GetBool(r["Compressed"]),
                    DriveType = r["DriveType"].ToString()
                };
                l.Add(d);
            }
            acd.LogicalDrives = l.ToArray();
        }

        void SelectWin32Printer(AssetComputerData acd, int AID)
        {
            System.Collections.Generic.List<AssetComputerPrinterData> l = new System.Collections.Generic.List<AssetComputerPrinterData>();
            DataSet ds = Select("select * from Win32_Printer where AID=" + AID);
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                AssetComputerPrinterData d = new AssetComputerPrinterData()
                {
                    PrinterName = r["Name"].ToString(),
                    PrinterDriver = r["DriverName"].ToString(),
                    PrinterPort = r["PortName"].ToString()
                };
                l.Add(d);
            }
            acd.Printers = l.ToArray();
        }

        void SelectWin32Software(AssetComputerData acd, int AID)
        {
            System.Collections.Generic.List<AssetComputerSoftwareData> l = new System.Collections.Generic.List<AssetComputerSoftwareData>();
            DataSet ds = Select("select * from Win32_RegisteredPackage where AID=" + AID);
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                AssetComputerSoftwareData d = new AssetComputerSoftwareData()
                {
                    SoftwareName = r["RegisteredName"].ToString(),
                    SoftwarePublisher = r["Creator"].ToString(),
                    SoftwareVersion = r["Version"].ToString()
                };
                l.Add(d);
            }
            acd.Softwares = l.ToArray();
        }

        void SelectWin32BaseAsset(AssetComputerData acd, int AID)
        {
            DataSet ds;
            DataRow r;

            ds = Select("select * from Win32_ComputerSystemProduct where AID=" + AID);
            if (ds.Tables[0].Rows.Count > 0)
            {
                r = ds.Tables[0].Rows[0];
                acd.PCSerialNumber = r["SerialNumber_PC"].ToString();
            }

            ds = Select("select * from Win32_Baseboard where AID=" + AID);
            if (ds.Tables[0].Rows.Count == 1)
            {
                r = ds.Tables[0].Rows[0];
                acd.MotherboardSerialNumber = r["SerialNumber"].ToString();
            }

            ds = Select("select * from Win32_ComputerSystem where AID=" + AID);
            if (ds.Tables[0].Rows.Count == 1)
            {
                r = ds.Tables[0].Rows[0];
                acd.RegisteredUser = r["PC_PrimaryOwnerName"].ToString();
                acd.RamMbytes = (int?)GetValue(r["PC_TotalPhysicalMemory"], Mb);
                acd.NetworkName = r["PC_Name"].ToString();
                acd.NetworkDomain = r["PC_Domain"].ToString();
                acd.Make = r["PC_Manufacturer"].ToString();
                acd.Model = r["PC_Model"].ToString();
                acd.AssetName = r["PC_Name"].ToString();
            }

            ds = Select("select * from Win32_OperatingSystem where AID=" + AID);
            if (ds.Tables[0].Rows.Count == 1)
            {
                r = ds.Tables[0].Rows[0];
                acd.OperatingSystem = r["Caption"].ToString();
                acd.OSSerial = r["SerialNumber"].ToString();
            }


            ds = Select("select * from Win32_VideoController where AID=" + AID);
            if (ds.Tables[0].Rows.Count >= 1)
            {
                r = ds.Tables[0].Rows[0];
                acd.VideoDescription = r["Description"].ToString();
                acd.VideoMemoryMbytes = (int?)GetValue(r["AdapterRAM"], Mb);
                acd.VideoHResolution = GetInt(r["CurrentHorizontalResolution"]);
                acd.VideoVResolution = GetInt(r["CurrentVerticalResolution"]);
            }


            ds = Select("select * from  Win32_NetInfo where AID=" + AID + " order by RowID");
            if (ds.Tables[0].Rows.Count >= 1)
            {
                r = ds.Tables[0].Rows[0];
                acd.NetworkCard1IP = r["IPAddr"].ToString();
                acd.NetworkCard1Mask = r["IPMask"].ToString();
                acd.NetworkCard1Gate = r["Gateway"].ToString();
                acd.NetworkCard1Address = r["MacAddr"].ToString();
                acd.NetworkCard1Description = r["Description"].ToString();
            }

            acd.Category = null;
            acd.Type = null;
            acd.Active = true;
        }

        void SelectWin32Asset(AssetComputerData acd, int AID)
        {
            SelectWin32BaseAsset(acd, AID);
            SelectWin32CPU(acd, AID);
            SelectWin32LocicalDrive(acd, AID);
            SelectWin32Printer(acd, AID);
            SelectWin32Software(acd, AID);
        }

        public System.Collections.Generic.List<AssetComputerData> SelectAll(out string ErrorMessage)
        {
            ErrorMessage = null;
            try
            {
                System.Collections.Generic.List<AssetComputerData> AssetComputerList = new System.Collections.Generic.List<AssetComputerData>();
                DataSet ds = Select("select * from Asset");
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    int AID = (int)r["AID"];
                    string AssetType = r["AssetType"].ToString();
                    AssetComputerData acd = new AssetComputerData()
                    {
                        AssetName = r["AssetID"].ToString(),
                    };

                    switch (AssetType)
                    {
                        case "Win32":
                            SelectWin32Asset(acd, AID);
                            break;
                        case "Mac":
                            acd = null;
                            break;
                        case "Linux":
                            acd = null;
                            break;
                    }

                    if (acd != null) AssetComputerList.Add(acd);
                }
                return AssetComputerList;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return null;
            }
            
        }
    }
}
