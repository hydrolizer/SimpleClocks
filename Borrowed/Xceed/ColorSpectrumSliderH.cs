using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace SimpleClocks.Borrowed.Xceed
{

	[TemplatePart(Name = "PART_SpectrumDisplay", Type = typeof(Rectangle))]
	public class ColorSpectrumSliderH : ColorSpectrumSlider
	{
		const string PartSpectrumDisplay = "PART_SpectrumDisplay";
		private Rectangle _spectrumDisplay;

		private LinearGradientBrush _pickerBrush;

		static ColorSpectrumSliderH()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSpectrumSliderH), new FrameworkPropertyMetadata(typeof(ColorSpectrumSliderH)));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_spectrumDisplay = (Rectangle)GetTemplateChild(PartSpectrumDisplay);
			CreateSpectrum();
			OnValueChanged(double.NaN, Value);
		}

		protected override void OnValueChanged(double oldValue, double newValue)
		{
			base.OnValueChanged(oldValue, newValue);
			SelectedColor = ColorUtilities.ConvertHsvToRgb(360.0 - newValue, 1.0, 1.0);
		}

		private void CreateSpectrum()
		{
			_pickerBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0.0, 0.5),
				EndPoint = new Point(1.0, 0.5),
				ColorInterpolationMode = ColorInterpolationMode.SRgbLinearInterpolation
			};
			var list = ColorUtilities.GenerateHsvSpectrum();
			var num = 1.0 / (list.Count - 1);
			int i;
			for (i = 0; i < list.Count; i++)
			{
				_pickerBrush.GradientStops.Add(new GradientStop(list[i], i * num));
			}
			_pickerBrush.GradientStops[i - 1].Offset = 1.0;
			_pickerBrush.Freeze();
			if (_spectrumDisplay != null)
			{
				_spectrumDisplay.Fill = _pickerBrush;
			}
		}
	}
}
