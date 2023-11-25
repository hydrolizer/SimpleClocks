using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using GalaSoft.MvvmLight.CommandWpf;
using PropertyChanged;
using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils;

namespace SimpleClocks.Dialogs
{
	public sealed partial class TextInputBox : INotifyPropertyChanged
	{
		readonly IDialogsService _dialogsService;
		internal TextInputBox(IDialogsService dialogsService)
		{
			InitializeComponent();
			_dialogsService = dialogsService ?? throw new ArgumentNullException(nameof(dialogsService));
			OkCommand = new RelayCommand(DoOk);
			DataContext = this;
			Loaded += TextInputBoxLoaded;
		}

		async void TextInputBoxLoaded(object sender, System.Windows.RoutedEventArgs e)
		{
			Loaded -= TextInputBoxLoaded;
			await Dispatcher.Yield(DispatcherPriority.ContextIdle);
			ValueTextBox.Focus();
		}

		public string Caption { get; set; } = "Enter value";
		public string Prompt { get; set; } = "Value:";
		public string Value { get; set; } = string.Empty;

		[DoNotNotify]
		public bool AllowEmptyValue { get; set; }
		[DoNotNotify]
		public Action<string> ValidateAction { get; set; }

		public RelayCommand OkCommand { get; }

		void DoOk()
		{
			if (!AllowEmptyValue && string.IsNullOrEmpty(Value))
			{
				_dialogsService.ShowError("Value cannot be empty.");
				return;
			}
			try
			{
				ValidateAction?.Invoke(Value);
			}
			catch (SimpleAppException exception)
			{
				_dialogsService.ShowError(exception.Message);
				return;
			}
			DialogResult = true;
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
