#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using AForge.Imaging;
using Hearthstone_Deck_Tracker.Exporting;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility
{
	public class RankDetection
	{
		// match threshold, only matches >= this will be returned
		private static readonly float _threshold = 0.9f;
		// min/max hue for legend rank
		private static readonly Range LegendHue = new Range(36.5, 49.5);
		// min/max brightness for legend rank
		private static readonly Range LegendBrightness = new Range(0.36, 0.5);
		// the size of the template images
		private static readonly Size TemplateSize = new Size(24, 24);
		// the top-left of the opponent rank rectangle (height 768)
		private static readonly Point OpponentLocation = new Point(26, 36);
		// the top-left of the player rank rectangle (height 768)
		private static readonly Point PlayerLocation = new Point(26, 650);
		// location of the templates to compare against
		private const string TemplateLocation = "/HearthstoneDeckTracker;component/Resources/Ranks/";

		private static readonly Dictionary<int, Bitmap> Templates;

		// Static initializer
		static RankDetection()
		{
			Log.Debug("Initalizing");
			Templates = new Dictionary<int, Bitmap>();
			LoadTemplates();
		}

		// The main match method, takes a screen capture as argument
		// and tries to find matching ranks for player and opponent
		public static async Task<RankResult> Match(Bitmap bmp)
		{
			var result = new RankResult();
			if(bmp == null)
			{
				Log.Error("Captured image is null");
				return result;
			}
			try
			{
				var capture = await Task.Run(() => ProcessImage(bmp));
				result.Player = await Task.Run(() => FindBest(capture.Player));
				result.Opponent = await Task.Run(() => FindBest(capture.Opponent));
			}
			catch(Exception e)
			{
				Log.Error(e);
			}
			Log.Debug($"Match: P={result.Player}, O={result.Opponent}");
			return result;
		}

		// Secondary match method, called from Match.
		// Useful for testing.
		public static int FindBest(Bitmap bmp)
		{
			var results = CompareAll(bmp);

			var rank = -1;
			if(results.Count > 0)
				rank = results[0].Rank;
			else if(IsLegendRank(bmp))
				rank = 0;
			return rank;
		}

		// Load template images
		private static void LoadTemplates()
		{
			// files should be named [1..25].bmp
			for(var i = 1; i <= 25; i++)
			{
				try
				{
					var resource = System.Windows.Application.GetResourceStream(
						new Uri(TemplateLocation + i + ".bmp", UriKind.Relative));
					Templates[i] = new Bitmap(resource.Stream);
				}
				catch (Exception e)
				{
					Log.Error($"Template image {i}.bmp not found. [{e.Message}]");
				}					
			}
			Log.Debug(Templates.Count + " templates loaded");
		}

		// Process a full screen capture to indvidual player and
		// opponent rectangle images.
		private static RankCapture ProcessImage(Bitmap bmp)
		{
			var scaled = ResizeImage(bmp);

			var opponentRect = new Rectangle(OpponentLocation.X, OpponentLocation.Y, TemplateSize.Width, TemplateSize.Height);
			var playerRect = new Rectangle(PlayerLocation.X, PlayerLocation.Y, TemplateSize.Width, TemplateSize.Height);

			var opponent = CropRect(scaled, opponentRect);
			var player = CropRect(scaled, playerRect);

			return new RankCapture(player, opponent);
		}

		private static Bitmap CropRect(Bitmap bmp, Rectangle rect)
		{
			var target = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
			using(var g = Graphics.FromImage(target))
				g.DrawImage(bmp, new Rectangle(0, 0, target.Width, target.Height), rect, GraphicsUnit.Pixel);
			return target;
		}

		// Resize captured image to a height of 768
		private static Bitmap ResizeImage(Bitmap original)
		{
			const int height = 768;

			if(original.Height == height)
				return original;

			const double ratio = 4.0 / 3.0;
			var width = Convert.ToInt32(height * ratio);
			var cropWidth = Convert.ToInt32(original.Height * ratio);
			const int posX = 0;

			var scaled = new Bitmap(width, height);
			using(var g = Graphics.FromImage(scaled))
				g.DrawImage(original, new Rectangle(0, 0, width, height), new Rectangle(posX, 0, cropWidth, original.Height), GraphicsUnit.Pixel);

			return scaled;
		}

		// Color check to determine if it is a legend rank
		private static bool IsLegendRank(Bitmap bmp)
		{
			var b = HueAndBrightness.GetAverage(bmp).Brightness;
			var h = HueAndBrightness.GetAverage(bmp).Hue;
			return b > LegendBrightness.Min && h > LegendHue.Min && b < LegendBrightness.Max && h < LegendHue.Max;
		}

		private static List<RankMatch> CompareAll(Bitmap sample)
		{
			var results = new List<RankMatch>();
			var matcher = new ExhaustiveTemplateMatching(_threshold);

			foreach(var t in Templates)
			{
				var tmatch = matcher.ProcessImage(sample, t.Value);
				if(tmatch.Length > 0)
					results.Add(new RankMatch(t.Key, tmatch[0].Similarity));
			}

			results.Sort();
			return results;
		}

		// Holds a the min & max of a range,
		// used in hue & brightness.
		private struct Range
		{
			public double Min { get; }
			public double Max { get; }

			public Range(double min, double max) : this()
			{
				Min = min;
				Max = max;
			}
		}
	}

	// Holds the result of template match,
	// includes determined player and opponent ranks,
	// and also whether it was successful (both players have ranks).
	public class RankResult
	{
		public RankResult()
		{
			Player = -1;
			Opponent = -1;
		}

		public int Player { get; set; }
		public int Opponent { get; set; }
		
		// for now we don't care if the opponent rank is detected.
		public bool Success => Player >= 0;
		public bool OpponentSuccess => Opponent >= 0;

		public override string ToString() => $"Player: {Player}, Opponent: {Opponent}, {Success}";
	}

	// Holds player and opponent rectangle bitmaps
	public class RankCapture
	{
		public RankCapture(Bitmap player, Bitmap opponent)
		{
			Player = player;
			Opponent = opponent;
		}

		public Bitmap Player { get; set; }
		public Bitmap Opponent { get; set; }
	}

	// Used to enable sorting of template matching results
	public class RankMatch : IEquatable<RankMatch>, IComparable<RankMatch>
	{
		public RankMatch(int rank, float score)
		{
			Rank = rank;
			Score = score;
		}

		public int Rank { get; set; }
		public float Score { get; set; }

		// descending
		public int CompareTo(RankMatch other) => other.Score.CompareTo(Score);

		public bool Equals(RankMatch other) => Rank == other.Rank && Score == other.Score;
	}
}