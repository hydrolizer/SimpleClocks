using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Interop;
using Binding = System.Windows.Data.Binding;
using // TODO ContextMenu is no longer supported. Use ContextMenuStrip instead. For more details see https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms#removed-controls
ContextMenu = System.Windows.Controls.ContextMenu;
using // TODO MenuItem is no longer supported. Use ToolStripMenuItem instead. For more details see https://docs.microsoft.com/en-us/dotnet/core/compatibility/winforms#removed-controls
MenuItem = System.Windows.Controls.MenuItem;

namespace SimpleClocks.Controls
{
	public enum Side
	{
		Left,
		Right,
		Up,
		Down
	}

	[TemplatePart(Name = "PART_DownArrow", Type = typeof(Image))]
	[TemplatePart(Name = "PART_DropDownBorder", Type = (typeof(Border)))]
	[TemplateVisualState(Name = "Opened", GroupName = "CommonStates")]
	public class DropDownButton : ToggleButton
	{
		public static readonly DependencyProperty DownArrowPaddingProperty = DependencyProperty.Register(
			"DownArrowPadding", typeof(Thickness), typeof(DropDownButton), new FrameworkPropertyMetadata(
				new Thickness(2,0,0,0), FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure
			)
		);

		public Thickness DownArrowPadding
		{
			get => (Thickness) GetValue(DownArrowPaddingProperty);
			set => SetValue(DownArrowPaddingProperty, value);
		}

		public static readonly DependencyProperty DropDownPlacementPropery = DependencyProperty.Register(
			"DropDownPlacement", typeof(UIElement), typeof(DropDownButton),
			new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure)
		);

		public UIElement DropDownPlacement
		{
			get => (UIElement)GetValue(DropDownPlacementPropery);
			set => SetValue(DropDownPlacementPropery, value);
		}

		public static readonly DependencyProperty ShowDownArrowProperty = DependencyProperty.Register(
			"ShowDownArrow", typeof(bool), typeof(DropDownButton), new FrameworkPropertyMetadata(true,
				FrameworkPropertyMetadataOptions.AffectsMeasure, OnShowDownArrowPropertyChanged));

		public static readonly DependencyProperty DropDownProperty = DependencyProperty.Register("DropDown",
				typeof(ContextMenu), typeof(DropDownButton),
				new FrameworkPropertyMetadata(null, OnDropdownChanged));

		private static readonly DependencyPropertyKey visibleItemsCountPropertyKey = DependencyProperty.RegisterReadOnly(
			"VisibleItemsCount", typeof (int), typeof (DropDownButton), new PropertyMetadata(1));

		public static readonly DependencyProperty VisibleItemsCountProperty = visibleItemsCountPropertyKey.DependencyProperty;

		public int VisibleItemsCount
		{
			get => (int) GetValue(VisibleItemsCountProperty);
			private set => SetValue(visibleItemsCountPropertyKey, value);
		}

		private static readonly DependencyPropertyKey IsOpenedPropertyKey = DependencyProperty.RegisterReadOnly(
			"IsOpened", typeof (bool), typeof (DropDownButton), new PropertyMetadata(false));

		public static readonly DependencyProperty IsOpenedProperty = IsOpenedPropertyKey.DependencyProperty;

		public bool IsOpened
		{
			get => (bool) GetValue(IsOpenedProperty);
			private set => SetValue(IsOpenedPropertyKey, value);
		}

		public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register(
			"DropDownContent", typeof (object), typeof (DropDownButton),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public object DropDownContent
		{
			get => GetValue(DropDownContentProperty);
			set => SetValue(DropDownContentProperty, value);
		}

		public static readonly DependencyProperty CommandStyleProperty = DependencyProperty.Register(
			"CommandStyle", typeof(Style), typeof(DropDownButton),
			new FrameworkPropertyMetadata(
				null,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public Style CommandStyle
		{
			get => (Style) GetValue(CommandStyleProperty);
			set => SetValue(CommandStyleProperty, value);
		}

		public static readonly DependencyProperty IsCommandButtonVisibleProperty = DependencyProperty.Register(
			"IsCommandButtonVisible", typeof(bool), typeof(DropDownButton),
			new FrameworkPropertyMetadata(
				false,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public bool IsCommandButtonVisible
		{
			get => (bool) GetValue(IsCommandButtonVisibleProperty);
			set => SetValue(IsCommandButtonVisibleProperty, value);
		}

		public static readonly DependencyProperty DropDownSideProperty = DependencyProperty.Register(
			"DropDownSide", typeof(Side), typeof(DropDownButton),
			new FrameworkPropertyMetadata(
				Side.Right,
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public Side DropDownSide
		{
			get => (Side) GetValue(DropDownSideProperty);
			set => SetValue(DropDownSideProperty, value);
		}

		public static readonly DependencyProperty DropDownWidthProperty = DependencyProperty.Register(
			"DropDownWidth", typeof(GridLength), typeof(DropDownButton),
			new FrameworkPropertyMetadata(
				new GridLength(1, GridUnitType.Star),
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public GridLength DropDownWidth
		{
			get => (GridLength) GetValue(DropDownWidthProperty);
			set => SetValue(DropDownWidthProperty, value);
		}

		public static readonly DependencyProperty CommandButtonWidthProperty = DependencyProperty.Register(
			"CommandButtonWidth", typeof(GridLength), typeof(DropDownButton),
			new FrameworkPropertyMetadata(
				new GridLength(1, GridUnitType.Star),
				FrameworkPropertyMetadataOptions.AffectsRender |
				FrameworkPropertyMetadataOptions.AffectsArrange |
				FrameworkPropertyMetadataOptions.AffectsMeasure
				)
		);

		public GridLength CommandButtonWidth
		{
			get => (GridLength)GetValue(CommandButtonWidthProperty);
			set => SetValue(CommandButtonWidthProperty, value);
		}

		public bool ShowDownArrow
		{
			get => (bool)GetValue(ShowDownArrowProperty);
			set => SetValue(ShowDownArrowProperty, value);
		}

		public static readonly DependencyProperty OpenIfEmptyProperty = DependencyProperty.Register(
			"OpenIfEmpty", typeof(bool), typeof(DropDownButton), new PropertyMetadata(true)
		);

		public bool OpenIfEmpty
		{
			get => (bool) (GetValue(OpenIfEmptyProperty) ?? true);
			set => SetValue(OpenIfEmptyProperty, value);
		}

		public static readonly DependencyProperty ArrowDirectionProperty = DependencyProperty.Register(
			"ArrowDirection", typeof(Side), typeof(DropDownButton), new PropertyMetadata(Side.Down)
		);

		public Side ArrowDirection
		{
			get => (Side) (GetValue(ArrowDirectionProperty) ?? Side.Down);
			set => SetValue(ArrowDirectionProperty, value);
		}

		public event RoutedEventHandler DropDownOpened
		{
			add
			{
				if(DropDown!=null)
					DropDown.Opened += value;
			}
			remove
			{
				if (DropDown!=null)
					DropDown.Opened -= value;
			}
		}

		private static void OnShowDownArrowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var ddb = d as DropDownButton;
			if (ddb?._downArrow == null) return;
			ddb._downArrow.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
		}

		private static void OnDropdownChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var ddb = sender as DropDownButton;
			if (ddb == null) return;
			var cmNew = e.NewValue as ContextMenu;
			if (cmNew != null)
			{
				cmNew.Closed += OnDropdownClosed;
				if (ddb.IsCommandButtonVisible)
					cmNew.Opened += ddb.OnDropDownOpened;
				cmNew.Tag = ddb;
				if (ddb.IsLoaded)
					ddb.VisibleItemsCount = cmNew.Items.OfType<MenuItem>().Count(mi => mi.Visibility == Visibility.Visible);
			}
			var cmOld = e.OldValue as ContextMenu;
			if (cmOld == null) return;
			cmOld.Closed -= OnDropdownClosed;
			if (ddb.IsCommandButtonVisible)
				cmOld.Opened -= ddb.OnDropDownOpened;
			if (cmNew == null)
				ddb.VisibleItemsCount = 0;
		}

		private static void OnDropdownClosed(object sender, RoutedEventArgs e)
		{
			var ddb = ((ContextMenu) sender).PlacementTarget as DropDownButton ?? (DropDownButton)((ContextMenu)sender).Tag;
			VisualStateManager.GoToState(ddb, "Normal", true);
			ddb.IsOpened = false;
		}

		static DropDownButton()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(DropDownButton),
				new FrameworkPropertyMetadata(typeof(DropDownButton)));
		}

		Image _downArrow;
		Border _dropDownBorder;

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_downArrow = GetTemplateChild("PART_DownArrow") as Image;
			_dropDownBorder = GetTemplateChild("PART_DropDownBorder") as Border;
			if (_downArrow!=null)
				_downArrow.Visibility = ShowDownArrow ? Visibility.Visible : Visibility.Collapsed;
			if (DropDown != null)
				VisibleItemsCount = DropDown.Items.OfType<MenuItem>().Count(mi=>mi.Visibility==Visibility.Visible);
		}

		public DropDownButton()
		{
			IsTabStop = false;
			var binding = new Binding("DropDown.IsOpen") {Source = this};
			SetBinding(IsCheckedProperty, binding);
		}

		public ContextMenu DropDown
		{
			get => (ContextMenu) GetValue(DropDownProperty);
			set => SetValue(DropDownProperty, value);
		}

		public new event EventHandler Click;
		protected override void OnClick()
		{
			Open();
			Click?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler Opening;

		public void Open()
		{
			Opening?.Invoke(this, EventArgs.Empty);
			if (DropDown == null || (!OpenIfEmpty && !DropDown.HasItems)) return;
			DropDown.PlacementTarget = DropDownPlacement ?? this;
				/*DropDownPlacement ?? (
					IsCommandButtonVisible && _dropDownBorder!=null
						? _dropDownBorder
						: (UIElement)this
					);*/
			DropDown.Placement = PlacementMode.Bottom;
			DropDown.IsOpen = true;
			VisualStateManager.GoToState(this, "Opened", true);
			IsOpened = true;
		}

		const double MenuWidthDelta = 4.0;
		void OnDropDownOpened(object sender, RoutedEventArgs ea)
		{
			var cm = sender as ContextMenu;
			if (cm==null || cm.ActualWidth>=ActualWidth) return;
			cm.Width = ActualWidth + MenuWidthDelta;
		}

		public static bool IsMenuDirectionReversed(ContextMenu menu)
		{
			var hwndSource = PresentationSource.FromVisual(menu.PlacementTarget) as HwndSource;
			if (hwndSource==null) return false;
			var targetHwnd = hwndSource.Handle;
			var screen = Screen.FromHandle(targetHwnd);
			var targetCtrl = menu.PlacementTarget as FrameworkElement;
			if (targetCtrl==null) return false;
			var targetLoc = targetCtrl.PointToScreen(new Point(0, 0));
			var targetBottom = targetLoc.Y + targetCtrl.ActualHeight;
			if (menu.Placement != PlacementMode.Bottom)
				throw new NotImplementedException("you need to implement your own logic for other modes");
			return screen.WorkingArea.Bottom < targetBottom + menu.ActualHeight;
		}
	}

	internal class DropDownBorderThicknessConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var bt = (Thickness)values[0]; // dropdown border thickness
			var cbv = (bool)values[1]; // is command button of dropdown visible
			var side = (Side)values[2];
			var isOpened = (bool) values[3];
			if (!cbv) return bt;
			return new Thickness(
				side == Side.Right ? 0 : bt.Left,
				bt.Top,
				side == Side.Right ? bt.Right : 0,
				isOpened ? 0 : bt.Bottom
			);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	internal class DropDownColumnConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && (Side)value == Side.Right ? 0 : 2;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
