﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using SportBetting.WPF.Prism.Shared.Converters;
using SportRadar.Common.Logs;
using SportRadar.DAL.OldLineObjects;

namespace SportBetting.WPF.Prism.Shared
{
    public class SportDescriptorImageSelector : IValueConverter
    {
        private static readonly ILog _logger = LogFactory.CreateLog(typeof(SportDescriptorImageSelector));
        private static IDictionary<string, BitmapImage> imagecash = new Dictionary<string, BitmapImage>(100);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object image = null;
            if (parameter != null)
            {
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                    image = ResolveImagePath.ResolvePath("LiveView/hockey-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                    image = ResolveImagePath.ResolvePath("LiveView/socker-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/Basket-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                    image = ResolveImagePath.ResolvePath("Baseball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                    image = ResolveImagePath.ResolvePath("MixedSports.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                    image = ResolveImagePath.ResolvePath("LiveView/tennis-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                    image = ResolveImagePath.ResolvePath("LiveView/rugby-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/hand-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                    image = ResolveImagePath.ResolvePath("LiveView/volley-ball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                    image = ResolveImagePath.ResolvePath("AmericanFootball.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                    image = ResolveImagePath.ResolvePath("MotoSport.png");
                if(parameter.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                    image = ResolveImagePath.ResolvePath("winter_sports_icon.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                    image = ResolveImagePath.ResolvePath("Darts.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_OLYMPICS)
                    image = ResolveImagePath.ResolvePath("olympics-icon.png");
                if (parameter.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                    image = ResolveImagePath.ResolvePath("Snooker.png");
            }
            else
            {
                if (value != null)
                {
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_ICE_HOCKEY)
                        image = ResolveImagePath.ResolvePath("LiveView/hockey-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_SOCCER)
                        image = ResolveImagePath.ResolvePath("LiveView/socker-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_BASKETBALL)
                        image = ResolveImagePath.ResolvePath("LiveView/Basket-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_BASEBALL)
                        image = ResolveImagePath.ResolvePath("Baseball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_MIXED)
                        image = ResolveImagePath.ResolvePath("MixedSports.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_TENNIS)
                        image = ResolveImagePath.ResolvePath("LiveView/tennis-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_RUGBY)
                        image = ResolveImagePath.ResolvePath("LiveView/rugby-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_HANDBALL)
                        image = ResolveImagePath.ResolvePath("LiveView/hand-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_VOLLEYBALL)
                        image = ResolveImagePath.ResolvePath("LiveView/volley-ball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_FOOTBALL)
                        image = ResolveImagePath.ResolvePath("AmericanFootball.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_MOTOSPORT)
                        image = ResolveImagePath.ResolvePath("MotoSport.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_WINTERSPORTS)
                        image = ResolveImagePath.ResolvePath("winter_sports_icon.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_DARTS)
                        image = ResolveImagePath.ResolvePath("Darts.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_OLYMPICS)
                        image = ResolveImagePath.ResolvePath("olympics-icon.png");
                    if (value.ToString() == SportSr.SPORT_DESCRIPTOR_SNOOKER)
                        image = ResolveImagePath.ResolvePath("Snooker.png");
                }
            }
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
