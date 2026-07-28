using System;
using System.Threading;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Hearthstone;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	/// <summary>
	/// The Outfinder has no trial of its own: it rides along with the trial of the mode
	/// being played. Ranked (standard/wild) attaches to <see cref="MulliganGuideTrial"/>,
	/// arena (normal/underground) to <see cref="ArenaTrial"/>. Whatever trial the mode
	/// already activated is reused, so a game never spends a second trial on the
	/// Outfinder, and the trial started during the mulligan stays in use for every
	/// Outfinder call of that game. Modes without a trial are premium only.
	/// </summary>
	public static class OutfinderTrial
	{
		private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

		private static bool _hasTrialAccess;

		public static string? Token { get; private set; }

		public static event Action? OnTrialActivated;

		private static bool UserOwnsPremium => HSReplayNetOAuth.AccountData?.IsPremium ?? false;

		public static bool HasAccess => UserOwnsPremium || _hasTrialAccess;

		public static async Task<bool> ActivateOrContinue(GameV2 game)
		{
			if(UserOwnsPremium)
			{
				Clear();
				return true;
			}

			await Semaphore.WaitAsync();
			try
			{
				var hadAccess = _hasTrialAccess;
				Token = null;

				var acc = Reflection.Client.GetAccountId();
				if(acc == null)
					return _hasTrialAccess = false;

				if(game.IsArenaMatch)
					_hasTrialAccess = await ContinueArenaTrial(acc.Hi, acc.Lo);
				else if(IsRankedStandardOrWild(game))
					_hasTrialAccess = await ActivateOrContinueMulliganGuideTrial(acc.Hi, acc.Lo, game.MetaData.ServerInfo?.GameHandle);
				else
					_hasTrialAccess = false;

				if(_hasTrialAccess && !hadAccess)
					OnTrialActivated?.Invoke();

				return _hasTrialAccess;
			}
			finally
			{
				Semaphore.Release();
			}
		}

		private static bool IsRankedStandardOrWild(GameV2 game) =>
			game.CurrentGameType == GameType.GT_RANKED
			&& game.CurrentFormatType is FormatType.FT_STANDARD or FormatType.FT_WILD;

		private static async Task<bool> ContinueArenaTrial(ulong accountHi, ulong accountLo)
		{
			var deckId = Reflection.Client.GetArenaDeck()?.Deck.Id;
			if(deckId == null)
				return false;

			await ArenaTrial.EnsureLoaded(accountHi, accountLo);
			return ArenaTrial.IsDeckResumable(deckId.Value);
		}

		private static async Task<bool> ActivateOrContinueMulliganGuideTrial(ulong accountHi, ulong accountLo, uint? gameId)
		{
			await MulliganGuideTrial.Update(accountHi, accountLo);
			Token = await MulliganGuideTrial.ActivateOrContinue(accountHi, accountLo, gameId, activateAfterMulligan: true);
			return Token != null;
		}

		public static void Clear()
		{
			Token = null;
			_hasTrialAccess = false;
		}
	}
}
