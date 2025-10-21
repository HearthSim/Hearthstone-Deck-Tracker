using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Hearthstone.RelatedCardsSystem;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

using static HearthDb.Enums.GameType;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class SecretsManager : SecretsEventHandler
	{
		public SecretsManager(IGame game, AvailableSecretsProvider availableSecrets, RelatedCardsManager relatedCardsManager)
		{
			Game = game;
			_availableSecrets = availableSecrets;
			_relatedCardsManager = relatedCardsManager;
		}

		private readonly AvailableSecretsProvider _availableSecrets;
		private readonly RelatedCardsManager _relatedCardsManager;
		protected override IGame Game { get; }
		protected override bool HasActiveSecrets => Secrets.Count > 0;

		public event Action<List<Card>>? OnSecretsChanged;

		public override void Reset()
		{
			base.Reset();
			Secrets.Clear();
			OnSecretsChanged?.Invoke(new List<Card>());
		}

		public override void Refresh()
		{
			OnSecretsChanged?.Invoke(GetSecretList());
		}

		public bool NewSecret(Entity entity)
		{
			if(entity == null || !entity.IsSecret || !entity.HasTag(GameTag.CLASS))
				return false;
			if(entity.HasCardId)
			{
				var secretMultiIdCard = CardIds.Secrets.GetSecretMultiIdCard(entity.CardId!);
				if(secretMultiIdCard is not null)
				{
					Exclude(secretMultiIdCard, false);
				}
			}
			var secret = new Secret(entity);
			Secrets.Add(secret);
			OnNewSecret(secret);
			Refresh();
			Log.Info(entity.ToString());
			return true;
		}

		public bool RemoveSecret(Entity entity)
		{
			if(entity == null)
				return false;
			var secret = Secrets.FirstOrDefault(s => s.Entity.Id == entity.Id);
			if(secret != null)
			{
				HandleFastCombat(entity);
				Secrets.Remove(secret);
				if(secret.Entity.HasCardId)
				{
					var secretMultiIdCard = CardIds.Secrets.GetSecretMultiIdCard(secret.Entity.CardId!);
					if(secretMultiIdCard is not null)
					{
						Exclude(secretMultiIdCard, false);
						SavedSecrets.Remove(secretMultiIdCard);
					}
				}
				Refresh();
				return true;
			}
			Log.Info("Secret not found: " + entity);
			return false;
		}

		public override void Exclude(List<MultiIdCard> cardIds)
		{
			for(var i = 0; i < cardIds.Count; i++)
				Exclude(cardIds[i], i == cardIds.Count - 1);
		}

		public override bool Exclude(MultiIdCard cardId, bool invokeCallback = true)
		{
			foreach(var secret in Secrets)
				secret.Exclude(cardId);
			//Log.Info("Excluded Secret " + cardId);
			if(invokeCallback)
				Refresh();
			return true;
		}

		public void Toggle(MultiIdCard cardId)
		{
			var excluded = Secrets.Any(s => s.IsExcluded(cardId));
			if(excluded)
			{
				foreach(var secret in Secrets)
					secret.Include(cardId);
			}
			else
				Exclude(cardId, false);
			Refresh();
		}

		public HashSet<string> GetAvailableSecrets(GameType gameMode, FormatType format)
		{
			if(_availableSecrets.ByType != null)
			{
				if(_availableSecrets.ByType.TryGetValue(gameMode.ToString(), out var gameModeSecrets))
					return gameModeSecrets;

				if(_availableSecrets.ByType.TryGetValue(format.ToString(), out var formatSecrets))
					return formatSecrets;
			}

			// Fallback in case query isn't available.
			return format switch
			{
				FormatType.FT_STANDARD => CardIds.Secrets.All.Where(x => x.IsStandard).Select(x => x.Ids[0]).ToHashSet(),
				_ => CardIds.Secrets.All.Where(x => x.IsWild).Select(x => x.Ids[0]).ToHashSet(),
			};
		}

		public Dictionary<string, HashSet<string>>? GetCreatedBySecretsByCreator(GameType gameMode, FormatType format)
		{
			if(gameMode is not (GT_ARENA or GT_UNDERGROUND_ARENA)) return null;
			if(_availableSecrets.CreatedByTypeByCreator != null)
			{
				if(_availableSecrets.CreatedByTypeByCreator.TryGetValue(gameMode.ToString(), out var gameModeSecrets))
					return gameModeSecrets;
			}

			return null;
		}

		public List<Card> GetSecretList()
		{
			var gameMode = Game.CurrentGameType;
			var format = Game.CurrentFormatType;

			var deckSecrets = GetSecretsFromDeck(gameMode, format);
			var createdSecretsList = GetSecretsCreatedBy(gameMode, format);

			return createdSecretsList.ConcatCardList(deckSecrets).ToList();
		}

		private IEnumerable<Card> GetSecretsFromDeck(GameType gameMode, FormatType format)
		{
			var gameModeHasCardLimit = gameMode switch
			{
				GT_CASUAL or GT_RANKED or GT_VS_FRIEND or GT_VS_AI => true,
				_ => false
			};

			var opponentEntities = Game.Opponent.RevealedEntities
				.Where(e => e.Id < 68 && e is { IsSecret: true, HasCardId: true })
				.ToList();

			var createdSecrets = Secrets
				.Where(s => s.Entity.Info.Created)
				.SelectMany(s => s.Excluded)
				.Where(x => !x.Value)
				.Select(x => x.Key)
				.Distinct()
				.ToList();

			var availableSecrets = GetAvailableSecrets(gameMode, format);

			var secretsFromDeck = Secrets
				.Where(s => !s.Entity.Info.Created);

			var filteredSecretsFromDeck = GetFilteredSecretsByDrawer(secretsFromDeck, availableSecrets);

			var cards = filteredSecretsFromDeck
				.GroupBy(m => m)
				.Select(group =>
				{
					var multiIdCard = CardIds.Secrets.GetSecretMultiIdCard(group.Key.Ids[0]);
					if (multiIdCard is null)
						return new QuantifiedMultiIdCard(group.Key, 0);
					return new QuantifiedMultiIdCard(group.Key, AdjustCount(multiIdCard, group.Count()));
				});

			return QuantifiedCardsToCards(cards, format);

			int AdjustCount(MultiIdCard card, int count) =>
				gameModeHasCardLimit && HasPlayedTwoOf(card) && !createdSecrets.Contains(card) ? 0 : count;

			bool HasPlayedTwoOf(MultiIdCard card) =>
				opponentEntities.Count(e => card == e.CardId! && !e.Info.Created) >= 2;
		}

		private List<Card> GetSecretsCreatedBy(GameType gameMode, FormatType format)
		{
			var createdBySecrets = Secrets.Where(s => s.Entity.Info.Created);
			var availableSecrets = GetAvailableSecrets(gameMode, format);

			if (gameMode is GT_ARENA or GT_UNDERGROUND_ARENA)
			{
				return GetArenaCreatedSecrets(createdBySecrets, availableSecrets, gameMode, format);
			}

			var secretsCreated = new List<MultiIdCard>();

			foreach (var secret in createdBySecrets)
			{
				var creator = TryGetCreator(secret);
				var drawer = TryGetDrawer(secret);

				if (creator != null && _relatedCardsManager.CardGeneratorCards.TryGetValue(creator.CardId ?? "", out var generator))
				{
					var creatableSecrets = GetCreatableSecretsFromGenerator(generator, gameMode, format);

					if (drawer != null && _relatedCardsManager.SpellSchoolTutorCards.TryGetValue(drawer.CardId ?? "", out var tutor))
					{
						secretsCreated.AddRange(GetFilteredSecretsByDrawerFromSingleSecret(secret, tutor, creatableSecrets));
					}
					else
					{
						secretsCreated.AddRange(GetFilteredSecrets(secret, creatableSecrets));
					}
				}
				else
				{
					if (drawer != null && _relatedCardsManager.SpellSchoolTutorCards.TryGetValue(drawer.CardId ?? "", out var tutor))
					{
						secretsCreated.AddRange(GetFilteredSecretsByDrawerFromSingleSecret(secret, tutor, availableSecrets));
					}
					else
					{
						secretsCreated.AddRange(GetFilteredSecrets(secret, availableSecrets));
					}
				}
			}

			return QuantifyAndConvertSecrets(secretsCreated, format);
		}

		private List<Card> GetArenaCreatedSecrets(IEnumerable<Secret> createdBySecrets, HashSet<string> availableSecrets, GameType gameMode, FormatType format)
		{
			var secretsCreated = new List<MultiIdCard>();
			var availableCreatedBy = GetCreatedBySecretsByCreator(gameMode, format);

			if (availableCreatedBy != null)
			{
				var creators = createdBySecrets.Select(s =>
					(s, Game.Opponent.RevealedEntities.FirstOrDefault(e => e.Id == s.Entity.Info.GetCreatorId()))
				);

				foreach (var (secret, creator) in creators)
				{
					var drawer =
						Game.Opponent.RevealedEntities.FirstOrDefault(e => e.Id == secret.Entity.Info.GetDrawerId());

					if(drawer is not null && _relatedCardsManager.SpellSchoolTutorCards.TryGetValue(drawer.CardId ?? "",
						   out var spellSchoolTutor))
					{
						var secrets = creator != null && availableCreatedBy.TryGetValue(creator.CardId ?? "", out var creatableSecrets)
							? GetFilteredSecretsByDrawerFromSingleSecret(secret, spellSchoolTutor, creatableSecrets)
							: GetFilteredSecretsByDrawerFromSingleSecret(secret, spellSchoolTutor, availableSecrets);

						secretsCreated.AddRange(secrets);
					}
					else
					{
						var secrets = creator != null && availableCreatedBy.TryGetValue(creator.CardId ?? "", out var creatableSecrets)
							? secret.Excluded.Where(x => x.Key.Ids.Any(creatableSecrets.Contains))
							: secret.Excluded.Where(x => x.Key.Ids.Any(availableSecrets.Contains));

						secretsCreated.AddRange(secrets.Where(x => !x.Value).Select(x => x.Key));
					}

				}

				var quantified = secretsCreated
					.GroupBy(m => m)
					.Select(g =>
					{
						var card = CardIds.Secrets.GetSecretMultiIdCard(g.Key.Ids[0]);
						return card is not null ? new QuantifiedMultiIdCard(g.Key, g.Count())
							: new QuantifiedMultiIdCard(g.Key, 0);
					});

				return QuantifiedCardsToCards(quantified, format);
			}

			var filteredSecrets = GetFilteredSecretsByDrawer(createdBySecrets, availableSecrets);

			var quantifiedSecrets = filteredSecrets
				.GroupBy(x => x)
				.Select(g =>
				{
					var card = CardIds.Secrets.GetSecretMultiIdCard(g.Key.Ids[0]);
					return card is not null ? new QuantifiedMultiIdCard(g.Key, g.Count()) : new QuantifiedMultiIdCard(g.Key, 0);
				});

			return QuantifiedCardsToCards(quantifiedSecrets, format);
		}

		private List<MultiIdCard> GetFilteredSecretsByDrawer(IEnumerable<Secret> allSecrets, HashSet<string> availableSecrets)
		{
			var secretAndDrawSource = allSecrets.Select(s =>
				(s, Game.Opponent.RevealedEntities.FirstOrDefault(e => e.Id == s.Entity.Info.GetDrawerId()))
			);

			var filteredSecrets = new List<MultiIdCard>();
			foreach (var (secret, drawSource) in secretAndDrawSource)
			{
				if(drawSource is not null && _relatedCardsManager.SpellSchoolTutorCards.TryGetValue(drawSource.CardId ?? "", out var spellSchoolTutor))
				{
					filteredSecrets.AddRange(GetFilteredSecretsByDrawerFromSingleSecret(secret, spellSchoolTutor, availableSecrets));
				}
				else
				{
					var secrets = secret.Excluded.Where(x => x.Key.Ids.Any(availableSecrets.Contains));
					filteredSecrets.AddRange(secrets.Where(x => !x.Value).Select(x => x.Key));
				}
			}

			return filteredSecrets;
		}

		private static IEnumerable<MultiIdCard> GetFilteredSecretsByDrawerFromSingleSecret(Secret secret, ISpellSchoolTutor spellSchoolTutor,
			HashSet<string> availableSecrets)
		{
			var spellSchools = spellSchoolTutor.TutoredSpellSchools;
			return secret.Excluded
				.Where(x => x.Key.Ids.Any(availableSecrets.Contains) && !x.Value)
				.Select(x => new Card(x.Key.Ids[0]))
				.Where(c => spellSchools.Contains(c.GetTag(GameTag.SPELL_SCHOOL)))
				.Select(c => CardIds.Secrets.GetSecretMultiIdCard(c.Id))
				.WhereNotNull();
		}

		private Entity? TryGetCreator(Secret secret) =>
			Game.Opponent.RevealedEntities.FirstOrDefault(e => e.Id == secret.Entity.Info.GetCreatorId());

		private Entity? TryGetDrawer(Secret secret) =>
			Game.Opponent.RevealedEntities.FirstOrDefault(e => e.Id == secret.Entity.Info.GetDrawerId());

		private HashSet<string> GetCreatableSecretsFromGenerator(ICardGenerator generator, GameType gameMode, FormatType format)
		{
			var allSecrets = CardIds.Secrets.Mage.All
				.Concat(CardIds.Secrets.Hunter.All)
				.Concat(CardIds.Secrets.Paladin.All)
				.Concat(CardIds.Secrets.Rogue.All);

			return allSecrets
				.Where(s => s.Ids.All(id => generator.IsInGeneratorPool(new Card(id), gameMode, format)))
				.SelectMany(m => m.Ids)
				.ToHashSet();
		}

		private IEnumerable<MultiIdCard> GetFilteredSecrets(Secret secret, HashSet<string> allowedSecretIds) =>
			secret.Excluded
				.Where(x => x.Key.Ids.Any(allowedSecretIds.Contains) && !x.Value)
				.Select(x => x.Key);

		private List<Card> QuantifyAndConvertSecrets(IEnumerable<MultiIdCard> secrets, FormatType format)
		{
			var quantified = secrets
				.GroupBy(m => m)
				.Select(g =>
				{
					var card = CardIds.Secrets.GetSecretMultiIdCard(g.Key.Ids[0]);
					return card is not null ? new QuantifiedMultiIdCard(g.Key, g.Count())
						: new QuantifiedMultiIdCard(g.Key, 0);
				});

			return QuantifiedCardsToCards(quantified, format);
		}

		private static List<Card> QuantifiedCardsToCards(IEnumerable<QuantifiedMultiIdCard> quantified, FormatType format) =>
        	quantified
        		.Select(x =>
        		{
        			var card = x.GetCardForFormat(format);
        			if (card != null)
        				card.Count = x.Count;
        			return card;
        		})
        		.WhereNotNull()
        		.ToList();
	}
}
