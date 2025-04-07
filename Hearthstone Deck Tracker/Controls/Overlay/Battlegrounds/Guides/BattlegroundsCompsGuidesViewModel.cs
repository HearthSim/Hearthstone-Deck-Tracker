using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HearthDb.Enums;
using HearthMirror;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides;

public class BattlegroundsCompsGuidesViewModel : ViewModel
{
	public List<BattlegroundsCompGuideViewModel>? Comps
	{
		get => GetProp<List<BattlegroundsCompGuideViewModel>?>(null);
		private set => SetProp(value);
	}

	public Dictionary<int, TieredComps>? CompsByTier
	{
		get => GetProp<Dictionary<int, TieredComps>?>(null);
		private set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Tier7FeatureVisibility));
			OnPropertyChanged(nameof(BaseFeatureVisibility));
		}
	}

	public async Task<List<BattlegroundsCompGuideViewModel>?> GetCompGuides()
	{
		try
		{
			var compsData = await ApiWrapper.GetCompsGuides(Helper.GetCardLanguage());
			var viewModelData = compsData?
				.OrderBy(comp => comp.Name)
				.Select(comp => new BattlegroundsCompGuideViewModel(comp))
				.ToList();
			return viewModelData;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error loading comps data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		return null;
	}

	public async Task<Dictionary<int,TieredComps>?> GetPremiumCompGuides(string? token)
	{
		try
		{
			var availableRaces = BattlegroundsUtils.GetAvailableRaces();
			if(availableRaces == null)
				return null;

			var currentRaces = availableRaces.Cast<int>().ToArray();
			var compsData = token != null
				? await ApiWrapper.GetPremiumCompsGuides(token, Helper.GetCardLanguage(), currentRaces)
				: await HSReplayNetOAuth.MakeRequest(c => c.GetTier7CompsGuides(Helper.GetCardLanguage(), currentRaces));

			if(compsData?.ByTier == null)
				return null;

			var result = new Dictionary<int, TieredComps>();

			foreach(var tierEntry in compsData.ByTier.OrderBy(t => t.Key))
			{
				var tier = tierEntry.Key;
				var comps = tierEntry.Value.OrderBy(comp => comp.Name).ToList();

				var viewModels = comps.Select(comp => new BattlegroundsCompGuideViewModel(comp)).ToList();

				result[tier] = new TieredComps
				{
					TierLetter = GetCompText(tier),
					TierColor = GetTierColor(tier),
					Comps = viewModels
				};
			}

			return CompsByTier = result;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Error loading comps data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
		}

		return null;
	}


	public BattlegroundsCompGuideViewModel? SelectedComp
	{
		get => GetProp<BattlegroundsCompGuideViewModel?>(null);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(IsCompSelected));
		}
	}

	public bool IsCompSelected => SelectedComp != null;

	private readonly SemaphoreSlim _updateCompGuidesSemaphore = new (1, 1);

	public async void OnMatchStart()
	{
		if(Core.Game.Spectator)
			await Task.Delay(1500);

		try
		{
			await _updateCompGuidesSemaphore.WaitAsync();
			await UpdateCompGuides();
		}
		finally
		{
			_updateCompGuidesSemaphore.Release();
		}
	}

	private async Task UpdateCompGuides()
	{
		// Always refresh the comp guides
		await TrySetCompsGuides();
	}

	private async Task TrySetCompsGuides()
	{
		var token = await GetTier7Token();
		var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;

		if(token != null || userOwnsTier7)
		{
			var premiumGuidesTask = GetPremiumCompGuides(token);

			Dictionary<int,TieredComps>? filteredBattlegroundsCompGuides = null;

#if(DEBUG)
	    Log.Debug($"Fetching Premium Battlegrounds Comp Guides...");
#endif
			try
			{
				filteredBattlegroundsCompGuides = await premiumGuidesTask;
			}
			catch(Exception e)
			{
				HandleCompGuidesError(e);
			}

			if(filteredBattlegroundsCompGuides is not null)
			{
				CompsByTier = filteredBattlegroundsCompGuides;
			}
		}
		else
		{
			var guidesTask = GetCompGuides();

			List<BattlegroundsCompGuideViewModel>? battlegroundsCompGuides = null;

#if(DEBUG)
	    Log.Debug($"Fetching Battlegrounds Comp Guides...");
#endif

			try
			{
				battlegroundsCompGuides = await guidesTask;
			}
			catch(Exception e)
			{
				HandleCompGuidesError(e);
			}

			if(battlegroundsCompGuides is not null)
			{
				Comps = battlegroundsCompGuides;
			}
		}
	}

	public void OnMatchEnd()
	{
		CompsByTier = null;
	}

	public class TieredComps
	{
		public string? TierLetter { get; set; }
		public LinearGradientBrush? TierColor { get; set; }
		public List<BattlegroundsCompGuideViewModel>? Comps { get; set; }
	}

	private string GetCompText(int tier)
	{
		return tier switch
		{
			1 => "S",
			2 => "A",
			3 => "B",
			4 => "C",
			5 => "D",
			_ => "?"
		};
	}


	private LinearGradientBrush GetTierColor(int tier)
	{
		LinearGradientBrush CreateLinearGradientBrush(Color color1, Color color2)
		{
			var brush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0.5),
				EndPoint = new Point(1, 0.5)
			};
			brush.GradientStops.Add(new GradientStop(color1, 0.0));
			brush.GradientStops.Add(new GradientStop(color2, 1.0));

			return brush;
		}

		return tier switch
		{
			1 => CreateLinearGradientBrush(Color.FromRgb(64, 138, 191), Color.FromRgb(56, 95, 122)),
			2 => CreateLinearGradientBrush(Color.FromRgb(107, 160, 54), Color.FromRgb(88, 121, 55)),
			3 => CreateLinearGradientBrush(Color.FromRgb(146, 160, 54), Color.FromRgb(104, 121, 55)),
			4 => CreateLinearGradientBrush(Color.FromRgb(160, 124, 54), Color.FromRgb(121, 95, 55)),
			5 => CreateLinearGradientBrush(Color.FromRgb(160, 72, 54), Color.FromRgb(121, 66, 55)),
			_ => CreateLinearGradientBrush(Color.FromRgb(112, 112, 112), Color.FromRgb(64, 64, 64))
		};
	}

	private async Task<string?> GetTier7Token()
	{
		var gameId = Core.Game.MetaData.ServerInfo?.GameHandle;
		var userOwnsTier7 = HSReplayNetOAuth.AccountData?.IsTier7 ?? false;
		var userHasTrials = Tier7Trial.RemainingTrials > 0;

		if(!userOwnsTier7 && gameId == null)
			return null;

		if(!userOwnsTier7 && !(userHasTrials || Tier7Trial.IsTrialForCurrentGameActive(gameId)))
			return null;

		string? token = null;
		if(!userOwnsTier7)
		{
			var acc = Reflection.Client.GetAccountId();
			token = acc != null ? await Tier7Trial.ActivateOrContinue(acc.Hi, acc.Lo, gameId) : null;
			if(!((Core.Game.GameEntity?.GetTag(GameTag.STEP) ?? 0) <= (int)Step.BEGIN_MULLIGAN) && token == null)
				return null;
		}

		return token;
	}

	private void HandleCompGuidesError(Exception error)
	{
		Influx.OnGetBattlegroundsCompositionGuidesError(error.GetType().Name, error.Message);

		// CompStatsErrorVisibility = Visibility.Visible;
		// CompStatsBodyVisibility = Visibility.Hidden;
	}

	public Visibility Tier7FeatureVisibility => CompsByTier != null ? Visibility.Visible : Visibility.Collapsed;
	public Visibility BaseFeatureVisibility => CompsByTier == null ? Visibility.Visible : Visibility.Collapsed;

	public void Reset()
	{
		Comps = null;
		CompsByTier = null;
		SelectedComp = null;
	}
}
