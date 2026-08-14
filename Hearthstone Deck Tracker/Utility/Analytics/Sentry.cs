#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
			Client.Compression = true;
			Client.Timeout = TimeSpan.FromSeconds(15);
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
			exception.Tags.Add("os_arch", RuntimeInformation.OSArchitecture.ToString().ToLower());

			return Client.Capture(exception);
		}

#if(SQUIRREL)
		private const int MaxBobsBuddyEventsPerGame = 5;
		private const int MaxBobsBuddyExceptionsPerGame = 5;
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
			if(BobsBuddyEventsSent >= MaxBobsBuddyEventsPerGame)
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
			Influx.OnBobsBuddySentryEventQueued("terminal_case", isDuos);
#endif
		}

		public static void FlushBattlegroundsEvents(string? shortId, bool logContainsStateComplete, bool isDuos)
		{
#if(SQUIRREL)
			if(logContainsStateComplete)
			{
				SendQueuedBobsBuddyEvents(shortId, "normal", RecodeIfBothSidesEmpty);
				SendQueuedHDTToolsEvents();
			}
			else if(!isDuos)
				SendQueuedBobsBuddyEvents(shortId, "state_complete_false", RecodeAsStateCompleteFalse);
			else
				Influx.OnBobsBuddySentryEventsDropped("duos_no_state_complete", BobsBuddyEvents.Count);
#endif
			ClearBattlegroundsEvents();
		}

		public static void DropBattlegroundsEvents(string reason)
		{
#if(SQUIRREL)
			Influx.OnBobsBuddySentryEventsDropped(reason, BobsBuddyEvents.Count);
#endif
			ClearBattlegroundsEvents();
		}

		private static void SendQueuedHDTToolsEvents()
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

#if(SQUIRREL)
		private static void SendQueuedBobsBuddyEvents(string? shortId, string path, Func<SentryEvent, SentryEvent> recode)
		{
			var sent = 0;
			var captureFailed = 0;
			while(BobsBuddyEvents.Count > 0)
			{
				if(BobsBuddyEventsSent >= MaxBobsBuddyEventsPerGame)
				{
					Influx.OnBobsBuddySentryEventsDropped("cap", BobsBuddyEvents.Count);
					ClearBobsBuddyEvents();
					break;
				}
				var e = BobsBuddyEvents.Dequeue();
				((BobsBuddyData)e.Extra).ShortId = shortId;

				var eventId = Client.Capture(WithSerializableExtra(recode(e)));
				if(eventId != null)
					sent++;
				else
					captureFailed++;
				BobsBuddyEventsSent++;
			}
			Influx.OnBobsBuddySentryEventsSent(path, sent, captureFailed);
		}

		private static SentryEvent RecodeIfBothSidesEmpty(SentryEvent e)
		{
			var bbData = (BobsBuddyData)e.Extra;
			if(
				bbData != null && bbData.Input != null &&
				bbData.Turn > 5 &&
				bbData.Input.Player.Side.Count == 0 &&
				bbData.Input.Opponent.Side.Count == 0
			)
				return Recode(e, $"BobsBuddy {BobsBuddyUtils.VersionString}: Both Sides Empty");
			return e;
		}

		private static SentryEvent RecodeAsStateCompleteFalse(SentryEvent e) =>
			Recode(e, $"BobsBuddy {BobsBuddyUtils.VersionString}: Incorrect Terminal Case: StateCompleteFalse");

		private static SentryEvent WithSerializableExtra(SentryEvent e)
		{
			if(e.Extra == null)
				return e;
			// BobsBuddy entities contain reference cycles (Enchantment.AttachedTo), which make SharpRaven's serialization throw
			var serializer = JsonSerializer.Create(new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Error = (_, args) => args.ErrorContext.Handled = true,
			});
			e.Extra = JObject.FromObject(e.Extra, serializer);
			return e;
		}

		private static SentryEvent Recode(SentryEvent e, string message) =>
			new SentryEvent(new SentryMessage(message))
			{
				Level = e.Level,
				Tags = e.Tags,
				Extra = e.Extra,
			};
#endif

		public static void CaptureBobsBuddyException(Exception ex, Input? input, int turn, bool isDuos)
		{
#if(SQUIRREL)
			if(BobsBuddyExceptionsSent >= MaxBobsBuddyExceptionsPerGame)
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

			// exceptions share the queue and the per-game cap, so they count toward the funnel too
			Influx.OnBobsBuddySentryEventQueued("exception", isDuos);
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

		private static void ClearBattlegroundsEvents()
		{
			ClearBobsBuddyEvents();
			ClearHDTToolsEvents();
#if(SQUIRREL)
			// Reset the counters after each game
			BobsBuddyEventsSent = 0;
			BobsBuddyExceptionsSent = 0;
#endif
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
		}
	}
}
