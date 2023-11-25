using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using SimpleClocks.Models;
using SimpleClocks.Services.Intefaces;

namespace SimpleClocks.Dialogs
{
	public partial class SettingsWindow
	{
		readonly ColorSettings _colorSettings;
		internal SettingsWindow(IDialogsService dialogsService, ColorSettings colorSettings)
		{
			InitializeComponent();
			_colorSettings = colorSettings ?? throw new ArgumentNullException(nameof(colorSettings));
			Title = $"Color preferences for {_colorSettings.ProfileName}";
			_colorSettings.InjectResources(WindowPreview);
			_colorSettings.OnColorContanierChanged += ColorSettingsContanierChanged;
			DataContext = new SettingsWindowModel(dialogsService, colorSettings);
			Closed += SettingsWindowClosed;
		}

		private void SettingsWindowClosed(object sender, EventArgs e)
		{
			_colorSettings.OnColorContanierChanged -= ColorSettingsContanierChanged;
		}

		private void ColorSettingsContanierChanged(ColorContainer container)
		{
			WindowPreview.Resources[container.Name] = container.Brush;
		}
	}

	public class ComboBoxX: ComboBox
	{
		readonly IValueConverter _fontStyleConverter = new FontStyleConverter();
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			if (!(GetTemplateChild("contentPresenter") is ContentPresenter cp)) return;
			var binding = new Binding
			{
				Path = new PropertyPath(nameof(ColorContainer.IsChanged)),
				Mode = BindingMode.OneWay,
				Converter = _fontStyleConverter
			};
			BindingOperations.SetBinding(cp, TextElement.FontStyleProperty, binding);
		}

		class FontStyleConverter: IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (!(value is bool b))
					return DependencyProperty.UnsetValue;
				return b ? FontStyles.Italic : FontStyles.Normal;
			}
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
				=> throw new NotImplementedException();
		}
	}
}
