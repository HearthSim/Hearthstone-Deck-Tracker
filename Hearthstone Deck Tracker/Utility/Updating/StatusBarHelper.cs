using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Utility.Updating;

public class StatusBarHelper : ViewModel
{
	public Visibility Visibility
	{
		get => GetProp(Visibility.Collapsed);
		set => SetProp(value);
	}
}
