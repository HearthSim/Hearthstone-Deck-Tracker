using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class ClassicBarImageBuilder : CardBarImageBuilder
	{
		private readonly Rect _frameCountBoxRect = new Rect(183, 0, 34, 34);
		private readonly Rect _imageRect = new Rect(108, 4, 108, 27);
		private readonly Rect _fadeRect = new Rect(28, 0, 189, 34);
		private readonly Rect _iconRect = new Rect(183, 0, 34, 34);

		public ClassicBarImageBuilder(Card card, string dir) : base(card, dir)
		{
			_card = card;
		}

		public override ImageBrush Build()
		{
			if(!_hasAllRequired)
				return new ImageBrush();

			AddCardImage();
			AddFadeOverlay();
			if(Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary)
			{
				AddCountBox();
				AddCountText();
			}
			if(_card.IsCreated)
				AddCreatedIcon();
			if(Math.Abs(_card.Count) <= 1 && _card.Rarity == Rarity.Legendary)
				AddLegendaryIcon();
			AddFrame();
			AddGem();
			if(_card.Count <= 0 || _card.Jousted)
				AddDarken();

			return new ImageBrush { ImageSource = new DrawingImage(_drawingGroup) };
		}

		protected override void AddCardImage()
		{
			var xOffset = Math.Abs(_card.Count) > 1
				|| _card.Rarity == Rarity.Legendary ? 19 : 0;
			var cardFile = Path.Combine(BarImageDir, _card.Id + ".png");
			if(File.Exists(cardFile))
				AddChild(cardFile, _imageRect.Move(-xOffset, 0));
		}

		protected override void AddFadeOverlay()
		{
			AddChild(_required[ThemeElement.FadeOverlay], _fadeRect);
		}

		protected override void AddCreatedIcon(int offset = 0)
		{
			if(Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary)
				AddChild(_required[ThemeElement.CreatedIcon],
					_iconRect.Move(-19, 0));
			else
				AddChild(_required[ThemeElement.CreatedIcon], _iconRect);
		}

		protected override void AddLegendaryIcon()
		{
			AddChild(_required[ThemeElement.LegendaryIcon], _iconRect);
		}

		protected override void AddCountBox()
		{
			var countBox = _required[ThemeElement.DefaultCountBox];
			if(Config.Instance.RarityCardFrames && _hasAllOptional)
			{
				switch(_card.Rarity)
				{
					case Rarity.Rare:
						countBox = _optional[ThemeElement.RareCountBox];
						break;

					case Rarity.Epic:
						countBox = _optional[ThemeElement.EpicCountBox];
						break;

					case Rarity.Legendary:
						countBox = _optional[ThemeElement.LegendaryCountBox];
						break;

					default:
						countBox = _optional[ThemeElement.CommonCountBox];
						break;
				}
			}
			AddChild(countBox, _frameCountBoxRect);
		}
	}
}