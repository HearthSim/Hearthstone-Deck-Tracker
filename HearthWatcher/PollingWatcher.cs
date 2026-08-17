using System;
using System.Threading;
using System.Threading.Tasks;

namespace HearthWatcher;

public abstract class PollingWatcher
{
	private static SynchronizationContext? _eventContext;
	private static int _eventThreadId = -1;

	// call once from the UI thread during app startup, before any watcher runs
	public static void SetEventContext(SynchronizationContext? ctx)
	{
		_eventContext = ctx;
		_eventThreadId = ctx != null ? Environment.CurrentManagedThreadId : -1;
	}

	private readonly object _stateLock = new();
	private readonly int _delay;
	private bool _watch;
	private Task? _loop;

	protected PollingWatcher(int delay)
	{
		_delay = delay;
	}

	public void Run()
	{
		lock(_stateLock)
		{
			_watch = true;
			_loop ??= Task.Run(RunLoopAsync);
		}
	}

	public void Stop()
	{
		lock(_stateLock)
			_watch = false;
	}

	private bool Watching
	{
		get
		{
			lock(_stateLock)
				return _watch;
		}
	}

	private async Task RunLoopAsync()
	{
		while(true)
		{
			OnLoopStart();
			while(Watching)
			{
				await Task.Delay(_delay).ConfigureAwait(false);
				if(!Watching)
					break;
				if(await TickAsync().ConfigureAwait(false))
				{
					Stop();
					break;
				}
			}
			OnLoopEnd();
			lock(_stateLock)
			{
				if(!_watch)
				{
					_loop = null;
					return;
				}
				// Run() landed while we were draining, restart with fresh state
			}
		}
	}

	/// <summary>
	/// One poll iteration, runs on a thread-pool thread. Return true to stop the loop.
	/// </summary>
	protected abstract Task<bool> TickAsync();

	protected virtual void OnLoopStart()
	{
	}

	protected virtual void OnLoopEnd()
	{
	}

	// raise an event on the UI thread, inline when already there (compare thread ids,
	// WPF hands out distinct SynchronizationContext instances per operation)
	protected static void Dispatch(Action raise)
	{
		var ctx = _eventContext;
		if(ctx == null || Environment.CurrentManagedThreadId == _eventThreadId)
			raise();
		else
			ctx.Post(_ => raise(), null);
	}
}
