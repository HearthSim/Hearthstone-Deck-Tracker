using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls
{
	/// <summary>
	/// Interaction logic for CardImage.xaml
	/// </summary>
	public partial class CardImage : INotifyPropertyChanged
	{
		private string _cardId = "";
		public string CardId
		{
			get => _cardId;
			set
			{
				_cardId = value;
				OnPropertyChanged();
			}
		}

		private string _cardImagePath = null;
		public string CardImagePath
		{
			get => _cardImagePath;
			set
			{
				_cardImagePath = value;
				OnPropertyChanged();
			}
		}

		private string _createdByText = "";
		public string CreatedByText
		{
			get => _createdByText;
			set
			{
				_createdByText = value;
				OnPropertyChanged();
			}
		}

		private Visibility _createdByVisibility = Visibility.Visible;
		public Visibility CreatedByVisibility
		{
			get => _createdByVisibility;
			set
			{
				_createdByVisibility = value;
				OnPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public CardImage()
		{
			InitializeComponent();
		}

		public async void SetCardId(string cardId)
		{
			if(cardId == CardId)
				return;
			CardId = cardId;
			if(string.IsNullOrEmpty(cardId))
			{
				CardImagePath = null;
				return;
			}
			if(!AssetDownloaders.cardImageDownloader.HasAsset(CardId))
			{
				await AssetDownloaders.cardImageDownloader.DownloadAsset(CardId);
			}
			CardImagePath = AssetDownloaders.cardImageDownloader.StoragePathFor(CardId);
			(FindResource("StoryboardExpand") as Storyboard)?.Begin();
		}
	}
}
