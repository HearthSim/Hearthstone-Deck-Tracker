using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Hearthstone_Deck_Tracker.Utility
{
	public class DelayedMouseOver
	{
		private readonly int _delay;
		private readonly int _tolerance;
		private object _current;

		public DelayedMouseOver(int delay, int tolerance = 3)
		{
			_delay = delay;
			_tolerance = tolerance;
		}

		public async void DelayedMouseOverDetection(object target, Action onSuccess, Action onMoved = null)
		{
			if(_current == target)
				return;
			_current = target;
			var mousePos = User32.GetMousePos();
			await Task.Delay(_delay);
			if(Distance(User32.GetMousePos(), mousePos) > _tolerance)
			{
				onMoved?.Invoke();
				_current = null;
				return;
			}
			if(_current != target)
				return;
			onSuccess?.Invoke();
		}

		private double Distance(Point p1, Point p2) => Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2);

		public bool HasCurrent => _current != null;

		public void Clear() => _current = null;
	}
}
