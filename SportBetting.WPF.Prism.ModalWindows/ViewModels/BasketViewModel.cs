using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using BaseObjects;
using BaseObjects.ViewModels;
using Shared;
using SportBetting.WPF.Prism.Models;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Modules.BetDomains.ViewModels;
using SportBetting.WPF.Prism.Modules.Ticket.ViewModels;
using SportBetting.WPF.Prism.Modules.LiveModule.ViewModels;
using SportBetting.WPF.Prism.OldCode;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportRadar.Common.Collections;
using SportRadar.Common.Enums;
using SportRadar.Common.Logs;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using WsdlRepository.oldcode;
using Timer = System.Timers.Timer;
using SportRadar.DAL.OldLineObjects;

//using WcfService;

namespace SportBetting.WPF.Prism.ModalWindows.ViewModels
{
    public class BasketViewModel : BaseViewModel
    {
        #region Constructors

        private static ILog Log = LogFactory.CreateLog(typeof(BasketViewModel));

        //private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly ScrollViewerModule _ScrollViewerModule;
        private bool _isEnabledPlaceBet = true;
        private bool bIsVerifyingLocks;

        private bool isLockingTournaments;

        public BasketViewModel()
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);
            ChangeTracker.IsBasketOpen = true;
            ChangeTracker.HeaderVisible = ChangeTracker.IsLandscapeMode;
            ChangeTracker.FooterVisible = true;
            ChangeTracker.FooterArrowsVisible = true;

            Mediator.Register<bool>(this, DoAskLoginAnonymous, MsgTag.AskLoginAnonymous);
            _notificationTimer.Elapsed += _notificationTimer_Elapsed;
            _notificationTimer.AutoReset = false;
            WheelLine1 = new WheelLine();
            WheelLine2 = new WheelLine();
            WheelLine3 = new WheelLine();


            ScrollDownStart = new Command(OnScrollDownStartExecute);
            ScrollDownStop = new Command(OnScrollDownStopExecute);
            ScrollUpStart = new Command(OnScrollUpStartExecute);
            ScrollUpStop = new Command(OnScrollUpStopExecute);
            CloseCommand = new Command(OnClose);
            SpinWheel = new Command<string>(OnArrowSpinWheel);
            ChangeStake = new Command<string>(OnChangeStake);
            BankBetCommand = new Command<TipItemVw>(OnBankBet);
            CheckBetCommand = new Command<TipItemVw>(OnCheckBet);
            ClickWheelButton = new Command<string>(OnClickWheelButton);
            RowClickCommand = new Command<TipItemVw>(OnRowClickCommand);
            OnDeleteBetCommand = new Command<TipItemVw>(OnDeleteTipItem);
            DeleteAllBetsCommand = new Command(OnDeleteAllBets);
            OpenLoginCommand = new Command(OnLogin);
            PlaceBetCommand = new Command(OnPlaceBet);
            ChangeStakeSingle = new Command<Ticket>(OnChangeSingleStake);
            StoreTicketCommand = new Command(OnStoreTicket);
            CashOutCommand = new Command(AskPrintCreditNote);
            InsertCreditNoteCommand = new Command(OnInsertCreditNote);
            AdditionalInfo = new Command<ToggleButton>(OnAdditionalInfo);

            ScrollChangedCommand = new Command<double>(ScrollChanged);

            if (TicketHandler.TicketsInBasket.All(c => c.TipItems.All(x => !x.IsChecked)))
                foreach (ITipItemVw selectedOdd in TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems))
                {
                    selectedOdd.IsSelected = true;
                }
            StationRepository.SetCashInDefaultState();
            StationRepository.BarcodeScannerTempActive = true;
            Log.Debug("Enabling scanner." + "TicketCheckerViewModel");

            // IsEnabledPlaceBet = CheckedOddsForBetting.Where(o => o.Match.IsMatchEnabled == false).Count() == 0 ? true : false;
            //Mediator.Register<bool>(this, CloseCurrentWindow, MsgTag.CloseCurrentWindow);
            //Mediator.Register<bool>(this, CloseCurrentWindowA, MsgTag.CloseCurrentWindowA);
            Mediator.Register<long>(this, LockStation, MsgTag.LockStation);
            Mediator.Register<Tuple<MultistringTag, string[]>>(this, ShowNotificationBar, MsgTag.ShowNotificationBar);
            Mediator.Register<MultistringTag>(this, HideNotificationBar, MsgTag.HideNotificationBar);

            Mediator.Register<bool>(this, DoRebindWheel, MsgTag.BasketRebindWheel);
        }



        #endregion

        #region Commands

        public Command<string> SpinWheel { get; private set; }
        public Command CloseCommand { get; private set; }
        public Command<string> ChangeStake { get; private set; }
        public Command<TipItemVw> CheckBetCommand { get; private set; }
        public Command<TipItemVw> BankBetCommand { get; private set; }
        public Command<string> ClickWheelButton { get; private set; }
        public Command<TipItemVw> RowClickCommand { get; private set; }
        public Command<TipItemVw> OnDeleteBetCommand { get; private set; }
        public Command DeleteAllBetsCommand { get; private set; }
        public Command OpenLoginCommand { get; set; }
        public Command PlaceBetCommand { get; private set; }

        public Command StoreTicketCommand { get; private set; }
        public Command<Ticket> ChangeStakeSingle { get; set; }
        public Command CashOutCommand { get; set; }
        public Command InsertCreditNoteCommand { get; set; }

        public Command ScrollDownStart { get; private set; }
        public Command ScrollDownStop { get; private set; }
        public Command ScrollUpStart { get; private set; }
        public Command ScrollUpStop { get; private set; }
        public Command<ToggleButton> AdditionalInfo { get; private set; }

        public Command<double> ScrollChangedCommand { get; private set; }

        #endregion

        #region Properties

        private readonly ColumnDefinition _colCashPoolWidth = new ColumnDefinition();
        private readonly ColumnDefinition _colFeeBonusWidth = new ColumnDefinition();
        private bool _additionalInfoOpen;
        private bool _animateNotificationText;
        private bool _hideNotification;
        private string _notificationText;
        private bool _showNotification;
        private WheelLine _wheelLine1 = new WheelLine();
        private WheelLine _wheelLine2 = new WheelLine();
        private WheelLine _wheelLine3 = new WheelLine();
        private decimal amount;

        public Visibility ShowRightColumn
        {
            get
            {
                if (ChangeTracker.Is34Mode)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowRightColumnInversed
        {
            get
            {
                if (ChangeTracker.Is34Mode)
                    return Visibility.Collapsed;
                else
                    return Visibility.Visible;
            }
        }

        public bool AdditionalInfoOpen
        {
            get { return _additionalInfoOpen; }
            set
            {
                if (_additionalInfoOpen != value)
                {
                    _additionalInfoOpen = value;
                    OnPropertyChanged("AdditionalInfoOpen");
                }
            }
        }

        public GridLength ColumnWidthBankButton
        {
            get
            {
                switch (TicketHandler.TicketState)
                {
                    default:
                        return new GridLength(0);
                        break;
                    case TicketStates.System:
                        return new GridLength(1.1, GridUnitType.Star);
                        break;
                }
            }
        }

        public GridLength ColumnWidthNamePane
        {
            get
            {
                switch (TicketHandler.TicketState)
                {
                    default:
                        return new GridLength(0);
                        break;
                    case TicketStates.System:
                        return new GridLength(8.2, GridUnitType.Star);
                        break;
                    case TicketStates.Single:
                    case TicketStates.MultySingles:
                        return TicketHandler.Count == 1 ? new GridLength(9.3, GridUnitType.Star) : new GridLength(8.1, GridUnitType.Star);
                        break;
                    case TicketStates.Multy:
                        return new GridLength(9.3, GridUnitType.Star);
                        break;
                }
            }
        }

        public GridLength ColumnWidthStakeSelector
        {
            get
            {
                switch (TicketHandler.TicketState)
                {
                    default:
                        return new GridLength(0);
                        break;
                    case TicketStates.Single:
                    case TicketStates.MultySingles:
                        return TicketHandler.Count > 1 ? new GridLength(1.2, GridUnitType.Star) : new GridLength(0);
                        break;
                }
            }
        }

        public ColumnDefinition ColCashPoolWidth
        {
            get { return _colCashPoolWidth; }
        }


        public ColumnDefinition ColFeeBonusWidth
        {
            get { return _colFeeBonusWidth; }
        }


        public TicketWS TicketWS
        {
            get { return ChangeTracker.Ticket; }
            set { ChangeTracker.Ticket = value; }
        }

        protected string LoadedTicketCheckSum
        {
            get { return ChangeTracker.LoadedTicketcheckSum; }
            set { ChangeTracker.LoadedTicketcheckSum = value; }
        }

        protected BarCodeConverter.BarcodeType LoadedTicketType
        {
            get { return ChangeTracker.LoadedTicketType; }
            set { ChangeTracker.LoadedTicketType = value; }
        }

        protected string LoadedTicketNumber
        {
            get { return ChangeTracker.LoadedTicket; }
            set { ChangeTracker.LoadedTicket = value; }
        }

        public bool AskLoginAnonymous
        {
            get { return !(ChangeTracker.CurrentUser is LoggedInUser) && !StationRepository.AllowAnonymousBetting; }
        }


        public WheelLine WheelLine1
        {
            get { return _wheelLine1; }
            set { _wheelLine1 = value; }
        }

        public WheelLine WheelLine2
        {
            get { return _wheelLine2; }
            set { _wheelLine2 = value; }
        }

        public WheelLine WheelLine3
        {
            get { return _wheelLine3; }
            set { _wheelLine3 = value; }
        }


        public bool IsEnabledPlaceBet
        {
            get { return _isEnabledPlaceBet; }
            set
            {
                _isEnabledPlaceBet = value;
                OnPropertyChanged();
            }
        }

        public string NotificationText
        {
            get { return _notificationText; }
            set
            {
                _notificationText = value;
                OnPropertyChanged();
            }
        }

        public bool ShowNotification
        {
            get { return _showNotification; }
            set
            {
                _showNotification = value;
                OnPropertyChanged();
            }
        }


        public bool AnimateNotificationText
        {
            get { return _animateNotificationText; }
            set
            {
                _animateNotificationText = value;
                OnPropertyChanged();
            }
        }

        public bool HideNotification
        {
            get { return _hideNotification; }
            set
            {
                _hideNotification = value;
                OnPropertyChanged();
            }
        }


        public Brush NotificationTextColor
        {
            get { return _notificationTextColor; }
            set
            {
                _notificationTextColor = value;
                OnPropertyChanged();
            }
        }

        public string SystemBanker
        {
            get
            {
                int banksCount = TicketHandler.TicketsInBasket.Sum(c => c.TipItems.Count(x => x.IsBank));
                int waysCount = TicketHandler.TicketsInBasket.Sum(c => c.TipItems.Count(x => x.IsWay));
                int iPathExtra = 0;
                int iPathsCount = StationRepository.PathCount(TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).ToList(), out iPathExtra);
                bool bIsMultiWay = IsMultiway(TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems.Where(c => c.IsChecked)).ToList());
                string sBanks = "";
                if (bIsMultiWay)
                {
                    sBanks = " + " + (iPathsCount).ToString() + " " + TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_WAYS);
                    if (banksCount - waysCount > 0)
                    {
                        sBanks += " + " + (banksCount - waysCount).ToString() + " " + TranslationProvider.Translate(MultistringTags.TERMINAL_BANKER);
                    }
                }
                else
                {
                    sBanks = banksCount > 0 ? " + " + banksCount.ToString() + " " + TranslationProvider.Translate(MultistringTags.TERMINAL_BANKER) : "";
                }

                //return string.Format("{0} {1}/{2} {3}",TranslationProvider.Translate(MultistringTags.SYSTEM).ToString(),
                //                        (WheelLine2.SystemX - (bIsMultiWay ? 0 : banksCount)).ToString(),
                //                        (ChangeTracker.TicketsInBasket[0].NumberOfBets - banksCount).ToString(),
                //                        sBanks);

                if (TicketHandler.TicketsInBasket.Count > 1 && TicketHandler.TicketState == TicketStates.Single)
                {
                    return TranslationProvider.Translate(MultistringTags.SINGLES) as string;
                }

                return string.Format("{0} {1}", WheelLine2.Text, sBanks);
            }
        }

        public bool IsAnonymousUser
        {
            get { return ChangeTracker.CurrentUser is AnonymousUser; }
        }

        public decimal MultyStakeValueByOddId
        {
            get { return 50; }
        }

        private bool IsMultiway(IList<ITipItemVw> tipItems)
        {
            var bdX = new List<long>();
            foreach (ITipItemVw tipItemVw in tipItems)
            {
                if (!bdX.Contains(tipItemVw.BetDomain.BetDomainId))
                {
                    bdX.Add(tipItemVw.BetDomain.BetDomainId);
                }
                else
                {
                    return true;
                }
            }
            return false;
        }

        public void AskPrintCreditNote()
        {
            amount = ChangeTracker.CurrentUser.Cashpool;

            string text = TranslationProvider.Translate(MultistringTags.CASHOUT_TOCREDITNOTE,  string.Format("{0:f2}", amount), Currency);
            var yesButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_OK) as string;
            var noButtonText = TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string;
            QuestionWindowService.ShowMessage(text, yesButtonText, noButtonText, PrintCreditNoteYes, null);
        }

        #endregion

        #region OnCommand

        private readonly Timer _notificationTimer = new Timer(31000);
        private readonly Dictionary<int, WheelLine> _wheelLines = new Dictionary<int, WheelLine>();
        private Brush _notificationTextColor = Brushes.Black;
        private bool _isprinting;

        public int WheelLinesCount
        {
            get { return _wheelLines.Count; }
        }



        #endregion



        #region Methods



        public void OnDeleteTipItem(ITipItemVw tip)
        {
            DeleteTipItem(tip);

            RebindWheel(true);


            if (CheckTicketForExceptions())
            {
                HideNotificationBar(MultistringTags.TERMINAL_TOO_MANY_ODDS);
            }
            if (TicketHandler.TicketsInBasket.Count > 0 && TicketHandler.TicketsInBasket.First().TipItems.Count(x => x.IsChecked) <= StationRepository.GetMaxCombination(TicketHandler.TicketsInBasket.First()))
            {
                HideNotificationBar(MultistringTags.SHOP_FORM_A_MULTIBET_MAY_HAVE_A_MAXIMUM_OF_GAMES);
            }

            if (!TicketHandler.TicketsInBasket.Any(x => x.MaxOddExceeded))
            {
                HideNotificationBar(MultistringTags.TOTAL_ODDS_LIMIT_EXCEEDED);
            }

            if (TicketHandler.TicketsInBasket.Count == 0)
            {
                //Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
                ChangeTracker.BetSelected = false;
                ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                //if (ChangeTracker.SelectedVirtualSports)
                //OnClose();
                //else
                //    Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
                OnClose();
            }

            if (TicketHandler.Stake <= ChangeTracker.CurrentUser.Cashpool)
            {
                HideNotificationBar(MultistringTags.ADD_XX_TO_STAKE);
            }

            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

            if (VerifyTournamentMatchLocks())
                HideNotificationBar(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED);
        }


        private void OnRowClickCommand(TipItemVw obj)
        {
            ChangeTracker.BetDomainViewFromBasket = true;
            ChangeTracker.CurrentMatch = obj.Match.MatchView;
            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
            //OnClose();
        }

        private void OnClickWheelButton(string sLineNumber)
        {
            if (sLineNumber == "1")
            {
                if (isLockingTournaments)
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED).ToString());
                OnSpinWheel("0");
            }
            else if (sLineNumber == "3")
            {
                if (isLockingTournaments)
                    ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED).ToString());
                OnSpinWheel("1");
            }
        }

        public void OnArrowSpinWheel(string direction)
        {
            if (isLockingTournaments)
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED).ToString());
            OnSpinWheel(direction);
        }

        public void OnSpinWheel(string direction)
        {
            if (TicketHandler.TicketsInBasket.Count == 0)
                return;

            if (!VerifyTournamentMatchLocks())
                return;

            if (direction == "0")
            {
                ChangeTracker.BasketWheelPosition--;
            }
            else
            {
                ChangeTracker.BasketWheelPosition++;
            }

            SetWheelPosition();

            if ((WheelLine2.TicketState == TicketStates.Multy || WheelLine2.TicketState == TicketStates.Single) && TicketHandler.TicketsInBasket[0].TipItems.Count(x => x.IsChecked) > 2)
            {
                // reset all bankers
                IEnumerable<ITipItemVw> varBanks = TicketHandler.TicketsInBasket[0].TipItems.Where(x => x.IsBank);
                foreach (ITipItemVw tipItemVw in varBanks)
                {
                    tipItemVw.IsBank = false;
                }

                // .. and recreate wheel
                if (WheelLine2.TicketState == TicketStates.Single)
                    ChangeTracker.BasketWheelPosition = 0;
                else if (WheelLine2.TicketState == TicketStates.Multy)
                    ChangeTracker.BasketWheelPosition = 1;

                RebindWheel(true);
            }

            TicketHandler.TicketState = WheelLine2.TicketState;

            TicketHandler.TicketsInBasket[0].SystemX = WheelLine2.SystemX;

            TicketHandler.UpdateTicket();
            if (TicketHandler.TicketsInBasket[0].Stake > TicketHandler.TicketsInBasket[0].MaxBet || TicketHandler.TicketsInBasket[0].CurrentTicketPossibleWin > TicketHandler.TicketsInBasket[0].MaxWin)
            {
                ShowNotificationBar(MultistringTags.TERMINAL_STAKE_EXCEEDED_MAXBET);
                TicketHandler.TicketsInBasket[0].Stake = TicketHandler.TicketsInBasket[0].MaxBet;
                TicketHandler.UpdateTicket();
            }

            if (TicketHandler.Stake <= ChangeTracker.CurrentUser.Cashpool)
            {
                HideNotificationBar(MultistringTags.ADD_XX_TO_STAKE);
            }


            OnPropertyChanged("WheelLine1");
            OnPropertyChanged("WheelLine2");
            OnPropertyChanged("WheelLine3");

            OnPropertyChanged("ColumnWidthBankButton");
            OnPropertyChanged("ColumnWidthNamePane");
            OnPropertyChanged("ColumnWidthStakeSelector");
        }

        public void RebindWheel(bool value)
        {
            lock (_wheelLines)
            {
                FillWheelLinesDictionary();
                SetWheelPosition();
            }
        }

        public int CountValidTipItems()
        {
            int amountOfValidTipItems = 0;
            var matchIds = new List<long>();

            foreach (Ticket ticket in TicketHandler.TicketsInBasket)
            {
                foreach (ITipItemVw tipItem in ticket.TipItems)
                {
                    if (tipItem.IsChecked && !matchIds.Contains(tipItem.Match.MatchId))
                    {
                        amountOfValidTipItems++;
                        matchIds.Add(tipItem.Match.MatchId);
                    }
                }
            }

            return amountOfValidTipItems;
        }

        private void FillWheelLinesDictionary()
        {
            _wheelLines.Clear();

            int iSystemY = DataBinding.TipListInfo.SystemY;
            int iSystemX = DataBinding.TipListInfo.SystemX;

            //int amountOfValidTipItems = TicketHandler.TicketsInBasket.Sum(c => c.TipItems.Count(x => x.IsChecked)); //only checked bets are valid
            int amountOfValidTipItems = CountValidTipItems();

            var tempTicket = new Ticket();
            foreach (ITipItemVw tipVw in TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems.Where(y => y.IsChecked)))
            {
                tempTicket.TipItems.Add(tipVw);
            }

            tempTicket.TicketState = TicketStates.System;
            tempTicket.SystemX = 2;
            tempTicket.User = ChangeTracker.CurrentUser;
            DataBinding.UpdateSystemOrCombiticket(tempTicket);
            iSystemY = DataBinding.TipListInfo.SystemY;
            iSystemX = DataBinding.TipListInfo.SystemX;

            if (TicketHandler.TicketsInBasket.Count == 0 || tempTicket.TipItems.Count == 0)
            {
                var wlSingleX = new WheelLine();
                wlSingleX.Text = "";
                wlSingleX.SystemX = 0;
                wlSingleX.TicketState = TicketStates.Empty;
                _wheelLines.Add(1, wlSingleX);
                return;
            }

            // SINGLE 
            var wlSingle = new WheelLine();
            wlSingle.Text = TranslationProvider.Translate(MultistringTags.TERMINAL_SINGLEBET) as string;
            wlSingle.SystemX = 0;
            wlSingle.TicketState = TicketStates.Single;
            _wheelLines.Add(1, wlSingle);
            if (TicketHandler.TicketState == TicketStates.Single)
                ChangeTracker.BasketWheelPosition = 0;

            if (amountOfValidTipItems >= 2)
            {
                // multiple
                var wlMultiple = new WheelLine();
                wlMultiple.Text = TranslationProvider.Translate(MultistringTags.Multiple) as string;
                wlMultiple.SystemX = 0;
                wlMultiple.TicketState = TicketStates.Multy;
                _wheelLines.Add(2, wlMultiple);
                //_wheelLines.Add(1, wlMultiple);
                if (TicketHandler.TicketState == TicketStates.Multy && amountOfValidTipItems == 2) //TicketHandler.TicketsInBasket[0].TipItems.Count
                    //if (TicketHandler.TicketState == TicketStates.Multy)
                    ChangeTracker.BasketWheelPosition = 1;


                if (LimitHandling.SystemBetYAllowed(amountOfValidTipItems, TicketHandler.TicketsInBasket[0]))
                {
                    //for (int i = 2; i < (bIsMultiWay ? iSystemY : iTipsCount); i++) //
                    for (int i = 2; i < iSystemY; i++) //
                    {
                        var wlX = new WheelLine();
                        wlX.Text = string.Format("{0} {1}/{2}", TranslationProvider.Translate(MultistringTags.SYSTEM) as string, i.ToString(), iSystemY);
                        wlX.SystemX = i; // +iBanksCount;
                        wlX.TicketState = TicketStates.System;
                        _wheelLines.Add(i + 1, wlX);
                        //_wheelLines.Add(i, wlX);
                        if (TicketHandler.TicketState == TicketStates.System && TicketHandler.TicketsInBasket[0].SystemX == wlX.SystemX)
                            ChangeTracker.BasketWheelPosition = _wheelLines.Count - 1;
                    }
                }
            }
        }

        private void SetWheelPosition()
        {
            if (_wheelLines.Count == 0)
            {
                OnPropertyChanged("SystemBanker");
                OnPropertyChanged("TicketState");
                return;
            }

            if (_wheelLines.Count == 1)
            {
                // exceptional situation when we need to set one item to center
                WheelLine1.Text = "";
                WheelLine2 = _wheelLines[1];
                WheelLine3.Text = "";
            }
            else if (_wheelLines.Count == 2)
            {
                if (ChangeTracker.BasketWheelPosition <= 0)
                {
                    ChangeTracker.BasketWheelPosition = 0;
                    WheelLine1 = new WheelLine();
                    WheelLine2 = _wheelLines[1];
                    WheelLine3 = _wheelLines[2];
                }
                if (ChangeTracker.BasketWheelPosition >= 1)
                {
                    ChangeTracker.BasketWheelPosition = 1;
                    WheelLine1 = _wheelLines[1];
                    WheelLine2 = _wheelLines[2];
                    WheelLine3 = new WheelLine();
                }
            }
            else
            {
                #region Line1

                // line 1
                if (ChangeTracker.BasketWheelPosition == 0)
                {
                    WheelLine1 = _wheelLines[_wheelLines.Count];
                }
                else if (ChangeTracker.BasketWheelPosition == -1)
                {
                    WheelLine1 = _wheelLines[_wheelLines.Count - 1];
                }
                else if (ChangeTracker.BasketWheelPosition == -2)
                {
                    WheelLine1 = _wheelLines[_wheelLines.Count - 2];
                    ChangeTracker.BasketWheelPosition = _wheelLines.Count - 2;
                }
                else if (ChangeTracker.BasketWheelPosition == _wheelLines.Count + 1)
                {
                    WheelLine1 = _wheelLines[1];
                }
                else
                {
                    WheelLine1 = _wheelLines[ChangeTracker.BasketWheelPosition];
                }

                #endregion

                #region Line2

                // line 2
                if (ChangeTracker.BasketWheelPosition == 0)
                {
                    WheelLine2 = _wheelLines[1];
                }
                else if (ChangeTracker.BasketWheelPosition == -1)
                {
                    WheelLine2 = _wheelLines[_wheelLines.Count];
                }
                else if (ChangeTracker.BasketWheelPosition + 1 == _wheelLines.Count + 1)
                {
                    WheelLine2 = _wheelLines[1];
                }
                else if (ChangeTracker.BasketWheelPosition + 1 == _wheelLines.Count + 2)
                {
                    WheelLine2 = _wheelLines[2];
                }
                else
                {
                    WheelLine2 = _wheelLines[ChangeTracker.BasketWheelPosition + 1];
                }

                #endregion

                #region Line3

                // line 3
                if (ChangeTracker.BasketWheelPosition == 0)
                {
                    WheelLine3 = _wheelLines[2];
                }
                else if (ChangeTracker.BasketWheelPosition + 2 == _wheelLines.Count + 1)
                {
                    WheelLine3 = _wheelLines[1];
                }
                else if (ChangeTracker.BasketWheelPosition + 2 == _wheelLines.Count + 2)
                {
                    WheelLine3 = _wheelLines[2];
                }
                else if (ChangeTracker.BasketWheelPosition + 2 == _wheelLines.Count + 3)
                {
                    WheelLine3 = _wheelLines[3];
                    ChangeTracker.BasketWheelPosition = 1;
                }
                else
                {
                    WheelLine3 = _wheelLines[ChangeTracker.BasketWheelPosition + 2];
                }

                #endregion
            }

            if (TicketHandler.TicketsInBasket.Count > 0)
            {
                TicketHandler.TicketState = WheelLine2.TicketState;
                if (TicketHandler.Stake > ChangeTracker.CurrentUser.Cashpool)
                {
                    decimal amountRequired = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                    if (amountRequired < 0)
                    {
                        ShowNotificationBar(MultistringTags.ADD_XX_TO_STAKE, new[] { (-amountRequired).ToString() });
                    }
                }

                TicketHandler.TicketsInBasket[0].SystemX = WheelLine2.SystemX;
            }
            TicketHandler.TicketState = WheelLine2.TicketState;
            OnPropertyChanged("WheelLine1");
            OnPropertyChanged("WheelLine2");
            OnPropertyChanged("WheelLine3");
            OnPropertyChanged("SystemBanker");
        }

        private void OnAdditionalInfo(ToggleButton obj)
        {
            AdditionalInfoOpen = !AdditionalInfoOpen;
        }

        private void CheckMinBetAboveMaxBet()
        {
            if (TicketHandler.MinBet > TicketHandler.MaxBet)
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_BASKET_MINBET_ABOVE_MAXBET).ToString());
        }

        public void DoRebindWheel(bool obj)
        {
            RebindWheel(true);
            if (obj)
            {
                Dispatcher.Invoke(() =>
                    {
                        _ScrollViewerModule.OnScrollDownStartExecute(GetScrollviewer("scroller"), null, true);
                    });
            }
            VerifyTournamentMatchLocks();
        }


        public override void OnNavigationCompleted()
        {
            if (TicketHandler.TicketState == TicketStates.Multy)
                ChangeTracker.BasketWheelPosition = 1;

            RebindWheel(true);
            VerifyTournamentMatchLocks();

            CheckMinBetAboveMaxBet();

            base.OnNavigationCompleted();
        }

        private void OnChangeSingleStake(Ticket obj)
        {
            bool prevValue = obj.IsEditingStake;
            foreach (Ticket ticket1 in TicketHandler.TicketsInBasket)
            {
                ticket1.IsEditingStake = false;
            }
            obj.IsEditingStake = !prevValue;
        }

        private void LoginYes(object sender, EventArgs e)
        {
            Mediator.SendMessage(MsgTag.OpenLogin, MsgTag.OpenLogin);
        }

        private void OnChangeStake(string obj)
        {
            if (StationRepository.AllowAnonymousBetting && ChangeTracker.CurrentUser.Cashpool == 0 && ChangeTracker.CurrentUser is AnonymousUser)
            {
                ShowNotificationBar(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN_OR_PAY_IN);
                return;
            }

            if (!StationRepository.AllowAnonymousBetting && ChangeTracker.CurrentUser.Cashpool == 0 && ChangeTracker.CurrentUser is AnonymousUser)
            {
                ShowNotificationBar(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN);
                return;
            }

            Tuple<MultistringTag, string[], bool> returnString = null;

            if (ChangeTracker.CurrentUser is AnonymousUser && !StationRepository.AllowAnonymousBetting)
            {
                QuestionWindowService.ShowMessage(TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_NOT_LOGGED_IN) as string, TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_LOGIN) as string, TranslationProvider.Translate(MultistringTags.SHOP_FORM_CANCEL) as string, LoginYes, null);
                return;
            }

            if (TicketHandler.TicketsInBasket.Any(x => x.IsEditingStake))
            {
                decimal amount = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                if (amount < 0)
                    amount = 0;
                returnString = TicketHandler.OnChangeStake(obj, TicketHandler.TicketsInBasket.First(x => x.IsEditingStake), amount);
            }
            else
            {
                decimal amount = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                if (obj == "max")
                {
                    if (TicketHandler.TicketsInBasket.Count(x => x.TipItems.Any(c => c.IsChecked)) == 0)
                        return;

                    var dividedAmount = (int)(ChangeTracker.CurrentUser.Cashpool / TicketHandler.TicketsInBasket.Count(x => x.TipItems.Any(c => c.IsChecked)) * 100);
                    amount = ((decimal)dividedAmount / 100);
                }
                foreach (Ticket ticket in TicketHandler.TicketsInBasket.Where(x => x.TipItems.Any(c => c.IsChecked)))
                {
                    if (obj != "max")
                    {
                        amount = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                    }
                    if (obj == "back")
                    {
                        returnString = TicketHandler.OnChangeStake(obj, ticket, ChangeTracker.CurrentUser.Cashpool);
                    }
                    else if (obj == "clear")
                    {
                        returnString = TicketHandler.OnChangeStake(obj, ticket, ChangeTracker.CurrentUser.Cashpool);
                    }
                    else
                    {
                        if (amount > 0)
                        {
                            returnString = TicketHandler.OnChangeStake(obj, ticket, amount);
                        }
                        else
                        {
                            returnString = TicketHandler.OnChangeStake(obj, ticket, 0);
                        }
                    }
                }
            }

            decimal amountRequired = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
            if (amountRequired < 0 && (returnString == null || !returnString.Item3))
            {
                returnString = new Tuple<MultistringTag, string[], bool>(MultistringTags.ADD_XX_TO_STAKE, new[] { (-amountRequired).ToString() }, true);
            }
            else if (amountRequired >= 0)
            {
                HideNotificationBar(MultistringTags.ADD_XX_TO_STAKE);
            }

            if (returnString != null)
            {
                if (returnString.Item3)
                    ShowNotificationBar(returnString.Item1, returnString.Item2);
                if (!returnString.Item3)
                    HideNotificationBar(returnString.Item1);
            }
        }


        private void OnBankBet(TipItemVw obj)
        {
            //bool previousValue = obj.IsBank;
            obj.IsBank = !obj.IsBank;

            if (obj.IsBank)
            {
                if (TicketHandler.TicketsInBasket[0].SystemY - TicketHandler.TicketsInBasket[0].SystemX == 1)
                    DataBinding.ChangeSystemX(-1, TicketStates.System, TicketHandler.TicketsInBasket[0]);
                if (WheelLine3.SystemX == 0 && WheelLinesCount > 3)
                    ChangeTracker.BasketWheelPosition--;
            }
            //else
            //{
            //    //if (WheelLine3.SystemX != 0)
            //    //    ChangeTracker.BasketWheelPosition++;
            //}
            //DataBinding.UpdateSystemOrCombiticket(ChangeTracker.TicketsInBasket[0]);
            int a = DataBinding.TipListInfo.PathCount;
            int iBanksCount = TicketHandler.TicketsInBasket[0].TipItems.Count(x => x.IsBank);
            int iTipsCount = TicketHandler.TicketsInBasket[0].TipItems.Count(x => x.IsChecked);

            int b = a + iBanksCount + iTipsCount;
            RebindWheel(true);

            TicketHandler.UpdateTicket();

            OnPropertyChanged("SystemBanker");
        }

        private void OnCheckBet(TipItemVw tip)
        {
            if (tip.IsChecked && !CheckTicketForExceptions())
            {
                tip.IsChecked = false;
                ShowNotificationBar(MultistringTags.TERMINAL_TOO_MANY_ODDS);
                return;
            }

            if (tip.IsChecked)
            {
                HideNotificationBar(MultistringTags.ADD_MORE_BETS);
                if (TicketHandler.TicketsInBasket.First().TipItems.Count(x => x.IsChecked) >= StationRepository.GetMinCombination(TicketHandler.TicketsInBasket.First()))
                {
                    HideNotificationBar(MultistringTags.SHOP_FORM_COMBINATION_DOES_NOT_REACH_MINIMUM);
                }
                ChangeSystem(1);
            }
            else
            {
                if (TicketHandler.TicketsInBasket.Count > 1)
                {
                    Ticket ticket = TicketHandler.TicketsInBasket.Where(x => x.TipItems.Contains(tip)).First();
                    ticket.Stake = 0;
                    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                }
                ChangeSystem(-1);
                tip.IsBank = false;
                if (TicketHandler.TicketState == TicketStates.System)
                {
                    List<ITipItemVw> checkedways = TicketHandler.TicketsInBasket[0].TipItems.Where(x => x.Match.MatchId == tip.Match.MatchId).Where(x => x.IsChecked).ToList();
                    if (checkedways.Count == 1)
                    {
                        foreach (ITipItemVw checkedway in checkedways)
                        {
                            checkedway.IsBank = false;
                        }
                    }
                }

                if (ChangeTracker.BasketWheelPosition > 2)
                    ChangeTracker.BasketWheelPosition--;
            }

            TicketHandler.UpdateTicket();

            if (CheckTicketForExceptions())
            {
                HideNotificationBar(MultistringTags.TERMINAL_TOO_MANY_ODDS);
            }
            if (TicketHandler.TicketsInBasket.First().TipItems.Count(x => x.IsChecked) <= StationRepository.GetMaxCombination(TicketHandler.TicketsInBasket.First()))
            {
                HideNotificationBar(MultistringTags.SHOP_FORM_A_MULTIBET_MAY_HAVE_A_MAXIMUM_OF_GAMES);
            }

            if (!TicketHandler.TicketsInBasket.Any(x => x.MaxOddExceeded))
            {
                HideNotificationBar(MultistringTags.TOTAL_ODDS_LIMIT_EXCEEDED);
            }
            if (TicketHandler.Stake <= ChangeTracker.CurrentUser.Cashpool)
            {
                HideNotificationBar(MultistringTags.ADD_XX_TO_STAKE);
            }

            RebindWheel(true);
            if (VerifyTournamentMatchLocks())
                HideNotificationBar(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED);

            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
        }

        public void ShowNotificationBar(Tuple<MultistringTag, string[]> text)
        {
            ShowNotificationBar(text.Item1, text.Item2);
        }


        private void _notificationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            decimal amountRequired = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
            if (amountRequired < 0)
            {
                ShowNotificationBar(MultistringTags.ADD_XX_TO_STAKE, new[] { (-amountRequired).ToString() });
            }
            else
            {
                HideNotificationBar(ChangeTracker.LastNotificationTag);
            }
        }

        public void ShowNotificationBar(MultistringTag multistringTag, params string[] args)
        {
            ChangeTracker.LastNotificationTag = multistringTag;
            Dispatcher.Invoke(() =>
            {
                if (multistringTag == MultistringTags.ADD_XX_TO_STAKE)
                {
                    NotificationTextColor = Brushes.Red;
                }
                else
                {
                    NotificationTextColor = Brushes.Black;
                    decimal amountRequired = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                    if (amountRequired < 0)
                    {
                        _notificationTimer.Interval = 5000;
                    }
                    else
                    {
                        _notificationTimer.Interval = 31000;
                    }
                }
                ShowNotification = true;

                AnimateNotificationText = true;
            });

            if (args == null || args.Length == 0)
            {
                NotificationText = TranslationProvider.Translate(multistringTag) as string;
            }
            else
            {
                NotificationText = TranslationProvider.Translate(multistringTag, args);
            }
            if (NotificationTextColor == Brushes.Black)
            {
                _notificationTimer.Stop();
                _notificationTimer.Start();
            }
            Dispatcher.Invoke(() =>
            {
                AnimateNotificationText = false;
                ShowNotification = false;
            });
        }


        private void HideNotificationBar(MultistringTag multistringTag)
        {
            if (ChangeTracker.LastNotificationTag == multistringTag)
            {
                ChangeTracker.LastNotificationTag = null;

                Dispatcher.Invoke(() =>
                {
                    HideNotification = true;
                    HideNotification = false;
                });
                NotificationText = null;
            }
        }

        public override void Close()
        {
            ChangeTracker.IsBasketOpen = false;
            ChangeTracker.HeaderVisible = true;
            ChangeTracker.FooterVisible = true;
            ChangeTracker.FooterArrowsVisible = true;

            StationRepository.SetCashInDefaultState();
            StationRepository.BarcodeScannerTempActive = false;

            base.Close();
        }

        private void OnClose()
        {
            Log.Debug("Disabling scanner." + "TicketCheckerViewModel");
            //Close();

            MyRegionManager.NavigatBack(RegionNames.ContentRegion);

            MyRegionManager.ClearForwardHistory(RegionNames.ContentRegion);
        }


        private void OnDeleteAllBets()
        {
            while (TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).Any())
            {
                DeleteTipItem(TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).First());
            }
            ChangeTracker.BetSelected = false;
            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
            ChangeTracker.BasketWheelPosition = 0;
            RebindWheel(true);
            //if (ChangeTracker.SelectedVirtualSports)
            OnClose();
            //else
            //    Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
        }

        private void OnScrollDownStartExecute()
        {
            _ScrollViewerModule.OnScrollDownStartExecute(GetScrollviewer("scroller"));
        }

        private void OnScrollDownStopExecute()
        {
            _ScrollViewerModule.OnScrollDownStopExecute();
        }

        private void OnScrollUpStartExecute()
        {
            _ScrollViewerModule.OnScrollUpStartExecute(GetScrollviewer("scroller"));
        }

        private void OnScrollUpStopExecute()
        {
            _ScrollViewerModule.OnScrollUpStopExecute();
        }

        private void OnLogin()
        {
            Mediator.SendMessage(MsgTag.OpenLogin, MsgTag.OpenLogin);
        }



        private void OnStoreTicket()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                return;
            }
            if (TicketHandler.TicketsInBasket[0].TipItems.Count < 1)
            {
                ShowNotificationBar(MultistringTags.TERMINAL_FORM_NO_ODDS_SELECTED);
                return;
            }
            ShowStoreTicketPin();

            if (TicketHandler.TicketsInBasket[0].TipItems.Count < 1)
                OnClose();
        }



        private void OnInsertCreditNote()
        {
            OnClose();
            LoadedTicketNumber = "";
            LoadedTicketCheckSum = "";
            LoadedTicketType = BarCodeConverter.BarcodeType.CREDIT_NOTE;
            StationRepository.BarcodeScannerTempActive = true;
            MyRegionManager.NavigateUsingViewModel<TicketCheckerViewModel>(RegionNames.ContentRegion);
        }


        [AsyncMethod]
        private void LoadRestoreTicket()
        {
            LoadRestoreTicketPleaseWait();
        }

        [PleaseWaitAspect]
        private void LoadRestoreTicketPleaseWait()
        {
            string result = "1";
            long id;
            try
            {
                result = WsdlRepository.GetAccountByTicket(BarCodeConverter.FullTicketNumber);
            }
            catch (Exception)
            {
            }
            long.TryParse(result, out id);
            if (id == ChangeTracker.CurrentUser.AccountId || id == 1)
            {
                Close();
                LoadedTicketNumber = BarCodeConverter.TicketNumber;
                if (BarCodeConverter.Type != null)
                    LoadedTicketType = (BarCodeConverter.BarcodeType)BarCodeConverter.Type;

                LoadedTicketCheckSum = BarCodeConverter.CheckSum;

                Mediator.SendMessage("", MsgTag.OpenStoredTicket);
            }
            else
            {
                ShowNotificationBar(MultistringTags.THIS_TICKET_DOES_NOT_BELONG_TO_YOU);
            }
        }


        private void DoAskLoginAnonymous(bool obj)
        {
            OnPropertyChanged("AskLoginAnonymous");
        }

        private bool VerifyNCombi()
        {
            if (TicketHandler.TicketsInBasket[0].TipItems != null && TicketHandler.TicketsInBasket[0].TipItems.Count > 0)
            {
                int iMinCombi = 0;

                // loop through selected odds to find biggest required MinCombination for Tournament or for BetDomain
                // mincombi
                //foreach (var selectedOdd in TicketsInBasket[0].TipItems.ToList())
                // {
                //     TournamentSr tX = SportRadarMatchCollection.GetTournamentById(selectedOdd.Match.TournamentId, TranslationProvider.CurrentLanguage);
                //     int iTmp = Math.Max((int)tX.MinCombination.Value, selectedOdd.OddSr.BetDomain.MinCombination.Value);
                //     iMinCombi = Math.Max(iMinCombi, iTmp);
                // }

                int iMinCombiMatch = TicketHandler.TicketsInBasket.SelectMany(c => c.TipItems).Max(x => x.Match.MatchView.MinCombination);
                int iMinCombiTournament = TicketHandler.TicketsInBasket.SelectMany(c => c.TipItems).Max(x => x.Match.MatchView.MinTournamentCombination) ?? 0;
                iMinCombi = Math.Max(iMinCombiMatch, iMinCombiTournament);

                if (TicketHandler.TicketsInBasket[0].TipItems.Count < iMinCombi)
                {
                    ShowNotificationBar(MultistringTags.MINIMUM_COMBINATION_LESS_THAN_NEEDED, iMinCombi.ToString());
                    return false;
                }
            }
            return true;
        }

        [AsyncMethod]
        private void OnPlaceBet()
        {
            if (ChangeTracker.CurrentUser is AnonymousUser && ChangeTracker.CurrentUser.Cashpool == 0)
            {
                ShowNotificationBar(MultistringTags.PLEASE_LOGIN_OR_ADD_YOUR_STAKE);
                return;
            }

            if (!VerifyNCombi())
            {
                return;
            }
            IsEnabledPlaceBet = false;
            OnSaveTicket();
        }

        [WsldServiceSyncOnTicketAspect]
        private void OnSaveTicket()
        {
            decimal odd = TicketHandler.Stake;
            bool finished = PleaseWaitSaveTicket();
            if (finished)
            {
                ChangeTracker.BetSelected = false;
                Mediator.SendMessage(odd, MsgTag.OddPlaced);
                //OnClose();

                eServerSourceType? type = ChangeTracker.SourceType;
                if (type != null && (type == eServerSourceType.BtrVfl || type == eServerSourceType.BtrVhc))
                    MyRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
                else if (!_isprinting)
                {
                    //Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
                    
                    if (type != null && (type == eServerSourceType.BtrVfl || type == eServerSourceType.BtrVhc))
                        MyRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
                    else
                        Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
                }
            }
            if (!_isprinting)
            {
                PrinterHandler.InitPrinter(false);
                IsEnabledPlaceBet = true;
            }
        }

        SyncQueue<TicketToPrint> _savedTickets = new SyncQueue<TicketToPrint>();
        public bool PleaseWaitSaveTicket()
        {
            PrinterHandler.InitPrinter(true);
            if (StationRepository.PrinterStatus == 0)
            {
                //Mediator.SendMessage<double>(0, MsgTag.PrinterNotReady);
                ShowPrinterErrorMessage();
                IsEnabledPlaceBet = true;
                return false;
            }

            if (!ValidateBets(TicketHandler.TicketsInBasket))
            {
                IsEnabledPlaceBet = true;
                return false;
            }

            int errorcode = 0;


            _savedTickets.Clear();
            var tickets = new List<Ticket>();
            foreach (var ticket1 in TicketHandler.TicketsInBasket)
            {
                tickets.Add(ticket1);
            }
            Thread printingThread = null;
            for (int i = 0; i < tickets.Count; i++)
            {
                var ticket = tickets[i];
                if (ticket.TipItems.All(x => x.IsChecked == false))
                {
                    TicketHandler.TicketsInBasket.Remove(ticket);
                    tickets.RemoveAt(i);
                    i--;
                    continue;
                }
                errorcode = 0;
                TicketWS ticketWs = null;
                try
                {
                    WaitOverlayProvider.ShowWaitOverlay(true);
                    ChangeTracker.MouseClickLastTime = DateTime.Now;
                    ticketWs = TicketActions.SaveTicket(out errorcode, new TicketWS(), ticket, ChangeTracker.CurrentUser);
                }
                catch (FaultException<HubServiceException> exception)
                {
                    switch (exception.Detail.code)
                    {
                        case Ticket.TICKET_SAVE_REJECTED:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_FORM_ERROR_TICKET_WAS_REJECTED_BY_SERVER);
                            return false;
                        case Ticket.TICKET_ALREADY_SAVED:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_FORM_ERROR_TICKET_ALREADY_EXISTS);
                            return false;
                        case Ticket.TICKET_SAVE_SUCCESSFUL:
                            break;
                        case 226:
                        case 219:
                        case 181:
                        case 210:
                        case 225:
                        case 211:
                        case 224:
                        case 215:
                        case 228:
                        case 218:
                        case 223:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TECH, errorcode.ToString());
                            return false;
                        case 217:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_ODD_CHANGED);
                            return false;
                        case 227:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_MATCH_BEGUN);
                            return false;
                        case 229:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_MATCH_STOPPED);
                            return false;
                        case 214:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_ODD_DISABLED);
                            return false;
                        case 404:
                            if (ChangeTracker.CurrentUser is AnonymousUser)
                                break;
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_FORM_NO_CONNECTION_TO_SERVER);
                            return false;
                        case 216:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TIPP_UNAVAILABLE);
                            return false;
                        case 209:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_STAKE_EXCEEDS_MAXSTAKE);
                            return false;
                        case 234:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_MARKET_DISABLED);
                            return false;
                        case 235:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_SELECTION_DISABLED);
                            return false;
                        case 236:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_STAKE_BELOW_MIN_PER_ROW);
                            return false;
                        case 208:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TERMINAL_STAKE_LOWER_THAN_MIN);
                            return false;
                        default:
                            IsEnabledPlaceBet = true;
                            ShowNotificationBar(MultistringTags.TICKET_SAVE_FAILED);
                            return false;
                    }
                    IsEnabledPlaceBet = true;
                    Log.Error(exception.Detail.message, exception);
                    ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TECH, exception.Detail.message);
                    return false;
                }

                // 213 - maxwin to reach
                if (errorcode == Ticket.TICKET_SAVE_SUCCESSFUL)
                {
                    ChangeTracker.CurrentUser.Withdrawmoney(-ticket.Stake);
                    ticket.Stake = 0;
                    ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

                    _savedTickets.Enqueue(new TicketToPrint(ticketWs, ticket));
                    if (!_isprinting)
                    {
                        printingThread = new Thread(() => PrintTickets());
                        printingThread.Start();
                        _isprinting = true;
                    }
                }
            }

            LineSr.VerifySelectedOdds(new SortableObservableCollection<ITipItemVw>(TicketHandler.TicketsInBasket.ToSyncList().SelectMany(x => x.TipItems).ToList()));

            //RebindWheel(true);

            while (_isprinting)
            {
                Thread.Sleep(10);
            }

            eServerSourceType? type = ChangeTracker.SourceType;
            if (type != null && (type == eServerSourceType.BtrVfl || type == eServerSourceType.BtrVhc))
                MyRegionManager.NavigateUsingViewModel<EntertainmentViewModel>(RegionNames.ContentRegion);
            else
                Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);

            //Mediator.SendMessage(MsgTag.ShowFirstViewAndResetFilters, MsgTag.ShowFirstViewAndResetFilters);
            PrinterHandler.InitPrinter(false);
            IsEnabledPlaceBet = true;

            TicketWS = null;
            ChangeTracker.BasketWheelPosition = 0;

            RebindWheel(true);

            return true;
        }

        private void PrintTickets()
        {
            WaitOverlayProvider.ShowWaitOverlay(true);
            bool bPrinterError = false;
            while (_savedTickets.Count > 0)
            {
                if (_savedTickets.Count < 1)
                {
                    Thread.Sleep(10);
                    continue;
                }

                TicketToPrint ticket = _savedTickets.Dequeue();
                bool isPrinted = TicketActions.PrintTicket(ticket.TicketWS, false, ticket.Ticket.TipItems.Where(x => x.IsChecked).Any(x => x.IsLiveBet));
                if (!isPrinted)
                    bPrinterError = true;
                //ChangeTracker.MultipleSingles.Remove(dictSavedTicket.Key);
                if (bPrinterError)
                {
                    ShowPrinterErrorMessage();
                    //IsEnabledPlaceBet = true;
                    //return false;
                    //break;
                }

                while (ticket.Ticket.TipItems.Count > 0)
                {
                    ticket.Ticket.TipItems.RemoveAt(0);
                }
                if (TicketHandler.TicketsInBasket.Count > 0)
                    TicketHandler.TicketsInBasket.RemoveAt(0);
                TicketHandler.UpdateTicket();

            }
            _isprinting = false;
            if (bPrinterError)
            {
                ShowError(TranslationProvider.Translate(MultistringTags.PRINTER_ERROR_ASK_OPERATOR) as string);
            }
        }


        private bool ValidateBets(IList<Ticket> tickets)
        {
            if (!tickets.Any(x => x.TipItems.Any(c => c.IsChecked)))
            {
                ShowNotificationBar(MultistringTags.ADD_MORE_BETS);
                return false;
            }

            if (TicketHandler.Stake > ChangeTracker.CurrentUser.Cashpool)
            {
                ErrorWindowService.ShowError(TranslationProvider.Translate(MultistringTags.ADD_MONEY) as string, errorLevel: ErrorLevel.Critical);
                return false;
            }

            foreach (Ticket ticket1 in tickets)
            {
                if (ticket1.TipItems.All(x => x.IsChecked == false))
                    continue;
                if (ticket1.Stake <= 0)
                {
                    ShowNotificationBar(MultistringTags.TERMINAL_PLEASE_ADD_YOUR_STAKE_AMOUNT);
                    return false;
                }
                if (ticket1.Stake < ticket1.MinBet)
                {
                    ShowNotificationBar(MultistringTags.SHOP_FORM_BELOW_MINIMUM_STAKE);
                    return false;
                }

                if (ticket1.TicketState == TicketStates.Multy)
                {
                    if (ticket1.TipItems.Count(x => x.IsChecked) > StationRepository.GetMaxCombination(ticket1))
                    {
                        ShowNotificationBar(MultistringTags.SHOP_FORM_A_MULTIBET_MAY_HAVE_A_MAXIMUM_OF_GAMES, StationRepository.GetMaxCombination(ticket1).ToString());
                        return false;
                    }
                    if (!LimitHandling.BetMaxOddAllowed(tickets[0].TotalOdd, ticket1))
                    {
                        ShowNotificationBar(MultistringTags.TOTAL_ODDS_LIMIT_EXCEEDED, StationRepository.GetMaxOdd(ticket1).ToString(CultureInfo.InvariantCulture));
                        ticket1.CurrentTicketPossibleWin = 0;
                        return false;
                    }
                    if (ticket1.TipItems.Count(x => x.IsChecked) < StationRepository.GetMinCombination(ticket1))
                    {
                        ShowNotificationBar(MultistringTags.SHOP_FORM_COMBINATION_DOES_NOT_REACH_MINIMUM, StationRepository.GetMinCombination(ticket1).ToString());
                        return false;
                    }
                    if (ticket1.Stake > ticket1.MaxBet)
                    {
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_MAXIMUM_ALLOWED_WIN_EXCEEDED);
                        ticket1.Stake = ticket1.MaxBet;
                        ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                        return false;
                    }
                }
                if (ticket1.TicketState == TicketStates.System)
                {
                    if (ticket1.TipItems.Count(x => x.IsChecked) > StationRepository.GetMaxSystemBet(ticket1))
                    {
                        ShowNotificationBar(MultistringTags.SHOP_FORM_A_PERMBET_MAY_HAVE_A_MAXIMUM_OF_ITEMS, StationRepository.GetMaxSystemBet(ticket1).ToString());
                        return false;
                    }

                    if (ticket1.Stake > ticket1.MaxBet)
                    {
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_MAXIMUM_ALLOWED_WIN_EXCEEDED);
                        ticket1.Stake = ticket1.MaxBet;
                        ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                        return false;
                    }
                }
                if (ticket1.TicketState == TicketStates.Single)
                {
                    if (ticket1.Stake > ticket1.MaxBet || ticket1.CurrentTicketPossibleWin > ticket1.MaxWin)
                    {
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_MAXIMUM_ALLOWED_WIN_EXCEEDED);
                        ticket1.Stake = ticket1.MaxBet;
                        ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;
                        return false;
                    }
                }
                if (ticket1.MaxOddExceeded)
                {
                    ShowNotificationBar(MultistringTags.TOTAL_ODDS_LIMIT_EXCEEDED, StationRepository.GetMaxOdd(ticket1).ToString(CultureInfo.InvariantCulture));
                    return false;
                }
            }


            return true;
        }

        private void ShowPrinterErrorMessage()
        {
            int status = PrinterHandler.currentStatus;

            string errorMessage = ", ";

            switch (status)
            {
                case 0:
                    ShowNotificationBar(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER);
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

            ShowNotificationBar(MultistringTags.ERROR_CANNOT_PRINT_TICKET, errorMessage);
        }


        public ScrollViewer GetScrollviewer(string scrollViewerName)
        {
            ScrollViewer scrollViewerTmp = null;

            //var mainWindow = Application.Current.Windows.OfType<BasketView>().FirstOrDefault();
            Dispatcher.Invoke(() =>
                {
                    DependencyObject mainWindow = GetActiveWindow();
                    if (mainWindow != null)
                    {
                        scrollViewerTmp = AppVisualTree.FindChild<ScrollViewer>(mainWindow, scrollViewerName);
                    }
                });


            return scrollViewerTmp;
        }

        private DependencyObject GetActiveWindow()
        {
            if (ViewWindow != null)
                return ViewWindow;
            if (View != null)
                return Window.GetWindow(View);
            return null;
        }


        private void ShowStoreTicketPin()
        {
            EnterPinWindowService.AskPin(viewModel_OkClick);
        }

        [AsyncMethod]
        private void viewModel_OkClick(object sender, EventArgs<string> e)
        {
            StoreTicket(sender, e);
        }

        [WsdlServiceSyncAspectSilent]
        private void StoreTicket(object sender, EventArgs<string> e)
        {
            PleaseWaitStoreTicket(e);
        }

        [PleaseWaitAspect]
        private void PleaseWaitStoreTicket(EventArgs<string> e)
        {
            int errorcode = 0;
            try
            {
                TicketWS = TicketActions.StoreTicket(ChangeTracker.CurrentUser, e.Value, out errorcode, TicketWS, TicketHandler.TicketsInBasket[0]);

                bool isPrinted = TicketActions.PrintStoredTicket(TicketWS, e.Value, StationRepository.StationNumber, DateTime.Today.AddHours(StationRepository.StoreTicketExpirationHours), ChangeTracker.CurrentUser.AccountId, TicketHandler.TicketsInBasket[0].TipItems.Where(x => x.IsChecked).Any(x => x.IsLiveBet));
                if (!isPrinted)
                    ShowPrinterErrorMessage();
                //ShowNotificationBar(TranslationProvider.Translate(MultistringTags.TERMINAL_PRINTER_ERROR_HEADER).ToString(), true);


                TicketWS = null;
            }
            catch (FaultException<HubServiceException> exception)
            {
                switch (exception.Detail.code)
                {
                    case Ticket.TICKET_SAVE_REJECTED:
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_ERROR_TICKET_WAS_REJECTED_BY_SERVER);
                        return;
                    case Ticket.TICKET_ALREADY_SAVED:
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_ERROR_TICKET_ALREADY_EXISTS);
                        return;
                    case Ticket.TICKET_SAVE_SUCCESSFUL:
                        break;
                    case 226:
                    case 219:
                    case 181:
                    case 210:
                    case 225:
                    case 211:
                    case 224:
                    case 215:
                    case 228:
                    case 218:
                    case 223:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TECH, errorcode.ToString());
                        return;
                    case 217:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_ODD_CHANGED);
                        return;
                    case 227:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_MATCH_BEGUN);
                        return;
                    case 229:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_MATCH_STOPPED);
                        return;
                    case 214:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_ODD_DISABLED);
                        return;
                    case 404:
                        ShowNotificationBar(MultistringTags.TERMINAL_FORM_NO_CONNECTION_TO_SERVER);
                        return;
                    case 216:
                        ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TIPP_UNAVAILABLE);
                        return;
                    case 209:
                        ShowNotificationBar(MultistringTags.TERMINAL_STAKE_EXCEEDS_MAXSTAKE);
                        return;
                    case 208:
                        ShowNotificationBar(MultistringTags.TERMINAL_STAKE_LOWER_THAN_MIN);
                        return;
                    default:
                        ShowNotificationBar(MultistringTags.TICKET_SAVE_FAILED);
                        return;
                }
                Log.Error(exception.Detail.message, exception);

                ShowNotificationBar(MultistringTags.ERROR_TICKET_SAVE_TECH, exception.Detail.message);
            }
            OnDeleteAllBets();
            //Mediator.SendMessage("", MsgTag.ResetFilters);
            //if (!ChangeTracker.isLiveActive)
            //{
            //    SimpleWorkflow.ActivateOnlyFirstView();
            //}
            //else
            //{
            //    SimpleWorkflow.ActivateView("SportBetting.WPF.Prism.Modules.LiveModule.Views.LiveView", RegionNames.ContentRegion);
            //    Mediator.SendMessage<long>(0, MsgTag.SportChosen);
            //}
            //ChangeTracker.CurrentUser.Refresh();
        }

        private void PrintCreditNoteYes(object sender, EventArgs e)
        {
            PrintCreditNote();
        }

        [AsyncMethod]
        private void PrintCreditNote()
        {
            PrintCreditNotePleaseWait();
        }


        [WsdlServiceSyncAspect]
        private void PrintCreditNotePleaseWait()
        {
            foreach (Ticket ticket in TicketHandler.TicketsInBasket)
            {
                ticket.Stake = 0;
            }
            TicketHandler.UpdateTicket();
            ChangeTracker.CurrentUser.AvailableCash = ChangeTracker.CurrentUser.Cashpool - TicketHandler.Stake;

            if ((ChangeTracker.CurrentUser.Cashpool > 0) && ChangeTracker.CurrentUser is AnonymousUser)
            {
                SaveCreditNote(amount);
            }
        }

        private void LockStation(long obj)
        {
            if (obj == 2)
            {
                OnClose();
            }
        }

        private bool VerifyTournamentMatchLocks()
        {
            // AntonK: UnitTests workaround. Match.MatchView can't be null in normal conditions.
            if (TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).Where(c => c.IsChecked).Select(y => y.Match.MatchView).Any(x => x == null || x.LineObject == null))
                return true;

            if (bIsVerifyingLocks)
                return true;
            bIsVerifyingLocks = true;
            isLockingTournaments = false;
            if (TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).Count(y => y.IsChecked) == 1)
            {
                bIsVerifyingLocks = false;
                return true;
            }

            List<ITipItemVw> tipItemstoCheck = TicketHandler.TicketsInBasket.SelectMany(x => x.TipItems).Where(c => c.IsChecked).ToList();

            foreach (ITipItemVw tip1 in tipItemstoCheck)
            {
                string firstItemMatchId = tip1.Match.MatchId.ToString();
                string firstItemTournamentId = tip1.Match.MatchView.TournamentView.LineObject.SvrGroupId.ToString();

                foreach (ITipItemVw tip2 in tipItemstoCheck)
                {
                    if (tip1.Odd.OutcomeId == tip2.Odd.OutcomeId || tip1.Match.MatchId == tip2.Match.MatchId)
                        continue;

                    string secondItemMatchId = tip2.Match.MatchId.ToString();
                    string secondItemTournamentId = tip2.Match.MatchView.TournamentView.LineObject.SvrGroupId.ToString();


                    //with TOURNAMENT_MO option match AND outright cannot be combined on one ticket
                    if (secondItemTournamentId == firstItemTournamentId && tip1.Odd.BetDomain.Match.outright_type != tip2.Odd.BetDomain.Match.outright_type)
                    {
                        TournamentMatchLocksLn ttMOlock = LineSr.TournamentMatchLocks().SafelyGetValue(firstItemTournamentId + "|TOURNAMENT_MO|TOURNAMENT_MO");
                        if (ttMOlock != null)
                        {
                            List<string> lstX = ttMOlock.arrlocks.Value.Split('|').ToList();
                            if (lstX.Contains(secondItemTournamentId))
                            {
                                isLockingTournaments = true;
                                break;
                            }
                        }
                    }

                    //and now comes fun part. 4 requests, one per combination. Match-match, match-tournament, tournament-match, tournament-tournament
                    TournamentMatchLocksLn ttLockX = LineSr.TournamentMatchLocks().SafelyGetValue(firstItemMatchId + "|MATCH|MATCH");
                    if (ttLockX != null)
                    {
                        List<string> lstX = ttLockX.arrlocks.Value.Split('|').ToList();
                        if (lstX.Contains(secondItemMatchId))
                        {
                            isLockingTournaments = true;
                            break;
                        }
                    }

                    ttLockX = LineSr.TournamentMatchLocks().SafelyGetValue(firstItemMatchId + "|MATCH|TOURNAMENT");
                    if (ttLockX != null)
                    {
                        List<string> lstX = ttLockX.arrlocks.Value.Split('|').ToList();
                        if (lstX.Contains(secondItemTournamentId))
                        {
                            isLockingTournaments = true;
                            break;
                        }
                    }

                    ttLockX = LineSr.TournamentMatchLocks().SafelyGetValue(firstItemTournamentId + "|TOURNAMENT|MATCH");
                    if (ttLockX != null)
                    {
                        List<string> lstX = ttLockX.arrlocks.Value.Split('|').ToList();
                        if (lstX.Contains(secondItemMatchId))
                        {
                            isLockingTournaments = true;
                            break;
                        }
                    }

                    ttLockX = LineSr.TournamentMatchLocks().SafelyGetValue(firstItemTournamentId + "|TOURNAMENT|TOURNAMENT");
                    if (ttLockX != null)
                    {
                        List<string> lstX = ttLockX.arrlocks.Value.Split('|').ToList();
                        if (lstX.Contains(secondItemTournamentId))
                        {
                            isLockingTournaments = true;
                            break;
                        }
                    }
                }

                if (isLockingTournaments)
                {
                    if (WheelLine2.TicketState != TicketStates.Single)
                    {
                        ChangeTracker.BasketWheelPosition = 1;
                        OnSpinWheel("0");
                    }
                    ShowNotificationBar(MultistringTags.TERMINAL_MATCHES_CANT_BE_COMBINED);
                    bIsVerifyingLocks = false;
                    return false;
                }
            }

            bIsVerifyingLocks = false;
            return true;
        }

        #endregion
    }

    public class TicketToPrint
    {
        public Ticket Ticket;
        public TicketWS TicketWS;

        public TicketToPrint(TicketWS ticketWs, Ticket ticket)
        {
            TicketWS = ticketWs;
            Ticket = ticket;
        }
    }
}