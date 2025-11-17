using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Utility;

namespace Hearthstone_Deck_Tracker.Controls;

public partial class AnimatedCard : IPoolItem, INotifyPropertyChanged
{
	public AnimatedCard()
	{
		InitializeComponent();
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged += OnPinsChanged;
	}

	private void OnPinsChanged(object? sender, EventArgs e)
	{
		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var isPinned = Card?.Id != null && Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(Card.Id);
		OnPropertyChanged(nameof(IsPinned));

		if(BtnPinMinion.Visibility != Visibility.Visible)
			return;

		var storyboardKey = isPinned ? "PinButtonPinned" : "PinButtonUnpinned";
		RunStoryBoardNonBlocking(storyboardKey);
	}

	public void Update(Hearthstone.Card card, bool showTier7InspirationBtn = false)
	{
		DataContext = card;
		CardTileViewModel = new CardTileViewModel(card);
		BtnTier7Inspiration.Visibility = showTier7InspirationBtn ? Visibility.Visible : Visibility.Collapsed;
		UpdatePinButtonState();
	}

	public void OnReuseFromPool()
	{
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged += OnPinsChanged;
	}

	public void OnReturnToPool()
	{
		DataContext = null;
		CardTileViewModel = null;
		foreach(var sb in _runningStoryBoards.Values.ToList())
			sb.TrySetResult(false);
		_runningStoryBoards.Clear();
		Core.Overlay.BattlegroundsMinionPinningViewModel.PinsChanged -= OnPinsChanged;
	}

	public Hearthstone.Card? Card => DataContext as Hearthstone.Card;

	private CardTileViewModel? _cardTileViewModel;
	public CardTileViewModel? CardTileViewModel
	{
		get => _cardTileViewModel;
		private set
		{
			if(_cardTileViewModel == value)
				return;
			_cardTileViewModel = value;
			OnPropertyChanged();
		}
	}

	private bool _showPinButton;
	public bool ShowPinButton
	{
		get => _showPinButton;
		set
		{
			if(_showPinButton == value)
				return;
			_showPinButton = value;
			OnPropertyChanged();
			BtnPinMinion.Visibility = _showPinButton ? Visibility.Visible : Visibility.Collapsed;
			UpdatePinButtonState();
		}
	}

	public bool IsPinned
	{
		get => Card?.Id != null && Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(Card.Id);
	}

	public async Task FadeIn(bool fadeIn)
	{
		Card?.Update();
		if(fadeIn && Config.Instance.OverlayCardAnimations)
		{
			if(Config.Instance.OverlayCardAnimationsOpacity)
				await RunStoryBoard("StoryboardFadeIn");
			else
				await RunStoryBoard("StoryboardFadeInNoOpacity");
		}
	}

	public async Task FadeOut(bool highlight)
	{
		if(highlight && Config.Instance.OverlayCardAnimations)
		{
			var complete = await RunStoryBoard("StoryboardUpdate");
			if(!complete)
				return;
		}
		Card?.Update();
		if(Config.Instance.OverlayCardAnimations)
		{
			if(Config.Instance.OverlayCardAnimationsOpacity)
				await RunStoryBoard("StoryboardFadeOut");
			else
				await RunStoryBoard("StoryboardFadeOutNoOpacity");
		}
	}

	public async Task Update(bool highlight)
	{
		if(highlight && Config.Instance.OverlayCardAnimations)
			await RunStoryBoard("StoryboardUpdate");
		Card?.Update();
	}

	private readonly Dictionary<string, TaskCompletionSource<bool>> _runningStoryBoards = new();

	private async Task<bool> RunStoryBoard(string key)
	{
		if(_runningStoryBoards.ContainsKey(key))
			return false;
		if(CardTileViewModel == null)
		{
			// The card was returned to the pool and we should not play any new animations
			return false;
		}
		var sb = (Storyboard)FindResource(key);
		var tcs = new TaskCompletionSource<bool>();
		_runningStoryBoards[key] = tcs;
		sb.Completed += (_, __) => tcs.TrySetResult(true);
		sb.Begin();
		var completed = await tcs.Task;
		sb.Remove();
		_runningStoryBoards.Remove(key);
		return completed;
	}

	private void RunStoryBoardNonBlocking(string key)
	{
		try
		{
			var sb = (Storyboard)FindResource(key);
			sb.Begin();
		}
		catch(Exception e)
		{
			// ignored
		}
	}

	private void BtnBgsInspiration_OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		Core.Overlay.BattlegroundsInspirationViewModel.SetKeyMinion(Card);
		Core.Overlay.ShowBgsInspiration();
		Core.Game.Metrics.BattlegroundsMinionsInspirationClicks++;
	}

	private void BtnPinMinion_OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if(Card?.Id is not { } id)
			return;
		Core.Overlay.BattlegroundsMinionPinningViewModel.TogglePinCard(id);
		// Don't call UpdatePinButtonState() here - the PinsChanged event will handle it
		Core.Game.Metrics.TavernMarkersPinnedFromAnimatedCard = true;
	}

	private void UpdatePinButtonState()
	{
		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var isPinned = Card?.Id != null && Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(Card.Id);
		OnPropertyChanged(nameof(IsPinned));

		// Use storyboard to set the initial state
		var storyboardKey = (isPinned && BtnPinMinion.Visibility == Visibility.Visible) ? "PinButtonPinned" : "PinButtonHidden";
		RunStoryBoardNonBlocking(storyboardKey);
	}

	private void Grid_OnMouseEnter(object sender, MouseEventArgs e)
	{
		AnimateButtonsIn();
	}

	private void Grid_OnMouseLeave(object sender, MouseEventArgs e)
	{
		AnimateButtonsOut();
	}

	private void AnimateButtonsIn()
	{
		RunStoryBoardNonBlocking("InspirationButtonIn");

		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		var isPinned = Card?.Id != null && Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(Card.Id);

		if(isPinned)
		{
			// For pinned cards, show immediately without animation
			RunStoryBoardNonBlocking("PinButtonPinned");
		}
		else if(BtnPinMinion.Visibility == Visibility.Visible)
		{
			// For non-pinned cards, animate in
			RunStoryBoardNonBlocking("PinButtonIn");
		}
	}

	private void AnimateButtonsOut()
	{
		RunStoryBoardNonBlocking("InspirationButtonOut");

		if(Core.Overlay.BgsMinionPinningVisibility != Visibility.Visible)
			return;

		if(BtnPinMinion.Visibility != Visibility.Visible)
			return;

		var isPinned = Card?.Id != null && Core.Overlay.BattlegroundsMinionPinningViewModel.IsCardPinned(Card.Id);

		if(isPinned)
		{
			// Keep pinned button at full opacity and scale
			RunStoryBoardNonBlocking("PinButtonPinned");
		}
		else
		{
			// Animate out for non-pinned cards
			RunStoryBoardNonBlocking("PinButtonOut");
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void AnimatedCard_OnLoaded(object sender, RoutedEventArgs e)
	{
		OnPinsChanged(null, EventArgs.Empty);
	}
}
