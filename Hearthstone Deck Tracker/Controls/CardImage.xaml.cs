using System.ComponentModel;
using System.Runtime.CompilerServices;
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

		private string _cardImagePath = "Resources/faceless_manipulator.png";
		public string CardImagePath
		{
			get => _cardImagePath;
			set
			{
				_cardImagePath = value;
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
			CardId = cardId;
			if(!AssetDownloaders.cardImageDownloader.HasAsset(CardId))
			{
				await AssetDownloaders.cardImageDownloader.DownloadAsset(CardId);
			}
			CardImagePath = AssetDownloaders.cardImageDownloader.StoragePathFor(CardId);
		}
	}
}
