using System.Collections.Generic;

namespace Hearthstone_Deck_Tracker.Utility;

public class Pool<T> where T : new()
{
	private readonly int _capacity;
	private readonly Stack<T> _items;

	public Pool(int capacity)
	{
		_capacity = capacity;
		_items = new Stack<T>(capacity);
	}

	public T GetOrCreate()
	{
		if(_items.Count == 0)
			return new T();
		var item = _items.Pop();
		(item as IPoolItem)?.OnReuseFromPool();
		return item;
	}

	public void Return(T item)
	{
		if(_items.Count <= _capacity)
		{
			(item as IPoolItem)?.OnReturnToPool();
			_items.Push(item);
		}
	}
}

public interface IPoolItem
{
	void OnReturnToPool();
	void OnReuseFromPool();
}
