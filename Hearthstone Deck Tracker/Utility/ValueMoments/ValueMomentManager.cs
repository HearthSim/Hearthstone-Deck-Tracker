using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using VMName = Hearthstone_Deck_Tracker.Utility.ValueMoments.ValueMoment.VMName;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMomentManager
	{
		internal static IEnumerable<ValueMoment> GetValueMoments(VMAction action)
		{
			switch(action.EventName)
			{
				case VMActions.CopyDeckAction.Name:
					yield return new ValueMoment(VMName.CopyDeck, ValueMoment.VMKind.Free);
					break;
				case VMActions.ClickAction.Name:
					switch(action.Properties["action_name"])
					{
						case VMActions.ClickAction.ActionName.ScreenshotCopyToClipboard:
						case VMActions.ClickAction.ActionName.ScreenshotSaveToDisk:
						case VMActions.ClickAction.ActionName.ScreenshotUploadToImgur:
							yield return new ValueMoment(VMName.ShareDeck, ValueMoment.VMKind.Free);
							break;
						case VMActions.ClickAction.ActionName.StatsArena:
						case VMActions.ClickAction.ActionName.StatsConstructed:
							yield return new ValueMoment(VMName.PersonalStats, ValueMoment.VMKind.Free);
							break;
					}
					break;
				case VMActions.EndMatchAction.Name:
				case VMActions.EndSpectateMatchAction.Name:
					var franchise = action.Properties["franchise"] as Franchise[];
					if(franchise.Contains(Franchise.HSConstructed))
					{
						var hdtGeneralSettings = action.EnrichedProperties.HDTGeneralSettingsEnabled;
						if(!hdtGeneralSettings.Contains(HDTGeneralSettings.OverlayHideCompletely))
							yield return new ValueMoment(VMName.DecklistVisible, ValueMoment.VMKind.Free);
					}
					else if(franchise.Contains(Franchise.Battlegrounds))
					{
						var hdtBgSettings = action.Properties[ValueMomentUtils.BG_GENERAL_SETTINGS_ENABLED] as string[];
						if(
							hdtBgSettings.Contains(ValueMomentUtils.BB_COMBAT_SIMULATIONS) &&
							(
								hdtBgSettings.Contains(ValueMomentUtils.BB_RESULTS_DURING_COMBAT) ||
								hdtBgSettings.Contains(ValueMomentUtils.BB_RESULTS_DURING_SHOPPING)
							)
						)
							yield return new ValueMoment(VMName.BGBobsBuddy, ValueMoment.VMKind.Free);

						if(
							hdtBgSettings.Contains(ValueMomentUtils.SESSION_RECAP) ||
							hdtBgSettings.Contains(ValueMomentUtils.SESSION_RECAP_BETWEEN_GAMES)
						)
							yield return new ValueMoment(VMName.BGSessionRecap, ValueMoment.VMKind.Free);

						if((int)action.Properties[ValueMomentUtils.NUM_CLICK_BATTLEGROUNDS_MINION_TAB] > 0)
							yield return new ValueMoment(VMName.BGMinionBrowser, ValueMoment.VMKind.Free);

						var isTrialActivated = action.Properties.TryGetValue(ValueMomentUtils.TRIALS_ACTIVATED, out var activatedTrials)
							&& activatedTrials is string[] trialsArr
							&& trialsArr.Contains(ValueMomentUtils.TIER7_OVERLAY_TRIAL);

						if((bool)action.Properties[ValueMomentUtils.TIER7_HERO_OVERLAY_DISPLAYED])
							yield return new ValueMoment(VMName.BGHeroPickOverlay, !isTrialActivated);

						if((bool)action.Properties[ValueMomentUtils.TIER7_QUEST_OVERLAY_DISPLAYED])
							yield return new ValueMoment(VMName.BGQuestStatsOverlay, !isTrialActivated);
					}
					else if(franchise.Contains(Franchise.Mercenaries))
					{
						if((int)action.Properties[ValueMomentUtils.NUM_HOVER_OPPONENT_MERC_ABILITY] > 0)
							yield return new ValueMoment(VMName.MercOpponentAbilities, ValueMoment.VMKind.Free);

						if((int)action.Properties[ValueMomentUtils.NUM_HOVER_MERC_TASK_OVERLAY] > 0)
							yield return new ValueMoment(VMName.MercFriendlyTasks, ValueMoment.VMKind.Free);
					}
					break;
			};
		}
		
		internal static Dictionary<string, object> GetValueMomentsProperties(List<ValueMoment> valueMoments)
		{
			var freeValueMoments = new List<string>();
			var paidValueMoments = new List<string>();
			var hasFreeValueMoment = false;
			var hasPaidValueMoment = false;

			foreach (var vm in valueMoments)
			{
				if (vm.IsFree)
				{
					freeValueMoments.Add(vm.Name);
					hasFreeValueMoment = true;
				}
				else if(vm.IsPaid)
				{
					paidValueMoments.Add(vm.Name);
					hasPaidValueMoment = true;
				}

			}

			return new Dictionary<string, object>
			{
				{ "free_value_moments", freeValueMoments },
				{ "paid_value_moments", paidValueMoments },
				{ "has_free_value_moment", hasFreeValueMoment },
				{ "has_paid_value_moment", hasPaidValueMoment },
			};
		}

		internal static bool ShouldSendEventToMixPanel(VMAction action, List<ValueMoment> valueMoments)
		{
			// Check action daily occurrences
			if(action.EnrichedProperties.MaximumDailyOccurrences == null)
				return true;

			// Always send match events when a trial was activated
			if(action.EventName == VMActions.EndMatchAction.Name
				&& action.Properties.TryGetValue(ValueMomentUtils.TRIALS_ACTIVATED, out var activated)
				&& activated is string[] strArr
				&& strArr.Length > 0)
			{
				return true;
			}

			// Check action value moments daily occurrences
			foreach(var vm in valueMoments)
			{
				if(DailyEventsCount.Instance.GetEventDailyCount(vm.Name) <= vm.MaxValueMomentCount)
					return true;
			}

			int? dailyCount = action.EnrichedProperties.CurrentDailyOccurrences;
			if (dailyCount != null)
				return (int) dailyCount <= action.EnrichedProperties.MaximumDailyOccurrences;

			return false;
		}
	}
}
