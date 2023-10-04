using System;
using SimpleClocks.Utils;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using NLog;
using WX.Core.Extensions;

namespace SimpleClocks
{
	public partial class SettingsWindow : INotifyPropertyChanged
	{
		readonly ColorSettings _colorSettings;
		public SettingsWindow()
		{
			InitializeComponent();
			_colorSettings = ColorProfileManager.Instance.CurrentProfile;
			Title = $"Color preferences for {_colorSettings.ProfileName}";
			CurrentTarget = _colorSettings.Colors.FirstOrDefault();
			ResetToDefaultsCommand = new RelayCommand(ResetToDefaults);
			ExportToFileCommand = new RelayCommand(Export);
			ImportFromFileCommand = new RelayCommand(Import);
			SaveAsProfileCommand = new RelayCommand(SaveAsProfile);
			DataContext = this;
		}

		public RelayCommand ResetToDefaultsCommand { get; }
		public RelayCommand ExportToFileCommand { get; }
		public RelayCommand ImportFromFileCommand { get; }
		public RelayCommand SaveAsProfileCommand { get; }

		void ResetToDefaults() => ColorProfileManager.Instance.CurrentProfile.ResetToDefaults();

		ColorContainer _currentTarget;
		public ColorContainer CurrentTarget
		{
			get => _currentTarget;
			set
			{
				if (value?.Equals( _currentTarget) ?? false) return;
				_currentTarget = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		void Export() => ExportImport(true);
		void Import() => ExportImport(false);
		void ExportImport(bool export)
		{
			var path = BrowseXmlFile(export);
			if (path == null) return;
			try
			{
				var currentTarget = _currentTarget;
				if (!export)
					CurrentTarget = null;
				(export
					? (Action<string>)_colorSettings.ExportToFile
					: _colorSettings.ImportFromFile
				)(path);
				CurrentTarget = currentTarget;
			}
			catch (Exception e)
			{
				log.Error(e.Format(ErrorFormat.Verbose));
				MessageBox.Show($"{e.GetType()}: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}


		string _lastFolder;
		static bool FolderPathValid(string path)
			=> !string.IsNullOrEmpty(path) && Directory.Exists(path);
		string BrowseXmlFile(bool forSave)
		{
			var fd = forSave
				? (FileDialog)new SaveFileDialog()
				: new OpenFileDialog();
			fd.Title = "Choose XML file";
			fd.InitialDirectory = FolderPathValid(_lastFolder)
				? _lastFolder
				: Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			fd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
			if (!fd.ShowDialog(this).GetValueOrDefault()) return null;
			var currentFolder = Path.GetDirectoryName(fd.FileName);
			if (FolderPathValid(currentFolder))
				_lastFolder = currentFolder;
			return fd.FileName;
		}

		void SaveAsProfile()
		{
			var plw = new ProfilesListWindow { Owner = this };
			var model = new ProfilesListModel(ColorProfileManager.Instance.ProfilesList, plw);
			plw.DataContext = model;
			if (!plw.ShowDialog().GetValueOrDefault() || string.IsNullOrEmpty(model.TargetProfileName)) return;
			ColorProfileManager.Instance.SaveProfile(model.TargetProfileName);
			DialogResult = false;
		}

		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}

	public class ComboBoxX: ComboBox
	{
		readonly IValueConverter _fontStyleConverter = new FontStyleConverter();
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			if (!(GetTemplateChild("contentPresenter") is ContentPresenter cp)) return;
			var binding = new Binding
			{
				Path = new PropertyPath(nameof(ColorContainer.IsChanged)),
				Mode = BindingMode.OneWay,
				Converter = _fontStyleConverter
			};
			BindingOperations.SetBinding(cp, TextElement.FontStyleProperty, binding);
		}

		class FontStyleConverter: IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				if (!(value is bool b))
					return DependencyProperty.UnsetValue;
				return b ? FontStyles.Italic : FontStyles.Normal;
			}
			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
				=> throw new NotImplementedException();
		}
	}
}
