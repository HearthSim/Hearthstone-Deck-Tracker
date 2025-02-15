#region

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Controls.Tooltips;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Attributes;
using static System.Windows.Visibility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class CardMarker : INotifyPropertyChanged, ICardTooltip
	{
		private static readonly Int32Rect CropRect = new() { Height = 59, Width = 59, X = 126, Y = 0 };

		private int _cardAge;
		private Visibility _cardAgeVisibility;
		private int _costReduction;
		private Visibility _costReductionVisibility;
		private BitmapImage? _icon;
		private BitmapSource? _sourceCardBitmap;
		private ScaleTransform _scaleTransform;
		private Hearthstone.Card? _sourceCard;
		private SourceType? _cardSourceType;

		public static readonly DependencyProperty ZonePositionProperty = DependencyProperty.Register(nameof(ZonePosition), typeof(int), typeof(CardMarker), new PropertyMetadata(0));

		public CardMarker()
		{
			InitializeComponent();
			_scaleTransform = new ScaleTransform(1, 1);
		}

		public int CardAge
		{
			get => _cardAge;
			set
			{
				_cardAge = value;
				OnPropertyChanged();
			}
		}

		public Visibility CardAgeVisibility
		{
			get => _cardAgeVisibility;
			set
			{
				_cardAgeVisibility = value;
				OnPropertyChanged();
			}
		}

		public BitmapImage? Icon
		{
			get => _icon;
			set
			{
				_icon = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IconVisibility));
			}
		}

		public BitmapSource? SourceCardBitmap
		{
			get => _sourceCardBitmap;
			set
			{
				_sourceCardBitmap = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(SourceCardVisibility));
				OnPropertyChanged(nameof(IconVisibility));
			}
		}

		public Visibility IconVisibility => Icon != null && SourceCard == null ? Visible : Collapsed;

		public Visibility CostReductionVisibility
		{
			get => _costReductionVisibility;
			set
			{
				_costReductionVisibility = value;
				OnPropertyChanged();
			}
		}

		public Visibility SourceCardVisibility => SourceCardBitmap == null ? Collapsed : Visible;

		public int CostReduction
		{
			get => _costReduction;
			set
			{
				_costReduction = value;
				OnPropertyChanged();
			}
		}

		public ScaleTransform ScaleTransform
		{
			get => _scaleTransform;
			set
			{
				_scaleTransform = value;
				OnPropertyChanged();
			}
		}

		public Hearthstone.Card? SourceCard
		{
			get => _sourceCard;
			private set
			{
				if(Equals(value, _sourceCard)) return;
				_sourceCard = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IconVisibility));
				OnPropertyChanged(nameof(TooltipText));
			}
		}

		public SourceType? CardSourceType
		{
			get => _cardSourceType;
			private set
			{
				if(value == _cardSourceType) return;
				_cardSourceType = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(TooltipText));
			}
		}

		public string? TooltipText => CardSourceType switch
		{
			SourceType.CreatedBy => $"Created by {SourceCard?.Name}",
			SourceType.DrawnBy => $"Drawn by {SourceCard?.Name}",
			_ => null,
		};

		public int ZonePosition
		{
			get => (int)GetValue(ZonePositionProperty);
			set => SetValue(ZonePositionProperty, value);
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void UpdateIcon(CardMark mark)
		{
			if(Helper.TryGetAttribute<AssetNameAttribute>(mark, out var assetName) && assetName?.Name != null)
			{
				var path = Path.Combine("/HearthstoneDeckTracker;component", assetName.Name);
				Icon = new BitmapImage(new Uri(path, UriKind.Relative));
			}
			else
				Icon = null;
		}

		public void UpdateCardAge(int? cardAge)
		{
			if(cardAge.HasValue)
			{
				CardAge = cardAge.Value;
				CardAgeVisibility = Visible;
			}
			else
				CardAgeVisibility = Collapsed;
		}

		public void UpdateCostReduction(int costReduction)
		{
			CostReduction = -costReduction;
			CostReductionVisibility = costReduction > 0 ? Visible : Collapsed;
		}

		public async void UpdateSource(Hearthstone.Card? card, SourceType? sourceType)
		{
			CardSourceType = sourceType;
			if(SourceCard == card)
				return;
			SourceCard = card;
			if(card == null || AssetDownloaders.cardTileDownloader == null)
			{
				SourceCardBitmap = null;
				return;
			}

			var bmp = await AssetDownloaders.cardTileDownloader.GetAssetData(card);
			SourceCardBitmap = bmp != null ? new CroppedBitmap(bmp, CropRect) : null;
		}

		public enum SourceType
		{
			Known,
			DrawnBy,
			CreatedBy,
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void UpdateTooltip(CardTooltipViewModel viewModel)
		{
			viewModel.Card = SourceCard;
			viewModel.Text = TooltipText;
		}
	}
}
