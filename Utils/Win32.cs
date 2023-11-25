using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleClocks.Utils
{
	public class Win32
	{
		public const uint MF_BYCOMMAND = 0x00000000;
		public const uint MF_BYPOSITION = 0x0400;

		const uint MF_ENABLED = 0x0;
		const uint MF_GRAYED = 0x00000001;
		const int MF_DISABLED = 0x0002;

		private const uint SC_CLOSE = 0xF060;

		[DllImport("kernel32.dll",
			EntryPoint = "AllocConsole",
			SetLastError = true,
			CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall)]
		public static extern int AllocConsole();

		[DllImport("kernel32.dll",
			EntryPoint = "AttachConsole",
			SetLastError = true,
			CharSet = CharSet.Auto,
			CallingConvention = CallingConvention.StdCall)]
		public static extern bool AttachConsole(int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
		public static extern bool FreeConsole();

		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr GetConsoleWindow();
		
		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll")]
		static extern bool EnableMenuItem(IntPtr hMenu, uint uIdEnableItem, uint uEnable);

		[DllImport("user32.dll", EntryPoint = "RemoveMenu")]
		public static extern int RemoveMenu(IntPtr hmenu, int npos, uint wflags);

		[DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
		public static extern int GetMenuItemCount(IntPtr hmenu);

		[DllImport("user32.dll", EntryPoint = "GetMenuItemID")]
		static extern uint GetMenuItemID(IntPtr hmenu, int pos);

		[DllImport("user32.dll")]
		public static extern bool DrawMenuBar(IntPtr hWnd);

		public enum ShowWindowCommands
		{
			Maximize = 3,
			Restore = 9,
		}

		public static IntPtr JoinConsole()
		{
			var res = JoinConsole(false);
			ConsoleEx.CheckStdOutput();
			return res;
		}

		public static IntPtr JoinConsole(bool setStandardOutput)
		{
			AllocConsole();
			var cptr = GetConsoleWindow();
			DisableWindowClosing(cptr, true);
			if (setStandardOutput)
				SetStandardOutput();
			return cptr;
		}

		public static bool SetStandardOutput()
		{
			var piPreamble = typeof(StreamWriter).GetProperty("HaveWrittenPreamble", BindingFlags.NonPublic | BindingFlags.Instance);
			if (piPreamble == null) return false;
			var bufferSizeConst = typeof(Console).GetField("DefaultConsoleBufferSize", BindingFlags.NonPublic | BindingFlags.Static);
			var bufferSize = bufferSizeConst != null ? (int)bufferSizeConst.GetValue(null) : 256;
			var consoleStream = Console.OpenStandardOutput(bufferSize);
			if (consoleStream == Stream.Null) return false;
			var stdxxx = new StreamWriter(consoleStream, Console.OutputEncoding, bufferSize);
			piPreamble.SetValue(stdxxx, true, null);
			stdxxx.AutoFlush = true;
			Console.SetOut(TextWriter.Synchronized(stdxxx));
			return true;
		}

		public static void DetachConsole()
		{
			DetachConsole(true);
		}

		public static void DetachConsole(bool resetStandardOutput)
		{
			if (!FreeConsole()) return;
			if (resetStandardOutput)
				Console.SetOut(TextWriter.Synchronized(StreamWriter.Null));
		}

		public static void DisableWindowClosing(IntPtr hWnd, bool disableDropDownItem = false)
		{
			var hMenu = GetSystemMenu(hWnd, false);
			if (hMenu == IntPtr.Zero) return;
			EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
			if (disableDropDownItem)
			{
				var cnt = GetMenuItemCount(hMenu);
				RemoveMenu(hMenu, cnt - 4, MF_BYPOSITION | MF_DISABLED);
			}
			DrawMenuBar(hWnd);
		}

		public static void EnableWindowClosing(IntPtr hWnd)
		{
			var hMenu = GetSystemMenu(hWnd, false);
			if (hMenu == IntPtr.Zero) return;
			EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_ENABLED);
			DrawMenuBar(hWnd);
		}

		public static void TwitchMouse()
		{
			mouse_event((uint)(MouseEventFlags.ABSOLUTE | MouseEventFlags.LEFTDOWN), 0, 0, 0, 0);
			mouse_event((uint)(MouseEventFlags.ABSOLUTE | MouseEventFlags.LEFTUP), 0, 0, 0, 0);
		}

		public static string GetErrorMessage(int errorCode)
		{
			var message = new StringBuilder(1025); // Most of error messages will be okey
			var num1 = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM, IntPtr.Zero, errorCode, 0, message, message.Capacity,
															 IntPtr.Zero);
			return num1 != 0 ? message.ToString() : string.Format("Win32 Error {0}", errorCode);
		}

		[DllImport("user32.dll")]
		static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

		[Flags]
		enum MouseEventFlags : uint
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010,
			WHEEL = 0x00000800,
			XDOWN = 0x00000080,
			XUP = 0x00000100
		}

		[DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
		public extern static Int32 GetCurrentThemeName(StringBuilder stringThemeName, int lengthThemeName, StringBuilder stringColorName, int lengthColorName, StringBuilder stringSizeName, int lengthSizeName);

		const int FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
		[DllImport("kernel32.dll", EntryPoint = "FormatMessageW", CharSet = CharSet.Unicode)]
		static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId,
																						StringBuilder lpBuffer, int nSize, IntPtr vaListArguments);
	}

	static class ConsoleEx
	{
		public static bool IsOutputRedirected
		{
			get { return FileType.Char != GetFileType(GetStdHandle(StdOutputHandle)); }
		}

		public static void CheckStdOutput()
		{
			if (!IsOutputRedirected) return;
			var outHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
			SetStdHandle(StdOutputHandle, outHandle);
			TextWriter writer = new StreamWriter(Console.OpenStandardOutput(), Console.OutputEncoding) { AutoFlush = true };
			Console.SetOut(writer);
		}

		// P/Invoke:
		private enum FileType { Unknown, Disk, Char, Pipe };
		const uint StdOutputHandle = 0xFFFFFFF5;
		[DllImport("kernel32.dll")]
		private static extern FileType GetFileType(IntPtr hdl);
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetStdHandle(uint nStdHandle);
		[DllImport("kernel32.dll")]
		private static extern void SetStdHandle(uint nStdHandle, IntPtr handle);
		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern IntPtr CreateFile(
			string fileName,
			[MarshalAs(UnmanagedType.U4)] uint fileAccess,
			[MarshalAs(UnmanagedType.U4)] uint fileShare,
			IntPtr securityAttributes,
			[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
			[MarshalAs(UnmanagedType.U4)] int flags,
			IntPtr template
		);
	}
}
