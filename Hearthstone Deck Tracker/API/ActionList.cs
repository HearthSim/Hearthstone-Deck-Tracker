#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Hearthstone_Deck_Tracker.Plugins;

#endregion

namespace Hearthstone_Deck_Tracker.API
{
	public class ActionList<T>
	{
		private readonly List<Action<T>> _actions;

		public ActionList()
		{
			_actions = new List<Action<T>>();
		}

		public void Add(Action<T> action)
		{
			_actions.Add(action);
		}

		internal void Execute(T arg)
		{
			foreach(var action in _actions)
			{
				var sw = Stopwatch.StartNew();
				try
				{
					action.Invoke(arg);
				}
				catch(Exception ex)
				{
					Logger.WriteLine(string.Format("Error invoking action:\n{0}", ex), "ActionListExecution");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Logger.WriteLine(string.Format("Invoking action took {0} ms. Removed action.", sw.ElapsedMilliseconds), "ActionListExecution");
					//TODO: ACTUALLY REMOVE
				}
			}
		}
	}

	public class ActionList
	{
		private readonly List<Action> _actions;

		public ActionList()
		{
			_actions = new List<Action>();
		}

		public void Add(Action action)
		{
			_actions.Add(action);
		}

		internal void Execute()
		{
			foreach(var action in _actions)
			{
				var sw = Stopwatch.StartNew();
				try
				{
					action.Invoke();
				}
				catch(Exception ex)
				{
					Logger.WriteLine(string.Format("Error invoking action:\n{0}", ex), "EventManager");
				}
				if(sw.ElapsedMilliseconds > PluginManager.MaxPluginExecutionTime)
				{
					Logger.WriteLine(string.Format("Invoking action took {0} ms. Removed action.", sw.ElapsedMilliseconds), "ActionListExecution");
					//TODO: ACTUALLY REMOVE
				}
			}
		}
	}
}