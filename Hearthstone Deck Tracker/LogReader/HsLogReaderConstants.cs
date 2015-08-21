using System.Text.RegularExpressions;

namespace Hearthstone_Deck_Tracker.LogReader
{
    public static class HsLogReaderConstants
    {
        public static readonly Regex ActionStartRegex = new Regex(@".*ACTION_START.*id=(?<id>\d*).*(cardId=(?<Id>(\w*))).*BlockType=POWER.*Target=(?<target>(.+))");
        public static readonly Regex CardIdRegex = new Regex(@"cardId=(?<cardId>(\w+))");
        public static readonly Regex CreationRegex = new Regex(@"FULL_ENTITY\ -\ Creating\ ID=(?<id>(\d+))\ CardID=(?<cardId>(\w*))");
        public static readonly Regex CreationTagRegex = new Regex(@"tag=(?<tag>(\w+))\ value=(?<value>(\w+))");
        public static readonly Regex EntityNameRegex = new Regex(@"TAG_CHANGE\ Entity=(?<name>(\w+))\ tag=PLAYER_ID\ value=(?<value>(\d))");
        public static readonly Regex EntityRegex = new Regex(@"(?=id=(?<id>(\d+)))(?=name=(?<name>(\w+)))?(?=zone=(?<zone>(\w+)))?(?=zonePos=(?<zonePos>(\d+)))?(?=cardId=(?<cardId>(\w+)))?(?=player=(?<player>(\d+)))?(?=type=(?<type>(\w+)))?");
        public static readonly Regex GameEntityRegex = new Regex(@"GameEntity\ EntityID=(?<id>(\d+))");
        public static readonly Regex PlayerEntityRegex = new Regex(@"Player\ EntityID=(?<id>(\d+))\ PlayerID=(?<playerId>(\d+))\ GameAccountId=(?<gameAccountId>(.+))");
        public static readonly Regex TagChangeRegex = new Regex(@"TAG_CHANGE\ Entity=(?<entity>(.+))\ tag=(?<tag>(\w+))\ value=(?<value>(\w+))");
        public static readonly Regex UpdatingEntityRegex = new Regex(@"SHOW_ENTITY\ -\ Updating\ Entity=(?<entity>(.+))\ CardID=(?<cardId>(\w*))");
        public static readonly Regex CardAlreadyInCacheRegex = new Regex(@"somehow\ the\ card\ def\ for\ (?<id>(\w+_\w+))\ was\ already\ in\ the\ cache...");
        public static readonly Regex GoldProgressRegex = new Regex(@"(?<wins>(\d))/3 wins towards 10 gold");
        public static readonly Regex GoldRewardRegex = new Regex(@"GoldRewardData: Amount=(?<amount>(\d+))");
        public static readonly Regex DustRewardRegex = new Regex(@"ArcaneDustRewardData: Amount=(?<amount>(\d+))");
        public static readonly Regex UnloadCardRegex = new Regex(@"unloading\ name=(?<id>(\w+_\w+))\ family=CardPrefab\ persistent=False");
        public static readonly Regex UnloadBrawlAsset = new Regex(@"unloading name=Tavern_Brawl\ ");
        public static readonly Regex CardMovementRegex = new Regex(@"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");
        public static readonly Regex ExistingHeroRegex = new Regex(@"Draft Deck ID: .*, Hero Card = (?<id>(HERO_\w+))");
        public static readonly Regex ExistingCardRegex = new Regex(@"Draft deck contains card (?<id>(\w+))");
        public static readonly Regex NewChoiceRegex = new Regex(@"Client chooses: .* \((?<id>(.+))\)");
    }
}