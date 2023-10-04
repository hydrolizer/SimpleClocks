using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleClocks.Utils
{
	public class EnumToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value?.Equals(parameter) ?? DependencyProperty.UnsetValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return DependencyProperty.UnsetValue;
			return value.Equals(true) ? parameter : Binding.DoNothing;
		}
	}
}
