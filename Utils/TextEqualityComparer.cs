using System;
using System.Collections.Generic;

namespace SimpleClocks.Utils
{
	public class TextEqualityComparer : IEqualityComparer<string>
	{
		public StringComparison Comparison { get; }
		public TextEqualityComparer(StringComparison comparison)
		{
			Comparison = comparison;
		}

		public TextEqualityComparer(): this(StringComparison.OrdinalIgnoreCase)
		{
		}

		public bool Equals(string x, string y)
		{
			return (x==null && y==null) || (x!=null && x.Equals(y, Comparison));
		}

		public int GetHashCode(string obj)
			=>
				Comparison==StringComparison.OrdinalIgnoreCase
				? obj.ToLowerInvariant().GetHashCode()
				: EqualityComparer<string>.Default.GetHashCode(obj);
	}

	public class TextComparer: IComparer<string>
	{
		public StringComparison Comparison { get; }

		public TextComparer(StringComparison comparison = StringComparison.OrdinalIgnoreCase)
			=> Comparison = comparison;

		public int Compare(string x, string y) => string.Compare(x, y, Comparison);
	}
}
