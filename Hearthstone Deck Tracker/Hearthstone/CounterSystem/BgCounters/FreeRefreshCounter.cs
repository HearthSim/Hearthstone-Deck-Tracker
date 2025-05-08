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
            HearthDb.CardIds.NonCollectible.Neutral.RefreshingAnomaly
        };
    }

    public override string ValueToShow() => Counter.ToString();

    public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
    {
        if (!Game.IsBattlegroundsMatch)
            return;

        if (entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
            return;

        if (entity.CardId != HearthDb.CardIds.NonCollectible.Neutral.RefreshingAnomaly_RefreshCosts0Enchantment)
            return;

        if (tag == GameTag.TAG_SCRIPT_DATA_NUM_2)
        {
            Counter += (value - prevValue);
            OnCounterChanged();
        }
    }
}
