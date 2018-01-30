//  *****************************************************************************
//  File:       DownloadTile.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/13/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************


#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using HearthDb;
using HearthDb.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

#endregion


namespace ResourceGenerator
{
    internal static partial class Program
    {
        /// <summary>
        ///   DownloadTile
        /// </summary>
        /// <param name="kvp"></param>
        /// <param name="card"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        private static void DownloadTile(KeyValuePair<CardSet, CardCollection> kvp, Card card, bool overwrite)
        {
            var img = new FileInfo(Path.Combine(Arguments[0], Arguments[1], $"{card.Id}.png"));
            if (!card.Collectible && card.Type != CardType.MINION && card.Type != CardType.SPELL && card.Type != CardType.WEAPON)
                return;

            if (img.Exists && img.Length != 0 && !overwrite)
                return;

            Console.WriteLine($@"Downloading missing image data for set [{card.Set.ToString() + ']',13} - {card.Name,-30} ({card.Id})");

            var data = new WebClient().DownloadData($"https://art.hearthstonejson.com/v1/tiles/{card.Id}.png");
            using (var ms = new MemoryStream(data))
            {
                var options = new ResizeOptions
                {
                    Size = new Size(130, 34),
                    Mode = ResizeMode.Stretch
                };

                var src = Image.Load(ms);
                var offset = src.Width - options.Size.Width;
                src.Mutate(context =>
                {
                    context.Crop(new Rectangle(offset, 0, src.Width - offset, src.Height)).Resize(options);
                });

                var path = Path.Combine(Arguments[0], Arguments[1]);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                var encoder = new PngEncoder
                {
                    PngColorType = PngColorType.Palette
                };
                src.Save(Path.Combine(path, img.Name), encoder);

                kvp.Value.DownloadCount++;
            }
        }
    }
}
