using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Utility.Assets;

namespace Hearthstone_Deck_Tracker.Controls
{
    public partial class GridCardImages : INotifyPropertyChanged
    {
        public class CardWithImage : INotifyPropertyChanged
        {
	        public Hearthstone.Card? Card { set; get; }

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

            private string? _cardImagePath = null;

            public string? CardImagePath
            {
	            get => _cardImagePath;
	            set
	            {
		            _cardImagePath = value;
		            OnPropertyChanged();
	            }
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            [NotifyPropertyChangedInvocator]
            private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            {
	            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ObservableCollection<CardWithImage> _cards = new ObservableCollection<CardWithImage>();
        public ObservableCollection<CardWithImage> Cards
        {
            get => _cards;
            set
            {
                _cards = value;
                OnPropertyChanged();
            }
        }

        public CornerRadius TitleCornerRadius  => Cards.Count > 0 ?
	        new CornerRadius(10, 10, 0,0) :
	        new CornerRadius(10);

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
	        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public GridCardImages()
        {
            InitializeComponent();
            DataContext = this;
        }

        private ImageSource? GetLoadingImagePath(Hearthstone.Card card)
        {
            switch (card?.Type)
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
        private IEnumerable<Hearthstone.Card>? _previousCards;
        Storyboard? ExpandAnimation => FindResource("AnimateGrid") as Storyboard;
        public async void SetCardIdsFromCards(IEnumerable<Hearthstone.Card>? cards, int? maxGridHeight = null)
        {
	        if(cards == null || (_previousCards != null && _previousCards.SequenceEqual(cards)))
	        {
		        if((maxGridHeight.HasValue && maxGridHeight != _maxGridHeight)
		           || (!maxGridHeight.HasValue && GridHeight != _maxGridHeight))
		        {
			        _maxGridHeight = maxGridHeight ?? GridHeight;
			        CardsCollectionChanged(_maxGridHeight);
			        ExpandAnimation?.Begin();
		        }

		        return;
	        }

	        _previousCards = cards;

            Cards.Clear();

            var downloader = AssetDownloaders.cardImageDownloader;
            if (downloader == null)
                return;

            foreach(var card in cards)
            {
	            var cardWithImage = new CardWithImage
	            {
		            Card = card,
		            LoadingImageSource = GetLoadingImagePath(card)
	            };

	            Cards.Add(cardWithImage);
            }

            CardsCollectionChanged(maxGridHeight);

            foreach (var cardWithImage in Cards.ToList())
            {
	            if(cardWithImage.Card == null) continue;

                if (!downloader.HasAsset(cardWithImage.Card))
                {
                    try
                    {
                        await downloader.DownloadAsset(cardWithImage.Card);
                    }
                    catch (ArgumentNullException)
                    {
                        continue;
                    }
                }

                try
                {
                    cardWithImage.CardImagePath = downloader.StoragePathFor(cardWithImage.Card);
                    cardWithImage.LoadingImageSource = null;
                }
                catch (ArgumentNullException)
                {
                    // Handle exception if needed
                }

            }

            OnPropertyChanged(nameof(CardsGridVisibility));
            OnPropertyChanged(nameof(TitleCornerRadius));
	        ExpandAnimation?.Begin();
        }

        private void CardsCollectionChanged(int? maxGridHeight = null)
        {
	        var cardCount = Cards.Count;
	        if (cardCount == 0)
		        return;

	        var columns = Math.Min(cardCount, MaxColumns);
	        var rows = (int)Math.Ceiling((double)cardCount / columns);

	        var cardRatio = CardAspectRatio;
	        var cardWidth = GridWidth / columns;
	        var cardHeight = GridHeight / rows;

	        if (cardWidth / cardHeight > cardRatio)
		        cardWidth = (int)(cardHeight * cardRatio);
	        else
		        cardHeight = (int)(cardWidth / cardRatio);

	        if (maxGridHeight.HasValue && cardHeight * rows > maxGridHeight.Value)
	        {
		        var scaleFactor = (double)maxGridHeight.Value / (cardHeight * rows);
		        cardWidth = (int)(cardWidth * scaleFactor);
		        cardHeight = (int)(cardHeight * scaleFactor);
	        }

	        CardWidth = Math.Min(cardWidth, (int)MaxCardWidth);
	        CardHeight = Math.Min(cardHeight, (int)MaxCardHeight);
        }

        public void SetTitle(string title)
        {
	        Title = title;
        }

        private const int MaxColumns = 3;
        public static int GridWidth { get; } = 600;
        public static int GridHeight { get; } = 750;

        private const double MaxCardWidth = 256 * 0.7;
        private const double MaxCardHeight = 388 * 0.7;

        private int _maxGridHeight = GridHeight;
        public Thickness CardMargin => CalculateCardMargin();

        private Thickness CalculateCardMargin()
        {
	        var scaleFactor = CardHeight / BaseCardHeight;
	        var topBottomMargin = -13 * scaleFactor;
	        var leftRightMargin = -2 * scaleFactor;
	        return new Thickness(leftRightMargin, topBottomMargin, leftRightMargin, topBottomMargin);
        }

        private const double CardAspectRatio = MaxCardWidth / MaxCardHeight;
        private int _cardWidth = 128;
        public int CardWidth
        {
	        get => _cardWidth;
	        set
	        {
		        _cardWidth = value;
		        OnPropertyChanged();
	        }
        }

        public Visibility CardsGridVisibility => Cards.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        private const double BaseCardHeight = 194.0;

        private int _cardHeight = (int)BaseCardHeight;
        public int CardHeight
        {
	        get => _cardHeight;
	        set
	        {
		        _cardHeight = value;
		        OnPropertyChanged();
		        OnPropertyChanged(nameof(CardMargin));
	        }
        }

        private string? _title;

        public string? Title
        {
	        get => _title;
	        set
	        {
		        _title = value;
		        OnPropertyChanged();
	        }
        }
    }
}
