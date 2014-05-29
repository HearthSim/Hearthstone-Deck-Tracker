using System;

namespace Hearthstone_Deck_Tracker
{
    public class Helper
    {
        public static bool IsNumeric(char c)
        {
            int output;
            return Int32.TryParse(c.ToString(), out output);
        }
    }
}
