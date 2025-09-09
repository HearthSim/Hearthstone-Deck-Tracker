using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Utility.Analytics;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Microsoft.Win32.TaskScheduler;
using Task = System.Threading.Tasks.Task;

namespace Hearthstone_Deck_Tracker.Utility.Battlegrounds;

public class BattlegroundsChinaModule
{
	private const string TaskName = "HDTTools_AdminTask";

	public static async Task<bool> RunHDTTools(string address, uint port)
	{
		try
		{
			var taskRunResult = await RunWithTask(address, port);

			if(taskRunResult == HDTToolsResult.ExecutionError)
			{
				Log.Error("BattlegroundsChinaModule: Failed to run scheduled task.");
				return false;
			}

			Log.Info("BattlegroundsChinaModule: Task started successfully.");
			return true;
		}
		catch(Exception e)
		{
			Log.Error($"BattlegroundsChinaModule: Failed to run scheduled task: {e.Message}");
			return false;
		}
	}

	internal static string? GetHdtToolsPath()
	{
		var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

		if(string.IsNullOrEmpty(baseDirectory))
		{
			Log.Warn("Failed to determine HDT directory");
			return null;
		}

		var hdtToolsPath = System.IO.Path.Combine(baseDirectory, "HDTTools", "bin", "HDTTools.exe");

		if(System.IO.File.Exists(hdtToolsPath))
			return hdtToolsPath;

		Log.Warn($"HDTTools executable not found at path: {hdtToolsPath}");
		return null;
	}

	private static async Task<bool> RunHDTToolsWithArgument(string argument)
	{
		try
		{
			var hdtToolsPath = GetHdtToolsPath();

			if(hdtToolsPath == null)
			{
				Log.Error("BattlegroundsChinaModule: HDTTools was not found.");
				OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.NotFound);
				return false;
			}

			Log.Info($"BattlegroundsChinaModule: Running locally with arguments: {argument}");

			var process = new Process
			{
				StartInfo =
				{
					FileName = hdtToolsPath,
					Arguments = argument,
					UseShellExecute = true,
					CreateNoWindow = false,
					Verb = "runas"
				}
			};

			var started = process.Start();

			if(started)
			{
				Log.Info("BattlegroundsChinaModule: HDTTools is running with elevated privileges...");
				await Task.Run(() => process.WaitForExit());
				var exitCode = (HDTToolsExitCode)process.ExitCode;
				Log.Info($"BattlegroundsChinaModule: HDTTools process exited with code {exitCode}");
				OnBattlegroundsHDTToolsExit(exitCode);
				return exitCode == HDTToolsExitCode.Success;
			}

			Log.Error("BattlegroundsChinaModule: Failed to start HDTTools process");
			OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.FailedToStart);
			return false;
		}
		// Refusing to give elevated privileges is a common case.
		catch(System.ComponentModel.Win32Exception ex) when(ex.NativeErrorCode == 1223)
		{
			Log.Warn("BattlegroundsChinaModule: User refused to give elevated privileges.");
			OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.ElevatedPrivilegesRefused);
			return false;
		}
		catch(Exception ex)
		{
			Log.Error($"BattlegroundsChinaModule: Failed to run locally with elevated privileges: {ex.Message}");
			OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.GeneralError);
			return false;
		}
	}

	public static Task<bool> SetupHDTToolsTask() => RunHDTToolsWithArgument("--clear");
	public static Task<bool> RemoveHDTToolsTask() => RunHDTToolsWithArgument("--remove");

	private static async Task<HDTToolsResult> RunWithTask(string address, uint port)
	{
		try
		{
			using var ts = new TaskService();
			var task = ts.GetTask(TaskName);
			if(task == null)
			{
				Log.Warn($"Task {TaskName} not found");
				OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.TaskNotFound);
				return HDTToolsResult.TaskNotFound;
			}

			if(!IsTaskUpToDate(task))
			{
				OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.TaskOutOfDate);
				return HDTToolsResult.TaskOutOfDate;
			}

			var arguments = $"{address} {port}";
			Log.Info($"BattlegroundsChinaModule: Running task with arguments: {arguments}");

			task.Run(arguments);
			Log.Info("BattlegroundsChinaModule: Task is running with elevated privileges...");

			const int maxWaitSeconds = 30;
			const int waitInterval = 500;
			const int totalWaits = (maxWaitSeconds * 1000) / waitInterval;

			for(var i = 0; i < totalWaits; i++)
			{
				await Task.Delay(waitInterval);

				// Get a fresh instance of the task to check the current status
				var currentTask = ts.GetTask(TaskName);
				if(currentTask == null)
				{
					Log.Error("BattlegroundsChinaModule: Task not found while checking status");
					OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.TaskStatusNotFound);
					return HDTToolsResult.ExecutionError;
				}

				if(currentTask.State != TaskState.Running)
				{
					var lastResult = currentTask.LastTaskResult;
					OnBattlegroundsHDTToolsExit((HDTToolsExitCode)lastResult);

					if(lastResult == (int)HDTToolsExitCode.Success)
					{
						Log.Info("BattlegroundsChinaModule: Task completed successfully");
						return HDTToolsResult.Success;
					}

					Log.Error($"BattlegroundsChinaModule: Task failed with result code: {lastResult}");
					return HDTToolsResult.ExecutionError;
				}
			}

			Log.Error("BattlegroundsChinaModule: Task timed out");
			OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.TaskTimedOut);
			return HDTToolsResult.ExecutionError;
		}
		catch(Exception ex)
		{
			Log.Error($"BattlegroundsChinaModule: Failed to run scheduled task: {ex.Message}");
			OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem.TaskGeneralError);
			return HDTToolsResult.ExecutionError;
		}
	}

	public static bool IsHDTToolsTaskReady()
	{
		try
		{
			using var ts = new TaskService();
			var task = ts.GetTask(TaskName);
			if(task == null)
			{
				Log.Warn($"BattlegroundsChinaModule: Task {TaskName} not found");
				return false;
			}

			if(!IsTaskUpToDate(task))
			{
				Log.Warn($"BattlegroundsChinaModule: Task {TaskName} outdated");
				return false;
			}

			Log.Warn($"BattlegroundsChinaModule: Task {TaskName} is ready.");
			return true;
		}
		catch(Exception ex)
		{
			Log.Error($"BattlegroundsChinaModule: Failed to check task readiness: {ex.Message}");
			return false;
		}
	}

	private static bool IsTaskUpToDate(Microsoft.Win32.TaskScheduler.Task hdtToolsTask)
	{
		if(hdtToolsTask.Definition.Actions.Count == 0
		   || hdtToolsTask.Definition.Actions[0] is not ExecAction execAction)
		{
			Log.Warn("BattlegroundsChinaModule: Task doesn't have an ExecAction");
			return false;
		}

		var binaryPath = execAction.Path;

		if(!System.IO.File.Exists(binaryPath))
		{
			Log.Warn("BattlegroundsChinaModule: Task binary path does not exist");
			return false;
		}

		var versionInfo = FileVersionInfo.GetVersionInfo(binaryPath);

		var hdtToolsPath = GetHdtToolsPath();
		if (hdtToolsPath == null)
		{
			Log.Warn("BattlegroundsChinaModule: HDTTools was not found.");
			throw new Exception("HDTTools was not found.");
		}

		var localHdtToolVersion = FileVersionInfo.GetVersionInfo(hdtToolsPath);

		return versionInfo.FileVersion == localHdtToolVersion.FileVersion;
	}

	private static void OnBattlegroundsHDTToolsExecutionProblem(HDTToolsExecutionProblem problem)
	{
		Influx.OnBattlegroundsHDTToolsExecutionProblem(problem);
		Influx.OnBattlegroundsHDTToolsNotFound(problem == HDTToolsExecutionProblem.NotFound, true);
		if(problem is not HDTToolsExecutionProblem.ElevatedPrivilegesRefused)
			Sentry.CaptureHDTToolsExecutionProblem(problem.ToString());
	}

	private static void OnBattlegroundsHDTToolsExit(HDTToolsExitCode exitCode)
	{
		Influx.OnBattlegroundsHDTToolsExit(exitCode);
		Influx.OnBattlegroundsHDTToolsNotFound(false);
		if(exitCode is not HDTToolsExitCode.Success)
			Sentry.CaptureHDTToolsExitProblem(exitCode.ToString(), HDTToolsManager.GetRecentLogs());
	}

 	public static bool IsChineseEnvironment()
	{
		var gameRegion = Core.Game.CurrentRegion;
		var gameLanguage = LocUtil.GetHearthstoneLanguageFromRegistry();
		var hdtLanguage = Config.Instance.Localization;
		var timeZone = TimeZoneInfo.Local.Id;

		return gameRegion is Region.CHINA &&
		       gameLanguage is "zhCN" &&
		       hdtLanguage is Language.zhCN &&
		       timeZone is "China Standard Time";
	}

	private enum HDTToolsResult
	{
		/// <summary>
		/// The task was successfully started with elevated privileges.
		/// </summary>
		Success,

		/// <summary>
		/// The required scheduled task was not found in the Windows Task Scheduler.
		/// </summary>
		TaskNotFound,

		/// <summary>
		/// The scheduled task is out of date and needs to be updated.
		///	</summary>
		TaskOutOfDate,

		/// <summary>
		/// An exception occurred while attempting to run the scheduled task.
		/// </summary>
		ExecutionError
	}

	internal enum HDTToolsExitCode
	{
		// Success
		Success = 0,

		// General errors
		GeneralError = 1,
		InvalidArguments = 2,

		// Elevation related errors
		NotElevated = 10,

		// Setup related errors
		SetupFolderError = 20,
		FileCopyError = 21,
		TaskCreationError = 22,

		// Disconnection-related errors
		ConnectionNotFound = 30,
		AccessDenied = 31,
		InvalidParameter = 32,
		NotSupported = 33,
		DisconnectionError = 39,

		// Wrong Environment
		WrongEnvironment = 99,
	}

	internal enum HDTToolsExecutionProblem
	{
		FailedToStart,
		NotFound,
		ElevatedPrivilegesRefused,
		GeneralError,
		TaskNotFound,
		TaskOutOfDate,
		TaskStatusNotFound,
		TaskTimedOut,
		TaskGeneralError,
	}
}
