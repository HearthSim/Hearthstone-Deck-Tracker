using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Utility.Updating;

// Used in Config.cs as int.
// Don't delete value of this enum. If one becomes obsolete, comment it out and continue the
// next one on the next higher value. GetCurrentMirror will fall back to GitHub if no enum
// value can be found for the stored int.
public enum UpdateMirror
{
	Github = 0,
	AwsHongKong = 1,
}

internal static partial class Updater
{
#if(VELOPACK)
	public static readonly Dictionary<UpdateMirror, string> MirrorUrls = new()
	{
		[UpdateMirror.Github] = "https://github.com/HearthSim/HDT-Velopack-Releases",
		[UpdateMirror.AwsHongKong] = "https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com/velopack"
	};

	private const string ReleaseFeedFileName = "releases.win.json";
#else
	public static readonly Dictionary<UpdateMirror, string> MirrorUrls = new()
	{
		[UpdateMirror.Github] = "https://github.com/HearthSim/HDT-Releases",
		[UpdateMirror.AwsHongKong] = "https://hdt-downloads-hongkong.s3.ap-east-1.amazonaws.com"
	};

	private const string ReleaseFeedFileName = "RELEASES";
#endif

	private static string GetReleaseFeedUrl(UpdateMirror mirror, string version)
	{
		var url = MirrorUrls[mirror];
		if(mirror == UpdateMirror.Github)
			return $"{url}/releases/download/v{version}/{ReleaseFeedFileName}";
		return $"{url}/{ReleaseFeedFileName}";
	}

	public static (UpdateMirror, string) GetCurrentMirror()
	{
#if(DEV)
		return (UpdateMirror.Github, MirrorUrls[UpdateMirror.Github]);
#else
		var mirror = (UpdateMirror)Config.Instance.SquirrelRemote;
		if(!MirrorUrls.TryGetValue(mirror, out var url))
			return (UpdateMirror.Github, MirrorUrls[UpdateMirror.Github]);
		return (mirror, url);
#endif
	}

	private static bool _didFindBestMirror;
	public static async void FindBestMirror()
	{
#if(DEV)
		// dev versions only exist on GitHub, so the speed test would always fail it
		return;
#else
		if(_didFindBestMirror)
		{
			// We only want to do this at most once per session
			return;
		}
		_didFindBestMirror = true;
		try
		{
			// Wait for HDT to be up and running. Not strictly necessary, but no reason to impact startup.
			// Will also make the logging a little cleaner.
			while(!Core.Initialized)
				await Task.Delay(100);

			Log.Info("Testing update mirrors...");
			var initial = (UpdateMirror)Config.Instance.SquirrelRemote;

			// Run this in a separate thread so that we get accurate response timing measurements
			// that are not impacted by other sync stuff happening in the main thread.
			var (results, successResults) = await Task.Run(async () =>
			{
				var version = Helper.GetCurrentVersion().ToVersionString();
				var results = await TestMirrors(millisecondsTimeout: 3000, version);
				var successResults = results.Where(x => x.Duration != null).ToArray();
				if(!successResults.Any())
				{
					Log.Warn($"Could not download any {ReleaseFeedFileName} within 3 seconds. Trying again with longer timeout.");
					results = await TestMirrors(millisecondsTimeout: 10_000, version);
					successResults = results.Where(x => x.Duration != null).ToArray();
				}
				return (results, successResults);
			});

			Log.Info("Test Completed");
			if(!successResults.Any())
			{
				Log.Warn($"Could not download any {ReleaseFeedFileName}. Connectivity problems?");
				Influx.OnSquirrelFindBestRemote("ConnectivityProblems", "None");
				return;
			}
			foreach(var result in results)
			{
				var resultInto = result.Duration.HasValue ? $"{result.Duration}ms" :
					result.Exception != null ? (result.Exception.InnerException ?? result.Exception).Message : "timeout";
				Log.Info($"| {result.Mirror}: {resultInto}" + (result.Mirror == initial ? " (current)" : ""));
			}

			var best = successResults.OrderBy(x => x.Duration).First();
			var github = successResults.FirstOrDefault(x => x.Mirror == UpdateMirror.Github);

			// If reasonable always prefer GitHub
			if(github != null && (github == best || github.Duration < 2000 || github.Duration * 0.6 < best.Duration))
			{
				Log.Info("Github looks good. Using Github.");
				Config.Instance.SquirrelRemote = (int)UpdateMirror.Github;
			}
			else
			{
				Log.Info($"Using {best}. Best result.");
				Config.Instance.SquirrelRemote = (int)best.Mirror;
			}
			Config.Save();

			Influx.OnSquirrelFindBestRemote("Success", best.Mirror.ToString());
			HSReplayNetClientAnalytics.OnUpdateMirrorChanged(initial, (UpdateMirror)Config.Instance.SquirrelRemote);
		}
		catch(Exception e)
		{
			Influx.OnSquirrelFindBestRemote("Error", "None");
			Log.Error(e);
		}
#endif
	}

	private static async Task<Result[]> TestMirrors(int millisecondsTimeout, string version)
	{
		var start = DateTime.Now;
		var tasks = Enum.GetValues(typeof(UpdateMirror)).Cast<UpdateMirror>().Select(async mirror =>
		{
			try
			{
				using var cts = new CancellationTokenSource(millisecondsTimeout);
				await Core.HttpClient.GetAsync(GetReleaseFeedUrl(mirror, version), cts.Token);
				return new Result
				{
					Mirror = mirror,
					Duration = (int?)(DateTime.Now - start).TotalMilliseconds,
				};
			}
			catch(TaskCanceledException)
			{
				Log.Info($"Request for {mirror} timed out");
				return new Result { Mirror = mirror };
			}
			catch(Exception e)
			{
				Log.Error($"Request for {mirror} failed: {e}");
				return new Result { Mirror = mirror, Exception = e};
			}
		});
		return await Task.WhenAll(tasks);
	}

	private record Result
	{
		public UpdateMirror Mirror { get; init; }
		public int? Duration { get; init; }
		public Exception? Exception { get; init; }
	}
}
