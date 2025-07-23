using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class ArenaPickHelper
{
	public ArenaPickHelper()
	{
		InitializeComponent();
		SetupCardListDirectionWatcher();
		SetupBottomDirectionWatcher();
	}

	private readonly MouseWatcher _bottomDirectionWatcher = new(timeout: TimeSpan.FromMilliseconds(400), minDirectionDistance: 10);

	private void SetupBottomDirectionWatcher()
	{
		_bottomDirectionWatcher.OnTimeout += () =>
		{
			_ = _bottomDirectionWatcher.Stop();
			if(DataContext is ArenaPickHelperViewModel vm)
				vm.HoveringBottomDirectionTrigger = false;
		};
		_bottomDirectionWatcher.OnDirectionChange += (direction) =>
		{
			if((direction & MouseWatcher.Direction.Up) > 0)
			{
				_ = _bottomDirectionWatcher.Stop();
				if(DataContext is ArenaPickHelperViewModel vm)
					vm.HoveringBottomDirectionTrigger = false;
			}
		};
	}
	private async void BottomDirectionTrigger_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		await _bottomDirectionWatcher.Stop();
		vm.HoveringBottomDirectionTrigger = true;

		// The trigger area overlaps the card. Wait for the card to no longer be hovered
		// before starting the watcher. Otherwise it might time out while on the card.
		var actor = vm.HoveredChoice;
		while(vm.HoveredChoice == actor)
			await Task.Delay(16);

		_bottomDirectionWatcher.Start();
	}

	private async void BottomDirectionTrigger_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		vm.HoveringBottomDirectionTrigger = false;
		await _bottomDirectionWatcher.Stop();
	}

	private void Bottom_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if(DataContext is ArenaPickHelperViewModel vm && e is OverlayWindow.CustomMouseEventArgs)
			vm.HoveringPanel = true;
	}

	private void Bottom_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if(DataContext is ArenaPickHelperViewModel vm && e is OverlayWindow.CustomMouseEventArgs)
			vm.HoveringPanel = false;
	}

	private readonly MouseWatcher[] _cardListDirectionWatchers =
	{
		new (timeout: TimeSpan.FromMilliseconds(400), minDirectionDistance: 10),
		new (timeout: TimeSpan.FromMilliseconds(400), minDirectionDistance: 10),
		new (timeout: TimeSpan.FromMilliseconds(400), minDirectionDistance: 10),
	};

	private void SetupCardListDirectionWatcher()
	{
		for(var i = 0; i < _cardListDirectionWatchers.Length; i++)
		{
			var idx = i;
			_cardListDirectionWatchers[idx].OnTimeout += () =>
			{
				_ = _cardListDirectionWatchers[idx].Stop();
				if(DataContext is ArenaPickHelperViewModel vm)
					vm.HoveringCardListDirection(idx, false);
			};
			_cardListDirectionWatchers[idx].OnDirectionChange += direction =>
			{
				if((direction & MouseWatcher.Direction.Left) > 0)
				{
					_ = _cardListDirectionWatchers[idx].Stop();
					if(DataContext is ArenaPickHelperViewModel vm)
						vm.HoveringCardListDirection(idx, false);
				}
			};
		}
	}

	private void CardListDirectionTrigger0_OnMouseEnter(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseEnter(0);
	}

	private void CardListDirectionTrigger0_OnMouseLeave(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseLeave(0);
	}

	private void CardListDirectionTrigger1_OnMouseEnter(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseEnter(1);
	}

	private void CardListDirectionTrigger1_OnMouseLeave(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseLeave(1);
	}

	private void CardListDirectionTrigger2_OnMouseEnter(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseEnter(2);
	}

	private void CardListDirectionTrigger2_OnMouseLeave(object sender, MouseEventArgs e)
	{
		CardListDirectionTrigger_OnMouseLeave(2);
	}

	private async void CardListDirectionTrigger_OnMouseEnter(int index)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		await _cardListDirectionWatchers[index].Stop();
		vm.HoveringCardListDirection(index, true);

		// The trigger area overlaps the card. Wait for the card to no longer be hovered
		// before starting the watcher. Otherwise it might time out while on the card.
		var actor = vm.HoveredChoice;
		while(vm.HoveredChoice == actor)
			await Task.Delay(16);

		_cardListDirectionWatchers[index].Start();
	}

	private async void CardListDirectionTrigger_OnMouseLeave(int index)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		vm.HoveringCardListDirection(index, false);
		await _cardListDirectionWatchers[index].Stop();
	}

	private void CardListTrigger_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if(DataContext is ArenaPickHelperViewModel vm)
			vm.HoveringCardList = true;
	}

	private void CardListTrigger_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if(DataContext is ArenaPickHelperViewModel vm)
			vm.HoveringCardList = false;
	}

	private void BottomPanel_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		CardListScrollViewer.ScrollToTop();
	}

	private void CardTileImprovement_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		if(sender is not FrameworkElement { DataContext: DeckListTileViewModel { HoveredChoiceActor.Index: { } index } tileVm })
			return;
		if (vm.CardStats?.ElementAtOrDefault(index) is {} card && tileVm.HasTooltip)
			card.HighlightImprovements = true;
	}

	private void CardTileImprovement_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if(DataContext is not ArenaPickHelperViewModel vm)
			return;
		if(sender is not FrameworkElement { DataContext: DeckListTileViewModel { HoveredChoiceActor.Index: { } index } })
			return;
		if (vm.CardStats?.ElementAtOrDefault(index) is {} card)
			card.HighlightImprovements = false;
	}
}
