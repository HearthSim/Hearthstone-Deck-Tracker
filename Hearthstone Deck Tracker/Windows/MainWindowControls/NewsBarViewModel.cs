using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public class NewsBarViewModel : ViewModel
	{
		private TextBlock _newsContent;
		private string _indexContent;

		public ICommand PreviousItemCommand => new Command(NewsManager.PreviousNewsItem);
		public ICommand NextItemCommand => new Command(NewsManager.NextNewsItem);
		public ICommand CloseCommand => new Command(NewsManager.ToggleNewsVisibility);

		public TextBlock NewsContent
		{
			get => _newsContent;
			set
			{
				_newsContent = value;
				OnPropertyChanged();
			}
		}

		public string IndexContent
		{
			get => _indexContent;
			set
			{
				_indexContent = value;
				OnPropertyChanged();
			}
		}
	}
}
