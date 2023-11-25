using FirstFloor.ModernUI.Presentation;
using System;
using System.Windows;
using NLog;
using SimpleClocks.Utils;
using SimpleClocks.Utils.Extensions;

namespace SimpleClocks
{
	public partial class App
	{
		// ReSharper disable once NotAccessedField.Local
		IAppCatcher _appCatcher;
		protected override void OnStartup(StartupEventArgs e)
		{
			_appCatcher = new AppCatcher(this, OnError) { JoinConsole = false };
			base.OnStartup(e);
			AppearanceManager.Current.ThemeSource = new Uri("/FirstFloor.ModernUI;component/Assets/ModernUI.Dark.xaml", UriKind.RelativeOrAbsolute);
		}

		static void OnError(Exception ex, string source)
		{
			try
			{
				MessageBoxUtil.ShowError(ex.Format(ErrorFormat.WithType), "Fatal error occured");
				Current.Shutdown(0);
				Environment.Exit(0);
			}
			catch (Exception e)
			{
				try
				{
					log.Error(e.Format(ErrorFormat.Verbose));
				}
				catch(Exception ex2)
				{
					Environment.FailFast("Fatal exception#3", ex2);
				}
			}
		}

		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}
