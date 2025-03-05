using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HearthDb.Enums;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public class CardAssetViewModel : ViewModel
	{
		public Hearthstone.Card? Card { get; init; }
		public CardAssetType CardAssetType { get; }
		private readonly AssetDownloader<Hearthstone.Card, BitmapImage>? _assetDownloader;

		public CardAssetViewModel(Hearthstone.Card? card, CardAssetType type)
		{
			Card = card;
			CardAssetType = type;
			_assetDownloader = AssetDownloaders.GetCardAssetDownloader(type);
			if(card != null)
				Asset = _assetDownloader?.TryGetAssetData(card);

			if(Asset == null)
			{
				if(card != null && type == CardAssetType.FullImage)
				{
					Asset = Application.Current.TryFindResource(card.TypeEnum switch
					{
						CardType.HERO => "LoadingHero",
						CardType.MINION => "LoadingMinion",
						CardType.WEAPON => "LoadingWeapon",
						_ => "LoadingSpell",
					}) as ImageSource;
				}
				else if(card != null && type == CardAssetType.Hero)
					Asset = Application.Current.TryFindResource("LoadingHeroFrame") as ImageSource;
				else
					Asset = _assetDownloader?.PlaceholderAsset;
			}
		}

		public ImageSource? Asset
		{
			get
			{
				if(Card == null)
					return null;
				var value =  GetProp<ImageSource?>(null);
				if(value == null)
					LoadAsset().Forget();
				return value;
			}
			private set => SetProp(value);
		}

		private bool _loading;
		private async Task LoadAsset()
		{
			if(_loading || Card == null || _assetDownloader == null)
				return;
			_loading = true;
			var asset = await _assetDownloader.GetAssetData(Card);
			if(asset != null)
				Asset = asset;
			_loading = false;
		}
	}
}
