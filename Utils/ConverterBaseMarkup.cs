using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SimpleClocks.Utils
{
	public abstract class ConvertorBase<T> : MarkupExtension, IValueConverter
		where T : class, new()
	{
		public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);
		public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();

		public override object ProvideValue(IServiceProvider serviceProvider)
			=> _converter ?? (_converter = new T());

		static T _converter;
	}
}
