using System.IO;
using System.Linq;
using Hearthstone_Deck_Tracker;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests
{
	[TestClass]
	public class ConfigTests
	{
		[TestMethod]
		public void Save_LeavesConfigFileUntouchedWhenConfigWasNeverLoaded()
		{
			Assert.IsFalse(Config.IsLoaded);

			var path = Config.Instance.ConfigPath;
			var before = File.Exists(path) ? File.ReadAllBytes(path) : null;

			Config.Save();

			var after = File.Exists(path) ? File.ReadAllBytes(path) : null;
			var unchanged = before == null ? after == null : after != null && before.SequenceEqual(after);

			// undo the damage before failing, the file is the developer's real config
			if(!unchanged)
			{
				if(before == null)
					File.Delete(path);
				else
					File.WriteAllBytes(path, before);
			}

			Assert.IsTrue(unchanged);
		}
	}
}
