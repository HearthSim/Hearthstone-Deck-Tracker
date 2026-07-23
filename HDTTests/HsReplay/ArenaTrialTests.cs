using Hearthstone_Deck_Tracker.HsReplay;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.HsReplay
{
	[TestClass]
	public class ArenaTrialTests
	{
		[TestMethod]
		public void FormatDiagnostics_IncludesCountsTotalAndResetHours()
		{
			var s = ArenaTrial.FormatDiagnostics(1, 2, 5);
			StringAssert.Contains(s, "starter=1");
			StringAssert.Contains(s, "recurring=2");
			StringAssert.Contains(s, "total=3");
			StringAssert.Contains(s, "resetInHours=5");
		}

		[TestMethod]
		public void FormatDiagnostics_NullResetHours_ShowsNotAvailable()
		{
			var s = ArenaTrial.FormatDiagnostics(0, 0, null);
			StringAssert.Contains(s, "total=0");
			StringAssert.Contains(s, "resetInHours=n/a");
		}
	}
}
