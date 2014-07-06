using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker
{
    class Logger
    {
        /// <summary>
        /// Writes line to trace
        /// </summary>
        public static void WriteLine(string line, string category = "")
        {
            Trace.WriteLine(string.Format("[{0}]{1}: {2}", DateTime.Now.ToLongTimeString(), category, line));
        }
    }
}
