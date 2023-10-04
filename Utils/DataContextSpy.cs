using System;
using System.Windows;
using System.Windows.Data;

namespace SimpleClocks.Utils
{
	public class DataContextSpy : Freezable
	{
		public DataContextSpy()
		{
			BindingOperations.SetBinding(this, DataContextProperty, new Binding());
		}

		public object DataContext
		{
			get => GetValue(DataContextProperty);
			set => SetValue(DataContextProperty, value);
		}

		public static readonly DependencyProperty DataContextProperty =
			FrameworkElement.DataContextProperty.AddOwner(typeof (DataContextSpy));

		protected override Freezable CreateInstanceCore()
		{
			throw new NotImplementedException();
		}
	}
}
