#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Extensions;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class ImageCache
	{
		private static readonly Dictionary<string, BitmapImage> ImageCacheDict = new Dictionary<string, BitmapImage>();

		public static BitmapImage Archived => GetClassIcon(HeroClassAll.Archived);
		public static BitmapImage ArchivedBlack => GetImage("ClassIcons/General/BaseLight/archived.png");
		public static BitmapImage Druid => GetClassIcon(HeroClassAll.Druid);
		public static BitmapImage Hunter => GetClassIcon(HeroClassAll.Hunter);
		public static BitmapImage Mage => GetClassIcon(HeroClassAll.Mage);
		public static BitmapImage Paladin => GetClassIcon(HeroClassAll.Paladin);
		public static BitmapImage Priest => GetClassIcon(HeroClassAll.Priest);
		public static BitmapImage Rogue => GetClassIcon(HeroClassAll.Rogue);
		public static BitmapImage Shaman => GetClassIcon(HeroClassAll.Shaman);
		public static BitmapImage Warlock => GetClassIcon(HeroClassAll.Warlock);
		public static BitmapImage Warrior => GetClassIcon(HeroClassAll.Warrior);

		public static BitmapImage GetImage(string resourcePath, string basePath = "Resources")
		{
			if(ImageCacheDict.TryGetValue(resourcePath, out var image))
				return image;
			var uri = new Uri($"pack://application:,,,/{basePath}/{resourcePath}", UriKind.Absolute);
			image = new BitmapImage(uri);
			ImageCacheDict.Add(resourcePath, image);
			return image;
		}

		public static BitmapImage GetClassIcon(string className)
			=> Enum.TryParse(className, out HeroClassAll heroClass) ? GetClassIcon(heroClass) : new BitmapImage();

		public static BitmapImage GetClassIcon(HeroClassAll @class)
		{
			if(LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return new BitmapImage();
			var path = new StringBuilder("ClassIcons");
			if(@class == HeroClassAll.All || @class == HeroClassAll.Archived)
			{
				path.Append("/General/");
				path.Append(Config.Instance.AppTheme);
				path.Append(@class == HeroClassAll.All ? "/all.png" : "/archived.png");
			}
			else
				path.Append($"/{Config.Instance.ClassIconStyle}/{@class.ToString().ToLower()}.png");
			return GetImage(path.ToString());
		}

		public static readonly Dictionary<string, Bitmap> CardBitmaps = new Dictionary<string, Bitmap>();
		public static readonly Dictionary<string, BitmapImage> CardBitmapImages = new Dictionary<string, BitmapImage>();
		public static readonly List<string> LoadedSets = new List<string>();

		public static BitmapImage GetCardImage(Card card)
		{
			if(CardBitmapImages.TryGetValue(card.Id, out BitmapImage bmpImg))
				return bmpImg;
			if(!CardBitmaps.TryGetValue(card.Id, out Bitmap bmp))
				LoadResource(card, out bmp);
			if(bmp != null)
				bmpImg = bmp.ToImageSource();
			CardBitmapImages.Add(card.Id, bmpImg);
			return bmpImg;
		}

		public static Bitmap GetCardBitmap(Card card)
		{
			if(!CardBitmaps.TryGetValue(card.Id, out Bitmap bmp))
				LoadResource(card, out bmp);
			return bmp;
		}

		private static void LoadResource(Card card, out Bitmap bitmap)
		{
			bitmap = null;
			var set = card.CardSet + (card.Collectible ? "" : "_NC");
			if(LoadedSets.Contains(set))
				return;
			LoadedSets.Add(set);
			var file = new FileInfo($"Images/Tiles/{set}.res");
			if(!file.Exists)
				return;
			using(var reader = new ResourceReader(file.FullName))
			{
				foreach(var entry in reader.OfType<DictionaryEntry>())
				{
					var key = entry.Key.ToString();
					if(key == card.Id)
						bitmap = (Bitmap)entry.Value;
					CardBitmaps.Add(key, (Bitmap)entry.Value);
				}
			}
		}
	}
}
