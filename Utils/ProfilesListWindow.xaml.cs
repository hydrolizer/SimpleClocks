namespace SimpleClocks.Utils
{
	public partial class ProfilesListWindow : IClosableView
	{
		internal ProfilesListWindow()
		{
			InitializeComponent();
		}

		public void Close(bool? dialogResult)
		{
			DialogResult = dialogResult.GetValueOrDefault();
		}
	}
}
