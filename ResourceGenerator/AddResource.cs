//  *****************************************************************************
//  File:       AddResource.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/09/2018
//  Author:     Latency McLaughlin
//  Copywrite:  Bio-Hazard Industries - 1998-2016
//  *****************************************************************************

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HearthDb;
using SixLabors.ImageSharp;

namespace ResourceGenerator {
  internal static partial class Program {
    /// <summary>
    ///   AddResource
    /// </summary>
    /// <param name="cd"></param>
    private static void AddResource(Card cd) {
      // Image file must already exist.
      try {
        using (var photo = Image.Load(Path.Combine(Arguments[0], Arguments[1], cd.Name))) {
          var ms = new MemoryStream();
          photo.SaveAsPng(ms);
          SetCollection[cd.Set].ResourceWriter.AddResource(cd.Name, ms);
		  Interlocked.Increment(ref SetCollection[cd.Set]._completed);
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }
  }
}
