using System;
using System.Collections.Generic;
using System.Linq;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Utility.Logging;
using HSReplay;

namespace Hearthstone_Deck_Tracker.HsReplay
{
	internal class PackUploader
	{
		internal static async void UploadPack(int packId, List<Card> cards)
		{
			Log.Info($"New Pack! Id={packId}, Cards=[{string.Join(", ", cards.Select(x => x.Id + (x.Premium ? " (g)" : "")))}]");
			if(Config.Instance.HsReplayUploadPacks == true)
			{
				try
				{
					var packData = PackDataGenerator.Generate(packId, cards.Select(x => new CardData { CardId = x.Id, Premium = x.Premium }));
					await ApiWrapper.UploadPack(packData);
					Log.Info("Successfully uploaded pack");
				}
				catch(Exception ex)
				{
					Log.Error(ex);
				}
			}
		}
	}
}
