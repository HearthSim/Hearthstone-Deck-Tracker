using System;
using System.Threading;
using System.Threading.Tasks;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	public static class Tier7Trial
	{
		private static readonly JsonSerializer<TrialData> Serializer;
		private static PlayerTrialStatus? _status;
		public static string? Token { get; private set; }
		public static int? RemainingTrials => _status?.TrialsRemaining;

		public static string? TimeRemaining => _status?.HoursUntilReset == null ? null
			: string.Format(LocUtil.Get("BattlegroundsPreLobby_Trial_ResetTimeRemaining_DaysHours"), _status.HoursUntilReset / 24, _status.HoursUntilReset % 24);

		public static event Action? OnTrialActivated;

		static Tier7Trial()
		{
			Serializer = new JsonSerializer<TrialData>("tier7_trial", true);
		}

		private class TrialData
		{
			public string? Token { get; set; }
			public uint? GameID { get; set; }
		}

		public static bool IsTrialForCurrentGameActive(uint? gameId) => Serializer.Load().GameID == gameId;

		public static bool IsAvailable =>
			(HSReplayNetOAuth.AccountData?.IsTier7 ?? false)
			|| RemainingTrials > 0 && CanActivateNewTrial
			|| Core.Game.MetaData.ServerInfo?.GameHandle is uint gameId && IsTrialForCurrentGameActive(gameId);

		private static bool CanActivateNewTrial => (Core.Game.GameEntity?.GetTag(GameTag.STEP) ?? 0) <= (int)Step.BEGIN_MULLIGAN;

		// call only once all request preconditions are met (so a trial is never wasted)
		public static async Task<Tier7Access?> GetAccess()
		{
			if(HSReplayNetOAuth.AccountData?.IsTier7 ?? false)
				return Tier7Access.Subscription;

			var gameId = Core.Game.MetaData.ServerInfo?.GameHandle;
			if(gameId == null)
				return null;

			var acc = Reflection.Client.GetAccountId();
			if(acc == null)
				return null;

			var token = await ActivateOrContinue(acc.Hi, acc.Lo, gameId);
			return token != null ? new Tier7Access(token) : null;
		}

		private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

		public static async Task<string?> ActivateOrContinue(ulong accountHi, ulong accountLo, uint? gameId, bool activateAfterMulligan = false)
		{
			await semaphore.WaitAsync();
			try
			{
				if(gameId == null)
					return null;

				var currentData = Serializer.Load();

				if(currentData.GameID == gameId)
				{
					return currentData.Token;
				}

				// Prevent using trials after mulligan phase
				if(!CanActivateNewTrial && !activateAfterMulligan)
					return null;

				if(_status == null || _status.TrialsRemaining == 0)
					return null;
				Token = await ApiWrapper.ActivatePlayerTrial("tier7-overlay", accountHi, accountLo);
				if(Token != null)
				{
					Core.Game.Metrics.Tier7TrialActivated = true;

					var data = new TrialData { Token = Token, GameID = gameId };
					Serializer.Save(data);
					Core.Game.Metrics.Tier7TrialsRemaining = Math.Max(0, (RemainingTrials ?? 0) - 1);

					OnTrialActivated?.Invoke();
				}

				return Token;
			}
			finally
			{
				semaphore.Release();
			}
		}

		public static async Task Update(ulong accountHi, ulong accountLo)
		{
			if(_status?.HoursUntilReset < 2)
				_status = null;
			_status ??= await ApiWrapper.GetPlayerTrialStatus("tier7-overlay", accountHi, accountLo);
		}

		public static void Clear()
		{
			_status = null;
			Token = null;
		}
	}

	public sealed class Tier7Access
	{
		public static readonly Tier7Access Subscription = new(null);

		// null for subscribers, whose requests go through OAuth instead
		public string? TrialToken { get; }

		internal Tier7Access(string? trialToken) => TrialToken = trialToken;
	}
}
