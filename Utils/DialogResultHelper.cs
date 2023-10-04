using System.Windows;
using System.Windows.Controls;

namespace SimpleClocks.Utils
{
	public static class DialogResultHelper
	{
		public static readonly DependencyProperty ResultProperty = DependencyProperty.RegisterAttached(
			"Result", typeof(bool?), typeof(DialogResultHelper),
			new PropertyMetadata(null, OnResultPropertyChanged));

		public static bool? GetResult(DependencyObject o)
		{
			return (bool?)o.GetValue(ResultProperty);
		}

		public static void SetResult(DependencyObject o, bool? value)
		{
			o.SetValue(ResultProperty, value);
		}

		static void OnResultPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is Button btn)) return;
			btn.Click -= HandleResult;
			if (ea.NewValue == null) return;
			btn.Click += HandleResult;
			if (ea.NewValue is bool res && !res)
				btn.IsCancel = true;
		}

		static void HandleResult(object sender, RoutedEventArgs ea)
		{
			if (!(sender is Button btn) || GetResult(btn) == null) return;
			var window = Window.GetWindow(btn);
			if (window == null) return;
			window.DialogResult = GetResult(btn);
			window.Close();
		}
	}
}
