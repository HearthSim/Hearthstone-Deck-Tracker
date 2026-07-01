using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Battlegrounds
{
	[TestClass]
	public class GuidesTabsOverlayCompatibilityTest
	{
		[TestMethod]
		public void GuidesTabs_ShouldNotUseWpfButtonControls()
		{
			// GuidesTabs is displayed in the Battlegrounds overlay which is a
			// WS_EX_TRANSPARENT click-through overlay window. Standard WPF Button
			// controls do not fire click events reliably under Wine/Linux in this
			// configuration. The tab triggers must use overlay-compatible click
			// handling (e.g. MouseLeftButtonUp on Border/UserControl).
			var xamlPath = Path.GetFullPath(Path.Combine(
				TestContextHelper.GetProjectRoot(),
				"Hearthstone Deck Tracker",
				"Controls",
				"Overlay",
				"Battlegrounds",
				"Guides",
				"GuidesTabs.xaml"
			));

			Assert.IsTrue(File.Exists(xamlPath), $"GuidesTabs.xaml not found at {xamlPath}");

			var xaml = File.ReadAllText(xamlPath);

			var buttonMatches = Regex.Matches(xaml, @"<\s*(Button|ButtonBase|ToggleButton|RepeatButton)[\s/>]", RegexOptions.IgnoreCase);
			var count = buttonMatches.Count;

			Assert.AreEqual(0, count,
				$"GuidesTabs.xaml contains {count} WPF Button/ButtonBase element(s). " +
				"Use overlay-compatible controls (OverlayButton) instead. " +
				"Standard WPF Button controls are unreliable in WS_EX_TRANSPARENT overlay windows under Wine.");
		}

		[TestMethod]
		public void BattlegroundsMinionPinning_ShouldNotUseWpfButtonControls()
		{
			// BattlegroundsMinionPinning is displayed in the Battlegrounds overlay
			// which is a WS_EX_TRANSPARENT click-through overlay window. Standard
			// WPF Button controls do not fire click events reliably under Wine/Linux
			// in this configuration. All interactive triggers must use overlay-
			// compatible click handling (e.g. MouseLeftButtonUp on Border).
			var xamlPath = Path.GetFullPath(Path.Combine(
				TestContextHelper.GetProjectRoot(),
				"Hearthstone Deck Tracker",
				"Controls",
				"Overlay",
				"Battlegrounds",
				"MinionPinning",
				"BattlegroundsMinionPinning.xaml"
			));

			Assert.IsTrue(File.Exists(xamlPath), $"BattlegroundsMinionPinning.xaml not found at {xamlPath}");

			var xaml = File.ReadAllText(xamlPath);

			var buttonMatches = Regex.Matches(xaml, @"<\s*(Button|ButtonBase|ToggleButton|RepeatButton)[\s/>]", RegexOptions.IgnoreCase);
			var count = buttonMatches.Count;

			Assert.AreEqual(0, count,
				$"BattlegroundsMinionPinning.xaml contains {count} WPF Button/ButtonBase element(s). " +
				"Use overlay-compatible controls (OverlayButton) instead. " +
				"Standard WPF Button controls are unreliable in WS_EX_TRANSPARENT overlay windows under Wine.");
		}

		[TestMethod]
		public void GuidesTabsContent_ShouldNotUseWpfButtonControls()
		{
			var relativePaths = new[]
			{
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Minions", "BattlegroundsMinions.xaml"),
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Minions", "BattlegroundsMinionsExtraFilters.xaml"),
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Guides", "Comps", "CompGuideList.xaml"),
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Guides", "Comps", "CompButton.xaml"),
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Guides", "Comps", "CompGuide.xaml"),
				Path.Combine("Hearthstone Deck Tracker", "Controls", "Overlay", "Battlegrounds", "Inspiration", "BattlegroundsInspiration.xaml"),
			};

			foreach(var relativePath in relativePaths)
			{
				var xamlPath = Path.GetFullPath(Path.Combine(TestContextHelper.GetProjectRoot(), relativePath));
				Assert.IsTrue(File.Exists(xamlPath), $"Overlay XAML file not found at {xamlPath}");

				var xaml = File.ReadAllText(xamlPath);
				var buttonMatches = Regex.Matches(xaml, @"<\s*(Button|ButtonBase|ToggleButton|RepeatButton)[\s/>]", RegexOptions.IgnoreCase);
				var count = buttonMatches.Count;

				Assert.AreEqual(0, count,
					$"{Path.GetFileName(xamlPath)} contains {count} WPF Button/ButtonBase element(s). " +
					"Use overlay-compatible controls (OverlayButton) instead. " +
					"Standard WPF Button controls are unreliable in WS_EX_TRANSPARENT overlay windows under Wine.");
			}
		}
	}

	internal static class TestContextHelper
	{
		/// <summary>
		/// Walk up from the test assembly location to find the repo root
		/// (the directory containing the .sln file).
		/// </summary>
		public static string GetProjectRoot()
		{
			var dir = Path.GetDirectoryName(typeof(TestContextHelper).Assembly.Location);
			while(dir != null)
			{
				if(Directory.GetFiles(dir, "*.sln").Any())
					return dir;
				dir = Path.GetDirectoryName(dir);
			}
			// Fallback: use relative from repo structure
			return Path.GetFullPath(Path.Combine(
				Path.GetDirectoryName(typeof(TestContextHelper).Assembly.Location) ?? ".",
				"..", "..", "..", ".."
			));
		}
	}
}
