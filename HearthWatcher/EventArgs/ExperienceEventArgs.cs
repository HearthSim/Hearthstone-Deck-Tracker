using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ExperienceEventArgs : System.EventArgs
	{
		public ExperienceEventArgs(int experience, int experienceNeeded, int level, int levelChange, bool animate)
		{
			Experience = experience;
			ExperienceNeeded = experienceNeeded;
			Level = level;
			LevelChange = levelChange;
			Animate = animate;
		}

		public int Experience;
		public int ExperienceNeeded;
		public int Level;
		public int LevelChange;
		public bool IsChanged;
		public bool Animate;

	}
}
