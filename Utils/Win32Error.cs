using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using SimpleClocks.Utils.Extensions;

namespace SimpleClocks.Utils
{
	static class Win32Error
	{
		static readonly Lazy<Func<int, string>> lazyGetErrorMessage
			= new Lazy<Func<int, string>>(() => ExpressionEx.StaticMethodInvoke<Win32Exception, Func<int, string>>("GetErrorMessage"));
		public static string GetErrorMessage(int win32ErrorCode) => lazyGetErrorMessage.Value(win32ErrorCode);
		public static string GetErrorMessage() => GetErrorMessage(Marshal.GetLastWin32Error());
	}
}
