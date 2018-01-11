//  *****************************************************************************
//  File:       CreateResource.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/08/2018
//  Author:     Latency McLaughlin
//  Copywrite:  Bio-Hazard Industries - 1998-2016
//  *****************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using ORM_Monitor;

namespace ResourceGenerator {
  internal static partial class Program {
    /// <summary>
    ///   CreateResource
    /// </summary>
    /// <param name="args"></param>
    /// <param name="token"></param>
    private static void CreateResource(IReadOnlyList<string> args, CancellationToken token) {
      if (token.IsCancellationRequested)
        token.ThrowIfCancellationRequested();

      var options = new ParallelOptions {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        CancellationToken = token
      };

	  // Create resource directory
	  Directory.CreateDirectory(CardCollection.TargetPath(Arguments));

      try {
        Parallel.ForEach(Cards.All, options, (kvp, idx) => {
          if (idx.ShouldExitCurrentIteration) {
            idx.Break();
            return;
          }

          // Checkpointing
          if (token.IsCancellationRequested)
            token.ThrowIfCancellationRequested();

          if (kvp.Value.Set == CardSet.CHEAT)
            return;

		  // Critical section
          Monitor.Enter(SetCollection);
          if (!SetCollection.ContainsKey(kvp.Value.Set))
            SetCollection.Add(kvp.Value.Set, new CardCollection(kvp.Value.Set + (kvp.Value.Collectible ? "" : "_NC")));
          SetCollection[kvp.Value.Set].Add(kvp.Value);
          _downloadCount++;
          Monitor.Exit(SetCollection);
        });

        if (SetCollection == null)
          throw new NullReferenceException(MethodBase.GetCurrentMethod().Name);

        Parallel.ForEach(SetCollection.Values, options, cardCollection => {
          // Checkpointing
          if (token.IsCancellationRequested)
            token.ThrowIfCancellationRequested();

          try {
            var tasks = new List<TaskEvent>();

            var loopResult = Parallel.ForEach(cardCollection, options, card => {
              if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

              var taskEvent = new TaskEvent {
			     Name = card.Name,
				 Tag = token,
				 OnRunning = async (sender, eventArgs) => await DownloadTile(card, !string.IsNullOrEmpty(args[2]) && args[2] != "0"),
				 OnCompleted = (sender, eventArgs) => tasks.Remove(sender as TaskEvent)
			  };

              taskEvent.AsyncMonitor();
			  tasks.Add(taskEvent);
            });

            while (!token.IsCancellationRequested && !loopResult.IsCompleted)
              Task.Delay(100, token);

            var t = tasks.Select(x => x.Task).ToArray();
			Task.WaitAll(t, token);

            if (tasks.Any())
              foreach (var z in tasks)
                z.Cancel();
 
			if (!IsMsBuildInvoked)
              Console.WriteLine("Generating {0}...", Path.Combine(CardCollection.TargetPath(Arguments), $"{cardCollection}.res"));

			// Write the stream
			cardCollection.ResourceWriter.Close();
          } catch (OperationCanceledException) { } catch (Exception ex) {
            Debug.WriteLine(ex.Message);
          }
        });
      } catch (OperationCanceledException) { } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }
  }
}
