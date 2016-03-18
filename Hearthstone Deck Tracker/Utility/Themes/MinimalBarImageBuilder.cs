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
		private readonly Typeface _countType = new Typeface(new FontFamily(
			new Uri("pack://application:,,,/"), "./resources/#Belwe Bd BT"),
				FontStyles.Normal, FontWeights.Normal, FontStretches.Condensed);

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

		protected override void AddText(object obj, int size, int x, int y, Brush fill)
		{
			Brush rarity = fill;
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

			var text = new FormattedText(obj.ToString(), CultureInfo.GetCultureInfo("en-us"),
						FlowDirection.LeftToRight, _countType, size, Brushes.White);
			var point = new Point(x, y);

			_drawingGroup.Children.Add(new GeometryDrawing(Brushes.Black,
				new Pen(Brushes.Black, 2.0), text.BuildGeometry(point)));
			_drawingGroup.Children.Add(new GeometryDrawing(rarity,
				new Pen(Brushes.White, 0), text.BuildGeometry(point)));
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