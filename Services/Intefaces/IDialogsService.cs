using SimpleClocks.Models;
using SimpleClocks.Utils;

namespace SimpleClocks.Services.Intefaces
{
	interface IDialogsService
	{
		void ShowError(string text, string caption = "Error");
		bool Confirm(string text, string caption = "Confirmation");
		void ManageProfiles();
		bool EditProfile(ColorSettings colorSettings);
		string BrowseXmlFile(string initialDirectory, bool forSave);
		bool QueryText(InputBoxParameters inputBoxParameters, out string value);
		bool QueryText(out string value);
	}
}
