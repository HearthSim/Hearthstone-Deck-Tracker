//  *****************************************************************************
//  File:       CardCollection.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/08/2018
//  Author:     Latency McLaughlin
//  Copywrite:  Bio-Hazard Industries - 1998-2016
//  *****************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using HearthDb;

namespace ResourceGenerator {
  internal class CardCollection : List<Card>, IDisposable {
	/// <inheritdoc cref="List{T}" />
	/// <summary>
	///  Constructor
	/// </summary>
	/// <param name="key"></param>
	/// <param name="capacity"></param>
    public CardCollection(string key)  : base(new List<Card>()) {
      ResourceWriter = new ResourceWriter(Path.Combine(TargetPath(Program.Arguments), key + ".res"));
    }


    public static readonly Func<string[], string> TargetPath = args => Path.Combine(args[0], @"Generated", args[1]);

    public IResourceWriter ResourceWriter { get; private set; }
    private readonly int _capacity;
    // ReSharper disable once InconsistentNaming
    internal int _completed;

    public int Completed {
      get => _completed;
      set {
        _completed = value;
        if (_completed == _capacity) {
		  // Flush the stream
          ResourceWriter.Close();
		  ResourceWriter.Dispose();
        }
      }
	}


	/// <inheritdoc />
	/// <summary>
	///  Dispose
	/// </summary>
    public void Dispose() {
      ResourceWriter?.Dispose();
      ResourceWriter = null;
	  GC.Collect();
    }
  }
}
