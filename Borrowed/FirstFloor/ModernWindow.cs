using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SystemCommands=Microsoft.Windows.Shell.SystemCommands;

namespace FirstFloor.ModernUI.Windows.Controls
{
	public class ModernWindow
			: DpiAwareWindow
	{
		public static readonly DependencyProperty MinimizeButtonProperty = DependencyProperty.Register(
			"MinimizeButton", typeof(bool), typeof(ModernWindow),
			new FrameworkPropertyMetadata(true,
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
			)
		);

		/// <summary/>
		public static readonly DependencyProperty MaximizeButtonProperty = DependencyProperty.Register(
			"MaximizeButton", typeof(bool), typeof(ModernWindow),
			new FrameworkPropertyMetadata(true,
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
			)
		);

		public ModernWindow()
		{
			DefaultStyleKey = typeof(ModernWindow);
			Buttons = new ItemsControl().Items;
			CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, OnCloseWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, OnMaximizeWindow, OnCanResizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, OnMinimizeWindow, OnCanMinimizeWindow));
			CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, OnRestoreWindow, OnCanResizeWindow));
		}

		private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
		}

		private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ResizeMode != ResizeMode.NoResize;
		}

		private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.CloseWindow(this);
		}

		private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MaximizeWindow(this);
		}

		private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.MinimizeWindow(this);
		}

		private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
		{
			SystemCommands.RestoreWindow(this);
		}

		public bool MinimizeButton
		{
			get => (bool)GetValue(MinimizeButtonProperty);
			set => SetValue(MinimizeButtonProperty, value);
		}

		public bool MaximizeButton
		{
			get => (bool)GetValue(MaximizeButtonProperty);
			set => SetValue(MaximizeButtonProperty, value);

		}

		public static readonly DependencyProperty ButtonsProperty = DependencyProperty.Register(
			"Buttons", typeof(ItemCollection),
			typeof(ModernWindow), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

		public ItemCollection Buttons
		{
			get => (ItemCollection)GetValue(ButtonsProperty);
			set => SetValue(ButtonsProperty, value);
		}

		const bool TitleBarOverContentDefault = false;
		public static readonly DependencyProperty TitleBarOverContentProperty = DependencyProperty.Register(
			nameof(TitleBarOverContent), typeof(bool), typeof(ModernWindow),
			new FrameworkPropertyMetadata(TitleBarOverContentDefault,
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
			)
		);

		public bool TitleBarOverContent
		{
			get => (bool)(GetValue(TitleBarOverContentProperty) ?? TitleBarOverContentDefault);
			set => SetValue(TitleBarOverContentProperty, value);
		}

		const bool IsTitleBarVisibleDefault = true;
		public static readonly DependencyProperty IsTitleBarVisibleProperty = DependencyProperty.Register(
			nameof(IsTitleBarVisible), typeof(bool), typeof(ModernWindow),
			new FrameworkPropertyMetadata(IsTitleBarVisibleDefault,
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
			)
		);

		public bool IsTitleBarVisible
		{
			get => (bool)(GetValue(IsTitleBarVisibleProperty) ?? IsTitleBarVisibleDefault);
			set => SetValue(IsTitleBarVisibleProperty, value);
		}

		public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register(
			nameof(TitleBarBackground), typeof(Brush), typeof(ModernWindow),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
		);

		public Brush TitleBarBackground
		{
			get => (Brush)GetValue(TitleBarBackgroundProperty);
			set => SetValue(TitleBarBackgroundProperty, value);
		}

		public static readonly DependencyProperty TitleBarMouseOverBackgroundProperty = DependencyProperty.Register(
			nameof(TitleBarMouseOverBackground), typeof(Brush), typeof(ModernWindow),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
		);

		public Brush TitleBarMouseOverBackground
		{
			get => (Brush)GetValue(TitleBarMouseOverBackgroundProperty);
			set => SetValue(TitleBarMouseOverBackgroundProperty, value);
		}

		public static readonly DependencyProperty TitleBarForegroundProperty = DependencyProperty.Register(
			nameof(TitleBarForeground), typeof(Brush), typeof(ModernWindow),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
		);

		public Brush TitleBarForeground
		{
			get => (Brush)GetValue(TitleBarForegroundProperty);
			set => SetValue(TitleBarForegroundProperty, value);
		}

		public static readonly DependencyProperty TitleBarMouseOverForegroundProperty = DependencyProperty.Register(
			nameof(TitleBarMouseOverForeground), typeof(Brush), typeof(ModernWindow),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender)
		);

		public Brush TitleBarMouseOverForeground
		{
			get => (Brush)GetValue(TitleBarMouseOverForegroundProperty);
			set => SetValue(TitleBarMouseOverForegroundProperty, value);
		}
	}
}
