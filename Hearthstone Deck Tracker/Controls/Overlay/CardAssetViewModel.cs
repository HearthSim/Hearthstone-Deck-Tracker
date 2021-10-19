using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Controls.Overlay
{
	public class CardAssetViewModel : ViewModel
	{
		private Hearthstone.Card _card { get; set; }
		private AssetDownloader<Hearthstone.Card> _assetDownloader;

		public CardAssetViewModel(Hearthstone.Card card, CardAssetType type)
		{
			_card = card;
			_assetDownloader = AssetDownloaders.GetCardAssetDownloader(type);
			_path = _assetDownloader.PlaceholderAssetPath;
		}

		private string _path;
		public string AssetPath
		{
			get
			{
				if(_path == _assetDownloader.PlaceholderAssetPath && !_loading)
					LoadImage().Forget();
				return _path;
			}
			private set
			{
				if(value != _path)
				{
					_path = value;
					OnPropertyChanged();
				}
			}
		}

		private bool _loading = false;
		private async Task LoadImage()
		{
			var card = _card;
			if(card == null)
			{
				_path = _assetDownloader.PlaceholderAssetPath;
				return;
			}
			if(_loading)
				return;
			_loading = true;
			try
			{
				if(!_assetDownloader.HasAsset(card))
					await _assetDownloader.DownloadAsset(card);
				if(card == _card)
					AssetPath = _assetDownloader.StoragePathFor(card);
			}
			catch
			{
				AssetPath = _assetDownloader.PlaceholderAssetPath;
			}
			finally
			{
				_loading = false;
			}
		}

		public async Task SetCard(Hearthstone.Card card)
		{
			_card = card;
			await LoadImage();
		}
	}
}
