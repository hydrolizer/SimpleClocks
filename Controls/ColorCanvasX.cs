using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Expression = System.Linq.Expressions.Expression;

namespace SimpleClocks.Controls
{
	public interface IFormattedColorSource
	{
		string HexadecimalString { get; }
	}

	public class ColorCanvasX : ColorCanvas, IFormattedColorSource
	{
		static readonly Func<ColorCanvas, Point?> currentColorPosition;
		static readonly Action<ColorCanvas, Point, bool> updatePositionInfo;
		static readonly Action<ColorCanvas, string, bool> setHexadecimalStringProperty;

		static ColorCanvasX()
		{
			setHexadecimalStringProperty = GetHexadecimalStringPropertyUpdateProc();
			var ccp = GetCurrentColorPositionFunc();
			if (ccp == null)
				return;
			var upi = GetUpdateProc();
			if (upi == null)
				return;
			currentColorPosition = ccp;
			updatePositionInfo = upi;
		}

		public ColorCanvasX()
		{
			TextBoxKeyDownCommand = new RelayCommand<object>(OnTextBoxKeyDown);
			TextBoxLostFocusCommand = new RelayCommand<object>(OnTextBoxLostFocus);
		}

		static Func<ColorCanvas, Point?> GetCurrentColorPositionFunc()
		{
			var fiCurrentColorPosition = typeof(ColorCanvas).GetField("_currentColorPosition", BindingFlags.Instance | BindingFlags.NonPublic);
			if (fiCurrentColorPosition == null)
				return null;
			var sourceExpr = Expression.Parameter(typeof(ColorCanvas));
			var fieldExpr = Expression.Field(sourceExpr, fiCurrentColorPosition);
			return Expression.Lambda<Func<ColorCanvas, Point?>>(fieldExpr, sourceExpr).Compile();
		}

		static Action<ColorCanvas, Point, bool> GetUpdateProc()
		{
			var miUpdate = typeof(ColorCanvas).GetMethod("UpdateColorShadeSelectorPositionAndCalculateColor", BindingFlags.Instance | BindingFlags.NonPublic);
			if (miUpdate == null)
				return null;
			var sourceExpr = Expression.Parameter(typeof(ColorCanvas));
			var @params = miUpdate.GetParameters()
				.Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
				.ToList();
			var methodExpr = Expression.Call(sourceExpr, miUpdate, @params);
			return Expression.Lambda<Action<ColorCanvas, Point, bool>>(methodExpr, new []{sourceExpr}.Concat(@params)).Compile();
		}

		static Action<ColorCanvas, string, bool> GetHexadecimalStringPropertyUpdateProc()
		{
			var miUpdate = typeof(ColorCanvas).GetMethod("SetHexadecimalStringProperty", BindingFlags.Instance | BindingFlags.NonPublic);
			if (miUpdate == null)
				return null;
			var sourceExpr = Expression.Parameter(typeof(ColorCanvas));
			var @params = miUpdate.GetParameters()
				.Select(pi => Expression.Parameter(pi.ParameterType, pi.Name))
				.ToList();
			var methodExpr = Expression.Call(sourceExpr, miUpdate, @params);
			return Expression.Lambda<Action<ColorCanvas, string, bool>>(methodExpr, new[] { sourceExpr }.Concat(@params)).Compile();
		}

		Canvas _colorShadingCanvas;
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_colorShadingCanvas = GetTemplateChild("PART_ColorShadingCanvas") as Canvas;
		}

		void SetHexadecimalStringProperty(TextBox textBox)
		{
			if (setHexadecimalStringProperty == null)
				return;
			setHexadecimalStringProperty(this, textBox.Text, true);
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
			if (_colorShadingCanvas == null || currentColorPosition==null || updatePositionInfo==null) return;
			var ccp = currentColorPosition?.Invoke(this);
			if (ccp == null) return;
			var p = new Point
			{
				X = ccp.Value.X * _colorShadingCanvas.ActualWidth,
				Y = ccp.Value.Y * _colorShadingCanvas.ActualHeight
			};
			updatePositionInfo.Invoke(this, p, true);
		}
	}
}