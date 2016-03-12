using System.Drawing;
using System.IO;
using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.RankedDetection
{
	[TestClass]
	public class RankedDetectionTest
	{
		// Full screen capture

		[TestMethod]
		public void ScreenCapturePositions()
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/Screen768_8-8.jpg");
			var rd = RankDetection.Match(bmp).Result;
			Assert.IsTrue(rd.Success);
			Assert.AreEqual(8, rd.Player);
			Assert.AreEqual(8, rd.Opponent);
		}

		// Regular ranks 1-25

		[TestMethod]
		public void Rank01()
		{
			Assert.IsTrue(RankIsCorrect(1));
		}

		[TestMethod]
		public void Rank02()
		{
			Assert.IsTrue(RankIsCorrect(2));
		}

		[TestMethod]
		public void Rank03()
		{
			Assert.IsTrue(RankIsCorrect(3));
		}

		[TestMethod]
		public void Rank04()
		{
			Assert.IsTrue(RankIsCorrect(4));
		}

		[TestMethod]
		public void Rank05()
		{
			Assert.IsTrue(RankIsCorrect(5));
		}

		[TestMethod]
		public void Rank06()
		{
			Assert.IsTrue(RankIsCorrect(6));
		}

		[TestMethod]
		public void Rank07()
		{
			Assert.IsTrue(RankIsCorrect(7));
		}

		[TestMethod]
		public void Rank08()
		{
			Assert.IsTrue(RankIsCorrect(8));
		}

		[TestMethod]
		public void Rank09()
		{
			Assert.IsTrue(RankIsCorrect(9));
		}

		[TestMethod]
		public void Rank10()
		{
			Assert.IsTrue(RankIsCorrect(10));
		}

		[TestMethod]
		public void Rank11()
		{
			Assert.IsTrue(RankIsCorrect(11));
		}

		[TestMethod]
		public void Rank12()
		{
			Assert.IsTrue(RankIsCorrect(12));
		}

		[TestMethod]
		public void Rank13()
		{
			Assert.IsTrue(RankIsCorrect(13));
		}

		[TestMethod]
		public void Rank14()
		{
			Assert.IsTrue(RankIsCorrect(14));
		}

		[TestMethod]
		public void Rank15()
		{
			Assert.IsTrue(RankIsCorrect(15));
		}

		[TestMethod]
		public void Rank16()
		{
			Assert.IsTrue(RankIsCorrect(16));
		}

		[TestMethod]
		public void Rank17()
		{
			Assert.IsTrue(RankIsCorrect(17));
		}

		[TestMethod]
		public void Rank18()
		{
			Assert.IsTrue(RankIsCorrect(18));
		}

		[TestMethod]
		public void Rank19()
		{
			Assert.IsTrue(RankIsCorrect(19));
		}

		[TestMethod]
		public void Rank20()
		{
			Assert.IsTrue(RankIsCorrect(20));
		}

		[TestMethod]
		public void Rank21()
		{
			Assert.IsTrue(RankIsCorrect(21));
		}

		[TestMethod]
		public void Rank22()
		{
			Assert.IsTrue(RankIsCorrect(22));
		}

		[TestMethod]
		public void Rank23()
		{
			Assert.IsTrue(RankIsCorrect(23));
		}

		[TestMethod]
		public void Rank24()
		{
			Assert.IsTrue(RankIsCorrect(24));
		}

		[TestMethod]
		public void Rank25()
		{
			Assert.IsTrue(RankIsCorrect(25));
		}

		// Legend rank tests (Rank 0)

		[TestMethod]
		public void LegendRankA()
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/LegendA.png");
			Assert.AreEqual(0, RankDetection.FindBest(bmp));
		}

		[TestMethod]
		public void LegendRankB()
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/LegendB.png");
			Assert.AreEqual(0, RankDetection.FindBest(bmp));
		}

		[TestMethod]
		public void LegendRankC()
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/LegendC.png");
			Assert.AreEqual(0, RankDetection.FindBest(bmp));
		}

		[TestMethod]
		public void LegendRankD()
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/LegendD.png");
			Assert.AreEqual(0, RankDetection.FindBest(bmp));
		}


		// Helper Methods

		private bool RankIsCorrect(int rank)
		{
			var bmp = new Bitmap("RankedDetection/TestFiles/" + rank + ".png");
			return rank == RankDetection.FindBest(bmp);
		}
	}
}
