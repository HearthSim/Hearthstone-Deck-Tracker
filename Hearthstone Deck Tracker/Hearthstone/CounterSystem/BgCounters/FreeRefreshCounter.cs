using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class FreeRefreshCounter : NumericCounter
{
    public override bool IsBattlegroundsCounter => true;
    protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.RefreshingAnomaly;
    public override string LocalizedName => LocUtil.Get("Counter_FreeRefresh", useCardLanguage: true);
    public override string[] RelatedCards => new string[] { };

    public FreeRefreshCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
    {
    }

    public override bool ShouldShow() => Game.IsBattlegroundsMatch && Counter > 0;

    public override string[] GetCardsToDisplay()
    {
        return new[]
        {
            HearthDb.CardIds.NonCollectible.Neutral.RefreshingAnomaly,
            HearthDb.CardIds.NonCollectible.Neutral.GhostlyYmirjar,
            HearthDb.CardIds.NonCollectible.Neutral.LeafThroughThePages
        };
    }

    public override string ValueToShow() => Counter.ToString();

    public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
    {
        if (!Game.IsBattlegroundsMatch)
            return;

        if (entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
            return;

	    if(tag == GameTag.BACON_FREE_REFRESH_COUNT)
	    {
		    Counter = value;
		    OnCounterChanged();
	    }
    }
}
