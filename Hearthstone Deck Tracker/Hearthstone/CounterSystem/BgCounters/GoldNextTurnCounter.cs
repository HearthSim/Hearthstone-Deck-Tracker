using System;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;
using Hearthstone_Deck_Tracker.Utility;
using Entity = Hearthstone_Deck_Tracker.Hearthstone.Entities.Entity;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem.BgCounters;

public class GoldNextTurnCounter : StatsCounter
{
    public override bool IsBattlegroundsCounter => true;
    protected override string? CardIdToShowInUI => HearthDb.CardIds.NonCollectible.Neutral.CarefulInvestment;
    public override string LocalizedName => LocUtil.Get("Counter_GoldNextTurn", useCardLanguage: true);

    public override string[] RelatedCards => new string[]
    {
	    HearthDb.CardIds.NonCollectible.Neutral.SouthseaBusker_ExtraGoldNextTurnDntEnchantment,
	    HearthDb.CardIds.NonCollectible.Neutral.Overconfidence_OverconfidentDntEnchantment,
	    HearthDb.CardIds.NonCollectible.Neutral.GraceFarsail_ExtraGoldIn2TurnsDntEnchantment,
	    HearthDb.CardIds.NonCollectible.Neutral.AccordOTron,
	    HearthDb.CardIds.NonCollectible.Neutral.CarefulInvestment
    };

    private int _overconfidence;
    private int Overconfidence
    {
	    get => _overconfidence;
	    set => _overconfidence = Math.Max(0, value);
    }
    private int _accordotron;
    private int Accordotron
    {
	    get => _accordotron;
	    set => _accordotron = Math.Max(0, value);
    }
    private int _goldSureAmount;
    private int GoldSureAmount
    {
	    get => _goldSureAmount;
	    set => _goldSureAmount = Math.Max(0, value);
    }
    private int ExtraGoldFromOverconfidence => Overconfidence * 3;

    public GoldNextTurnCounter(bool controlledByPlayer, GameV2 game) : base(controlledByPlayer, game)
    {
    }

    public override bool ShouldShow() => Game.IsBattlegroundsMatch && (GoldSureAmount > 0 || Overconfidence > 0 || Accordotron > 0);

    public override string[] GetCardsToDisplay()
    {
        return new[]
        {
            HearthDb.CardIds.NonCollectible.Neutral.SouthseaBusker,
            HearthDb.CardIds.NonCollectible.Neutral.Overconfidence,
            HearthDb.CardIds.NonCollectible.Neutral.GraceFarsailBATTLEGROUNDS,
            HearthDb.CardIds.NonCollectible.Neutral.AccordOTron,
            HearthDb.CardIds.NonCollectible.Neutral.RecordSmuggler,
            HearthDb.CardIds.NonCollectible.Neutral.CarefulInvestment,
        };
    }

    public override string ValueToShow()
    {
        var sureAmount = GoldSureAmount + Accordotron;
        if (ExtraGoldFromOverconfidence > 0)
            return $"{sureAmount} ({sureAmount + ExtraGoldFromOverconfidence})";
        return $"{sureAmount}";
    }

    public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
    {
        if (!Game.IsBattlegroundsMatch)
            return;

        if (entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
            return;

        if(tag == GameTag.BACON_PLAYER_EXTRA_GOLD_NEXT_TURN)
        {
	        GoldSureAmount = value;
	        OnCounterChanged();
        }

        if (entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.Overconfidence_OverconfidentDntEnchantment)
        {
            if (tag == GameTag.ZONE && value == (int)Zone.PLAY && prevValue != (int)Zone.PLAY)
            {
                Overconfidence++;
                OnCounterChanged();
            }
            else if (tag == GameTag.ZONE && value != (int)Zone.PLAY && prevValue == (int)Zone.PLAY)
            {
                Overconfidence--;
                OnCounterChanged();
            }
        }

        var isAccordotronMinion = entity.CardId is HearthDb.CardIds.NonCollectible.Neutral.AccordOTron
	        or HearthDb.CardIds.NonCollectible.Neutral.AccordoTron_AccordOTron;
        var isAccordotronEnchantment = entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.AccordoTron_AccordOTronEnchantment;
        if((isAccordotronMinion && tag == GameTag.ZONE)
           || (isAccordotronEnchantment && tag is GameTag.ZONE or GameTag.TAG_SCRIPT_DATA_NUM_1))
        {
	        UpdateAccordotron();
        }
    }

    private void UpdateAccordotron()
    {
        var controllerId = IsPlayerCounter ? Game.Player.Id : Game.Opponent.Id;
        var total = Game.Entities.Values
	        .Where(e => e.IsInPlay && e.IsControlledBy(controllerId))
	        .Sum(e => e.CardId switch
	        {
		        HearthDb.CardIds.NonCollectible.Neutral.AccordOTron => 1,
		        HearthDb.CardIds.NonCollectible.Neutral.AccordoTron_AccordOTron => 2,
		        HearthDb.CardIds.NonCollectible.Neutral.AccordoTron_AccordOTronEnchantment => e.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1),
		        _ => 0,
	        });
        if(total != Accordotron)
        {
	        Accordotron = total;
	        OnCounterChanged();
        }
    }
}
