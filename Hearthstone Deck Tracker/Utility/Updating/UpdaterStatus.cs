using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Utility.Updating;

public class UpdaterStatus : ViewModel
{
	public Visibility Visibility
	{
		get => GetProp(Visibility.Collapsed);
		set => SetProp(value);
	}

	public UpdaterState UpdaterState
	{
		get => GetProp(UpdaterState.None);
		set => SetProp(value);
	}

	public int UpdateProgress
	{
		get => GetProp(0);
		set => SetProp(value);
	}

	public bool SkipStartupCheck
	{
		get => GetProp(false);
		set => SetProp(value);
	}

	public void OnDownloadProgressChanged(int progress)
	{
		UpdaterState = UpdaterState.Downloading;
		UpdateProgress = progress;
	}

	public void OnInstallProgressChanged(int progress)
	{
		UpdaterState = UpdaterState.Installing;
		UpdateProgress = progress;
	}
}

public enum UpdaterState
{
	None,
	Checking,
	Downloading,
	Installing,
	Installed
}
