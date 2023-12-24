using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using CommunityToolkit.Mvvm.Input;

namespace SimpleClocks.Models
{
	class ClockModel: INotifyPropertyChanged, IDisposable
	{
		readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
		readonly CancellationToken _token;
		// ReSharper disable once NotAccessedField.Local
		readonly ITargetBlock<DateTimeOffset> _refreshTask;
		readonly AsyncRefreshTimeHandler _onRefresh;
		readonly TimeSpan _refreshDelay = TimeSpan.FromMilliseconds(Properties.Settings.Default.RefreshMilliseconds);
		readonly IDialogsService _dialogsService;

		public ClockModel(AsyncRefreshTimeHandler onRefresh, IDialogsService dialogsService, IColorProfileManager colorProfileManager)
		{
			_dialogsService = dialogsService ?? throw new ArgumentNullException(nameof(dialogsService));
			ColorProfileManager = colorProfileManager ?? throw new ArgumentNullException(nameof(colorProfileManager));
			_token = _tokenSource.Token;
			_onRefresh = onRefresh;
			_refreshTask = NeverEndingTaskFactory.CreateNeverEndingTask(RefreshNow, _refreshDelay, _token);
			OpenSettingsCommand = new RelayCommand(EditCurrentProfile);
			ManageProfilesCommand = new RelayCommand(_dialogsService.ManageProfiles);
		}

		public IColorProfileManager ColorProfileManager { get; }

		public DateTime Now { get; set; } = DateTime.Now;

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
		void EditCurrentProfile()
		{
			var cs = ColorProfileManager.CurrentProfile;
			if (_dialogsService.EditProfile(cs))
			{
				cs.Save();
				cs.InjectResources();
				return;
			}
			cs.RejectChanges();
		}
		public RelayCommand ManageProfilesCommand { get; }

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
