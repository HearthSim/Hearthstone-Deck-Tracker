#region

using System;
using System.Collections.Generic;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.LogReader.Handlers;

#endregion

namespace Hearthstone_Deck_Tracker.LogReader.Interfaces
{
	public interface IHsGameState
	{
		bool CurrentEntityHasCardId { get; set; }
		int CurrentEntityId { get; }
		bool GameEnded { get; set; }
		IGameHandler? GameHandler { get; set; }
		DateTime LastGameStart { get; set; }
		int LastId { get; set; }
		bool OpponentUsedHeroPower { get; set; }
		bool PlayerUsedHeroPower { get; set; }
		bool FoundSpectatorStart { get; set; }
		int JoustReveals { get; set; }
		Dictionary<int, IList<(string, DeckLocation, string?, EntityInfo?)>> KnownCardIds { get; set; }
		int LastCardPlayed { get; set; }
		Stack<string> LastPlagueDrawn { get; set; }
		bool WasInProgress { get; set; }
		int GameTriggerCount { get; set; }
		Zone CurrentEntityZone { get; set; }
		bool DeterminedPlayers { get; }
		int Turn { get; }
		int GetTurnNumber();
		void Reset();
		void SetCurrentEntity(int id);
		void ResetCurrentEntity();
		void BlockStart(string? type, string? cardId, string? target);
		void BlockEnd();
		Block? CurrentBlock { get; }
		Tuple<int, string>? ChameleosReveal { get; set; }
		int DredgeCounter { get; set; }
		Dictionary<string, int> PlayerIdsByPlayerName { get; }
		Dictionary<int, IHsChoice> ChoicesById { get; }
		Dictionary<int, List<IHsChoice>> ChoicesByTaskList { get; }
		bool TriangulatePlayed { get; set; }
		List<int?> StarshipLauchBlockIds { get; }

	}
}
