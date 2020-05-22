#region

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Hearthstone_Deck_Tracker.Annotations;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Attributes;
using static System.Windows.Visibility;

#endregion

namespace Hearthstone_Deck_Tracker.Controls
{
	public partial class CardMarker : INotifyPropertyChanged
	{
		private static readonly Int32Rect CropRect = new Int32Rect() { Height = 34, Width = 34, X = 55, Y = 0 };

		private int _cardAge;
		private Visibility _cardAgeVisibility;
		private int _costReduction;
		private Visibility _costReductionVisibility;
		private BitmapImage _icon;
		private BitmapSource _sourceCardBitmap;
		private ScaleTransform _scaleTransform;

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

		public BitmapImage Icon
		{
			get => _icon;
			set
			{
				_icon = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(IconVisibility));
			}
		}

		public BitmapSource SourceCardBitmap
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

		public Hearthstone.Card SourceCard { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void UpdateIcon(CardMark mark)
		{
			if(Helper.TryGetAttribute<AssetNameAttribute>(mark, out var assetName) && assetName.Name != null)
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

		public void UpdateSourceCard(Hearthstone.Card card)
		{
			if(SourceCard == card)
				return;
			SourceCard = card;
			var cardTile = card != null ? ImageCache.GetCardImage(card) : null;
			SourceCardBitmap = cardTile != null ? new CroppedBitmap(cardTile, CropRect) : null;
		}

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
