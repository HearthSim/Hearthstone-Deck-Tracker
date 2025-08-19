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
	    HearthDb.CardIds.NonCollectible.Neutral.CarefulInvestment
    };

    private int _overconfidence;
    private int Overconfidence
    {
	    get => _overconfidence;
	    set => _overconfidence = Math.Max(0, value);
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

    public override bool ShouldShow() => Game.IsBattlegroundsMatch && (GoldSureAmount > 0 || Overconfidence > 0);

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
        if (ExtraGoldFromOverconfidence > 0)
            return $"{GoldSureAmount} ({GoldSureAmount + ExtraGoldFromOverconfidence})";
        return $"{GoldSureAmount}";
    }

    public override void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue)
    {
        if (!Game.IsBattlegroundsMatch)
            return;

        if (entity.IsControlledBy(Game.Player.Id) != IsPlayerCounter)
            return;

        if (RelatedCards.Contains(entity.CardId))
        {
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
            else if (entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.SouthseaBusker_ExtraGoldNextTurnDntEnchantment)
            {
                if (tag == GameTag.TAG_SCRIPT_DATA_NUM_1)
                {
                    if (entity.GetTag(GameTag.ZONE) == (int)Zone.PLAY)
                    {
                        GoldSureAmount += value - prevValue;
                        OnCounterChanged();
                    }
                }
                else if (tag == GameTag.ZONE)
                {
                    if (value == (int)Zone.PLAY && prevValue != (int)Zone.PLAY)
                    {
                        GoldSureAmount += entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
                        OnCounterChanged();
                    }
                    else if (value != (int)Zone.PLAY && prevValue == (int)Zone.PLAY)
                    {
                        GoldSureAmount -= entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
                        OnCounterChanged();
                    }
                }
            }
            else if (entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.GraceFarsail_ExtraGoldIn2TurnsDntEnchantment)
            {
                if (tag == GameTag.TAG_SCRIPT_DATA_NUM_2 && value == 1)
                {
                    GoldSureAmount += entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
                    OnCounterChanged();
                }
                else if (tag == GameTag.TAG_SCRIPT_DATA_NUM_2 && prevValue == 1)
                {
                    GoldSureAmount -= entity.GetTag(GameTag.TAG_SCRIPT_DATA_NUM_1);
                    OnCounterChanged();
                }
            }
            else if(entity.CardId == HearthDb.CardIds.NonCollectible.Neutral.CarefulInvestment)
            {
	            if(tag == GameTag.ZONE && value == (int)Zone.PLAY && prevValue != (int)Zone.PLAY)
	            {
		            GoldSureAmount += 2;
		            OnCounterChanged();
	            }
	            else if (value == (int)Zone.REMOVEDFROMGAME && prevValue == (int)Zone.GRAVEYARD)
	            {
		            GoldSureAmount -= 2;
		            OnCounterChanged();
	            }
            }
            return;
        }

        if(tag != GameTag.ZONE || entity.CardId == null)
	        return;

        var goldValue = GetGoldFromCard(entity.CardId, entity.HasTag(GameTag.PREMIUM));

        if(goldValue <= 0)
	        return;

        if (value == (int)Zone.PLAY && prevValue != (int)Zone.PLAY)
        {
	        GoldSureAmount += goldValue;
	        OnCounterChanged();
        }
        else if (value != (int)Zone.PLAY && prevValue == (int)Zone.PLAY)
        {
	        GoldSureAmount -= goldValue;
	        OnCounterChanged();
        }
    }

    private static int GetGoldFromCard(string cardId, bool golden)
    {
	    return cardId switch
	    {
		    HearthDb.CardIds.NonCollectible.Neutral.AccordOTron => golden ? 2 : 1,
		    HearthDb.CardIds.NonCollectible.Neutral.RecordSmuggler => golden ? 2 : 4,
		    _ => 0
	    };
    }
}
