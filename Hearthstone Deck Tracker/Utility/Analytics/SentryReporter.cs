#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BobsBuddy;
using BobsBuddy.Simulation;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.RemoteData;
using Newtonsoft.Json.Linq;
using Sentry;

#if(SQUIRREL)
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
#endif
#endregion


namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class SentryReporter
	{
		static SentryReporter()
		{
			Log.OnLogLine += AddHDTLogLine;
		}

		private static readonly Regex _debuglineToIgnore = new Regex(@"\|(Player|Opponent|TagChangeActions)\.");
		private static List<string> _recentHDTLog = new List<string>();
		static int LogLinesKept = Remote.Config.Data?.BobsBuddy?.LogLinesKept ?? 100;

		private const string ExtraKey = "hdt";

		// BobsBuddy entities contain reference cycles (Enchantment.AttachedTo), which make serialization throw
		private static readonly Newtonsoft.Json.JsonSerializerSettings SerializerSettings = new()
		{
			ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
			Error = (_, args) => args.ErrorContext.Handled = true,
		};
		private static readonly TimeSpan CrashFlushTimeout = TimeSpan.FromSeconds(5);

		static void AddHDTLogLine(string toLog)
		{
			if(_debuglineToIgnore.IsMatch(toLog))
				return;
			if(_recentHDTLog.Count >= LogLinesKept)
				_recentHDTLog.RemoveAt(0);
			_recentHDTLog.Add(toLog);
		}

		// baked in at build time, see AssemblyMetadata in the csproj
		private static string? GetBuildMetadata(string key) => typeof(SentryReporter).Assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(x => x.Key == key)?.Value;

		public static void Initialize()
		{
			SentrySdk.Init(options =>
			{
				options.Dsn = GetBuildMetadata("SentryDsn") ?? "";
				// Release is left unset so the SDK resolves it from InformationalVersion, which is also
				// where sentry-cli reads it from when it creates the release at build time
				options.Distribution = Helper.GetCurrentVersion().Revision.ToString();
				options.Environment = GetBuildMetadata("SentryEnvironment");
				if(string.IsNullOrWhiteSpace(options.Environment))
				{
#if(SQUIRREL)
					options.Environment = "Squirrel";
#else
					options.Environment = "Portable";
#endif
				}
				options.IsGlobalModeEnabled = true;

				// System.Text.Json ignores public fields, which the BobsBuddy payloads consist of
				options.AddJsonConverter(new NewtonsoftConverter<BobsBuddyData>());
				options.AddJsonConverter(new NewtonsoftConverter<HDTToolsData>());
				options.AddJsonConverter(new NewtonsoftConverter<JObject>());
			});
		}

		public static SentryId CaptureException(Exception ex)
		{
			var plugins = PluginManager.Instance.Plugins.Where(x => x.IsEnabled).ToList();
			ex.Data.Add("active-plugins", plugins.Any() ? string.Join(", ", plugins.Select(x => x.NameAndVersion)) : "none");

			var exception = new SentryEvent(ex);
#if(SQUIRREL)
			exception.SetTag("squirrel", "true");
#else
			exception.SetTag("squirrel", "false");
#endif
			exception.SetTag("hearthstone", Helper.GetHearthstoneBuild()?.ToString() ?? "");
			exception.SetTag("os_arch", RuntimeInformation.OSArchitecture.ToString().ToLower());

			return SentrySdk.CaptureEvent(exception);
		}

		public static void CaptureUserFeedback(SentryId eventId, string message)
		{
			SentrySdk.CaptureFeedback(
				message,
				associatedEventId: eventId != SentryId.Empty ? eventId : null
			);
		}

		// the crash dialog closes right into a shutdown, so give anything still queued a chance to go out
		public static void FlushBeforeShutdown() => SentrySdk.Flush(CrashFlushTimeout);

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

#if(SQUIRREL)
		private static void AddReportContextTags(SentryEvent e, BobsBuddySentryReportContext context)
		{
			e.SetTag("cm_active", context.CMActive.ToString());
			e.SetTag("reconnected_after_snapshot", context.ReconnectedAfterSnapshot.ToString());
			e.SetTag("entities_cleared", context.EntitiesCleared.ToString());
			e.SetTag("snapshot_input_was_set", context.SnapshotInputWasSet.ToString());
			e.SetTag("duos_partial_combat", context.IsDuosPartialCombat.ToString());
		}
#endif

		public static void QueueBobsBuddyTerminalCase(
			Input testInput, Output output, string result, int turn, Region region,
			bool isDuos, bool isOpposingAkazamzarak, BobsBuddySentryReportContext reportContext
		)
		{
#if(SQUIRREL)
			if(BobsBuddyEventsSent >= MaxBobsBuddyEventsPerGame)
				return;

			// Clean up data
			output.ClearListsForReporting(); //ignoring for some temporary debugging

			var data = new BobsBuddyData()
			{
				ShortId = "",
				Turn = turn,
				Result = result,
				ThreadCount = BobsBuddyInvoker.ThreadCount,
				Iterations = output.simulationCount,
				ExitCondition = output.myExitCondition.ToString(),
				Input = testInput,
				UnitTestableVersion = testInput.UnitTestableVersion,
				Output = output,
				Log = ReverseAndClone(_recentHDTLog),
				Region = region,

			};

			var bbEvent = new SentryEvent
			{
				Message = isDuos ?
					$"BobsBuddy {BobsBuddyUtils.VersionString} (Duos): Incorrect Terminal Case: {result}" :
					$"BobsBuddy {BobsBuddyUtils.VersionString}: Incorrect Terminal Case: {result}",
				Level = SentryLevel.Warning,
			};

			bbEvent.SetTag("bobs_buddy_version", BobsBuddyUtils.VersionString);
			bbEvent.SetTag("turn", turn.ToString());
			bbEvent.SetTag("region", data.Region.ToString());
			bbEvent.SetTag("is_duos", isDuos.ToString());
			bbEvent.SetTag("opposing_akazamzarak", isOpposingAkazamzarak.ToString());

			if(testInput.Anomaly != null)
				bbEvent.SetTag("anomaly_card_id", testInput.Anomaly.CardID);

			AddReportContextTags(bbEvent, reportContext);

			bbEvent.SetExtra(ExtraKey, data);
			bbEvent.SetFingerprint(result, BobsBuddyUtils.VersionString, isDuos.ToString());

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
				SentrySdk.CaptureEvent(e);
				HDTToolsEventsSent++;
			}
#endif
		}

#if(SQUIRREL)
		private static void SendQueuedBobsBuddyEvents(string? shortId, string path, Func<SentryEvent, SentryEvent> recode)
		{
			var sent = 0;
			var maxExtraBytes = 0;
			var totalExtraBytes = 0;
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
				var bbData = GetBobsBuddyData(e);
				if(bbData != null)
					bbData.ShortId = shortId;

				var toCapture = WithSerializableExtra(recode(e));
				var extraSize = GetExtraSize(toCapture);
				maxExtraBytes = Math.Max(maxExtraBytes, extraSize);
				totalExtraBytes += extraSize;
				StripIfTooLarge(toCapture, extraSize);

				var eventId = SentrySdk.CaptureEvent(toCapture);
				if(eventId != SentryId.Empty)
					sent++;
				else
					captureFailed++;
				BobsBuddyEventsSent++;
			}
			Influx.OnBobsBuddySentryEventsSent(path, sent, captureFailed, maxExtraBytes, totalExtraBytes);
		}

		private static SentryEvent RecodeIfBothSidesEmpty(SentryEvent e)
		{
			var bbData = GetBobsBuddyData(e);
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

		// Structural guard against back-references that would otherwise re-expand large object
		// graphs into a report (see the CardEntity.simulator incident, which alone blew a single
		// BobsBuddy event up to ~17MB). Anything nested deeper than this is replaced with a marker.
		private const int MaxExtraDepth = 10;

		// Backstop for events that are still too big after depth pruning (e.g. a large flat list).
		private const int MaxExtraSizeBytes = 500_000;

		private static void PruneDeepTokens(JToken token, int depth)
		{
			if(depth >= MaxExtraDepth)
				return;
			switch(token)
			{
				case JObject obj:
					foreach(var property in obj.Properties().ToList())
					{
						if(depth + 1 >= MaxExtraDepth && property.Value is JContainer)
							property.Value = new JValue("[truncated: max depth exceeded]");
						else
							PruneDeepTokens(property.Value, depth + 1);
					}
					break;
				case JArray arr:
					for(var i = 0; i < arr.Count; i++)
					{
						if(depth + 1 >= MaxExtraDepth && arr[i] is JContainer)
							arr[i] = new JValue("[truncated: max depth exceeded]");
						else
							PruneDeepTokens(arr[i], depth + 1);
					}
					break;
			}
		}

		private static JObject? GetSerializedExtra(SentryEvent e) =>
			e.Extra.TryGetValue(ExtraKey, out var extra) ? extra as JObject : null;

		private static int GetExtraSize(SentryEvent e)
			=> GetSerializedExtra(e)?.ToString(Newtonsoft.Json.Formatting.None).Length ?? 0;

		private static void StripIfTooLarge(SentryEvent e, int extraSize)
		{
			if(extraSize <= MaxExtraSizeBytes)
				return;
			var extra = GetSerializedExtra(e);
			if(extra == null)
				return;
			extra["Input"] = "[stripped: payload exceeded size limit]";
			extra["Output"] = "[stripped: payload exceeded size limit]";
			e.SetTag("stripped_for_size", "true");
		}

		private static SentryEvent WithSerializableExtra(SentryEvent e)
		{
			if(!e.Extra.TryGetValue(ExtraKey, out var data) || data == null)
				return e;
			var extra = JObject.FromObject(data, Newtonsoft.Json.JsonSerializer.Create(SerializerSettings));
			PruneDeepTokens(extra, 0);
			e.SetExtra(ExtraKey, extra);
			return e;
		}
#endif

		public static void CaptureBobsBuddyException(Exception ex, Input? input, int turn, bool isDuos, BobsBuddySentryReportContext reportContext)
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
				UnitTestableVersion = input.UnitTestableVersion,
				Log = ReverseAndClone(_recentHDTLog)
			};

			var bbEvent = new SentryEvent(ex)
			{
				Message = isDuos ?
					$"BobsBuddy {BobsBuddyUtils.VersionString} (Duos): {ex.Message}" :
					$"BobsBuddy {BobsBuddyUtils.VersionString}: {ex.Message}",
				Level = SentryLevel.Warning,
			};

			bbEvent.SetTag("bobs_buddy_version", BobsBuddyUtils.VersionString);
			bbEvent.SetTag("turn", turn.ToString());

			AddReportContextTags(bbEvent, reportContext);

			bbEvent.SetExtra(ExtraKey, data);
			bbEvent.SetFingerprint(BobsBuddyUtils.VersionString, isDuos.ToString());

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

		private static BobsBuddyData? GetBobsBuddyData(SentryEvent e) =>
			e.Extra.TryGetValue(ExtraKey, out var extra) ? extra as BobsBuddyData : null;

		private static SentryEvent Recode(SentryEvent original, string message)
		{
			var recoded = new SentryEvent
			{
				Message = message,
				Level = original.Level,
			};
			foreach(var tag in original.Tags)
				recoded.SetTag(tag.Key, tag.Value);
			foreach(var extra in original.Extra)
				recoded.SetExtra(extra.Key, extra.Value);
			return recoded;
		}
#endif

		public static void CaptureHDTToolsExecutionProblem(string problem)
		{
#if(SQUIRREL)
			var data = new HDTToolsData()
			{
				Problem = problem,
				Log = ReverseAndClone(_recentHDTLog)
			};

			var hdttoolsEvent = new SentryEvent
			{
				Message = $"HDTTools {HDTToolsManager.VersionString} Problem: {problem}",
				Level = SentryLevel.Warning,
			};

			hdttoolsEvent.SetTag("hdttools_version", HDTToolsManager.VersionString);
			hdttoolsEvent.SetTag("problem", problem);

			hdttoolsEvent.SetExtra(ExtraKey, data);
			hdttoolsEvent.SetFingerprint(HDTToolsManager.VersionString, problem);

			HDTToolsEvents.Enqueue(hdttoolsEvent);
#endif
		}

		public static void CaptureHDTToolsExitProblem(string exitProblem, List<string> hdtToolsLog)
		{
#if(SQUIRREL)
			var data = new HDTToolsData()
			{
				ExitProblem = exitProblem,
				Log = ReverseAndClone(_recentHDTLog),
				HDTToolsLog = ReverseAndClone(hdtToolsLog)
			};

			var hdttoolsEvent = new SentryEvent
			{
				Message = $"HDTTools {HDTToolsManager.VersionString} Exit Problem: {exitProblem}",
				Level = SentryLevel.Warning,
			};

			hdttoolsEvent.SetTag("hdttools_version", HDTToolsManager.VersionString);
			hdttoolsEvent.SetTag("exit_problem", exitProblem);

			hdttoolsEvent.SetExtra(ExtraKey, data);
			hdttoolsEvent.SetFingerprint(HDTToolsManager.VersionString, exitProblem);

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

		private class NewtonsoftConverter<T> : JsonConverter<T>
		{
			public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> throw new NotSupportedException();

			public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
				=> writer.WriteRawValue(Newtonsoft.Json.JsonConvert.SerializeObject(value, SerializerSettings));
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
			// Captured explicitly (rather than derived from Input) so it survives StripIfTooLarge
			public string? UnitTestableVersion { get; set; }
			public Output? Output { get; set; }

			public Region Region { get; set; }

			public List<string>? Log { get; set; }
		}
	}

	// Tracks how a BobsBuddyInvoker instance's input was obtained.
	internal record BobsBuddySentryReportContext(
		// The China module is running HDT Tools for this game (Core.Game.IsChinaModuleActive).
		bool CMActive,

		// A reconnect was detected after this instance snapshotted its board state.
		bool ReconnectedAfterSnapshot,

		// Core.Game.Entities was cleared mid-game.
		bool EntitiesCleared,

		// SnapshotBoardState found _input already set before this instance's first snapshot.
		bool SnapshotInputWasSet,

		// A duos combat that ran with a teammate missing from the input.
		bool IsDuosPartialCombat
	);
}
