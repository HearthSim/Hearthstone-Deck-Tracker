using System;
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
		if(_items.Count < _capacity)
		{
			(item as IPoolItem)?.OnReturnToPool();
			_items.Push(item);
		}
		else
			(item as IDisposable)?.Dispose();
	}
}

// Prefers handing back the instance that was last returned under the same key. This allows
// consumers to skip re-initialization entirely (e.g. an AnimatedCard that is asked to display
// the same card again can keep its bound viewmodel and rendered visuals).
public class KeyedPool<T> where T : class, new()
{
	private readonly int _capacity;
	private readonly Dictionary<string, Stack<T>> _byKey = new();
	private int _count;

	public KeyedPool(int capacity)
	{
		_capacity = capacity;
	}

	public T GetOrCreate(string key)
	{
		if(!_byKey.TryGetValue(key, out var stack))
		{
			// no exact match, reuse any pooled instance
			foreach(var pair in _byKey)
			{
				key = pair.Key;
				stack = pair.Value;
				break;
			}
		}
		if(stack == null)
			return new T();
		var item = stack.Pop();
		if(stack.Count == 0)
			_byKey.Remove(key);
		_count--;
		(item as IPoolItem)?.OnReuseFromPool();
		return item;
	}

	public void Return(string key, T item)
	{
		if(_count < _capacity)
		{
			(item as IPoolItem)?.OnReturnToPool();
			if(!_byKey.TryGetValue(key, out var stack))
				_byKey[key] = stack = new Stack<T>();
			stack.Push(item);
			_count++;
		}
		else
			(item as IDisposable)?.Dispose();
	}
}

public interface IPoolItem
{
	void OnReturnToPool();
	void OnReuseFromPool();
}
