using PropertyChanged;
using SimpleClocks.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace SimpleClocks.Models
{
	public class ColorContainer : INotifyPropertyChanged, INamedColor
	{
		readonly INamedColor _colorInfo;
		readonly INotifyColorChanged _owner;
		internal ColorContainer(INotifyColorChanged owner, INamedColor namedColor)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_colorInfo = namedColor ?? throw new ArgumentNullException(nameof(namedColor));
			_color = namedColor.Color;
			_sourceValue = namedColor.Color;
		}


		public ColorDataItem GetDataItem() => new ColorDataItem
		{
			Name = Name,
			Description = Description,
			Color = Color
		};

		public bool IsChanged { get; private set; }

		Color _sourceValue;
		Color _color;
		[DoNotNotify]
		public Color Color
		{
			get => _color;
			set
			{
				if (value == _color) return;
				_color = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
				_owner.ColorContanierChanged(this);
				if (IsChanged) return;
				IsChanged = true;
			}
		}

		public void RejectChanges()
		{
			if (!IsChanged) return;
			Color = _sourceValue;
			IsChanged = false;
		}

		public void ApplyChanges()
		{
			IsChanged = false;
			_sourceValue = Color;
		}

		[DependsOn(nameof(Color))]
		public SolidColorBrush Brush
		{
			get
			{
				var scb = new SolidColorBrush(_color);
				scb.Freeze();
				return scb;
			}
		}

		public string Name => _colorInfo.Name;
		public string Description => _colorInfo.Description;

		public event PropertyChangedEventHandler PropertyChanged;

		public override bool Equals(object obj)
		{
			return obj is ColorContainer cc && Name.Equals(cc.Name, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return Name.ToUpperInvariant().GetHashCode();
		}
	}

	public class ColorDataItem : INamedColor
	{
		public string Name { get; set; }
		public string Description { get; set; }
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
