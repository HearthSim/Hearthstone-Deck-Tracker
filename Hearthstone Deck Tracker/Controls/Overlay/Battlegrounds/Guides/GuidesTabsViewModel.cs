using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public class GuidesTabsViewModel : ViewModel
{
	public ICommand ShowMinionsCommand => new Command(() => ActiveViewModel = ActiveViewModel == Core.Overlay.BattlegroundsMinionsVM ?  null : Core.Overlay.BattlegroundsMinionsVM);
	public ICommand ShowCompsCommand => new Command(() => ActiveViewModel = ActiveViewModel == Core.Overlay.BattlegroundsCompsGuidesVM ?  null : Core.Overlay.BattlegroundsCompsGuidesVM);
	// public ICommand ShowHeroesCommand => new Command(() => ShowContent(ContentType.Heroes));

	public ViewModel? ActiveViewModel
	{
		get => GetProp<ViewModel?>(null);
		set => SetProp(value);
	}
}
