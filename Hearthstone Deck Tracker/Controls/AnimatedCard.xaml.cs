using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for AnimatedCard.xaml
	/// </summary>
	public partial class AnimatedCard
	{
		public AnimatedCard(Hearthstone.Card card, bool showTier7InspirationBtn = false)
		{
			InitializeComponent();
			DataContext = card;
			CoinCost.Visibility = Card.TypeEnum == CardType.BATTLEGROUND_SPELL ? Visibility.Visible : Visibility.Collapsed;
			BtnTier7Inspiration.Visibility = showTier7InspirationBtn ? Visibility.Visible : Visibility.Collapsed;
			Cost.Text = Card.Cost.ToString();
		}

		public Hearthstone.Card Card => (Hearthstone.Card)DataContext;
		public AnimatedCardViewModel ViewModel { get; } = new();

		public async Task FadeIn(bool fadeIn)
		{
			Card.Update();
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
				await RunStoryBoard("StoryboardUpdate");
			Card.Update();
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
			Card.Update();
		}

		private readonly List<string> _runningStoryBoards = new List<string>();
		public async Task RunStoryBoard(string key)
		{
			if(_runningStoryBoards.Contains(key))
				return;
			_runningStoryBoards.Add(key);
			var sb = (Storyboard)FindResource(key);
			sb.Begin();
			await Task.Delay(sb.Duration.TimeSpan);
			_runningStoryBoards.Remove(key);
		}

		private void BtnBgsInspiration_OnMouseUp(object sender, MouseButtonEventArgs e)
		{
			Core.Overlay.BattlegroundsInspirationViewModel.SetKeyMinion(Card);
			Core.Overlay.ShowBgsInspiration();
			Core.Game.Metrics.BattlegroundsMinionsInspirationClicks++;
		}
	}

	public class AnimatedCardViewModel : ViewModel
	{
		public HighlightColor Highlight
		{
			get => GetProp(HighlightColor.None);
			set => SetProp(value);
		}
	}
}
