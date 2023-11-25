using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using NLog;

namespace SimpleClocks.Utils.Attached
{

	public static class WindowCustomizer
	{
		static void SetState(this Window window, Action<Window> trueAction, Action<Window> falseAction, Func<bool> condition)
		{
			void LoadedHandler(object sender, RoutedEventArgs ea)
			{
				window.Loaded -= LoadedHandler;
				(condition() ? trueAction : falseAction)(window);
			}
			if (!window.IsLoaded)
				window.Loaded += LoadedHandler;
			else
				LoadedHandler(null, null);
		}

		static void SetState(this DependencyObject d, DependencyPropertyChangedEventArgs ea, Action<Window> trueAction, Action<Window> falseAction)
			=> (d as Window)?.SetState(trueAction, falseAction, () => ea.NewValue is bool b && b);

		public static readonly DependencyProperty CanMaximize =
				DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanMaximizeChanged));

		private static void OnCanMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> d.SetState(e, WindowHelper.EnableMaximize, WindowHelper.DisableMaximize);

		public static void SetCanMaximize(DependencyObject d, bool value) => d.SetValue(CanMaximize, value);
		public static bool GetCanMaximize(DependencyObject d) => (bool)d.GetValue(CanMaximize);

		public static readonly DependencyProperty CanMinimize =
				DependencyProperty.RegisterAttached("CanMinimize", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanMinimizeChanged));
		private static void OnCanMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> d.SetState(e, WindowHelper.EnableMinimize, WindowHelper.DisableMinimize);

		public static void SetCanMinimize(DependencyObject d, bool value) => d.SetValue(CanMinimize, value);
		public static bool GetCanMinimize(DependencyObject d) => (bool)d.GetValue(CanMinimize);

		public static readonly DependencyProperty CanClose =
				DependencyProperty.RegisterAttached("CanClose", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanCloseChanged));
		private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
			=> d.SetState(e, WindowHelper.EnableClose, WindowHelper.DisableClose);

		public static void SetCanClose(DependencyObject d, bool value) => d.SetValue(CanClose, value);
		public static bool GetCanClose(DependencyObject d)=>(bool)d.GetValue(CanClose);

		public static readonly DependencyProperty TranslateMouseClicksProperty = DependencyProperty.RegisterAttached(
			"TranslateMouseClicks", typeof(bool), typeof(WindowCustomizer),
			new PropertyMetadata(false, OnTranslateMouseClicksPropertyChanged)
		);

		public static bool GetTranslateMouseClicks(DependencyObject o)
			=> (bool)(o.GetValue(TranslateMouseClicksProperty) ?? false);
		public static void SetTranslateMouseClicks(DependencyObject o, bool value)
			=> o.SetValue(TranslateMouseClicksProperty, value);

		static void OnTranslateMouseClicksPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			void p(Window w) => WindowHelper.SetTranslateMouseClicks(w, e.NewValue is bool b && b);
			d.SetState(e, p, p);
		}

		public static class WindowHelper
		{
			private const int GwlStyle = -16;
			private const int WsMaximizebox = 0x00010000;
			private const int WsMinimizebox = 0x00020000;

			private const uint MfBycommand = 0x00000000;
			private const uint MfGrayed = 0x00000001;

			private const uint ScClose = 0xF060;
			
			[DllImport("user32.dll")]
			static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
			
			[DllImport("user32.dll")]
			static extern bool EnableMenuItem(IntPtr hMenu, uint uIdEnableItem, uint uEnable);
			
			[DllImport("User32.dll", EntryPoint = "GetWindowLong")]
			static extern int GetWindowLongPtr(IntPtr hWnd, int nIndex);

			[DllImport("User32.dll", EntryPoint = "SetWindowLong")]
			static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

			// ReSharper disable InconsistentNaming
			const int WS_EX_TRANSPARENT = 0x00000020;
			const int GWL_EXSTYLE = (-20);
			// ReSharper restore InconsistentNaming

			[DllImport("user32.dll")]
			static extern int GetWindowLong(IntPtr hwnd, int index);

			[DllImport("user32.dll", SetLastError = true)]
			static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

			public static void SetTranslateMouseClicks(Window window, bool translate)
			{
				if (window == null)
					throw new ArgumentNullException(nameof(window));
				var hwnd = new WindowInteropHelper(window).EnsureHandle();
				if (hwnd == IntPtr.Zero) return;
				var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
				if (extendedStyle == 0) return;
				if (translate)
					extendedStyle |= WS_EX_TRANSPARENT;
				else if ((extendedStyle & WS_EX_TRANSPARENT)==WS_EX_TRANSPARENT)
					extendedStyle ^= WS_EX_TRANSPARENT;
				var result = SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle);
				if (result>0) return;
				log.Error(Win32Error.GetErrorMessage());
			}

			public static void DisableMaximize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMaximizebox);
				}
			}

			public static void DisableMinimize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMinimizebox);
				}
			}

			public static void EnableMaximize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle | WsMaximizebox);
				}
			}

			public static void DisableClose(Window window)
			{
				lock(window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					/*Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
					SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_SYSMENU);*/
					var hMenu = GetSystemMenu(hWnd, false);
					if (hMenu != IntPtr.Zero)
					{
						EnableMenuItem(hMenu, ScClose, MfBycommand | MfGrayed);
						window.Closing += OnDisabledClosing;
					}
				}
			}

			public static void OnDisabledClosing(object sender, CancelEventArgs e)
			{
				e.Cancel = true;
			}

			public static void EnableClose(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					/*Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
					SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_SYSMENU);*/
					var hMenu = GetSystemMenu(hWnd, false);
					if (hMenu != IntPtr.Zero)
					{
						EnableMenuItem(hMenu, ScClose, MfBycommand & ~MfGrayed);
						window.Closing -= OnDisabledClosing;
					}
				}
			}

			public static void EnableMinimize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle | WsMinimizebox);
				}
			}

			public static void ToggleMaximize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);

					if ((windowStyle | WsMaximizebox) == windowStyle)
					{
						SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMaximizebox);
					}
					else
					{
						SetWindowLongPtr(hWnd, GwlStyle, windowStyle | WsMaximizebox);
					}
				}
			}

			public static void ToggleMinimize(Window window)
			{
				lock (window)
				{
					var hWnd = new WindowInteropHelper(window).Handle;
					var windowStyle = GetWindowLongPtr(hWnd, GwlStyle);

					if ((windowStyle | WsMinimizebox) == windowStyle)
					{
						SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMinimizebox);
					}
					else
					{
						SetWindowLongPtr(hWnd, GwlStyle, windowStyle | WsMinimizebox);
					}
				}
			}
		}

		public static bool IsModal(Window window)
			=>
				window.IsVisible
					? typeof(Window)
						  .GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic)
						  ?.GetValue(window) is bool b && b
					: window.WindowStartupLocation == WindowStartupLocation.CenterOwner;

		const double AdjustSizeDelta = 20;
		public static void SetWindowState(this Window window, Rectangle windowArea, WindowState windowState, bool restorePosition = false)
		{
			if (window==null)
				throw new ArgumentNullException(nameof(window));

			var screenSize = GetScreenSize();

			double AdjustSize(double windowValue, double screenValue) =>
				windowValue - AdjustSizeDelta > screenValue
					? screenValue - AdjustSizeDelta
					: windowValue;
			void ApplyPartialState()
			{
				window.Width = AdjustSize(windowArea.Width, screenSize.Width);
				window.Height = AdjustSize(windowArea.Height, windowArea.Height);
				window.WindowState = windowState;
			}

			if (IsModal(window) && !restorePosition)
			{
				ApplyPartialState();
				return;
			}

			if (windowState==WindowState.Maximized)
			{
				window.Top = windowArea.Top < 0 ? 0 : windowArea.Top;
				window.Left = windowArea.Left < 0 ? 0 : windowArea.Left;
				window.Width = windowArea.Width;
				window.Height = windowArea.Height;
				window.SetMaximized();
				return;
			}

			if ((windowArea.Left + AdjustSizeDelta) >= screenSize.Width)
			{
				if (Math.Abs((int)window.Left-windowArea.Left)<=AdjustSizeDelta)
					window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
				ApplyPartialState();
				return;
			}

			window.WindowStartupLocation = WindowStartupLocation.Manual;
			window.Top = windowArea.Top < 0 ? 0 : windowArea.Top;
			window.Left = windowArea.Left < 0 ? 0 : windowArea.Left;
			ApplyPartialState();
		}

		static void SetMaximized(this Window window)
		{
			if (!window.IsLoaded)
			{
				window.WindowState = WindowState.Normal;
				window.WindowStartupLocation = WindowStartupLocation.Manual;
			}
			var screen = WpfScreen.GetScreenFrom(window.Left, window.Top);
			var workingArea = screen.WorkingArea;
			window.Left = workingArea.Left;
			window.Top = workingArea.Top;
			window.Width = workingArea.Width;
			window.Height = workingArea.Height;
			Console.WriteLine("Placing at {0} screen in ({1}.{2}) with size {3}x{4}",
				screen.IsPrimary ? "primary" : "secondary",
				window.Left, window.Top,
				window.Width, window.Height
			);
			// If window isn't loaded then maxmizing will result in the window displaying on the primary monitor
			if (window.IsLoaded)
			{
				window.WindowState = WindowState.Maximized;
				return;
			}
			void LoadedHandler(object sender, RoutedEventArgs ea)
			{
				window.WindowState = WindowState.Maximized;
				window.Loaded -= LoadedHandler;
			}
			window.Loaded += LoadedHandler;
		}

		public static System.Windows.Size GetScreenSize()
/*			#if DEBUG
			=> new System.Windows.Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
			#else*/
			=> new System.Windows.Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
			//#endif
		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}