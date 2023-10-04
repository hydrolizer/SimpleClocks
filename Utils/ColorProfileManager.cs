using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using GalaSoft.MvvmLight.Command;
using WX.Core.Extensions;
using WX.Core.Utils;

namespace SimpleClocks.Utils
{

	sealed class ColorProfileManager : INotifyPropertyChanged
	{
		static readonly Lazy<ColorProfileManager> factory = new Lazy<ColorProfileManager>(() => new ColorProfileManager());
		public static ColorProfileManager Instance => factory.Value;

		static readonly TextEqualityComparer keysEqualityComparer = new TextEqualityComparer(StringComparison.OrdinalIgnoreCase);
		readonly ObservableCollection<ColorSettings> _profiles = new ObservableCollection<ColorSettings>();
		ColorProfileManager()
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

		void ChooseProfile(ColorSettings colorSettings)
		{
			foreach (var p in _profiles)
				p.IsActive = false;
			CurrentProfile = colorSettings;
			CurrentProfile.IsActive = true;
			if (!colorSettings.ProfileName.Equals(Properties.Settings.Default.ColorProfileName, StringComparison.Ordinal))
				Properties.Settings.Default.ColorProfileName = colorSettings.ProfileName;
			OnPropertyChanged(nameof(CurrentProfile));
		}

		public ColorSettings CurrentProfile { get; private set; }

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void SaveProfile(string profileName)
		{
			if (string.IsNullOrEmpty(profileName)) return;
			var target = ProfilesList.FirstOrDefault(p => profileName.Equals(p.ProfileName, StringComparison.Ordinal));
			if (target!=null)
			{
				CurrentProfile.CopyTo(target);
				target.Save();
				return;
			}
			_profiles.Add(CurrentProfile.SaveAs(profileName));
			CurrentProfile.RejectChanges();
		}
		static IsolatedStorageFile GetStorageFile() => IsolatedStorageFile.GetUserStoreForDomain();
		static readonly Logger log = LogManager.GetCurrentClassLogger();
	}
}
