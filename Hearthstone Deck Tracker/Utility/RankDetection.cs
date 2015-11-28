using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using AForge.Imaging;
using Hearthstone_Deck_Tracker.Exporting;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class RankDetection
	{
		private static readonly float _threshold = 0.9f;
		private static readonly double _legendMinHue = 36.5;
		private static readonly double _legendMinBrightness = 0.36;
		private static readonly Size _templateSize = new Size(24, 24);
		private static readonly Point _opponentLocation = new Point(26, 36);
		private static readonly Point _playerLocation = new Point(26, 650);

		private static ExhaustiveTemplateMatching _templateMatcher;
		private static string _templateLocation;
		private static Dictionary<int, Bitmap> _templates;
		private static bool _debug;

		static RankDetection()
		{
			_templateLocation = "./Images/Ranks";
			_templateMatcher = new ExhaustiveTemplateMatching(_threshold);
			_templates = new Dictionary<int, Bitmap>();
			_debug = false;
			LoadTemplates();
		}

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
			}
		}

		public static RankResult Match(Bitmap bmp, bool debug = false)
		{
			_debug = debug;
			RankCapture capture = ProcessImage(bmp);
			RankResult result = new RankResult();
			result.Player = FindBest(capture.Player);
			result.Opponent = FindBest(capture.Opponent);
			return result;
		}

		private static RankCapture ProcessImage(Bitmap bmp)
		{
			Bitmap scaled = ResizeImage(bmp);

			Rectangle opponentRect = new Rectangle(_opponentLocation.X, _opponentLocation.Y,
				_templateSize.Width, _templateSize.Height);
			Rectangle playerRect = new Rectangle(_playerLocation.X, _playerLocation.Y,
				_templateSize.Width, _templateSize.Height);

			Bitmap opponent = CropRect(scaled, opponentRect);
			Bitmap player = CropRect(scaled, playerRect);

			if(_debug)
			{
				var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				opponent.Save(ts + "_opponent.png");
				player.Save(ts + "_player.png");
			}

			return new RankCapture(player, opponent);
		}

		private static Bitmap CropRect(Bitmap bmp, Rectangle rect)
		{
			Bitmap target = new Bitmap(rect.Width, rect.Height, PixelFormat.Format24bppRgb);
			using(Graphics g = Graphics.FromImage(target))
			{
				// set graphic quality settings
				//g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				//g.SmoothingMode = SmoothingMode.HighQuality;
				//g.PixelOffsetMode = PixelOffsetMode.HighQuality;
				//g.CompositingQuality = CompositingQuality.HighQuality;
				g.DrawImage(bmp,
					new Rectangle(0, 0, target.Width, target.Height), rect, GraphicsUnit.Pixel);
			}
			return target;
		}

		private static Bitmap ResizeImage(Bitmap original)
		{
			int height = 768;

			if(original.Height == height)
				return original;

			double ratio = 4.0 / 3.0;
			int width = Convert.ToInt32(height * ratio);

			int cropWidth = Convert.ToInt32(original.Height * ratio);
			int posX = 0;// Convert.ToInt32((original.Width - cropWidth) / 2);

			Bitmap scaled = new Bitmap(width, height);
			Graphics graphic = Graphics.FromImage(scaled);
			//graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
			//graphic.SmoothingMode = SmoothingMode.HighQuality;
			//graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
			//graphic.CompositingQuality = CompositingQuality.HighQuality;

			graphic.DrawImage(original,
				new Rectangle(0, 0, width, height),
				new Rectangle(posX, 0, cropWidth, original.Height),
				GraphicsUnit.Pixel);

			graphic.Dispose();

			if(_debug)
			{
				var ts = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				scaled.Save(ts + "_resize.png");
			}

			return scaled;
		}

		public static int FindBest(Bitmap bmp)
		{
			List<RankMatch> results = CompareAll(bmp);

			if(_debug)
			{
				for(var i = 0; i < 3 && i < results.Count; i++)
				{
					Console.WriteLine("{0}: {1}", results[i].Rank, results[i].Score);
				}
			}

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

		private static bool IsLegendRank(Bitmap bmp)
		{
			var b = HueAndBrightness.GetAverage(bmp).Brightness;
			var h = HueAndBrightness.GetAverage(bmp).Hue;
			return b > _legendMinBrightness && h > _legendMinHue;
		}

		private static List<RankMatch> CompareAll(Bitmap sample)
		{
			List<RankMatch> results = new List<RankMatch>();

			foreach(var t in _templates)
			{
				TemplateMatch[] tmatch = _templateMatcher.ProcessImage(sample, t.Value);
				if(tmatch.Length > 0)
				{
					results.Add(new RankMatch(t.Key, tmatch[0].Similarity));
				}
			}
			results.Sort();
			return results;
		}

		public static void SetTemplateLocation(string dir)
		{
			if(Directory.Exists(dir))
			{
				_templateLocation = dir;
			}
		}
	}

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

		public override string ToString()
		{
			return string.Format("Player: {0}, Opponent: {1}, {2}", Player, Opponent, Success);
		}
	}

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
