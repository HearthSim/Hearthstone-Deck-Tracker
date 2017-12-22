namespace HSReplay.OAuth
{
	public class Scope
	{
		public Scope(string name)
		{
			Name = name;
		}

		public string Name { get; }

		public static Scope ReadWebhooks => new Scope("webhooks:read");
		public static Scope ReadGames => new Scope("games:read");
		public static Scope ReadSocialAccounts => new Scope("account.social:read");

		public static Scope WriteWebhooks => new Scope("webhooks:write");
		public static Scope WriteGames => new Scope("games:write");
	}
}
