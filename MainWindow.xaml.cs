using SimpleClocks.Models;
using SimpleClocks.Services;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using SystemCommands = Microsoft.Windows.Shell.SystemCommands;

namespace SimpleClocks
{
	public partial class MainWindow
	{
		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		readonly ClockModel _model;
		public MainWindow()
		{
			InitializeComponent();
			_model = ServicesManager.ClockModel(RefreshNowAsync);
			DataContext = _model;
			Loaded += MainWindowLoaded;
			Closing += MainWindowClosing;
		}

		private void MainWindowLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Loaded -= MainWindowLoaded;
			_model.Start();
		}

		void MainWindowClosing(object sender, CancelEventArgs e)
		{
			Properties.Settings.Default.Save();
			_model.Dispose();
		}

		async Task RefreshNowAsync(Action action, CancellationToken token)
		{
			try
			{
				await Dispatcher.InvokeAsync(action, DispatcherPriority.ContextIdle, token);
			}
			catch (OperationCanceledException)
			{
			}
		}
		
		void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}
	}
}
