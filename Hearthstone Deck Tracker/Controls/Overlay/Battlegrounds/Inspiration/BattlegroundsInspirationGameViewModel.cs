using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility.Assets;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.Inspiration;

public class BattlegroundsInspirationGameViewModel : ViewModel
{
	public BattlegroundsInspirationGameViewModel(InspirationApiResponse.ResponseData.Game game)
	{
		var hero = Database.GetBattlegroundsHeroFromDbf(game.HeroDbfId);
		if(hero != null)
			AssetDownloaders.heroImageDownloader?.GetAssetData(hero).ContinueWith(async x => HeroImage = await x);


		var heroPower = Database.GetCardFromDbfId(game.HeroPower, false);
		if(heroPower != null)
		{
			heroPower.BaconCard = true;
			HeroPower = new HeroPowerViewModel
			{
				Card = heroPower,
				IsCoinCost = true,
				Cost = heroPower.HideCost ? null : heroPower.Cost,
			};
		}

		Board = game.FinalMinions.Where(x => x.ZonePosition is long).Select(x =>
		{
			var minion = Database.GetCardFromDbfId(x.MinionDbfId, false);
			if(minion == null)
				return null;
			minion.BaconCard = true;
			minion.BaconTriple = x.Premium;
			return new BattlegroundsMinionViewModel
			{
				HasVenomous = x.Venomous,
				HasDivineShield = x.DivineShield,
				HasReborn = x.Reborn,
				IsPremium = x.Premium,
				HasTaunt = x.Taunt,
				HasPoisonous = x.Poisonous,
				// HasWindfury = m.Windfury, -- Not supported by BattlegroundsMinion
				HasDeathrattle = x.Deathrattle,
				Attack = x.Attack,
				Health = x.Health,
				Card = minion,
				ShowTripleTooltip = false,
			};
		}).WhereNotNull();
	}

	public ImageSource? HeroImage
	{
		get => GetProp<ImageSource?>(null);
		private set => SetProp(value);
	}

	public HeroPowerViewModel HeroPower { get; } = new();

	// Currently unused
	public IEnumerable<TrinketViewModel> Trinkets { get; } = new TrinketViewModel[] { };

	public IEnumerable<BattlegroundsMinionViewModel> Board { get; }
}
