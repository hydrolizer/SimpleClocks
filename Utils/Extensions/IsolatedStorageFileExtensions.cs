using System;
using System.IO.IsolatedStorage;

namespace SimpleClocks.Utils.Extensions
{
	public static class IsolatedStorageFileExtensions
	{
		static readonly Lazy<Func<IsolatedStorageFile, string>> lazy = new Lazy<Func<IsolatedStorageFile, string>>(
			()=>ExpressionEx.FieldGet<IsolatedStorageFile, string>("m_RootDir")
		);

		public static string GetPath(this IsolatedStorageFile isoFile) => lazy.Value(isoFile);
	}
}
