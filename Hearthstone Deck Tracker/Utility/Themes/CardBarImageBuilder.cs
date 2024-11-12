using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using HearthDb.Enums;
using ColorConverter = System.Windows.Media.ColorConverter;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public abstract class CardBarImageBuilder
	{
		protected Card Card;
		protected string ThemeDir;
		protected DrawingGroup DrawingGroup = new DrawingGroup();
		protected bool HasAllRequired;
		protected bool HasAllOptionalFrames;
		protected bool HasAllOptionalGems;
		protected bool HasAllOptionalCountBoxes;
		protected int CreatedIconOffset = -23;
		protected int FadeOffset = -23;
		protected int ImageOffset = -23;
		protected Typeface TextTypeFace;
		protected Typeface NumbersTypeFace = new Typeface(new FontFamily(new Uri("pack://application:,,,/"), "./resources/#Chunkfive"), FontStyles.Normal,
										  FontWeights.Normal, FontStretches.Condensed);

		protected double CountFontSize = 17;
		protected double MulliganWinRateFontSize = 15;
		protected double TextFontSize = 13;
		protected double CostFontSize = 20;

		protected static readonly Rect FrameRect = new Rect(0, 0, 217, 34);
		protected static readonly Rect GemRect = new Rect(0, 0, 34, 34);
		protected static readonly Rect BoxRect = new Rect(183, 0, 34, 34);
		protected static readonly Rect MulliganWinrateBoxRect = new Rect(136, 4, 54, 26);
		protected static readonly Rect ImageRect = new Rect(83, 0, 134, 34);
		protected static readonly Rect CountTextRect = new Rect(196, 0, 14, 34);
		protected static readonly Rect CostTextRect = new Rect(0, 0, 34, 34);

		protected static readonly Dictionary<ThemeElement, ThemeElementInfo> Required = new Dictionary<ThemeElement, ThemeElementInfo>
		{
			{ThemeElement.DefaultFrame, new ThemeElementInfo("frame.png", FrameRect)},
			{ThemeElement.DefaultGem, new ThemeElementInfo("gem.png", GemRect)},
			{ThemeElement.DefaultCountBox, new ThemeElementInfo("countbox.png", BoxRect)},
			{ThemeElement.DefaultStatsBox, new ThemeElementInfo("stats_box.png", MulliganWinrateBoxRect)},
			{ThemeElement.DefaultStatsActiveBox, new ThemeElementInfo("stats_box_active.png", MulliganWinrateBoxRect)},
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

		public virtual DrawingBrush Build(Action<bool>? onCardImageLoaded = null)
		{
			DrawingGroup = new DrawingGroup();

			if(!HasAllRequired)
				return new DrawingBrush();

			AddCardImage(onCardImageLoaded);
			AddFadeOverlay();
			if(Math.Abs(Card.Count) > 1 || Card is { Rarity: Rarity.LEGENDARY, BaconCard: false })
			{
				AddCountBox();
				AddCountText();
			}
			if(Card.IsCreated)
				AddCreatedIcon();
			if(Math.Abs(Card.Count) <= 1 && Card is { Rarity: Rarity.LEGENDARY, BaconCard: false })
				AddLegendaryIcon();
			AddFrame();
			AddGem();
			AddCost();
			AddCardName();
			if(Card.Count <= 0 || Card.Jousted)
				AddDarken();
			if(Card.CardWinrates != null)
			{
				AddMulliganWinRate();
				AddMulliganWinRateText();
			}

			return new DrawingBrush(DrawingGroup);
		}

		protected virtual void AddCardImage(Action<bool>? onCardImageLoaded) => AddCardImage(ImageRect, false, onCardImageLoaded);
		protected void AddCardImage(Rect rect, bool offsetByCountBox, Action<bool>? onCardImageLoaded)
		{
			var bmp = GetCardTile(onCardImageLoaded);
			if(bmp == null)
				return;
			if(offsetByCountBox && (Math.Abs(Card.Count) > 1 || Card is { Rarity: Rarity.LEGENDARY, BaconCard: false }))
				AddChild(bmp, rect.Move(ImageOffset, 0));
			else
				AddChild(bmp, rect);
		}

		protected BitmapSource? GetCardTile(Action<bool>? onCardImageLoaded)
		{
			var downloader = AssetDownloaders.cardTileDownloader;
			if(downloader == null)
				return null;

			var bmp = downloader.TryGetAssetData(Card);
			if(bmp == null)
			{
				// HasAsset kicks off the download.
				// if the download succeeds before we DownloadAsset, DownloadAsset might fail.
				async void LoadAsset()
				{
					try
					{
						var data = await downloader!.GetAssetData(Card);
						onCardImageLoaded?.Invoke(data != null);
					}
					catch(Exception ex)
					{
						Log.Error(ex);
						onCardImageLoaded?.Invoke(false);
						// Pass. Guess we can't render the image.
					}
				};
				LoadAsset();
			}

			if(bmp != null && Card.BaconCard)
			{
				// Battlegrounds cards have more "white" on the left side of
				// the image. Zoom in to not show that.
				const double zoom = 1.25;
				var zoomedHeight = bmp.Height / zoom;
				var zoomedWidth = bmp.Width / zoom;
				var crop = new Int32Rect(
					(int)(bmp.Width - zoomedWidth), // right aligned
					(int)((bmp.Height - zoomedHeight) / 2), // vertical center
					(int)zoomedWidth,
					(int)zoomedHeight);
				return new CroppedBitmap(bmp, crop);
			}

			return bmp ?? downloader.PlaceholderAsset;
		}

		protected virtual void AddFadeOverlay() => AddFadeOverlay(Required[ThemeElement.FadeOverlay].Rectangle, false);
		protected virtual void AddFadeOverlay(Rect rect, bool offsetByCountBox)
		{
			if(offsetByCountBox && (Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.LEGENDARY))
				AddChild(Required[ThemeElement.FadeOverlay], rect.Move(FadeOffset, 0));
			else
				AddChild(Required[ThemeElement.FadeOverlay], rect);
		}

		protected virtual void AddFrame()
		{
			var frame = Required[ThemeElement.DefaultFrame];
			if(Config.Instance.RarityCardFrames && HasAllOptionalFrames)
			{
				switch(Card.Rarity)
				{
					case Rarity.RARE:
						frame = OptionalFrame[ThemeElement.RareFrame];
						break;

					case Rarity.EPIC:
						frame = OptionalFrame[ThemeElement.EpicFrame];
						break;

					case Rarity.LEGENDARY:
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
					case Rarity.RARE:
						gem = OptionalGems[ThemeElement.RareGem];
						break;

					case Rarity.EPIC:
						gem = OptionalGems[ThemeElement.EpicGem];
						break;

					case Rarity.LEGENDARY:
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
					case Rarity.RARE:
						countBox = OptionalCountBoxes[ThemeElement.RareCountBox];
						break;

					case Rarity.EPIC:
						countBox = OptionalCountBoxes[ThemeElement.EpicCountBox];
						break;

					case Rarity.LEGENDARY:
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

		protected virtual void AddCountText() => AddCountText(CountTextRect);
		protected void AddCountText(Rect rect)
		{
			var count = Math.Abs(Card.Count);
			if(count <= 1)
				return;
			AddText(count, CountFontSize, rect, CountTextBrush, NumbersTypeFace, centered: true);
		}

		protected virtual void AddMulliganWinRate()
		{
			var mulliganWinRateBox = Required[Card.IsMulliganOption ? ThemeElement.DefaultStatsActiveBox : ThemeElement.DefaultStatsBox];
			AddChild(mulliganWinRateBox);
		}
		protected virtual void AddMulliganWinRateText() => AddMulliganWinRateText(MulliganWinrateBoxRect);
		protected void AddMulliganWinRateText(Rect rect)
		{
			var winrate = Card.CardWinrates?.MulliganWinrate;
			double? baseWinrate = Card.CardWinrates?.BaseWinrate;

			var delta = winrate != null ? winrate - (baseWinrate ?? 50.0d): null;
			var color = delta is double theDelta ? Helper.GetColorString(theDelta, 75) : "White";
			var mulliganWinrateTextBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
			var text = $"{winrate?.ToString("0.0")}%";
			var textRect = new Rect(rect.Location, rect.Size);
			textRect.Height += 6;
			AddText(text, MulliganWinRateFontSize, textRect, mulliganWinrateTextBrush, TextTypeFace, 2.0, true);
		}

		protected virtual void AddCreatedIcon() => AddCreatedIcon(Required[ThemeElement.CreatedIcon].Rectangle);
		protected virtual void AddCreatedIcon(Rect rect)
		{
			if(Math.Abs(Card.Count) > 1 || Card.Rarity == Rarity.LEGENDARY)
				AddChild(Required[ThemeElement.CreatedIcon], rect.Move(CreatedIconOffset, 0));
			else
				AddChild(Required[ThemeElement.CreatedIcon], rect);
		}

		protected virtual void AddLegendaryIcon() => AddLegendaryIcon(Required[ThemeElement.LegendaryIcon].Rectangle);
		protected void AddLegendaryIcon(Rect rect) => AddChild(Required[ThemeElement.LegendaryIcon], rect);

		protected virtual void AddCost() => AddCost(CostTextRect);
		protected void AddCost(Rect rect)
		{
			if(!Card.HideStats)
				AddText(Card.Cost, CostFontSize, rect, Card.ColorPlayer, NumbersTypeFace, 3.0, true);
		}

		protected virtual void AddCardName()
		{
			var keepRateWidth = Card.CardWinrates != null ? MulliganWinrateBoxRect.Width - 5 : 0;
			var width = FrameRect.Width - BoxRect.Width - keepRateWidth - 38;
			AddCardName(new Rect(38, 8, width, 34));
		}

		protected void AddCardName(Rect rect)
			=> AddText(Card.LocalizedName, TextFontSize, rect, Card.ColorPlayer, TextTypeFace);

		protected virtual void AddText(object? obj, double size, Rect rect, Brush fill, Typeface typeface, double strokeThickness = 2.0, bool centered = false)
		{
			foreach(var d in CardTextImageBuilder.GetOutlinedText(obj?.ToString() ?? "", size, rect, fill, Brushes.Black, typeface, 2.0, centered))
				DrawingGroup.Children.Add(d);
		}

		protected void AddChild(string uri, Rect rect)
			=> DrawingGroup.Children.Add(new ImageDrawing(new BitmapImage(new Uri(uri, UriKind.Relative)), rect));

		protected void AddChild(BitmapSource bmp, Rect rect)
			=> DrawingGroup.Children.Add(new ImageDrawing(bmp, rect));

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
