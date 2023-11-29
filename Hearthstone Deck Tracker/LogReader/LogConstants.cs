#region

using System.Text.RegularExpressions;
using HearthWatcher.LogReader;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader
{
	public static class LogConstants
	{
		public static readonly Regex GoldProgressRegex = new Regex(@"(?<wins>(\d))/3 wins towards 10 gold");
		public static readonly Regex UnloadCardRegex = new Regex(@"unloading\ name=(?<id>(\w+_\w+))\ family=CardPrefab\ persistent=False");
		public static readonly Regex UnloadBrawlAsset = new Regex(@"unloading name=Tavern_Brawl\ ");

		public static readonly Regex CardMovementRegex =
			new Regex(@"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");

		public static readonly Regex NewChoiceRegex = new Regex(@"Client chooses: .* \((?<id>(.+))\)");
		public static readonly Regex GameModeRegex = new Regex(@"prevMode=(?<prev>(\w+)).*currMode=(?<curr>(\w+))");
		public static readonly Regex NextGameModeRegex = new Regex(@"prevMode=(?<prev>(\w+)).*nextMode=(?<next>(\w+))");
		public static readonly Regex ConnectionRegex = new Regex(@"ConnectAPI\.GotoGameServer -- address=(?<address>(.*)), game=(?<game>(.*)), client=(?<client>(.*)), spectateKey=(?<spectateKey>(.*)),? reconn");

		public static class PowerTaskList
		{
			public static readonly Regex BlockStartRegex =
				new Regex(@".*BLOCK_START.*BlockType=(?<type>(\w+)).*id=(?<id>\d*).*(cardId=(?<Id>(\w*))).*player=(?<player>\d).*EffectCardId=(?<effectCardId>(.*))\sEffectIndex=.*Target=(?<target>(.+)).*SubOption=(?<subOption>(.+))");

			public static readonly Regex CardIdRegex = new Regex(@"cardId=(?<cardId>(\w+))");
			public static readonly Regex CreationRegex = new Regex(@"FULL_ENTITY - Updating.*id=(?<id>(\d+)).*zone=(?<zone>(\w+)).*CardID=(?<cardId>(\w*))");
			public static readonly Regex CreationTagRegex = new Regex(@"tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

			public static readonly Regex EntityRegex =
				new Regex(
					@"(?=id=(?<id>(\d+)))(?=name=(?<name>(\w+)))?(?=zone=(?<zone>(\w+)))?(?=zonePos=(?<zonePos>(\d+)))?(?=cardId=(?<cardId>(\w+)))?(?=player=(?<player>(\d+)))?(?=type=(?<type>(\w+)))?");

			public static readonly Regex GameEntityRegex = new Regex(@"GameEntity\ EntityID=(?<id>(\d+))");

			public static readonly Regex PlayerEntityRegex =
				new Regex(@"Player\ EntityID=(?<id>(\d+))\ PlayerID=(?<playerId>(\d+))\ GameAccountId=(?<gameAccountId>(.+))");

			public static readonly Regex TagChangeRegex =
				new Regex(@"TAG_CHANGE\ Entity=(?<entity>(.+))\ tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

			public static readonly Regex UpdatingEntityRegex =
				new Regex(@"(?<type>(SHOW_ENTITY|CHANGE_ENTITY))\ -\ Updating\ Entity=(?<entity>(.+))\ CardID=(?<cardId>(\w*))");

			public static readonly Regex HideEntityRegex = new Regex(@"HIDE_ENTITY\ -\ .* id=(?<id>(\d+))");

			public static readonly Regex ShuffleRegex = new Regex(@"SHUFFLE_DECK\ PlayerID=(?<id>(\d+))");
		}

		public static class Choices
		{
			public static readonly Regex SendChoicesHeaderRegex = new Regex(@"id=(?<id>(\d+)) ChoiceType=(?<choiceType>(\w+))");
			public static readonly Regex SendChoicesBodyRegex = new Regex(@"m_chosenEntities\[(?<index>(\d+))]=.* id=(?<id>(\d+))");
		}
	}
}
