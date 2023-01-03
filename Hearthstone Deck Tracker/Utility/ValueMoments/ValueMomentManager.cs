using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Actions;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Enums;
using Hearthstone_Deck_Tracker.Utility.ValueMoments.Utility;
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
						var hdtGeneralSettings = action.ClientProperties.HDTGeneralSettingsEnabled;
						if(!hdtGeneralSettings.Contains(HDTGeneralSettings.OverlayHideCompletely))
							yield return new ValueMoment(VMName.DecklistVisible, ValueMoment.VMKind.Free);
					}
					else if(franchise.Contains(Franchise.Battlegrounds))
					{
						var bgsSettings = action.FranchiseProperties?.BattlegroundsSettingsEnabled;
						var bgsExtraData = action.FranchiseProperties?.BattlegroundsExtraData;
						if (bgsSettings == null || bgsExtraData == null)
							break;

						if(
							bgsSettings.Contains(BattlegroundsSettings.BobsBuddyCombatSimulations) &&
							(
								bgsSettings.Contains(BattlegroundsSettings.BobsBuddyCombatSimulations) ||
								bgsSettings.Contains(BattlegroundsSettings.BobsBuddyResultsDuringShopping)
							)
						)
							yield return new ValueMoment(VMName.BGBobsBuddy, ValueMoment.VMKind.Free);

						if(
							(bgsSettings.Contains(BattlegroundsSettings.SessionRecap) ||
							bgsSettings.Contains(BattlegroundsSettings.SessionRecapBetweenGames))
						)
							yield return new ValueMoment(VMName.BGSessionRecap, ValueMoment.VMKind.Free);

						if((int)bgsExtraData[BattlegroundsExtraData.NumClickBattlegroundsMinionTab] > 0)
							yield return new ValueMoment(VMName.BGMinionBrowser, ValueMoment.VMKind.Free);

						var isTrialActivated = bgsExtraData.TryGetValue(BattlegroundsExtraData.TrialsActivated, out var activatedTrials)
						                       && activatedTrials is string[] trialsArr
						                       && trialsArr.Contains(ValueMomentsConstants.TIER7_OVERLAY_TRIAL);

						if((bool)bgsExtraData[BattlegroundsExtraData.Tier7HeroOverlayDisplayed])
							yield return new ValueMoment(VMName.BGHeroPickOverlay, !isTrialActivated);

						if((bool)bgsExtraData[BattlegroundsExtraData.Tier7QuestOverlayDisplayed])
							yield return new ValueMoment(VMName.BGQuestStatsOverlay, !isTrialActivated);
					}
					else if(franchise.Contains(Franchise.Mercenaries))
					{
						var mercsExtraData = action.FranchiseProperties?.MercenariesExtraData;
						if(mercsExtraData == null)
							break;
						if((int)mercsExtraData[MercenariesExtraData.NumHoverOpponentMercAbility] > 0)
							yield return new ValueMoment(VMName.MercOpponentAbilities, ValueMoment.VMKind.Free);

						if((int)mercsExtraData[MercenariesExtraData.NumHoverMercTaskOverlay] > 0)
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
			if(action.MaximumDailyOccurrences == null)
				return true;

			// Always send match events when a trial was activated
			if(
				action is {
					EventName: VMActions.EndMatchAction.Name,
					FranchiseProperties:
					{
						BattlegroundsExtraData: { }
					}
				}
				&& (int)action.FranchiseProperties.BattlegroundsExtraData[BattlegroundsExtraData.TrialsActivated] > 0
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
