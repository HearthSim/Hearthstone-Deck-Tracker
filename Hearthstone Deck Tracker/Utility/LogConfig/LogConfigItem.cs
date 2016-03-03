#region

using System;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.Utility.LogConfig
{
	internal class LogConfigItem
	{
		[RequiredValue(1)]
		public int LogLevel;
		[RequiredValue(true)]
		public bool FilePrinting;
		[RequiredValue]
		public bool ConsolePrinting;
		public bool ScreenPrinting;
		[RequiredValue]
		public bool Verbose;

		public LogConfigItem(string name, bool consolePrinting = false, bool verbose = false)
		{
			Name = name;
			if(verbose)
			{
				SetRequiredValue(nameof(Verbose), true);
				Verbose = true;
			}
			if(consolePrinting)
			{
				SetRequiredValue(nameof(ConsolePrinting), true);
				ConsolePrinting = true;
			}
		}

		public string Name { get; set; }

		private void SetRequiredValue(string fieldName, object value)
			=> ((RequiredValueAttribute)GetType().GetField(fieldName).GetCustomAttributes(typeof(RequiredValueAttribute), false)[0]).Value = value;

		public bool VerifyAndUpdate()
		{
			var modified = false;
			foreach(var field in GetType().GetFields())
			{
				var req = field.GetCustomAttributes(typeof(RequiredValueAttribute), false).FirstOrDefault() as RequiredValueAttribute;
				if(req?.Value == null || Equals(field.GetValue(this), req.Value))
					continue;
				field.SetValue(this, req.Value);
				Log.Info($"[{Name}] set {field.Name}={req.Value}");
				modified = true;
			}
			return modified;
		}

		public override string ToString() => GetType().GetFields().OrderBy(x => x.MetadataToken)
			.Aggregate($"[{Name}]" + Environment.NewLine, (c, prop) => c + ($"{prop.Name}={prop.GetValue(this)}" + Environment.NewLine));
	}
}