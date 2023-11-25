using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace SimpleClocks.Utils.Extensions
{
	public delegate bool ItemSearchHandler(Visual item);

	public static class DependencyObjectExtensions
	{
		public static T FindAncestor<T>(this DependencyObject dependencyObject)
			where T : DependencyObject
		{
			if (dependencyObject==null)
				throw new ArgumentNullException(nameof(dependencyObject));
			var parent = VisualTreeHelper.GetParent(dependencyObject);
			if (parent == null) return null;
			var parentT = parent as T;
			return parentT ?? FindAncestor<T>(parent);
		}

		public static IEnumerable<DependencyObject> GetAllAncestors(this DependencyObject dependencyObject)
		{
			if (dependencyObject==null)
				throw new ArgumentNullException(nameof(dependencyObject));
			var parent = VisualTreeHelper.GetParent(dependencyObject);
			while(parent!=null)
			{
				yield return parent;
				parent = VisualTreeHelper.GetParent(parent);
			}
		}

		public static DependencyObject GetTopLevelControl(this DependencyObject control)
		{
			if (control==null) return null;
			var tmp = control;
			DependencyObject parent = null;
			while ((tmp = VisualTreeHelper.GetParent(tmp)) != null)
				parent = tmp;
			return parent;
		}

		public static T GetVisualChild<T>(this DependencyObject parent, ItemSearchHandler filter) where T : Visual
			=> parent.GetVisualChild(filter == null ? (Func<T, bool>) null : item => filter(item));

		public static T GetVisualChild<T>(this DependencyObject parent, Func<T, bool> filter=null) where T : Visual
		{
			if (parent == null) return null;
			var child = default(T);
			var numVisuals = VisualTreeHelper.GetChildrenCount(parent);
			for (var i = 0; i < numVisuals; i++)
			{
				var v = (Visual)VisualTreeHelper.GetChild(parent, i);
				child = v as T ?? GetVisualChild(v, filter);
				if (child == null) continue;
				if (filter != null && !filter(child)) continue;
				break;
			}
			return child;
		}

		public static T GetLogicalChild<T>(this DependencyObject parent) where T : DependencyObject
		{
			var child = default(T);
			var visuals = LogicalTreeHelper.GetChildren(parent);
			foreach (var v in visuals.Cast<object>().Where(o => o is DependencyObject))
			{
				child = v as T ?? GetLogicalChild<T>(v as DependencyObject);
				if (child != null)
					break;
			}
			return child;
		}

		public static IEnumerable<T> GetLogicalChildren<T>(this DependencyObject parent, bool allHierarchy) where T : DependencyObject
		{
			if (parent==null)
				yield break;
			var visuals = LogicalTreeHelper.GetChildren(parent);
			foreach (var v in visuals.OfType<DependencyObject>())
			{
				if (v is T child)
					yield return child;
				if (!allHierarchy) continue;
				foreach (var childOfChild in GetLogicalChildren<T>(v, true))
					yield return childOfChild;
			}
		}

		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject root) where T : DependencyObject
		{
			if (root != null)
			{
				for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
				{
					var child = VisualTreeHelper.GetChild(root, i);
					if (child is T t)
						yield return t;
					foreach (var childOfChild in FindVisualChildren<T>(child))
					{
						yield return childOfChild;
					}
				}
			}
		}
	}
}
