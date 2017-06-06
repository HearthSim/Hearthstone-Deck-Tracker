using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class FrostBarImageBuilder : CardBarImageBuilder
	{
		public FrostBarImageBuilder(Card card, string dir) : base(card, dir)
		{
		}

		protected override void AddCardImage() => AddCardImage(ImageRect.Move(-1, 0), false);

		protected override void AddCountText() => AddCountText(CountTextRect.Move(1, 0));

		protected override void AddLegendaryIcon() => AddLegendaryIcon(BoxRect.Move(-1,0));
	}
}
