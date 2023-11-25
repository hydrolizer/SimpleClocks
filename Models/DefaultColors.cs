using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SimpleClocks.Utils;

namespace SimpleClocks.Models
{
	public interface IColorInfo
	{
		string Name { get; }
		string Description { get; }
	}

	public interface INamedColor : IColorInfo
	{
		Color Color { get; }
	}

	public class NamedColor : INamedColor
	{
		public string Name { get; }
		public string Description { get; }
		public Color Color { get; }
		public NamedColor(string name, string description, string color)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrEmpty(description))
				throw new ArgumentNullException(nameof(description));
			if (string.IsNullOrEmpty(color))
				throw new ArgumentNullException(nameof(color));
			if (!(ColorConverter.ConvertFromString(color) is Color c))
				throw new ArgumentException($"Incorrect color representation in {color}");
			Name = name;
			Description = description;
			Color = c;
			Brush = new SolidColorBrush(Color);
			Brush.Freeze();
		}

		public NamedColor(string name, string description, Color color)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrEmpty(description))
				throw new ArgumentNullException(nameof(description));
			Name = name;
			Description = description;
			Color = color;
			Brush = new SolidColorBrush(Color);
			Brush.Freeze();
		}

		public Brush Brush { get; }
	}

	public static class DefaultColors
	{
		public static NamedColor ClockWindowForeground { get; }
		public static NamedColor ClockWindowBackground { get; }
		public static NamedColor ClockTitleBarBackground { get; }
		public static NamedColor ClockTitleBarForeground { get; }
		public static NamedColor ActiveClockWindowTitleBarBackground { get; }
		public static NamedColor ActiveClockWindowTitleBarForeground { get; }
		public static NamedColor ClockButtonBackground { get; }
		public static NamedColor ClockButtonForeground { get; }
		public static NamedColor ActiveWindowClockButtonBackground { get; }
		public static NamedColor ActiveWindowClockButtonForeground { get; }
		public static NamedColor ClockButtonMouseOverBackground { get; }
		public static NamedColor ClockButtonMouseOverForeground { get; }
		public static NamedColor ClockButtonPressedBackground { get; }
		public static NamedColor ClockButtonPressedForeground { get; }

		public static Dictionary<string, NamedColor> Values { get; }

		static DefaultColors()
		{
			ClockWindowForeground = new NamedColor(nameof(ClockWindowForeground), "Window foreground", "#FF000000");
			ClockWindowBackground = new NamedColor(nameof(ClockWindowBackground), "Window background", "#FFFFFFFF");
			ClockTitleBarBackground = new NamedColor(nameof(ClockTitleBarBackground), "Title bar background", "#80FFFFFF");
			ClockTitleBarForeground = new NamedColor(nameof(ClockTitleBarForeground), "Title bar foreground", "#333333");
			ActiveClockWindowTitleBarBackground = new NamedColor(nameof(ActiveClockWindowTitleBarBackground), "Active window title bar background", "#20D0D0D0");
			ActiveClockWindowTitleBarForeground = new NamedColor(nameof(ActiveClockWindowTitleBarForeground), "Active window title bar foreground", "#1ba1e2");
			ClockButtonBackground = new NamedColor(nameof(ClockButtonBackground), "Button background", Colors.Transparent);
			ClockButtonForeground = new NamedColor(nameof(ClockButtonForeground), "Button foreground", "#333333");
			ActiveWindowClockButtonBackground = new NamedColor(nameof(ActiveWindowClockButtonBackground), "Active window button background", Colors.Transparent);
			ActiveWindowClockButtonForeground = new NamedColor(nameof(ActiveWindowClockButtonForeground), "Active window button foreground", "#333333");
			ClockButtonMouseOverBackground = new NamedColor(nameof(ClockButtonMouseOverBackground),"Button mouse over background", "#dddddd");
			ClockButtonMouseOverForeground = new NamedColor(nameof(ClockButtonMouseOverForeground), "Button mouse over foreground", "#333333");
			ClockButtonPressedBackground = new NamedColor(nameof(ClockButtonPressedBackground), "Pressed button background", "#1ba1e2");
			ClockButtonPressedForeground = new NamedColor(nameof(ClockButtonPressedForeground), "Pressed button foreground", Colors.White);
			
			Values = new[]
			{
				ClockWindowForeground,
				ClockWindowBackground,
				ClockTitleBarBackground,
				ClockTitleBarForeground,
				ActiveClockWindowTitleBarBackground,
				ActiveClockWindowTitleBarForeground,
				ClockButtonBackground,
				ClockButtonForeground,
				ClockButtonMouseOverBackground,
				ClockButtonMouseOverForeground,
				ClockButtonPressedBackground,
				ClockButtonPressedForeground,
				ActiveWindowClockButtonBackground,
				ActiveWindowClockButtonForeground
			}
				.ToDictionary(nc=>nc.Name, nc=>nc, new TextEqualityComparer(StringComparison.Ordinal));
		}
	}
}
