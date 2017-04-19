using System.ComponentModel;
using System.Runtime.CompilerServices;
using SportBetting.WPF.Prism.Shared.Annotations;
using SportBetting.WPF.Prism.Shared.Converters;
using SportRadar.DAL.OldLineObjects;
using System.Windows.Media;
using System.Collections.Generic;
using System.Linq;

namespace SportBetting.WPF.Prism.Shared.Models
{
    public class Category : INotifyPropertyChanged
    {

        public Category()
            : this(0, null)
        {
        }

        public Category(long id, string name)
        {
            Id = id;
            Name = name;
        }

        public Category(string displayName, long groupId, int sortValue, string sportDescriptor, int matchesCount)
        {
            Name = displayName;
            Id = groupId;
            Sort = sortValue;
            SportDescriptor = sportDescriptor;
            MatchesCount = matchesCount;
            switch (SportDescriptor)
            {
                case SportSr.SPORT_DESCRIPTOR_SOCCER:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF3F8145"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FF90C696"), 0));                    
                    break;

                case SportSr.SPORT_DESCRIPTOR_BASKETBALL:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#AF6828"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#D8A362"), 0));                    
                    break;

                case SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A5181"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#73B0D8"), 0));                    
                    break;

                case SportSr.SPORT_DESCRIPTOR_TENNIS:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#979F0D"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#DDE04A"), 0));                   
                    break;
                case SportSr.SPORT_DESCRIPTOR_HANDBALL:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#C8C8CA"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#F4F4F4"), 0));
                    break;

                case SportSr.SPORT_DESCRIPTOR_RUGBY:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#963D2D"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#989E98"), 0));
                    break;

                case SportSr.SPORT_DESCRIPTOR_VOLLEYBALL:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3685D3"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#FDC903"), 0));
                    break;
                default:
                    Color = new LinearGradientBrush() { StartPoint = new System.Windows.Point(0, 0), EndPoint = new System.Windows.Point(0, 1) };
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#185a9d"), 1));
                    Color.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#43cea2"), 0));
                    break;

            }

            _instances.Add(this);
        }


        public long Id { get; set; }

        public int Sort { get; set; }

        public LinearGradientBrush Color { get; set; }

        private bool _isButtonChecked = false;
        public bool IsButtonChecked
        {
            get { return _isButtonChecked; }
            set
            {
                _isButtonChecked = value;
                OnPropertyChanged();
                if(value == true)
                {
                    foreach(var instance in _instances.Where(i => !ReferenceEquals(i, this)))
                    {
                        instance.IsButtonChecked = false;
                    }
                }
            }
        }

        public int MatchesCount { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string SportDescriptor = "";

        private string _name;

        public object BackgroundBySport
        {
            get
            {
                object image = null;

                switch (this.SportDescriptor)
                {
                    case SportSr.SPORT_DESCRIPTOR_SOCCER:
                        image = ResolveImagePath.ResolvePath("LiveView/socker-fon.png");
                        break;

                    case SportSr.SPORT_DESCRIPTOR_BASKETBALL:
                        image = ResolveImagePath.ResolvePath("LiveView/Basketball-fon.png");
                        break;

                    case SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY:
                        image = ResolveImagePath.ResolvePath("LiveView/Hokkey-fon.png");
                        break;
                    case SportSr.SPORT_DESCRIPTOR_TENNIS:
                        image = ResolveImagePath.ResolvePath("LiveView/tennis-fon.png");
                        break;
                    case SportSr.SPORT_DESCRIPTOR_RUGBY:
                        image = ResolveImagePath.ResolvePath("LiveView/rugby-fon.png").ToString();                      
                        break;
                    case SportSr.SPORT_DESCRIPTOR_HANDBALL:
                        image = ResolveImagePath.ResolvePath("LiveView/handball-fon.png").ToString();
                        break;
                    case SportSr.SPORT_DESCRIPTOR_VOLLEYBALL:
                        image = ResolveImagePath.ResolvePath("LiveView/volleyball-fon.png").ToString();
                        break;
                    default:
                        image = ResolveImagePath.ResolvePath("BackgroundTile.png");
                        break;
                }
                return image;

            }
        }

        public object CategoryIconBySport
        {
            get
            {
                object image = null;

                if (this.SportDescriptor.ToString() == "")
                    image = ResolveImagePath.ResolvePath("OtherSportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    image = ResolveImagePath.ResolvePath("LiveView/hockey-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    image = ResolveImagePath.ResolvePath("LiveView/socker-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    image = ResolveImagePath.ResolvePath("BaseballInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                    image = ResolveImagePath.ResolvePath("MixedSportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    image = ResolveImagePath.ResolvePath("LiveView/tennis-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    image = ResolveImagePath.ResolvePath("LiveView/rugby-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/hand-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/volley-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    image = ResolveImagePath.ResolvePath("AmFootballInctive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    image = ResolveImagePath.ResolvePath("MotorsportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    image = ResolveImagePath.ResolvePath("SnookerInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.ALL_SPORTS)
                    image = ResolveImagePath.ResolvePath("AllsportsInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/Basket-ball.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                    image = ResolveImagePath.ResolvePath("DartslInactive.png");
                else if (this.SportDescriptor.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    image = ResolveImagePath.ResolvePath("WintersportsInactive.png");
                else
                    image = ResolveImagePath.ResolvePath("OtherSportsInactive.png");

                return image;
            }
        }

        public static List<Category> _instances = new List<Category>();

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}