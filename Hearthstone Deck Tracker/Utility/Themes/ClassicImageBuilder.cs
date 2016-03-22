using System;
using System.IO;
using System.Windows;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class ClassicBarImageBuilder : CardBarImageBuilder
	{
		private const int FadeOffset = -19;
		private readonly Rect _fadeRect = new Rect(28, 0, 189, 34);
		private readonly Rect _imageRect = new Rect(108, 4, 108, 27);
		private int ImageOffset => Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.Legendary? -19 : 0;

		public ClassicBarImageBuilder(Card card, string dir) : base(card, dir)
		{
			Card = card;
			CreatedIconOffset = -19;
		}

		protected override void AddFadeOverlay()
		{
			if(Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.Legendary)
				AddChild(Required[ThemeElement.FadeOverlay], _fadeRect.Move(FadeOffset, 0));
			else
				AddChild(Required[ThemeElement.FadeOverlay], _fadeRect);
		}

		protected override void AddCardImage()
		{
			var cardFile = Path.Combine(BarImageDir, Card.Id + ".png");
			if(File.Exists(cardFile))
				AddChild(cardFile, _imageRect.Move(ImageOffset, 0));
		}
	}
}