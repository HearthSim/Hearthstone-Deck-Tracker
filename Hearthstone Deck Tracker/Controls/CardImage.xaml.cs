using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class CardImage : INotifyPropertyChanged
	{
		private string? _cardId;
		public string? CardId
		{
			get => _cardId;
			set
			{
				_cardId = value;
				OnPropertyChanged();
			}
		}

		private ImageSource? _loadingImageSource = null;
		public ImageSource? LoadingImageSource
		{
			get => _loadingImageSource;
			set
			{
				_loadingImageSource = value;
				OnPropertyChanged();
			}
		}

		private BitmapImage? _cardAsset = null;
		public BitmapImage? CardAsset
		{
			get => _cardAsset;
			set
			{
				_cardAsset = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(QuestionmarkVisibility));
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

		public double IconScaling => Math.Min(1, ActualHeight / 500);

		public Visibility QuestionmarkVisibility => CardAsset == null || !ShowQuestionmark ? Visibility.Collapsed : Visibility.Visible;

		private bool _showQuestionmark = false;
		public bool ShowQuestionmark
		{
			get => _showQuestionmark;
			set
			{
				_showQuestionmark = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(QuestionmarkVisibility));
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

		public event PropertyChangedEventHandler? PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public CardImage()
		{
			InitializeComponent();
		}

		private ImageSource? GetLoadingImagePath(Hearthstone.Card card)
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

		Storyboard? ExpandAnimation => FindResource("StoryboardExpand") as Storyboard;

		public async void SetCardIdFromCard(Hearthstone.Card? card)
		{
			var newCardId = card?.Id;
			if(newCardId == CardId)
				return;
			CardId = newCardId;
			if(card == null || string.IsNullOrEmpty(newCardId))
			{
				CardAsset = null;
				LoadingImageSource = null;
				return;
			}
			var downloader = AssetDownloaders.cardImageDownloader;
			if(downloader == null)
				return;

			var asset = downloader.TryGetAssetData(card);
			if(asset == null)
			{
				CardAsset = null;
				LoadingImageSource = GetLoadingImagePath(card);
				ExpandAnimation?.Begin();
				asset = await downloader.GetAssetData(card);
			}

			if(newCardId != CardId)
				return;
			CardAsset = asset;
			if (LoadingImageSource == null)
				ExpandAnimation?.Begin();
			LoadingImageSource = null;
		}

		private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			OnPropertyChanged(nameof(IconScaling));
		}
	}
}
