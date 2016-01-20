#region

using System.Drawing;

#endregion

namespace Hearthstone_Deck_Tracker.Exporting
{
	public class HueAndBrightness
	{
		public HueAndBrightness(double hue, double brightness)
		{
			Hue = hue;
			Brightness = brightness;
		}

		public double Hue { get; }
		public double Brightness { get; }

		public static HueAndBrightness GetAverage(Bitmap bmp, double saturationThreshold = 0.05)
		{
			var totalHue = 0.0f;
			var totalBrightness = 0.0f;
			var validPixels = 0;
			for(var i = 0; i < bmp.Width; i++)
			{
				for(var j = 0; j < bmp.Height; j++)
				{
					var pixel = bmp.GetPixel(i, j);

					//ignore sparkle
					if(pixel.GetSaturation() > saturationThreshold)
					{
						totalHue += pixel.GetHue();
						totalBrightness += pixel.GetBrightness();
						validPixels++;
					}
				}
			}

			return new HueAndBrightness(totalHue / validPixels, totalBrightness / validPixels);
		}
	}
}