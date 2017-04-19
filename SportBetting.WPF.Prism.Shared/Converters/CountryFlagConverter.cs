using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Services.Interfaces;

namespace SportBetting.WPF.Prism.Shared
{
    public class CountryFlagConverter : IValueConverter
    {
        public object Convert(object values, Type targetType, object parameter, CultureInfo culture)
        {
            byte[] src = System.Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABYAAAAQCAYAAAAS7Y8mAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAIGNIUk0AAHolAACAgwAA+f8AAIDpAAB1MAAA6mAAADqYAAAXb5JfxUYAAAJLSURBVHjaxNRPSJMBGMfx7/vundve6eZepwNt0XApahIqri79MTp4Lzp08VB07NitS9Sx6BCFRBChRB0KUoouGYVURklm6Fb4f7mpW5tT9+59975vh44KTqF67s+Hh9/D8wiWZVn8hRL+G2wYFms5leRKloXFIgG/TDDoRXaJSDZhd3C+YPJlLIW0+hXNlSSeFXCJMqoZBKGO4xEffsW+M1jTLHqfxknH3tBzYgllv8jChkQmLdP/QuHRy0rOdtdy7WKIinLbpn5xK7RoWAx9yHL19hTLecDtJL7hIrrio+9tPU+GQ6TSLu4+jvN8KIVpbjakreB0psj9gQRLCRfv5poYHM+Q0h18nvYwNuUjmZUQnOuoOYMb/XFOHlGoqpS2j2LgVZpL12eZnBEQKz2EawXCQQtflYEo2bBjMDopMDqi4XbkGLjTQNch3/YTR3/kWVvXwObCNARi03bmMzKH2yx6uuZoa47x/ruDe1KA6CedhZ9qaVE0hp24ZRFSIthMQCefs5iIGqgdCcq980Q6JRR3il5LQfG1lLa8yMEK2g94oaiDqf/BTZXlpMbgsJO5CQ8NQhmKbhHa46G92VMaXK2Ucf5UDQ65ADkVLBPsBQy9yMhkLfGEn/EJg1sPvURamwlU20uDRQGOdni5fKEOh6hCJgfZNdByNO1bIb2uc6WvBrevhTPdAURhh5dX0CyevU5x88Eis1MqRbvFsc5f+L0y4b1hzp0O4HGLu/sVmm4xE1f5+G2V2MwGrWE7jSEf9UEZ2SX8++/2ewAzoAXjSoPlRQAAAABJRU5ErkJggg==");
            Image image;
            using (MemoryStream ms = new MemoryStream(src))
            {
                var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);

                return decoder.Frames[0];
            }
        }



        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}