using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class PackOpeningWatcher
	{
		public delegate void PackEventHandler(object sender, PackEventArgs args);

		private readonly List<Card> _previousPack = new List<Card>();
		private readonly int _delay;
		private bool _running;
		private bool _watch;
		private readonly IPackProvider _packProvider;

		public PackOpeningWatcher(IPackProvider packProvider, int delay = 1000)
		{
			if(packProvider == null)
				throw new ArgumentNullException(nameof(packProvider));
			_packProvider = packProvider;
			_delay = delay;
		}
		public event PackEventHandler NewPackEventHandler;

		public void Run()
		{
			_watch = true;
			if(!_running)
				CheckForPacks();
		}

		public void Stop() => _watch = false;

		private async void CheckForPacks()
		{
			_running = true;
			while(_watch)
			{
				await Task.Delay(_delay);
				if(!_watch)
					break;
				var cards = _packProvider.GetCards();
				if(cards?.Any() ?? false)
				{
					if(cards.All(x => _previousPack.Any(c => c.Id == x.Id & c.Premium == x.Premium)))
						continue;
					_previousPack.Clear();
					_previousPack.AddRange(cards);
					NewPackEventHandler?.Invoke(this, new PackEventArgs(cards, _packProvider.GetPackId()));
				}
			}
			_running = false;
		}
	}
}
