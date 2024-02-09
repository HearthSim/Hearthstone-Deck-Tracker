namespace HearthWatcher.EventArgs
{
	public class SceneEventArgs : System.EventArgs
	{
		public int PrevMode { get; }
		public int Mode { get; }
		public bool SceneLoaded { get; }
		public bool Transitioning { get; }

		public SceneEventArgs(int prevMode, int mode, bool sceneLoaded, bool transitioning)
		{
			PrevMode = prevMode;
			Mode = mode;
			SceneLoaded = sceneLoaded;
			Transitioning = transitioning;
		}

		public override bool Equals(object obj) => obj is SceneEventArgs args
			&& args.PrevMode == PrevMode
			&& args.Mode == Mode
			&& args.SceneLoaded == SceneLoaded
			&& args.Transitioning == Transitioning;

		public override int GetHashCode()
		{
			var hashCode = -2012095321;
			hashCode = hashCode * -1521134295 + PrevMode.GetHashCode();
			hashCode = hashCode * -1521134295 + Mode.GetHashCode();
			hashCode = hashCode * -1521134295 + SceneLoaded.GetHashCode();
			hashCode = hashCode * -1521134295 + Transitioning.GetHashCode();
			return hashCode;
		}
	}
}
