using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Interfaces;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;

public abstract class BaseCounter : INotifyPropertyChanged
{
	protected GameV2 Game { get; }
	public bool IsPlayerCounter { get; }
	public virtual string LocalizedName => Database.GetCardFromId(CardIdToShowInUI)!.LocalizedName!;
	protected virtual string? CardIdToShowInUI { get; }

	public abstract string[] RelatedCards { get; }

	protected BaseCounter(bool controlledByPlayer, GameV2 game)
	{
		IsPlayerCounter = controlledByPlayer;
		Game = game;
	}

	public Card? CardToShowInUi => Database.GetCardFromId(CardIdToShowInUI);

	public CardAssetViewModel CardAsset => new(CardToShowInUi, Utility.Assets.CardAssetType.Portrait);

	public abstract bool ShouldShow();

	public abstract string ValueToShow();

	public abstract string[] GetCardsToDisplay();

	public string CounterValue => ValueToShow();

	public virtual bool IsDisplayValueLong => false;

	public virtual void HandleTagChange(GameTag tag, IHsGameState gameState, Entity entity, int value, int prevValue){}

	private bool InDeckOrKnown(string cardId)
	{
		var contains = DeckList.Instance.ActiveDeck?.Cards.Any(x => x.Id == cardId);

		if(!contains.HasValue)
			return false;

		return contains.Value || Game.Player.PlayerEntities.FirstOrDefault(x => x.CardId == cardId && x.Info.OriginalZone != null) != null;
	}

	protected bool InPlayerDeckOrKnown(string[] cardIds) => cardIds.Any(InDeckOrKnown);

	protected List<string> GetCardsInDeckOrKnown(string[] cardIds)
	{
		var knownCards = new List<string>();
		foreach (var cardId in cardIds)
		{
			if (InDeckOrKnown(cardId))
			{
				knownCards.Add(cardId);
			}
		}
		return knownCards;
	}

	protected bool OpponentMayHaveRelevantCards(bool ignoreNeutral = false) => FilterCardsByClassAndFormat(RelatedCards, Game.Opponent.OriginalClass, ignoreNeutral).Length > 0;

	protected string[] FilterCardsByClassAndFormat(string[] cardIds, string? playerClass, bool ignoreNeutral = false)
	{
		return cardIds
			.Select(Database.GetCardFromId)
			.FilterCardsByFormat(Game.CurrentFormat)!
			.FilterCardsByPlayerClass(playerClass, ignoreNeutral)
			.Select(card => card!.Id)
			.ToArray();
	}


	public event EventHandler? CounterChanged;

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnCounterChanged()
	{
		CounterChanged?.Invoke(this, EventArgs.Empty);
		OnPropertyChanged(nameof(CounterValue));
	}

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
