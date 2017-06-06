using System;
using System.Collections.ObjectModel;
using System.Linq;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.FlyoutControls.DeckHistory
{
	public class DeckHistoryViewModel : ViewModel
	{
		private Deck _deck;

		public Deck Deck
		{
			get => _deck;
			set
			{
				_deck = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(CurrentVersion));
				UpdateVersions(value);
			}
		}

		private void UpdateVersions(Deck deck)
		{
			DeckVersions.Clear();
			if(!deck.HasVersions)
				return;

			var prev = deck;
			var versions = deck.Versions.OrderByDescending(x => x.Version);
			foreach(var version in versions)
			{
				DeckVersions.Add(new DeckVersionChangeViewModel(version, prev));
				prev = version;
			}
			OnPropertyChanged(nameof(DeckVersions));
		}

		public string CurrentVersion => Deck?.Version.ShortVersionString;

		public ObservableCollection<DeckVersionChangeViewModel> DeckVersions { get; set; } = new ObservableCollection<DeckVersionChangeViewModel>();
	}
}
