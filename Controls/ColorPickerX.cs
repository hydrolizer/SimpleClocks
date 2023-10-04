using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace SimpleClocks.Controls
{
	public class ColorPickerX : Xceed.Wpf.Toolkit.ColorPicker
	{
		public static readonly DependencyProperty ColorSquareSizeProperty = DependencyProperty.Register(
			"ColorSquareSize", typeof(double), typeof(ColorPickerX),
			new FrameworkPropertyMetadata(
				14d,
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure |
				FrameworkPropertyMetadataOptions.AffectsParentArrange |
				FrameworkPropertyMetadataOptions.AffectsParentMeasure |
				FrameworkPropertyMetadataOptions.AffectsRender,
				OnColorSquareSizePropertyChanged,
				OnColorSquareSizePropertyCoerce
			)
		);

		public static double MinColorSquareSize => 14d;
		public static double MaxColorSquareSize => 50d;

		public double ColorSquareSize
		{
			get => (double) (GetValue(ColorSquareSizeProperty) ?? MinColorSquareSize);
			set => SetValue(ColorSquareSizeProperty, value);
		}

		static void OnColorSquareSizePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
		{}

		static object OnColorSquareSizePropertyCoerce(DependencyObject o, object baseValue)
		{
			if (!(baseValue is double d))
				return MinColorSquareSize;
			if (d < MinColorSquareSize)
				return MinColorSquareSize;
			if (d > MaxColorSquareSize)
				return MaxColorSquareSize;
			return d;
		}

		public static readonly DependencyProperty ColorSquareSizeHeaderProperty = DependencyProperty.Register(
			"ColorSquareSizeHeader", typeof(string), typeof(ColorPickerX),
			new UIPropertyMetadata("Color square size:")
		);

		public string ColorSquareSizeHeader
		{
			get => (string) GetValue(ColorSquareSizeHeaderProperty);
			set => SetValue(ColorSquareSizeHeaderProperty, value);
		}

		ListBox _availableColors;
		ListBox _standardColors;
		ColorCanvas _colorCanvas;

		static readonly FieldInfo selectionChangedFieldInfo =
			typeof(Xceed.Wpf.Toolkit.ColorPicker).GetField("_selectionChanged", BindingFlags.Instance | BindingFlags.NonPublic);

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			if (selectionChangedFieldInfo == null) return;
			_colorCanvas = GetTemplateChild("ColorCanvas") as ColorCanvas;
			if (_colorCanvas == null) return;
			_colorCanvas.SelectedColorChanged += (s, e) => Console.WriteLine("Canvas color changed: {0}/{1}", e.OldValue, e.NewValue);
			_availableColors = GetTemplateChild("PART_AvailableColors") as ListBox;
			if (_availableColors != null)
			{
				ClearSelectionChangedEvent(_availableColors);
				_availableColors.SelectionChanged += ColorSelectionChanged;
			}
			_standardColors = GetTemplateChild("PART_StandardColors") as ListBox;
			if (_standardColors != null)
			{
				ClearSelectionChangedEvent(_standardColors);
				_standardColors.SelectionChanged += ColorSelectionChanged;
			}
		}

		public IFormattedColorSource ColorSource => _colorCanvas as IFormattedColorSource;

		static void ClearSelectionChangedEvent(ListBox listBox)
		{
			var piEventHandlersStore = typeof(Selector).GetProperty("EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
			if (piEventHandlersStore == null) return;
			var eventHandlersStore = piEventHandlersStore.GetValue(listBox, null);
			if (eventHandlersStore == null) return;
			var miGetEventHandlers = eventHandlersStore.GetType().GetMethod("GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public);
			if (miGetEventHandlers == null) return;
			var handlers = miGetEventHandlers.Invoke(eventHandlersStore, new object[] {Selector.SelectionChangedEvent}) as RoutedEventHandlerInfo[];
			if (handlers == null || handlers.Length == 0) return;
			foreach(var h in handlers)
				listBox.RemoveHandler(Selector.SelectionChangedEvent, h.Handler);
		}

		private void ColorSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var listBox = (ListBox)sender;
			if (e.AddedItems.Count <= 0) return;
			var colorItem = (ColorItem)e.AddedItems[0];
			if (!colorItem.Color.HasValue) return;
			var newColor = colorItem.Color.Value;
			var alpha = _colorCanvas.A;
			SelectedColor = alpha<255
				? Color.FromArgb(alpha, newColor.R, newColor.G, newColor.B)
				: newColor;
			if (!string.IsNullOrEmpty(colorItem.Name))
			{
				SelectedColorText = colorItem.Name;
			}
			//UpdateRecentColors(colorItem);
			//_selectionChanged = true;
			selectionChangedFieldInfo.SetValue(this, true);
			listBox.SelectedIndex = -1;
		}
	}

	internal class DoubleToByteConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is double d ? (byte) d : (byte) ColorPickerX.MinColorSquareSize;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is byte b ? b : ColorPickerX.MinColorSquareSize;
	}

	internal class AlphaToOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is byte b ? (255 - b) * 100d / 255d : DependencyProperty.UnsetValue;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch(value)
			{
				case double d:
					return (byte)Math.Round((100 - d) * 255d / 100d);
				case byte n:
					return (byte)Math.Round((100 - n) * 255d / 100d);
				default:
					return Binding.DoNothing;
			}
		}
	}

	public class SelectedColorTextConverter: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			=> value is string s && !string.IsNullOrEmpty(s)
				? Visibility.Visible
				: Visibility.Collapsed;

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			=> throw new NotImplementedException();
	}
}
