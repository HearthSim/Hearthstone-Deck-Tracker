using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface IChoicesProvider
	{
		CardChoices? CurrentChoice { get; }
	}
}
