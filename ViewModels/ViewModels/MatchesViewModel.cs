using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.Common.Collections;
using SportRadar.DAL.CommonObjects;
using SportRadar.DAL.NewLineObjects;
using SportRadar.DAL.ViewObjects;
using SportBetting.WPF.Prism.Shared.WpfHelper;
using SportBetting.WPF.Prism.Shared.Models;
using SportRadar.DAL.OldLineObjects;
using TranslationByMarkupExtension;
using System.Windows;
using System.Windows.Media;
using SportBetting.WPF.Prism.Shared.Converters;
using System.Windows.Media.Imaging;

namespace ViewModels.ViewModels
{
    /// <summary>
    /// Matches view model.
    /// </summary>
    [ServiceAspect]
    public class MatchesViewModel : BaseViewModel
    {
        //private read-only static Lazy<MatchesViewModel> _Instance = new Lazy<MatchesViewModel>(() => new MatchesViewModel(), true);
        //public static MatchesViewModel Instance { get { return _Instance.Value; } }

        #region Constructors
        private static object _itemsLock = new object();
        private static object _itemsLock2 = new object();
        private SortableObservableCollection<IMatchVw> _matches = new SortableObservableCollection<IMatchVw>();
        private System.Windows.Controls.ScrollViewer scrollViewerPreMatch = null;

        private readonly ScrollViewerModule _ScrollViewerModule;
        public List<string> SelectedDescriptors
        {
            get { return ChangeTracker.SelectedDescriptorsPreMatch; }
        }

        private SortableObservableCollection<SportBarItem> _sportsBarItemsPreMatch = new SortableObservableCollection<SportBarItem>();
        public SortableObservableCollection<SportBarItem> SportsBarItemsPreMatch
        {
            get
            {
                return _sportsBarItemsPreMatch;
            }
            set
            {
                _sportsBarItemsPreMatch = value;
                OnPropertyChanged("SportsBarItemsPreMatch");
            }
        }

        public MatchesViewModel(params object[] args)
        {
            _ScrollViewerModule = new ScrollViewerModule(Dispatcher);

            BindingOperations.EnableCollectionSynchronization(_matches, _itemsLock);
            BindingOperations.EnableCollectionSynchronization(_sportsBarItemsPreMatch, _itemsLock2);
            OpenMatch = new Command<IMatchVw>(OnChoiceExecute);
            OpenOutrightMatch = new Command<IMatchVw>(OnOutrightChoiceExecute);

            Mediator.Register<string>(this, OnLanguageChosenExecute, MsgTag.LanguageChosen);
            Mediator.Register<bool>(this, Refresh, MsgTag.Refresh);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);
            Mediator.Register<bool>(this, ClearSelectedSports, MsgTag.ClearSelectedSports);
            //Mediator.Register<bool>(this, ShowSelectedTournaments, MsgTag.ShowSelectedTournaments);
            PreMatchScrollChangedCommand = new Command(PreMatchScrollChanged);
            PreMatchScrollLoadedCommand = new Command<System.Windows.Controls.ScrollViewer>(PreMatchScrollLoaded);

            PlaceBet = new Command<IOddVw>(OnBet);
            ScrollChangedCommand = new Command<double>(ScrollChanged);

            ScrollLeftStart = new Command(OnScrollLeftStart);
            ScrollRightStart = new Command(OnScrollRightStart);
            CheckedBox = new Command<SportBarItem>(OnCheckedExecute);

            ElementsVisibility = Visibility.Collapsed;
            MainElementsVisibility = Visibility.Visible;

            if (args.Length > 0 && args[0] is HashSet<string>)
            {
                SelectedTournaments = args[0] as HashSet<string>;
            }

            if (args.Length > 0 && args[0] is int)
            {
                if ((int)args[0] == 1)
                {
                    MainElementsVisibility = Visibility.Collapsed;
                    ElementsVisibility = Visibility.Visible;
                    }
            }
            


            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }
            //selected tournaments handling

            LineSr.SubsribeToEvent(LineSr_DataSqlUpdateSucceeded);
        }


        #endregion

        #region Properties

        private SortableObservableCollection<IMatchVw> _soccerMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> SoccerMatches
        {
            get { return _soccerMatches; }
            set
            {
                _soccerMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<IMatchVw> _tennisMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> TennisMatches
        {
            get { return _tennisMatches; }
            set
            {
                _tennisMatches = value;
                OnPropertyChanged();
            }
        }
        private SortableObservableCollection<IMatchVw> _basketballMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> BasketballMatches
        {
            get { return _basketballMatches; }
            set
            {
                _basketballMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<IMatchVw> _hockeyMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> HockeyMatches
        {
            get { return _hockeyMatches; }
            set
            {
                _hockeyMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<IMatchVw> _rugbyMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> RugbyMatches
        {
            get { return _rugbyMatches; }
            set
            {
                _rugbyMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<IMatchVw> _handballMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> HandballMatches
        {
            get { return _handballMatches; }
            set
            {
                _handballMatches = value;
                OnPropertyChanged();
            }
        }

        private SortableObservableCollection<IMatchVw> _volleyballMatches = new SortableObservableCollection<IMatchVw>();
        public SortableObservableCollection<IMatchVw> VolleyballMatches
        {
            get { return _volleyballMatches; }
            set
            {
                _volleyballMatches = value;
                OnPropertyChanged();
            }
        }

        private Visibility _soccerVisibility = Visibility.Visible;
        public Visibility SoccerVisibility
        {
            get { return _soccerVisibility; }
            set
            {
                _soccerVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _tennisVisibility = Visibility.Visible;
        public Visibility TennisVisibility
        {
            get { return _tennisVisibility; }
            set
            {
                _tennisVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _basketballVisibility = Visibility.Visible;
        public Visibility BasketballVisibility
        {
            get { return _basketballVisibility; }
            set
            {
                _basketballVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _hockeyVisibility = Visibility.Visible;
        public Visibility HockeyVisibility
        {
            get { return _hockeyVisibility; }
            set
            {
                _hockeyVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _rugbyVisibility = Visibility.Visible;
        public Visibility RugbyVisibility
        {
            get { return _rugbyVisibility; }
            set
            {
                _rugbyVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _handballVisibility = Visibility.Visible;
        public Visibility HandballVisibility
        {
            get { return _handballVisibility; }
            set
            {
                _handballVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _volleyballVisibility = Visibility.Visible;
        public Visibility VolleyballVisibility
        {
            get { return _volleyballVisibility; }
            set
            {
                _volleyballVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _mainElementsVisibility = Visibility.Visible;
        public Visibility MainElementsVisibility
        {
            get { return _mainElementsVisibility; }
            set
            {
                _mainElementsVisibility = value;
                OnPropertyChanged();
            }
        }

        private Visibility _elementsVisibility = Visibility.Collapsed;
        public Visibility ElementsVisibility
        {
            get { return _elementsVisibility; }
            set
            {
                _elementsVisibility = value;
                OnPropertyChanged();
            }
        }

        #region SoccerExpanderIsExpanded
        private bool _soccerExpanderIsExpanded = false;
        public bool SoccerExpanderIsExpanded
        {
            get { return _soccerExpanderIsExpanded; }
            set
            {
                _soccerExpanderIsExpanded = value;
                OnPropertyChanged(nameof(SoccerExpanderIsExpanded));
            }
        }
        #endregion

        #region TennisExpanderIsExpanded
        private bool _tennisExpanderIsExpanded = false;
        public bool TennisExpanderIsExpanded
        {
            get { return _tennisExpanderIsExpanded; }
            set
            {
                _tennisExpanderIsExpanded = value;
                OnPropertyChanged(nameof(TennisExpanderIsExpanded));
            }
        }
        #endregion

        #region BasketballExpanderIsExpanded
        private bool _basketballExpanderIsExpanded = false;
        public bool BasketballExpanderIsExpanded
        {
            get { return _basketballExpanderIsExpanded; }
            set
            {
                _basketballExpanderIsExpanded = value;
                OnPropertyChanged(nameof(BasketballExpanderIsExpanded));
            }
        }
        #endregion

        #region HockeyExpanderIsExpanded
        private bool _hockeyExpanderIsExpanded = false;
        public bool HockeyExpanderIsExpanded
        {
            get { return _hockeyExpanderIsExpanded; }
            set
            {
                _hockeyExpanderIsExpanded = value;
                OnPropertyChanged(nameof(HockeyExpanderIsExpanded));
            }
        }
        #endregion

        #region VolleyballExpanderIsExpanded
        private bool _volleyballExpanderIsExpanded = false;
        public bool VolleyballExpanderIsExpanded
        {
            get { return _volleyballExpanderIsExpanded; }
            set
            {
                _volleyballExpanderIsExpanded = value;
                OnPropertyChanged(nameof(VolleyballExpanderIsExpanded));
            }
        }
        #endregion

        #region HandballExpanderIsCollapsed
        private bool _handballExpanderIsCollapsed = false;
        public bool HandballExpanderIsExpanded
        {
            get { return _handballExpanderIsCollapsed; }
            set
            {
                _handballExpanderIsCollapsed = value;
                OnPropertyChanged(nameof(HandballExpanderIsExpanded));
            }
        }
        #endregion

        #region RugbuExpanderIsExpanded
        private bool _rugbyExpanderIsExpanded = false;
        public bool RugbyExpanderIsExpanded
        {
            get { return _rugbyExpanderIsExpanded; }
            set
            {
                _rugbyExpanderIsExpanded = value;
                OnPropertyChanged(nameof(RugbyExpanderIsExpanded));
            }
        }
        #endregion

        private LinearGradientBrush _sportColor;
        public LinearGradientBrush SportColor
        {
            get
            {
                return _sportColor;
            }
            set
            {
                _sportColor = value;
                OnPropertyChanged();
            }
        }

        private string _sportName;
        public string SportName
        {
            get
            {
                return _sportName;
            }
            set
            {
                _sportName = value;
                OnPropertyChanged();
            }
        }

        private object _sportIcon;
        public object SportIcon
        {
            get
            {
                return _sportIcon;
            }
            set
            {
                _sportIcon = value;
                OnPropertyChanged();
            }
        }

        private object _backgroundImage;
        public object BackgroundImage
        {
            get
            {
                return _backgroundImage;
            }
            set
            {
                _backgroundImage = value;
                OnPropertyChanged();
            }
        }

        public bool CanScrollLeft
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentHorizontalOffset == 0)
                    res = false;
                else if (scrollViewerPreMatch == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollRight
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentHorizontalOffset + scrollViewerPreMatch.ViewportWidth >= scrollViewerPreMatch.ExtentWidth)
                    res = false;
                else if (scrollViewerPreMatch == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollUp
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentVerticalOffset == 0)
                    res = false;
                else if (scrollViewerPreMatch == null)
                    res = false;

                return res;
            }
        }

        public bool CanScrollDown
        {
            get
            {
                bool res = true;

                GetSportsBarScrollviewer();

                if (scrollViewerPreMatch != null && scrollViewerPreMatch.ContentVerticalOffset + scrollViewerPreMatch.ViewportHeight >= scrollViewerPreMatch.ExtentHeight)
                    res = false;
                else if (scrollViewerPreMatch == null)
                    res = false;

                return res;
            }
        }

        //protected new static readonly log4net.ILog Log = LogManager.GetLogger(typeof(MatchesViewModel));

        private IMatchVw SelectedMatch
        {
            set { ChangeTracker.CurrentMatch = value; }
        }

        public SortableObservableCollection<IMatchVw> Matches
        {
            get { return _matches; }
            set { _matches = value; }
        }

        public Visibility SportsBarVisibility { get { return SportsBarItemsPreMatch.Count > 2 ? Visibility.Visible : Visibility.Collapsed; } }

        #endregion

        #region Commands

        public Command ScrollLeftStart { get; private set; }
        public Command ScrollRightStart { get; private set; }
        public Command<SportBarItem> CheckedBox { get; private set; }
        public Command<IMatchVw> OpenMatch { get; private set; }
        public Command<IMatchVw> OpenOutrightMatch { get; private set; }
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }
        public Command PreMatchScrollChangedCommand { get; private set; }
        public Command<System.Windows.Controls.ScrollViewer> PreMatchScrollLoadedCommand { get; private set; }

        #endregion

        #region Methods

        private void PreMatchScrollLoaded(System.Windows.Controls.ScrollViewer scroller)
        {
            scrollViewerPreMatch = scroller;
            CheckSportBarButtons();
        }

        private void ClearSelectedSports(bool res)
        {
            SportsBarItemsPreMatch.Clear();
            SelectedDescriptors.Clear();
            SportsBarItemsPreMatch.Add(new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));
            SportsBarItemsPreMatch.ElementAt(0).IsChecked = true;
            SelectedDescriptors.Add(SportsBarItemsPreMatch.ElementAt(0).SportDescriptor);

            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                scrollViewerPreMatch.ScrollToVerticalOffset(0);
            }
            else
                scrollViewerPreMatch.ScrollToHorizontalOffset(0);
        }

        private void CheckSportBarButtons()
        {
            OnPropertyChanged("CanScrollLeft");
            OnPropertyChanged("CanScrollRight");
            OnPropertyChanged("CanScrollUp");
            OnPropertyChanged("CanScrollDown");
        }

        private void PreMatchScrollChanged()
        {
            CheckSportBarButtons();
        }

        private System.Windows.Controls.ScrollViewer GetSportsBarScrollviewer()
        {
            if (scrollViewerPreMatch == null)
                scrollViewerPreMatch = this.GetScrollviewerForActiveWindowByName("SportsBarScrollPreMatch");

            return scrollViewerPreMatch;
        }

        public void OnCheckedExecute(SportBarItem barItem)
        {
            if (barItem == null)
                return;

            CheckedExecute(barItem);
        }

        private void CheckedExecute(SportBarItem barItem)
        {
            if (barItem.SportDescriptor == SportSr.ALL_SPORTS && SelectedDescriptors.Count == 1 && SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
            {
                SportBarItem allsports = SportsBarItemsPreMatch.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                }

                return;
            }
            else if (SelectedDescriptors.Contains(barItem.SportDescriptor))
                SelectedDescriptors.Remove(barItem.SportDescriptor);
            else
            {
                if (barItem.SportDescriptor == SportSr.ALL_SPORTS)
                {
                    for (int i = 1; i < SportsBarItemsPreMatch.Count; i++)
                        SportsBarItemsPreMatch[i].IsChecked = false;

                    SelectedDescriptors.Clear();
                }
                else //all sports should be unchecked automatically
                {
                    if (SelectedDescriptors.Contains(SportSr.ALL_SPORTS))
                    {
                        SportsBarItemsPreMatch[0].IsChecked = false;
                        SelectedDescriptors.Remove(SportSr.ALL_SPORTS);
                    }
                }

                SelectedDescriptors.Add(barItem.SportDescriptor);
            }

            if (SelectedDescriptors.Count == 0)
            {
                SportBarItem allsports = SportsBarItemsPreMatch.Where(x => x.SportDescriptor == SportSr.ALL_SPORTS).First();
                if (allsports != null)
                {
                    allsports.IsChecked = true;
                    SelectedDescriptors.Add(allsports.SportDescriptor);
                }
            }


            SoccerVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (SoccerVisibility == Visibility.Visible) SoccerExpanderIsExpanded = true;
            else SoccerExpanderIsExpanded = false;

            TennisVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (TennisVisibility == Visibility.Visible) TennisExpanderIsExpanded = true;
            else SoccerExpanderIsExpanded = false;

            BasketballVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (BasketballVisibility == Visibility.Visible) BasketballExpanderIsExpanded = true;
            else BasketballExpanderIsExpanded = false;

            HockeyVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (HockeyVisibility == Visibility.Visible) HockeyExpanderIsExpanded = true;
            else HockeyExpanderIsExpanded = false;

            RugbyVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (RugbyVisibility == Visibility.Visible) RugbyExpanderIsExpanded = true;
            else RugbyExpanderIsExpanded = false;

            HandballVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_HANDBALL && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (HandballVisibility == Visibility.Visible) HandballExpanderIsExpanded = true;
            else HandballExpanderIsExpanded = false;

            VolleyballVisibility = SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL && x.IsChecked) ? Visibility.Visible : Visibility.Collapsed;
            if (VolleyballVisibility == Visibility.Visible) VolleyballExpanderIsExpanded = true;
            else VolleyballExpanderIsExpanded = false;

            if (SportsBarItemsPreMatch.Any(x => x.SportDescriptor == SportSr.ALL_SPORTS && x.IsChecked))
            {
                SoccerVisibility = Visibility.Visible;
                TennisVisibility = Visibility.Visible;
                BasketballVisibility = Visibility.Visible;
                HockeyVisibility = Visibility.Visible;
                RugbyVisibility = Visibility.Visible;
                HandballVisibility = Visibility.Visible;
                VolleyballVisibility = Visibility.Visible;

                SoccerExpanderIsExpanded = false;
                TennisExpanderIsExpanded = false;
                BasketballExpanderIsExpanded = false;
                HockeyExpanderIsExpanded = false;
                RugbyExpanderIsExpanded = false;
                HandballExpanderIsExpanded = false;
                VolleyballExpanderIsExpanded = false;
            }


            Refresh(true);

            ScrollToVertivalOffset(0);
        }

        public void FillSportsBar()
        {
            SortableObservableCollection<IMatchVw> PreMatches = new SortableObservableCollection<IMatchVw>();
            Repository.FindMatches(PreMatches, "", SelectedLanguage, MatchFilterSportBar, delegate (IMatchVw m1, IMatchVw m2) { return 0; });

            try
            {
                var sports = PreMatches.Where(x => x.SportView != null).Select(x => x.SportView).Distinct().ToList();

                SportBarItem allsports = SportsBarItemsPreMatch.FirstOrDefault(x => x.SportDescriptor == SportSr.ALL_SPORTS);
                if (allsports != null)
                    allsports.SportName = TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string;
                else
                    SportsBarItemsPreMatch.Insert(0, new SportBarItem(TranslationProvider.Translate(MultistringTags.ALL_SPORTS) as string, SportSr.ALL_SPORTS));

                foreach (var group in sports)
                {
                    {
                        if (SportsBarItemsPreMatch.Count(x => x.SportDescriptor == group.LineObject.GroupSport.SportDescriptor) == 0)
                        {
                            SportsBarItemsPreMatch.Add(new SportBarItem(group.DisplayName, group.LineObject.GroupSport.SportDescriptor));
                        }
                        else
                        {
                            SportsBarItemsPreMatch.First(x => x.SportDescriptor == @group.LineObject.GroupSport.SportDescriptor).SportName = @group.DisplayName;
                        }
                    }
                }

                for (int i = 1; i < SportsBarItemsPreMatch.Count;)
                {
                    var item = SportsBarItemsPreMatch[i];

                    if (sports.Count(x => x.LineObject.GroupSport.SportDescriptor == item.SportDescriptor) == 0)
                    {
                        SportsBarItemsPreMatch.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                foreach (SportBarItem item in SportsBarItemsPreMatch)
                {
                    if (SelectedDescriptors.Contains(item.SportDescriptor))
                        item.IsChecked = true;
                    else
                        item.IsChecked = false;
                }

                SportsBarItemsPreMatch.Sort(ComparisonSportsBar);
                
                OnPropertyChanged("SportsBarVisibility");
            }
            catch (Exception ex)
            {
            }
        }

        public int ComparisonSportsBar(SportBarItem m1, SportBarItem m2)
        {
            return m1.SortingOrder.CompareTo(m2.SortingOrder);
        }

        private void OnScrollLeftStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollUpStartExecute(scrollViewerPreMatch, true);
            }
            else
                this._ScrollViewerModule.OnScrollLeftStartExecute(scrollViewerPreMatch, true);
        }

        private void OnScrollRightStart()
        {
            GetSportsBarScrollviewer();

            if (scrollViewerPreMatch == null)
                return;

            if (ChangeTracker.IsLandscapeMode)
            {
                this._ScrollViewerModule.OnScrollDownStartExecute(scrollViewerPreMatch, true);
            }
            else
                this._ScrollViewerModule.OnScrollRightStartExecute(scrollViewerPreMatch, true);
        }

        private void LineSr_DataSqlUpdateSucceeded(eUpdateType eut, string sproviderdescription)
        {
            if (eut == eUpdateType.PreMatches)
            {
                FillMatches();
            }
        }

        public override void OnNavigationCompleted()
        {
            FillMatches();
            if (Matches.Count == 0)
            {
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
            }
            //Mediator.SendMessage<bool>(false, MsgTag.BlockSportFilter);
            Mediator.SendMessage(true, MsgTag.ActivateForwardSelected);
            //Mediator.SendMessage<bool>(false, MsgTag.BlockTimeFilter);
            ChangeTracker.SelectedSports = true;

            base.OnNavigationCompleted();
        }


        private void OnLanguageChosenExecute(string lang)
        {
            FillMatches();
        }
        object _locker = new object();
        private HashSet<string> _selectedTournaments = new HashSet<string>();

        private void FillMatches()
        {
            lock (_locker)
            {
                FillSportsBar();
                Matches = Repository.FindMatches(Matches, "", SelectedLanguage, MatchFilter, Comparison);

                long oldSportId = 0;
                for (int i = 0; i < Matches.Count; i++)
                {
                    long currentSportId = Matches.ElementAt(i).TournamentView.LineObject.GroupId;


                    if (currentSportId != oldSportId)
                    {
                        Matches.ElementAt(i).IsHeaderForPreMatch = true;
                        oldSportId = currentSportId;
                    }
                    else
                        Matches.ElementAt(i).IsHeaderForPreMatch = false;
                }
            }
            if (Matches.Count > 0 && MainElementsVisibility == Visibility.Visible)
                switch (Matches.First().SportDescriptor)
                {
                    case SportSr.SPORT_DESCRIPTOR_SOCCER:
                        SportIcon = new ResolveImagePath("LiveView/socker-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/socker-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF3F8145"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF90C696"), 0));                     
                        SportColor.Freeze();                     
                        SportName = string.Format("{0} / {1}", TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_SOCCER) as string, Matches.First().TournamentNameToShow);
                        break;

                    case SportSr.SPORT_DESCRIPTOR_BASKETBALL:
                        SportIcon = new ResolveImagePath("LiveView/Basket-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/Basketball-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#AF6828"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D8A362"), 0));
                        SportColor.Freeze();
                        SportName = string.Format("{0} / {1}", TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_BASKETBALL) as string, Matches.First().TournamentNameToShow);
                        break;

                    case SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY:
                        SportIcon = new ResolveImagePath("LiveView/hockey-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/Hokkey-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A5181"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#73B0D8"), 0));
                        SportColor.Freeze();
                        SportName = string.Format("{0} / {1}", TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_ICEHOCKEY) as string, Matches.First().TournamentNameToShow);
                        break;

                    case SportSr.SPORT_DESCRIPTOR_TENNIS:
                        SportIcon = new ResolveImagePath("LiveView/tennis-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/tennis-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#979F0D"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DDE04A"), 0));
                        SportColor.Freeze();
                        SportName = string.Format("{0} / {1}", TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_TENNIS) as string, Matches.First().TournamentNameToShow);
                        break;

                    case SportSr.SPORT_DESCRIPTOR_HANDBALL:
                        SportIcon = new ResolveImagePath("LiveView/hand-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/handball-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C8C8CA"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F4F4F4"), 0));
                        SportColor.Freeze();
                        SportName = string.Format("{0} / {1}", "Handball", Matches.First().TournamentNameToShow);
                        //SportName = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_HANDBALL) as string;
                        break;

                    case SportSr.SPORT_DESCRIPTOR_RUGBY:
                        SportIcon = new ResolveImagePath("LiveView/rugby-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/rugby-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#963D2D"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#989E98"), 0));
                        SportColor.Freeze();
                        //SportName = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_RUGBY) as string;                       
                        SportName = string.Format("{0} / {1}", "Rugby", Matches.First().TournamentNameToShow);
                        break;

                    case SportSr.SPORT_DESCRIPTOR_VOLLEYBALL:
                        SportIcon = new ResolveImagePath("LiveView/volley-ball.png").ProvideValue(null);
                        BackgroundImage = new ResolveImagePath("LiveView/volleyball-fon.png").ProvideValue(null);
                        SportColor = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3685D3"), 1));
                        SportColor.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FDC903"), 0));
                        SportColor.Freeze();
                        //SportName = TranslationProvider.Translate(MultistringTags.TERMINAL_FORM_VOLLEYBALL) as string;                      
                        SportName = string.Format("{0} / {1}", "Volleyball", Matches.First().TournamentNameToShow);
                        break;
                }

            SoccerMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_SOCCER));
            TennisMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_TENNIS));
            BasketballMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_BASKETBALL));
            HockeyMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY));
            RugbyMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_RUGBY));
            VolleyballMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL));
            HandballMatches = new SortableObservableCollection<IMatchVw>(Matches.Where(a => a.SportDescriptor == SportSr.SPORT_DESCRIPTOR_HANDBALL));

            SoccerVisibility = SoccerMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            TennisVisibility = TennisMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            BasketballVisibility = BasketballMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            HockeyVisibility = HockeyMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            RugbyVisibility = RugbyMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            VolleyballVisibility = VolleyballMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            HandballVisibility = HandballMatches.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private static int Comparison(IMatchVw m1, IMatchVw m2)
        {
            if (m1.TournamentView.LineObject.GroupId == m2.TournamentView.LineObject.GroupId)
            {
                if (m1.StartDate == m2.StartDate)
                {
                    if (m2.TournamentView.LineObject.GroupId == m1.TournamentView.LineObject.GroupId)
                        return m2.Name.CompareTo(m1.Name);
                    return m2.TournamentView.LineObject.GroupId.CompareTo(m1.TournamentView.LineObject.GroupId);
                }

                return m1.StartDate.CompareTo(m2.StartDate);
            }
            return m2.TournamentView.LineObject.GroupId.CompareTo(m1.TournamentView.LineObject.GroupId);
        }

        public bool MatchFilter(IMatchLn match)
        {
            if (!match.Active.Value)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            if (match.IsLiveBet.Value)
                return false;

            if (SportsBarItemsPreMatch.Count > 1 && !SportsBarItemsPreMatch.ElementAt(0).IsChecked)
            {
                if (!SelectedDescriptors.Contains(match.MatchView.SportView.LineObject.GroupSport.SportDescriptor))
                    return false;
            }

            if (match.MatchView.CategoryView == null)
                return false;

            string id = (match.MatchView.TournamentView.LineObject.GroupId.ToString());
            string tourId = match.outright_type == eOutrightType.Outright ? id + "*1" : id + "*0";

            if (SelectedTournaments.Count > 0 && !SelectedTournaments.Contains(tourId))
                return false;


            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;


            if (ChangeTracker.PreMatchSelectedMode == 1)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate >= DateTime.Now.AddDays(1).Date)
                    return false;
            }
            if (ChangeTracker.PreMatchSelectedMode == 2)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate > DateTime.Now.AddMinutes(90))
                    return false;
            }

            if (match.outright_type == eOutrightType.Outright && SelectedTournaments.Contains(match.MatchView.TournamentView.LineObject.GroupId.ToString() + "*1"))
                return true;
            else if (match.outright_type == eOutrightType.None && SelectedTournaments.Contains(match.MatchView.TournamentView.LineObject.GroupId.ToString() + "*0"))
                return true;

            if (match.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(match.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;

        }

        public bool MatchFilterSportBar(IMatchLn match)
        {
            if (!match.Active.Value)
                return false;

            if (match.MatchView.VisibleBetDomainCount == 0)
                return false;

            if (match.MatchView.AllVisibleOddCount == 0)
                return false;

            if (match.IsLiveBet.Value)
                return false;

            if (match.MatchView.CategoryView == null)
                return false;

            string id = (match.MatchView.TournamentView.LineObject.GroupId.ToString());
            string tourId = match.outright_type == eOutrightType.Outright ? id + "*1" : id + "*0";

            if (SelectedTournaments.Count > 0 && !SelectedTournaments.Contains(tourId))
                return false;


            if (match.ExpiryDate.Value.LocalDateTime < DateTime.Now)
                return false;


            if (ChangeTracker.PreMatchSelectedMode == 1)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate >= DateTime.Now.AddDays(1).Date)
                    return false;
            }
            if (ChangeTracker.PreMatchSelectedMode == 2)
            {
                if (match.MatchView.StartDate < DateTime.Now)
                    return false;
                if (match.MatchView.StartDate > DateTime.Now.AddMinutes(180))
                    return false;
            }

            if (match.MatchView.TournamentView != null)
                if (!LineSr.IsTournamentVisible(match.MatchView.TournamentView.LineObject.SvrGroupId.ToString()))
                    return false;

            return true;

        }

        public HashSet<string> SelectedTournaments
        {
            get { return _selectedTournaments; }
            set { _selectedTournaments = value; }
        }

        private void OnChoiceExecute(IMatchVw chosenMatch)
        {
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<BetDomainsViewModel>(RegionNames.ContentRegion);
        }

        private void OnOutrightChoiceExecute(IMatchVw chosenMatch)
        {
            SelectedMatch = chosenMatch;
            MyRegionManager.NavigateUsingViewModel<OutrightViewModel>(RegionNames.ContentRegion);
        }

        public void Refresh(bool state)
        {
            Matches.Clear();
            FillMatches();
        }



        public override void Close()
        {
            LineSr.UnsubscribeFromEnent(LineSr_DataSqlUpdateSucceeded);
            base.Close();
        }

        private void HeaderShowFirstView(string obj)
        {
            if (!StationRepository.IsPrematchEnabled)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion
    }
}