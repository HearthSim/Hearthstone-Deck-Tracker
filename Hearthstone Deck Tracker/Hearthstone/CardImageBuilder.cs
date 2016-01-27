#region

using System;
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
		private const string FrameDefault = "Images/frame.png";
		private const string FrameGolden = "Images/frame_golden.png";
		private const string FrameCommon = "Images/frame_rarity_common.png";
		private const string FrameRare = "Images/frame_rarity_rare.png";
		private const string FrameEpic = "Images/frame_rarity_epic.png";
		private const string FrameLegendary = "Images/frame_rarity_legendary.png";
		private const string GemCommon = "Images/gem_rarity_common.png";
		private const string GemRare = "Images/gem_rarity_rare.png";
		private const string GemEpic = "Images/gem_rarity_epic.png";
		private const string GemLegendary = "Images/gem_rarity_legendary.png";
		private const string FrameCountBox = "Images/frame_countbox.png";
		private const string FrameCounterLegendary = "Images/frame_legendary.png";
		private const string CardCreatedIcon = "card-icon-created.png";
		private const string CardMarker = "card-marker.png";
		private const string DarkOverlay = "Images/dark.png";
		private readonly Card _card;
		private readonly DrawingGroup _drawingGroup = new DrawingGroup();
		private readonly Rect _frameCountBoxRect = new Rect(189, 6, 25, 24);
		private readonly Rect _frameCounterRect = new Rect(194, 8, 18, 21);
		private readonly Rect _frameRect = new Rect(0, 0, 218, 35);
		private readonly Rect _gemRect = new Rect(3, 3, 28, 28);
		private readonly Rect _iconRect = new Rect(194, 9, 16, 16);
		private readonly Rect _imageRect = new Rect(104, 1, 110, 34);
		private readonly Rect _markerRekt = new Rect(192, 8, 21, 21);

		public CardImageBuilder(Card card)
		{
			_card = card;
		}

		private string FrameCounterNumber => $"Images/frame_{Math.Abs(_card.Count)}.png";

		public ImageBrush Build()
		{
			_drawingGroup.Children.Clear();

			AddCardImage();
			AddFrame();
			if(Config.Instance.RarityCardGems)
				AddRarityGem();
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
			if (_card.HighlightFrame)
				AddChild(FrameGolden, _frameRect);
		}

		private void AddMarkers()
		{
			var xOffset = Math.Abs(_card.Count) > 1 || _card.Rarity == Rarity.Legendary ? 23 : 3;
			_drawingGroup.Children.Add(new ImageDrawing(ImageCache.GetImage(CardMarker, "Images"), _markerRekt.Move(-xOffset, 0)));
			_drawingGroup.Children.Add(new ImageDrawing(ImageCache.GetImage(CardCreatedIcon, "Images"), _iconRect.Move(-xOffset, 0)));
		}

		private void AddFrameCounter()
		{
			AddChild(FrameCountBox, _frameCountBoxRect);
			if(Math.Abs(_card.Count) > 1 && Math.Abs(_card.Count) <= 9)
				AddChild(FrameCounterNumber, _frameCounterRect);
			else
				AddChild(FrameCounterLegendary, _frameCounterRect);
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

		private void AddCardImage()
		{
			var cardFileName = _card.CardFileName + ".png";
			if(File.Exists("Images/" + cardFileName))
				AddChild("Images/" + cardFileName, _imageRect);
		}

		private void AddChild(string uri, Rect rect)
			=> _drawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(uri, UriKind.Relative)), rect));
	}

	public static class RectExtensions
	{
		public static Rect Move(this Rect rect, int x, int y) => new Rect(rect.X + x, rect.Y + y, rect.Width, rect.Height);
	}
}