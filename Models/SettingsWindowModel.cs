using NLog;
using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;

namespace SimpleClocks.Models
{
	sealed class SettingsWindowModel : INotifyPropertyChanged
	{
		readonly IDialogsService _dialogsService;
		public ColorSettings ColorSettings { get; }

		public SettingsWindowModel(IDialogsService dialogsService, ColorSettings colorSettings)
		{
			_dialogsService = dialogsService ?? throw new ArgumentNullException(nameof(dialogsService));
			ColorSettings = colorSettings ?? throw new ArgumentNullException(nameof(colorSettings));
			CurrentTarget = ColorSettings.Colors.FirstOrDefault();
			ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
			ExportToFileCommand = new RelayCommand(Export);
			ImportFromFileCommand = new RelayCommand(Import);
		}

		public RelayCommand ResetToDefaultsCommand { get; }
		public RelayCommand ExportToFileCommand { get; }
		public RelayCommand ImportFromFileCommand { get; }

		void ResetToDefaults() => ColorSettings.ResetToDefaults();

		public ColorContainer CurrentTarget { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		void Export() => ExportImport(true);
		void Import() => ExportImport(false);
		void ExportImport(bool export)
		{
			var path = BrowseXmlFile(export);
			if (path == null) return;
			try
			{
				var currentTarget = CurrentTarget;
				if (!export)
					CurrentTarget = null;
				(export
					? (Action<string>)ColorSettings.ExportToFile
					: ColorSettings.ImportFromFile
				)(path);
				CurrentTarget = currentTarget;
			}
			catch (Exception e)
			{
				log.Error(e.Format(ErrorFormat.Verbose));
				MessageBox.Show($"{e.GetType()}: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		static readonly LastFolder lastFolder = new LastFolder();
		string BrowseXmlFile(bool forSave)
		{
			var filePath = _dialogsService.BrowseXmlFile(lastFolder.Path, forSave);
			if (filePath == null) return null;
			lastFolder.Path = Path.GetDirectoryName(filePath);
			return filePath;
		}

		class LastFolder
		{
			string _lastFolder;
			static bool FolderPathValid(string path)
				=> !string.IsNullOrEmpty(path) && Directory.Exists(path);
			public string Path
			{
				get => FolderPathValid(_lastFolder) ? _lastFolder : null;
				set
				{
					if (FolderPathValid(value))
						_lastFolder = value;
				}
			}
		}

		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}
