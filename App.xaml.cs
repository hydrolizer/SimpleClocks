using FirstFloor.ModernUI.Presentation;
using System;
using System.Windows;

namespace SimpleClocks
{
	public partial class App
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			ClockModel.AdjustPositionToSingleScreen();
			base.OnStartup(e);
			AppearanceManager.Current.ThemeSource = new Uri("/FirstFloor.ModernUI;component/Assets/ModernUI.Dark.xaml", UriKind.RelativeOrAbsolute);
		}
	}
}
