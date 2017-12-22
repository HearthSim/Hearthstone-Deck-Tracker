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
		private bool _invokeEvent;
		private readonly IPackProvider _packProvider;

		public PackOpeningWatcher(IPackProvider packProvider, int delay = 500)
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
				if(cards?.Count == 5)
				{
					if(cards.All(x => _previousPack.Any(c => c.Id == x.Id & c.Premium == x.Premium)))
						continue;
					if(_previousPack.Any())
						_invokeEvent = true;
					_previousPack.Clear();
					_previousPack.AddRange(cards);
					if(_invokeEvent)
						NewPackEventHandler?.Invoke(this, new PackEventArgs(cards, _packProvider.GetPackId()));
				}
				else
					_invokeEvent = true;
			}
			_running = false;
		}
	}
}
