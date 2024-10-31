using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using NuGet;

namespace Hearthstone_Deck_Tracker.Hearthstone;

public static class CardUtils
{
	public static IEnumerable<Card?> FilterCardsByFormat(this IEnumerable<Card?> cards, Format? format)
	{
		return cards.Where(card => IsCardFromFormat(card, format));
	}

	public static bool IsCardFromFormat(Card? card, Format? format)
	{
		return format switch
		{
			Format.Classic => card != null && Helper.ClassicOnlySets.Contains(card.Set),
			Format.Wild => card != null && !Helper.ClassicOnlySets.Contains(card.Set),
			Format.Standard => card != null && !Helper.WildOnlySets.Contains(card.Set) && !Helper.ClassicOnlySets.Contains(card.Set),
			Format.Twist => card != null && Helper.TwistSets.Contains(card.Set),
			_ => true
		};
	}

	public static IEnumerable<Card?> FilterCardsByPlayerClass(this IEnumerable<Card?> cards, string? playerClass, bool ignoreNeutral = false)
	{
		return cards.Where(card => IsCardFromPlayerClass(card, playerClass, ignoreNeutral));
	}

	public static bool IsCardFromPlayerClass(Card? card, string? playerClass, bool ignoreNeutral = false)
	{
		return card != null &&
		       (card.PlayerClass == playerClass || card.GetTouristVisitClass() == playerClass ||
		        (!ignoreNeutral && card.CardClass == CardClass.NEUTRAL));
	}

	public static bool MayCardBeRelevant(Card? card, Format? format, string? playerClass,
		bool ignoreNeutral = false)
	{
		return IsCardFromFormat(card, format) && IsCardFromPlayerClass(card, playerClass, ignoreNeutral);
	}

	public static Card? HandleZilliax3000(this Card? card, Player player)
	{
		if (card is null) return null;
		if(card.Id.StartsWith(HearthDb.CardIds.Collectible.Neutral.ZilliaxDeluxe3000))
		{
			var sideboard = player.PlayerSideboardsDict.FirstOrDefault(sb => sb.OwnerCardId == HearthDb.CardIds.Collectible.Neutral.ZilliaxDeluxe3000);
			if(sideboard is { Cards.Count: > 0 })
			{
				var cosmetic = sideboard.Cards.FirstOrDefault(module => !module.ZilliaxCustomizableFunctionalModule);
				var modules = sideboard.Cards.Where(module => module.ZilliaxCustomizableFunctionalModule);

				// Clone Zilliax with new cost, attack, health and mechanics
				card = cosmetic != null ? (Card)cosmetic.Clone() : (Card)card.Clone();
				List<string> mechanics = new();
				foreach(var module in modules)
				{
					if(module.Mechanics != null) mechanics.AddRange(module.Mechanics);
				}
				card.Mechanics = mechanics.ToArray();
				card.Attack = modules.Sum(module => module.Attack);
				card.Health = modules.Sum(module => module.Health);
				card.Cost = modules.Sum(module => module.Cost);
			}
		}

		return card;
	}

	private static string[] _starshipIds =
	{
		HearthDb.CardIds.NonCollectible.Neutral.ArkoniteDefenseCrystal_TheExilesHopeToken,
		HearthDb.CardIds.NonCollectible.Deathknight.ArkoniteDefenseCrystal_TheSpiritsPassageToken,
		HearthDb.CardIds.NonCollectible.Demonhunter.ArkoniteDefenseCrystal_TheLegionsBaneToken,
		HearthDb.CardIds.NonCollectible.Druid.ArkoniteDefenseCrystal_TheCelestialArchiveToken,
		HearthDb.CardIds.NonCollectible.Hunter.ArkoniteDefenseCrystal_TheAstralCompassToken,
		HearthDb.CardIds.NonCollectible.Rogue.ArkoniteDefenseCrystal_TheScavengersWillToken,
		HearthDb.CardIds.NonCollectible.Warlock.ArkoniteDefenseCrystal_TheNethersEyeToken,
	};

	public static bool IsStarship(string? cardId) => _starshipIds.Contains(cardId);

	private static Card? HandleStarship(this Entity? entity, Player player)
	{
		if (entity is null) return null;

		var card = (Card)entity.Card.Clone();

		var starshipPieces = entity.Info.StoredCardIds.Select(Database.GetCardFromId).WhereNotNull().ToArray();

		HashSet<string> mechanics = new();
		foreach(var piece in starshipPieces)
		{
			if(piece.Mechanics != null) mechanics.AddRange(piece.Mechanics);
		}
		card.Mechanics = mechanics.ToArray();
		card.Attack = starshipPieces.Sum(piece => piece.Attack);
		card.Health = starshipPieces.Sum(piece => piece.Health);
		card.Cost = Math.Max(10, starshipPieces.Sum(piece => piece.Cost));

		return card;
	}

	public static Card? GetProcessedCardFromEntity(Entity entity, Player player)
	{
		if(IsStarship(entity.CardId))
		{
			return HandleStarship(entity, player);
		}
		var card = Database.GetCardFromId(entity.CardId);
		return card?.HandleZilliax3000(player);
	}
}
