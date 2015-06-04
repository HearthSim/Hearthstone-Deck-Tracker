using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class ImageCache
	{
		private static readonly Dictionary<string, BitmapImage> ImageCacheDict = new Dictionary<string, BitmapImage>();
		public static BitmapImage GetImage(string resourcePath)
		{
			BitmapImage image;
			if(ImageCacheDict.TryGetValue(resourcePath, out image))
				return image;
			var uri = new Uri(string.Format("../Resources/{0}", resourcePath), UriKind.Relative);
			image = new BitmapImage(uri);
			ImageCacheDict.Add(resourcePath, image);
			return image;
		}
		private static BitmapImage _archiveedMarker;
		public static BitmapImage ArchivedMarker
		{
			get
			{
				return GetImage("archived_marker.png");
			}
		}
	}
}
