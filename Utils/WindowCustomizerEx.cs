using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using WX.Core.Utils.WinAPI;

namespace SimpleClocks.Utils
{
	public static partial class WindowCustomizer
	{
		public static readonly DependencyProperty TranslateMouseClicksProperty = DependencyProperty.RegisterAttached(
			"TranslateMouseClicks", typeof(bool), typeof(WindowCustomizer),
			new PropertyMetadata(false, OnTranslateMouseClicksPropertyChanged)
		);

		public static bool GetTranslateMouseClicks(DependencyObject o)
			=> (bool)(o.GetValue(TranslateMouseClicksProperty) ?? false);
		public static void SetTranslateMouseClicks(DependencyObject o, bool value)
			=> o.SetValue(TranslateMouseClicksProperty, value);

		static void OnTranslateMouseClicksPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs ea)
		{
			if (!(
				o is Window w &&
				ea.OldValue is bool old &&
				ea.NewValue is bool @new &&
				old!=@new
			))
				return;
			TranslateMouseClicks(w, @new);
		}

		public static void TranslateMouseClicks(Window window, bool translate)
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
			Console.WriteLine(Defs.GetErrorMessage(Marshal.GetLastWin32Error()));
		}

		// ReSharper disable InconsistentNaming
		const int WS_EX_TRANSPARENT = 0x00000020;
		const int GWL_EXSTYLE = (-20);
		// ReSharper restore InconsistentNaming

		[DllImport("user32.dll")]
		static extern int GetWindowLong(IntPtr hwnd, int index);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
	}
}
