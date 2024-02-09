using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class DeckPickerEventArgs : System.EventArgs
	{
		public VisualsFormatType SelectedFormatType { get; }
		public List<CollectionDeckBoxVisual?> DecksOnPage { get; }
		public long? SelectedDeck { get; }
		public bool IsModalOpen { get; }

		public DeckPickerEventArgs(VisualsFormatType selectedFormatType, List<CollectionDeckBoxVisual?> decksOnPage, long? selectedDeck, bool isModalOpen)
		{
			SelectedFormatType = selectedFormatType;
			DecksOnPage = decksOnPage;
			SelectedDeck = selectedDeck;
			IsModalOpen = isModalOpen;
		}

		public override bool Equals(object obj) => obj is DeckPickerEventArgs args
		                                           && args.SelectedFormatType == SelectedFormatType
		                                           && args.DecksOnPage.SequenceEqual(DecksOnPage)
		                                           && args.SelectedDeck == SelectedDeck
		                                           && args.IsModalOpen == IsModalOpen;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + SelectedFormatType.GetHashCode();
			hashCode = hashCode * -1521134295 + DecksOnPage.GetHashCode();
			hashCode = hashCode * -1521134295 + SelectedDeck.GetHashCode();
			hashCode = hashCode * -1521134295 + IsModalOpen.GetHashCode();
			return hashCode;
		}
	}
}
