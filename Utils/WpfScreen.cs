using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace SimpleClocks.Utils
{
	public class WpfScreen
	{
		public static IEnumerable<WpfScreen> AllScreens() => Screen.AllScreens.Select(screen => new WpfScreen(screen));

		public static WpfScreen GetScreenFrom(Window window)
		{
			var windowInteropHelper = new WindowInteropHelper(window);
			var screen = Screen.FromHandle(windowInteropHelper.Handle);
			var wpfScreen = new WpfScreen(screen);
			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(Point point)
		{
			var x = (int)Math.Round(point.X);
			var y = (int)Math.Round(point.Y);

			// are x,y device-independent-pixels ??
			var drawingPoint = new System.Drawing.Point(x, y);
			var screen = Screen.FromPoint(drawingPoint);
			var wpfScreen = new WpfScreen(screen);

			return wpfScreen;
		}

		public static WpfScreen GetScreenFrom(double left, double top)
			=> GetScreenFrom(new Point(left, top));

		public static WpfScreen Primary => new WpfScreen(Screen.PrimaryScreen);

		private readonly Screen _screen;

		internal WpfScreen(Screen screen) => _screen = screen;

		public Rect DeviceBounds => GetRect(_screen.Bounds);

		public Rect WorkingArea => GetRect(_screen.WorkingArea);

		static Rect GetRect(Rectangle value)
		=>
			new Rect
			{
				X = value.X,
				Y = value.Y,
				Width = value.Width,
				Height = value.Height
			};

		public bool IsPrimary => _screen.Primary;

		public string DeviceName => _screen.DeviceName;
	}
}
