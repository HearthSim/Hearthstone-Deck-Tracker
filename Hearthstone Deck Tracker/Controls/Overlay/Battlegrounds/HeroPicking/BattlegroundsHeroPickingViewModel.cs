using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Tier7;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Requests;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.HeroPicking
{
	public class BattlegroundsHeroPickingViewModel : ViewModel
	{
		public Visibility Visibility
		{
			get => GetProp(Collapsed);
			set
			{
				SetProp(value);
				if(value == Visible)
					StatsVisibility = Config.Instance.ShowBattlegroundsHeroPicking ? Visible : Collapsed;
			}
		}

		public Visibility StatsVisibility
		{
			get => GetProp(Collapsed);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(VisibilityToggleIcon));
				OnPropertyChanged(nameof(VisibilityToggleText));
			}
		}

		public Visual? VisibilityToggleIcon =>
			Core.Overlay.BattlegroundsHeroPicking.TryFindResource(StatsVisibility == Visible ? "eye_slash" : "eye") as Visual;
		public string VisibilityToggleText => StatsVisibility == Visible
			? LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Hide")
			: LocUtil.Get("BattlegroundsHeroPicking_VisibilityToggle_Show");

		public List<BattlegroundsSingleHeroViewModel>? HeroStats
		{
			get => GetProp<List<BattlegroundsSingleHeroViewModel>?>(null);
			set => SetProp(value);
		}

		public OverlayMessageViewModel Message { get; } = new();

		public void Reset()
		{
			HeroStats = null;
			Visibility = Collapsed;
			StatsVisibility = Collapsed;
			Message.Clear();
		}

		public double Scaling { get => GetProp(1.0); set => SetProp(value); }

		public int SelectedHeroDbfId
		{
			get => GetProp(0);
			set
			{
				SetProp(value);
				if(HeroStats == null)
					return;
				var selectedHeroIndex = HeroStats.FindIndex(x => x.HeroDbfId == value);
				if(selectedHeroIndex != -1)
				{
					var direction = (selectedHeroIndex >= HeroStats.Count / 2) ? -1 : 1;
					for(var i = 0; i < HeroStats.Count; i++)
						HeroStats[i].SetHiddenByHeroPower(i == selectedHeroIndex + direction || i == selectedHeroIndex + 2 * direction);
				}
				else
				{
					for(var i = 0; i < HeroStats.Count; i++)
						HeroStats[i].SetHiddenByHeroPower(false);
				}
			}
		}

		public string? StatsText { get => GetProp(""); set { SetProp(value); } }

		public async void SetHeroes(int[] heroIds)
		{
			if(!Config.Instance.EnableBattlegroundsTier7Overlay)
				return;
			// Trials not supported for now.
			if(Core.Game.Spectator)
				return;

			// No Trials for anonymous users
			if(!HSReplayNetOAuth.IsFullyAuthenticated)
				return;

			var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;

			if(!userOwnsTier7 && (Tier7Trial.RemainingTrials ?? 0) == 0)
				return;

			Message.Loading();

			// Avoid using a trial when we can't get the api params anyway.
			var requestParams = GetApiParams(Core.Game, heroIds);
			if(requestParams == null)
			{
				Message.Error();
				return;
			}

			if(!userOwnsTier7)
			{
				var trialActive = await Tier7Trial.Activate();
				if(!trialActive)
				{
					Message.Error();
					return;
				}
			}

			// At this point the user either owns tier7 or has an active trial!

			var stats = await HSReplayNetOAuth.MakeRequest(c => c.GetTier7HeroPickStats(requestParams));
			if(stats == null)
			{
				Message.Error();
				return;
			}

			HeroStats = heroIds.Select(x => {
				var heroStats = stats.FirstOrDefault(heroData => heroData.HeroDbfId == x);
				return new BattlegroundsSingleHeroViewModel(heroStats, SetPlacementVisible);
			}).ToList();
			Message.Mmr(stats[0].MmrFilterValue, stats[0].MinMmr);
			Visibility = Visible;

			if(Config.Instance.ShowBattlegroundsHeroPicking)
				Core.Game.Metrics.Tier7HeroOverlayDisplayed = true;
		}

		public void SetPlacementVisible(bool isVisible)
		{
			if(HeroStats == null)
				return;
			var visibility = isVisible ? Visible : Collapsed;
			foreach(var hero in HeroStats)
				hero.BgsHeroHeaderVM.PlacementDistributionVisibility = visibility;
		}

		private BattlegroundsHeroPickStatsParams? GetApiParams(GameV2 game, int[] heroIds)
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces(game.CurrentGameStats?.GameId);
			if(availableRaces == null)
				return null;

			return new BattlegroundsHeroPickStatsParams
			{
				HeroDbfIds = heroIds,
				BattlegroundsRaces = availableRaces.Cast<int>().ToArray(),
				LanguageCode = Config.Instance.SelectedLanguage
			};
		}
	}
}
