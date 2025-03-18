#region

using System.Text.RegularExpressions;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader;

public static class LogConstants
{
	// Compiling Regexes takes a long time, but we only do it once and matching will be significantly faster
	// Once HDT supports .NET 7 we can start using GeneratedRegex that compiles the Regex at build time
	private static Regex CompileRegex(string pattern) => new(pattern, RegexOptions.Compiled);

	public static readonly Regex GameModeRegex = CompileRegex(@"prevMode=(?<prev>(\w+)).*currMode=(?<curr>(\w+))");
	public static readonly Regex NextGameModeRegex = CompileRegex(@"prevMode=(?<prev>(\w+)).*nextMode=(?<next>(\w+))");

	public static class PowerTaskList
	{
		public static readonly Regex BlockStartRegex =
			CompileRegex(@".*BLOCK_START.*BlockType=(?<type>(\w+)).*id=(?<id>\d*).*(cardId=(?<Id>(\w*))).*player=(?<player>\d).*EffectCardId=(?<effectCardId>(.*))\sEffectIndex=.*Target=(?<target>(.+)).*SubOption=(?<subOption>[^\s]*)(?:\sTriggerKeyword=(?<triggerKeyword>\w+))?");

		public static readonly Regex CardIdRegex = CompileRegex(@"cardId=(?<cardId>(\w+))");
		public static readonly Regex CreationRegex = CompileRegex(@"FULL_ENTITY - Updating.*id=(?<id>(\d+)).*zone=(?<zone>(\w+)).*CardID=(?<cardId>(\w*))");
		public static readonly Regex CreationTagRegex = CompileRegex(@"tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

		public static readonly Regex EntityRegex =
			CompileRegex(@"(?=id=(?<id>(\d+)))(?=name=(?<name>(\w+)))?(?=zone=(?<zone>(\w+)))?(?=zonePos=(?<zonePos>(\d+)))?(?=cardId=(?<cardId>(\w+)))?(?=player=(?<player>(\d+)))?(?=type=(?<type>(\w+)))?");

		public static readonly Regex GameEntityRegex = CompileRegex(@"GameEntity\ EntityID=(?<id>(\d+))");

		public static readonly Regex PlayerEntityRegex =
			CompileRegex(@"Player\ EntityID=(?<id>(\d+))\ PlayerID=(?<playerId>(\d+))\ GameAccountId=(?<gameAccountId>(.+))");

		public static readonly Regex TagChangeRegex =
			CompileRegex(@"TAG_CHANGE\ Entity=(?<entity>(.+))\ tag=(?<tag>(\w+))\ value=(?<value>(\w+))");

		public static readonly Regex UpdatingEntityRegex =
			CompileRegex(@"(?<type>(SHOW_ENTITY|CHANGE_ENTITY))\ -\ Updating\ Entity=(?<entity>(.+))\ CardID=(?<cardId>(\w*))");

		public static readonly Regex HideEntityRegex = CompileRegex(@"HIDE_ENTITY\ -\ .* id=(?<id>(\d+))");

		public static readonly Regex ShuffleRegex = CompileRegex(@"SHUFFLE_DECK\ PlayerID=(?<id>(\d+))");

		public static readonly Regex SubSpellStartRegex =
			CompileRegex(@"SUB_SPELL_START - SpellPrefabGUID=(?<spellPrefabGuid>(.*)) Source=(?<source>(\d+))");
	}

	public static class Choices
	{
		public static readonly Regex ChoicesHeaderRegex = CompileRegex(@"id=(?<id>(\d+)) Player=(?<player>(.+)) TaskList=(?<taskList>(\d+))? ChoiceType=(?<choiceType>(\w+))");
		public static readonly Regex ChosenHeaderRegex = CompileRegex(@"id=(?<id>(\d+)) Player=(?<player>(.+)) EntitiesCount=.*");
		public static readonly Regex ChoicesSourceRegex = CompileRegex(@"Source=.* id=(?<id>(\d+))");
		public static readonly Regex ChoicesEntityRegex = CompileRegex(@"Entities\[(?<index>(\d+))]=.* id=(?<id>(\d+))");
		public static readonly Regex EndTaskListRegex = CompileRegex(@"m_currentTaskList=(?<taskList>(\d+))");
	}

	public static class GameInfo
	{
		public static readonly Regex PlayerRegex = CompileRegex(@"PlayerID=(?<playerId>(\d+)), PlayerName=(?<playerName>(.+))");
	}
}
