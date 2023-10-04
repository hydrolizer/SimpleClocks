using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SimpleClocks.Utils
{
	public class DatabindingDebugConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			//Debugger.Break();
			return value;
		}

		public object ConvertBack(object value, Type targetType,
				object parameter, CultureInfo culture)
		{
			//Debugger.Break();
			return value;
		}
	}

	public class MultiBindingDebugConverter: IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			Debugger.Break();
			return DependencyProperty.UnsetValue;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			Debugger.Break();
			return Enumerable.Repeat(Binding.DoNothing, targetTypes.Length).ToArray();
		}
	}
}
