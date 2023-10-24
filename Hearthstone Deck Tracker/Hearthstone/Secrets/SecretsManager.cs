using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

using static HearthDb.Enums.GameType;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class SecretsManager : SecretsEventHandler
	{
		public SecretsManager(IGame game, AvailableSecretsProvider availableSecrets)
		{
			Game = game;
			_availableSecrets = availableSecrets;
		}

		private readonly AvailableSecretsProvider _availableSecrets;
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
				Exclude(entity.CardId!, false);
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
					Exclude(secret.Entity.CardId!, false);
					SavedSecrets.Remove(secret.Entity.CardId!);
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

		public List<Card> GetSecretList()
		{
			var gameMode = Game.CurrentGameType;
			var format = Game.CurrentFormatType;


			var gameModeHasCardLimit = gameMode switch
			{
				GT_CASUAL or GT_RANKED or GT_VS_FRIEND or GT_VS_AI => true,
				_ => false
			};

			var opponentEntities = Game.Opponent.RevealedEntities.Where(e => e.Id < 68 && e.IsSecret && e.HasCardId).ToList();

			//List of all non-excluded cardIds for created secrets
			var createdSecrets = Secrets
				.Where(s => s.Entity.Info.Created)
				.SelectMany(s => s.Excluded)
				.Where(x => !x.Value)
				.Select(x => x.Key)
				.Distinct().ToList();

			bool HasPlayedTwoOf(MultiIdCard card) => opponentEntities.Count(e => card == e.CardId! && !e.Info.Created) >= 2;

			int AdjustCount(MultiIdCard card, int count)
				=> gameModeHasCardLimit && HasPlayedTwoOf(card) && !createdSecrets.Contains(card) ? 0 : count;

			var cards = Secrets
				.SelectMany(secret => secret.Excluded)
				.GroupBy(id => id.Key)
				.Select(group => new QuantifiedMultiIdCard(group.Key, AdjustCount(group.Key.Ids[0], group.Count(x => !x.Value))));

			var availableSecrets = GetAvailableSecrets(gameMode, format);
			cards = cards.Where(x => x.Ids.Any(availableSecrets.Contains));

			return cards.Select(x =>
			{
				var card = x.GetCardForFormat(format);
				if (card != null)
					card.Count = x.Count;
				return card;
			}).WhereNotNull().ToList();
		}
	}
}
