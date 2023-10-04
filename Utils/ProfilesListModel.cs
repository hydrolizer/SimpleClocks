using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;

namespace SimpleClocks.Utils
{
	sealed class ProfilesListModel : INotifyPropertyChanged
	{
		readonly IClosableView _owner;
		readonly IReadOnlyList<ColorSettings> _profiles;
		public ProfilesListModel(IReadOnlyList<ColorSettings> profiles, IClosableView owner)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_profiles = profiles;
			Profiles = (ListCollectionView)CollectionViewSource.GetDefaultView(_profiles);
			Profiles.SortDescriptions.Add(new SortDescription(nameof(ColorSettings.IsDefault), ListSortDirection.Descending));
			Profiles.SortDescriptions.Add(new SortDescription(nameof(ColorSettings.ProfileName), ListSortDirection.Ascending));
			SelectedProfile = _profiles.FirstOrDefault();
			OverwriteSelectedCommand = new RelayCommand(OverwriteSelected);
			SaveAsNewCommand = new RelayCommand(SaveAsNew);
		}

		public ListCollectionView Profiles { get; }

		ColorSettings _selectedProfile;
		public ColorSettings SelectedProfile
		{
			get => _selectedProfile;
			set
			{
				_selectedProfile = value;
				OnPropertyChanged();
			}
		}

		string _targetProfileName;
		public string TargetProfileName
		{
			get => _targetProfileName;
			set
			{
				_targetProfileName = value;
				OnPropertyChanged();
			}
		}

		public string ChoosedProfile { get; private set; }
		public RelayCommand OverwriteSelectedCommand { get; }
		public RelayCommand SaveAsNewCommand { get; }

		void OverwriteSelected()
		{
			if (SelectedProfile==null)
			{
				MessageBoxUtil.ShowError("No profile selected.");
				return;
			}
			ChoosedProfile = SelectedProfile.ProfileName;
			_owner.Close(true);
		}

		void SaveAsNew()
		{
			if (string.IsNullOrEmpty(TargetProfileName))
			{
				MessageBoxUtil.ShowError("Please enter name of new profile.");
				return;
			}
			if (
				_profiles.Any(p => TargetProfileName.Equals(p.ProfileName, StringComparison.Ordinal)) &&
				!MessageBoxUtil.ShowConfirm("The name of the new profile matches with the name of one of the existing ones. It will be overwritten. Continue?")
			)
				return;
			ChoosedProfile = TargetProfileName;
			_owner.Close(true);
		}

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
