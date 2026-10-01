using HearthMirror.Enums;

namespace HearthWatcher.EventArgs
{
	public class SceneEventArgs : System.EventArgs
	{
		public int PrevMode { get; }
		public int Mode { get; }
		public bool SceneLoaded { get; }
		public bool Transitioning { get; }
		public LoadingScreenPhase LoadingScreenPhase { get; }

		public SceneEventArgs(int prevMode, int mode, bool sceneLoaded, bool transitioning, LoadingScreenPhase loadingScreenPhase)
		{
			PrevMode = prevMode;
			Mode = mode;
			SceneLoaded = sceneLoaded;
			Transitioning = transitioning;
			LoadingScreenPhase = loadingScreenPhase;
		}

		public override bool Equals(object obj) => obj is SceneEventArgs args
			&& args.PrevMode == PrevMode
			&& args.Mode == Mode
			&& args.SceneLoaded == SceneLoaded
			&& args.Transitioning == Transitioning
			&& args.LoadingScreenPhase == LoadingScreenPhase;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + PrevMode.GetHashCode();
			hashCode = hashCode * -1521134295 + Mode.GetHashCode();
			hashCode = hashCode * -1521134295 + SceneLoaded.GetHashCode();
			hashCode = hashCode * -1521134295 + Transitioning.GetHashCode();
			hashCode = hashCode * -1521134295 + LoadingScreenPhase.GetHashCode();
			return hashCode;
		}
	}
}
