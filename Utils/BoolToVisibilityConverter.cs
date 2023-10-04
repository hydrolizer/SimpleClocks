using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleClocks.Utils
{
	public class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value is Visibility v && v == Visibility.Visible;
		}
	}

	public class BoolToVisibilityInverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var nonVisibility = parameter != null ? (Visibility) parameter : Visibility.Collapsed;
			return (bool)value ? nonVisibility : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Collapsed;
		}
	}

	public class VisibilityToBoolConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return (Visibility)value == Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && (bool)value ? Visibility.Visible : Visibility.Collapsed;
		}
	}
}
