using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace SimpleClocks.Utils
{
	public enum ConvertFormat
	{
		WithType,
		WithoutType
	}

	public class ToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var wt = parameter is ConvertFormat format && format == ConvertFormat.WithoutType;
			if (value == null) return "null";
			if (value == DependencyProperty.UnsetValue) return "UnsetValue";
			return string.Format("{0}{1}",
				wt ? string.Empty : string.Format("{0}: ", value.GetType().Name),
				value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public class StringJoinConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values == null)
				return DependencyProperty.UnsetValue;
			if (!(parameter is string joinFormat))
				joinFormat = "{0}{1}";
			return values.Select(v => v?.ToString() ?? "null").Aggregate((a, b) => string.Format(joinFormat, a, b));
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
