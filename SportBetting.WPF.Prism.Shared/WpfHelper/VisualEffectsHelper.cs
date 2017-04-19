using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SportBetting.WPF.Prism.WpfHelper
{
    public sealed class VisualEffectsHelper : INotifyPropertyChanged
    {
        #region SingletonLiveSportMatchDetailsIsOpened
        private static readonly VisualEffectsHelper _singleton = new VisualEffectsHelper();
        public static VisualEffectsHelper Singleton => _singleton;
        #endregion

        #region INPC
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion

        #region LiveSportMatchDetailsIsOpened
        private bool _liveSportMatchDetailsIsOpened;
        public bool LiveSportMatchDetailsIsOpened
        {
            get { return _liveSportMatchDetailsIsOpened; }
            set
            {
                _liveSportMatchDetailsIsOpened = value;
                OnPropertyChanged(nameof(LiveSportMatchDetailsIsOpened));
            }
        }
        #endregion
    }

    public sealed class LiveSportMatchDetailsIsOpenedToBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mathDetailsIsOpened = (bool)value;
            if (mathDetailsIsOpened) return new SolidColorBrush(Colors.Blue);
            else return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class NextButtonVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
