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
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		public DeckVersionChangeViewModel(Deck prev, Deck next)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
		{
			Versions = new DeckDiffData(prev, next);
		}

		private DeckDiffData _versions;
		private string? _header;
		private IEnumerable<Card>? _cards;

		public DeckDiffData Versions
		{
			get => _versions;
			set
			{
				_versions = value; 
				OnPropertyChanged();
				Header = $"{value.Previous.Version.ShortVersionString} -> {value.Next.Version.ShortVersionString}";
				Cards = value.Next - value.Previous;
			}
		}

		public string? Header
		{
			get => _header;
			set
			{
				_header = value; 
				OnPropertyChanged();
			}
		}

		public IEnumerable<Card>? Cards
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
