using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using SimpleClocks.Utils;

namespace SimpleClocks
{
	public class ClockModel: INotifyPropertyChanged, IDisposable
	{
		readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
		readonly CancellationToken _token;
		// ReSharper disable once NotAccessedField.Local
		readonly ITargetBlock<DateTimeOffset> _refreshTask;
		readonly Func<Action, CancellationToken, Task> _onRefresh;
		readonly TimeSpan _refreshDelay = TimeSpan.FromMilliseconds(Properties.Settings.Default.RefreshMilliseconds);

		public ClockModel(Func<Action, CancellationToken, Task> onRefresh)
		{
			_token = _tokenSource.Token;
			_onRefresh = onRefresh;
			_refreshTask = NeverEndingTaskFactory.CreateNeverEndingTask(RefreshNow, _refreshDelay, _token);
			OpenSettingsCommand = new RelayCommand(OpenSettings);
			SystemEvents.DisplaySettingsChanged += (s, e) => AdjustPositionToSingleScreen();
		}

		DateTime _now = DateTime.Now;
		public DateTime Now
		{
			get => _now;
			set
			{
				if (value == _now) return;
				_now = value;
				OnPropertyChanged();
			}
		}

		async Task RefreshNow(DateTimeOffset _)
		{
			await _onRefresh(() => Now = DateTime.Now, _token);
		}

		bool _isStarted;
		public void Start()
		{
			if (_isStarted) return;
			_refreshTask.Post(DateTimeOffset.Now);
			_isStarted = true;
		}

		bool _isDisposed;
		public void Dispose()
		{
			if (_isDisposed) return;
			try
			{
				if (_tokenSource.IsCancellationRequested) return;
				_tokenSource.Cancel();
				_tokenSource.Dispose();
				_refreshTask.Completion.Wait(TimeSpan.FromSeconds(5));
				_isDisposed = true;
			}
			catch(AggregateException ae) when(ae.InnerExceptions.All(e=>e is OperationCanceledException))
			{
			}
			catch (OperationCanceledException)
			{
			}
		}

		public RelayCommand OpenSettingsCommand { get; }
		void OpenSettings()
		{
			var res = new SettingsWindow
			{
				Owner = Application.Current.MainWindow
			}.ShowDialog();
			var cs = ColorProfileManager.Instance.CurrentProfile;
			(res.GetValueOrDefault(false) ? (Action)cs.Save : cs.RejectChanges)();
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public static void AdjustPositionToSingleScreen()
		{
			var screens = WpfScreen.AllScreens().ToList();
			var prefs = Properties.Settings.Default;
			if (screens.Count > 1) return;
			if (prefs.X < (screens[0].WorkingArea.Width - prefs.Width)) return;
			prefs.X -= screens[0].WorkingArea.Width;
			prefs.Save();
		}
	}
}
