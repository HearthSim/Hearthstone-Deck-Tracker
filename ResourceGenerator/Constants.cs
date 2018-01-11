//  *****************************************************************************
//  File:       Constants.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/08/2018
//  Author:     Latency McLaughlin
//  Copywrite:  Bio-Hazard Industries - 1998-2016
//  *****************************************************************************

using System.Collections.Generic;
using System.Threading;
using HearthDb.Enums;

namespace ResourceGenerator {
  internal static partial class Program {
    private static int _downloadCount;
    private static volatile CancellationTokenSource _tokenSource = new CancellationTokenSource();
    private static readonly Dictionary<CardSet, CardCollection> SetCollection = new Dictionary<CardSet, CardCollection>();
  }
}
