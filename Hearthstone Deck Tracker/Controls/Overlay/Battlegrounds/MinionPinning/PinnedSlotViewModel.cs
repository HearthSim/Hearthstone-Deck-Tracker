﻿using System;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Controls.Overlay;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.MinionPinning
{
	public class PinnedSlotViewModel : ViewModel
	{
		private readonly BattlegroundsMinionPinningViewModel _owner;

		public PinnedSlotViewModel(BattlegroundsMinionPinningViewModel owner)
		{
			_owner = owner;
			UnpinCommand = new Command(() =>
			{
				if(IsClearButton)
					_owner.ClearPins();
				else if(!string.IsNullOrEmpty(CardId))
					_owner.UnpinCard(CardId!);
			});
		}

		public bool IsClearButton
		{
			get => GetProp(false);
			set => SetProp(value);
		}

		public string? CardId
		{
			get => GetProp<string?>(null);
			set
			{
				SetProp(value);
				if(string.IsNullOrEmpty(value))
				{
					CardAsset = null;
					Tier = 1;
				}
				else
				{
					var card = Database.GetCardFromId(value);
					CardAsset = card != null ? new CardAssetViewModel(card, CardAssetType.Portrait) : null;
					Tier = Math.Max(1, card?.TechLevel ?? 1);
				}
				OnPropertyChanged(nameof(HasCard));
			}
		}

		public bool HasCard => !string.IsNullOrEmpty(CardId);

		public CardAssetViewModel? CardAsset
		{
			get => GetProp<CardAssetViewModel?>(null);
			private set => SetProp(value);
		}

		public int Tier
		{
			get => GetProp(1);
			private set => SetProp(value);
		}

		public ICommand UnpinCommand { get; }
	}
}
