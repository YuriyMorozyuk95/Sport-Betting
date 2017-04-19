using System;
using System.Collections.Generic;
using System.Data;
using BaseObjects;
using SportRadar.Common;
using SportRadar.DAL.CommonObjects;
using TranslationByMarkupExtension;
using StationCashSr = SportRadar.DAL.CommonObjects.StationCashSr;

namespace SportBetting.WPF.Prism.Modules.Accounting.ViewModels
{
    using System.Collections.ObjectModel;
    using SportBetting.WPF.Prism.Models;

    /// <summary>
    /// Authorization Login view model.
    /// </summary>
    public class FilterViewModel : AccountingBaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FilterViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>
        public FilterViewModel()
        {
            _startdate = ChangeTracker.StartDateAccounting;
            _enddate = ChangeTracker.EndDateAccounting;
            CalStartDate = ChangeTracker.CalendarStartDateAccounting;
            CalEndDate = ChangeTracker.CalendarEndDateAccounting;
            if (CalStartDate == CalEndDate)
            {
                _startdate = StartDate == DateTime.MinValue ? DateTime.Today.AddDays(-7) : StartDate;
                _enddate = EndDate == DateTime.MinValue ? DateTime.Today.AddDays(1).AddSeconds(-1) : EndDate;
                CalStartDate = StartDate;
                CalEndDate = EndDate;
            }
            ShowFromDateWindowCommand = new Command(OnShowFromDateWindowExecute);
            ShowToDateWindowCommand = new Command(OnShowToDateWindowCommandExecute);
            Close1Command = new Command(CloseBalance);
            IsEnabledPrintInfo = false;
            bool cashin = ChangeTracker.CashInAccounting;
            bool cashout = ChangeTracker.CashOutAccounting;
            if (cashin && !cashout)
            {
                cashOutChecked = false;
                allChecked = false;
                CashInChecked = true;
            }
            else if (!cashin && cashout)
            {
                cashInChecked = false;
                allChecked = false;
                CashOutChecked = true;
            }
            else
            {
                cashInChecked = false;
                cashOutChecked = false;
                AllChecked = true;
            }

            CheckDates();
            ChooseFromCheckpoints = ChangeTracker.FromCheckPointsAccounting;
            if (ChooseFromCheckpoints)
            {
                long startidx = 0;
                long endidx = 0;
                foreach (DateCollection startDateFilter in StartDateFilters)
                {
                    if (startDateFilter.Date == StartDate) startidx = startDateFilter.Index;
                }
                foreach (DateCollection dateCollection in EndDateFilters)
                {
                    if (dateCollection.Date == EndDate) endidx = dateCollection.Index;
                }
                SelectedStartDateFilterIndex = (int)startidx;
                SelectedEndDateFilterIndex = (int)endidx;
                if (startidx == 0) this.OnPropertyChanged("CalStartDate");
                if (endidx == 0) OnPropertyChanged("CalEndDate");
            }
            else
            {
                CalStartDate = StartDate;
                CalEndDate = EndDate;
            }

            Show_ShowButton = true;
        }

        #endregion


        #region Properties

        private bool cashInChecked;
        private bool cashOutChecked;
        private bool allChecked;
        private bool _isEnabledPrintInfo;
        private DateTime curStartdate;
        private decimal cashinCurrentAmount;
        private decimal cashoutCurrentAmount;
        private DateTime _startdate = DateTime.MinValue;
        private DateTime _enddate = DateTime.MinValue;
        private DateTime oldStartdate = DateTime.MinValue;
        private DateTime oldEnddate = DateTime.MinValue;
        private DateTime oldCalStartdate = DateTime.MinValue;
        private DateTime oldCalEnddate = DateTime.MinValue;

        public bool ShowResults { get; set; }


        public ObservableCollection<DateCollection> EndDateFilters { get; set; }
        private int _selectedEndDateFilterIndex;
        public int SelectedEndDateFilterIndex
        {
            get { return _selectedEndDateFilterIndex; }
            set
            {
                int oldval = _selectedEndDateFilterIndex;
                _selectedEndDateFilterIndex = value;
                if (oldval != value && value >= 0)
                    SelectedEndDate_SelectionChanged(value);

                OnPropertyChanged("SelectedEndDateFilterIndex");
            }
        }
        public ObservableCollection<DateCollection> StartDateFilters { get; set; }

        private int _selectedStartDateFilterIndex;
        public int SelectedStartDateFilterIndex
        {
            get { return _selectedStartDateFilterIndex; }
            set
            {
                int oldval = _selectedStartDateFilterIndex;
                _selectedStartDateFilterIndex = value;
                if (oldval != value && value >= 0)
                {
                    SelectedStartDate_SelectionChanged(StartDateFilters[value].Date, value);
                    CheckDates();
                }
                OnPropertyChanged("SelectedStartDateFilterIndex");
            }
        }

        bool _chooseFromCheckpoints;
        public bool ChooseFromCheckpoints
        {
            get
            {
                return _chooseFromCheckpoints;
            }
            set
            {
                _chooseFromCheckpoints = value;
                ChangeTracker.FromCheckPointsAccounting = value;
                if (value)
                {
                    CheckDates();
                }
                else
                {
                    StartDate = CalStartDate;
                    EndDate = CalEndDate;
                }
                OnPropertyChanged("ChooseFromCheckpoints");
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        public DateTime StartDate
        {
            get { return _startdate; }
            set
            {
                _startdate = value;
                ChangeTracker.StartDateAccounting = value;
                CheckDates();
            }
        }


        public DateTime EndDate
        {
            get { return _enddate; }
            set
            {
                _enddate = value;
                ChangeTracker.EndDateAccounting = value;
                CheckDates();
            }
        }


        public bool CashInChecked
        {
            get { return cashInChecked; }
            set
            {
                cashInChecked = value;
                Pay();
            }
        }
        public bool CashOutChecked
        {
            get { return cashOutChecked; }
            set
            {
                cashOutChecked = value;
                Pay();
            }
        }
        public bool AllChecked
        {
            get { return allChecked; }
            set
            {
                allChecked = value;
                Pay();
            }
        }

        public void Pay()
        {
            ChangeTracker.CashInAccounting = cashInChecked || allChecked;
            ChangeTracker.CashOutAccounting = cashOutChecked || allChecked;
        }

        public bool IsEnabledPrintInfo
        {
            get { return _isEnabledPrintInfo; }
            set
            {
                _isEnabledPrintInfo = value;
                OnPropertyChanged("IsEnabledPrintInfo");
            }
        }


        private DateTime _calStartDate;
        public DateTime CalStartDate
        {
            get { return _calStartDate; }
            set
            {
                _calStartDate = value;
                ChangeTracker.CalendarStartDateAccounting = value;
                OnPropertyChanged("CalStartDate");

            }
        }

        private DateTime _calEndDate;
        public DateTime CalEndDate
        {
            get { return _calEndDate; }
            set
            {
                _calEndDate = value;
                ChangeTracker.CalendarEndDateAccounting = value;
                OnPropertyChanged("CalEndDate");
            }
        }

        #endregion

        #region Commands
        public Command ShowFromDateWindowCommand { get; private set; }
        public Command ShowToDateWindowCommand { get; private set; }

        #endregion

        #region Methods
        /// <summary>
        /// TODO get values (start & end date) from FromToDateWindow
        /// </summary>
        private void OnShowFromDateWindowExecute()
        {
            CalStartDate = DateHelper.SelectDate(this.CalStartDate) ?? DateTime.Today;
            if ((!ChooseFromCheckpoints || SelectedStartDateFilterIndex == 0) || StartDate < CalStartDate)
            {
                StartDate = CalStartDate;
                SelectedStartDate_SelectionChanged(StartDate, SelectedStartDateFilterIndex);
                CheckDates();
            }
            if (CalStartDate > CalEndDate)
                CalEndDate = CalStartDate.Add(new TimeSpan(23, 59, 59));
            if (EndDate < CalEndDate)
            {
                EndDate = CalEndDate;
            }
            this.OnPropertyChanged("CalStartDate");
        }

        public override void OnNavigationCompleted()
        {
            ChangeTracker.CashOperationsChecked = true;
            ChangeTracker.AdminTitle1 = MultistringTags.TERMINAL_ADMIN_MENU_ACCOUNTING;
            ChangeTracker.AdminTitle2 = MultistringTags.CASH_OPERATIONS;
            base.OnNavigationCompleted();
        }

        private void CheckDates()
        {
            if (oldStartdate != StartDate || oldEnddate != EndDate || oldCalStartdate != CalStartDate || oldCalEnddate != CalEndDate)
            {
                oldStartdate = StartDate;
                oldEnddate = EndDate;
                oldCalStartdate = CalStartDate;
                oldCalEnddate = CalEndDate;
                if (StartDateFilters == null) StartDateFilters = new ObservableCollection<DateCollection>();
                if (EndDateFilters == null) EndDateFilters = new ObservableCollection<DateCollection>();
                SetDates(GetStartDates());
            }
        }


        private void OnShowToDateWindowCommandExecute()
        {
            DateTime end = DateHelper.SelectDate(this.CalEndDate, this.StartDate) ?? DateTime.Today;
            end = end.Date;
            end = end.AddHours(23);
            end = end.AddMinutes(59);
            end = end.AddSeconds(59);
            CalEndDate = end;
            if ((!ChooseFromCheckpoints || SelectedEndDateFilterIndex == 0) || this.EndDate < CalEndDate)
            {
                this.EndDate = CalEndDate;
                CheckDates();
            }
            this.OnPropertyChanged("CalEndDate");
        }


     


        public static StationCashSr[] GetStartDates()
        {
            try
            {
                List<StationCashSr> lStationCash = StationCashSr.GetStationCashListByQuery("SELECT * FROM StationCash where StationCash.CashCheckPoint = 1 order by DateModified desc", new List<IDbDataParameter>());

                List<StationCashSr> cashin = StationCashSr.GetStationCashListByQuery("SELECT * FROM StationCash where StationCash.MoneyIn = 1 order by DateModified asc", new List<IDbDataParameter>());

                if (cashin.Count > 0)
                {
                    cashin[0].DateModified = cashin[0].DateModified.AddSeconds(-1);
                    lStationCash.Add(cashin[0]);
                }
                return lStationCash.ToArray();

            }
            catch (Exception)
            {

            }
            return null;
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


        public void SetDates(StationCashSr[] start)
        {
            if (ChangeTracker.CurrentUser.GetType() == typeof(OperatorUser))
            {
                int startIdx = SelectedStartDateFilterIndex;
                int endIdx = SelectedEndDateFilterIndex;

                StartDateFilters.Clear();
                EndDateFilters.Clear();
                long idx = 1;
                DateCollection noDate = new DateCollection()
                                            {
                                                Date = DateTimeUtils.DATETIME1700,
                                                Index = 0,
                                                Value =
                                                    TranslationProvider.Translate(
                                                        MultistringTags.TERMINAL_FROM_CALENDER).ToString()
                                            };
                StartDateFilters.Add(noDate);
                EndDateFilters.Add(noDate);

                if (start != null && start.Length > 0)
                {
                    foreach (StationCashSr cash in start)
                    {
                        DateCollection date = new DateCollection()
                                                  {
                                                      Date = cash.DateModified,
                                                      Index = idx,
                                                      Value = cash.DateModified.ToString("dd.MM.yyyy HH:mm") + " (" + (
                                                      ((OperatorUser)ChangeTracker.CurrentUser).AccountId.ToString() == cash.OperatorID ? ((OperatorUser)ChangeTracker.CurrentUser).Username : StationRepository.StationTyp) + ")"
                                                  };
                        if (CalStartDate <= cash.DateModified) StartDateFilters.Add(date);
                        if (cash.DateModified > StartDate && cash.DateModified <= CalEndDate) EndDateFilters.Add(date);

                        idx++;
                    }
                }
                if (idx <= 1)
                {
                    DateTime creationDate = DateTimeUtils.DATETIMENULL;
                    if (StationRepository.Created_At != null)
                        creationDate = StationRepository.Created_At.Value;
                    DateCollection date = new DateCollection()
                                              {
                                                  Date = creationDate,
                                                  Index = idx,
                                                  Value = creationDate.ToString("dd.MM.yyyy HH:mm")
                                              };
                    StartDateFilters.Add(date);
                    EndDateFilters.Add(date);
                }
                if (StartDate > CalEndDate)
                {
                    CalEndDate = StartDate.Date.Add(new TimeSpan(23, 59, 59));
                }
                if (StartDateFilters.Count - 1 >= startIdx)
                    SelectedStartDateFilterIndex = startIdx;
                else SelectedStartDateFilterIndex = StartDateFilters.Count - 1;
                if (EndDateFilters.Count - 1 >= endIdx)
                    SelectedEndDateFilterIndex = endIdx;
                else SelectedEndDateFilterIndex = EndDateFilters.Count - 1;
            }
        }

        private void SelectedStartDate_SelectionChanged(DateTime startDate, int value)
        {
            //int idx = SelectedEndDateFilterIndex;
            //EndDateFilters.Clear();
            //foreach (DateCollection startDateFilter in StartDateFilters)
            //{
            //    if ((startDateFilter.Date > startDate && EndDate <= startDateFilter.Date) || startDateFilter.Date == DateTimeUtils.DATETIME1700)
            //        EndDateFilters.Add(startDateFilter);
            //}
            //if (EndDateFilters.Count - 1 >= idx)
            //    SelectedEndDateFilterIndex = idx;
            //else SelectedEndDateFilterIndex = EndDateFilters.Count - 1;
            //CheckDates();
            StartDate = value > 0 ? startDate : CalStartDate;
            if (SelectedEndDateFilterIndex == 0)
            {
                if (EndDate < StartDate) EndDate = DateTime.Today.AddDays(1).AddMinutes(-1);
                OnPropertyChanged("CalEndDate");
            }
        }

        private void SelectedEndDate_SelectionChanged(int value)
        {
            EndDate = value > 0 ? EndDateFilters[value].Date : CalEndDate;
        }

        #endregion


    }

    public class DateCollection
    {
        public long Index { get; set; }
        public string Value { get; set; }
        public DateTime Date { get; set; }
    }
}