using HearthMirror.Objects;

namespace HearthWatcher.Providers
{
	public interface ISceneProvider
	{
		SceneMgrState? State { get; }
	}
}
