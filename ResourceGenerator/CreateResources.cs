//  *****************************************************************************
//  File:       CreateResources.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/13/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************


#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using HearthDb.Enums;

#endregion


namespace ResourceGenerator
{
    internal static partial class Program
    {
        /// <summary>
        ///   CreateResources
        /// </summary>
        /// <param name="args"></param>
        /// <param name="options"></param>
        /// <param name="setCollection"></param>
        private static void CreateResources(IReadOnlyList<string> args, ParallelOptions options, IDictionary<CardSet, CardCollection> setCollection)
        {
            // Checkpointing
            if (options.CancellationToken.IsCancellationRequested)
                return;

            try
            {
                // Create resource directory
                Directory.CreateDirectory(CardCollection.TargetPath(Arguments));

                var it = 0;

                Parallel.ForEach(setCollection, options, (kvp, idx) =>
                {
                    // Checkpointing
                    if (options.CancellationToken.IsCancellationRequested || idx.ShouldExitCurrentIteration)
                    {
                        idx.Stop();
                        return;
                    }

                    var resx = Path.Combine(CardCollection.TargetPath(Arguments), $"{kvp.Value.Name}.res");

                    try
                    {
                        // Create resource file.  [0 bytes]
                        kvp.Value.ResourceWriter = new ResourceWriter(resx);

                        Parallel.ForEach(kvp.Value, options, (card, loop) =>
                        {
                            // Checkpointing
                            if (options.CancellationToken.IsCancellationRequested || loop.ShouldExitCurrentIteration)
                            {
                                loop.Stop();
                                return;
                            }
                            DownloadTile(kvp, card, !string.IsNullOrEmpty(args[2]) && args[2] != "0");
                            AddResource(kvp, card);
                        });
                    } catch (ThreadAbortException)
                    {
                        // Ignored
                    } catch (OperationCanceledException)
                    {
                        // Ignored
                    } catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    } finally
                    {
                        Console.WriteLine($@"{++it,2}. Generating ""{resx}""");
                        kvp.Value.ResourceWriter.Close();
                    }
                });
            } catch (ThreadAbortException)
            {
                // Ignored
            } catch (OperationCanceledException)
            {
                // Ignored
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
