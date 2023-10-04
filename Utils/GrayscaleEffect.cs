using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace SimpleClocks.Utils
{
	public class GrayscaleEffect : ShaderEffect
	{
		static readonly PixelShader pixelShader = new PixelShader
		{
			UriSource = new Uri("pack://application:,,,/SimpleClocks;component/Resources/effects/GrayscaleEffect.ps")
		};

		public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);

		public static readonly DependencyProperty DesaturationFactorProperty = DependencyProperty.Register(
			"DesaturationFactor",
			typeof(double),
			typeof(GrayscaleEffect),
			new UIPropertyMetadata(0.0, PixelShaderConstantCallback(0), CoerceDesaturationFactor)
		);

		public Brush Input
		{
			get => (Brush)GetValue(InputProperty);
			set => SetValue(InputProperty, value);
		}

		public double DesaturationFactor
		{
			get => (double)(GetValue(DesaturationFactorProperty) ?? 0d);
			set => SetValue(DesaturationFactorProperty, value);
		}

		public GrayscaleEffect()
		{
			PixelShader = pixelShader;
			UpdateShaderValue(InputProperty);
			UpdateShaderValue(DesaturationFactorProperty);
		}

		private static object CoerceDesaturationFactor(DependencyObject sender, object value)
			=> value is double d && !(d < 0d || d > 1d)
				? d
				: ((GrayscaleEffect) sender).DesaturationFactor;
	}
}