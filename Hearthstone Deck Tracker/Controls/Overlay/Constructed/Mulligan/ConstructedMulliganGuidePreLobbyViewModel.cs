using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using HearthDb.Deckstrings;
using HearthDb.Enums;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HSReplay.Requests;
using HSReplay.Responses;
using Newtonsoft.Json;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Constructed.Mulligan;

public enum SingleDeckState
{
	INVALID,
	LOADING, // indicates that a task is currently fetching some
	NO_DATA,
	READY,
}

public class SingleDeckStatus
{
	public Visibility Visibility { get; }
	public SingleDeckState State { get; }
	public bool HasRunes { get; }
	public bool IsFocused { get; }
	public string Padding => HasRunes ? "18,16,29,0" : "18,16,15,0";

	public SingleDeckStatus()
	{
		Visibility = Visibility.Hidden;
		State = SingleDeckState.INVALID;
		HasRunes = false;
	}

	public SingleDeckStatus(SingleDeckState state, bool hasRunes, bool isFocused)
	{
		Visibility = Visibility.Visible;
		State = state;
		HasRunes = hasRunes;
		IsFocused = isFocused;
	}

	public Visibility IconVisibility
	{
		get => State switch
		{
			SingleDeckState.READY or SingleDeckState.NO_DATA or SingleDeckState.LOADING => Visibility.Visible,
			_ => Visibility.Collapsed,
		};
	}

	public string IconSource
	{
		get => State switch
		{
			SingleDeckState.NO_DATA => "/HearthstoneDeckTracker;component/Resources/mulligan-guide-no-data.png",
			_ => "/HearthstoneDeckTracker;component/Resources/mulligan-guide-data.png",
		};
	}

	public string BorderBrush => State switch
	{
		SingleDeckState.NO_DATA => "#CCE3D000",
		SingleDeckState.READY => "#CC00AA00",
		_ => "#CC555555"
	};

	public string Background => State switch
	{
		SingleDeckState.NO_DATA => "#CC1A1100",
		SingleDeckState.READY => "#CC002200",
		_ => "#CC000000",
	};

	public string Label => State switch
	{
		SingleDeckState.LOADING => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_Loading"),
		SingleDeckState.NO_DATA => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_NoData"),
		SingleDeckState.READY => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_Ready"),
		_ => State.ToString(),
	};

	public Visibility LabelVisibility => IsFocused ? Visibility.Visible : Visibility.Collapsed;
}

public class ConstructedMulliganGuidePreLobbyViewModel : ViewModel
{
	private Dictionary<BnetGameType, Dictionary<string, SingleDeckState>> _deckStatusByDeckstring = new();

	public ConstructedMulliganGuidePreLobbyViewModel()
	{
		HSReplayNetOAuth.AccountDataUpdated += () => Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
		HSReplayNetOAuth.LoggedOut += () => Core.Overlay.UpdateMulliganGuidePreLobbyVisibility();
	}

	#region Pagination

	public List<CollectionDeckBoxVisual?>? DecksOnPage
	{
		get { return GetProp<List<CollectionDeckBoxVisual?>?>(null); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(PageStatus));
			OnPropertyChanged(nameof(PageStatusRows));
			OnPropertyChanged(nameof(ValidDecksOnPage));
		}
	}

	public List<CollectionDeckBoxVisual?>? ValidDecksOnPage => DecksOnPage?.Select(x =>
	{
		if(x == null)
			return null;
		if(x.IsShowingInvalidCardCount || x.InvalidSideboardCardCount > 0 || x.MissingSideboardCardCount > 0)
			return null;
		return x;
	}).ToList();

	#endregion

	#region Deckstrings

	struct DeckData
	{
		public string Deckstring;
		public bool HasRunes;
	}

	private Dictionary<FormatType, Dictionary<long, DeckData>> _decksByFormatAndDeckId = new();

	private static bool IsEligibleForFormat(HearthMirror.Objects.Deck deck, FormatType formatType)
	{
		var deckFormat = (FormatType)deck.FormatType;
		return formatType switch
		{
			FormatType.FT_STANDARD => deckFormat == FormatType.FT_STANDARD,
			FormatType.FT_WILD => deckFormat == FormatType.FT_STANDARD || deckFormat == FormatType.FT_WILD,
			FormatType.FT_CLASSIC => deckFormat == FormatType.FT_CLASSIC,
			FormatType.FT_TWIST => deckFormat == FormatType.FT_TWIST,
			_ => false,
		};
	}

	private static Dictionary<long, DeckData> GetDeckDataByDeckId(FormatType formatType)
	{
		Dictionary<long, DeckData> cache = new();

		var decks = Reflection.Client.GetDecks();
		foreach(var deck in decks)
		{
			if(!IsEligibleForFormat(deck, formatType))
				continue;
			var hearthDbDeck = HearthDbConverter.ToHearthDbDeck(deck, formatType);
			if(hearthDbDeck is null)
				continue;
			cache[deck.Id] = new DeckData
			{
				Deckstring = DeckSerializer.Serialize(hearthDbDeck, false),
				HasRunes = (
					hearthDbDeck.GetHero()?.Class == CardClass.DEATHKNIGHT ||
					hearthDbDeck.GetCards().Keys.Any(x => x.Entity.GetTag(GameTag.DEATH_KNIGHT_TOURIST) > 0)
				),
			};
		}

		return cache;
	}

	private Dictionary<long, DeckData> CacheDecks(FormatType formatType)
	{
		var cache = GetDeckDataByDeckId(formatType);
		_decksByFormatAndDeckId[formatType] = cache;
		return cache;
	}

	#endregion

	#region VisualsFormatType

	public VisualsFormatType VisualsFormatType
	{
		get { return GetProp(VisualsFormatType.VFT_UNKNOWN); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(GameType));
			OnPropertyChanged(nameof(FormatType));
			OnPropertyChanged(nameof(PageStatus));
			OnPropertyChanged(nameof(PageStatusRows));
			EnsureLoaded().Forget();
		}
	}

	private BnetGameType GameType => VisualsFormatType switch
	{
		VisualsFormatType.VFT_STANDARD => BnetGameType.BGT_RANKED_STANDARD,
		VisualsFormatType.VFT_WILD => BnetGameType.BGT_RANKED_WILD,
		VisualsFormatType.VFT_TWIST => BnetGameType.BGT_RANKED_TWIST,
		VisualsFormatType.VFT_CASUAL => BnetGameType.BGT_CASUAL_WILD,
		_ => BnetGameType.BGT_UNKNOWN,
	};

	public FormatType FormatType => VisualsFormatType switch
	{
		VisualsFormatType.VFT_STANDARD => FormatType.FT_STANDARD,
		VisualsFormatType.VFT_WILD => FormatType.FT_WILD,
		VisualsFormatType.VFT_TWIST => FormatType.FT_TWIST,
		VisualsFormatType.VFT_CASUAL => FormatType.FT_WILD,
		_ => FormatType.FT_UNKNOWN,
	};

	#endregion

	#region Visibiliy
	public bool IsModalOpen
	{
		get { return GetProp(false); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public bool IsInQueue
	{
		get { return GetProp(false); }

		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => IsModalOpen || IsInQueue ? Visibility.Hidden : Visibility.Visible;
	#endregion

	private static async Task<Dictionary<string, MulliganGuideStatusData.Status>> LoadMulliganGuideStatus(
		BnetGameType gameType,
		int? starLevel,
		IEnumerable<string> deckstrings
	)
	{
		var dss = deckstrings.Distinct().ToArray();
		if(!dss.Any())
			return new Dictionary<string, MulliganGuideStatusData.Status>();

		var parameters = new MulliganGuideStatusParams()
		{
			Decks = dss,
			GameType = (int)gameType,
			PlayerStarLevel = starLevel,
		};

		var result = await ApiWrapper.GetMulliganGuideStatus(parameters);

		// Select from dss so that if the API messes up we still have *some* status (NO_DATA)
		return dss.ToDictionary(x => x,
			x => result?.Decks?.TryGetValue(x, out var deck) ?? false ? deck.Status : MulliganGuideStatusData.Status.NO_DATA
		);
	}

	public async Task EnsureLoaded()
	{
		try
		{
			await Update(true);
			await Update();
		}
		catch(Exception ex)
		{
			Log.Error($"Could not load Mulligan status: {ex}");
		}
	}

	private async Task Update(bool onlyVisiblePage = false)
	{
		if(GameType == BnetGameType.BGT_UNKNOWN || FormatType == FormatType.FT_UNKNOWN)
			return;

		// Generate the deckstrings for the current format
		var deckboxes = _decksByFormatAndDeckId.ContainsKey(FormatType)
			? _decksByFormatAndDeckId[FormatType]
			: CacheDecks(FormatType);

		// Assemble the deck strings that are not known yet
		if(!_deckStatusByDeckstring.ContainsKey(GameType))
			_deckStatusByDeckstring[GameType] = new();

		List<string> toLoad = new();
		if(onlyVisiblePage)
		{
			if(ValidDecksOnPage is null)
				return;
			foreach(var box in ValidDecksOnPage)
			{
				if(box?.DeckId is not long deckId)
					continue;
				if(deckboxes.TryGetValue(deckId, out var deckData) && !_deckStatusByDeckstring[GameType].ContainsKey(deckData.Deckstring))
				{
					toLoad.Add(deckData.Deckstring);
					_deckStatusByDeckstring[GameType][deckData.Deckstring] = SingleDeckState.LOADING;
				}
			}
		}
		else
		{
			foreach(var deckbox in deckboxes.Values)
			{
				if(!_deckStatusByDeckstring[GameType].ContainsKey(deckbox.Deckstring))
				{
					toLoad.Add(deckbox.Deckstring);
					_deckStatusByDeckstring[GameType][deckbox.Deckstring] = SingleDeckState.LOADING;
				}
			}
		}

		OnPropertyChanged(nameof(PageStatus));
		OnPropertyChanged(nameof(PageStatusRows));

		// Assemble the request
		if(toLoad.Any())
		{
			var medalInfo = await Helper.RetryWhileNull(Reflection.Client.GetMedalInfo);
			int? starLevel = null;
			if(medalInfo != null)
			{
				var medalInfoData = VisualsFormatType switch
				{
					VisualsFormatType.VFT_STANDARD => medalInfo.Standard,
					VisualsFormatType.VFT_WILD => medalInfo.Wild,
					VisualsFormatType.VFT_CLASSIC => medalInfo.Classic,
					VisualsFormatType.VFT_TWIST => medalInfo.Twist,
					_ => null,
				};
				starLevel = medalInfoData?.StarLevel;
			}

			// It's important to copy this out, because it can change while awaiting the mulligan guide status
			// => this would lead to a "miscache"
			var theGameType = GameType;
			var results = await LoadMulliganGuideStatus(
				GameType,
				starLevel,
				toLoad
			);
			foreach(var result in results)
			{
				_deckStatusByDeckstring[theGameType][result.Key] = result.Value switch
				{
					MulliganGuideStatusData.Status.READY => SingleDeckState.READY,
					_ => SingleDeckState.NO_DATA,
				};
			}

			OnPropertyChanged(nameof(PageStatus));
			OnPropertyChanged(nameof(PageStatusRows));
		}
	}

	public List<SingleDeckStatus> PageStatus
	{
		get
		{
			if(
				ValidDecksOnPage is null ||
				FormatType == FormatType.FT_UNKNOWN ||
				!_decksByFormatAndDeckId.TryGetValue(FormatType, out var deckMap) ||
				!_deckStatusByDeckstring.TryGetValue(GameType, out var allDecks)
			)
				return new();

			return ValidDecksOnPage.Select(x =>
			{
				try
				{
					if(x is CollectionDeckBoxVisual box && box.DeckId is long deckId && deckMap.TryGetValue(deckId, out var deckData))
					{
						// At this point we know the deck is valid for this format, so either fetch the API status or show NO_DATA
						if(allDecks.TryGetValue(deckData.Deckstring, out var state))
							return new SingleDeckStatus(
								state,
								deckData.HasRunes,
								box.IsFocused || box.IsSelected
							);
						return new SingleDeckStatus(SingleDeckState.NO_DATA, deckData.HasRunes, box.IsSelected);
					}
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}

				// Something went wrong grabbing this deck (maybe not eligible for the format), hide it
				return new SingleDeckStatus();
			}).ToList();
		}
	}

	// PageStatus, but grouped into 3 rows of 3 cols
	public List<List<SingleDeckStatus>> PageStatusRows
	{
		get
		{
			List<List<SingleDeckStatus>> retval = new();
			for(var i = 0; i < 9; i++)
				retval.Add(PageStatus.Skip(i * 3).Take(3).ToList());

			return retval;
		}
	}

	public void InvalidateDeck(long deckId)
	{
		// Clear from deckId -> deckstring mapping
		foreach(var formatType in _decksByFormatAndDeckId.Keys)
		{
			if(_decksByFormatAndDeckId[formatType].ContainsKey(deckId)) {
				_decksByFormatAndDeckId[formatType].Remove(deckId);
			}
		}
	}

	public void InvalidateAllDecks()
	{
		_decksByFormatAndDeckId.Clear();
	}

	public void Reset()
	{
		_decksByFormatAndDeckId = new();
		_deckStatusByDeckstring = new();
	}
}
