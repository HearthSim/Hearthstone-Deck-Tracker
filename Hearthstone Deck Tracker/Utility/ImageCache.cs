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

		public static BitmapImage Archived
		{
			get { return GetClassIcon(HeroClassAll.Archived); }
		}

		public static BitmapImage ArchivedBlack
		{
			get { return GetImage("ClassIcons/General/BaseLight/archived.png"); }
		}

		public static BitmapImage Druid
		{
			get { return GetClassIcon(HeroClassAll.Druid); }
		}

		public static BitmapImage Hunter
		{
			get { return GetClassIcon(HeroClassAll.Hunter); }
		}

		public static BitmapImage Mage
		{
			get { return GetClassIcon(HeroClassAll.Mage); }
		}

		public static BitmapImage Paladin
		{
			get { return GetClassIcon(HeroClassAll.Paladin); }
		}

		public static BitmapImage Priest
		{
			get { return GetClassIcon(HeroClassAll.Priest); }
		}

		public static BitmapImage Rogue
		{
			get { return GetClassIcon(HeroClassAll.Rogue); }
		}

		public static BitmapImage Shaman
		{
			get { return GetClassIcon(HeroClassAll.Shaman); }
		}

		public static BitmapImage Warlock
		{
			get { return GetClassIcon(HeroClassAll.Warlock); }
		}

		public static BitmapImage Warrior
		{
			get { return GetClassIcon(HeroClassAll.Warrior); }
		}

		public static BitmapImage GetImage(string resourcePath)
		{
			BitmapImage image;
			if(ImageCacheDict.TryGetValue(resourcePath, out image))
				return image;
			var uri = new Uri(string.Format("pack://application:,,,/Resources/{0}", resourcePath), UriKind.Absolute);
			image = new BitmapImage(uri);
			ImageCacheDict.Add(resourcePath, image);
			return image;
		}

		public static BitmapImage GetClassIcon(HeroClassAll @class)
		{
			var path = new StringBuilder("ClassIcons");
			if(@class == HeroClassAll.All || @class == HeroClassAll.Archived)
			{
				path.Append("/General/");
				path.Append(string.IsNullOrEmpty(Config.Instance.ThemeName) ? "BaseLight" : Config.Instance.ThemeName);
				path.Append(@class == HeroClassAll.All ? "/all.png" : "/archived.png");
			}
			else
				path.Append(string.Format("/{0}/{1}.png", Config.Instance.ClassIconStyle, @class.ToString().ToLower()));
			return GetImage(path.ToString());
		}
	}
}