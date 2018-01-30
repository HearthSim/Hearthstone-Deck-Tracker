//  *****************************************************************************
//  File:       InitializeCollection.cs
//  Solution:   Hearthstone Deck Tracker
//  Project:    ResourceGenerator
//  Date:       01/13/2018
//  Author:     Latency McLaughlin
//  *****************************************************************************


#region

using System.Collections.Generic;
using HearthDb;
using HearthDb.Enums;

#endregion


namespace ResourceGenerator
{
    internal static partial class Program
    {
        /// <summary>
        ///   InitializeCollection
        /// </summary>
        /// <param name="collection"></param>
        private static int InitializeCollection(out IDictionary<CardSet, CardCollection> collection)
        {
            var count = 0;
            collection = new Dictionary<CardSet, CardCollection>();

            foreach (var kvp in Cards.All)
            {
                if (kvp.Value.Set == CardSet.CHEAT)
                    continue;

                // Critical section
                if (!collection.ContainsKey(kvp.Value.Set))
                    collection.Add(kvp.Value.Set, new CardCollection(kvp.Value.Set + (kvp.Value.Collectible ? "" : "_NC")));
                collection[kvp.Value.Set].Add(kvp.Value);
                count++;
            }

            return count;
        }
    }
}
