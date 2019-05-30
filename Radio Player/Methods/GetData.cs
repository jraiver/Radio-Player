using System.Windows;
using System.Windows.Controls;

namespace radio
{
	internal class GetData
	{
		public static WrapPanel GetWrapPanel(TabItem tab)
		{
			return ((tab.Content as Grid).Children[0] as ScrollViewer).Content as WrapPanel;
		}

		public static int GetPositionElement(WrapPanel panel, string name)
		{
			var ID = 0;

			if (panel.Children.Count > 0)
			{
				foreach (StackPanel item in panel.Children)
				{
					if (string.Compare((item.DataContext as Radio.RadioFull).Name, name) == 0)
						return ID;
					ID++;
				}

				return -1;
			}

			return -1;
		}

		public static Image GetImage(StackPanel panel)
		{
			return panel.Children[0] as Image;
		}

		public static string GetName(StackPanel panel)
		{
			return (panel.Children[1] as Label).Content.ToString();
		}

		public static DependencyObject GetParentStack(object item)
		{
			if (item.GetType() == typeof(Label))
				return (item as Label).Parent;
			if (item.GetType() == typeof(Image))
				return (item as Image).Parent;
			if (item.GetType() == typeof(Button))
				return (item as Button).Parent;
			if (item.GetType() == typeof(Grid))
				return (item as Grid).Parent;
			return null;
		}

		public static WrapPanel GetWrapPanel(object item)
		{
			while (true)
				if (item == null)
				{
					return null;
				}
				else if (item.GetType() == typeof(WrapPanel))
				{
					return item as WrapPanel;
				}
				else
				{
					if (item.GetType() == typeof(Label))
						item = (item as Label).Parent;
					else if (item.GetType() == typeof(Grid))
						item = (item as Grid).Parent;
					else if (item.GetType() == typeof(Image))
						item = (item as Image).Parent;
					else if (item.GetType() == typeof(Button))
						item = (item as Button).Parent;
					else if (item.GetType() == typeof(StackPanel))
						item = (item as StackPanel).Parent;
				}
		}
	}
}