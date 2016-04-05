using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Enums
{
    public enum CardSet
    {
        None,
        Basic,
        Classic,
        Reward,
        Missions,
        System,
        Debug,
        Promotion,
        Credits,
        Hero_Skins,
        Tavern_Brawl,
        Curse_of_Naxxramas,
        Goblins_vs_Gnomes,
        Blackrock_Mountain,
        The_Grand_Tournament,
        League_of_Explorers,
        Whispers_of_the_Old_Gods,
    }

    //public enum ExpansionSet
    //{
    //    Whispers_of_the_Old_Gods,
    //    League_of_Explorers,
    //    The_Grand_Tournament,
    //    Blackrock_Mountain,
    //    Goblins_vs_Gnomes,
    //    Curse_of_Naxxramas,
    //}

    public static class Extensions
    {
        /// <summary>
        /// Will return an enum as a string with all underscores replaced with spaces.
        /// </summary>
        public static string ToSpacedString(this Enum name) => name.ToString().Replace('_', ' ');        
    }
}
