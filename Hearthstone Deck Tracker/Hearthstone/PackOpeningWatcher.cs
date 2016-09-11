using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror;
using Hearthstone_Deck_Tracker.Utility.Logging;

namespace Hearthstone_Deck_Tracker.Hearthstone
{
	public class PackOpeningWatcher
	{
		public delegate void PackEventHandler(object sender, PackEventArgs args);

		private readonly List<HearthMirror.Objects.Card> _previousPack = new List<HearthMirror.Objects.Card>();
		private bool _running;
		private bool _watch;

		private PackOpeningWatcher()
		{
		}

		public static PackOpeningWatcher Instance { get; } = new PackOpeningWatcher();
		public event PackEventHandler NewPackEventHandler;

		internal void Run()
		{
			_watch = true;
			if(!_running)
				CheckForPacks();
		}

		internal void Stop() => _watch = false;

		private async void CheckForPacks()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(1000);
				if(!_watch)
					break;
				var cards = Reflection.GetPackCards();
				if(cards?.Any() ?? false)
				{
					if(cards.All(x => _previousPack.Any(c => c.Id == x.Id & c.Premium == x.Premium)))
						continue;
					Log.Info("Found new pack: " + string.Join(", ", cards.Select(x => $"{x.Id}{(x.Premium ? " (golden)" : "")}")));
					_previousPack.Clear();
					_previousPack.AddRange(cards);
					NewPackEventHandler?.Invoke(this, new PackEventArgs(cards));
				}
			}
			_running = false;
		}
	}

	public class PackEventArgs : EventArgs
	{
		public PackEventArgs(List<HearthMirror.Objects.Card> cards)
		{
			Cards = cards;
		}

		public List<HearthMirror.Objects.Card> Cards { get; }
	}
}