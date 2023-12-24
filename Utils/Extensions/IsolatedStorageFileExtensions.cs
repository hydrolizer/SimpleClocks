using System;
using System.IO.IsolatedStorage;

namespace SimpleClocks.Utils.Extensions
{
	public static class IsolatedStorageFileExtensions
	{
		static readonly Lazy<Func<IsolatedStorageFile, string>> lazy = new (
			()=>ExpressionEx.PropertyGet<IsolatedStorageFile, string>("RootDirectory")
		);

		public static string GetPath(this IsolatedStorageFile isoFile) => lazy.Value(isoFile);
	}
}
