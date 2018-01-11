//  *****************************************************************************
//  File:       CreateWatcher.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/04/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HearthDb;

namespace ResourceGenerator {
  internal static partial class Program {
    /// <summary>
    ///   CreateWatcher
    /// </summary>
    /// <param name="tilesDir"></param>
    private static FileSystemWatcher CreateWatcher(string tilesDir) {
	  // Create a directory for the watchdog.
      Directory.CreateDirectory(Path.Combine(Arguments[0], Arguments[1]));

      var mWatcher = new FileSystemWatcher(tilesDir ?? throw new NullReferenceException(MethodBase.GetCurrentMethod().Name), "*.png") {
        IncludeSubdirectories = false,
        EnableRaisingEvents = true
      };

      mWatcher.Created += (sender, args) => {
        while (!_tokenSource.IsCancellationRequested) {
          try {
            using (new StreamReader(args.FullPath)) {
              Console.WriteLine($@"Adding resource `{args.Name}'");

              Card card = null;
			  foreach (var x in SetCollection) {
                card = x.Value.FirstOrDefault(y => y.Name == args.Name);
				if (card != null)
			      break;
			  }

			  if (card == null)
				throw new NullReferenceException(MethodBase.GetCurrentMethod().Name);

              AddResource(card);
              break;
            }
          } catch (IOException) { // Locked by another process or file not found.
            Thread.Sleep(100);
          } catch (Exception ex) {
            Debug.WriteLine(ex.Message);
          }
        }
      };

      return mWatcher;
    }
  }
}
