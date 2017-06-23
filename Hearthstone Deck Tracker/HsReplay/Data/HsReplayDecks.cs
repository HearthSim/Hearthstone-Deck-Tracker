using System;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.HsReplay.Data
{

	public class HsReplayDecks : JsonCache<DecksData>
	{
		private DecksData _data;
		private bool _loading;

		public string[] AvailableDecks
		{
			get
			{
				if(!(_data?.IsStale ?? true) || _loading)
					return _data?.Decks ?? new string[0];
				Load();
				return new string[0];
			}
		}

		public event Action OnLoaded;

		public HsReplayDecks() : base("hsreplay_decks.cache")
		{
			Load();
		}

		private async void Load()
		{
			_loading = true;
			var data = await LoadFromDisk();
			Log.Info($"Loaded from disk: {data}");
			if(data?.IsStale ?? true)
			{
				Log.Info("Cached data was not found or stale. Fetching latest...");
				data = await ApiWrapper.GetAvailableDecks();
				if(data == null)
				{
					Log.Warn("No data. Can retry in 30 minutes.");
					data = new DecksData
					{
						ClientTimeStamp = DateTime.Now.Subtract(TimeSpan.FromHours(23.5)),
					};
				}
				Log.Info("Writing hsreplay_decks.cache to disk...");
				await WriteToDisk(data);
			}
			_data = data;
			Log.Info($"Complete: {data}");
			OnLoaded?.Invoke();
			_loading = false;
		}
	}
}
