using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IPackProvider
	{
		List<Card> GetCards();
		int GetPackId();
	}
}
