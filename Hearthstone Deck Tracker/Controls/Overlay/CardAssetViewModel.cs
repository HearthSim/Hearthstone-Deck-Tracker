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

			var cached = card != null ? _assetDownloader?.TryGetAssetData(card) : null;
			if(cached != null)
			{
				_resolved = true;
				Asset = cached;
			}
			else
				Asset = Placeholder(card, type);
		}

		private ImageSource? Placeholder(Hearthstone.Card? card, CardAssetType type)
		{
			if(card != null && type == CardAssetType.FullImage)
			{
				return Application.Current.TryFindResource(card.TypeEnum switch
				{
					CardType.HERO => "LoadingHero",
					CardType.MINION => "LoadingMinion",
					CardType.WEAPON => "LoadingWeapon",
					_ => "LoadingSpell",
				}) as ImageSource;
			}
			if(card != null && type == CardAssetType.Hero)
				return Application.Current.TryFindResource("LoadingHeroFrame") as ImageSource;
			return _assetDownloader?.PlaceholderAsset;
		}

		public ImageSource? Asset
		{
			get
			{
				if(Card == null)
					return null;
				// Asset holds a placeholder until the real asset resolves, so gate loading on
				// _resolved rather than on the stored value. This keeps retrying on a transient
				// failure instead of leaving the placeholder up permanently.
				if(!_resolved)
					LoadAsset().Forget();
				return GetProp<ImageSource?>(null);
			}
			private set => SetProp(value);
		}

		private bool _resolved;
		private bool _loading;
		private async Task LoadAsset()
		{
			if(_loading || _resolved || Card == null || _assetDownloader == null)
				return;
			_loading = true;
			try
			{
				var asset = await _assetDownloader.GetAssetData(Card);
				if(asset != null)
				{
					_resolved = true;
					Asset = asset;
				}
			}
			finally
			{
				_loading = false;
			}
		}
	}
}
