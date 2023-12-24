using NLog;
using SimpleClocks.Models;
using SimpleClocks.Services.Intefaces;
using SimpleClocks.Utils;
using SimpleClocks.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;

namespace SimpleClocks.Services.Classes
{

	sealed class ColorProfileManager : INotifyPropertyChanged, IColorProfileManager
	{
		static readonly TextEqualityComparer keysEqualityComparer = new TextEqualityComparer(StringComparison.OrdinalIgnoreCase);
		readonly ObservableCollection<ColorSettings> _profiles = new ObservableCollection<ColorSettings>();
		public ColorProfileManager()
		{
			List<ColorSettings> profiles;
			using (var sf = GetStorageFile())
			{
				log.Debug(sf.GetPath());
				profiles = sf.GetFileNames($"*{ColorSettings.ProfileFileExtension}")
					.Select(f => ColorSettings.FromFile(sf, f))
					.Where(p=>p?.ProfileName != null)
					.GroupBy(p => p.ProfileName, keysEqualityComparer)
					.Where(g => g.Count() == 1)
					.SelectMany(g => g)
					.ToList();
			}
			if (!profiles.Any(p=>ColorSettings.DefaultProfileName.Equals(p.ProfileName)))
				profiles.Add(ColorSettings.Default());
			foreach(var p in profiles)
				_profiles.Add(p);
			Profiles = (ListCollectionView)CollectionViewSource.GetDefaultView(_profiles);
			Profiles.SortDescriptions.Add(new SortDescription(nameof(ColorSettings.IsDefault), ListSortDirection.Descending));
			Profiles.SortDescriptions.Add(new SortDescription(nameof(ColorSettings.ProfileName), ListSortDirection.Ascending));
			ChooseProfileCommand = new RelayCommand<ColorSettings>(ChooseProfile);
			ChooseProfile(Properties.Settings.Default.ColorProfileName);
		}

		public IReadOnlyList<ColorSettings> ProfilesList => _profiles;
		public ListCollectionView Profiles { get; }

		public void ChooseProfile(string profileName)
		{
			if (profileName == null)
				profileName = string.Empty;
			var profile = _profiles.FirstOrDefault(p => profileName.Equals(p.ProfileName, StringComparison.Ordinal));
			if (profile==null)
			{
				profileName = ColorSettings.DefaultProfileName;
				profile = _profiles.FirstOrDefault(p => profileName.Equals(p.ProfileName, StringComparison.Ordinal));
				if (profile==null)
					throw new InvalidOperationException($"Profile with name {profileName} notfound");
			}
			ChooseProfile(profile);
		}

		public RelayCommand<ColorSettings> ChooseProfileCommand { get; }

		public void ChooseProfile(ColorSettings colorSettings)
		{
			foreach (var p in _profiles)
				p.IsActive = false;
			CurrentProfile = colorSettings;
			CurrentProfile.IsActive = true;
			if (!colorSettings.ProfileName.Equals(Properties.Settings.Default.ColorProfileName, StringComparison.Ordinal))
				Properties.Settings.Default.ColorProfileName = colorSettings.ProfileName;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProfile)));
			CurrentProfile.InjectResources();
		}

		public ColorSettings CurrentProfile { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public ColorSettings CreateCopy(string profileName, ColorSettings baseProfile)
		{
			if (string.IsNullOrEmpty(profileName))
				throw new ArgumentNullException(nameof(profileName));
			if (ProfilesList.Any(p => profileName.Equals(p.ProfileName, StringComparison.CurrentCulture)))
				throw new InvalidOperationException("Duplicated prifile name");
			var newProfile = ColorSettings.CreateCopy(profileName, baseProfile);
			newProfile.Save();
			_profiles.Add(newProfile);
			return newProfile;
		}

		public void DeleteProfile(ColorSettings colorSettings)
		{
			if (!_profiles.Remove(colorSettings))
				throw new InvalidOperationException($"Profile {colorSettings.ProfileName} notfound");
			colorSettings.DeleteData();
		}

		static IsolatedStorageFile GetStorageFile() => IsolatedStorageFile.GetUserStoreForDomain();
		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}
