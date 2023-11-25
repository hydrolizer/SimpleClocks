using System;

namespace SimpleClocks.Utils
{
	public class SimpleAppException: ApplicationException
	{
		public SimpleAppException(string message)
			:base(message)
		{}
	}
}
