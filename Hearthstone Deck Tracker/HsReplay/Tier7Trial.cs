using System;
using System.Threading;
using System.Threading.Tasks;
using HearthDb.Enums;
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
				if(!((Core.Game.GameEntity?.GetTag(GameTag.STEP) ?? 0) <= (int)Step.BEGIN_MULLIGAN) && !activateAfterMulligan)
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
}
