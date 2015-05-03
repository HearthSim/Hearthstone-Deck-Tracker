#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Hearthstone_Deck_Tracker.Plugins;

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
					continue;
				try
				{
					action.Item2.Invoke(arg);
				}
				catch(Exception ex)
				{
					Logger.WriteLine(string.Format("Error invoking action{0}:\n{1}", GetInfo(plugin), ex), "ActionListExecution");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Logger.WriteLine(string.Format("Invoking action{0} took {1} ms. Removed action.", GetInfo(plugin), sw.ElapsedMilliseconds),
					                 "ActionListExecution");
#if(!DEBUG)
					remove.Add(action);
#endif
				}
			}
			foreach(var action in remove)
				_actions.Remove(action);
		}

		private string GetInfo(PluginWrapper p)
		{
			return p != null ? string.Format(" (Plugin: {0})", p.Name) : "";
		}
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
			foreach(var action in _actions)
			{
				var sw = Stopwatch.StartNew();
				var plugin = action.Item1 as PluginWrapper;
				if(plugin != null && !plugin.IsEnabled)
					continue;
				try
				{
					action.Item2.Invoke();
				}
				catch(Exception ex)
				{
					Logger.WriteLine(string.Format("Error invoking action{0}:\n{1}", GetInfo(plugin), ex), "ActionListExecution");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Logger.WriteLine(string.Format("Invoking action{0} took {1} ms. Removed action.", GetInfo(plugin), sw.ElapsedMilliseconds),
					                 "ActionListExecution");
					//TODO: ACTUALLY REMOVE
				}
			}
		}

		private string GetInfo(PluginWrapper p)
		{
			return p != null ? string.Format(" (Plugin: {0})", p.Name) : "";
		}
	}
}