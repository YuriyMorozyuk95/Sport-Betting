using System;
using System.Windows;
using System.Windows.Data;

namespace SportBetting.WPF.Prism
{
	public class InverseBooleanToVisibilityConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(Visibility))
				throw new InvalidOperationException("The target must be a Visibility");

			return !(bool)value ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter,
			System.Globalization.CultureInfo culture)
		{
			throw new NotSupportedException();
		}

		#endregion
	}
}