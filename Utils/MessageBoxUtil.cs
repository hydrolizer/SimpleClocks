using System.Windows;

namespace SimpleClocks.Utils
{
	static class MessageBoxUtil
	{
		public const string ErrorCaption = "Error";
		public const string ConfirmCaption = "Are you sure?";
		public static void ShowError(string text, string caption=ErrorCaption)
			=>MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Error);
		public static bool ShowConfirm(string text, string caption=ConfirmCaption)
			=>MessageBox.Show(text, caption, MessageBoxButton.OKCancel, MessageBoxImage.Warning)==MessageBoxResult.OK;
	}
}
