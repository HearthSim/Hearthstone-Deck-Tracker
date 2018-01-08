//  *****************************************************************************
//  File:       Program.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/04/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using ORM_Monitor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ResourceGenerator {
  internal class Program {
    private static int _downloadCount;
    private static List<Task> _tasks = new List<Task>();
    private static readonly CancellationTokenSource TokenSource = new CancellationTokenSource();

    /// <summary>
    ///   Main entry point.
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args) {

      if (args.Length < 3 || args.Length > 4) {
        Console.WriteLine(args.Length + " " + args[3]);
        Console.WriteLine("Usage:  <*.exe> <path> <folder> <IsOverwriten>");
        return;
      }

      try {
        if (args.Length == 4 && args[3] != "msbuild") {
          var t0 = new TaskEvent {
            Name = "t0",
            OnRunning = (sender, tea) => {
              if (!(tea.Tag is CancellationTokenSource tSrc))
                throw new NullReferenceException($"Tag:  {MethodBase.GetCurrentMethod().Name}");

              Console.WriteLine(@"Press `CTRL+C' to abort the process...");

              // Establish an event handler to process key press events.
              Console.CancelKeyPress += (o, e) => {
                e.Cancel = true;
                // ReSharper disable once AccessToDisposedClosure
                tSrc.Cancel();
              };

              while (!tSrc.IsCancellationRequested) {
                if (Console.KeyAvailable)
                  Console.ReadKey(true);
                else
                  Thread.Sleep(250);
              }
            },
            OnExited = (sender, tea) => _tasks.Remove(tea.Task),
            Tag = TokenSource
          };

          _tasks.Add(t0.AsyncMonitor());
        }

        // =================================================================================

        var t1 = new TaskEvent {
          Name = "t1",
          // ReSharper disable once AccessToDisposedClosure
          OnRunning = (sender, tea) => CreateResource(args, TokenSource.Token),
          OnCompleted = (sender, tea) => {
            if (!(tea.Tag is CancellationTokenSource tSrc))
              throw new NullReferenceException($"Tag:  {MethodBase.GetCurrentMethod().Name}");

            tSrc.Cancel();
            Console.WriteLine($@"Downloaded information for {_downloadCount} files.");
          },
          OnExited = (sender, tea) => _tasks.Remove(tea.Task),
          Tag = TokenSource
        };

        _tasks.Add(t1.AsyncMonitor());

        // Wait for all the tasks to finish.
        Task.WaitAll(_tasks.ToArray(), TokenSource.Token);

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
		// Synchronize exited callbacks back to main thread.
        while (_tasks.Count > 0)
          Task.Delay(250);

        _tasks = null;
        TokenSource.Dispose();

        if (args.Length != 4 || args.Length == 4 && args[3] != "msbuild") {
		  Console.WriteLine(@"Press any key to continue...");
		  Console.ReadKey(true);
		}
      }
    }


    /// <summary>
    ///   CreateResource
    /// </summary>
    /// <param name="args"></param>
    /// <param name="token"></param>
    private static void CreateResource(IReadOnlyList<string> args, CancellationToken token) {
      if (token.IsCancellationRequested)
        token.ThrowIfCancellationRequested();

      var genDir = Path.Combine(args[0], "Generated", args[1]);
      var dict = new Dictionary<string, List<Card>>();
      var obj = new object();

      Directory.CreateDirectory(genDir);

      var options = new ParallelOptions {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
        CancellationToken = token
      };

      try {
        Parallel.ForEach(Cards.All, options, card => {
          if (token.IsCancellationRequested)
            token.ThrowIfCancellationRequested();

          if (card.Value.Set == CardSet.CHEAT)
            return;

          var key = card.Value.Set + (card.Value.Collectible ? "" : "_NC");
          lock (obj) {
            if (!dict.ContainsKey(key))
              dict[key] = new List<Card>();
            dict[key].Add(card.Value);
          }
        });

        Parallel.ForEach(dict.Keys, options, set => {
          if (token.IsCancellationRequested)
            token.ThrowIfCancellationRequested();

          var file = $"{genDir}\\{set}.res";

          try {
            ResourceWriter[] rw = {
              new ResourceWriter(file)
            };

            Console.WriteLine($@"Generating {file}...");

            var collection = dict[set].Select(card => new Tuple<IResourceWriter, Card>(rw[0], card)).ToList();
            var setCount = dict[set].Count;

            Parallel.ForEach(collection, options, async tuple => {
              if (token.IsCancellationRequested)
                token.ThrowIfCancellationRequested();

              await DownloadTile(tuple, set, Path.Combine(args[0], args[1]), !string.IsNullOrEmpty(args[2]) && args[2] != "0");

              Interlocked.Decrement(ref setCount);
            });

            while (!token.IsCancellationRequested) {
              if (setCount == 0) {
                rw[0].Dispose();
                rw[0] = null;
                break;
              }
            }
          } catch (OperationCanceledException) {
          } catch (Exception ex) {
            Debug.WriteLine(ex.Message);
          }
        });
      } catch (OperationCanceledException) {
	  } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }


    private static async Task DownloadTile(Tuple<IResourceWriter, Card> tuple, string set, string tilesDir, bool overwrite) {
      var rw = tuple.Item1;
      var card = tuple.Item2;
	  var img = new FileInfo($"{tilesDir}\\{card.Id}.png");
      if (!card.Collectible && card.Type != CardType.MINION && card.Type != CardType.SPELL && card.Type != CardType.WEAPON)
        return;

      if (!img.Exists || overwrite) {
        Console.WriteLine($@"Downloading missing image data for set [{set}] - {card.Name} ({card.Id})");

        Interlocked.Increment(ref _downloadCount);

        var data = await new WebClient().DownloadDataTaskAsync($"https://art.hearthstonejson.com/v1/tiles/{card.Id}.png");
        using (var ms = new MemoryStream(data)) {
          var options = new ResizeOptions {
            Size = new Size(130, 34),
            Mode = ResizeMode.Stretch
          };

          var src = Image.Load(ms);
          var offset = src.Width - options.Size.Width;
		  src.Mutate(context => {
			context.Crop(new Rectangle(offset, 0, src.Width - offset, src.Height)).Resize(options);
		  });
		  var path = Path.GetDirectoryName(img.FullName);
          Directory.CreateDirectory(path ?? throw new InvalidOperationException());

          var encoder = new PngEncoder {
            PngColorType = PngColorType.Palette
          };
          src.Save(img.FullName, encoder);
        }
      }

      if (img.Exists) {
        try {
          using (var photo = Image.Load(img.FullName)) {
            var ms = new MemoryStream();
			photo.SaveAsPng(ms);
            rw.AddResource(card.Id, ms);
          }
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
        }
      }
    }
  }
}
