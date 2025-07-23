using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility;

public class MouseWatcher
{
	[Flags]
	public enum Direction
	{
		None  = 0,
		Up    = 1,
		Down  = 1 << 1,
		Left  = 1 << 2,
		Right = 1 << 3
	}

	private const int MinHistoryThreshold = 5;
	private const int MaxHistory = 10;
	private const int IntervalMs = 50;
	private readonly List<System.Drawing.Point> _history = new();
	private readonly TimeSpan _timeout;
	private readonly int _minDirectionDistance;

	public MouseWatcher(TimeSpan timeout, int minDirectionDistance)
	{
		_timeout = timeout;
		_minDirectionDistance = minDirectionDistance;
	}

	public event Action? OnTimeout;

	private Direction _prevDirection = Direction.None;
	public event Action<Direction>? OnDirectionChange;

	private bool _run = true;

	private async Task Run()
	{
		var timeoutStart = DateTime.Now;
		_prevDirection = Direction.None;
		_history.Clear();

		while(_run)
		{
			if(DateTime.Now - timeoutStart >= _timeout)
			{
				OnTimeout?.Invoke();
				_run = false;
				return;
			}

			var mousePos = User32.GetMousePos();
			if(_history.Count == 0)
				_history.Insert(0, mousePos);
			else
			{
				var prevPos = _history.First();
				if(prevPos.X != mousePos.X || prevPos.Y != mousePos.Y)
				{
					timeoutStart = DateTime.Now;
					_history.Insert(0, mousePos);
				}
			}

			while(_history.Count > MaxHistory)
				_history.RemoveRange(MaxHistory, _history.Count - MaxHistory);

			if(_history.Count > MinHistoryThreshold)
			{
				var startPos = _history.Last();
				var direction = Direction.None;
				if(startPos.Y - mousePos.Y > _minDirectionDistance)
					direction |= Direction.Up;
				else if(mousePos.Y - startPos.Y > _minDirectionDistance)
					direction |= Direction.Down;
				if(startPos.X - mousePos.X > _minDirectionDistance)
					direction |= Direction.Left;
				else if(mousePos.X - startPos.X > _minDirectionDistance)
					direction |= Direction.Right;

				if(direction != _prevDirection)
				{
					OnDirectionChange?.Invoke(direction);
					_prevDirection = direction;
				}
			}

			await Task.Delay(IntervalMs);
		}
	}

	private Task? _runTask;

	public void Start()
	{
		if(_run)
			return;
		_run = true;
		_runTask = Run();
	}

	public async Task Stop()
	{
		_run = false;
		if(_runTask != null)
			await _runTask;
	}

}
