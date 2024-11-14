using System;
using System.Windows;
using Hearthstone_Deck_Tracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests
{
	[TestClass]
	public class OutlinedTextBlockTests
	{
		[TestMethod]
		public void MeasureOverride_BadSize_DoesNotThrow()
		{
			var textBlock = new TestTextBlock
			{
				TextWrapping = TextWrapping.NoWrap,
				Text = "Test",
				FontSize = 0.1
			};
			try
			{
				textBlock.CallMeasureOverride(new Size(0, 0));
			}
			catch(Exception e)
			{
				Assert.Fail(e.Message);
			}
		}

		private class TestTextBlock : OutlinedTextBlock
		{
			public void CallMeasureOverride(Size availableSize)
			{
				MeasureOverride(availableSize);
			}
		}
	}
}
