namespace SimpleClocks.Controls
{
	public class HexColorTextBox : AutoWidthTextBox
	{
		public HexColorTextBox()
		{
			MaxTextLength = 9;
			AutoWidthMode = AutoWidthMode.All;
			AutoWidthAdjustMode = AutoWidthAdjustMode.ByMaximum;
		}

		const string TextForMeasure = "#0123456789abcdefABCDEF";
		protected override string GetTextForMeasure() => TextForMeasure;
	}
}
