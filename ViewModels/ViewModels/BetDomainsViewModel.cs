using System;
using System.Timers;
using BaseObjects;
using BaseObjects.ViewModels;
using SportBetting.WPF.Prism.Modules.Aspects;
using SportBetting.WPF.Prism.Shared;
using SportRadar.DAL.OldLineObjects;
using SportRadar.DAL.ViewObjects;
using SportRadar.DAL.CommonObjects;
using SportBetting.WPF.Prism.Shared.Models;
using TranslationByMarkupExtension;
using System.Windows.Media;
using SportBetting.WPF.Prism.Shared.Converters;
using System.Windows.Media.Imaging;


namespace ViewModels.ViewModels
{

    /// <summary>
    /// BetDomains view model.
    /// </summary>
    [ServiceAspect]
    public sealed class BetDomainsViewModel : BaseViewModel
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BetDomainsViewModel"/> class. 
        /// </summary>
        /// <remarks>
        /// </remarks>

        Timer timer = new Timer(1000);
        public BetDomainsViewModel()
        {
            PlaceBet = new Command<IOddVw>(OnBet);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockSportFilter);
            //Mediator.SendMessage<bool>(true, MsgTag.BlockTimeFilter);
            Mediator.Register<string>(this, LanguageChosen, MsgTag.LanguageChosenHeader);
            Mediator.Register<string>(this, HeaderShowFirstView, MsgTag.RefreshStation);

            var scroller = this.GetScrollviewer();
            if (scroller != null)
            {
                scroller.ScrollToVerticalOffset(0);
            }

            ScrollChangedCommand = new Command<double>(ScrollChanged);

            if (StationRepository.IsStatisticsEnabled && MatchInfo == null && !ChangeTracker.CurrentMatch.IsLiveBet)
            {
                MatchInfo = new MatchStatistic(ChangeTracker.CurrentMatch.LineObject.BtrMatchId, ChangeTracker.CurrentMatch.TournamentView.LineObject.GroupTournament.BtrTournamentId);
            }

            if (ChangeTracker.CurrentMatch != null)
            {
                ChangeTracker.CurrentMatch.IsStartUp = true;
            }
            
            switch (ChangeTracker.CurrentMatch.SportDescriptor)
            {
                case SportSr.SPORT_DESCRIPTOR_SOCCER:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF3F8145"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF90C696"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/socker-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/socker-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#008000")); 
                    break;

                case SportSr.SPORT_DESCRIPTOR_BASKETBALL:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#AF6828"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D8A362"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/Basket-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/Basketball-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AF6828")); 
                    break;

                case SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A5181"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#73B0D8"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/hockey-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/Hokkey-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A5181"));
                    break;

                case SportSr.SPORT_DESCRIPTOR_TENNIS:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#979F0D"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DDE04A"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/tennis-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/tennis-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#DDE04A"));
                    break;

                case SportSr.SPORT_DESCRIPTOR_HANDBALL:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C8C8CA"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F4F4F4"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/hand-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/handball-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EFEFDA"));
                    break;

                case SportSr.SPORT_DESCRIPTOR_RUGBY:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#963D2D"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#989E98"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/rugby-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/rugby-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E86F25"));
                    break;

                case SportSr.SPORT_DESCRIPTOR_VOLLEYBALL:
                    BackgroundBrush = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3685D3"), 1));
                    BackgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FDC903"), 0));
                    PicturePath = (BitmapImage)new ResolveImagePath("LiveView/volley-ball.png").ProvideValue(null);
                    BackgroundPath = (BitmapImage)new ResolveImagePath("LiveView/volleyball-fon.png").ProvideValue(null);
                    SimpleColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8C425"));
                    break;
            }

            LineSr.SubsribeToEvent(DataCopy_DataSqlUpdateSucceeded);
            timer.Elapsed += timer_Tick;
            timer.Start();
            ChangeTracker.IsBetdomainViewOpen = true;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (!ChangeTracker.CurrentMatch.IsLiveBet && ChangeTracker.CurrentMatch.ExpiryDate < DateTime.Now)
            {
                timer.Stop();
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_BETDOMAINS) as string, null, true, 3);
                MyRegionManager.NavigatBack(RegionNames.ContentRegion);
            }
        }

        #endregion

        #region Properties

        //private new static readonly ILog Log = LogManager.GetLogger(typeof(BetDomainsViewModel));
        private SolidColorBrush _simpleColor;
        public SolidColorBrush SimpleColor
        {
            get { return _simpleColor; }
            set
            {
                _simpleColor = value;
                OnPropertyChanged("SimpleColor");
            }
        }


        private BitmapImage _picturePath;
        public BitmapImage PicturePath
        {
            get { return _picturePath; }
            set { _picturePath = value; }
        }

        private BitmapImage _backgroundPath;
        public BitmapImage BackgroundPath
        {
            get { return _backgroundPath; }
            set { _backgroundPath = value; }
        }

        private LinearGradientBrush _backgroundBrush;
        public LinearGradientBrush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set { _backgroundBrush = value; }
        }

        public string BigEndDate
        {
            get
            {
                DateTime date = (ChangeTracker.CurrentMatch != null) ? ChangeTracker.CurrentMatch.ExpiryDate : DateTime.MinValue;
                if (date.Date.CompareTo(DateTime.Today) == 0)
                {
                    string text = date.Hour.ToString();
                    return text;
                }
                else
                {
                    string text = date.Date.Day.ToString() + "  " + date.Hour.ToString();
                    return text;
                }
            }
        }

        private MatchStatistic _matchInfo;

        public MatchStatistic MatchInfo
        {
            get { return _matchInfo; }
            set
            { _matchInfo = value; }
        }

        public string TournamentName
        {
            get
            {
                return ChangeTracker.CurrentMatch.TournamentNameToShow;
            }
        }

        #endregion

        #region Commands
        public Command<IOddVw> PlaceBet { get; private set; }
        public Command<double> ScrollChangedCommand { get; private set; }

        #endregion

        #region Methods

        public override void Close()
        {
            if (ChangeTracker.CurrentMatch != null)
            {
                ChangeTracker.CurrentMatch.IsStartUp = true;

                if (ChangeTracker.CurrentMatch.GoalsTimer != null)
                    ChangeTracker.CurrentMatch.GoalsTimer.Stop();
            }
            LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
            timer.Stop();
            timer.Elapsed -= timer_Tick;
            ChangeTracker.IsBetdomainViewOpen = false;
            base.Close();

        }

        public override void OnNavigationCompleted()
        {
            //if (ChangeTracker.CurrentMatch != null)
            //{
            //    ChangeTracker.CurrentMatch.IsStartUp = true;
            //}

            base.OnNavigationCompleted();
        }

        private void DataCopy_DataSqlUpdateSucceeded(eUpdateType eut, string sProviderDescription)
        {
            if (ChangeTracker.CurrentMatch.VisibleBetDomainCount == 0)
            {
                LineSr.UnsubscribeFromEnent(DataCopy_DataSqlUpdateSucceeded);
                Mediator.SendMessage(true, MsgTag.NavigateBack);
                ShowError(TranslationProvider.Translate(MultistringTags.TERMINAL_NO_BETDOMAINS).ToString(), null, true, 3);

            }
        }




        private void LanguageChosen(string lang)
        {
            if (!ChangeTracker.CurrentMatch.IsLiveBet)
            {
                MatchInfo = new MatchStatistic(ChangeTracker.CurrentMatch.LineObject.BtrMatchId, ChangeTracker.CurrentMatch.TournamentView.LineObject.GroupTournament.BtrTournamentId);
                OnPropertyChanged("MatchInfo");
            }
        }

        private void HeaderShowFirstView(string obj)
        {
            var source = ChangeTracker.CurrentMatch.MatchSource;
            if (source == null) return;
            if (source == eServerSourceType.BtrPre && !StationRepository.IsPrematchEnabled
                || source == eServerSourceType.BtrLive && !StationRepository.IsLiveMatchEnabled
                || source == eServerSourceType.BtrVfl && !StationRepository.AllowVfl
                || source == eServerSourceType.BtrVhc && !StationRepository.AllowVhc)
                Mediator.SendMessage("", MsgTag.ShowFirstViewAndResetFilters);
        }

        #endregion

    }

}