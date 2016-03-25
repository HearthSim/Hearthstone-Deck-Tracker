using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for AnimatedCard.xaml
	/// </summary>
	public partial class AnimatedCard
	{
		public AnimatedCard(Hearthstone.Card card)
		{
			InitializeComponent();
			DataContext = card;
		}

		public Hearthstone.Card Card => (Hearthstone.Card)DataContext;

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
	}
}
