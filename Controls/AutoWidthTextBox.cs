using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using SimpleClocks.Utils;

namespace SimpleClocks.Controls
{
	[Flags]
	public enum AutoWidthMode
	{
		None=0x0,
		MinWidth = 0x1,
		MaxWidth = 0x2,
		All = MinWidth | MaxWidth
	}

	public enum AutoWidthAdjustMode
	{
		None,
		ByMinimum,
		ByMaximum
	}

	public abstract class AutoWidthTextBox : TextBox
	{
		readonly TypefaceConverter _typefaceConverter = new TypefaceConverter();
		protected AutoWidthTextBox()
		{
			var typefaceBinding = new MultiBinding
			{
				Converter = _typefaceConverter,
				Mode = BindingMode.OneWay
			};
			foreach (var dp in new[]
				{
					FontFamilyProperty,
					FontStyleProperty,
					FontWeightProperty,
					FontStretchProperty
				})
				typefaceBinding.Bindings.Add(new Binding { Source = this, Path = new PropertyPath(dp) });
			BindingOperations.SetBinding(this, typefaceProperty, typefaceBinding);
			Loaded += AutoWidthTextBoxLoaded;
		}

		private void AutoWidthTextBoxLoaded(object sender, RoutedEventArgs e)
		{
			Loaded -= AutoWidthTextBoxLoaded;
			RecalcWidth();
		}

		protected abstract string GetTextForMeasure();

		void RecalcWidth(
			AutoWidthMode? mode = null,
			int? minTextLength = null,
			int? maxTextLength = null,
			double? autoWidthDelta = null,
			AutoWidthAdjustMode? adjustMode = null
		)
		{
			var tfm = GetTextForMeasure();
			if (string.IsNullOrEmpty(tfm)) return;
			var lMode = mode ?? AutoWidthMode;
			if (lMode==AutoWidthMode.None) return;
			var lMinLength = minTextLength ?? MinTextLength;
			var lMaxLength = maxTextLength ?? MaxTextLength;
			if (lMinLength == DefaultMinTextLength && lMaxLength == DefaultMaxTextLength)
				return;
			var lAdjustMode = adjustMode ?? AutoWidthAdjustMode;
			var lengths = tfm
				.Select(c => GetText(c.ToString(CultureInfo.CurrentUICulture)).Width)
				.Where(w => DoubleUtil.IsValidDouble(w) && w>0)
				.ToList();
			var minWidth = lengths.Min() * lMinLength + (autoWidthDelta ?? AutoWidthDelta);
			var maxWidth = lengths.Max() * lMaxLength + (autoWidthDelta ?? AutoWidthDelta);
			switch(lAdjustMode)
			{
				case AutoWidthAdjustMode.ByMaximum:
					minWidth = maxWidth;
					break;
				case AutoWidthAdjustMode.ByMinimum:
					maxWidth = minWidth;
					break;
			}
			SetCurrentValue(MinWidthProperty, (lMode & AutoWidthMode.MinWidth) == AutoWidthMode.MinWidth ? minWidth : 0);
			SetCurrentValue(MaxWidthProperty, (lMode & AutoWidthMode.MaxWidth) == AutoWidthMode.MaxWidth ? maxWidth : double.PositiveInfinity);
		}

		FormattedText GetText(string text)
			=> new FormattedText(
				text,
				CultureInfo.InvariantCulture,
				FlowDirection.LeftToRight,
				Typeface,
				FontSize,
				Foreground,
				null, //NumberSubstitution
				TextOptions.GetTextFormattingMode(this),
				96d
			);

		const AutoWidthMode DefaultAutoWidthMode = AutoWidthMode.None;
		public static readonly DependencyProperty AutoWidthModeProperty = DependencyProperty.Register(
			nameof(AutoWidthMode), typeof(AutoWidthMode), typeof(AutoWidthTextBox),
			new FrameworkPropertyMetadata(
				DefaultAutoWidthMode,
				FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnAutoWidthModePropertyChanged
			)
		);

		public AutoWidthMode AutoWidthMode
		{
			get => (AutoWidthMode)(GetValue(AutoWidthModeProperty) ?? DefaultAutoWidthMode);
			set => SetValue(AutoWidthModeProperty, value);
		}

		static void OnAutoWidthModePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb && ea.NewValue is AutoWidthMode awm))
				return;
			awtb.RecalcWidth(mode: awm);
		}

		const AutoWidthAdjustMode DefaultAutoWidthAdjustMode = AutoWidthAdjustMode.None;
		public static readonly DependencyProperty AutoWidthAdjustModeProperty = DependencyProperty.Register(
			nameof(AutoWidthAdjustMode), typeof(AutoWidthAdjustMode), typeof(AutoWidthTextBox),
			new FrameworkPropertyMetadata(
				DefaultAutoWidthAdjustMode,
				FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnAutoWidthAdjustModePropertyChanged
			)
		);

		public AutoWidthAdjustMode AutoWidthAdjustMode
		{
			get => (AutoWidthAdjustMode)(GetValue(AutoWidthAdjustModeProperty) ?? DefaultAutoWidthAdjustMode);
			set => SetValue(AutoWidthAdjustModeProperty, value);
		}

		static void OnAutoWidthAdjustModePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb && ea.NewValue is AutoWidthAdjustMode awdm))
				return;
			awtb.RecalcWidth(adjustMode: awdm);
		}

		static AutoWidthTextBox()
		{
			defaultTypeface = new Typeface(
				new FontFamily("Verdana"),
				FontStyles.Normal,
				FontWeights.Normal,
				FontStretches.Normal
			);
			typefaceProperty = DependencyProperty.Register(
				nameof(Typeface), typeof(Typeface), typeof(AutoWidthTextBox),
				new PropertyMetadata(defaultTypeface, OnTypefacePropertyChanged)
			);
		}

		static readonly Typeface defaultTypeface;
		static readonly DependencyProperty typefaceProperty;
		static void OnTypefacePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb))
				return;
			awtb.RecalcWidth();
		}

		Typeface Typeface => (Typeface)GetValue(typefaceProperty);

		const int DefaultMinTextLength = 0;
		public static readonly DependencyProperty MinTextLengthProperty = DependencyProperty.Register(
			nameof(MinTextLength), typeof(int), typeof(AutoWidthTextBox),
			new FrameworkPropertyMetadata(
				DefaultMinTextLength,
				FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnMinTextLengthPropertyChanged
			)
		);

		static void OnMinTextLengthPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb && ea.NewValue is int n))
				return;
			awtb.RecalcWidth(minTextLength: n);
		}

		public int MinTextLength
		{
			get => (int)(GetValue(MinTextLengthProperty) ?? DefaultMinTextLength);
			set => SetValue(MinTextLengthProperty, value);
		}

		const int DefaultMaxTextLength = int.MaxValue;
		public static readonly DependencyProperty MaxTextLengthProperty = DependencyProperty.Register(
			nameof(MaxTextLength), typeof(int), typeof(AutoWidthTextBox),
			new FrameworkPropertyMetadata(
				DefaultMaxTextLength,
				FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnMaxTextLengthPropertyChanged
			)
		);

		static void OnMaxTextLengthPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb && ea.NewValue is int n))
				return;
			awtb.RecalcWidth(maxTextLength: n);
		}

		public int MaxTextLength
		{
			get => (int)(GetValue(MaxTextLengthProperty) ?? DefaultMaxTextLength);
			set => SetValue(MaxTextLengthProperty, value);
		}

		const double DefaultAutoWidthDelta = 0d;
		public static readonly DependencyProperty AutoWidthDeltaProperty = DependencyProperty.Register(
			nameof(AutoWidthDelta), typeof(double), typeof(AutoWidthTextBox),
			new FrameworkPropertyMetadata(
				DefaultAutoWidthDelta,
				FrameworkPropertyMetadataOptions.AffectsMeasure,
				OnAutoWidthDeltaPropertyChanged
			)
		);

		static void OnAutoWidthDeltaPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(o is AutoWidthTextBox awtb && ea.NewValue is double d))
				return;
			awtb.RecalcWidth(autoWidthDelta: d);
		}

		public double AutoWidthDelta
		{
			get => (double)(GetValue(AutoWidthDeltaProperty) ?? 1d);
			set => SetValue(AutoWidthDeltaProperty, value);
		}

		class TypefaceConverter: IMultiValueConverter
		{
			public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
			{
				if (!(
						values != null &&
						values.Length == 4 &&
						values[0] is FontFamily fontFamily &&
						values[1] is FontStyle fontStyle &&
						values[2] is FontWeight fontWeight &&
						values[3] is FontStretch fontStretch
					))
					return defaultTypeface;
				return new Typeface(
					fontFamily,
					fontStyle,
					fontWeight,
					fontStretch
				);
			}

			public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

	}
}
