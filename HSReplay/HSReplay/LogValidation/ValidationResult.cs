namespace HSReplay.LogValidation
{
	public class ValidationResult
	{
		public ValidationResult(string reason) : this(false, reason)
		{
		}

		public ValidationResult(bool isValid) : this(isValid, string.Empty)
		{
		}

		public ValidationResult(bool isValid, string reason)
		{
			IsValid = isValid;
			Reason = reason;
		}

		public bool IsValid { get; }
		public string Reason { get; }

		public override string ToString() => $"ValidationResult: IsValid={IsValid}, Reason='{Reason}'";
	}
}