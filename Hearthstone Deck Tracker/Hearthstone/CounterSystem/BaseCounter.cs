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
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using NuGet;

namespace Hearthstone_Deck_Tracker.Hearthstone.CounterSystem;

public abstract class BaseCounter : INotifyPropertyChanged
{
	protected GameV2 Game { get; }
	public bool IsPlayerCounter { get; }
	public virtual string LocalizedName => Database.GetCardFromId(CardIdToShowInUI)!.LocalizedName!;
	protected virtual string? CardIdToShowInUI { get; }

	public abstract string[] RelatedCards { get; }

	public virtual bool IsBattlegroundsCounter => false;

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

	public virtual void HandleChoicePicked(IHsCompletedChoice choice){}

	private bool InDeckOrKnown(string cardId)
	{
		var deckContains = DeckList.Instance.ActiveDeck?.Cards.Any(x => x.Id == cardId) ?? false;

		var playerEntitiesContains = Game.Player.PlayerEntities.Any(x =>
			x.CardId == cardId &&
			x.Info.OriginalZone != null &&
			// non-picked discover option entities now go to the graveyard
			x is { IsInSetAside: false, IsInGraveyard: false }
		);

		var discoverEntitiesContains = Game.Player.OfferedEntities.Any(x => x.CardId == cardId);

		return deckContains || playerEntitiesContains || discoverEntitiesContains;
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

	private readonly string[] _alwaysAvailableCards = {
		HearthDb.CardIds.NonCollectible.Neutral.BoonofBeetles_BeetleToken1,
		HearthDb.CardIds.NonCollectible.Neutral.BloodGem1,
	};

	private HashSet<int>? _availableCardIds;
	private HashSet<int> GetAvailableCardIds()
	{
		if (_availableCardIds == null)
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			var currentRaces = new HashSet<Race>(availableRaces.Concat(new[] { Race.ALL, Race.INVALID }));
			var availableCards = BattlegroundsDbSingleton.Instance.GetCardsByRaces(currentRaces, Core.Game.IsBattlegroundsDuosMatch)
				.Concat(BattlegroundsDbSingleton.Instance.GetSpells(Core.Game.IsBattlegroundsDuosMatch));

			_availableCardIds = new HashSet<int>(availableCards.Select(card => card.DbfId));
		}
		return _availableCardIds;
	}

	public IEnumerable<Card> CardsToDisplay
	{
		get
		{
			foreach(var cardId in GetCardsToDisplay())
			{
				var card = Database.GetCardFromId(cardId);
				if(card == null)
					continue;

				if(
					IsBattlegroundsCounter &&
					!GetAvailableCardIds().Contains(card.DbfId) &&
					!_alwaysAvailableCards.Contains(cardId)
				)
					continue;

				card.BaconCard = IsBattlegroundsCounter;
				yield return card;
			}
		}
	}


	public event EventHandler? CounterChanged;

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void OnCounterChanged()
	{
		CounterChanged?.Invoke(this, EventArgs.Empty);
		OnPropertyChanged(nameof(CounterValue));
		OnPropertyChanged(nameof(CardsToDisplay));
	}

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}
