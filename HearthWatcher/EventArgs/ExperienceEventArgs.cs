using System.Collections.Generic;
using HearthMirror.Objects;

namespace HearthWatcher.EventArgs
{
	public class ExperienceEventArgs : System.EventArgs
	{
		public ExperienceEventArgs(int experience, int experienceNeeded, int level)
		{
			Experience = experience;
			ExperienceNeeded = experienceNeeded;
			Level = level;
		}

		public int Experience;
		public int ExperienceNeeded;
		public int Level;
		public bool IsChanged;

	}
}
