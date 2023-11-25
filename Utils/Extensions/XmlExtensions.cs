using System;
using System.Xml;
using System.Xml.XPath;

namespace SimpleClocks.Utils.Extensions
{
	static class XmlExtensions
	{
		public static XPathNavigator EnsureNavigator(this IXPathNavigable xdata)
		{
			if (xdata==null)
				throw new ArgumentNullException(nameof(xdata));
			return xdata.CreateNavigator() ?? throw new InvalidOperationException("Navigator is null");
		}

		public static XmlReader GetReader(this IXPathNavigable xdata)
			=> xdata.EnsureNavigator().ReadSubtree();

		public static XmlWriter GetWriter(this IXPathNavigable xdata)
			=> xdata.EnsureNavigator().AppendChild();
	}
}
