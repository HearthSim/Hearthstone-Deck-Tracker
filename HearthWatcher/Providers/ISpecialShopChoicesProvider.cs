using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface ISpecialShopChoicesProvider
	{
		SpecialShopChoicesState? SpecialShopChoicesState { get; }
	}
}
