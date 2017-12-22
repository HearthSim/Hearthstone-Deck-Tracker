#region

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;

#endregion

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckHistory
{
	public partial class DeckVersionChangeView : UserControl
	{
		public DeckVersionChangeView()
		{
			InitializeComponent();
		}
	}

	public class DeckVersionChangeViewModel : ViewModel
	{
		public DeckVersionChangeViewModel(Deck prev, Deck next)
		{
			Versions = new DeckDiffData(prev, next);
		}

		private DeckDiffData _versions;
		private string _header;
		private IEnumerable<Card> _cards;

		public DeckDiffData Versions
		{
			get => _versions;
			set
			{
				_versions = value; 
				OnPropertyChanged();
				Header = $"{Versions?.Previous.Version.ShortVersionString} -> {Versions?.Next.Version.ShortVersionString}";
				Cards = value.Next - value.Previous;
			}
		}

		public string Header
		{
			get => _header;
			set
			{
				_header = value; 
				OnPropertyChanged();
			}
		}

		public IEnumerable<Card> Cards
		{
			get => _cards;
			set
			{
				_cards = value; 
				OnPropertyChanged();
			}
		}

		public class DeckDiffData
		{
			public Deck Previous { get; set; }
			public Deck Next { get; set; }

			public DeckDiffData(Deck prev, Deck next)
			{
				Previous = prev;
				Next = next;
			}
		}
	}
}
