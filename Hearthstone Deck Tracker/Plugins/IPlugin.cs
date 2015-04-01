#region

using System;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	public interface IPlugin
	{
		string Name { get; }
		string Description { get; }
		string ButtonText { get; }
		string Author { get; }
		Version Version { get; }
		MenuItem MenuItem { get; }
		void OnLoad();
		void OnUnload();
		void OnButtonPress();
		void OnUpdate();
	}
}