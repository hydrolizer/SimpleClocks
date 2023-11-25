using GalaSoft.MvvmLight.CommandWpf;
using SimpleClocks.Utils.Extensions;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;

namespace SimpleClocks.Controls
{
	public interface IFormattedColorSource
	{
		string HexadecimalString { get; }
	}

	public class ColorCanvasX : ColorCanvas, IFormattedColorSource
	{

		public ColorCanvasX()
		{
			TextBoxKeyDownCommand = new RelayCommand<object>(OnTextBoxKeyDown);
			TextBoxLostFocusCommand = new RelayCommand<object>(OnTextBoxLostFocus);
		}

		static readonly Lazy<Func<ColorCanvas, Point?>> lazyGetCurrentColorPosition = new Lazy<Func<ColorCanvas, Point?>>(
			() => ExpressionEx.FieldGet<ColorCanvas, Point?>("_currentColorPosition")
		);

		static readonly Lazy<Action<ColorCanvas, Point, bool>> lazyUpdateColorShade = new Lazy<Action<ColorCanvas, Point, bool>>(
			() => ExpressionEx.InstanceMethodInvoke<ColorCanvas, Action<ColorCanvas, Point, bool>>("UpdateColorShadeSelectorPositionAndCalculateColor")
		);

		static readonly Lazy<Action<ColorCanvas, string, bool>> lazySetHexadecimalString = new Lazy<Action<ColorCanvas, string, bool>>(
			() => ExpressionEx.InstanceMethodInvoke<ColorCanvas, Action<ColorCanvas, string, bool>>("SetHexadecimalStringProperty")
		);

		Canvas _colorShadingCanvas;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_colorShadingCanvas = GetTemplateChild("PART_ColorShadingCanvas") as Canvas;
		}

		void SetHexadecimalStringProperty(TextBox textBox)
		{
			lazySetHexadecimalString.Value(this, textBox.Text, true);
			textBox.SetCurrentValue(TextBox.TextProperty, HexadecimalString);
		}

		void OnTextBoxKeyDown(object parameter)
		{
			if (!(parameter is KeyEventArgs kea && kea.Source is TextBox tb && kea.Key == Key.Return))
				return;
			SetHexadecimalStringProperty(tb);
		}

		void OnTextBoxLostFocus(object parameter)
		{
			if(!(parameter is RoutedEventArgs rea && rea.OriginalSource is TextBox tb))
				return;
			SetHexadecimalStringProperty(tb);
		}

		public RelayCommand<object> TextBoxKeyDownCommand { get; }
		public RelayCommand<object> TextBoxLostFocusCommand { get; }
		protected override void OnSelectedColorChanged(Color? oldValue, Color? newValue)
		{
			base.OnSelectedColorChanged(oldValue, newValue);
			UpdateSelector();
		}

		void UpdateSelector()
		{
			if (_colorShadingCanvas == null) return;
			var ccp = lazyGetCurrentColorPosition.Value(this);
			if (ccp == null) return;
			var p = new Point
			{
				X = ccp.Value.X * _colorShadingCanvas.ActualWidth,
				Y = ccp.Value.Y * _colorShadingCanvas.ActualHeight
			};
			lazyUpdateColorShade.Value(this, p, true);
		}
	}
}