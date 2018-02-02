//  *****************************************************************************
//  File:       AddResource.cs
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
using System.Threading;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using SixLabors.ImageSharp;

#endregion


namespace ResourceGenerator
{
    internal static partial class Program
    {
        /// <summary>
        ///   AddResource
        /// </summary>
        /// <param name="kvp"></param>
        /// <param name="card"></param>
        private static async void AddResource(KeyValuePair<CardSet, CardCollection> kvp, Card card)
        {
            var path = Path.Combine(Arguments[0], Arguments[1], card.Id + ".png");
            var fi = new FileInfo(path);

            if (!fi.Exists)
            {
                if (card.Collectible || card.Type == CardType.MINION || card.Type == CardType.SPELL || card.Type == CardType.WEAPON)
                    Console.WriteLine($@"    Missing card `{card.Id}'.");
                return;
            }

            try
            {
                // Poll until the file that has been created on disk actually gets its stream closed with data before continuing.
                var startTime = DateTime.Now;
                while (fi.Length == 0)
                {
                    // Prevent deadlock
                    if (DateTime.Now.Subtract(startTime) > TimeSpan.FromSeconds(5))
                        return;
                    await Task.Delay(250);
                }

                using (var photo = Image.Load(path))
                {
                    var ms = new MemoryStream();
                    photo.SaveAsPng(ms);
                    kvp.Value.ResourceWriter.AddResource(card.Id, ms);
                }
            } catch (FileNotFoundException)
            {
                // Should have already been handled from above.
                throw;
            } catch (ThreadAbortException)
            {
                // Ignored
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
