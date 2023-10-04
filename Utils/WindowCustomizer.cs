using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace SimpleClocks.Utils
{
	public static partial class WindowCustomizer
	{
		#region CanMaximize
		public static readonly DependencyProperty CanMaximize =
				DependencyProperty.RegisterAttached("CanMaximize", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanMaximizeChanged));
		private static void OnCanMaximizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (!(d is Window window)) return;

			void LoadedHandler(object sender, RoutedEventArgs ea)
			{
				if ((bool) e.NewValue)
				{
					WindowHelper.EnableMaximize(window);
				}
				else
				{
					WindowHelper.DisableMaximize(window);
				}
				window.Loaded -= LoadedHandler;
			}

			if (!window.IsLoaded)
			{
				window.Loaded += LoadedHandler;
			}
			else
			{
				LoadedHandler(null, null);
			}
		}
		public static void SetCanMaximize(DependencyObject d, bool value)
		{
			d.SetValue(CanMaximize, value);
		}
		public static bool GetCanMaximize(DependencyObject d)
		{
			return (bool)d.GetValue(CanMaximize);
		}
		#endregion CanMaximize

		#region CanMinimize
		public static readonly DependencyProperty CanMinimize =
				DependencyProperty.RegisterAttached("CanMinimize", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanMinimizeChanged));
		private static void OnCanMinimizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = d as Window;
			if (window == null) return;

			void LoadedHandler(object sender, RoutedEventArgs ea)
			{
				if ((bool) e.NewValue)
				{
					WindowHelper.EnableMinimize(window);
				}
				else
				{
					WindowHelper.DisableMinimize(window);
				}
				window.Loaded -= LoadedHandler;
			}

			if (!window.IsLoaded)
			{
				window.Loaded += LoadedHandler;
			}
			else
			{
				LoadedHandler(null, null);
			}
		}
		public static void SetCanMinimize(DependencyObject d, bool value)
		{
			d.SetValue(CanMinimize, value);
		}
		public static bool GetCanMinimize(DependencyObject d)
		{
			return (bool)d.GetValue(CanMinimize);
		}
		#endregion CanMinimize

		#region CanClose
		public static readonly DependencyProperty CanClose =
				DependencyProperty.RegisterAttached("CanClose", typeof(bool), typeof(WindowCustomizer),
						new PropertyMetadata(true, OnCanCloseChanged));
		private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var window = d as Window;
			if (window == null) return;

			void LoadedHandler(object sender, RoutedEventArgs ea)
			{
				if ((bool) e.NewValue)
				{
					WindowHelper.EnableClose(window);
				}
				else
				{
					WindowHelper.DisableClose(window);
				}
				window.Loaded -= LoadedHandler;
			}

			if (!window.IsLoaded)
			{
				window.Loaded += LoadedHandler;
			}
			else
			{
				LoadedHandler(null, null);
			}
		}
		public static void SetCanClose(DependencyObject d, bool value)
		{
			d.SetValue(CanClose, value);
		}
		public static bool GetCanClose(DependencyObject d)
		{
			return (bool)d.GetValue(CanClose);
		}
		#endregion

		#region WindowHelper Nested Class
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

			/// <summary>
			/// Disables the maximize functionality of a WPF window.
			/// </summary>
			///The WPF window to be modified.
			public static void DisableMaximize(Window window)
			{
				lock (window)
				{
					IntPtr hWnd = new WindowInteropHelper(window).Handle;
					int windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMaximizebox);
				}
			}

			/// <summary>
			/// Disables the minimize functionality of a WPF window.
			/// </summary>
			///The WPF window to be modified.
			public static void DisableMinimize(Window window)
			{
				lock (window)
				{
					IntPtr hWnd = new WindowInteropHelper(window).Handle;
					int windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle & ~WsMinimizebox);
				}
			}

			/// <summary>
			/// Enables the maximize functionality of a WPF window.
			/// </summary>
			///The WPF window to be modified.
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
					IntPtr hWnd = new WindowInteropHelper(window).Handle;
					/*Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
					SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle & ~WS_SYSMENU);*/
					IntPtr hMenu = GetSystemMenu(hWnd, false);
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
					IntPtr hWnd = new WindowInteropHelper(window).Handle;
					/*Int32 windowStyle = GetWindowLongPtr(hWnd, GWL_STYLE);
					SetWindowLongPtr(hWnd, GWL_STYLE, windowStyle | WS_SYSMENU);*/
					IntPtr hMenu = GetSystemMenu(hWnd, false);
					if (hMenu != IntPtr.Zero)
					{
						EnableMenuItem(hMenu, ScClose, MfBycommand & ~MfGrayed);
						window.Closing -= OnDisabledClosing;
					}
				}
			}

			/// <summary>
			/// Enables the minimize functionality of a WPF window.
			/// </summary>
			///The WPF window to be modified.
			public static void EnableMinimize(Window window)
			{
				lock (window)
				{
					IntPtr hWnd = new WindowInteropHelper(window).Handle;
					int windowStyle = GetWindowLongPtr(hWnd, GwlStyle);
					SetWindowLongPtr(hWnd, GwlStyle, windowStyle | WsMinimizebox);
				}
			}

			/// <summary>
			/// Toggles the enabled state of a WPF window's maximize functionality.
			/// </summary>
			///The WPF window to be modified.
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

			/// <summary>
			/// Toggles the enabled state of a WPF window's minimize functionality.
			/// </summary>
			///The WPF window to be modified.
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
		#endregion WindowHelper Nested Class

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
	}
}