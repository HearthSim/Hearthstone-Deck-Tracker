using System;
using System.Collections.Generic;
using System.Linq;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;

namespace Hearthstone_Deck_Tracker.Utility.ValueMoments
{
	internal class ValueMomentManager
	{
		internal static IEnumerable<ValueMoment> GetValueMoments(VMAction action)
		{
			switch(action.EventName)
			{
				case VMActions.CopyDeckAction.Name:
					yield return new ValueMoment(ValueMoment.VMName.CopyDeck, ValueMoment.VMKind.Free);
					break;
				case VMActions.ClickAction.Name:
					switch(action.Properties["action_name"])
					{
						case VMActions.ClickAction.ActionName.ScreenshotCopyToClipboard:
						case VMActions.ClickAction.ActionName.ScreenshotSaveToDisk:
						case VMActions.ClickAction.ActionName.ScreenshotUploadToImgur:
							yield return new ValueMoment(ValueMoment.VMName.ShareDeck, ValueMoment.VMKind.Free);
							break;
						case VMActions.ClickAction.ActionName.StatsArena:
						case VMActions.ClickAction.ActionName.StatsConstructed:
							yield return new ValueMoment(ValueMoment.VMName.PersonalStats, ValueMoment.VMKind.Free);
							break;
					}
					break;
				case VMActions.EndMatchAction.Name:
					var franchise = action.Properties["franchise"] as string[];
					if(franchise.Contains(Franchise.HSConstructedValue))
					{
						var hdtGeneralSettings = action.Properties[ValueMomentUtils.HDT_GENERAL_SETTINGS_ENABLED] as string[];
						if(!hdtGeneralSettings.Contains(ValueMomentUtils.OVERLAY_HIDE_COMPLETELY))
							yield return new ValueMoment(ValueMoment.VMName.DecklistVisible, ValueMoment.VMKind.Free);
					}
					else if(franchise.Contains(Franchise.BattlegroundsValue))
					{
						var hdtBgSettings = action.Properties[ValueMomentUtils.BG_GENERAL_SETTINGS_ENABLED] as string[];
						if(
							hdtBgSettings.Contains(ValueMomentUtils.BB_COMBAT_SIMULATIONS) &&
							(
								hdtBgSettings.Contains(ValueMomentUtils.BB_RESULTS_DURING_COMBAT) ||
								hdtBgSettings.Contains(ValueMomentUtils.BB_RESULTS_DURING_SHOPPING)
							)
						)
							yield return new ValueMoment(ValueMoment.VMName.BGBobsBuddy, ValueMoment.VMKind.Free);

						if(
							hdtBgSettings.Contains(ValueMomentUtils.SESSION_RECAP) ||
							hdtBgSettings.Contains(ValueMomentUtils.SESSION_RECAP_BETWEEN_GAMES)
						)
							yield return new ValueMoment(ValueMoment.VMName.BGSessionRecap, ValueMoment.VMKind.Free);

						if((int)action.Properties[ValueMomentUtils.NUM_CLICK_BATTLEGROUNDS_MINION_TAB] > 0)
							yield return new ValueMoment(ValueMoment.VMName.BGMinionBrowser, ValueMoment.VMKind.Free);
					}
					else if(franchise.Contains(Franchise.MercenariesValue))
					{
						if((int)action.Properties[ValueMomentUtils.NUM_HOVER_OPPONENT_MERC_ABILITY] > 0)
							yield return new ValueMoment(ValueMoment.VMName.MercOpponentAbilities, ValueMoment.VMKind.Free);

						if((int)action.Properties[ValueMomentUtils.NUM_HOVER_MERC_TASK_OVERLAY] > 0)
							yield return new ValueMoment(ValueMoment.VMName.MercMyTasks, ValueMoment.VMKind.Free);
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
			if(action.MaxDailyOccurrences == null)
				return true;

			// Check action value moments daily occurrences
			foreach(var vm in valueMoments)
				if(DailyEventsCount.Instance.GetEventDailyCount(vm.Name) <= vm.MaxValueMomentCount)
					return true;

			action.Properties.TryGetValue(ValueMomentUtils.CURRENT_DAILY_OCCURRENCES, out var dailyCount);
			if (dailyCount != null)
				return (int) dailyCount <= action.MaxDailyOccurrences;

			return false;
		}
	}
}
