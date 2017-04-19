using System;
using System.Linq;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using System.Collections.Generic;
using SportRadar.DAL.OldLineObjects;
using System.Threading;
using System.Windows;
using SportBetting.WPF.Prism.WpfHelper;

namespace ViewModels.ViewModels
{

    /// <summary>
    /// Categories view model.
    /// </summary>
    [ServiceAspect]
    public class CategoriesViewModel : BaseViewModel
    {

        #region Constructors
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private static object _itemsLock = new object();

        public CategoriesViewModel()
        {
            Category._instances.Clear();
            BindingOperations.EnableCollectionSynchronization(_categories, _itemsLock);

            TournamentChoice = new Command<TournamentVw>(OnTournamentChoiceExecute);
            CountryChoice = new Command<object>(OnCountryChoiceExecute);
            Choice = new Command<long>(OnChoiceExecute);
            PlaceBet = new Command<IOddVw>(OnBet);
            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            ScrollChangedCommand = new Command<double>(ScrollChanged);
            LayoutUpdatedCommand = new Command<double>(LayoutUpdated);           
            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
        }

        void LineSr_DataSqlUpdateSucceeded(SportRadar.DAL.CommonObjects.eUpdateType eut, string sProviderDescription)
        {
            if (eut == eUpdateType.PreMatches)
            {
                FillCategories();
            }
        }

        public void LayoutUpdated(double width)
        {
            ColumnsAmount = (Int32)(width / 259);
        }

        public override void OnNavigationCompleted()
        {
            Mediator.SendMessage("", MsgTag.ResetFilters);
            FillCategories();
            //Mediator.SendMessage<bool>(false, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            ChangeTracker.SelectedSports = true;
            base.OnNavigationCompleted();
            Mediator.SendMessage<bool>(true, MsgTag.UpdateLiveMonitorTemplates);
        }




        #endregion

        #region Properties
        private SortableObservableCollection<IMatchVw> _tournamentMatches;
        public SortableObservableCollection<IMatchVw> TournamentMatches
        {
            get
            {               
                return _tournamentMatches;                
            }
            set
            {
                _tournamentMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<TournamentVw> _selectedTournaments;
        public SortableObservableCollection<TournamentVw> SelectedTournaments
        {
            get { return _selectedTournaments; }
            set
            {
                _selectedTournaments = value;
                OnPropertyChanged();
            }
        }


        private SortableObservableCollection<TournamentVw> _countries;
        public SortableObservableCollection<TournamentVw> ShowCountries
        {
            get { return _countries; }
            set
            {
                _countries = value;
                OnPropertyChanged();
            }
        }

        protected SortableObservableCollection<TournamentVw> _tournaments;

        public SortableObservableCollection<TournamentVw> Tournaments
        {
            get { return _tournaments; }
            set
            {
                _tournaments = value;
                OnPropertyChanged();
            }
        }

        protected SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }


        public SortableObservableCollection<Category> Categories
        {
            get { return _categories; }
            set
            {
                _categories = value;
                OnPropertyChanged();
            }
        }

        protected string SelectedSportDescriptor { get; set; }

        private IMatchVw SelectedMatch
        {
            set { ChangeTracker.CurrentMatch = value; }
        }

        private object _buttonBackground;
        public object ButtonBackground { get; set; }
        //{
        //    get { return _buttonBackground; }
        //    set
        //    {
        //        _buttonBackground = value;
        //        OnPropertyChanged();
        //    }
        //}

        private int _columnsAmount = 1;
        public int ColumnsAmount
        {
            get
            {
                return _columnsAmount;
            }
            set
            {
                if (_columnsAmount != value)
                {
                    _columnsAmount = value;
                    OnPropertyChanged("ColumnsAmount");
                }
            }
        }

        #endregion

        #region Commands
        /// <summary>
        /// Gets the Choice command.
        /// </summary>
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<long> Choice { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<object> CountryChoice { get; private set; }
        public Command<TournamentVw> TournamentChoice { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command<double> LayoutUpdatedCommand { get; set; }

        #endregion

        #region Methods

        public override void Close()
        {
            LineSr.UnsubscribeFromEnent(LineSr_DataSqlUpdateSucceeded);
            base.Close();
        }
        object _lockerTimer = new object();
        private SortableObservableCollection<Category> _categories = new SortableObservableCollection<Category>();

        private void FillCategories()
        {           
            lock (_lockerTimer)
            {

                Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, delegate(IMatchVw m1, IMatchVw m2) { return 0; });

                var groups = Matches.Where(x => x.CategoryView != null).Select(x => x.CategoryView).Distinct().ToList();

                foreach (var group in groups)
                {
                    //string descriptor = Matches.Where(x => x.CategoryView != null && x.CategoryView.LineObject.GroupId == group.LineObject.GroupId).FirstOrDefault().SportDescriptor;

                    //set icon for mixed sports
                    List<IMatchVw> CategorieMatches = Matches.Where(x => x.CategoryView != null && x.CategoryView.LineObject.GroupId == group.LineObject.GroupId).ToList();
                    string descriptor = CategorieMatches[0].SportDescriptor;
                    foreach (IMatchVw match in CategorieMatches)
                    {
                        if (match.SportDescriptor != descriptor)
                        {
                            descriptor = SportSr.SPORT_DESCRIPTOR_MIXED;
                            break;
                        }
                    }

                    if (Categories.Count(x => x.Id == group.LineObject.GroupId) == 0)
                    {  
                        Categories.Add(new Category(group.DisplayName, group.LineObject.GroupId, group.LineObject.Sort.Value, descriptor, CategorieMatches.Count));                        
                    }
                }
                Categories.Sort(delegate(Category m1, Category m2) { return m1.Sort.CompareTo(m2.Sort); });
                for (int i = 0; i < Categories.Count; )
                {
                    var comboBoxItem = Categories[i];

                    if (groups.Count(x => x.LineObject.GroupId == comboBoxItem.Id) == 0)
                    {
                        Categories.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

            }       
        }

        private bool MatchFilter(IMatchLn matchLn)
        {

            if (!matchLn.Active.Value)
                return false;

            if (matchLn.IsLiveBet.Value)
                return false;

            if (matchLn.MatchView.CategoryView == null)
                return false;

            if (matchLn.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (matchLn.MatchView.AllVisibleOddCount == 0)
                return false;

            if (matchLn.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;

            if(matchLn.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(matchLn.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;
        }

        private long SelectedCategoryId;

        private void OnChoiceExecute(IMatchVw chosenEntity)
        {
            SelectedMatch = chosenEntity;

            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);

            VisualEffectsHelper.Singleton.LiveSportMatchDetailsIsOpened = true;
        }

        private void OnChoiceExecute(long id)
        {
            TournamentVw.CheckFlag = true;
            TournamentVw._instances.Clear();
            if (TournamentMatches != null && TournamentMatches.Count > 0)
                TournamentMatches = null;
            if (SelectedTournaments != null && SelectedTournaments.Count > 0)
                SelectedTournaments.Clear();
            var category = Categories.Where(c => c.Id == id).FirstOrDefault();
            SelectedSportDescriptor = category.SportDescriptor;
            ButtonBackground = category.BackgroundBySport;
            var tournamentsData = new TournamentsViewModel(id);
            tournamentsData.OnNavigationCompleted();
            Tournaments = new SortableObservableCollection<TournamentVw>(tournamentsData.Tournaments);
            ShowCountries = new SortableObservableCollection<TournamentVw>(Tournaments.GroupBy(p => p.Country).Select(g => g.First()).ToList());
            ShowCountries[0].Country = "Outrights";
            SelectedCategoryId = id;
            TournamentVw.CheckFlag = false;
        }

        private void OnCountryChoiceExecute(object parameters)
        {
                  
            var arr = (object[])parameters;
            var country = (string)arr[0];
            var id = (long)arr[1];
            if(country != "Outrights" && country != "")
            SelectedTournaments = new SortableObservableCollection<TournamentVw>(Tournaments.Where(a => a.Country  == country));
            else
            SelectedTournaments = new SortableObservableCollection<TournamentVw>(Tournaments.Where(a => a.Id == id));

        }
        
        private void OnTournamentChoiceExecute (TournamentVw tournament)
        {
            if (tournament.IsOutrightGroup)
            {              
                var tournamentsData = new TournamentsViewModel(SelectedCategoryId, tournament);
                tournamentsData.OnNavigationCompleted();

                var outrightsMatches = new SortableObservableCollection<IMatchVw>();
                var tournaments = new SortableObservableCollection<TournamentVw>(tournamentsData.Tournaments);
                foreach (var tornmt in tournaments)
                {
                    var matchData = new MatchesViewModel(new HashSet<string>() { tornmt.ContainsOutrights ? tornmt.Id.ToString() + "*1" : tornmt.Id.ToString() + "*0" });
                    matchData.SelectedDescriptors.Add(SelectedSportDescriptor);
                    matchData.OnNavigationCompleted();
                    outrightsMatches.AddRange(matchData.Matches);
                }

                TournamentMatches = outrightsMatches;               
            }
            else
            {
                var matchesData = new MatchesViewModel(new HashSet<string>() { tournament.ContainsOutrights ? tournament.Id.ToString() + "*1" : tournament.Id.ToString() + "*0" });
                matchesData.SelectedDescriptors.Add(SelectedSportDescriptor);
                matchesData.OnNavigationCompleted();
                TournamentMatches = matchesData.Matches;
            }     
        }

        private void OnLanguageChosenExecute(string lang)
        {
            FillCategories();
        }

        private void Refresh(bool state)
        {
            Category._instances.Clear();
            Categories.Clear();
            FillCategories();
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsPrematchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}