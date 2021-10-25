using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries
{
	public class MercenariesTaskViewModel : ViewModel
	{

		public MercenariesTaskViewModel(Hearthstone.Card mercCard, string title, string description, int quota, int progress)
		{
			CardPortrait = new CardAssetViewModel(mercCard, Utility.Assets.CardAssetType.Portrait);
			Title = title;
			Description = description;
			var completed = progress >= quota;
			ProgressText = completed ? LocUtil.Get("MercenariesTaskList_TaskCompleted") : $"{progress} / {quota}";
			Progress = 1.0 * progress / quota;
		}

		public CardAssetViewModel CardPortrait { get; }
		public string Title { get; }
		public string Description { get; }
		public string ProgressText { get; }
		public double Progress { get; }
	}
}
