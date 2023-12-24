using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;

namespace SimpleClocks.Models
{
	sealed class ProfilesListModel : INotifyPropertyChanged
	{
		readonly IColorizedView _owner;
		readonly IDialogsService _dialogService;
		readonly IColorProfileManager _colorProfileManager;

		public ProfilesListModel(
			IDialogsService dialogsService,
			IColorProfileManager colorProfileManager,
			IColorizedView owner
		)
		{
			_dialogService = dialogsService ?? throw new ArgumentNullException(nameof(dialogsService));
			_colorProfileManager = colorProfileManager ?? throw new ArgumentNullException(nameof(colorProfileManager));
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			Profiles = _colorProfileManager.Profiles;
			var l = Profiles.Cast<ColorSettings>().ToList();
			SelectedProfile = l.SingleOrDefault(cs=>cs.IsActive) ?? l.First();
			CreateCopyCommand = new RelayCommand(CreateCopy);
			EditProfileCommand = new RelayCommand(EditProfile);
			ActivateProfileCommand = new RelayCommand(ActivateProfile);
			DeleteProfileCommand = new RelayCommand(DeleteProfile);
		}

		public ListCollectionView Profiles { get; }

		public ColorSettings SelectedProfile { get; set; }

		void OnSelectedProfileChanged()
		{
			if (SelectedProfile!=null)
				_owner.ApplyColorProfile(SelectedProfile);
		}

		void CheckNewProfileName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				throw new SimpleAppException("New color profile name cannot be empty.");
			if (Profiles.Cast<ColorSettings>().Any(p => p.ProfileName.Equals(value, StringComparison.CurrentCulture)))
				throw new SimpleAppException("The name of the new color profile must be unique.");
		}

		bool EnsureSelectedProfile()
		{
			if (SelectedProfile != null)
				return true;
			_dialogService.ShowError("Choose any color profile.");
			return false;
		}

		public RelayCommand CreateCopyCommand { get; }
		void CreateCopy()
		{
			if (!EnsureSelectedProfile()) return;
			if (!_dialogService.QueryText(
				new InputBoxParameters
				{
					Caption = "Enter a name for the new color profile",
					Prompt = "Profile name:",
					ValidateAction = CheckNewProfileName
				},
				out var name))
				return;
			SelectedProfile = _colorProfileManager.CreateCopy(name, SelectedProfile);
		}

		public RelayCommand EditProfileCommand { get; }
		void EditProfile()
		{
			if (!EnsureSelectedProfile()) return;
			if (!_dialogService.EditProfile(SelectedProfile))
			{
				SelectedProfile.RejectChanges();
				return;
			}
			SelectedProfile.Save();
			_owner.ApplyColorProfile(SelectedProfile);
		}

		public RelayCommand ActivateProfileCommand { get; }
		void ActivateProfile()
		{
			if (!EnsureSelectedProfile()) return;
			_colorProfileManager.ChooseProfile(SelectedProfile);
		}

		public RelayCommand DeleteProfileCommand { get; }
		void DeleteProfile()
		{
			if (!EnsureSelectedProfile()) return;
			if (SelectedProfile.IsActive)
			{
				_dialogService.ShowError("Unable to delete active profile.");
				return;
			}
			if (SelectedProfile.IsDefault)
			{
				_dialogService.ShowError("Unable to delete default profile.");
				return;
			}
			if (!_dialogService.Confirm("Are you sure you want to delete the selected profile?"))
				return;
			_colorProfileManager.DeleteProfile(SelectedProfile);
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
