﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Accounting.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.Keyboard.ViewModels;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Logs;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;

namespace SportBetting.WPF.Prism.Modules.UserManagement.ViewModels
{
    /// <summary>
    ///     Categories view model.
    /// </summary>
    public class UserManagementViewModel : BaseViewModel
    {
        private static readonly ILog Log = LogFactory.CreateLog(typeof(UserManagementViewModel));

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserManagementViewModel" /> class.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public UserManagementViewModel()
        {
            HidePleaseWait = false;

            //OpenTicketCategory();
            Mediator.Register<string[]>(this, ActivateSendLogs, MsgTag.ZippedLogsUploaded);
            Mediator.Register<bool>(this, ShowMenuAction, MsgTag.ShowMenu);
            ChangeTracker.IsUserManagementWindowVisible = true;
            TerminalAccountingCommand = new Command(OpenTerminalAccounting);
            PrintTestPageCommand = new Command(PrintTestPage);
            ShopPaymentsCommand = new Command(OpenShopPayments);
            ProfitAccountingCommand = new Command(OpenProfitAccounting);
            CashOperationsCommand = new Command(OpenCashOperations);
            BalanceOperationsCommand = new Command(OpenBalance);
            CloseBalance = new Command(OnCloseBalance);
            ShowPayoutCommand = new Command(OnPayout);
            ShowSearchUsersCommand = new Command(OnShowSearch);
            ShowUserRegistrationCommand = new Command(OnShowUserRegistration);
            ShowOperatorRegistrationCommand = new Command(OnOperatorRegistration);
            VerifyCommand = new Command(OnVerify);
            ShowSearchOperatorsCommand = new Command(OnSearchOperators);
            ShowCashHistoryCommand = new Command(OnShowCashHistoryCommand);
            ShowOperatorShiftReportCommand = new Command(onShowOperatorShiftReportCommand);
            ShowOperatorShiftReportsCommand = new Command(onShowOperatorShiftReportsCommand);
            ShowSystemInfoCommand = new Command(OnShowSystemInfo);
            ShowSystemInfoMonitorsCommand = new Command(OnShowSystemInfoMonitors);
            ShowSystemInfoNetworkCommand = new Command(OnShowSystemInfoNetwork);
            SendLogsCommand = new Command(OnSendLogsCommand);
            RestartTerminalCommand = new Command(OnShowRestartTerminal);
            PrintDublicateCommand = new Command(OnPrintDublicate);
            OpenAccountingCategoryCommand = new Command(OpenAccountingCategory);
            OpenTicketCategoryCommand = new Command(OpenTicketCategory);
            OpenSystemCategoryCommand = new Command(OpenSystemCategory);
            OpenAdministrationCategoryCommand = new Command(OpenAdministrationCategory);
            OpenUserCategoryCommand = new Command(OpenUserCategory);
            ShowCardPin = new Command(OnShowCardPin);
            PrintLastTicketCommand = new Command(PrintLastTicket);
            TestModeCommand = new Command(OnTestModeCommand);


            if (!LogSending.sendingThreadIsAlive)
            {
                EnabledSendLogs = true;
                SendInProgress = Visibility.Collapsed;
            }
            else
            {
                EnabledSendLogs = false;
                SendInProgress = Visibility.Visible;
            }


            if (ChangeTracker.CurrentUser.OperatorShiftCheckpointRead || ChangeTracker.CurrentUser.OperatorShiftCheckpointWrite)
                OperatorShiftButtonActive = true;
            else
                OperatorShiftButtonActive = false;


            if (ChangeTracker.CurrentUser.OperatorShiftSettlementRead || ChangeTracker.CurrentUser.OperatorShiftSettlementWrite)
                OperatorShiftReportButtonActive = true;
            else
                OperatorShiftReportButtonActive = false;

            IsCashAcceptorDatasetValid = ChangeTracker.IsCashAcceporDatasetValid;
            IsCashAcceptorLocked = ChangeTracker.LockCashAcceptors;
            Mediator.Register<Tuple<BarCodeConverter.BarcodeType, string>>(this, OpenPaymentView, MsgTag.OpenOperatorPaymentView);
            Mediator.Register<valueField[]>(this, OpenSearchUserView, MsgTag.OpenSearchUserView);

            Mediator.Register<string>(this, ShowCashAcceptorLockedLabel, MsgTag.ShowCashAcceptorLockedLabel);

            ChangeTracker.IsBetdomainViewOpen = true;

            UpdateCashSummary();

            if (PrinterHandler != null)
                PrinterHandler.RefreshNotPrintedCount += PrinterHandler_RefreshNotPrintedCount;
        }

        protected bool SystemInfoChecked
        {
            get { return _systemInfoChecked; }
            set
            {
                _systemInfoChecked = value;
                OnPropertyChanged();
            }
        }

        protected bool VerifyStationChecked
        {
            get { return _verifyStationChecked; }
            set
            {
                _verifyStationChecked = value;
                OnPropertyChanged();
            }
        }

        protected bool SystemInfoMonitorsChecked
        {
            get { return _systemInfoMonitorsChecked; }
            set
            {
                _systemInfoMonitorsChecked = value;
                OnPropertyChanged();
            }
        }

        protected bool SystemInfoNetworkChecked
        {
            get { return _systemInfoNetworkChecked; }
            set
            {
                _systemInfoNetworkChecked = value;
                OnPropertyChanged();
            }
        }

        protected bool CashHistoryChecked
        {
            get { return _cashHistoryChecked; }
            set
            {
                _cashHistoryChecked = value;
                OnPropertyChanged();
            }
        }

        private void ShowMenuAction(bool value)
        {
            ShowMenu = value;
        }

        #endregion

        #region Properties

        private decimal _cashinCurrentAmount;
        private Thickness _panelMargin = new Thickness(0, 0, 0, 0);

        public bool ShowBindCard
        {
            get { return string.IsNullOrEmpty(ChangeTracker.CurrentUser.CardNumber); }
        }

        public bool PinEnabled
        {
            get { return ChangeTracker.CurrentUser.PinEnabled; }
        }

        public bool EnabledSendLogs { get; set; }
        public Visibility SendInProgress { get; set; }

        public bool IsCashAcceptorLocked { get; set; }
        public bool IsCashAcceptorDatasetValid { get; set; }

        public bool OperatorShiftButtonActive { get; set; }
        public bool OperatorShiftReportButtonActive { get; set; }

        public Thickness PanelMargin
        {
            get { return _panelMargin; }
            set { _panelMargin = value; }
        }


        public string CardNumber
        {
            get { return ChangeTracker.CardNumber; }
            set { ChangeTracker.CardNumber = value; }
        }

        public Visibility IsEnablePinButtonVisible
        {
            get
            {
                if (ChangeTracker.CurrentUser.HasActiveCard && StationRepository.OperatorCardPinSetting == 3 && ChangeTracker.CurrentUser.CardNumber != "")
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility IsNewPinButtonVisible
        {
            get
            {
                //if (ChangeTracker.CurrentUser.HasActiveCard && (StationRepository.OperatorCardPinSetting == 3 || StationRepository.OperatorCardPinSetting == 1) && ChangeTracker.CurrentUser.CardNumber != "")
                if (ChangeTracker.CurrentUser.HasActiveCard && (StationRepository.OperatorCardPinSetting == 3 || StationRepository.OperatorCardPinSetting == 1))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }


        public ObservableCollection<FoundOperator> FoundOperators
        {
            get { return ChangeTracker.FoundOperators; }
            set
            {
                ChangeTracker.FoundOperators = value;
                OnPropertyChanged("FoundOperators");
            }
        }

        protected ObservableCollection<FoundUser> FoundUsers
        {
            get { return ChangeTracker.FoundUsers; }
            set { ChangeTracker.FoundUsers = value; }
        }

        public String CashAcceptorLockedMessage
        {
            get { return StationRepository.Active == (int)Shared.Models.Repositories.StationRepository.StationState.Locked ? TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_STATION_LOCKED_BY_ADMIN).ToString() : TranslationProvider.Translate(MultistringTags.TERMINAL_CASH_LOCKED).ToString(); }
        }

        #endregion

        #region Commands

        public Command TerminalAccountingCommand { get; private set; }
        public Command PrintTestPageCommand { get; private set; }
        public Command ShopPaymentsCommand { get; private set; }
        public Command ProfitAccountingCommand { get; private set; }
        public Command BalanceOperationsCommand { get; private set; }
        public Command CashOperationsCommand { get; private set; }
        public Command ShowPayoutCommand { get; set; }
        public Command ShowSearchUsersCommand { get; set; }
        public Command ShowSearchOperatorsCommand { get; set; }
        public Command ShowUserRegistrationCommand { get; set; }
        public Command ShowOperatorRegistrationCommand { get; set; }
        public Command VerifyCommand { get; set; }
        public Command ShowCashHistoryCommand { get; set; }
        public Command ShowOperatorShiftReportCommand { get; set; }
        public Command CloseBalance { get; set; }
        public Command ShowOperatorShiftReportsCommand { get; set; }
        public Command ShowSystemInfoMonitorsCommand { get; set; }
        public Command ShowSystemInfoNetworkCommand { get; set; }
        public Command ShowSystemInfoCommand { get; set; }
        public Command SendLogsCommand { get; set; }
        public Command RestartTerminalCommand { get; set; }
        public Command OpenAccountingCategoryCommand { get; set; }
        public Command OpenTicketCategoryCommand { get; set; }
        public Command OpenUserCategoryCommand { get; set; }
        public Command OpenSystemCategoryCommand { get; set; }
        public Command OpenAdministrationCategoryCommand { get; set; }
        public Command ShowCardPin { get; set; }
        public Command PrintLastTicketCommand { get; set; }
        public Command TestModeCommand { get; set; }
        public Command PrintDublicateCommand { get; set; }

        public bool ShowMenu
        {
            get { return _showMenu; }
            private set
            {
                _showMenu = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        private bool _cashHistoryChecked;
        private bool _showMenu = true;
        private bool _systemInfoChecked;
        private bool _systemInfoMonitorsChecked;
        private bool _systemInfoNetworkChecked;
        private bool _verifyStationChecked;

        private void OpenEmptyView()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            ChangeTracker.TerminalAccountingChecked = false;
            ChangeTracker.ShopPaymentsChecked = false;
            ChangeTracker.ProfitAccountingChecked = false;
            ChangeTracker.CashOperationsChecked = false;
            ChangeTracker.OpenShiftsChecked = false;
            ChangeTracker.CashHistoryChecked = false;
            ChangeTracker.OperatorsShiftsReporChecked = false;
            ChangeTracker.EmptyBoxChecked = true;

            MyRegionManager.NavigateUsingViewModel<EmptyViewModel>(RegionNames.UsermanagementContentRegion);
        }

        public void OpenEmptyViewSystem()
        {
            ChangeTracker.RestartChecked = false;
            ChangeTracker.SystemInfoNetworkChecked = false;
            ChangeTracker.SystemInfoMonitorsChecked = false;
            ChangeTracker.SystemInfoChecked = false;
            ChangeTracker.VerifyStationChecked = false;

            ChangeTracker.PrintTestPageChecked = true;

            MyRegionManager.NavigateUsingViewModel<EmptyViewModel>(RegionNames.UsermanagementContentRegion);
        }

        public DateTime StartDate
        {
            get { return ChangeTracker.StartDateAccounting; }
            set { ChangeTracker.StartDateAccounting = value; }
        }


        public bool EnabledShopPayments
        {
            get { return true; }
        }

        public bool EnabledBalanceOpereations
        {
            get { return true; }
        }

        public Visibility PrintDublicateVisibility
        {
            get { return PrinterHandler != null && PrinterHandler.NotPrintedItemsCount > 0 ? Visibility.Visible : Visibility.Collapsed; }
        }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.AdministratorWindowLoading = true;
            MyRegionManager.NavigateUsingViewModel<KeyboardViewModel>(RegionNames.UserManagementKeyboardRegion);

            if (StationRepository.IsTestMode)
            {
                OpenUserCategory();
            }
            else if (ChangeTracker.CurrentUser.PayoutPaymentNote)
            {
                OpenTicketCategory();
            }
            else if (!ChangeTracker.CurrentUser.PayoutPaymentNote)
            {
                OpenAccountingCategory();
            }
            base.OnNavigationCompleted();
            ChangeTracker.AdministratorWindowLoading = false;
            Mediator.SendMessage<long>(-1, MsgTag.ShowSystemMessage);
        }

        private void PrinterHandler_RefreshNotPrintedCount(object sender, EventArgs e)
        {
            OnPropertyChanged("PrintDublicateVisibility");
        }


        private void OpenBalance()
        {
            ChangeTracker.Balance = null;
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<BalanceCheckpointsViewModel>(RegionNames.UsermanagementContentRegion);

            //MyRegionManager.NavigateUsingViewModel("SportBetting.WPF.Prism.Modules.Accounting.Views.StationBalanceView", RegionNames.UsermanagementContentRegion);
        }

        private void OpenTerminalAccounting()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<TerminalAccountingViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [AsyncMethod]
        private void PrintTestPage()
        {
            PleaseWaitPrintTestPage();
        }

        [PleaseWaitAspect]
        private void PleaseWaitPrintTestPage()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage("test page");
                return;
            }
            PrinterHandler.PrintTestString();
        }

        private void ShowPrinterErrorMessage(string type)
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            if (type == "ticket")
                errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_TICKET) + ", ";
            else
                errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_TEST_PAGE) + ", ";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }

        private void OpenShopPayments()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<ShopPaymentsViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OpenProfitAccounting()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<ProfitAccountingViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OpenCashOperations()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<FilterViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OpenDefaultView(bool command)
        {
            if (ChangeTracker.CurrentUser.PayoutPaymentNote)
            {
                OpenTicketCategory();
            }
            else
            {
                OpenAccountingCategory();
            }
        }

        private void OpenAccountingCategory()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);


            if (OperatorShiftButtonActive)
            {
                onShowOperatorShiftReportCommand();
            }
            else if (ChangeTracker.CurrentUser.ViewCashHistory)
            {
                //MyRegionManager.NavigateUsingViewModel<CashHistoryViewModel>(RegionNames.UsermanagementContentRegion);
                OnShowCashHistoryCommand();
            }
            else if (ChangeTracker.CurrentUser.ViewEmptyBox)
            {
                //if (!ChangeTracker.AdministratorWindowLoading)
                //    OnCloseBalance();
                //OpenCashOperations();
                OpenEmptyView();
            }
            else if (OperatorShiftReportButtonActive)
            {
                //onShowOperatorShiftReportCommand();
                MyRegionManager.NavigateUsingViewModel<OperatorShiftReportsViewModel>(RegionNames.UsermanagementContentRegion);
            }
            else if (ChangeTracker.CurrentUser.ProfitShareCheckpointRead)
            {
                OpenProfitAccounting();
            }
            else if (ChangeTracker.CurrentUser.ShopPaymentsRead)
            {
                OpenShopPayments();
            }
            else if (EnabledShopPayments)
            {
                OpenTerminalAccounting();
            }
        }


        private void OpenTicketCategory()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<PaymentViewModel>(RegionNames.UsermanagementContentRegion);
        }

        public override void Close()
        {
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
            MyRegionManager.CloseAllViewsInRegion(RegionNames.UserManagementKeyboardRegion);

            ChangeTracker.IsUserManagementWindowVisible = false;
            ChangeTracker.IsBetdomainViewOpen = false;
            PrinterHandler.RefreshNotPrintedCount -= PrinterHandler_RefreshNotPrintedCount;
            Mediator.SendMessage<long>(-1, MsgTag.ShowSystemMessage);
            base.Close();
        }

        private void OpenSystemCategory()
        {
            if (ChangeTracker.CurrentUser.VerifyStation)
                OnVerify();
            else if (ChangeTracker.CurrentUser.ViewSystemInfo)
            {
                OnShowSystemInfo();
            }
            else if (ChangeTracker.CurrentUser.ViewSystemInfoMonitors)
            {
                OnShowSystemInfoMonitors();
            }
            else if (ChangeTracker.CurrentUser.ViewSystemInfoNetwork)
            {
                OnShowSystemInfoNetwork();
            }
            else
            {
                OpenEmptyViewSystem();
            }
        }


        private void OpenUserCategory()
        {
            if (ChangeTracker.CurrentUser.UserManagement)
                OnShowSearch();
            else if (ChangeTracker.CurrentUser.CreateUser)
                OnShowUserRegistration();
        }

        private void OpenAdministrationCategory()
        {
            if (ChangeTracker.CurrentUser.AdminManagement)
                OnShowOperatorSearch();
            else if (ChangeTracker.CurrentUser.CreateOperator)
                OnOperatorRegistration();
            else
            {
                MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
                ChangeTracker.AdministrationHiddenChecked = true;
                ChangeTracker.CardAndPinChecked = true;
                MyRegionManager.NavigateUsingViewModel<CardPinViewModel>(RegionNames.UsermanagementContentRegion);
            }
        }


        private void OpenSearchUserView(valueField[] form)
        {
            Log.Debug("BARCODE: got message OpenSearchUserView");
            if (!ChangeTracker.OperatorSearchUserViewOpen)
            {
                MyRegionManager.CloseAllViewsInRegion(RegionNames.UsermanagementContentRegion);
                MyRegionManager.NavigateUsingViewModel<SearchUsersViewModel>(RegionNames.UsermanagementContentRegion);
            }

            if (form != null)
            {
                Mediator.SendMessage(form, MsgTag.OpenUserProfile);
            }
        }

        [AsyncMethod]
        private void OpenPaymentView(Tuple<BarCodeConverter.BarcodeType, string> paymentNoteNumber)
        {
            WaitOverlayProvider.ShowWaitOverlay();
            Log.Debug("BARCODE: got message OpenOperatorPaymentView");
            if (!ChangeTracker.OperatorPaymentViewOpen)
            {
                MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
                var model = MyRegionManager.NavigateUsingViewModel<PaymentViewModel>(RegionNames.UsermanagementContentRegion);
                while (!model.IsReady)
                {
                    Thread.Sleep(1);
                }
                switch (paymentNoteNumber.Item1)
                {
                    case BarCodeConverter.BarcodeType.CREDIT_NOTE:
                        Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                        Mediator.SendMessage(true, MsgTag.SetCreditNoteButton);
                        Mediator.SendMessage(paymentNoteNumber.Item2, MsgTag.LoadPaymentNote);
                        break;
                    case BarCodeConverter.BarcodeType.PAYMENT_NOTE:
                        Mediator.SendMessage(MsgTag.HideKeyboard, MsgTag.HideKeyboard);
                        Mediator.SendMessage(false, MsgTag.SetCreditNoteButton);
                        Mediator.SendMessage(paymentNoteNumber.Item2, MsgTag.LoadPaymentNote);
                        break;
                }
            }
        }

        private void ShowCashAcceptorLockedLabel(string obj)
        {
            IsCashAcceptorDatasetValid = ChangeTracker.IsCashAcceporDatasetValid;
            OnPropertyChanged("CashAcceptorLockedMessage");
            if (IsCashAcceptorLocked != ChangeTracker.LockCashAcceptors)
            {
                IsCashAcceptorLocked = ChangeTracker.LockCashAcceptors;
                OnPropertyChanged("IsCashAcceptorLocked");
            }
        }

        private void onShowOperatorShiftReportCommand()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<OperatorShiftReportViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void onShowOperatorShiftReportsCommand()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<OperatorShiftReportsViewModel>(RegionNames.UsermanagementContentRegion);
            Mediator.SendMessage(true, MsgTag.LoadOperShiftReports);
        }

        private void OnShowCashHistoryCommand()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<CashHistoryViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnSearchOperators()
        {
            FoundOperators = new ObservableCollection<FoundOperator>();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SearchOperatorsViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowCardPin()
        {
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<CardPinViewModel>(RegionNames.UsermanagementContentRegion);
        }

        [WsdlServiceSyncAspect]
        public void askWindow_YesClick(object sender, EventArgs e)
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage();
                return;
            }

            long cashoutId = GetLastCashoutId() + 1;
            Dictionary<Decimal, int> CashinNotes = GetNotesValuesAndCount(cashoutId);
            TransactionQueueHelper.TryRegisterMoneyOnHub(StationRepository.GetUid(ChangeTracker.CurrentUser), BusinessPropsHelper.GetNextTransactionId(), ref _cashinCurrentAmount, false, "STATION_CASH_OUT", (int)ChangeTracker.CurrentUser.AccountId, true);
            foreach (var cashinNote in CashinNotes)
            {
                Log.Debug("cashin notes:" + cashinNote.Key + " amount: " + cashinNote.Value);
            }
            PrinterHandler.PrintCashBalance(CashinNotes, ChangeTracker.LastCashoutDate, DateTime.Now, _cashinCurrentAmount, 0, _cashinCurrentAmount, false, false, ChangeTracker.CurrentUser.Username, GetNumberOfCheckpoints() + 1);
            UpdateCashSummary();
            Mediator.SendMessage("", MsgTag.RefreshCashOperations);
        }

        private int GetNumberOfCheckpoints()
        {
            string sQuery = string.Format("SELECT Count(*) FROM StationCash WHERE StationCash.CashCheckpoint=1");

            int count = 0;
            using (DataTable dt = DataCopy.GetDataTable(sQuery))
            {
                foreach (DataRow dr in dt.Rows)
                {
                    count = DbConvert.ToInt32(dr, "count");
                }
            }
            return count;
        }

        public static Dictionary<Decimal, int> GetNotesValuesAndCount(long startId, long endId = long.MaxValue)
        {
            var cashinNotes = new Dictionary<decimal, int>();
            try
            {
                var lParams = new List<IDbDataParameter> { SqlObjectFactory.CreateParameter("lastCashoutStartId", startId, string.Empty), SqlObjectFactory.CreateParameter("lastCashoutEndId", endId, string.Empty) };


                List<StationCashSr> lStationCash = StationCashSr.GetStationCashListByQuery("SELECT * FROM StationCash where stationcashid >= @lastCashoutStartId and stationcashid <= @lastCashoutEndId order by stationcashid desc", lParams);


                foreach (StationCashSr stationCashBo in lStationCash)
                {
                    if (stationCashBo.MoneyIn)
                    {
                        if (cashinNotes.ContainsKey(stationCashBo.Cash))
                        {
                            cashinNotes[stationCashBo.Cash]++;
                        }
                        else
                        {
                            cashinNotes.Add(stationCashBo.Cash, 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return cashinNotes;
        }

        public static long GetLastCashoutId()
        {
            long lastCashoutId = 0;
            try
            {
                List<StationCashSr> lStationCash = StationCashSr.GetStationCashListByQuery("SELECT * From StationCash where StationCash.CashCheckPoint = 1 order by stationcashid desc", new List<IDbDataParameter>());

                if (lStationCash.Count > 0)
                {
                    lastCashoutId = lStationCash[0].StationCashID;
                }
            }
            catch (Exception ex)
            {
            }
            return lastCashoutId;
        }


        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;
            string errorMessage = "";

            errorMessage = TranslationProvider.Translate(MultistringTags.ERROR_CANNOT_PRINT_BALANCE_NOTE) + ", ";

            switch (status)
            {
                case 0:
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), null, true);
                    return;
                case 4:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_PAPER).ToString();
                    break;
                case 6:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_NO_TONER).ToString();
                    break;
                case 7:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OPEN).ToString();
                    break;
                case 8:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_JAMMED).ToString();
                    break;
                case 9:
                    errorMessage += TranslationProvider.Translate(MultistringTags.ERROR_PRINTER_OFFLINE).ToString();
                    break;
            }

            ShowError(errorMessage, null, true);
        }


        public void OnCloseBalance()
        {
            decimal cashoutCurrentAmount = 0;
            BusinessPropsHelper.GetAccountingAmount(out _cashinCurrentAmount, out cashoutCurrentAmount);

            if (_cashinCurrentAmount - cashoutCurrentAmount > 0)
            {
                string text = TranslationProvider.Translate(MultistringTags.TERMINAL_COLLECT_CASH, _cashinCurrentAmount, StationRepository.Currency);
                QuestionWindowService.ShowMessage(text, null, null, askWindow_YesClick, null);
            }
            else
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_CASH), null, false, 3);
            }
        }

        private void OnPayout()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<PaymentViewModel>(RegionNames.UsermanagementContentRegion);
        }


        private void OnOperatorRegistration()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<CreateOperatorViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowSearch()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            FoundUsers = new ObservableCollection<FoundUser>();
            ChangeTracker.SearchRequest = new List<criteria>();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SearchUsersViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowOperatorSearch()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            ChangeTracker.FoundOperators = new ObservableCollection<FoundOperator>();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SearchOperatorsViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowUserRegistration()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<UserRegistrationViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnVerify()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.NavigateUsingViewModel<SystemStationVerificationViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowSystemInfo()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SystemInfoViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowSystemInfoMonitors()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SystemInfoMonitorsViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void OnShowSystemInfoNetwork()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<SystemInfoNetworkViewModel>(RegionNames.UsermanagementContentRegion);
        }


        private void OnSendLogsCommand()
        {
            LogSending.stationNumber = StationRepository.StationNumber;
            EnabledSendLogs = false;
            SendInProgress = Visibility.Visible;

            OnPropertyChanged("EnabledSendLogs");
            OnPropertyChanged("SendInProgress");

            LogSending.SendLogs();
        }

        private void OnTestModeCommand()
        {
            decimal cashoutCurrentAmount = 0;
            BusinessPropsHelper.GetAccountingAmount(out _cashinCurrentAmount, out cashoutCurrentAmount);

            if (_cashinCurrentAmount - cashoutCurrentAmount > 0)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_PRODUCE_EMPTY_BOX).ToString(), null, false, 3);
            }
            else
            {
                Mediator.SendMessage("testmode", MsgTag.RestartInTestMode);
            }
        }

        private void ActivateSendLogs(string[] s)
        {
            EnabledSendLogs = true;
            SendInProgress = Visibility.Collapsed;
            OnPropertyChanged("EnabledSendLogs");
            OnPropertyChanged("SendInProgress");
        }

        private void OnShowRestartTerminal()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            MyRegionManager.ClearHistory(RegionNames.UsermanagementContentRegion);
            MyRegionManager.NavigateUsingViewModel<RestartViewModel>(RegionNames.UsermanagementContentRegion);
        }

        private void PrintLastTicket()
        {
            MyRegionManager.NavigateUsingViewModel<PrintTicketViewModel>(RegionNames.UsermanagementContentRegion);

        }

        private void OnPrintDublicate()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                ShowPrinterErrorMessage("ticket");
                return;
            }
            if (!PrinterHandler.PrintLastObject(PrinterHandler.NotPrintedItemsCount))
                Log.Error("printing ticket duplicate failed", new Exception());
        }

        private void UpdateCashSummary()
        {
            try
            {
                decimal locationCashPosition, totalStationCash, totalLocationCash, totalLocationPaymentBalance;
                ChangeTracker.TerminalBalance = WsdlRepository.GetCashInfo(StationRepository.StationNumber, out totalStationCash, out locationCashPosition, out totalLocationCash, out totalLocationPaymentBalance);

                ChangeTracker.TotalStationCash = totalStationCash;
                ChangeTracker.LocationCashPosition = locationCashPosition;
                ChangeTracker.LocationBalance = totalLocationCash;
                ChangeTracker.TotalLocationPaymentBalance = totalLocationPaymentBalance;
            }
            catch (Exception e)
            {
                Log.Error(e.Message, e);
            }
        }

        #endregion
    }
}