using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Plugins
{
	public interface IPlugin
	{
		string Name { get; }
		string Description { get; }
		string ButtonText { get; }
		string Author { get; }
		Version Version { get; }
		void OnLoad();
		void OnUnload();
		void OnButtonPress();
		void OnUpdate();
	}
}
