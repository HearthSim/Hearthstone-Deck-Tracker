using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Updating;

// Used in Config.cs as int.
// Don't delete value of this enum. If one becomes obsolete, comment it out and continue the
// next one on the next higher value. GetCurrentRemote will fall back to GitHub if no enum
// value can be found for the stored int.
public enum SquirrelRemote
{
	Github = 0,
	AwsHongKong = 1,
}

public static class SquirrelConnection
{
	public static readonly Dictionary<SquirrelRemote, string> SquirrelRemoteUrls = new()
	{
		[SquirrelRemote.Github] = "https://github.com/HearthSim/HDT-Releases",
		[SquirrelRemote.AwsHongKong] = "https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com"
	};

	private static string GetReleasesUrl(SquirrelRemote remote, string version)
	{
		var url = SquirrelRemoteUrls[remote];
		if(remote == SquirrelRemote.Github)
			return $"{url}/releases/download/v{version}/RELEASES";
		return $"{url}/RELEASES";
	}

	public static (SquirrelRemote, string) GetCurrentRemote()
	{
		var remote = (SquirrelRemote)Config.Instance.SquirrelRemote;
		if(!SquirrelRemoteUrls.TryGetValue(remote, out var url))
			return (SquirrelRemote.Github, SquirrelRemoteUrls[SquirrelRemote.Github]);
		return (remote, url);
	}

	private static bool _didRun;
	public static async void FindBestRemote()
	{
		if(_didRun)
		{
			// We only want to do this at most once per session
			return;
		}
		_didRun = true;
		try
		{
			// Wait for HDT to be up and running. Not strictly necessary, but no reason to impact startup.
			// Will also make the logging a little cleaner.
			while(!Core.Initialized)
				await Task.Delay(100);

			Log.Info("Testing Squirrel Remotes...");
			var initial = (SquirrelRemote)Config.Instance.SquirrelRemote;

			// Run this in a separate thread so that we get accurate response timing measurements
			// that are not impacted by other sync stuff happening in the main thread.
			var (results, successResults) = await Task.Run(async () =>
			{
				var version = Helper.GetCurrentVersion().ToVersionString();
				var results = await TestReleasesDownload(millisecondsTimeout: 3000, version);
				var successResults = results.Where(x => x.Duration != null).ToArray();
				if(!successResults.Any())
				{
					Log.Warn("Could not download any RELEASES file within 3 seconds. Trying again with longer timeout.");
					results = await TestReleasesDownload(millisecondsTimeout: 10_000, version);
					successResults = results.Where(x => x.Duration != null).ToArray();
				}
				return (results, successResults);
			});

			Log.Info("Test Completed");
			if(!successResults.Any())
			{
				Log.Warn("Could not download any RELEASES file. Connectivity problems?");
				return;
			}
			foreach(var result in results)
			{
				var resultInto = result.Duration.HasValue ? $"{result.Duration}ms" :
					result.Exception != null ? (result.Exception.InnerException ?? result.Exception).Message : "timeout";
				Log.Info($"| {result.Remote}: {resultInto}" + (result.Remote == initial ? " (current)" : ""));
			}

			var best = successResults.OrderBy(x => x.Duration).First();
			var github = successResults.FirstOrDefault(x => x.Remote == SquirrelRemote.Github);

			// If reasonable always prefer GitHub
			if(github != null && (github == best || github.Duration < 2000 || github.Duration * 0.6 < best.Duration))
			{
				Log.Info("Github looks good. Using Github.");
				Config.Instance.SquirrelRemote = (int)SquirrelRemote.Github;
			}
			else
			{
				Log.Info($"Using {best}. Best result.");
				Config.Instance.SquirrelRemote = (int)best.Remote;
			}
			Config.Save();

			HSReplayNetClientAnalytics.OnSquirrelRemoteChanged(initial, (SquirrelRemote)Config.Instance.SquirrelRemote);
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	private static async Task<Result[]> TestReleasesDownload(int millisecondsTimeout, string version)
	{
		var start = DateTime.Now;
		var tasks = Enum.GetValues(typeof(SquirrelRemote)).Cast<SquirrelRemote>().Select(async remote =>
		{
			try
			{
				using var cts = new CancellationTokenSource(millisecondsTimeout);
				await Core.HttpClient.GetAsync(GetReleasesUrl(remote, version), cts.Token);
				return new Result
				{
					Remote = remote,
					Duration = (int?)(DateTime.Now - start).TotalMilliseconds,
				};
			}
			catch(TaskCanceledException)
			{
				Log.Info($"Request for {remote} timed out");
				return new Result { Remote = remote };
			}
			catch(Exception e)
			{
				Log.Error($"Request for {remote} failed: {e}");
				return new Result { Remote = remote, Exception = e};
			}
		});
		return await Task.WhenAll(tasks);
	}

	private record Result
	{
		public SquirrelRemote Remote { get; init; }
		public int? Duration { get; init; }
		public Exception? Exception { get; init; }
	}
}
