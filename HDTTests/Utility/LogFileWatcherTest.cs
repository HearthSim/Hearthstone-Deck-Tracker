using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HearthWatcher.LogReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HDTTests.Utility
{
	[TestClass]
	public class LogFileWatcherTest
	{
		[TestMethod]
		public async Task TruncatedLog_ContinuesReadingFromBeginning()
		{
			var directory = Path.Combine(Path.GetTempPath(), "hdt-log-watcher-" + Guid.NewGuid());
			Directory.CreateDirectory(directory);
			var filePath = Path.Combine(directory, "Power.log");
			File.WriteAllText(filePath, string.Empty);
			var watcher = new LogFileWatcher(new LogWatcherInfo { Name = "Power" });

			try
			{
				watcher.Start(DateTime.MinValue, directory);
				File.WriteAllText(filePath, "D 00:00:00.000 first entry\r\n");
				Assert.IsTrue(await WaitForLine(watcher, "first entry", TimeSpan.FromSeconds(3)));

				File.WriteAllText(filePath, "D 00:00:01.000 second\r\n");
				Assert.IsTrue(await WaitForLine(watcher, "second", TimeSpan.FromSeconds(3)));
			}
			finally
			{
				await watcher.Stop();
				Directory.Delete(directory, true);
			}
		}

		private static async Task<bool> WaitForLine(LogFileWatcher watcher, string value, TimeSpan timeout)
		{
			var stopwatch = Stopwatch.StartNew();
			while(stopwatch.Elapsed < timeout)
			{
				if(watcher.Collect().Any(x => x.LineContent.Contains(value)))
					return true;
				await Task.Delay(50);
			}
			return false;
		}
	}
}
