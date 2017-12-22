namespace HSReplay
{
	public class ClientConfig
	{
		private const string DefaultClaimAccountUrl = "https://hsreplay.net/api/v1/claim_account/";
		private const string DefaultDeckInventoryUrl = "https://hsreplay.net/api/v1/analytics/query/list_deck_inventory/";
		private const string DefaultDeckWinrateUrl = "https://hsreplay.net/api/v1/analytics/query/single_deck_base_winrate_by_opponent_class/";
		private const string DefaultTokensUrl = "https://hsreplay.net/api/v1/tokens/";
		private const string DefaultUploadRequestUrl = "https://upload.hsreplay.net/api/v1/replay/upload/request/";
		private const string DefaultUploadPackUrl = "https://hsreplay.net/api/v1/packs/";
		private const string DefaultArchetypesUrl = "https://hsreplay.net/api/v1/archetypes/";
		private const string DefaultArchetypeMatchupsUrl = "https://hsreplay.net/analytics/query/head_to_head_archetype_matchups/";
		public string ClaimAccountUrl { get; set; } = DefaultClaimAccountUrl;
		public string DeckInventoryUrl { get; set; } = DefaultDeckInventoryUrl;
		public string DeckWinrateUrl { get; set; } = DefaultDeckWinrateUrl;
		public string TokensUrl { get; set; } = DefaultTokensUrl;
		public string UploadRequestUrl { get; set; } = DefaultUploadRequestUrl;
		public string UploadPackUrl { get; set; } = DefaultUploadPackUrl;
		public string ArchetypesUrl { get; set; } = DefaultArchetypesUrl;
		public string ArchetypeMatchupsUrl { get; set; } = DefaultArchetypeMatchupsUrl;
	}
}
