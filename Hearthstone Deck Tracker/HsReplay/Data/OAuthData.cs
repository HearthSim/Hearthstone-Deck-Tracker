using System;
using System.Collections.Generic;
using HSReplay.OAuth.Data;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{
	internal class OAuthData
	{
		public string Code { get; set; }
		public string RedirectUrl { get; set; }
		public TokenData TokenData { get; set; }
		public DateTime TokenDataCreatedAt { get; set; }
		public List<TwitchAccount> TwitchUsers { get; set; }
		public User Account { get; set; }
	}
}
