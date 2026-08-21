using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public class BattlegroundsGuidesTabsViewModel : ViewModel
{
	public ICommand ShowMinionsCommand => new Command(() =>
	{
		ToggleTab(Core.Overlay.BattlegroundsMinionsVM);
		Core.Game.Metrics.BattlegroundsCardsTabClicks++;
	});
	public ICommand ShowCompsCommand => new Command(() =>
	{
		ToggleTab(Core.Overlay.BattlegroundsCompsGuidesVM);
		Core.Game.Metrics.BattlegroundsCompsTabClicks++;
	});

	public ICommand ShowHeroesCommand => new Command(() =>
	{
		ToggleTab(Core.Overlay.BattlegroundsHeroGuideListViewModel);
		Core.Game.Metrics.BattlegroundsHeroesTabClicks++;
	});

	private void ToggleTab(ViewModel viewModel)
	{
		var isClosing = ActiveViewModel == viewModel;
		// the meta snapshot takes over the space the tab content just gave up, so fading it in there looks like a glitch
		AnimateMetaSnapshot = !isClosing;
		ActiveViewModel = isClosing ? null : viewModel;
		AnimateMetaSnapshot = true;
	}

	public bool AnimateMetaSnapshot
	{
		get => GetProp(true);
		private set => SetProp(value);
	}

	public ViewModel? ActiveViewModel
	{
		get => GetProp<ViewModel?>(null);
		set
		{
			SetProp(value);
			Core.Overlay.BattlegroundsMinionsVM.IsFiltersOpen = false;
			OnPropertyChanged(nameof(MetaSnapshotHasRoom));
			OnPropertyChanged(nameof(MetaSnapshotVisible));
		}
	}

	public ICommand MetaSnapshotCommand => new Command(() =>
	{
		var url = Helper.BuildHsReplayNetUrl("battlegrounds", "bgs_lobby_meta_snapshot", null, new[] { "meta-snapshot" });
		Helper.TryOpenUrl(url);
	});

	private bool _isInQueue;
	private bool _gameFound;

	public void OnQueueChanged(bool isInQueue, bool gameFound)
	{
		if(_isInQueue == isInQueue)
			return;
		_isInQueue = isInQueue;
		// the pre-lobby only goes away once the match has loaded in, so showing the meta snapshot as
		// soon as the queue ends would flash it for as long as the match takes to load
		_gameFound = !isInQueue && gameFound;
		OnPropertyChanged(nameof(MetaSnapshotVisible));
	}

	public bool MetaSnapshotVisible => IsPreLobby && !_isInQueue && !_gameFound && ActiveViewModel == null;

	// the meta snapshot sits where the tab content goes, so both of these hide it without animating
	public bool MetaSnapshotHasRoom => ActiveViewModel == null && HeroesTabVisible;

	public bool IsPreLobby
	{
		get => GetProp(false);
		set
		{
			SetProp(value);
			if(value)
				_gameFound = false;
			OnPropertyChanged(nameof(HeroesTabEnabled));
			OnPropertyChanged(nameof(MetaSnapshotVisible));
		}
	}

	// there are no heroes to guide before a match has started
	public bool HeroesTabEnabled => !IsPreLobby;

	// anything squarer than 16:10 has no room for the heroes tab or the meta snapshot notice
	private const double MinAspectRatio = 1680.0 / 1050.0;

	public double AspectRatio
	{
		get => GetProp(16.0 / 9.0);
		set
		{
			if(AspectRatio == value)
				return;
			SetProp(value);
			OnPropertyChanged(nameof(HeroesTabVisible));
			OnPropertyChanged(nameof(MetaSnapshotHasRoom));
		}
	}

	public bool HeroesTabVisible => AspectRatio >= MinAspectRatio;

	public bool HasQuests
	{
		get => GetProp(false);
		private set => SetProp(value);
	}

	public void Reset()
	{
		HasQuests = false;
	}

	public void OnQuestSelected(bool hasQuests)
	{
		HasQuests = hasQuests;
	}
}
