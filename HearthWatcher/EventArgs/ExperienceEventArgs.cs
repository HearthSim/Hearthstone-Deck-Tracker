using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ExperienceEventArgs : System.EventArgs
	{
		public ExperienceEventArgs(int experience, int experienceNeeded, int level, int levelChange)
		{
			Experience = experience;
			ExperienceNeeded = experienceNeeded;
			Level = level;
			LevelChange = levelChange;
		}

		public int Experience;
		public int ExperienceNeeded;
		public int Level;
		public int LevelChange;
		public bool IsChanged;

	}
}
