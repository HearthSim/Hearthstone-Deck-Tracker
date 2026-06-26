using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Hearthstone_Deck_Tracker.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility
{
	[TestClass]
	public class AttachedFormattedStringTests
	{
		[TestMethod]
		public void FormattedText_PreservesAmpersand()
		{
			var textBlock = new TextBlock();

			AttachedFormattedString.SetFormattedText(textBlock, "A & B");

			var text = string.Concat(textBlock.Inlines.OfType<Run>().Select(x => x.Text));
			Assert.AreEqual("A & B", text);
		}

		[TestMethod]
		public void FormattedText_PreservesBoldTag()
		{
			var textBlock = new TextBlock();

			AttachedFormattedString.SetFormattedText(textBlock, "A <b>B</b>");

			var runs = textBlock.Inlines.OfType<Run>().ToArray();
			Assert.AreEqual(2, runs.Length);
			Assert.AreEqual("A ", runs[0].Text);
			Assert.AreEqual("B", runs[1].Text);
			Assert.AreEqual(FontWeights.Bold, runs[1].FontWeight);
		}
	}
}
