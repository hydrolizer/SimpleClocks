using SimpleClocks.Models;
using System.Windows;
using System.Windows.Controls;

namespace SimpleClocks.Controls
{
	class MainWindowPreview : Control
	{
		public static readonly DependencyProperty ColorSettingsProperty = DependencyProperty.RegisterAttached(
			nameof(ColorSettings), typeof(ColorSettings), typeof(MainWindowPreview),
			new PropertyMetadata(null)
		);

		public ColorSettings ColorSettings
		{
			get => (ColorSettings)GetValue(ColorSettingsProperty);
			set => SetValue(ColorSettingsProperty, value);
		}
	}
}
