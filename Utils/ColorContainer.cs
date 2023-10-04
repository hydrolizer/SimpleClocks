using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using WX.Core.Extensions;

namespace SimpleClocks.Utils
{
	public class ColorContainer : INotifyPropertyChanged
	{
		public ColorContainer(string name, Color color)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			Name = name;
			_color = color;
			_sourceValue = color;
		}

		public ColorDataItem GetDataItem() => new ColorDataItem { Name = Name, Color = Color };

		public ColorContainer(ColorDataItem dataItem)
			:this(dataItem.Name, dataItem.Color)
		{
		}

		public bool IsChanged { get; private set; }

		Color _sourceValue;
		Color _color;
		public Color Color
		{
			get => _color;
			set
			{
				if ("Window background".Equals(Name, StringComparison.Ordinal))
					Console.WriteLine();
				if (value == _color) return;
				_color = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Brush));
				if (IsChanged) return;
				IsChanged = true;
				OnPropertyChanged(nameof(IsChanged));
			}
		}

		public void RejectChanges()
		{
			if (!IsChanged) return;
			Color = _sourceValue;
			IsChanged = false;
			OnPropertyChanged(nameof(IsChanged));
		}

		public void ApplyChanges()
		{
			IsChanged = false;
			_sourceValue = Color;
			OnPropertyChanged(nameof(IsChanged));
		}

		public SolidColorBrush Brush
		{
			get
			{
				var scb = new SolidColorBrush(_color);
				scb.Freeze();
				return scb;
			}
		}

		public string Name { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public override bool Equals(object obj)
		{
			return obj is ColorContainer cc && Name.Equals(cc.Name, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return Name.ToUpperInvariant().GetHashCode();
		}
	}

	public class ColorDataItem
	{
		public string Name { get; set; }
		public Color Color { get; set; }

		static readonly Type serializationType = typeof(List<ColorDataItem>);

		public static IReadOnlyList<ColorDataItem> FromXml(Stream colorsData)
		{
			var xd = new XmlDocument();
			using(var xr = XmlReader.Create(colorsData))
			{
				xd.Load(xr);
				return FromXml(xd);
			}
		}

		public static IReadOnlyList<ColorDataItem> FromXml(IXPathNavigable colorPreferences)
		{
			if (colorPreferences == null)
				return null;
			using(var xr = colorPreferences.EnsureNavigator().ReadSubtree())
			{
				if (!(new XmlSerializer(serializationType).Deserialize(xr) is List<ColorDataItem> list))
					return null;
				return list.GroupBy(i => i.Name).Where(g => g.Count() == 1).SelectMany(g => g).ToList();
			}
		}

		public static XmlDocument ToXml(IEnumerable<ColorDataItem> items)
		{
			var xd = new XmlDocument();
			using(var xw = xd.EnsureNavigator().AppendChild())
				new XmlSerializer(serializationType).Serialize(xw, items.ToList());
			return xd;
		}
	}
}
