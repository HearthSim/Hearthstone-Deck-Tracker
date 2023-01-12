using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions.Action;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
using VMName = Hearthstone_Deck_Tracker.Utility.ValueMoments.ValueMoment.VMName;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMomentManager
	{
		internal static IEnumerable<ValueMoment> GetValueMoments(VMAction action)
		{
			switch(action)
			{
				case CopyDeckAction _:
					yield return new ValueMoment(VMName.CopyDeck, ValueMoment.VMKind.Free);
					break;
				case ClickAction clickAction:
					switch(clickAction.ActionName)
					{
						case ClickAction.Action.ScreenshotCopyToClipboard:
						case ClickAction.Action.ScreenshotSaveToDisk:
						case ClickAction.Action.ScreenshotUploadToImgur:
							yield return new ValueMoment(VMName.ShareDeck, ValueMoment.VMKind.Free);
							break;
						case ClickAction.Action.StatsArena:
						case ClickAction.Action.StatsConstructed:
							yield return new ValueMoment(VMName.PersonalStats, ValueMoment.VMKind.Free);
							break;
					}
					break;
				case VMEndMatchAction _:
					switch (action.Franchise)
					{
						case Franchise.HSConstructed:
						{
							if(!action.GeneralSettings.OverlayHideCompletely)
								yield return new ValueMoment(VMName.DecklistVisible, ValueMoment.VMKind.Free);
							break;
						}
						case Franchise.Battlegrounds:
						{
							foreach (var vmBattlegrounds in GetEndMatchBattlegroundsValueMoments(action))
								yield return vmBattlegrounds;
							break;
						}
						case Franchise.Mercenaries:
						{
							foreach (var vmMercenaries in GetEndMatchMercenariesValueMoments(action))
								yield return vmMercenaries;
							break;
						}
					}
					break;
			};
		}

		private static IEnumerable<ValueMoment> GetEndMatchBattlegroundsValueMoments(VMAction action)
		{
			var battlegroundsAction = (VMBattlegroundsAction)action;

			if (
				battlegroundsAction.BattlegroundsSettings.BobsBuddyCombatSimulations &&
				(
					battlegroundsAction.BattlegroundsSettings.BobsBuddyResultsDuringCombat ||
					battlegroundsAction.BattlegroundsSettings.BobsBuddyResultsDuringShopping
				)
			)
				yield return new ValueMoment(VMName.BGBobsBuddy, ValueMoment.VMKind.Free);

			if (
				battlegroundsAction.BattlegroundsSettings.SessionRecap ||
				battlegroundsAction.BattlegroundsSettings.SessionRecapBetweenGames
			)
				yield return new ValueMoment(VMName.BGSessionRecap, ValueMoment.VMKind.Free);

			if (battlegroundsAction.NumClickBattlegroundsMinionTab > 0)
				yield return new ValueMoment(VMName.BGMinionBrowser, ValueMoment.VMKind.Free);

			var isTrialActivated = battlegroundsAction.TrialsActivated != null &&
			                       battlegroundsAction.TrialsActivated.Contains(ValueMomentsConstants.Tier7OverlayTrial);
			if (battlegroundsAction.Tier7HeroOverlayDisplayed)
				yield return new ValueMoment(VMName.BGHeroPickOverlay, !isTrialActivated);

			if (battlegroundsAction.Tier7QuestOverlayDisplayed)
				yield return new ValueMoment(VMName.BGQuestStatsOverlay, !isTrialActivated);
		}

		private static IEnumerable<ValueMoment> GetEndMatchMercenariesValueMoments(VMAction action)
		{
			var mercenariesAction = (VMMercenariesAction)action;

			if (mercenariesAction.NumHoverOpponentMercAbility > 0)
				yield return new ValueMoment(VMName.MercOpponentAbilities, ValueMoment.VMKind.Free);

			if (mercenariesAction.NumHoverMercTaskOverlay > 0)
				yield return new ValueMoment(VMName.MercFriendlyTasks, ValueMoment.VMKind.Free);
		}
		
		internal static bool ShouldSendEventToMixPanel(VMAction action, List<ValueMoment> valueMoments)
		{
			// Check action daily occurrences
			if(action.MaximumDailyOccurrences == null)
				return true;

			// Always send match events when a trial was activated
			if(
				action is {
					Name: ValueMomentsConstants.EndMatchName,
					Franchise: Franchise.Battlegrounds,
				}
				&& ((VMBattlegroundsAction)action).TrialsActivated?.Length > 0
			)
			{
				return true;
			}

			// Check action value moments daily occurrences
			foreach(var vm in valueMoments)
			{
				if(DailyEventsCount.Instance.GetEventDailyCount(vm.Name) <= vm.MaxValueMomentCount)
					return true;
			}

			var dailyCount = action.CurrentDailyOccurrences;
			if (dailyCount != null)
				return (int) dailyCount <= action.MaximumDailyOccurrences;

			return false;
		}
	}
}
