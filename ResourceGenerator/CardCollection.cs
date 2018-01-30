//  *****************************************************************************
//  File:       CardCollection.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/13/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************


#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Resources;
using HearthDb;

#endregion


namespace ResourceGenerator
{
    internal sealed class CardCollection : List<Card>, IDisposable
    {
        /// <summary>
        ///   TargetPath
        /// </summary>
        public static readonly Func<string[], string> TargetPath = args => Path.Combine(args[0], @"Generated");


        /// <inheritdoc />
        /// <summary>
        ///   Constructor
        /// </summary>
        /// <param name="key"></param>
        public CardCollection(string key) : base(new List<Card>())
        {
            Name = key;
        }


        /// <summary>
        ///   Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///   DownloadCount
        /// </summary>
        public int DownloadCount { get; set; }

        /// <summary>
        ///   ResourceWriter
        /// </summary>
        public IResourceWriter ResourceWriter { get; set; }


        /// <inheritdoc />
        /// <summary>
        ///   Dispose
        /// </summary>
        public void Dispose()
        {
            ResourceWriter?.Dispose();
            ResourceWriter = null;
            GC.Collect();
        }
    }
}
