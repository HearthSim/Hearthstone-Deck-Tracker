//  *****************************************************************************
//  File:       Main.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/13/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************


#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ORM_Monitor;
using ORM_Monitor.Events;

#endregion


namespace ResourceGenerator
{
    internal static partial class Program
    {
        internal static string[] Arguments;
        internal static bool IsMsBuildInvoked;


        /// <summary>
        ///   Main entry point.
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine($@"Usage:  {args[0]} <path> <folder> <IsOverwriten> [msbuild]");
                return;
            }

            var tokenSource = new CancellationTokenSource();
            var tasks = new List<TaskEvent>();
            Arguments = args;
            IsMsBuildInvoked = Arguments.Length == 4 && Arguments[3] == "msbuild";
            ConsoleCancelEventHandler onCancel = null;

            try
            {
                if (!IsMsBuildInvoked)
                {
                    var t0 = new TaskEvent
                    {
                        Name = "t0",
                        Tag = tasks,
                        OnRunning = async (sender, tea) =>
                        {
                            Console.WriteLine(@"Press `CTRL+C' to abort the process...");

                            onCancel = (o, e) =>
                            {
                                e.Cancel = true;
                                tea.TokenSource?.Cancel();
                            };

                            // Establish an event handler to process key press events.
                            Console.CancelKeyPress += onCancel;

                            while (!tea.TokenSource.IsCancellationRequested)
                            {
                                if (Console.KeyAvailable)
                                    Console.ReadKey(true);
                                else
                                    await Task.Delay(250, tea.TokenSource.Token);
                            }
                        },
                        OnExited = (sender, tea) =>
                        {
                            tea.Task.Dispose();
                            (tea.Tag as List<TaskEvent>)?.Remove((sender as ExitedEvent)?.TaskEvent);
                        }
                    };

                    tasks.Add(t0);
                    t0.AsyncMonitor();
                }

                // =================================================================================

                var t1 = new TaskEvent
                {
                    Name = "t1",
                    Tag = tasks,
                    OnRunning = (sender, tea) =>
                    {
                        var options = new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount,
                            CancellationToken = tea.TokenSource.Token
                        };
                        var cardCount = InitializeCollection(out var setCollection);

                        CreateResources(args, options, setCollection);

                        var dlCount = setCollection.Sum(kvp => kvp.Value.DownloadCount);
                        Console.WriteLine($@"Downloaded information for ({dlCount}/{cardCount}) file{(cardCount > 1 ? "s" : string.Empty)}.");
                    },
                    OnExited = (sender, tea) =>
                    {
                        tea.Task.Dispose();
                        (tea.Tag as List<TaskEvent>)?.Remove((sender as ExitedEvent)?.TaskEvent);
                    }
                };

                tasks.Add(t1);
                t1.AsyncMonitor();

                // Wait for any the tasks to finish and send the other one a cancellation signal.
                var b = Task.WaitAny(tasks.Select(x => x.Task).ToArray(), tokenSource.Token);
                if (b == 0)
                    Console.CancelKeyPress -= onCancel;
                tasks[tasks.Count > 1 && b == 0 ? 1 : 0].Cancel();
            } catch (OperationCanceledException)
            {
                Debug.WriteLine("");
            } catch (Exception ex)
            {
                Debug.Print("Exception messages:");
                var ie = ex.InnerException;
                while (ie != null)
                {
                    Debug.Print($"   {ie.GetType().Name}: {ie.Message}");
                    ie = ie.InnerException;
                }
            } finally
            {
                Task.WaitAll(tasks.Select(y => y.Task).ToArray());

                Debug.WriteLine(@"All Tasks Completed");

                tokenSource.Dispose();

                if (!IsMsBuildInvoked)
                {
                    Console.WriteLine(@"Press any key to continue...");
                    Console.ReadKey(true);
                }
            }
        }
    }
}
