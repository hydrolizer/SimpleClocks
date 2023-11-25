using Microsoft.Win32;
using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils;
using System;
using System.Linq;
using System.Windows;
using SimpleClocks.Dialogs;
using SimpleClocks.Models;

namespace SimpleClocks.Services.Classes
{
	class DialogsService : IDialogsService
	{
		readonly IColorProfileManager _colorProfileManager;

		public DialogsService(IColorProfileManager colorProfileManager)
			=> _colorProfileManager = colorProfileManager ?? throw new ArgumentNullException(nameof(colorProfileManager));

		static Window MainWindow => Application.Current.Windows.OfType<MainWindow>().SingleOrDefault();
		static Window TopChild
		{
			get
			{
				var topChild = Application.Current.Windows.Cast<Window>()
					.FirstOrDefault(w =>
						!Equals(w, MainWindow) &&
						w.IsLoaded &&
						w.IsVisible &&
						w.OwnedWindows.Count == 0);
				return topChild ?? MainWindow;
			}
		}

		public void ManageProfiles()
		{
			var plw = new ProfilesListWindow { Owner = MainWindow };
			var model = new ProfilesListModel(this, _colorProfileManager, plw);
			plw.DataContext = model;
			plw.ShowDialog();
		}

		public bool EditProfile(ColorSettings colorSettings)
		{
			return new SettingsWindow(this, colorSettings)
			{
				Owner = TopChild
			}.ShowDialog().GetValueOrDefault(false);
		}

		public string BrowseXmlFile(string initialDirectory, bool forSave)
		{
			var fd = forSave
				? (FileDialog)new SaveFileDialog()
				: new OpenFileDialog();
			fd.Title = "Choose XML file";
			fd.InitialDirectory = string.IsNullOrEmpty(initialDirectory)
				? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
				: initialDirectory;
			fd.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
			if (!fd.ShowDialog(TopChild).GetValueOrDefault()) return null;
			return fd.FileName;
		}

		public bool QueryText(InputBoxParameters inputBoxParameters, out string value)
		{
			var ib = new TextInputBox(this) { Owner = TopChild };
			if (inputBoxParameters!=null)
			{
				if (inputBoxParameters.Caption!=null)
					ib.Caption = inputBoxParameters.Caption;
				if (inputBoxParameters.Prompt != null)
					ib.Prompt = inputBoxParameters.Prompt;
				if (inputBoxParameters.InitialValue != null)
					ib.Value = inputBoxParameters.InitialValue;
				ib.AllowEmptyValue = inputBoxParameters.AllowEmptyValue;
				ib.ValidateAction = inputBoxParameters.ValidateAction;
			}
			var res = ib.ShowDialog().GetValueOrDefault(false);
			value = null;
			if (res)
				value = ib.Value;
			return res;
		}

		public bool QueryText(out string value) => QueryText(null, out value);

		public void ShowError(string text, string caption = "Error")
			=> MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Error);

		public bool Confirm(string text, string caption = "Confirmation")
			=> MessageBox.Show(text, caption, MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK;
	}
}
