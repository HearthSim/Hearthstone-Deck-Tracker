using System.Linq;

namespace HSReplay.LogValidation
{
	public static class LogValidator
	{
		public static ValidationResult Validate(string[] log)
		{
			if(log.Length == 0)
				return new ValidationResult("Log is empty");
			if(log[0].StartsWith("["))
				return new ValidationResult("Output log not supported");
			if(log[0].Contains("PowerTaskList."))
				return new ValidationResult("PowerTaskList is not supported");
			if(!log.Any(line => line.Contains("CREATE_GAME")))
				return new ValidationResult("'CREATE_GAME' not found");
			return new ValidationResult(true);
		}
	}
}