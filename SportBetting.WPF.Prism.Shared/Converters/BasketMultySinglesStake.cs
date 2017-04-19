using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using IocContainer;
using Ninject;
using Shared;
using SportBetting.WPF.Prism.Services.Interfaces;
using SportBetting.WPF.Prism.Shared.Services.Interfaces;

namespace SportBetting.WPF.Prism.Converters
{
    public class BasketMultySinglesStake : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            Dictionary<long,Ticket> dictX = IoCContainer.Kernel.Get<IChangeTracker>().MultipleSingles;
            string sStake = dictX == null ? "" : 
                dictX.Keys.Contains((long)values[0]) ? dictX[(long)values[0]].Stake.ToString() : "";

            decimal dS = 0m;
            if (sStake != "")
            {
                if (dictX[(long) values[0]].TipItems[0].IsChecked)
                {
                    dS = decimal.Parse(sStake);
                }
                else
                {
                    dS = 0;
                }
            }

            return dS;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
