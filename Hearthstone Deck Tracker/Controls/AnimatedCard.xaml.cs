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
	}

	public void Update(Hearthstone.Card card, bool showTier7InspirationBtn = false)
	{
		DataContext = card;
		CardTileViewModel = new CardTileViewModel(card);
		BtnTier7Inspiration.Visibility = showTier7InspirationBtn ? Visibility.Visible : Visibility.Collapsed;
	}

	public void OnReuseFromPool()
	{
	}

	public void OnReturnToPool()
	{
		DataContext = null;
		CardTileViewModel = null;
		foreach(var sb in _runningStoryBoards.Values.ToList())
			sb.TrySetResult(false);
		_runningStoryBoards.Clear();
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

	private void BtnBgsInspiration_OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		Core.Overlay.BattlegroundsInspirationViewModel.SetKeyMinion(Card);
		Core.Overlay.ShowBgsInspiration();
		Core.Game.Metrics.BattlegroundsMinionsInspirationClicks++;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
