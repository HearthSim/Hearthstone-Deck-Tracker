namespace Hearthstone_Deck_Tracker.Plugins
{
	public class PluginSettings
	{
		public string FileName;
		public bool IsEnabled;
		public string Name;

		public PluginSettings()
		{
		}

		internal PluginSettings(PluginWrapper p)
		{
			FileName = p.FileName;
			Name = p.Name;
			IsEnabled = p.IsEnabled;
		}
	}
}