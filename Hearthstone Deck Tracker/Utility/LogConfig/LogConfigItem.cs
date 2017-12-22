#region

using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfigItem
	{
		private readonly Dictionary<string, object> _requiredValues = new Dictionary<string, object>
		{
			{ nameof(LogLevel), 1 },
			{ nameof(FilePrinting), true }
		};

		public int LogLevel;
		public bool FilePrinting;
		public bool ConsolePrinting;
		public bool ScreenPrinting;
		public bool Verbose;

		public LogConfigItem(string name, bool consolePrinting = false, bool verbose = false)
		{
			Name = name;
			if(verbose)
			{
				_requiredValues[nameof(Verbose)] = true;
				Verbose = true;
			}
			else if(LogConfigConstants.Verbose.Contains(name))
				_requiredValues[nameof(Verbose)] = true;
			if(consolePrinting)
			{
				_requiredValues[nameof(ConsolePrinting)] = true;
				ConsolePrinting = true;
			}
		}

		public string Name { get; set; }

		public bool VerifyAndUpdate()
		{
			var modified = false;
			foreach(var field in GetType().GetFields())
			{
				if(!_requiredValues.TryGetValue(field.Name, out var req) || Equals(field.GetValue(this), req))
					continue;
				field.SetValue(this, req);
				Log.Info($"[{Name}] set {field.Name}={req}");
				modified = true;
			}
			return modified;
		}

		public override string ToString() => GetType().GetFields().OrderBy(x => x.MetadataToken)
			.Aggregate($"[{Name}]" + Environment.NewLine, (c, prop) => c + ($"{prop.Name}={prop.GetValue(this)}" + Environment.NewLine));
	}
}
