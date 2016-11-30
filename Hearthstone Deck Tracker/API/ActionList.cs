#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hearthstone_Deck_Tracker.Plugins;
using Hearthstone_Deck_Tracker.Utility.Logging;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class ActionList<T>
	{
		private readonly List<Tuple<object, Action<T>>> _actions;

		public ActionList()
		{
			_actions = new List<Tuple<object, Action<T>>>();
		}

		public void Add(Action<T> action)
		{
			var caller = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
			var plugin = PluginManager.Instance.Plugins.FirstOrDefault(p => p.Plugin.GetType() == caller);
			_actions.Add(new Tuple<object, Action<T>>(plugin, action));
		}

		internal void Execute(T arg)
		{
			var remove = new List<Tuple<object, Action<T>>>();
			foreach(var action in _actions)
			{
				var sw = Stopwatch.StartNew();
				var plugin = action.Item1 as PluginWrapper;
				if(plugin != null && !plugin.IsEnabled)
				{
					remove.Add(action);
					continue;
				}
				try
				{
					action.Item2.Invoke(arg);
				}
				catch(Exception ex)
				{
					Log.Error($"Error invoking action{GetInfo(plugin)}:\n{ex}");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Log.Warn($"Warning: Invoking action{GetInfo(plugin)} took {sw.ElapsedMilliseconds} ms.");
#if(!DEBUG)
	//remove.Add(action);
#endif
				}
			}
			foreach(var action in remove)
				_actions.Remove(action);
		}

		private string GetInfo(PluginWrapper p) => p != null ? $" (Plugin: {p.Name})" : "";
	}

	public class ActionList
	{
		private readonly List<Tuple<object, Action>> _actions;

		public ActionList()
		{
			_actions = new List<Tuple<object, Action>>();
		}

		public void Add(Action action)
		{
			var caller = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
			var plugin = PluginManager.Instance.Plugins.FirstOrDefault(p => p.Plugin.GetType() == caller);
			_actions.Add(new Tuple<object, Action>(plugin, action));
		}

		internal void Execute()
		{
			var remove = new List<Tuple<object, Action>>();
			foreach(var action in _actions)
			{
				var sw = Stopwatch.StartNew();
				var plugin = action.Item1 as PluginWrapper;
				if(plugin != null && !plugin.IsEnabled)
				{
					remove.Add(action);
					continue;
				}
				try
				{
					action.Item2.Invoke();
				}
				catch(Exception ex)
				{
					Log.Error($"Error invoking action{GetInfo(plugin)}:\n{ex}");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Log.Warn($"Invoking action{GetInfo(plugin)} took {sw.ElapsedMilliseconds} ms.");
#if(!DEBUG)
	//remove.Add(action);
#endif
				}
			}
			foreach(var action in remove)
				_actions.Remove(action);
		}

		private string GetInfo(PluginWrapper p) => p != null ? $" (Plugin: {p.Name})" : "";
	}
}
