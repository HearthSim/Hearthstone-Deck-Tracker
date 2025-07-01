#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BobsBuddy;
using BobsBuddy.Simulation;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using SharpRaven;
using SharpRaven.Data;

#if(SQUIRREL)
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
#endif
#endregion


namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Sentry
	{
		static Sentry()
		{
			Client.Release = Helper.GetCurrentVersion().ToVersionString(true);
			Log.OnLogLine += AddHDTLogLine;
		}

		private static readonly Regex _debuglineToIgnore = new Regex(@"\|(Player|Opponent|TagChangeActions)\.");
		private static List<string> _recentHDTLog = new List<string>();
		static int LogLinesKept = Remote.Config.Data?.BobsBuddy?.LogLinesKept ?? 100;

		static void AddHDTLogLine(string toLog)
		{
			if(_debuglineToIgnore.IsMatch(toLog))
				return;
			if(_recentHDTLog.Count >= LogLinesKept)
				_recentHDTLog.RemoveAt(0);
			_recentHDTLog.Add(toLog);
		}

		private static readonly RavenClient Client = new RavenClient("https://0a6c07cee8d141f0bee6916104a02af4:883b339db7b040158cdfc42287e6a791@app.getsentry.com/80405");

		public static string CaptureException(Exception ex)
		{
			var plugins = PluginManager.Instance.Plugins.Where(x => x.IsEnabled).ToList();
			ex.Data.Add("active-plugins", plugins.Any() ? string.Join(", ", plugins.Select(x => x.NameAndVersion)) : "none");

			var exception = new SentryEvent(ex);
#if(SQUIRREL)
			exception.Tags.Add("squirrel", "true");
#else
			exception.Tags.Add("squirrel", "false");
#endif
			exception.Tags.Add("hearthstone", Helper.GetHearthstoneBuild()?.ToString());

			return Client.Capture(exception);
		}

#if(SQUIRREL)
		private const int MaxBobsBuddyEvents = 10;
		private const int MaxBobsBuddyExceptions = 1;
		private const int MaxHDTToolsEvents = 10;
		private static int BobsBuddyEventsSent;
		private static int BobsBuddyExceptionsSent;
		private static int HDTToolsEventsSent;
#endif
		private static Queue<SentryEvent> BobsBuddyEvents = new Queue<SentryEvent>();
		private static Queue<SentryEvent> HDTToolsEvents = new Queue<SentryEvent>();

		public static void QueueBobsBuddyTerminalCase(
			Input testInput, Output output, string result, int turn, Region region,
			bool isDuos, bool isOpposingAkazamzarak
		)
		{
#if(SQUIRREL)
			if(BobsBuddyEventsSent >= MaxBobsBuddyEvents)
				return;

			// Clean up data
			output.ClearListsForReporting(); //ignoring for some temporary debugging

			var msg = new SentryMessage(isDuos ?
				$"BobsBuddy {BobsBuddyUtils.VersionString} (Duos): Incorrect Terminal Case: {result}" :
				$"BobsBuddy {BobsBuddyUtils.VersionString}: Incorrect Terminal Case: {result}"
			);

			var data = new BobsBuddyData()
			{
				ShortId = "",
				Turn = turn,
				Result = result,
				ThreadCount = BobsBuddyInvoker.ThreadCount,
				Iterations = output.simulationCount,
				ExitCondition = output.myExitCondition.ToString(),
				Input = testInput,
				Output = output,
				Log = ReverseAndClone(_recentHDTLog),
				Region = region,

			};

			var tags = new Dictionary<string, string>() {
				{"bobs_buddy_version", BobsBuddyUtils.VersionString},
				{"turn", turn.ToString()},
				{"region", data.Region.ToString()},
				{"is_duos", isDuos.ToString()},
				{"opposing_akazamzarak", isOpposingAkazamzarak.ToString()}
			};

			if(testInput.Anomaly != null)
			{
				tags["anomaly_card_id"] = testInput.Anomaly.CardID;
			}

			var bbEvent = new SentryEvent(msg)
			{
				Level = ErrorLevel.Warning,
				Tags = tags,
				Extra = data,
			};

			bbEvent.Fingerprint.Add(result);
			bbEvent.Fingerprint.Add(BobsBuddyUtils.VersionString);
			bbEvent.Fingerprint.Add(isDuos.ToString());

			BobsBuddyEvents.Enqueue(bbEvent);
#endif
		}

		public static void SendQueuedBattlegroundsEvents(string? shortId = null)
		{
#if(SQUIRREL)
			SendQueuedBobsBuddyEvents(shortId);
			SendQueuedHDTToolsEvents();
#endif
		}

		public static void SendQueuedHDTToolsEvents()
		{
#if(SQUIRREL)
			while(HDTToolsEvents.Count > 0)
			{
				if(HDTToolsEventsSent >= MaxHDTToolsEvents)
				{
					ClearHDTToolsEvents();
					break;
				}
				var e = HDTToolsEvents.Dequeue();
				Client.Capture(e);
				HDTToolsEventsSent++;
			}
#endif
		}

		public static void SendQueuedBobsBuddyEvents(string? shortId)
		{
#if(SQUIRREL)
			while(BobsBuddyEvents.Count > 0)
			{
				if(BobsBuddyEventsSent >= MaxBobsBuddyEvents)
				{
					ClearBobsBuddyEvents();
					break;
				}
				var e = BobsBuddyEvents.Dequeue();
				((BobsBuddyData)e.Extra).ShortId = shortId;
				Client.Capture(e);
				BobsBuddyEventsSent++;
			}
#endif
		}

		public static void CaptureBobsBuddyException(Exception ex, Input? input, int turn, bool isDuos)
		{
#if(SQUIRREL)
			if(BobsBuddyExceptionsSent >= MaxBobsBuddyExceptions)
				return;
			if(input == null)
				return;
			BobsBuddyExceptionsSent++;

			// Clean up data
			var data = new BobsBuddyData()
			{
				ShortId = "",
				Turn = turn,
				ThreadCount = BobsBuddyInvoker.ThreadCount,
				Input = input,
				Log = ReverseAndClone(_recentHDTLog)
			};

			var tags = new Dictionary<string, string>() {
				{"bobs_buddy_version", BobsBuddyUtils.VersionString},
				{"turn", turn.ToString()},
			};

			var bbEvent = new SentryEvent(ex)
			{
				Level = ErrorLevel.Warning,
				Tags = tags,
				Extra = data,
			};

			bbEvent.Message = isDuos ?
				$"BobsBuddy {BobsBuddyUtils.VersionString} (Duos): {bbEvent.Message}":
				$"BobsBuddy {BobsBuddyUtils.VersionString}: {bbEvent.Message}";
			bbEvent.Fingerprint.Add(BobsBuddyUtils.VersionString);
			bbEvent.Fingerprint.Add(isDuos.ToString());

			BobsBuddyEvents.Enqueue(bbEvent);
#endif
		}

#if(SQUIRREL)
		private static List<string> ReverseAndClone(List<string> toReverseAndClone)
		{
			var toReturn = toReverseAndClone.ToList();
			toReturn.Reverse();
			return toReturn;
		}
#endif

		public static void CaptureHDTToolsExecutionProblem(string problem)
		{
#if(SQUIRREL)
			var msg = new SentryMessage($"HDTTools {HDTToolsManager.VersionString} Problem: {problem}");

			var tags = new Dictionary<string, string>() {
				{"hdttools_version", HDTToolsManager.VersionString},
				{"problem", problem}
			};

			var data = new HDTToolsData()
			{
				Problem = problem,
				Log = ReverseAndClone(_recentHDTLog)
			};

			var hdttoolsEvent = new SentryEvent(msg)
			{
				Level = ErrorLevel.Warning,
				Tags = tags,
				Extra = data
			};
			hdttoolsEvent.Fingerprint.Add(HDTToolsManager.VersionString);
			hdttoolsEvent.Fingerprint.Add(problem);

			HDTToolsEvents.Enqueue(hdttoolsEvent);
#endif
		}

		public static void CaptureHDTToolsExitProblem(string exitProblem, List<string> hdtToolsLog)
		{
#if(SQUIRREL)
			var msg = new SentryMessage($"HDTTools {HDTToolsManager.VersionString} Exit Problem: {exitProblem}");

			var tags = new Dictionary<string, string>() {
				{"hdttools_version", HDTToolsManager.VersionString},
				{"exit_problem", exitProblem}
			};

			var data = new HDTToolsData()
			{
				ExitProblem = exitProblem,
				Log = ReverseAndClone(_recentHDTLog),
				HDTToolsLog = ReverseAndClone(hdtToolsLog)
			};

			var hdttoolsEvent = new SentryEvent(msg)
			{
				Level = ErrorLevel.Warning,
				Tags = tags,
				Extra = data
			};
			hdttoolsEvent.Fingerprint.Add(HDTToolsManager.VersionString);
			hdttoolsEvent.Fingerprint.Add(exitProblem);

			HDTToolsEvents.Enqueue(hdttoolsEvent);
#endif
		}

		public static void ClearBobsBuddyEvents() => BobsBuddyEvents.Clear();
		public static void ClearHDTToolsEvents() => HDTToolsEvents.Clear();

		public static void ClearBattlegroundsEvents()
		{
			ClearBobsBuddyEvents();
			ClearHDTToolsEvents();
		}

		private class HDTToolsData
		{
			public string? Problem { get; set; }
			public string? ExitProblem { get; set; }
			public List<string>? Log { get; set; }
			public List<string>? HDTToolsLog { get; set; }
		}

		private class BobsBuddyData
		{
			public string? ShortId { get; set; }
			public int Turn { get; set; }
			public string? Result { get; set; }
			public int ThreadCount { get; set; }
			public int Iterations { get; set; }
			public string? ExitCondition { get; set; }
			public Input? Input { get; set; }
			public string? UnitTestableVersion => Input?.UnitTestableVersion;
			public Output? Output { get; set; }

			public Region Region { get; set; }

			public List<string>? Log { get; set; }
			public string Replay => $"https://hsreplay.net/replay_debug/{ShortId}#turn={Turn}b";
		}
	}
}
