using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public abstract class CardBarImageBuilder
	{
		protected const string BarImageDir = "Images/Bars";

		protected Card Card;
		protected string ThemeDir;
		protected DrawingGroup DrawingGroup = new DrawingGroup();
		protected bool HasAllRequired;
		protected bool HasAllOptionalFrames;
		protected bool HasAllOptionalGems;
		protected bool HasAllOptionalCountBoxes;
		protected int CreatedIconOffset = -23;
		protected readonly Typeface TextTypeFace;
		protected readonly Typeface NumbersTypeFace = new Typeface(new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT"), FontStyles.Normal,
										  FontWeights.Normal, FontStretches.Condensed);

		protected static readonly Rect FrameRect = new Rect(0, 0, 217, 34);
		protected static readonly Rect GemRect = new Rect(0, 0, 34, 34);
		protected static readonly Rect BoxRect = new Rect(183, 0, 34, 34);
		protected static readonly Rect ImageRect = new Rect(83, 0, 134, 34);

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> Required = new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ThemeElement.DefaultFrame, new ThemeElementInfo("frame.png", FrameRect)},
			{ThemeElement.DefaultGem, new ThemeElementInfo("gem.png", GemRect)},
			{ThemeElement.DefaultCountBox, new ThemeElementInfo("countbox.png", BoxRect)},
			{ThemeElement.DarkOverlay, new ThemeElementInfo("dark.png", FrameRect)},
			{ThemeElement.FadeOverlay, new ThemeElementInfo("fade.png", FrameRect)},
			{ThemeElement.CreatedIcon, new ThemeElementInfo("icon_created.png", BoxRect)},
			{ThemeElement.LegendaryIcon, new ThemeElementInfo("icon_legendary.png", BoxRect)}
		};

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> OptionalFrame = new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ThemeElement.CommonFrame, new ThemeElementInfo("frame_common.png", FrameRect)},
			{ThemeElement.RareFrame, new ThemeElementInfo("frame_rare.png", FrameRect)},
			{ThemeElement.EpicFrame, new ThemeElementInfo("frame_epic.png", FrameRect)},
			{ThemeElement.LegendaryFrame, new ThemeElementInfo("frame_legendary.png", FrameRect)},
		};

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> OptionalGems = new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ThemeElement.CommonGem, new ThemeElementInfo("gem_common.png", GemRect)},
			{ThemeElement.RareGem, new ThemeElementInfo("gem_rare.png", GemRect)},
			{ThemeElement.EpicGem, new ThemeElementInfo("gem_epic.png", GemRect)},
			{ThemeElement.LegendaryGem, new ThemeElementInfo("gem_legendary.png", GemRect)},
		};

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> OptionalCountBoxes = new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ThemeElement.CommonCountBox, new ThemeElementInfo("countbox_common.png", BoxRect)},
			{ThemeElement.RareCountBox, new ThemeElementInfo("countbox_rare.png", BoxRect)},
			{ThemeElement.EpicCountBox, new ThemeElementInfo("countbox_epic.png", BoxRect)},
			{ThemeElement.LegendaryCountBox, new ThemeElementInfo("countbox_legendary.png", BoxRect)}
		};

		protected CardBarImageBuilder(Card card, string dir)
		{
			Card = card;
			ThemeDir = dir;
			TextTypeFace = Helper.LatinLanguages.Contains(Config.Instance.SelectedLanguage)
							   ? NumbersTypeFace : new Typeface(new FontFamily(), FontStyles.Normal, FontWeights.Bold, FontStretches.Condensed);
			HasAllRequired = Required.All(x => File.Exists(Path.Combine(ThemeDir, x.Value.FileName)));
			HasAllOptionalFrames = OptionalFrame.All(x => File.Exists(Path.Combine(ThemeDir, x.Value.FileName)));
			HasAllOptionalGems = OptionalGems.All(x => File.Exists(Path.Combine(ThemeDir, x.Value.FileName)));
			HasAllOptionalCountBoxes = OptionalCountBoxes.All(x => File.Exists(Path.Combine(ThemeDir, x.Value.FileName)));
		}

		public virtual ImageBrush Build()
		{
			DrawingGroup.Children.Clear();

			if(!HasAllRequired)
				return new ImageBrush();

			AddCardImage();
			AddFadeOverlay();
			if(Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.Legendary)
			{
				AddCountBox();
				AddCountText();
			}
			if(Card.IsCreated)
				AddCreatedIcon();
			if(Math.Abs(Card.Count) <= 1 && Card.Rarity == Rarity.Legendary)
				AddLegendaryIcon();
			AddFrame();
			AddGem();
			AddCost();
			AddCardName();
			if(Card.Count <= 0 || Card.Jousted)
				AddDarken();

			return new ImageBrush { ImageSource = new DrawingImage(DrawingGroup) };
		}

		protected virtual void AddCardImage()
		{
			var cardFile = Path.Combine(BarImageDir, Card.Id + ".png");
			if(File.Exists(cardFile))
				AddChild(cardFile, ImageRect);
		}

		protected virtual void AddFadeOverlay() => AddChild(Required[ThemeElement.FadeOverlay]);

		protected virtual void AddFrame()
		{
			var frame = Required[ThemeElement.DefaultFrame];
			if(Config.Instance.RarityCardFrames && HasAllOptionalFrames)
			{
				switch(Card.Rarity)
				{
					case Rarity.Rare:
						frame = OptionalFrame[ThemeElement.RareFrame];
						break;

					case Rarity.Epic:
						frame = OptionalFrame[ThemeElement.EpicFrame];
						break;

					case Rarity.Legendary:
						frame = OptionalFrame[ThemeElement.LegendaryFrame];
						break;

					default:
						frame = OptionalFrame[ThemeElement.CommonFrame];
						break;
				}
			}
			AddChild(frame);
		}

		protected virtual void AddGem()
		{
			var gem = Required[ThemeElement.DefaultGem];
			if(Config.Instance.RarityCardGems && HasAllOptionalGems)
			{
				switch(Card.Rarity)
				{
					case Rarity.Rare:
						gem = OptionalGems[ThemeElement.RareGem];
						break;

					case Rarity.Epic:
						gem = OptionalGems[ThemeElement.EpicGem];
						break;

					case Rarity.Legendary:
						gem = OptionalGems[ThemeElement.LegendaryGem];
						break;

					default:
						gem = OptionalGems[ThemeElement.CommonGem];
						break;
				}
			}
			AddChild(gem);
		}

		protected virtual void AddDarken() => AddChild(Required[ThemeElement.DarkOverlay]);

		protected virtual void AddCountBox()
		{
			var countBox = Required[ThemeElement.DefaultCountBox];
			if(Config.Instance.RarityCardFrames && HasAllOptionalCountBoxes)
			{
				switch(Card.Rarity)
				{
					case Rarity.Rare:
						countBox = OptionalCountBoxes[ThemeElement.RareCountBox];
						break;

					case Rarity.Epic:
						countBox = OptionalCountBoxes[ThemeElement.EpicCountBox];
						break;

					case Rarity.Legendary:
						countBox = OptionalCountBoxes[ThemeElement.LegendaryCountBox];
						break;

					default:
						countBox = OptionalCountBoxes[ThemeElement.CommonCountBox];
						break;
				}
			}
			AddChild(countBox);
		}

		protected virtual SolidColorBrush CountTextBrush => new SolidColorBrush(Color.FromRgb(240, 195, 72));

		protected virtual void AddCountText()
		{
			var count = Math.Abs(Card.Count);
			if(count > 1)
			{
				var countText = count > 9 ? "9" : count.ToString();
				AddText(countText, 20, new Rect(198, 4, double.NaN, double.NaN), CountTextBrush, NumbersTypeFace);
				if(count > 9)
					AddText("+", 13, new Rect(203, 3, double.NaN, double.NaN), CountTextBrush, TextTypeFace);
			}
		}

		protected virtual void AddCreatedIcon()
		{
			if(Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.Legendary)
				AddChild(Required[ThemeElement.CreatedIcon], Required[ThemeElement.CreatedIcon].Rectangle.Move(CreatedIconOffset, 0));
			else
				AddChild(Required[ThemeElement.CreatedIcon]);
		}

		protected virtual void AddLegendaryIcon()
		{
			AddChild(Required[ThemeElement.LegendaryIcon]);
		}

		protected virtual void AddCost() => AddText(Card.Cost, 22, new Rect(6, 0, 25, 34), Card.ColorPlayer, NumbersTypeFace, true);

		protected virtual void AddCardName()
			=> AddText(Card.LocalizedName, 14, new Rect(38, 8, FrameRect.Width - BoxRect.Width - 38, 34), Card.ColorPlayer, TextTypeFace);

		protected virtual void AddText(object obj, int size, Rect rect, Brush fill, Typeface typeface, bool centered = false)
		{
			foreach(var d in CardTextImageBuilder.GetOutlinedText(obj.ToString(), size, rect, fill, Brushes.Black, typeface, centered: centered))
				DrawingGroup.Children.Add(d);
		}

		protected void AddChild(string uri, Rect rect)
			=> DrawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(uri, UriKind.Relative)), rect));

		protected void AddChild(ThemeElementInfo element)
		{
			DrawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(
				Path.Combine(ThemeDir, element.FileName), UriKind.Relative)), element.Rectangle));
		}

		protected void AddChild(ThemeElementInfo element, Rect overwrite)
		{
			DrawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(
				Path.Combine(ThemeDir, element.FileName), UriKind.Relative)), overwrite));
		}
	}
}