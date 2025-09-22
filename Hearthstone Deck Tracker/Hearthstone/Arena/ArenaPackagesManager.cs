using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.HsReplay;
using Hearthstone_Deck_Tracker.Utility.Logging;
using NuGet;

namespace Hearthstone_Deck_Tracker.Hearthstone.Arena;

public class ArenaPackagesManager
{
	private Dictionary<string, string[]> _packagesByKeyCard = new();
	private  Dictionary<string, string> _packagesKeyByPackageOnlyCard = new();

	public async Task UpdatePackages()
	{
		try
		{
			var data = await ApiWrapper.GetArenaPackages();
			if(data == null)
			{
				_packagesByKeyCard.Clear();
				_packagesKeyByPackageOnlyCard.Clear();
				return;
			}

			_packagesByKeyCard = data.Data.PackagesByKeyCard;
			_packagesKeyByPackageOnlyCard = data.Data.PackagesKeyByPackageOnlyCard;
		}
		catch(Exception e)
		{
			Log.Error(e);
		}
	}

	public (Card?, IEnumerable<Card>) GetOpponentsPackageCards(List<Card> opponentsDecklist)
	{
		if(_packagesByKeyCard.IsEmpty() || _packagesKeyByPackageOnlyCard.IsEmpty())
		{
			UpdateMetrics(false);
			return (null, new Card[] { });
		}

		foreach(var card in opponentsDecklist.Where(c => !c.IsCreated))
		{
			if(!_packagesKeyByPackageOnlyCard.TryGetValue(card.Id, out var packageKey)) continue;
			if(!_packagesByKeyCard.TryGetValue(packageKey, out var package))
			{
				Log.Error($"Missing Package for {packageKey}");
				continue;
			}

			UpdateMetrics(true);
			var keyCard = new Card(packageKey);
			return (
				keyCard, new [] { keyCard }.Concat(package.Select(id => new Card(id)))
			);
		}

		UpdateMetrics(false);
		return (null, new Card[] { });
	}

	private static void UpdateMetrics(bool hasPackage)
	{
		Core.Game.Metrics.ArenaShowedOpponentPackage = hasPackage && !Config.Instance.HideOpponentArenaPackages;
	}

}
