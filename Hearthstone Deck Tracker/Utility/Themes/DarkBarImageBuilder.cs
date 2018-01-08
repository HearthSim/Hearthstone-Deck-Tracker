using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class DarkBarImageBuilder : CardBarImageBuilder
	{
		private readonly Rect _fadeRect = new Rect(34, 0, 183, 34);
		public DarkBarImageBuilder(Card card, string dir) : base(card, dir)
		{
		}

		protected override void AddFadeOverlay() => AddFadeOverlay(_fadeRect, true);

		protected override void AddCardImage() => AddCardImage(ImageRect, true);

		protected override void AddCountText() => AddCountText(CountTextRect.Move(2, 0));
	}
}
