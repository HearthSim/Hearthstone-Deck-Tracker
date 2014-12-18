using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WPFLocalizeExtension.Extensions;

namespace Hearthstone_Deck_Tracker.Utility
{
    public static class Lang
    {
        /// <summary>
        /// Programmatic method for retrieving a localized string via key from a resource file.
        /// </summary>
        /// <param name="key">The key for the string.</param>
        /// <param name="resourceFileName">(Optional) The name of the Resource file. Default: Strings.</param>
        /// <param name="addSpaceAfter">A System.Boolean indicating if a whitespace character should be appended to the end of the localized string.</param>
        /// <returns></returns>
        public static string GetLocalizedString(string key, string resourceFileName = "Strings", bool addSpaceAfter = false)
        {
            var localizedString = String.Empty;

            // Build up the fully-qualified name of the key
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var fullKey = assemblyName + ":" + resourceFileName + ":" + key;
            var locExtension = new LocExtension(fullKey);
            locExtension.ResolveLocalizedValue(out localizedString);

            // Add a space to the end, if requested
            if (addSpaceAfter)
            {
                localizedString += " ";
            }

            return localizedString;
        }
    }
}
