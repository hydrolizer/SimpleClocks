using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Xml;
using WX.Core.Extensions;
using WX.Core.Utils;

namespace SimpleClocks.Utils
{
	sealed class ColorSettings : INotifyPropertyChanged
	{
		public const string TargetColumnName = "Target";
		public const string ColorColumnName = "Color";
		public const string ProfileNameAttribute = "ProfileName";
		public const string DefaultProfileName = "default";
		public const string ProfileFileExtension = ".sccp";

		static readonly TextEqualityComparer keysEqualityComparer = new TextEqualityComparer(StringComparison.OrdinalIgnoreCase);
		static readonly TextComparer keysComparer = new TextComparer(StringComparison.Ordinal);

		readonly Dictionary<string, ColorContainer> _colors = new Dictionary<string, ColorContainer>(keysEqualityComparer);
		readonly Dictionary<string, Color> _defaults = new Dictionary<string, Color>(keysEqualityComparer);

		public string ProfileName { get; }
		public bool IsDefault { get; }
		string _fileName;
		ColorSettings(string name, string fileName, IEnumerable<ColorDataItem> colorDataItems)
		{
			_fileName = fileName;
			ProfileName = name;
			IsDefault = DefaultProfileName.Equals(name, StringComparison.Ordinal);
			Load(colorDataItems);
		}

		ColorSettings()
		{
			ProfileName = DefaultProfileName;
			_fileName = NewFileName();
			IsDefault = true;
		}

		static string NewFileName()=>$"{Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)}{ProfileFileExtension}";
		static IsolatedStorageFile GetStorageFile() => IsolatedStorageFile.GetUserStoreForDomain();

		internal static ColorSettings FromFile(string isoFileName) => FromFile(GetStorageFile(), isoFileName);
		internal static ColorSettings FromFile(IsolatedStorageFile file, string isoFileName)
		{
			try
			{
				var xd = new XmlDocument();
				using(var isf = file.OpenFile(isoFileName, FileMode.Open, FileAccess.Read))
				using(var xr = XmlReader.Create(isf))
				{
					xd.Load(xr);
					var root = xd.DocumentElement;
					if (root == null) return null;
					return new ColorSettings(root.GetAttribute(ProfileNameAttribute), isoFileName, ColorDataItem.FromXml(xd));
				}
			}
			catch (Exception e)
			{
				log.Error(e.Format(ErrorFormat.Verbose));
				return null;
			}
		}

		internal static ColorSettings Default() => new ColorSettings();

		public IReadOnlyList<ColorContainer> Colors =>
			_colors.OrderBy(kvp=>kvp.Key, keysComparer).Select(kvp=>kvp.Value).ToList();

		bool _isActive;
		public bool IsActive
		{
			get => _isActive;
			set
			{
				_isActive = value;
				OnPropertyChanged();
			}
		}

		public ColorContainer this[string target, Color defaultColor]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(target))
					throw new ArgumentNullException(nameof(target));
				if (!_defaults.ContainsKey(target))
					_defaults.Add(target, defaultColor);
				if (_colors.TryGetValue(target, out var colorContainer))
					return colorContainer;
				colorContainer = new ColorContainer(target, defaultColor);
				_colors.Add(target, colorContainer);
				return colorContainer;
			}
		}

		public void CopyTo(ColorSettings other)
		{
			other.Load(_colors.Values.Select(cc=>cc.GetDataItem()));
		}

		void Load(IEnumerable<ColorDataItem> colorDataItems)
		{
			try
			{
				if (colorDataItems == null)
					return;
				foreach(var cdi in colorDataItems)
				{
					if (_colors.TryGetValue(cdi.Name, out var cc))
						cc.Color = cdi.Color;
					else
						_colors.Add(cdi.Name, new ColorContainer(cdi));
				}
			}
			catch (Exception e)
			{
				log.Error(e.Format(ErrorFormat.Verbose));
			}
		}

		public void RejectChanges()
		{
			foreach(var container in _colors.Values)
				container.RejectChanges();
		}

		public void ResetToDefaults()
		{
			foreach(var kvp in _defaults)
				if (_colors.TryGetValue(kvp.Key, out var colorContainer))
					colorContainer.Color = kvp.Value;
		}

		XmlDocument ToXml(string profileName, bool fixChanges)
		{
			if (fixChanges)
				foreach(var container in _colors.Values)
					container.ApplyChanges();
			var xd = ColorDataItem.ToXml(_colors.Values.Select(cc=>cc.GetDataItem()));
			var root = xd.DocumentElement;
			if (root == null)
				throw new InvalidOperationException("Root element is missing");
			if (!string.IsNullOrEmpty(profileName))
				root.SetAttribute(ProfileNameAttribute, profileName);
			return xd;
		}

		public void Save() => Save(_fileName, ProfileName, true);

		public ColorSettings SaveAs(string newName)
		{
			var fileName = NewFileName();
			Save(fileName, newName, false);
			return FromFile(fileName);
		}

		void Save(string fileName, string profileName, bool fixChanges)
		{
			using(var isf = GetStorageFile().OpenFile(fileName, FileMode.Create))
			using(var xw = XmlWriter.Create(isf))
				ToXml(profileName, fixChanges).Save(xw);
		}

		public void ExportToFile(string destination)
		{
			using(var fs = new FileStream(destination, FileMode.Create))
				SaveColorsData(fs);
		}

		void SaveColorsData(Stream stream)
		{
			using(var xw = XmlWriter.Create(stream, new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				IndentChars = "\t"
			}))
				ToXml(null, false).Save(xw);
		}

		public void ImportFromFile(string source)
		{
			var xd = new XmlDocument();
			using(var fs = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using(var xr = XmlReader.Create(fs))
				xd.Load(xr);
			Load(ColorDataItem.FromXml(xd));
		}

		static readonly Logger log = LogManager.GetCurrentClassLogger();

		public event PropertyChangedEventHandler PropertyChanged;
		void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
