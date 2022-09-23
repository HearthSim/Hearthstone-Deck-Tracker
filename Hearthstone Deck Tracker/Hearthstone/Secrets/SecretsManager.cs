using System;
using System.Collections.Generic;
using System.Linq;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone.Secrets
{
	public class SecretsManager : SecretsEventHandler
	{
		public SecretsManager(IGame game, ArenaSettingsProvider settings)
		{
			Game = game;
			_settings = settings;
		}

		private readonly ArenaSettingsProvider _settings;
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
			Log.Info("Excluded Secret " + cardId);
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

		public List<Card> GetSecretList()
		{
			var gameMode = Game.CurrentGameType;
			var format = Game.CurrentFormat;

			var opponentEntities = Game.Opponent.RevealedEntities.Where(e => e.Id < 68 && e.IsSecret && e.HasCardId).ToList();
			var gameModeHasCardLimit = new[] { GameType.GT_CASUAL, GameType.GT_RANKED, GameType.GT_VS_FRIEND, GameType.GT_VS_AI }
				.Contains(gameMode);

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
			var foo = cards.ToList();

			if(gameMode == GameType.GT_ARENA)
			{
				cards = cards.Where(c => _settings.CurrentSets.Any(c.HasSet));
				if (_settings.BannedSecrets.Count > 0)
					cards = cards.Where(c => _settings.BannedSecrets.All(s => c != s));
			}
			else
			{
				if(_settings.ExclusiveSecrets.Count > 0)
					cards = cards.Where(c => _settings.ExclusiveSecrets.All(s => c != s));
				if(format == Format.Standard)
					cards = cards.Where(c => c.IsStandard);
				else if(format == Format.Classic)
					cards = cards.Where(c => c.IsClassic);
				else if(format == Format.Wild)
					cards = cards.Where(c => c.IsWild);
			}

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
