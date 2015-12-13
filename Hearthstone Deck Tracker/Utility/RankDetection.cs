using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using AForge.Imaging;
using Hearthstone_Deck_Tracker.Exporting;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class RankDetection
	{
		// match threshold, only matches >= this will be returned
		private static readonly float _threshold = 0.9f;
		// min/max hue for legend rank
		private static readonly Range _legendHue = new Range(36.5, 49.5);
		// min/max brightness for legend rank
		private static readonly Range _legendBrightness = new Range(0.36, 0.5);
		// the size of the template images
		private static readonly Size _templateSize = new Size(24, 24);
		// the top-left of the opponent rank rectangle (height 768)
		private static readonly Point _opponentLocation = new Point(26, 36);
		// the top-left of the player rank rectangle (height 768)
		private static readonly Point _playerLocation = new Point(26, 650);
		// location of the templates to compare against
		private static readonly string _templateLocation = "./Images/Ranks";

		private static Dictionary<int, Bitmap> _templates;

		// Static initializer
		static RankDetection()
		{
			Logger.WriteLine("Initalizing", "RankedDetection", 1);
			_templates = new Dictionary<int, Bitmap>();
			LoadTemplates();
		}

		// The main match method, takes a screen capture as argument
		// and tries to find matching ranks for player and opponent
		public static async Task<RankResult> Match(Bitmap bmp)
		{
			RankResult result = new RankResult();
			if(bmp == null)
			{
				Logger.WriteLine("Captured image is null", "RankedDetection");
				return result;
			}
			try
			{
				RankCapture capture = await Task<RankCapture>.Run(() => ProcessImage(bmp));

				result.Player = await Task<int>.Run(() => FindBest(capture.Player));
				result.Opponent = await Task<int>.Run(() => FindBest(capture.Opponent));
			}
			catch(Exception e)
			{
				Logger.WriteLine("Failed: " + e.Message, "RankDetection");
			}
			Logger.WriteLine("Match: P=" + result.Player + ", O=" + result.Opponent, "RankedDetection", 1);
			return result;
		}

		// Secondary match method, called from Match.
		// Useful for testing.
		public static int FindBest(Bitmap bmp)
		{
			List<RankMatch> results = CompareAll(bmp);

			var rank = -1;
			if(results.Count > 0)
			{
				rank = results[0].Rank;
			}
			else if(IsLegendRank(bmp))
			{
				rank = 0;
			}
			return rank;
		}

		// Load template images
		private static void LoadTemplates()
		{
			// files should be named [1..25].bmp
			for(var i = 1; i <= 25; i++)
			{
				var path = _templateLocation + "/" + i + ".bmp";
				if (File.Exists(path))
				{
					_templates[i] = new Bitmap(path);
				}
				else
				{
					Logger.WriteLine("Template image " + _templateLocation + "/" + i + ".bmp not found", 
						"RankedDetection", 1);
				}
			}
			Logger.WriteLine(_templates.Count + " templates loaded", "RankedDetection", 1);
		}

		// Process a full screen capture to indvidual player and
		// opponent rectangle images.
		private static RankCapture ProcessImage(Bitmap bmp)
		{
			Bitmap scaled = ResizeImage(bmp);

			Rectangle opponentRect = new Rectangle(_opponentLocation.X, _opponentLocation.Y,
				_templateSize.Width, _templateSize.Height);
			Rectangle playerRect = new Rectangle(_playerLocation.X, _playerLocation.Y,
				_templateSize.Width, _templateSize.Height);

			Bitmap opponent = CropRect(scaled, opponentRect);
			Bitmap player = CropRect(scaled, playerRect);

			return new RankCapture(player, opponent);
		}

		private static Bitmap CropRect(Bitmap bmp, Rectangle rect)
		{
			Bitmap target = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
			using(Graphics g = Graphics.FromImage(target))
			{
				g.DrawImage(bmp,
					new Rectangle(0, 0, target.Width, target.Height), rect, GraphicsUnit.Pixel);
			}
			return target;
		}

		// Resize captured image to a height of 768
		private static Bitmap ResizeImage(Bitmap original)
		{
			int height = 768;

			if(original.Height == height)
				return original;

			double ratio = 4.0 / 3.0;
			int width = Convert.ToInt32(height * ratio);
			int cropWidth = Convert.ToInt32(original.Height * ratio);
			int posX = 0;

			Bitmap scaled = new Bitmap(width, height);
			using(Graphics g = Graphics.FromImage(scaled))
			{
				g.DrawImage(original,
					new Rectangle(0, 0, width, height),
					new Rectangle(posX, 0, cropWidth, original.Height),
					GraphicsUnit.Pixel);
			}

			return scaled;
		}

		// Color check to determine if it is a legend rank
		private static bool IsLegendRank(Bitmap bmp)
		{
			var b = HueAndBrightness.GetAverage(bmp).Brightness;
			var h = HueAndBrightness.GetAverage(bmp).Hue;
			return b > _legendBrightness.Min && h > _legendHue.Min
				&& b < _legendBrightness.Max && h < _legendHue.Max;
		}

		private static List<RankMatch> CompareAll(Bitmap sample)
		{
			List<RankMatch> results = new List<RankMatch>();
			ExhaustiveTemplateMatching matcher = new ExhaustiveTemplateMatching(_threshold);

			foreach(var t in _templates)
			{
				TemplateMatch[] tmatch = matcher.ProcessImage(sample, t.Value);
				if(tmatch.Length > 0)
				{
					results.Add(new RankMatch(t.Key, tmatch[0].Similarity));
				}
			}

			results.Sort();
			return results;
		}

		// Holds a the min & max of a range,
		// used in hue & brightness.
		private struct Range
		{
			public double Min { get; set; }
			public double Max { get; set; }

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
		public int Player { get; set; }
		public int Opponent { get; set; }
		public bool Success
		{
			get
			{
				// TODO: what about when only one is -1
				return Player >= 0 && Opponent >= 0;
			}
		}

		public RankResult()
		{
			Player = -1;
			Opponent = -1;
		}

		public override string ToString()
		{
			return string.Format("Player: {0}, Opponent: {1}, {2}", Player, Opponent, Success);
		}
	}

	// Holds player and opponent rectangle bitmaps
	public class RankCapture
	{
		public Bitmap Player { get; set; }
		public Bitmap Opponent { get; set; }

		public RankCapture(Bitmap player, Bitmap opponent)
		{
			Player = player;
			Opponent = opponent;
		}
	}

	// Used to enable sorting of template matching results
	public class RankMatch : IEquatable<RankMatch>, IComparable<RankMatch>
	{
		public int Rank { get; set; }
		public float Score { get; set; }

		public RankMatch(int rank, float score)
		{
			Rank = rank;
			Score = score;
		}

		public int CompareTo(RankMatch other)
		{
			// descending
			return other.Score.CompareTo(this.Score);
		}

		public bool Equals(RankMatch other)
		{
			return this.Rank == other.Rank && this.Score == other.Score;
		}
	}
}
