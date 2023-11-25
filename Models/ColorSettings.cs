using NLog;
using SimpleClocks.Utils;
using SimpleClocks.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Xml;

namespace SimpleClocks.Models
{
	delegate void ColorContanierChangedHandler(ColorContainer container);
	interface INotifyColorChanged
	{
		void ColorContanierChanged(ColorContainer container);
		event ColorContanierChangedHandler OnColorContanierChanged;
	}

	sealed class ColorSettings : INotifyPropertyChanged, INotifyColorChanged
	{
		public const string TargetColumnName = "Target";
		public const string ColorColumnName = "Color";
		public const string ProfileNameAttribute = "ProfileName";
		public const string DefaultProfileName = "default";
		public const string ProfileFileExtension = ".sccp";

		static readonly TextEqualityComparer keysEqualityComparer = new TextEqualityComparer(StringComparison.OrdinalIgnoreCase);
		static readonly TextComparer keysComparer = new TextComparer(StringComparison.Ordinal);

		readonly Dictionary<string, ColorContainer> _colors = new Dictionary<string, ColorContainer>(keysEqualityComparer);

		public string ProfileName { get; }
		public bool IsDefault { get; }
		readonly string _fileName;
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
			foreach(var kvp in DefaultColors.Values)
				_colors.Add(kvp.Key, new ColorContainer(this, kvp.Value));
			IsDefault = true;
		}

		ColorSettings(string name, ColorSettings baseSettings)
		{
			ProfileName = name;
			_fileName = NewFileName();
			foreach(var kvp in baseSettings._colors)
				_colors.Add(kvp.Key, new ColorContainer(this, kvp.Value));
		}

		static string NewFileName()=>$"{Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture)}{ProfileFileExtension}";
		static IsolatedStorageFile GetStorageFile() => IsolatedStorageFile.GetUserStoreForDomain();

		internal static ColorSettings CreateCopy(string name, ColorSettings baseSettings)
			=> new ColorSettings(name, baseSettings);
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

		internal void DeleteData()
		{
			using(var isf = GetStorageFile())
				isf.DeleteFile(_fileName);

		}

		public IReadOnlyList<ColorContainer> Colors =>
			_colors.OrderBy(kvp=>kvp.Key, keysComparer).Select(kvp=>kvp.Value).ToList();

		public bool IsActive { get; set; }

		public void InjectResources(FrameworkElement owner)
		{
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));
			foreach (var kvp in _colors)
				owner.Resources[kvp.Key] = kvp.Value.Brush;
		}

		public void InjectResources()
		{
			foreach (var kvp in _colors)
				Application.Current.Resources[kvp.Key] = kvp.Value.Brush;
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
					if (!DefaultColors.Values.ContainsKey(cdi.Name)) continue;
					if (_colors.TryGetValue(cdi.Name, out var cc))
						cc.Color = cdi.Color;
					else
						_colors.Add(cdi.Name, new ColorContainer(this, cdi));
				}
				foreach(var kvp in DefaultColors.Values)
					if (!_colors.ContainsKey(kvp.Key))
						_colors.Add(kvp.Key, new ColorContainer(this, kvp.Value));
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
			foreach(var kvp in DefaultColors.Values)
				if (_colors.TryGetValue(kvp.Key, out var colorContainer))
					colorContainer.Color = kvp.Value.Color;
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
			// see https://stackoverflow.com/questions/48868767/c-sharp-infinite-recursion-during-resource-lookup
			// for details
			var currentCulture = Thread.CurrentThread.CurrentCulture;
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
				using(var isf = GetStorageFile().OpenFile(fileName, FileMode.Create))
				{
					using(var xw = XmlWriter.Create(isf))
					{
						ToXml(profileName, fixChanges).Save(xw);
					}
				}
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
				Thread.CurrentThread.CurrentUICulture = currentUiCulture;
			}
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

		void INotifyColorChanged.ColorContanierChanged(ColorContainer container)
		{
			OnColorContanierChanged?.Invoke(container);
		}

		public event ColorContanierChangedHandler OnColorContanierChanged;
	}
}
