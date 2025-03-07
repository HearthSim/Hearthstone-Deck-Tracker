using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public class GuidesTabsViewModel : ViewModel
{
	public ICommand ShowMinionsCommand => new Command(() =>
	{
		ActiveViewModel = ActiveViewModel == Core.Overlay.BattlegroundsMinionsVM ? null : Core.Overlay.BattlegroundsMinionsVM;
		Core.Game.Metrics.BattlegroundsCardsTabClicks++;
	});
	public ICommand ShowCompsCommand => new Command(() =>
	{
		ActiveViewModel = ActiveViewModel == Core.Overlay.BattlegroundsCompsGuidesVM ? null : Core.Overlay.BattlegroundsCompsGuidesVM;
		Core.Game.Metrics.BattlegroundsCompsTabClicks++;
	});

	public ICommand ShowHeroesCommand => new Command(() =>
	{
		ActiveViewModel = ActiveViewModel == Core.Overlay.BattlegroundsHeroGuideListViewModel ? null : Core.Overlay.BattlegroundsHeroGuideListViewModel;
		Core.Game.Metrics.BattlegroundsHeroesTabClicks++;
	});

	public ViewModel? ActiveViewModel
	{
		get => GetProp<ViewModel?>(null);
		set
		{
			SetProp(value);
			Core.Overlay.BattlegroundsMinionsVM.IsFiltersOpen = false;
		}
	}
}
