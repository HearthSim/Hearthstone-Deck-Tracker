using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HearthMirror.Objects;
using HearthWatcher.EventArgs;
using HearthWatcher.Providers;

namespace HearthWatcher
{
	public class PackOpeningWatcher : PollingWatcher
	{
		public delegate void PackEventHandler(object sender, PackEventArgs args);

		// deliberately persists across runs to not re-report the last pack
		private readonly List<Card> _previousPack = new List<Card>();
		private bool _invokeEvent;
		private readonly IPackProvider _packProvider;

		public PackOpeningWatcher(IPackProvider packProvider, int delay = 500) : base(delay)
		{
			if(packProvider == null)
				throw new ArgumentNullException(nameof(packProvider));
			_packProvider = packProvider;
		}

		public event PackEventHandler? NewPackEventHandler;

		protected override Task<bool> TickAsync()
		{
			var cards = _packProvider.GetCards();
			if(cards?.Count == 5)
			{
				if(cards.All(x => _previousPack.Any(c => c.Id == x.Id & c.PremiumType == x.PremiumType)))
					return Task.FromResult(false);
				if(_previousPack.Any())
					_invokeEvent = true;
				_previousPack.Clear();
				_previousPack.AddRange(cards);
				if(_invokeEvent)
				{
					var packId = _packProvider.GetPackId();
					Dispatch(() => NewPackEventHandler?.Invoke(this, new PackEventArgs(cards, packId)));
				}
			}
			else
				_invokeEvent = true;
			return Task.FromResult(false);
		}
	}
}
