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
	V1_READY,
	V2_READY,
	V2_PARTIAL,
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
			SingleDeckState.V1_READY or SingleDeckState.V2_READY or SingleDeckState.NO_DATA or SingleDeckState.LOADING or SingleDeckState.V2_PARTIAL => Visibility.Visible,
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
		SingleDeckState.V2_PARTIAL => "#CCCCAA00",
		SingleDeckState.V1_READY => "#CC00AA00",
		SingleDeckState.V2_READY => "#CC00AA00",
		_ => "#CC555555"
	};

	public string Background => State switch
	{
		SingleDeckState.NO_DATA => "#CC1A1100",
		SingleDeckState.V2_PARTIAL => "#CC373700",
		SingleDeckState.V1_READY => "#CC002200",
		SingleDeckState.V2_READY => "#CC002200",
		_ => "#CC000000",
	};

	public string Label => State switch
	{
		SingleDeckState.LOADING => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_Loading"),
		SingleDeckState.NO_DATA => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_NoData"),
		SingleDeckState.V2_PARTIAL => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_Partial"),
		SingleDeckState.V1_READY => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_V1Ready"),
		SingleDeckState.V2_READY => LocUtil.Get("ConstructedMulliganGuidePreLobby_Status_V2Ready"),
		_ => State.ToString(),
	};

	public Visibility LabelVisibility => IsFocused ? Visibility.Visible : Visibility.Collapsed;
}

public class ConstructedMulliganGuidePreLobbyViewModel : ViewModel
{
	public Dictionary<BnetGameType, Dictionary<string, SingleDeckState>> DeckStatusByDeckstring = new();

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
		public int[] CardsDbfIds;
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
				CardsDbfIds = hearthDbDeck.CardDbfIds.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)).ToArray(),
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

	public bool IsOutOfTrials
	{
		get { return GetProp(false); }
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(Visibility));
		}
	}

	public Visibility Visibility => IsModalOpen || IsInQueue || IsOutOfTrials ? Visibility.Hidden : Visibility.Visible;
	#endregion

	private static async Task<Dictionary<string, SingleDeckState>> LoadMulliganGuideStatus(
    BnetGameType gameType,
    int? starLevel,
    IEnumerable<MulliganStatusDeck> deckData)
	{
	    var distinctDecks = deckData?
	        .Distinct()
	        .ToArray() ?? Array.Empty<MulliganStatusDeck>();

	    if (distinctDecks.Length == 0)
	        return new Dictionary<string, SingleDeckState>();

	    var deckstrings = distinctDecks
	        .Select(d => d.Deckstring)
	        .ToArray();

	    var isV2 = gameType == BnetGameType.BGT_RANKED_STANDARD;

	    if (isV2)
	        return await LoadV2Status(gameType, starLevel, distinctDecks, deckstrings);

	    return await LoadV1Status(gameType, starLevel, deckstrings);
	}

	private static async Task<Dictionary<string, SingleDeckState>> LoadV2Status(
	    BnetGameType gameType,
	    int? starLevel,
	    MulliganStatusDeck[] distinctDecks,
	    string[] deckstrings)
	{
	    var region = ((BnetRegion)await Helper.GetCurrentRegion()).ToString();

	    var parameters = new MulliganV2StatusParams
	    {
	        Decks = distinctDecks,
	        GameType = (int)gameType,
	        PlayerStarLevel = starLevel,
	        PlayerRegion = region
	    };

#if(DEBUG)
	    var json = JsonConvert.SerializeObject(parameters);
	    Log.Debug($"Fetching Mulligan V2 Status with parameters={json}...");
#endif

	    var result = await ApiWrapper.GetMulliganGuideStatus(parameters);

	    var resultDecks = result?.Decks ?? Enumerable.Empty<MulliganV2StatusData.Deck>();

	    return deckstrings.ToDictionary(
	        deckstring => deckstring,
	        deckstring =>
	        {
	            var status = resultDecks
	                .FirstOrDefault(d => d.Deckstring == deckstring)?.Status
	                ?? MulliganV2StatusData.Status.NONE;

	            return MapV2Status(status);
	        });
	}

	private static async Task<Dictionary<string, SingleDeckState>> LoadV1Status(
	    BnetGameType gameType,
	    int? starLevel,
	    string[] deckstrings)
	{
	    var parameters = new MulliganGuideStatusParams
	    {
	        Decks = deckstrings,
	        GameType = (int)gameType,
	        PlayerStarLevel = starLevel
	    };

	    var result = await ApiWrapper.GetMulliganGuideStatus(parameters);
	    var resultDecks = result?.Decks;

	    return deckstrings.ToDictionary(
		    deckstring => deckstring,
		    deckstring =>
		    {
			    var status = resultDecks != null && resultDecks.TryGetValue(deckstring, out var deck)
				    ? deck.Status
				    : MulliganGuideStatusData.Status.NO_DATA;

			    return MapV1Status(status);
		    }
	    );
	}

	private static SingleDeckState MapV2Status(
	    MulliganV2StatusData.Status status)
	{
	    return status switch
	    {
	        MulliganV2StatusData.Status.SUPPORTED => SingleDeckState.V2_READY,
	        MulliganV2StatusData.Status.PARTIAL   => SingleDeckState.V2_PARTIAL,
	        _                                     => SingleDeckState.NO_DATA
	    };
	}

	private static SingleDeckState MapV1Status(
		MulliganGuideStatusData.Status status)
	{
		return status switch
		{
			MulliganGuideStatusData.Status.READY => SingleDeckState.V1_READY,
			MulliganGuideStatusData.Status.NO_DATA   => SingleDeckState.NO_DATA,
			_                                     => SingleDeckState.NO_DATA
		};
	}

	public async Task EnsureLoaded()
	{
		try
		{
			await Update(true);
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
		if(!DeckStatusByDeckstring.ContainsKey(GameType))
			DeckStatusByDeckstring[GameType] = new();

		List<MulliganStatusDeck> toLoad = new();
		if(onlyVisiblePage)
		{
			if(ValidDecksOnPage is null)
				return;
			foreach(var box in ValidDecksOnPage)
			{
				if(box?.DeckId is not long deckId)
					continue;
				if(deckboxes.TryGetValue(deckId, out var deckData) && !DeckStatusByDeckstring[GameType].ContainsKey(deckData.Deckstring))
				{
					toLoad.Add(new MulliganStatusDeck
					{
						Deckstring = deckData.Deckstring,
						DbfIds = deckData.CardsDbfIds
					});
					DeckStatusByDeckstring[GameType][deckData.Deckstring] = SingleDeckState.LOADING;
				}
			}
		}
		else
		{
			foreach(var deckbox in deckboxes.Values)
			{
				if(!DeckStatusByDeckstring[GameType].ContainsKey(deckbox.Deckstring))
				{
					toLoad.Add(new MulliganStatusDeck
					{
						Deckstring = deckbox.Deckstring,
						DbfIds = deckbox.CardsDbfIds
					});
					DeckStatusByDeckstring[GameType][deckbox.Deckstring] = SingleDeckState.LOADING;
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
				DeckStatusByDeckstring[theGameType][result.Key] = result.Value;
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
				!DeckStatusByDeckstring.TryGetValue(GameType, out var allDecks)
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
		DeckStatusByDeckstring = new();
	}

	public string HSReplayIcon => "/HearthstoneDeckTracker;component/Resources/hsreplay_logo_white.png";
}
