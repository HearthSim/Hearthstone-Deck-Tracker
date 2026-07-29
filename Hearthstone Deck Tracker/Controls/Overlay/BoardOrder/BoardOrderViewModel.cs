using System.Collections.Generic;
using System.Windows;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using HearthWatcher.EventArgs;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.BoardOrder
{
	public class BoardOrderViewModel : ViewModel
	{
		// 7 board positions plus one spacer for the drop gap.
		private const int SlotCount = 8;

		// Bounded by both boards plus both weapons; avoid allocating a string per badge per update.
		private static readonly string[] RankLabels =
		{
			"1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16"
		};

		private static string RankLabel(int rank)
			=> rank >= 1 && rank <= RankLabels.Length ? RankLabels[rank - 1] : rank.ToString();

		public BoardOrderViewModel()
		{
			for(var i = 0; i < SlotCount; i++)
				Slots.Add(new BoardOrderSlotViewModel());
		}

		public List<BoardOrderSlotViewModel> Slots { get; } = new();

		public BoardOrderSlotViewModel Weapon { get; } = new();

		public Visibility Visibility
		{
			get => GetProp(Visibility.Collapsed);
			set => SetProp(value);
		}

		public void UpdateSizes(double width, double height, Thickness margin)
		{
			foreach(var slot in Slots)
			{
				slot.Width = width;
				slot.Height = height;
				slot.Margin = margin;
			}
			Weapon.Width = width;
			Weapon.Height = height;
		}

		public void Clear()
		{
			foreach(var slot in Slots)
			{
				slot.Label = null;
				slot.IsOccupied = false;
			}
			Weapon.Label = null;
			Weapon.IsOccupied = false;
		}

		public void Update(PlayZoneArgs? zone, Entity? weapon, IReadOnlyDictionary<int, int> ranks)
		{
			foreach(var slot in Slots)
			{
				slot.Label = null;
				slot.IsOccupied = false;
			}

			Weapon.IsOccupied = weapon != null;
			Weapon.Label = weapon != null && ranks.TryGetValue(weapon.Id, out var weaponRank)
				? RankLabel(weaponRank)
				: null;

			var cards = zone?.BoardCards;
			if(zone == null || cards == null)
				return;

			if(zone.MousedOverSlot > 0 && zone.MousedOverSlot <= Slots.Count)
				Slots[zone.MousedOverSlot - 1].IsOccupied = true;

			for(var i = 0; i < cards.Count; i++)
			{
				var oneBasedIndex = i + 1;
				var targetPos = zone.MousedOverSlot > 0 && oneBasedIndex >= zone.MousedOverSlot
					? oneBasedIndex + 1
					: oneBasedIndex;

				var targetIdx = targetPos - 1;
				if(targetIdx < 0 || targetIdx >= Slots.Count)
					continue;

				Slots[targetIdx].IsOccupied = true;
				if(cards[i]?.EntityId is int entityId && ranks.TryGetValue(entityId, out var rank))
					Slots[targetIdx].Label = RankLabel(rank);
			}
		}
	}
}
