#region

using System;
using System.Collections.Generic;
using System.Linq;
using BobsBuddy;
using BobsBuddy.Simulation;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using SharpRaven;
using SharpRaven.Data;

#endregion


namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
	internal class Sentry
	{
		static Sentry()
		{
			Client.Release = Helper.GetCurrentVersion().ToVersionString(true);
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

		private const int MaxBobsBuddyEvents = 10;
		private static int BobsBuddyEventsSent;
		private static Queue<SentryEvent> BobsBuddyEvents = new Queue<SentryEvent>();
		public static void QueueBobsBuddyTerminalCase(TestInput testInput, TestOutput output, string result, int turn, List<string> debugLog)
		{
			if(BobsBuddyEventsSent >= MaxBobsBuddyEvents)
				return;
			// Clean up data
			testInput.RemoveSelfReferencesFromMinions();
			output.ClearListsForReporting(); //ignoring for some temporary debugging
			output.results.Clear();

			var msg = new SentryMessage($"BobsBuddy {BobsBuddyUtils.VersionString}: Incorrect Terminal Case: {result}");

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
				Log = debugLog
			};

			BobsBuddyEvents.Enqueue(new SentryEvent(msg)
			{
				Level = ErrorLevel.Warning,
				Extra = data
			});
		}

		public static void SendQueuedBobsBuddyEvents(string shortId)
		{
			while(BobsBuddyEvents.Count > 0)
			{
				if(BobsBuddyEventsSent >= MaxBobsBuddyEvents)
				{
					BobsBuddyEvents.Clear();
					break;
				}
				var e = BobsBuddyEvents.Dequeue();
				((BobsBuddyData)e.Extra).ShortId = shortId;
				Client.Capture(e);
				BobsBuddyEventsSent++;
			}
		}

		private class BobsBuddyData
		{
			public string ShortId { get; set; }
			public int Turn { get; set; }
			public string Result { get; set; }
			public int ThreadCount { get; set; }
			public int Iterations { get; set; }
			public string ExitCondition { get; set; }
			public TestInput Input { get; set; }
			public TestOutput Output { get; set; }

			public List<string> Log { get; set; }
		}
	}
}
