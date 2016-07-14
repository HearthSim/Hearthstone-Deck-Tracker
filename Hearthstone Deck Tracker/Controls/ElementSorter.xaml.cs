#region

using System.Linq;
using Hearthstone_Deck_Tracker.Enums;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for ElementSorter.xaml
	/// </summary>
	public partial class ElementSorter
	{
		public bool IsPlayer;

		public ElementSorter()
		{
			InitializeComponent();
		}

		public void AddItem(ElementSorterItem item) => StackPanel.Children.Add(item);

		public void MoveItem(ElementSorterItem item, SortDirection direction)
		{
			var index = StackPanel.Children.IndexOf(item) + (direction == SortDirection.Up ? -1 : 1);

			if(index < 0)
				index = 0;
			else if(index > StackPanel.Children.Count - 1)
				index = StackPanel.Children.Count - 1;

			StackPanel.Children.Remove(item);
			StackPanel.Children.Insert(index, item);

			if(IsPlayer)
				Config.Instance.PanelOrderPlayer = StackPanel.Children.Cast<ElementSorterItem>().Select(x => x.ItemName).ToArray();
			else
				Config.Instance.PanelOrderOpponent = StackPanel.Children.Cast<ElementSorterItem>().Select(x => x.ItemName).ToArray();
		}
	}
}