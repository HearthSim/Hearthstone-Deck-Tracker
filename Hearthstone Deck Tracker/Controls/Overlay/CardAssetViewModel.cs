using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public class CardAssetViewModel : ViewModel
	{
		private Hearthstone.Card? _card { get; set; }
		private readonly AssetDownloader<Hearthstone.Card, BitmapImage>? _assetDownloader;

		public CardAssetViewModel(Hearthstone.Card? card, CardAssetType type)
		{
			_card = card;
			_assetDownloader = AssetDownloaders.GetCardAssetDownloader(type);
			_asset = _assetDownloader?.PlaceholderAsset;
		}

		private BitmapImage? _asset;
		public BitmapImage? Asset
		{
			get
			{
				if(_asset == _assetDownloader?.PlaceholderAsset && !_loading)
					LoadImage().Forget();
				return _asset;
			}
			private set
			{
				if(value != _asset)
				{
					_asset = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _loading;
		private async Task LoadImage()
		{
			if(_assetDownloader == null)
				return;
			var card = _card;
			if(card == null)
			{
				_asset = _assetDownloader?.PlaceholderAsset;
				return;
			}
			if(_loading)
				return;
			_loading = true;
			var asset = await _assetDownloader.GetAssetData(card);
			if(asset != null)
				Asset = asset;
			_loading = false;
		}

		public async Task SetCard(Hearthstone.Card card)
		{
			_card = card;
			await LoadImage();
		}
	}
}
