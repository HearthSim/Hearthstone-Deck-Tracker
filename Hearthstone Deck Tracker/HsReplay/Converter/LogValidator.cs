using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay.Converter
{
	internal class LogValidator
	{
		public static ValidationResult Validate(List<string> log)
		{
			var result = new ValidationResult { Valid = log.Count > 0 };
			if(!result.Valid)
			{
				Log.Warn($"Log was empty. {result}");
				return result;
			}
			Log.Info($"Log length: {log.Count} lines");
			result.IsPowerTaskList = log[0].Contains("PowerTaskList.");
			var createGameLine = -1;
			for(var i = 0; i < log.Count - 1; i++)
			{
				if(log[i].Contains("CREATE_GAME"))
				{
					createGameLine = i;
					Log.Info($"Found 'CREATE_GAME' at line {i+1}");
					break;
				}
			}
			if(createGameLine == -1)
			{
				var debugPrintPowerLine = log.FirstOrDefault(x => x.Contains("DebugPrintPower"));
				if(debugPrintPowerLine == null)
				{
					result.Valid = false;
					Log.Warn($"Log contains no 'DebugPrintPower'. {result}");
					return result;
				}
				var createLine = new string(debugPrintPowerLine.TakeWhile(x => x != '-').ToArray()) + "- ";
				if(result.IsPowerTaskList)
					createLine += "    ";
				createLine += "CREATE_GAME";
				log.Insert(0, createLine);
				Log.Info("Inserted 'CREATE_GAME' at line 0.");
			}
			Log.Info(result.ToString());
			return result;
		}

		public class ValidationResult
		{
			public bool Valid { get; set; }
			public bool IsPowerTaskList { get; set; }

			public override string ToString() => $"ValidationResult: Valid={Valid}, IsPowerTaskList={IsPowerTaskList}";
		}
	}
}