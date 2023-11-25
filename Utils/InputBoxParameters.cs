using System;

namespace SimpleClocks.Utils
{
	public class InputBoxParameters
	{
		public string Caption { get; set; }
		public string Prompt { get; set; }
		public string InitialValue { get; set; }
		public bool AllowEmptyValue { get; set; }
		public Action<string> ValidateAction { get; set; }
	}
}
