#region

using System;
using System.Windows.Controls;

#endregion

namespace Hearthstone_Deck_Tracker.Plugins
{
	public interface IPlugin
	{
		/// <summary>
		/// Name of the Plugin
		/// </summary>
		/// <returns></returns>
		string Name { get; }

		/// <summary>
		/// Description of the Plugin
		/// </summary>
		/// <returns></returns>
		string Description { get; }

		/// <summary>
		/// Text displayed on the button in "options > tracker > plugins"
		/// </summary>
		/// <returns></returns>
		string ButtonText { get; }

		/// <summary>
		/// You
		/// </summary>
		/// <returns></returns>
		string Author { get; }

		/// <summary>
		/// Version of the Plugin
		/// </summary>
		/// <returns></returns>
		Version Version { get; }

		/// <summary>
		/// MenuItem added to the "Plugins" main menu. Return null to not add one.
		/// </summary>
		/// <returns></returns>
		MenuItem MenuItem { get; }

		/// <summary>
		/// Called when the Plugin is loaded (enabled) by HDT
		/// </summary>
		void OnLoad();

		/// <summary>
		/// Called when the Plugin is unloaded (disabled) by HDT
		/// </summary>
		void OnUnload();

		/// <summary>
		/// Called when the button in "options > tracker > plugins" is pressed.
		/// </summary>
		void OnButtonPress();

		/// <summary>
		/// Called every ~100ms
		/// </summary>
		void OnUpdate();
	}
}
