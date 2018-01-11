//  *****************************************************************************
//  File:       Main.cs
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
using System.Threading;
using System.Threading.Tasks;
using ORM_Monitor;

namespace ResourceGenerator {
  internal static partial class Program {
    internal static string[] Arguments;
    internal static bool IsMsBuildInvoked;

    /// <summary>
    ///   Main entry point.
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args) {
      if (args.Length < 3 || args.Length > 4) {
        Console.WriteLine($@"Usage:  {args[0]} <path> <folder> <IsOverwriten> [msbuild]");
        return;
      }

      FileSystemWatcher mWatcher = null;
      Arguments = args;
      IsMsBuildInvoked = Arguments.Length == 4 && Arguments[3] == "msbuild";

      // ReSharper disable once ConvertToLocalFunction
	  ConsoleCancelEventHandler onCancel = (o, e) => {
	    e.Cancel = true;
	    // ReSharper disable AccessToDisposedClosure
		Monitor.Enter(_tokenSource);
		_tokenSource.Cancel();
		Monitor.Exit(_tokenSource);
	    // ReSharper restore AccessToDisposedClosure
	  };

      var tasks = new List<TaskEvent>();

	  try {
        if (!IsMsBuildInvoked) {
          var t0 = new TaskEvent {
            Name = "t0",
            OnRunning = (sender, tea) => {
              Console.WriteLine(@"Press `CTRL+C' to abort the process...");

              // Establish an event handler to process key press events.
              Console.CancelKeyPress += onCancel;

              while (!tea.TokenSource.IsCancellationRequested) {
                if (Console.KeyAvailable)
                  Console.ReadKey(true);
                else
                  Thread.Sleep(250);
              }
            },
            OnExited = (sender, tea) => {
              // ReSharper disable once AccessToModifiedClosure
              tasks.Remove(sender as TaskEvent);
            }
          };

          tasks.Add(t0);
          t0.AsyncMonitor();
        }

        // =================================================================================

        var t1 = new TaskEvent {
          Name = "t1",
		  OnRunning = (sender, tea) => {
            mWatcher = CreateWatcher(Path.Combine(args[0], args[1]));
            CreateResource(args, tea.TokenSource.Token);
          },
          OnCompleted = (sender, tea) => {
            tea.TokenSource.Cancel();

			if (!IsMsBuildInvoked)
              Console.WriteLine($@"Downloaded information for {_downloadCount} files.");
          },
          OnExited = (sender, tea) => tasks.Remove(sender as TaskEvent)
        };

	    tasks.Add(t1);
        t1.AsyncMonitor();

	    // Wait for any the tasks to finish and send the other one a cancellation signal.
        var b = Task.WaitAny(tasks.Select(x => x.Task).ToArray(), _tokenSource.Token);
		if (b == 0)
		  Console.CancelKeyPress -= onCancel;
		tasks[tasks.Count > 1 && b == 0 ? 1 : 0].Cancel();

		// Wait for all to complete.
	    Task.WaitAll(tasks.Select(y => y.Task).ToArray());

        Debug.WriteLine(@"All Tasks Completed");
      } catch (OperationCanceledException) {
        Debug.WriteLine("");
      } catch (Exception ex) {
        Debug.Print("Exception messages:");
        var ie = ex.InnerException;
        while (ie != null) {
          Debug.Print($"   {ie.GetType().Name}: {ie.Message}");
          ie = ie.InnerException;
        }
        //Environment.Exit(1);
      } finally {
	    tasks.Clear();
        tasks = null;

		Monitor.Enter(mWatcher);
		mWatcher?.Dispose();
		Monitor.Exit(mWatcher);
        mWatcher = null;

		Monitor.Enter(_tokenSource);
        _tokenSource?.Dispose();
		Monitor.Exit(_tokenSource);
        _tokenSource = null;

        if (!IsMsBuildInvoked) {
          Console.WriteLine(@"Press any key to continue...");
          Console.ReadKey(true);
        }
      }
    }
  }
}
