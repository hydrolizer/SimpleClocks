using SimpleClocks.Models;
using SimpleClocks.Utils;

namespace SimpleClocks.Dialogs
{
	internal partial class ProfilesListWindow : IColorizedView
	{
		internal ProfilesListWindow()
		{
			InitializeComponent();
		}


		public void ApplyColorProfile(ColorSettings colorSettings)
		{
			colorSettings.InjectResources(MainWindowPreview);
		}
	}
}
