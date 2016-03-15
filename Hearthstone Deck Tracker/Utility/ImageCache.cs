#region

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Enums;

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
			BitmapImage image;
			if(ImageCacheDict.TryGetValue(resourcePath, out image))
				return image;
			var uri = new Uri($"pack://application:,,,/{basePath}/{resourcePath}", UriKind.Absolute);
			image = new BitmapImage(uri);
			ImageCacheDict.Add(resourcePath, image);
			return image;
		}

		public static BitmapImage GetClassIcon(string className)
		{
			HeroClassAll heroClass;
			return Enum.TryParse(className, out heroClass) ? GetClassIcon(heroClass) : new BitmapImage();
		}

		public static BitmapImage GetClassIcon(HeroClassAll @class)
		{
			var path = new StringBuilder("ClassIcons");
			if(@class == HeroClassAll.All || @class == HeroClassAll.Archived)
			{
				path.Append("/General/");
				path.Append(Config.Instance.ThemeName);
				path.Append(@class == HeroClassAll.All ? "/all.png" : "/archived.png");
			}
			else
				path.Append($"/{Config.Instance.ClassIconStyle}/{@class.ToString().ToLower()}.png");
			return GetImage(path.ToString());
		}
	}
}