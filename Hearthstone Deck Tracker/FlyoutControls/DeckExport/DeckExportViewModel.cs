﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HearthDb.Deckstrings;
using HearthDb.Enums;
using Hearthstone_Deck_Tracker.Controls.Error;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using Deck = Hearthstone_Deck_Tracker.Hearthstone.Deck;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckExport
{
	public class DeckExportViewModel : ViewModel
	{
		private Deck _deck;
		private string _deckString;
		private string _deckStringWithComments;
		private string _copyAllButtonText;
		private string _copyCodeButtonText;
		private bool _hasCollectionData;
		private List<DustCostViewModel> _missingCards = new List<DustCostViewModel>();
		private bool _updatingCollection;
		private bool _hasMissingCards;
		private int _totalDustCost;
		private bool _requiresKarazhan;
		private string _missingCardsHeader = LocUtil.Get(LocMissingCardsHeader);
		private const string LocCopyAll = "DeckExport_Button_CopyAll";
		private const string LocCopyCode = "DeckExport_Button_CopyCode";
		private const string LocCopied = "DeckExport_Button_Copied";
		private const string LocMissingCardsHeader = "DeckExport_Header_MissingCards";

		public DeckExportViewModel()
		{
			CopyAllButtonText = LocUtil.Get(LocCopyAll, true);
			CopyCodeButtonText = LocUtil.Get(LocCopyCode, true);
			CollectionHelper.OnCollectionChanged += UpdateMissingCards;
		}

		private async void UpdateMissingCards()
		{
			if(Deck == null || _updatingCollection)
				return;
			_updatingCollection = true;
			var collection = await CollectionHelper.GetCollection();
			if(!collection?.Any() ?? true)
			{
				_updatingCollection = false;
				return;
			}
			var missingCards = new List<DustCostViewModel>();
			foreach(var card in Deck.Cards)
			{
				collection.TryGetValue(card.Id, out var count);
				var missingCount = card.Count - count;
				if(missingCount > 0)
				{
					var missing = (Card)card.Clone();
					missing.Count = missingCount;
					missingCards.Add(new DustCostViewModel(missing));
				}
			}
			MissingCards = missingCards;
			Helper.SortCardCollection(MissingCards, false);
			HasMissingCards = missingCards.Any();
			TotalDustCost = missingCards.Sum(x => x.DustCost);
			RequiresKarazhan = missingCards.Any(c => c.Card.CardSet == CardSet.KARA);
			var missingCardsCount = missingCards.Sum(c => c.Card.Count);
			MissingCardsHeader = LocUtil.Get(LocMissingCardsHeader) + $" ({missingCardsCount})";
			HasCollectionData = true;
			_updatingCollection = false;
		}

		public Deck Deck
		{
			get => _deck;
			set
			{
				_deck = value;
				if(_deck != null)
				{
					try
					{
						var deck = HearthDbConverter.ToHearthDbDeck(_deck);
						DeckString = DeckSerializer.Serialize(deck, false);
						DeckStringWithComments = DeckSerializer.Serialize(deck, true) + "# Generated by HDT - https://hsdecktracker.net";
					}
					catch(Exception e)
					{
						Log.Error(e);
					}
				}
				OnPropertyChanged();
				UpdateMissingCards();
			}
		}

		public string DeckStringWithComments
		{
			get => _deckStringWithComments;
			set
			{
				_deckStringWithComments = value;
				OnPropertyChanged();
			}
		}

		public string DeckString
		{
			get => _deckString;
			set
			{
				_deckString = value;
				OnPropertyChanged();
			}
		}

		public string CopyAllButtonText
		{
			get => _copyAllButtonText;
			set
			{
				_copyAllButtonText = value;
				OnPropertyChanged();
			}
		}

		public string CopyCodeButtonText
		{
			get => _copyCodeButtonText;
			set
			{
				_copyCodeButtonText = value;
				OnPropertyChanged();
			}
		}

		public ICommand CopyAllCommand => new Command(CopyAll);

		public ICommand CopyCodeCommand => new Command(CopyCode);

		public bool HasCollectionData
		{
			get => _hasCollectionData;
			set
			{
				_hasCollectionData = value; 
				OnPropertyChanged();
			}
		}

		public bool HasMissingCards
		{
			get => _hasMissingCards;
			set
			{
				_hasMissingCards = value; 
				OnPropertyChanged();
			}
		}

		public List<DustCostViewModel> MissingCards
		{
			get => _missingCards;
			set
			{
				if(value != _missingCards)
				{
					_missingCards = value; 
					OnPropertyChanged();
				}
			}
		}

		public int TotalDustCost
		{
			get => _totalDustCost;
			set
			{
				_totalDustCost = value; 
				OnPropertyChanged();
			}
		}

		public bool RequiresKarazhan
		{
			get => _requiresKarazhan;
			set
			{
				_requiresKarazhan = value; 
				OnPropertyChanged();
			}
		}

		public string RequiresKarazhanText => "+ " + LocUtil.Get("MainWindow_DeckBuilder_Filter_Set_Kara");

		public string MissingCardsHeader
		{
			get => _missingCardsHeader;
			set
			{
				_missingCardsHeader = value; 
				OnPropertyChanged();
			}
		}

		public async void CopyAll()
		{
			if(Deck == null)
				return;
			try
			{
				Clipboard.SetDataObject(DeckStringWithComments);
			}
			catch(Exception e)
			{
				ErrorManager.AddError("Error copying deck to clipboard", e.ToString());
			}
			CopyAllButtonText = LocUtil.Get(LocCopied, true);
			await Task.Delay(2000);
			CopyAllButtonText = LocUtil.Get(LocCopyAll, true);
		}

		public async void CopyCode()
		{
			if(Deck == null)
				return;
			try
			{
				Clipboard.SetDataObject(DeckString);
			}
			catch(Exception e)
			{
				ErrorManager.AddError("Error copying deck to clipboard", e.ToString());
			}
			CopyCodeButtonText = LocUtil.Get(LocCopied, true);
			await Task.Delay(2000);
			CopyCodeButtonText = LocUtil.Get(LocCopyCode, true);
		}
	}

	public class DustCostViewModel
	{
		private readonly Card _card;

		public DustCostViewModel(Card card)
		{
			_card = card;
		}

		public int Cost => _card.Cost;

		public string LocalizedName => _card.LocalizedName;

		public string CostString => _card.CardSet == CardSet.KARA ? "*" : DustCost.ToString();

		public int DustCost => _card.Count * _card.DustCost;

		public Card Card => _card;
	}
}
