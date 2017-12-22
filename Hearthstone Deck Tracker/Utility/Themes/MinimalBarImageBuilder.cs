using System.IO;
﻿using System.Drawing;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Hearthstone;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class MinimalBarImageBuilder : CardBarImageBuilder
	{
		public MinimalBarImageBuilder(Card card, string dir) : base(card, dir)
		{
			CreatedIconOffset = -15;
		}

		protected override void AddCardImage()
		{
			var bmp = ImageCache.GetCardBitmap(Card);
			if(bmp == null)
				return;
			var img = new GaussianBlur(bmp).Process(2);
			DrawingGroup.Children.Add(new ImageDrawing(img.ToImageSource(), FrameRect));
		}

		protected override void AddCountBox()
		{
		}

		protected override SolidColorBrush CountTextBrush
		{
			get
			{
				switch(Card.Rarity)
				{
					case Rarity.RARE:
						return new SolidColorBrush(Color.FromRgb(49, 134, 222));
					case Rarity.EPIC:
						return new SolidColorBrush(Color.FromRgb(173, 113, 247));
					case Rarity.LEGENDARY:
						return new SolidColorBrush(Color.FromRgb(255, 154, 16));
					default:
						return Brushes.White;
				}
			}
		}
	}
}
