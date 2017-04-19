using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using WsdlRepository.WsdlServiceReference;
using System.Globalization;
using Nbt.Station.Design;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// MainWindow view model.
    /// </summary>
    [ServiceAspect]
    public class UserTicketsViewModel : BaseViewModel
    {
        #region Variables

        public int Pagesize = 0;
        double GridHeight { get; set; }
        double RowHeight { get; set; }

        #endregion

        #region Constructor & destructor

        public UserTicketsViewModel()
        {
            WaitOverlayProvider.ShowWaitOverlay();
            HidePleaseWait = false;
            ItemCreated = new Command<UIElement>(OnRowItemCreated);
            GridCreated = new Command<UIElement>(OnGridCreated);
            ShowTicketCommand = new Command<TicketView>(OnShowTicket);
            TicketTypeCommand = new Command<string>(OnTicketTypeCommand);

            PreviousPage = new Command(OnPreviousPage);
            FirstPage = new Command(OnFirstPage);
            NextPage = new Command(OnNextPage);
            LastPage = new Command(OnLastPage);
            TicketsStartPage = ChangeTracker.TicketsStartPage;

            ShowTickets(new[] { new UserTicket() });

        }

        #endregion

        #region Properties

        public string Station
        {
            get { return _station; }
            set
            {
                _station = value;
                OnPropertyChanged("Station");
            }
        }

        public string TicketNumber
        {
            get { return _ticketNumber; }
            set
            {
                _ticketNumber = value;
                OnPropertyChanged("TicketNumber");
            }
        }

        public DateTime Date
        {
            get { return _date; }
            set
            {
                _date = value;
                OnPropertyChanged("Date");
            }
        }

        public string BetType
        {
            get { return _betType; }
            set
            {
                _betType = value;
                OnPropertyChanged("BetType");
            }
        }

        public string ShowStakePerRow
        {
            get { return _showStakePerRow; }
            set
            {
                _showStakePerRow = value;
                OnPropertyChanged("ShowStakePerRow");
            }
        }

        public decimal PossibleWinning
        {
            get { return _possibleWinning; }
            set
            {
                _possibleWinning = value;
                OnPropertyChanged("PossibleWinning");
            }
        }

        public string Paid
        {
            get { return _paid; }
            set
            {
                _paid = value;
                OnPropertyChanged("Paid");
            }
        }

        public string PaidAt
        {
            get { return _paidAt; }
            set
            {
                _paidAt = value;
                OnPropertyChanged("PaidAt");
            }
        }

        public decimal PaymentAmount
        {
            get { return _paymentAmount; }
            set
            {
                _paymentAmount = value;
                OnPropertyChanged("PaymentAmount");
            }
        }

        public string TabForecolor
        {
            get { return _tabForecolor; }
            set
            {
                _tabForecolor = value;
                OnPropertyChanged("TabForecolor");
            }
        }

        public string TabBgcolor
        {
            get { return _tabBgcolor; }
            set
            {
                _tabBgcolor = value;
                OnPropertyChanged("TabBgcolor");
            }
        }

        public decimal Stake
        {
            get { return _stake; }
            set
            {
                _stake = value;
                OnPropertyChanged();
            }
        }

        public decimal StakePerRow
        {
            get
            {
                return _stakePerRow; // bet.stake / bet.rows
            }
            set
            {
                _stakePerRow = value;
                OnPropertyChanged("StakePerRow");
            }
        }

        private string _station;
        private string _ticketNumber;
        private DateTime _date;
        private string _betType;
        private string _showStakePerRow;
        private decimal _possibleWinning;
        private string _paid;
        private string _paidAt;
        private decimal _paymentAmount;
        private string _tabForecolor;
        private string _tabBgcolor;
        private decimal _stakePerRow;
        private string _tabText1;

        public string TabText
        {
            get { return _tabText1; }
            set
            {
                if (value == _tabText1)
                    return;
                _tabText1 = value;
                OnPropertyChanged("TabText");
            }
        }

        private string _tabText2;

        public string TabTextApproval
        {
            get { return _tabText2; }
            set
            {
                if (value == _tabText2)
                    return;
                _tabText2 = value;
                OnPropertyChanged("TabTextApproval");
            }
        }

        private bool _showSuperBonus = false;
        public bool ShowSuperBonus
        {
            get
            {
                return _showSuperBonus;
            }
            set
            {
                if (_showSuperBonus == value)
                    return;

                _showSuperBonus = value;
                OnPropertyChanged("ShowSuperBonus");
                OnPropertyChanged("SuperBonusPercent");
            }
        }

        private bool _showManipulationFee = false;
        private decimal _superBonusPercent;
        private decimal _stake;
        private decimal _manipulationFee;
        private decimal _manipulationFeeValue;
        private decimal _superBonusValue;

        public bool ShowManipulationFee
        {
            get
            {
                return _showManipulationFee;
            }
            set
            {
                _showManipulationFee = value;
                OnPropertyChanged("ShowManipulationFee");
            }
        }

        public decimal SuperBonusPercent
        {
            get { return _superBonusPercent; }
            set
            {
                _superBonusPercent = value;
                OnPropertyChanged();
            }
        }

        public decimal SuperBonusValue
        {
            get { return _superBonusValue; }
            set
            {
                _superBonusValue = value;
                OnPropertyChanged();
            }
        }


        public int AllPages
        {
            get { return _allPages; }
            set
            {
                _allPages = value;
                OnPropertyChanged("AllPages");
            }
        }

        public int SelectedType
        {
            get { return ChangeTracker.SelectedTycketType; }
            set 
            {
                ChangeTracker.SelectedTycketType = value;
                OnPropertyChanged("SelectedType");
                TicketsStartPage = 1;
                ChangeTracker.TicketsStartPage = TicketsStartPage;
                UpdateTickets();
            }
        }

        private ObservableCollection<Tip> bets;
      
        /// <summary>
        /// Gets or sets the Operations.
        /// </summary>
        public ObservableCollection<Tip> BalanceOperations
        {
            get { return bets; }
            set
            {
                bets = value;
                OnPropertyChanged();
            }
        }

        //public ComboBoxItem SelectedType
        //{
        //    get { return ChangeTracker.SelectedType; }
        //    set
        //    {
        //        ChangeTracker.SelectedType = value;
        //        OnPropertyChanged("SelectedType");
        //        TicketsStartPage = 1;
        //        ChangeTracker.TicketsStartPage = TicketsStartPage;
        //        UpdateTickets();
        //    }
        //}

        public int TicketsStartPage
        {
            get { return _ticketsStartPage; }
            set
            {
                _ticketsStartPage = value;
                OnPropertyChanged();
            }
        }

        protected TicketWS CurrentTicket
        {
            get { return ChangeTracker.CurrentTicket; }
            set { ChangeTracker.CurrentTicket = value; }
        }

        //protected ObservableCollection<TicketDetailsWS> CurrentTicketDetails
        //{
        //    get { return ChangeTracker.CurrentTicketDetails; }
        //    set { ChangeTracker.CurrentTicketDetails = value; }
        //}


        public ObservableCollection<TicketView> Tickets
        {
            get { return ChangeTracker.Tickets; }
            set
            {
                ChangeTracker.Tickets = value;
                OnPropertyChanged();
            }
        }

        private string _errorLabel;
        private int _allPages;
        private int _ticketsStartPage;

        public string ErrorLabel
        {
            get { return _errorLabel; }
            set
            {
                _errorLabel = value;
                OnPropertyChanged("ErrorLabel");
                OnPropertyChanged("ErrorVisible");
            }
        }

        public bool ErrorVisible
        {
            get
            {
                return string.IsNullOrEmpty(ErrorLabel);
            }
        }

        public decimal ManipulationFee
        {
            get { return _manipulationFee; }
            set
            {
                _manipulationFee = value;
                OnPropertyChanged();
            }
        }

        public decimal ManipulationFeeValue
        {
            get { return _manipulationFeeValue; }
            set
            {
                _manipulationFeeValue = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands
        public Command PreviousPage { get; set; }
        public Command FirstPage { get; set; }
        public Command NextPage { get; set; }
        public Command LastPage { get; set; }
        public Command<UIElement> ItemCreated { get; set; }
        public Command<UIElement> GridCreated { get; set; }
        public Command<TicketView> ShowTicketCommand { get; set; }
        public Command<string> TicketTypeCommand { get; private set; }

        #endregion


        #region Methods

        public override void OnNavigationCompleted()
        {
            base.OnNavigationCompleted();
        }

        [WsdlServiceSyncAspect]
        private void OnShowTicket(TicketView ticketView)
        {
          
            try
            {
                CurrentTicket = WsdlRepository.LoadTicket(ticketView.Number, ticketView.CheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);
            var a = new TicketDetailsViewModel();
            a.OnNavigationCompleted();
                BalanceOperations = a.BalanceOperations;

                if (CurrentTicket != null)
                {
                    OnPropertyChanged("PendingApprovalVisibility");
                    ShowSuperBonus = CurrentTicket.superBonus > 1;
                    SuperBonusValue = CurrentTicket.superBonusValue;
                    ShowManipulationFee = CurrentTicket.manipulationFee > 0;
                    ManipulationFee = CurrentTicket.manipulationFee;
                    ManipulationFeeValue = CurrentTicket.manipulationFeeValue;
                    Stake = CurrentTicket != null ? CurrentTicket.stake : 0;

                    Station = CurrentTicket.ticketNbr.Substring(0, 4);
                    TicketNumber = CurrentTicket.ticketNbr;
                    Date = CurrentTicket.acceptedTime;
                    ShowStakePerRow = "";
                    SuperBonusPercent = (CurrentTicket.superBonus - 1) * 100;
                    if (CurrentTicket.bets != null)
                    {
                        long prevBankGroupID = -1;
                        int bankCount = 0;
                        int multiWayCount = 0;
                        foreach (TipWS tip in CurrentTicket.bets[0].tips2BetMulti)
                        {
                            if (tip.bankGroupID != prevBankGroupID && !tip.bank)
                            {
                                multiWayCount++;
                                prevBankGroupID = tip.bankGroupID;
                            }
                            if (tip.bank)
                            {
                                bankCount++;
                            }
                        }
                        StakePerRow = CurrentTicket.stake / CurrentTicket.bets[0].rows;
                        switch (CurrentTicket.bets[0].betType)
                        {
                            case Bet.BET_TYPE_COMBI:
                                BetType = TranslationProvider.Translate(MultistringTags.Multiple);
                                break;
                            case Bet.BET_TYPE_COMBIPATH:
                                BetType = TranslationProvider.Translate(MultistringTags.Multiple_and_Ways);
                                ShowStakePerRow = "1";
                                break;
                            case Bet.BET_TYPE_SINGLE:
                                BetType = TranslationProvider.Translate(MultistringTags.SINGLES);
                                break;
                            case Bet.BET_TYPE_SYSTEM:
                                BetType = TranslationProvider.Translate(MultistringTags.Full_Cover) + " " + CurrentTicket.bets[0].systemX + "/" + CurrentTicket.bets[0].systemY;
                                if (bankCount > 0)
                                {
                                    BetType += "+" + bankCount + "B";
                                }
                                ShowStakePerRow = "1";
                                break;
                            case Bet.BET_TYPE_SYSTEMPATH:
                                string sTmp =
                                    TranslationProvider.Translate(MultistringTags.TERMINAL_PRINT_SYSTEM) + " " +
                                    CurrentTicket.bets[0].systemX + "/" + CurrentTicket.bets[0].systemY;
                                if (bankCount > 0)
                                {
                                    sTmp += "+" + bankCount + "B";
                                }
                                if (multiWayCount > 0)
                                {
                                    sTmp += "+" + multiWayCount + "W";
                                }
                                BetType = sTmp;
                                ShowStakePerRow = "1";
                                break;
                        }
                        PossibleWinning = CurrentTicket.bets[0].maxWin;
                    }
                    Paid = CurrentTicket.paid
                               ? TranslationProvider.Translate(MultistringTags.terminal_yes)
                               : TranslationProvider.Translate(MultistringTags.terminal_no);
                    //PaidAt = CurrentTicket.Paid ? CurrentTicket.PaidTime.ToShortDateString() : "-";
                    PaidAt = CurrentTicket.paid ? String.Format("{0:dd.MM.yyyy HH:mm}", CurrentTicket.paidTime) : "-";
                    PaymentAmount = CurrentTicket.wonAmount;

                    //if (CurrentTicket.bets != null)
                    //    BalanceOperations = LoadOdds(new ObservableCollection<BetWS>(CurrentTicket.bets));
                    //string[] tabColor = GetTabColor();
                    //TabForecolor = tabColor[0];
                    //TabBgcolor = tabColor[1];
                    OnPropertyChanged("ShowTranferToAccountButton");
                    OnPropertyChanged("ShowCreditNoteButton");
                    OnPropertyChanged("TransferMoneyString");

                }
            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {
                ShowError(exception.Detail.message);
            }
            //CurrentTicket = WsdlRepository.LoadTicket(ticketView.Number, ticketView.CheckSum, StationRepository.StationNumber, SelectedLanguage, SelectedLanguage, true);

            //    MyRegionManager.NavigateUsingViewModel<TicketDetailsViewModel>(RegionNames.UserProfileContentRegion);
           
        }



        private void OnGridCreated(UIElement obj)
        {
            if (GridHeight > 0)
                return;

            GridHeight = obj.RenderSize.Height;

            if (GridHeight > 0 && RowHeight > 0 && Tickets[0].Hidden)
            {
                UpdateTickets();
            }
        }

        private void OnRowItemCreated(UIElement obj)
        {
            if (RowHeight > 0)
                return;
            RowHeight = obj.RenderSize.Height;

            if (GridHeight > 0 && RowHeight > 0 && Tickets[0].Hidden)
            {
                UpdateTickets();
            }
        }


        private void OnPreviousPage()
        {
            if (TicketsStartPage < 2)
                return;

            TicketsStartPage--;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void OnFirstPage()
        {
            if (TicketsStartPage < 2)
                return;

            TicketsStartPage = 1;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void OnNextPage()
        {
            if (TicketsStartPage < AllPages)
                TicketsStartPage++;
            ChangeTracker.TicketsStartPage = TicketsStartPage;

            UpdateTickets();
        }

        private void OnLastPage()
        {
            if (TicketsStartPage < AllPages)
                TicketsStartPage = AllPages;
            ChangeTracker.TicketsStartPage = TicketsStartPage;
            UpdateTickets();
        }

        private void ShowTickets(UserTicket[] obj)
        {
            TicketView._instances.Clear();
            Tickets.Clear();
            foreach (var ticket in obj)
            {
                string name = "";
                int id = (int)ticket.ticketCategory;
                switch (id)
                {
                    case 0: 
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_ALL).ToString();
                        break;
                    case 1:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETWON).ToString();
                        break;
                    case 2:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETLOST).ToString();
                        break;
                    case 3:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_CANCELLED).ToString();
                        break;
                    case 4:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_TICKETOPEN).ToString();
                        break;
                    case 5:
                        name = TranslationProvider.Translate(MultistringTags.TERMINAL_PENDING_APPROVAL).ToString();
                        break;
                }
                    var ticketView = new TicketView(ticket.ticketNumber, ticket.checkSum, name, id, ticket.createdAt, Currency);
                    Tickets.Add(ticketView);
            }
            if (RowHeight == 0 && Tickets.Count > 0)
            {
                if (Tickets[0].CreatedAt == DateTime.MinValue)
                    Tickets[0].Hidden = true;
            }
        }


        private void OnTicketTypeCommand(string day)
        {
            SelectedType = Int16.Parse(day, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture);
        }


        private void UpdateTickets()
        {
            PleaseWaitOnUpdateTickets();
        }

        [WsdlServiceSyncAspect]
        private void PleaseWaitOnUpdateTickets()
        {

            ErrorLabel = string.Empty;
            if (GridHeight > 0 && RowHeight > 0)
                Pagesize = (int)(GridHeight / RowHeight);
            else
            {
                Pagesize = 100;
            }


            UserTicket[] tickets = new UserTicket[0];
            long total = 1;
            string tempTotal = "1";
            try
            {
                tickets = WsdlRepository.GetUserTickets(ChangeTracker.CurrentUser.AccountId.ToString(), (ticketCategory)SelectedType, new AccountTicketSorting() { field = AccountTicketSortingFields.DateCreated, value = AccountTicketSortingValues.Desc }, (int)((TicketsStartPage - 1) * Pagesize), (int)Pagesize, out tempTotal);


            }
            catch (System.ServiceModel.FaultException<HubServiceException> exception)
            {

                if (exception.Detail.code == 131)
                {
                    tickets = new UserTicket[0];
                    ErrorLabel = TranslationProvider.Translate(MultistringTags.TERMINAL_NO_TICKET_FOUND).ToString();
                }
                else
                {
                    ShowError(exception.Detail.message);
                }
            }
            total = Convert.ToInt64(tempTotal);
            if (tickets != null)
            {
                ShowTickets(tickets);
            }

            double pagesCount = total / (double)Pagesize;
            if (pagesCount < 1)
            {
                pagesCount = 1;
            }
            if (pagesCount % 1 > 0)
            {
                pagesCount++;
            }
            AllPages = (int)pagesCount;


        }

        #endregion
    }
}