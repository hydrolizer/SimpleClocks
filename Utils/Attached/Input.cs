using System.Windows;
using System.Windows.Input;

namespace SimpleClocks.Utils.Attached
{
	public static class Input
	{
		public static readonly DependencyProperty BindingsProperty =
			DependencyProperty.RegisterAttached("Bindings", typeof(InputBindingCollection), typeof(Input),
				new FrameworkPropertyMetadata(new InputBindingCollection(),
					(sender, e) =>
					{
						var element = sender as UIElement;
						if (element == null) return;
						element.InputBindings.Clear();
						element.InputBindings.AddRange((InputBindingCollection)e.NewValue);
					}));

		public static InputBindingCollection GetBindings(UIElement element)
		{
			return (InputBindingCollection)element.GetValue(BindingsProperty);
		}

		public static void SetBindings(UIElement element, InputBindingCollection inputBindings)
		{
			element.SetValue(BindingsProperty, inputBindings);
		}
	}
}
