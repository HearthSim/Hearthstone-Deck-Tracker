namespace HSReplay.OAuth.Data
{
	public class AuthData
	{
		public AuthData(string code, string redirectUrl)
		{
			Code = code;
			RedirectUrl = redirectUrl;
		}
		public string Code { get; set; }
		public string RedirectUrl { get; set; }
	}
}
