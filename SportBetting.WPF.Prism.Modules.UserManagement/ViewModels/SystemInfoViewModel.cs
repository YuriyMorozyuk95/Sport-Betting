﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using BaseObjects;
using BaseObjects.ViewModels;
using IDCardReader;
using SportBetting.WPF.Prism.Shared;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using Nbt.Services.Scf.CashIn.Validator.CCTalk;
using SportRadar.Common.Logs;
using TranslationByMarkupExtension;
using Command = BaseObjects.Command;
using SportBetting.WPF.Prism.OldCode;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{
    using SLID = Guid;
    public enum DeviceStatus
    {
        STATUS_UNKNOWN = 0,
        STATUS_OK = 1,
        STATUS_NOK = -1
    }

    public class SystemInfoViewModel : BaseViewModel
    {
        DeviceInventory cash_di = new DeviceInventory();
        DeviceInventory coin_di = new DeviceInventory();
        private static ILog Log = LogFactory.CreateLog(typeof(SystemInfoViewModel));

        CCTNote CashValidator = new CCTNote();
        CCTCoin CoinValidator = new CCTCoin();
        Link862Reader CardReader = new Link862Reader("id");
        DispatcherTimer dispatcherTimerCash = new DispatcherTimer();
        DispatcherTimer dispatcherTimerCoin = new DispatcherTimer();
        DispatcherTimer dispatcherTimerIdReader = new DispatcherTimer();

        private static readonly string _VersionFile = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["VersionInfoFilename"];

        public enum SL_GENUINE_STATE
        {
            SL_GEN_STATE_IS_GENUINE = 0,
            SL_GEN_STATE_INVALID_LICENSE = 1,
            SL_GEN_STATE_TAMPERED = 2,
            SL_GEN_STATE_LAST = 3
        }

        [DllImportAttribute("Slwga.dll", EntryPoint = "SLIsGenuineLocal", CharSet = CharSet.None, ExactSpelling = false, SetLastError = false, PreserveSig = true, CallingConvention = CallingConvention.Winapi, BestFitMapping = false, ThrowOnUnmappableChar = false)]
        [PreserveSigAttribute()]
        internal static extern uint SLIsGenuineLocal(ref SLID slid, [In, Out] ref SL_GENUINE_STATE genuineState, IntPtr val3);




        #region Constructors

        public SystemInfoViewModel()
        {
            CashValidator.SetRequestInventory();
            CoinValidator.SetRequestInventory();
            CardReader.RequestReadID();
            dispatcherTimerCash.Tick += new EventHandler(dispatcherTimerCash_Tick);
            dispatcherTimerCash.Interval = new TimeSpan(0, 0, 0, 1, 500);
            dispatcherTimerCoin.Tick += new EventHandler(dispatcherTimerCoin_Tick);
            dispatcherTimerCoin.Interval = new TimeSpan(0, 0, 0, 1, 500);
            dispatcherTimerIdReader.Tick += new EventHandler(dispatcherTimerIdReader_Tick);
            dispatcherTimerIdReader.Interval = new TimeSpan(0, 0, 0, 0, 740);
            dispatcherTimerCash.Start();
            dispatcherTimerCoin.Start();
            dispatcherTimerIdReader.Start();
            _driveTotalSize = new string[2];
            _driveAvailableSize = new string[2];
            _driveName = new string[2];
            BuildVersion = GetBuildVersion();

        }

        #endregion

        #region Properties



        public string[] DriveTotalSize
        {
            get { return _driveTotalSize; }
            set { _driveTotalSize = value; }
        }

        public string[] DriveAvailableSize
        {
            get { return _driveAvailableSize; }
            set { _driveAvailableSize = value; }
        }

        public string[] DriveName
        {
            get { return _driveName; }
            set { _driveName = value; }
        }

        public string OsName
        {
            get { return _osName; }
            set { _osName = value; OnPropertyChanged("OsName"); }
        }

        public string SystemType
        {
            get { return _systemType; }
            set { _systemType = value; OnPropertyChanged("SystemType"); }
        }

        public string InstalledMemory
        {
            get { return _installedMemory; }
            set { _installedMemory = value; OnPropertyChanged("InstalledMemory"); }
        }



        public string PrinterProducer
        {
            get { return _printerProducer; }
            set { _printerProducer = value; OnPropertyChanged("PrinterProducer"); }
        }
        public string PrinterModel
        {
            get { return _printerModel; }
            set { _printerModel = value; OnPropertyChanged("PrinterModel"); }
        }

        public string PrinterDriver
        {
            get { return _printerDriver; }
            set { _printerDriver = value; OnPropertyChanged("PrinterDriver"); }
        }

        public string BillValidatorProducer
        {
            get { return _billValidatorProducer; }
            set { _billValidatorProducer = value; OnPropertyChanged("BillValidatorProducer"); }
        }

        public string BillValidatorModel
        {
            get { return _billValidatorModel; }
            set { _billValidatorModel = value; OnPropertyChanged("BillValidatorModel"); }
        }

        public string BillValidatorSN
        {
            get { return _billValidatorSN; }
            set { _billValidatorSN = value; OnPropertyChanged("BillValidatorSN"); }
        }

        public string BillValidatorFW
        {
            get { return _billValidatorFW; }
            set { _billValidatorFW = value; OnPropertyChanged("BillValidatorFW"); }
        }

        public string CoinValidatorProducer
        {
            get { return _coinValidatorProducer; }
            set { _coinValidatorProducer = value; OnPropertyChanged("CoinValidatorProducer"); }
        }

        public string CoinValidatorModel
        {
            get { return _coinValidatorModel; }
            set { _coinValidatorModel = value; OnPropertyChanged("CoinValidatorModel"); }
        }

        public string CoinValidatorSN
        {
            get { return _coinValidatorSN; }
            set { _coinValidatorSN = value; OnPropertyChanged("CoinValidatorSN"); }
        }

        public string CoinValidatorFW
        {
            get { return _coinValidatorFW; }
            set { _coinValidatorFW = value; OnPropertyChanged("CoinValidatorFW"); }
        }

        public string IdReaderProducer
        {
            get { return _idReaderProducer; }
            set { _idReaderProducer = value; OnPropertyChanged("IdReaderProducer"); }
        }
        public string IdReaderModel
        {
            get { return _idReaderModel; }
            set { _idReaderModel = value; OnPropertyChanged("IdReaderModel"); }
        }

        public string IdReaderSN
        {
            get { return _idReaderSN; }
            set { _idReaderSN = value; OnPropertyChanged("IdReaderSN"); }
        }

        public string IdReaderFW
        {
            get { return _idReaderFW; }
            set { _idReaderFW = value; OnPropertyChanged("IdReaderFW"); }
        }

        public bool IsPrinterConnected
        {
            get { return _isPrinterConnected; }
            set
            {
                _isPrinterConnected = value;
                OnPropertyChanged("IsPrinterConnected");
            }
        }

        public DeviceStatus IsCashValidatorConnected
        {
            get { return _isCashValidatorConnected; }
            set
            {
                _isCashValidatorConnected = value;
                OnPropertyChanged("IsCashValidatorConnected");
            }
        }

        public bool IsCoinValidatorConnected
        {
            get { return _isCoinValidatorConnected; }
            set
            {
                _isCoinValidatorConnected = value;
                OnPropertyChanged("IsCoinValidatorConnected");
            }
        }

        public DeviceStatus IsIdReaderConnected
        {
            get { return _isIdReaderConnected; }
            set
            {
                _isIdReaderConnected = value;
                OnPropertyChanged("IsIdReaderConnected");
            }
        }

        public string IsWindowsGenuine
        {
            get { return _isWindowsGenuine; }
            set
            {
                _isWindowsGenuine = value;
                OnPropertyChanged("IsWindowsGenuine");
            }
        }

        private string _osName;
        private string[] _driveAvailableSize;
        private string[] _driveName;
        private string[] _driveTotalSize;
        private string _systemType;
        private string _installedMemory;

        private string _printerProducer;
        private string _printerModel;
        private string _printerDriver;
        private string _billValidatorProducer;
        private string _billValidatorModel;
        private string _billValidatorSN;
        private string _billValidatorFW;
        private string _coinValidatorProducer;
        private string _coinValidatorModel;
        private string _coinValidatorSN;
        private string _coinValidatorFW;
        private string _idReaderProducer;
        private string _idReaderModel;
        private string _idReaderSN;
        private string _idReaderFW;
        private bool _isPrinterConnected;
        private DeviceStatus _isCashValidatorConnected = DeviceStatus.STATUS_NOK;
        private bool _isCoinValidatorConnected;
        private DeviceStatus _isIdReaderConnected = DeviceStatus.STATUS_NOK;

        private bool IsIdReaderInventoryChecked = false;
        private string IdReaderInventory;
        private string _isWindowsGenuine;

        #endregion

        #region Commands


        public Command IdentifyMonitors { get; private set; }

        #endregion

        #region Methods

        public override void OnNavigationCompleted()
        {
            StationRepository.EnableCashIn();
            ChangeTracker.SystemInfoChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_PRINT_SYSTEM;
            ChangeTracker.AdminTitle2 = MultistringTags.TERMINAL_SYSTEM_INFO;
            ShowSystemInfo("");
            base.OnNavigationCompleted();
        }

        public override void Close()
        {
            StationRepository.DisableCashIn();
            dispatcherTimerCash.Stop();
            dispatcherTimerCoin.Stop();
            dispatcherTimerIdReader.Stop();

            dispatcherTimerCash.Tick -= new EventHandler(dispatcherTimerCash_Tick);
            dispatcherTimerCoin.Tick -= new EventHandler(dispatcherTimerCoin_Tick);
            dispatcherTimerIdReader.Tick -= new EventHandler(dispatcherTimerIdReader_Tick);

            base.Close();
        }

        public bool IsGenuineWindows()
        {
            bool _IsGenuineWindows = false;
            Guid ApplicationID = new Guid("55c92734-d682-4d71-983e-d6ec3f16059f"); //Application ID GUID http://technet.microsoft.com/en-us/library/dd772270.aspx
            SLID windowsSlid = (Guid)ApplicationID;
            try
            {
                SL_GENUINE_STATE genuineState = SL_GENUINE_STATE.SL_GEN_STATE_LAST;
                uint ResultInt = SLIsGenuineLocal(ref windowsSlid, ref genuineState, IntPtr.Zero);
                if (ResultInt == 0)
                {
                    _IsGenuineWindows = (genuineState == SL_GENUINE_STATE.SL_GEN_STATE_IS_GENUINE);
                }
                else
                {
                    Log.Debug(String.Format("IsGenuineWindows: Error getting information: {0}", ResultInt.ToString()));
                }

            }
            catch (Exception ex)
            {
                Log.Debug(String.Format("IsGenuineWindows throw an exception: Exception getting information: {0}", ex.Message));
            }
            return _IsGenuineWindows;
        }

        private string GetBuildVersion()
        {

            const string _VersionRegExpFilter = @"[^a-zA-Z_]*?$";

            if (File.Exists(_VersionFile))
                try
                {
                    using (StreamReader streamReader = new StreamReader(_VersionFile))
                    {
                        return string.Format("{0}", System.Text.RegularExpressions.Regex.Match(streamReader.ReadLine(), _VersionRegExpFilter).Value);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message, ex);
                }
            return string.Empty;
        }

        private void dispatcherTimerCash_Tick(object sender, EventArgs e)
        {
            dispatcherTimerCash.Stop();
            if (GetCashValidatorInventory())
            {
                // dispatcherTimerCash.Stop();
            }
            CashValidator.SetRequestInventory();
            dispatcherTimerCash.Start();
        }

        private void dispatcherTimerIdReader_Tick (object sender, EventArgs e)
        {

            dispatcherTimerIdReader.Stop();
            if (StationRepository.IsIdCardEnabled)
            {
                if (!IsIdReaderInventoryChecked)
                {
                    IdReaderInventory = CardReader.ReadReaderID();
                    IsIdReaderInventoryChecked = true;
                    CardReader.RequestReadSN();
                }
                else
                {
                    IsIdReaderInventoryChecked = false;
                    GetIdReaderInventory();
                    CardReader.RequestReadID();
                }
                dispatcherTimerIdReader.Start();
            }
            else
            {
                IsIdReaderConnected = DeviceStatus.STATUS_UNKNOWN;
                IdReaderProducer = "ID Card Reader is disabled!";
            }
        }

        private void dispatcherTimerCoin_Tick(object sender, EventArgs e)
        {
            dispatcherTimerCoin.Stop();
            if (GetCoinValidatorInventory())
            {
                // dispatcherTimerCoin.Stop();
            }
            CoinValidator.SetRequestInventory();
            dispatcherTimerCoin.Start();
        }

        private void ShowSystemInfo(String s)
        {
            if (IsGenuineWindows())
            {
                IsWindowsGenuine = "Windows is activated";
            }
            else
            {
                IsWindowsGenuine = "Windows is not activated";
            }
            LoadSystemInfo();
            LoadPeripheralsInfo();
        }

        private void LoadSystemInfo()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            int i = 0;
            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true && d.DriveType == DriveType.Fixed && i < 2)
                {
                    DriveName[i] = d.Name;
                    DriveTotalSize[i] = (d.TotalSize / 1073741824).ToString(CultureInfo.InvariantCulture) + " GB";
                    DriveAvailableSize[i] = (d.AvailableFreeSpace / 1073741824).ToString(CultureInfo.InvariantCulture) + " GB";
                    i++;

                }

            }

            OnPropertyChanged("DriveName");
            OnPropertyChanged("DriveTotalSize");
            OnPropertyChanged("DriveAvailableSize");

            var osInfo = Environment.OSVersion;
            OsName = osInfo.VersionString;
            SystemType = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
            InstalledMemory = (new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1000 / 1000 / 1000).ToString(CultureInfo.InvariantCulture) + " GB";
        }


        private bool GetCashValidatorInventory ()
        {
            bool result = false;
            // after over devices will be added, refactoring is needed!!!

            try
            {
                List <Nbt.Services.Scf.CashIn.DeviceInfo> list = StationSettings.GetDeviceInventoryList();

                if (list != null)
                {
                    foreach (Nbt.Services.Scf.CashIn.DeviceInfo di in list)
                    {
                        if (di.device_type == Nbt.Services.Scf.CashIn.DeviceType.BILL_VALIDATOR)
                        {
                            BillValidatorProducer = di.device_producer;
                            BillValidatorModel = di.device_model;
                            BillValidatorSN = di.device_serial_number;
                            BillValidatorFW = di.device_firmware_version;
                            IsCashValidatorConnected = DeviceStatus.STATUS_OK;

                            return true;
                        }
                    }

                }

                cash_di.Clone(CashValidator.GetInventoryData());
                if (!CashValidator.IsDataSetValid())
                {
                    IsCashValidatorConnected = DeviceStatus.STATUS_UNKNOWN;
                }

                if (cash_di != null)
                {
                    BillValidatorProducer = cash_di.ManufacturerID;
                    BillValidatorModel = cash_di.ProductCode;
                    BillValidatorSN = cash_di.SerialNumber;
                    BillValidatorFW = cash_di.SW_Version;
                    if (CashValidator.IsDataSetValid())
                    {
                        if (cash_di.IsInitialized && cash_di.BillOperationMode != 3)
                        {
                            IsCashValidatorConnected = DeviceStatus.STATUS_OK;
                            result = true;
                        }
                        else
                        {
                            IsCashValidatorConnected = DeviceStatus.STATUS_NOK;
                        }
                    }


                }
                else if (CashValidator.IsDataSetValid())
                {
                    IsCashValidatorConnected = DeviceStatus.STATUS_NOK;
                }

            }
            catch
            {
            }

            return result;
        }

        private bool GetCoinValidatorInventory()
        {
            bool result = false;

            try
            {
                List<Nbt.Services.Scf.CashIn.DeviceInfo> list = StationSettings.GetDeviceInventoryList();

                if (list != null)
                {
                    foreach (Nbt.Services.Scf.CashIn.DeviceInfo di in list)
                    {
                        if (di.device_type == Nbt.Services.Scf.CashIn.DeviceType.COIN_ACCEPTOR)
                        {
                            CoinValidatorProducer = di.device_producer;
                            CoinValidatorModel = di.device_model;
                            CoinValidatorSN = di.device_serial_number;
                            CoinValidatorFW = di.device_firmware_version;
                            IsCoinValidatorConnected = true;

                            return true;
                        }
                    }

                }
                coin_di.Clone(CoinValidator.GetInventoryData());

                if (coin_di != null)
                {
                    IsCoinValidatorConnected = coin_di.IsInitialized;
                    CoinValidatorProducer = coin_di.ManufacturerID;
                    CoinValidatorModel = coin_di.ProductCode;
                    CoinValidatorSN = coin_di.SerialNumber;
                    CoinValidatorFW = coin_di.SW_Version;
                    if (IsCoinValidatorConnected)
                    {
                        result = true;
                    }

                }
                else
                {
                    IsCoinValidatorConnected = false;
                }
            }
            catch
            {
            }

            return result;
        }

        private bool GetIdReaderInventory()
        {
            bool result = false;

            string ReaderModel = null;
            string ReaderFW = null;
            string ReaderProducer = null;
            string ReaderSN = null;
            const int MIN_VALID_INVENTORY_LEN = 60;
            const int MIN_VALID_SN_LEN = 2;

            try
            {

                if (IdReaderInventory != null)
                {
                    ReaderSN = CardReader.ReadReaderSN();
                    int len = IdReaderInventory.Length;

                    if (ReaderSN != null)
                    {

                        if (ReaderSN.Length > MIN_VALID_SN_LEN && len > MIN_VALID_INVENTORY_LEN)
                        {
                            int pos = IdReaderInventory.IndexOf(',');
                            if (pos > 0)
                            {
                                ReaderModel = IdReaderInventory.Substring(0, pos);
                            }
                            if (len > pos + 1)
                            {
                                int pos_next = IdReaderInventory.IndexOf(',', pos + 1);
                                if (pos_next > 0)
                                {
                                    ReaderFW = IdReaderInventory.Substring(pos + 1, pos_next - pos - 1);
                                }
                                pos = IdReaderInventory.IndexOf('"', pos_next);
                                if (pos > 0)
                                {
                                    if (len > pos + 1)
                                    {
                                        ReaderProducer = IdReaderInventory.Substring(pos + 1, len - pos - 2);
                                        result = true;
                                    }
                                }

                            }
                        }
                    }

                }

            }
            catch
            {
            }
            finally
            {
                if (result)
                {
                    IsIdReaderConnected = DeviceStatus.STATUS_OK;
                }
                else
                {
                    IsIdReaderConnected = DeviceStatus.STATUS_NOK;
                }
                IdReaderModel = ReaderModel;
                IdReaderProducer = ReaderProducer;
                IdReaderFW = ReaderFW;
                IdReaderSN = ReaderSN;
            }

            return result;
        }



        private void LoadPeripheralsInfo()
        {

            int pos = -1;
            PrinterSettings settings = new PrinterSettings();
            string printerName = settings.PrinterName;

            string query = string.Format("SELECT * from Win32_Printer WHERE Name LIKE '%{0}'", printerName.Replace("\\", "\\\\"));
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection coll = searcher.Get();

            try
            {
                foreach (ManagementObject printer in coll)
                {
                    IsPrinterConnected = !printer["WorkOffline"].ToString().ToLowerInvariant().Equals("true");
                    ManagementObjectSearcher sr =
                                new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PrinterDrive");

                }

                DriversMonitor DrvMonitor = DriversMonitor.Instance;
                DriverData drd = DrvMonitor.GetPrinterDriverInfoA();

                PrinterDriver = drd.driver_file_version;
                PrinterModel = settings.PrinterName;
                if (drd.mnf_name != null)
                {
                    PrinterProducer = drd.mnf_name;
                }
                else
                {
                    if (drd.device_name != null)
                    {
                        pos = drd.device_name.IndexOf("Microsoft");
                        if (pos != -1)
                        {
                            PrinterProducer = "Microsoft";
                        }
                    }
                    if ((pos == -1) && (PrinterModel != null))
                    {
                        pos = PrinterModel.IndexOf(' ');
                        if (pos != -1)
                        {
                            PrinterProducer = PrinterModel.Substring(0, pos);
                        }

                    }
                }

            }
            catch
            {
                IsPrinterConnected = false;
            }


        }


        #endregion
    }
}