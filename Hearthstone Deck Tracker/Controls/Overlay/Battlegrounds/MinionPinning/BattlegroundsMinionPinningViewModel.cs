using System;
using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Guides.Comps;
using System.Collections.ObjectModel;
using Hearthstone_Deck_Tracker.Utility;
using HSReplay.Responses;


namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.MinionPinning
{

	public class BattlegroundsMinionPinningViewModel : ViewModel
	{
		public event EventHandler? PinsChanged;

		public double Scaling { get => GetProp(1.0); set => SetProp(value); }

		private readonly List<string> _pinnedCardIds = new();
		private readonly HashSet<string> _racePinnedCardIds = new();
		private readonly HashSet<Race> _selectedRaces = new();
		private readonly HashSet<string> _recommendedCardIds = new();
		private readonly Dictionary<string, List<BattlegroundsCompGuideViewModel>> _recommendedCardGuides = new();

		public int PinnedCount => _pinnedCardIds.Count;
		public bool HasPins => PinnedCount > 0;

		public ObservableCollection<PinnedSlotViewModel> PinnedSlots { get; } = new();

		private const int SlotGroupSize = 5;

		public BattlegroundsMinionPinning? View { get; set; }

		public BattlegroundsMinionPinningViewModel()
		{
			for(var i = 0; i < SlotGroupSize; i++)
				PinnedSlots.Add(new PinnedSlotViewModel(this));

			AvailableRaces = BattlegroundsUtils.GetAvailableRaces();
		}

		private void EnsureCapacity(int required)
		{
			var groupsNeeded = (required + SlotGroupSize - 1) / SlotGroupSize;
			if(groupsNeeded < 1)
				groupsNeeded = 1;
			var target = groupsNeeded * SlotGroupSize;

			if(PinnedSlots.Count < target)
			{
				var toAdd = target - PinnedSlots.Count;
				for(var i = 0; i < toAdd; i++)
					PinnedSlots.Add(new PinnedSlotViewModel(this));
				OnPropertyChanged(nameof(PinnedSlots));
			}
			else if(PinnedSlots.Count > target)
			{
				var toRemove = PinnedSlots.Count - target;
				for(var i = 0; i < toRemove; i++)
					PinnedSlots.RemoveAt(PinnedSlots.Count - 1);
				OnPropertyChanged(nameof(PinnedSlots));
			}
		}

		public int MousedOverSlot
		{
			get => GetProp(-1);
			set
			{
				SetProp(value);
				OnPropertyChanged(nameof(ShopCards));
			}
		}

		public List<BattlegroundsMinionPinningCardViewModel> ShopCards { get; } = new() {
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
			new BattlegroundsMinionPinningCardViewModel(),
		};

		public void PinCard(string cardId)
		{
			if(string.IsNullOrEmpty(cardId))
				return;
			if(_pinnedCardIds.Contains(cardId))
				return;
			// Add the pin
			_pinnedCardIds.Add(cardId);
			IsExpanded = true;
			EnsureCapacity(_pinnedCardIds.Count);
			OnPropertyChanged(nameof(PinnedCount));
			OnPropertyChanged(nameof(HasPins));
			SyncPinnedSlotsFromIds();
			UpdatePinnedFlags();
			PinsChanged?.Invoke(this, EventArgs.Empty);
		}

		public void TogglePinCard(string cardId)
		{
			if(_pinnedCardIds.Contains(cardId))
			{
				UnpinCard(cardId);
				return;
			}
			PinCard(cardId);
		}

		public void UnpinCard(string cardId)
		{
			if(_pinnedCardIds.Remove(cardId))
			{
				IsExpanded = true;
				EnsureCapacity(_pinnedCardIds.Count);
				OnPropertyChanged(nameof(PinnedCount));
				OnPropertyChanged(nameof(HasPins));
				SyncPinnedSlotsFromIds();
				UpdatePinnedFlags();
				PinsChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		public void ClearPins()
		{
			if(_pinnedCardIds.Count == 0)
				return;
			_pinnedCardIds.Clear();
			EnsureCapacity(_pinnedCardIds.Count);
			OnPropertyChanged(nameof(PinnedCount));
			OnPropertyChanged(nameof(HasPins));
			SyncPinnedSlotsFromIds();
			UpdatePinnedFlags();
			PinsChanged?.Invoke(this, EventArgs.Empty);
		}

		public bool IsCardPinned(string cardId)
		{
			return !string.IsNullOrEmpty(cardId) && _pinnedCardIds.Contains(cardId);
		}

		public bool EnableRecommended
		{
			get => GetProp(false);
			set
			{
				var current = GetProp(false);
				if(current == value)
					return;

				SetProp(value);

				if(value)
					RecomputeRecommendedFromGuides();
				else
				{
					_recommendedCardIds.Clear();
					_recommendedCardGuides.Clear();
				}

				UpdatePinnedFlags();
			}
		}

		public Command ToggleRecommendedCommand => new Command(() =>
		{
			var wasEnabled = EnableRecommended;
			EnableRecommended = !EnableRecommended;
			Core.Game.Metrics.TavernMarkersRecommendedToggled = true;

			if(!EnableRecommended)
				Core.Game.Metrics.TavernMarkersRecommendedDisabledTurn = Core.Game.GetTurnNumber();

			// Show popup when turning off
			if (wasEnabled && !EnableRecommended)
			{
				View?.ShowAutoEnablePopup();
			}
		});

		private void RecomputeRecommendedFromGuides()
		{
			_recommendedCardIds.Clear();
			_recommendedCardGuides.Clear();

			var guidesVm = Core.Overlay.BattlegroundsCompsGuidesVM;
			if(guidesVm == null)
				return;

			var availableRaces = AvailableRaces ?? BattlegroundsDbSingleton.Instance.Races;
			var currentRaces = new HashSet<Race>(availableRaces.Concat(new[] { Race.ALL, Race.INVALID }));
			var availableCards = BattlegroundsDbSingleton.Instance.GetCardsByRaces(currentRaces, Core.Game.IsBattlegroundsDuosMatch);
			var availableDbfIds = new HashSet<int>(availableCards.Select(c => c.DbfId));

			var allGuides = Enumerable.Empty<BattlegroundsCompGuideViewModel>();
			if(guidesVm.CompsByTier != null)
				allGuides = guidesVm.CompsByTier.Values.Where(v => v.Comps != null).SelectMany(v => v.Comps!);
			else if(guidesVm.Comps != null)
				allGuides = guidesVm.Comps;

			foreach(var guide in allGuides)
			{
				foreach(var line in guide.WhenToCommitTags)
				{
					foreach(var inline in line)
					{
						if(inline is Guides.ReferencedCardRun r && r.Card?.DbfId is int dbfId)
						{
							if(!availableDbfIds.Contains(dbfId))
								continue;
							var card = Database.GetCardFromDbfId(dbfId, false);
							if(card?.Id is { } id && !string.IsNullOrEmpty(id) && card.IsKnownCard)
							{
								_recommendedCardIds.Add(id);
								if(!_recommendedCardGuides.TryGetValue(id, out var list))
								{
									list = new List<BattlegroundsCompGuideViewModel>();
									_recommendedCardGuides[id] = list;
								}
								if(!list.Contains(guide))
									list.Add(guide);
							}
						}
					}
				}
			}
		}

		public void SetTooltipPosition(BattlegroundsMinionPinningCardViewModel slot, int cardCount, int cardIndex)
		{
			if(slot == null)
				return;

			var halfCount = cardCount / 2;
			var isOdd = cardCount % 2 == 1;
			if (isOdd)
			{
				if (cardIndex == halfCount)
					slot.RecommendedSectionCanvasLeft = -620;
				else if (cardIndex < halfCount)
					slot.RecommendedSectionCanvasLeft = -80;
				else
					slot.RecommendedSectionCanvasLeft = -50;
			}
			else
			{
				if (cardIndex == halfCount - 1 || cardIndex == halfCount)
					slot.RecommendedSectionCanvasLeft = -620;
				else if (cardIndex < halfCount - 1)
					slot.RecommendedSectionCanvasLeft = -80;
				else
					slot.RecommendedSectionCanvasLeft = -50;
			}
		}

		public void OnShopChange(List<BoardCard> boardCards, int mousedOverSlot)
		{
			MousedOverSlot = mousedOverSlot;

			ClearShopCards();

			if(MousedOverSlot > 0 && MousedOverSlot <= ShopCards.Count)
			{
				ShopCards[MousedOverSlot - 1].IsMinionPinned = false;
				ShopCards[MousedOverSlot - 1].IsSlotOccupied = true;
			}

			for(int i = 0; i < boardCards.Count; i++)
			{
				var oneBasedIndex = i + 1;
				var targetPos = (MousedOverSlot > 0 && oneBasedIndex >= MousedOverSlot)
					? oneBasedIndex + 1
					: oneBasedIndex;

				var targetIdx = targetPos - 1;
				if(targetIdx < 0 || targetIdx >= ShopCards.Count)
					continue;

				var cardId = boardCards[i]?.CardId;
				ShopCards[targetIdx].IsSlotOccupied = true;
				ShopCards[targetIdx].CardId = cardId;
				ShopCards[targetIdx].IsHovered = boardCards[i]?.Hovered ?? false;
				SetTooltipPosition(ShopCards[targetIdx], boardCards.Count, i);
			}
			UpdatePinnedFlags();
		}

		public void OnShopPhaseEnd()
		{
			// Nothing to do here for now
		}

		private void UpdatePinnedFlags()
		{
			foreach(var slot in ShopCards)
			{
				if(!slot.IsSlotOccupied || string.IsNullOrEmpty(slot.CardId))
				{
					slot.IsMinionPinned = false;
					slot.IsTribePinned = false;
					slot.TribeIconRace = Race.INVALID;
					slot.IsRecommendedPinned = false;
					slot.RecommendedComps = null;
					continue;
				}

				// Manual pin indicator (slot-level)
				if(slot.CardId != null)
				{
					slot.IsMinionPinned = _pinnedCardIds.Contains(slot.CardId);

					// Tribe pin indicator (separate state driven by selected races)
					var card = Database.GetCardFromId(slot.CardId);
					var primary = card?.RaceEnum;
					var secondary = card?.SecondaryRaceEnum;
					Race pinnedRace = Race.INVALID;
					if(primary is Race pr && _selectedRaces.Contains(pr) && pr != Race.INVALID && pr != Race.ALL)
						pinnedRace = pr;
					else if(secondary is Race sr && _selectedRaces.Contains(sr) && sr != Race.INVALID && sr != Race.ALL)
						pinnedRace = sr;

					slot.IsTribePinned = pinnedRace != Race.INVALID;
					slot.TribeIconRace = pinnedRace;

					// Recommended pin indicator (separate)
					if(EnableRecommended && slot.CardId != null && _recommendedCardIds.Contains(slot.CardId))
					{
						slot.IsRecommendedPinned = true;
						if(_recommendedCardGuides.TryGetValue(slot.CardId, out var guides) && guides.Count > 0)
							slot.RecommendedComps = guides;
						else
							slot.RecommendedComps = null;
					}
					else
					{
						slot.IsRecommendedPinned = false;
						slot.RecommendedComps = null;
					}
				}
			}
		}

	private void SyncPinnedSlotsFromIds()
	{
		int clearButtonIndex = -1;
		if(_pinnedCardIds.Count > 0)
		{
			var totalItems = _pinnedCardIds.Count + 1;
			var rowForClearButton = (totalItems - 1) / SlotGroupSize;
			clearButtonIndex = (rowForClearButton + 1) * SlotGroupSize - 1;
		}

		// Ensure we have enough slots (up to and including the Clear All button position)
		var requiredSlots = clearButtonIndex >= 0 ? clearButtonIndex + 1 : SlotGroupSize;
		EnsureCapacity(requiredSlots);

		for(var i = 0; i < PinnedSlots.Count; i++)
		{
			if(i == clearButtonIndex)
			{
				PinnedSlots[i].IsClearButton = true;
				PinnedSlots[i].CardId = null;
			}
			else if(i < _pinnedCardIds.Count)
			{
				PinnedSlots[i].IsClearButton = false;
				PinnedSlots[i].CardId = _pinnedCardIds[i];
			}
			else
			{
				PinnedSlots[i].IsClearButton = false;
				PinnedSlots[i].CardId = null;
			}
		}
		OnPropertyChanged(nameof(PinnedSlots));
	}

		public void ClearShopCards()
		{
			ShopCards.ForEach(x =>
			{
				x.IsMinionPinned = false;
				x.IsSlotOccupied = false;
				x.CardId = null;
				x.IsHovered = false;
				x.IsTribePinned = false;
				x.TribeIconRace = Race.INVALID;
				x.IsRecommendedPinned = false;
				x.RecommendedComps = null;
			});
		}

		public void Reset()
		{
			Core.Game.Metrics.TavernMarkersRecommendedEnabled = EnableRecommended;

			MousedOverSlot = -1;
			ClearShopCards();
			ClearPins();
			_selectedRaces.Clear();
			_racePinnedCardIds.Clear();
			EnableRecommended = Config.Instance.AutoEnableTavernMarkersRecommended;
			if(!EnableRecommended)
			{
				_recommendedCardIds.Clear();
				_recommendedCardGuides.Clear();
			}
			IsExpanded = Config.Instance.TavernMarkersPanelExpanded;
			OnPropertyChanged(nameof(MinionTypeButtons));
		}

		public IEnumerable<Race>? AvailableRaces
		{
			get => GetProp<IEnumerable<Race>?>(null);
			set
			{
				SetProp(value);
				var newSet = (value ?? BattlegroundsDbSingleton.Instance.Races).ToHashSet();
				_selectedRaces.RemoveWhere(r => !newSet.Contains(r));
				RecomputeRacePinnedIds();
				if(EnableRecommended)
					RecomputeRecommendedFromGuides();
				UpdatePinnedFlags();
				OnPropertyChanged(nameof(MinionTypeButtons));
			}
		}

		public class MinionTypeButtonModel
		{
			public Race MinionType { get; set; }
			public bool Active { get; set; }
			public bool Available { get; set; }
			public bool Faded { get; set; }
			public int Size { get; set; }
		}

		public List<MinionTypeButtonModel> MinionTypeButtons
		{
			get
			{
				var races = (AvailableRaces ?? BattlegroundsDbSingleton.Instance.Races).ToList();
				// Don't show unwanted races in buttons
				races.Remove(Race.INVALID);
				races.Remove(Race.ALL);

				var hasActiveSelection = _selectedRaces.Count > 0;

				return races.Select(x => new MinionTypeButtonModel
				{
					MinionType = x,
					Active = _selectedRaces.Contains(x),
					Available = (AvailableRaces ?? races).Contains(x),
					Faded = hasActiveSelection && !_selectedRaces.Contains(x),
					Size = 34
				}).ToList();
			}
		}

		public Command<Race> SetActiveMinionTypeCommand => new Command<Race>(race =>
		{
			if(_selectedRaces.Contains(race))
				_selectedRaces.Remove(race);
			else
				_selectedRaces.Add(race);

			RecomputeRacePinnedIds();
			OnPropertyChanged(nameof(MinionTypeButtons));
			UpdatePinnedFlags();
			Core.Game.Metrics.TavernMarkersTribeToggled = true;
		});

		// public Command<string> TogglePinCardCommand => new Command<string>(cardId =>
		// {
		// 	if(string.IsNullOrEmpty(cardId))
		// 		return;
		// 	TogglePinCard(cardId);
		// });

		private void RecomputeRacePinnedIds()
		{
			_racePinnedCardIds.Clear();
			if(_selectedRaces.Count == 0)
				return;
			var duos = Core.Game.IsBattlegroundsDuosMatch;
			var cards = BattlegroundsDbSingleton.Instance
				.GetCardsByRaces(_selectedRaces, duos)
				.Where(c => c.IsKnownCard && c.IsBaconMinion);
			foreach(var c in cards)
			{
				if(!string.IsNullOrEmpty(c.Id))
					_racePinnedCardIds.Add(c.Id);
			}
		}

		public bool IsExpanded
		{
			get => GetProp(Config.Instance.TavernMarkersPanelExpanded);
			set
			{
				SetProp(value);
				ConfigWrapper.TavernMarkersPanelExpanded = value;
				Core.Overlay.UpdateElementSizes();
			}
		}

		public Command ToggleExpandCommand => new (() => IsExpanded = !IsExpanded);
	}
}
