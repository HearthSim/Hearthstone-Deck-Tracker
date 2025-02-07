using System;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Windows;

public static class WindowExtensions
{
	/// <summary>
	/// Get the parent window of the specified type
	/// </summary>
	/// <param name="element">Element in the window</param>
	/// <typeparam name="T">Type of the window</typeparam>
	/// <returns>Return the window if it matches type T, otherwise null.</returns>
	public static T? ParentWindow<T>(this FrameworkElement element) where T : Window => Window.GetWindow(element) as T;

	/// <summary>
	/// Get the parent MainWindow
	/// </summary>
	/// <param name="element">Element in the window</param>
	/// <returns>The MainWindow if element is in the MainWindow, otherwise null</returns>
	public static MainWindow? ParentMainWindow(this FrameworkElement element) => ParentWindow<MainWindow>(element);
}
