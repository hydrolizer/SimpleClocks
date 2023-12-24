using System.Collections.Generic;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Input;
using SimpleClocks.Models;

namespace SimpleClocks.Services.Intefaces
{
	interface IColorProfileManager
	{
		IReadOnlyList<ColorSettings> ProfilesList { get; }
		ListCollectionView Profiles { get; }
		RelayCommand<ColorSettings> ChooseProfileCommand { get; }
		ColorSettings CurrentProfile { get; }
		void ChooseProfile(string profileName);
		void ChooseProfile(ColorSettings colorSettings);
		ColorSettings CreateCopy(string profileName, ColorSettings baseProfile);
		void DeleteProfile(ColorSettings colorSettings);
	}
}