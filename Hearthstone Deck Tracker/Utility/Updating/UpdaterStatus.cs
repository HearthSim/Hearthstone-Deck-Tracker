using System;
using System.Windows;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using static System.Windows.Visibility;

namespace Hearthstone_Deck_Tracker.Utility.Updating;

public class UpdaterStatus : ViewModel
{
	public Visibility StatusBarVisibility
	{
		get => GetProp(Collapsed);
		set => SetProp(value);
	}

	public Visibility DownloadingUpdateVisibility => UpdaterState == UpdaterState.Downloading ? Visible : Collapsed;
	public Visibility InstallingUpdateVisibility => UpdaterState == UpdaterState.Installing ? Visible : Collapsed;
	public Visibility UpdateInstalledVisibility => UpdaterState == UpdaterState.Available ? Visible : Collapsed;
	public Visibility UpdateFailedVisibility => UpdaterState == UpdaterState.Failed ? Visible : Collapsed;

	public UpdaterState UpdaterState
	{
		get => GetProp(UpdaterState.None);
		set
		{
			SetProp(value);
			OnPropertyChanged(nameof(DownloadingUpdateVisibility));
			OnPropertyChanged(nameof(InstallingUpdateVisibility));
			OnPropertyChanged(nameof(UpdateInstalledVisibility));
			OnPropertyChanged(nameof(UpdateFailedVisibility));
		}
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

		if(SkipStartupCheck)
			StatusBarVisibility = Visible;
	}

	public void OnInstallProgressChanged(int progress)
	{
		UpdaterState = UpdaterState.Installing;
		UpdateProgress = progress;

		if(SkipStartupCheck)
			StatusBarVisibility = Visible;
	}

	public void OnInstalled()
	{
		UpdaterState = UpdaterState.Available;
		StatusBarVisibility = Visible;
	}

	public void OnFailed(Exception ex)
	{
		// We might want special handling for specific exceptions in the future?

		UpdaterState = UpdaterState.Failed;
		StatusBarVisibility = Visible;
	}
}

public enum UpdaterState
{
	None,
	Checking,
	Downloading,
	Installing,
	Available,
	Failed
}
