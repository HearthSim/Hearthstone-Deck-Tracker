using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Hearthstone;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using HearthDb.Enums;
using SixLabors.ImageSharp;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class MinimalBarImageBuilder : CardBarImageBuilder
	{
	    public MinimalBarImageBuilder(Card card, string dir, double pixelsPerDip) : base(card, dir, pixelsPerDip)
		{
			CreatedIconOffset = -15;
		}

		protected override void AddCardImage()
		{
			var bmp = ImageCache.GetCardBitmap(Card);
			if(bmp == null)
				return;
            bmp.Mutate(context => {
              context.GaussianBlur(2);
            });
			DrawingGroup.Children.Add(new ImageDrawing(bmp.ToImageSource(), FrameRect));
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