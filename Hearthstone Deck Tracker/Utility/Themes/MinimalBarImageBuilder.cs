using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.Utility.Themes
{
	public class MinimalBarImageBuilder : CardBarImageBuilder
	{
		public MinimalBarImageBuilder(Card card, string dir) : base(card, dir)
		{
		}

		public override ImageBrush Build()
		{
			_drawingGroup.Children.Clear();

			if(!_hasAllRequired)
				return new ImageBrush();

			AddCardImage();
			AddFadeOverlay();
			AddGem();
			AddCountBox();
			if(Math.Abs(_card.Count) > 1)
				AddCountText();
			if(_card.IsCreated)
				AddCreatedIcon(-15);
			if(Math.Abs(_card.Count) <= 1 && _card.Rarity == Rarity.Legendary)
				AddLegendaryIcon();
			AddFrame();
			AddCost();
			AddCardName();
			if(_card.Count <= 0 || _card.Jousted)
				AddDarken();

			return new ImageBrush { ImageSource = new DrawingImage(_drawingGroup) };
		}

		protected override void AddCardImage()
		{
			var cardFile = Path.Combine(BarImageDir, _card.Id + ".png");
			if(File.Exists(cardFile))
			{
				var img = AForge.Imaging.Image.FromFile(cardFile);
				AForge.Imaging.Filters.GaussianBlur filter = new AForge.Imaging.Filters.GaussianBlur(2, 8);
				filter.ApplyInPlace(img);

				_drawingGroup.Children.Add(new ImageDrawing(BitmapToImageSource(img),
					new Rect(0, 0, 217, 34)));
			}
		}

		protected override void AddCountText()
		{
			Brush rarity;
			switch(_card.Rarity)
			{
				case Rarity.Rare:
					rarity = new SolidColorBrush(Color.FromRgb(49, 134, 222));
					break;

				case Rarity.Epic:
					rarity = new SolidColorBrush(Color.FromRgb(173, 113, 247));
					break;

				case Rarity.Legendary:
					rarity = new SolidColorBrush(Color.FromRgb(255, 154, 16));
					break;

				default:
					rarity = Brushes.White;
					break;
			}
			var count = Math.Abs(_card.Count);
			if(count > 1)
			{
				var countText = count > 9 ? "9" : count.ToString();
				AddText(countText, 20, new Rect(198, 4, double.NaN, double.NaN), rarity);
				if(count > 9)
					AddText("+", 13, new Rect(203, 3, double.NaN, double.NaN), rarity);
			}
		}

		private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
		{
			using(MemoryStream memory = new MemoryStream())
			{
				bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
				memory.Position = 0;
				BitmapImage bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();

				return bitmapimage;
			}
		}
	}
}