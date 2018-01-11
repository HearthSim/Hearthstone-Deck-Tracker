//  *****************************************************************************
//  File:       DownloadTile.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/08/2018
//  Author:     Latency McLaughlin
//  Copywrite:  Bio-Hazard Industries - 1998-2016
//  *****************************************************************************

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using HearthDb;
using HearthDb.Enums;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ResourceGenerator {
  internal static partial class Program {
    /// <summary>
    ///   DownloadTile
    /// </summary>
    /// <param name="card"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    private static async Task DownloadTile(Card card, bool overwrite) {
      var img = new FileInfo(Path.Combine(CardCollection.TargetPath(Arguments), $"{card.Id}.png"));
      if (!card.Collectible && card.Type != CardType.MINION && card.Type != CardType.SPELL && card.Type != CardType.WEAPON)
        return;

      if (!img.Exists || overwrite) {
        if (!IsMsBuildInvoked)
          Console.WriteLine($@"Downloading missing image data for set [{card.Set.ToString()}] - {card.Name} ({card.Id})");

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

          var path = Path.Combine(Arguments[0], Arguments[1]);
          if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

          var encoder = new PngEncoder {
            PngColorType = PngColorType.Palette
          };
          src.Save(Path.Combine(path, img.Name), encoder);
		  // AddResource moved to the callback of mWatcher since there is a delay of inf. time for the filestream to be completely written.
        }
      } else {
        AddResource(card);
      }
    }
  }
}
