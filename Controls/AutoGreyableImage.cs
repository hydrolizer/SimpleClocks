using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SimpleClocks.Controls
{
	public class AutoGreyableImage : Image
	{
		static AutoGreyableImage()
		{
			IsEnabledProperty.OverrideMetadata(typeof(AutoGreyableImage),
				new FrameworkPropertyMetadata(true, OnAutoGreyScaleImageIsEnabledPropertyChanged));
		}

		private static void OnAutoGreyScaleImageIsEnabledPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs args)
		{
			var image = source as AutoGreyableImage;
			if (image == null || image.Source == null) return;
			var isEnable = Convert.ToBoolean(args.NewValue);
			if (!isEnable)
			{
				var bitmapImage = new BitmapImage(new Uri(image.Source.ToString()));
				image.Source = new FormatConvertedBitmap(bitmapImage, PixelFormats.Gray32Float, null, 0);
				image.OpacityMask = new ImageBrush(bitmapImage);
			}
			else
			{
				var imgSrc = image.Source as FormatConvertedBitmap;
				if (imgSrc != null)
					image.Source = imgSrc.Source;
				image.OpacityMask = null;
			}
		}
	}
}
