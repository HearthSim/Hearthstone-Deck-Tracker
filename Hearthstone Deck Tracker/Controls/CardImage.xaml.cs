using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
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

		private ImageSource _loadingImageSource = null;
		public ImageSource LoadingImageSource
		{
			get => _loadingImageSource;
			set
			{
				_loadingImageSource = value;
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

		private ImageSource GetLoadingImagePath(Hearthstone.Card card)
		{
			switch(card?.Type)
			{
				case "Hero":
					return FindResource("LoadingHero") as ImageSource;
				case "Minion":
					return FindResource("LoadingMinion") as ImageSource;
				case "Weapon":
					return FindResource("LoadingWeapon") as ImageSource;
				default:
					return FindResource("LoadingSpell") as ImageSource;
			}
		}

		Storyboard ExpandAnimation => FindResource("StoryboardExpand") as Storyboard;

		public async void SetCardIdFromCard(Hearthstone.Card card)
		{
			var newCardId = card?.Id;
			if(newCardId == CardId)
				return;
			CardId = newCardId;
			if(string.IsNullOrEmpty(newCardId))
			{
				CardImagePath = null;
				LoadingImageSource = null;
				return;
			}
			var hasAsset = AssetDownloaders.cardImageDownloader.HasAsset(card);
			if(!hasAsset)
			{
				CardImagePath = null;
				LoadingImageSource = GetLoadingImagePath(card);
				ExpandAnimation?.Begin();
				try
				{
					await AssetDownloaders.cardImageDownloader.DownloadAsset(card);
				}
				catch(ArgumentNullException)
				{
					return;
				}
			}
			if(newCardId != CardId)
				return;
			try
			{
				CardImagePath = AssetDownloaders.cardImageDownloader.StoragePathFor(card);
				LoadingImageSource = null;
				if (hasAsset)
					ExpandAnimation?.Begin();
			}
			catch(ArgumentNullException)
			{
			}
		}
	}
}
