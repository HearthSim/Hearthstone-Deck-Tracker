#region

using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility;

#endregion

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class CardImageBuilder
	{
		private const string ThemeDir = "Images/Themes/Bars/classic/";
		private const string Fade = ThemeDir + "fade.png";
		private const string FrameDefault = ThemeDir + "frame.png";		
		private const string FrameGolden = ThemeDir + "frame_golden.png";
		private const string FrameCommon = ThemeDir + "frame_common.png";
		private const string FrameRare = ThemeDir + "frame_rare.png";
		private const string FrameEpic = ThemeDir + "frame_epic.png";
		private const string FrameLegendary = ThemeDir + "frame_legendary.png";
		private const string GemDefault = ThemeDir + "gem.png";
		private const string GemCommon = ThemeDir + "gem_common.png";
		private const string GemRare = ThemeDir + "gem_rare.png";
		private const string GemEpic = ThemeDir + "gem_epic.png";
		private const string GemLegendary = ThemeDir + "gem_legendary.png";		
		private const string FrameCountBox = ThemeDir + "countbox.png";
		private const string BoxCommon = ThemeDir + "countbox_common.png";
		private const string BoxRare = ThemeDir + "countbox_rare.png";
		private const string BoxEpic = ThemeDir + "countbox_epic.png";
		private const string BoxLegendary = ThemeDir + "countbox_legendary.png";
		private const string FrameCounterLegendary = ThemeDir + "icon_legendary.png";
		private const string CardCreatedIcon = ThemeDir + "icon_gift.png";
		private const string CardMarker = "card-marker.png";
		private const string DarkOverlay = ThemeDir + "dark.png";
		private readonly Card _card;
		private readonly DrawingGroup _drawingGroup = new DrawingGroup();
		private readonly Rect _frameCountBoxRect = new Rect(183, 0, 34, 34);
		private readonly Rect _frameCounterRect = new Rect(195, 7, 18, 21);
		private readonly Rect _frameRect = new Rect(0, 0, 217, 34);
		private readonly Rect _gemRect = new Rect(0, 0, 34, 34);
		private readonly Rect _imageRect = new Rect(108, 4, 108, 27);
		private readonly Rect _fadeRect = new Rect(28, 0, 189, 34);
		private readonly Rect _iconRect = new Rect(183, 0, 34, 34);		
		private readonly Rect _markerRect = new Rect(192, 8, 21, 21);
		private readonly Typeface _countType = new Typeface(
			new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT"),
			FontStyles.Normal, FontWeights.Normal, FontStretches.Condensed);

		public CardImageBuilder(Card card)
		{
			_card = card;
		}

		public ImageBrush Build()
		{
			_drawingGroup.Children.Clear();

			AddCardImage();
			AddFrame();
			if(Config.Instance.RarityCardGems)
				AddRarityGem();
			else
				AddChild(GemDefault, _gemRect);
			if(Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary)
				AddFrameCounter();
			if(_card.IsCreated)
				AddMarkers();
			if(_card.Count <= 0 || _card.Jousted )
				AddDarken();

			return new ImageBrush {ImageSource = new DrawingImage(_drawingGroup)};
		}

		private void AddDarken()
		{
			AddChild(DarkOverlay, _frameRect);
			if(_card.HighlightFrame)
				AddChild(FrameGolden, _frameRect);
		}

		private void AddMarkers()
		{
			var xOffset = Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary ? 16 : 0;
			AddChild(CardCreatedIcon, _iconRect.Move(-xOffset, 0));
		}

		private void AddFrameCounter()
		{
			if(Config.Instance.RarityCardFrames)
			{
				switch (_card.Rarity)
				{
					case Rarity.Common:
					case Rarity.Free:
						AddChild(BoxCommon, _frameCountBoxRect);
						break;
					case Rarity.Rare:
						AddChild(BoxRare, _frameCountBoxRect);
						break;
					case Rarity.Epic:
						AddChild(BoxEpic, _frameCountBoxRect);
						break;
					case Rarity.Legendary:
						AddChild(BoxLegendary, _frameCountBoxRect);
						break;
					default:
						AddChild(FrameCountBox, _frameCountBoxRect);
						break;
				}
			}
			else
				AddChild(FrameCountBox, _frameCountBoxRect);

			var count = Math.Abs(_card.Count);
			if(count <= 1 && _card.Rarity == Rarity.Legendary)
				AddChild(FrameCounterLegendary, _frameCountBoxRect);
			else
			{
				var countText = count > 9 ? "9" : count.ToString();
				AddText(countText, 20, 198, 4);
				if(count > 9)
					AddText("+", 13, 203, 3);
			}
		}

		private void AddRarityGem()
		{
			switch(_card.Rarity)
			{
				case Rarity.Rare:
					AddChild(GemRare, _gemRect);
					break;
				case Rarity.Epic:
					AddChild(GemEpic, _gemRect);
					break;
				case Rarity.Legendary:
					AddChild(GemLegendary, _gemRect);
					break;
				default:
					AddChild(GemCommon, _gemRect);
					break;
			}
		}

		private void AddFrame()
		{
			var frame = FrameDefault;
			if(_card.HighlightFrame)
			{
				frame = FrameGolden;
				_card.IsFrameHighlighted = true;
			}
			else
			{
				_card.IsFrameHighlighted = false;
				if(Config.Instance.RarityCardFrames)
				{
					switch(_card.Rarity)
					{
						case Rarity.Free:
						case Rarity.Common:
							frame = FrameCommon;
							break;
						case Rarity.Rare:
							frame = FrameRare;
							break;
						case Rarity.Epic:
							frame = FrameEpic;
							break;
						case Rarity.Legendary:
							frame = FrameLegendary;
							break;
					}
				}
			}
			AddChild(frame, _frameRect);
		}

		private void AddText(object obj, int size, int x, int y)
		{
			var text = new FormattedText(obj.ToString(), CultureInfo.GetCultureInfo("en-us"),
						FlowDirection.LeftToRight, _countType, size, Brushes.White);
			var point = new Point(x, y);
			_drawingGroup.Children.Add(new GeometryDrawing(Brushes.Black, 
				new Pen(Brushes.Black, 2.0), text.BuildGeometry(point)));
			_drawingGroup.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(240,195,72)), 
				new Pen(Brushes.White, 0), text.BuildGeometry(point)));
		}

		private void AddCardImage()
		{
			var xOffset = Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary ? 19 : 0;
			var cardFileName = _card.Id + ".png";
			if(File.Exists("Images/Bars/" + cardFileName))
				AddChild("Images/Bars/" + cardFileName, _imageRect.Move(-xOffset, 0));

			AddChild(Fade, _fadeRect.Move(-xOffset, 0));
		}

		private void AddChild(string uri, Rect rect)
			=> _drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(uri, UriKind.Relative)), rect));
	}

	public static class RectExtensions
	{
		public static Rect Move(this Rect rect, int x, int y) => new Rect(rect.X + x, rect.Y + y, rect.Width, rect.Height);
	}
}