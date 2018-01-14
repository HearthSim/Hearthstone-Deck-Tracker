using System.IO;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
	public static class BitmapExtensions
	{
		// http://stackoverflow.com/a/1069509
		// Load a WPF BitmapImage from a System.Drawing.Bitmap		
	    public static BitmapImage ToImageSource(this Image<Rgba32> bitmap)
		{
			using(var memory = new MemoryStream())
			{
			    bitmap.SaveAsBmp(memory);
				memory.Position = 0;
				var bitmapimage = new BitmapImage();
				bitmapimage.BeginInit();
				bitmapimage.StreamSource = memory;
				bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapimage.EndInit();
				return bitmapimage;
			}
		}
	}
}
