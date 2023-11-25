using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ColorPicker;
using SimpleClocks.Utils;
using GalaSoft.MvvmLight.CommandWpf;
using SimpleClocks.Utils.Extensions;

namespace SimpleClocks.Borrowed.PixiEditor.ColorPicker
{
	public static class ColorSlidersEx
	{
		public static ICommand UpdateTabsCommand { get; } = new RelayCommand<object>(UpdateTabs);
		static void UpdateTabs(object eventArgs) => UpdateTabs((ColorSliders)((RoutedEventArgs)eventArgs).Source);

		public static void UpdateTabs(ColorSliders colorSliders)
		{
			var tabControl = colorSliders.GetVisualChild<TabControl>();
			if (tabControl == null) return;
			var tabItems = tabControl.Items.OfType<TabItem>().ToList();
			var rgbItem = tabItems.FirstOrDefault(i=>i.Header is string s && "RGB".Equals(s, StringComparison.OrdinalIgnoreCase));
			if (rgbItem == null) return;
			tabControl.Items.Remove(rgbItem);
			tabControl.Items.Insert(0, rgbItem);
			tabControl.SelectedItem = rgbItem;
			tabControl.Margin = new Thickness(0);
			colorSliders.MinHeight = 0;
		}

		public static readonly DependencyProperty ExtraContentProperty = DependencyProperty.RegisterAttached(
			"ExtraContent", typeof(object), typeof(ColorSlidersEx),
			new PropertyMetadata(null)
		);

		public static object GetExtraContent(DependencyObject o) => o.GetValue(ExtraContentProperty);
		public static void SetExtraContent(DependencyObject o, object value) => o.SetValue(ExtraContentProperty, value);
	}
}
