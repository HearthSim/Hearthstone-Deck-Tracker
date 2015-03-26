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
		Version Version { get; }
		void Load();
		void Unload();
		void OnButtonPress();
		void OnUpdate();
	}
}
