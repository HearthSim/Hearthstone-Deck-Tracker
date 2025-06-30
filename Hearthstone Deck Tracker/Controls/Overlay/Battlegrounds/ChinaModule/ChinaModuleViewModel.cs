using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.BobsBuddy;
using Hearthstone_Deck_Tracker.Commands;
using Hearthstone_Deck_Tracker.Utility.Battlegrounds;
using Hearthstone_Deck_Tracker.Utility.Logging;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Battlegrounds.ChinaModule
{
    public class ChinaModuleViewModel : ViewModel
    {
	    // After action delay for State and Button reset
        private const int DelayedResetTimeMs = 5000;
        private CancellationTokenSource? _delayedResetCts;

        public void Reset()
        {
            _delayedResetCts?.Cancel();
            _delayedResetCts = null;
            _isExecuting = false;

            HdtToolsActionState = HDTToolsActionState.WaitingCombat;
            Core.Game.IsChinaModuleActive = false;
            BobsBuddyState = BobsBuddyState.Initial;
            IsHDTToolsActionPending = false;
            AutoHDTToolsActionEnabled = false;
        }

        public BobsBuddyState BobsBuddyState
        {
            get => GetProp(BobsBuddyState.Initial);
            set
            {
                SetProp(value);
                NotifyButtonStateChanged();
                UpdateHDTToolsActionStateBasedOnCombat();
            }
        }

        public bool IsCombatState => BobsBuddyState is
            BobsBuddyState.Combat or
            BobsBuddyState.CombatPartial or
            BobsBuddyState.CombatWithoutSimulation;

        public bool IsHDTToolsActionPending
        {
            get => GetProp(false);
            set
            {
                SetProp(value);
                NotifyButtonStateChanged();
            }
        }

        private HDTToolsActionState HdtToolsActionState
        {
            get => GetProp(HDTToolsActionState.WaitingCombat);
            set
            {
                SetProp(value);
                NotifyButtonStateChanged();
                HandleStateTransitions(value);
                TryToAutoHDTToolsAction();
            }
        }

        public ButtonState ButtonState
        {
            get
            {
                // Button is disabled when auto-HDTToolsAction is enabled
                if (AutoHDTToolsActionEnabled)
                    return ButtonState.Disabled;

                return HdtToolsActionState switch
                {
                    HDTToolsActionState.Ready or
                    HDTToolsActionState.WaitingCombat => ButtonState.Enabled,
                    HDTToolsActionState.Failed or
                    HDTToolsActionState.SetupFailed or
                    HDTToolsActionState.Succeeded => ButtonState.Disabled,
                    HDTToolsActionState.InProgress or
	                HDTToolsActionState.QueuedForExecution or
                    HDTToolsActionState.SetupInProgress => ButtonState.Loading,
                    _ => ButtonState.Disabled
                };
            }
        }

        public bool ShowWaitingTooltip => AutoHDTToolsActionEnabled || IsHDTToolsActionPending;

        public bool ButtonEnabled => ButtonState == ButtonState.Enabled;

        public string ChinaModuleButtonText
        {
            get
            {
                if (IsHDTToolsActionPending)
                    return "已加入重连队列";

                return HdtToolsActionState switch
                {
                    HDTToolsActionState.Ready or HDTToolsActionState.WaitingCombat => "断线重连",
                    HDTToolsActionState.QueuedForExecution => "已加入重连队列",
                    HDTToolsActionState.InProgress => "重新连接中",
                    HDTToolsActionState.Succeeded => "重新连接成功",
                    HDTToolsActionState.SetupInProgress => "正在配置",
                    HDTToolsActionState.SetupFailed => "配置失败",
                    HDTToolsActionState.Failed => "重连失败",
                    _ => string.Empty
                };
            }
        }

        public bool AutoHDTToolsActionEnabled
        {
            get => GetProp(false);
            set
            {
                SetProp(value);

                if(IsCombatState && !IsHDTToolsActionPending)
                {
	                ExecuteHDTToolsActionAsync();
                }

                if (IsHDTToolsActionPending)
                {
                    IsHDTToolsActionPending = false;
                }
                NotifyButtonStateChanged();
            }
        }

        public ICommand HDTToolsActionCommand => new Command(() =>
        {
	        if(!ButtonEnabled)
		        return;

	        Core.Game.Metrics.BattlegroundsChinaModuleActionClicks++;

	        if(IsCombatState && !IsHDTToolsActionPending)
	        {
		        ExecuteHDTToolsActionAsync();
		        return;
	        }

	        IsHDTToolsActionPending = !IsHDTToolsActionPending;
        });
        public ICommand EnableAutoHDTToolsAction => new Command(() =>
        {
	        Core.Game.Metrics.BattlegroundsChinaModuleAutoActionClicks++;
            AutoHDTToolsActionEnabled = !AutoHDTToolsActionEnabled;
            if(AutoHDTToolsActionEnabled)
	            Core.Game.Metrics.BattlegroundsChinaModuleAutoActionEnabled = true;
        });

        private void UpdateHDTToolsActionStateBasedOnCombat()
        {
	        if (!IsCombatState && HdtToolsActionState == HDTToolsActionState.Ready)
	        {
		        HdtToolsActionState = HDTToolsActionState.WaitingCombat;
	        }
	        else if (IsCombatState && HdtToolsActionState == HDTToolsActionState.WaitingCombat)
	        {
		        if (IsHDTToolsActionPending)
		        {
			        HdtToolsActionState = HDTToolsActionState.QueuedForExecution;
			        IsHDTToolsActionPending = false;
		        }
		        else
		        {
			        HdtToolsActionState = HDTToolsActionState.Ready;
		        }
	        }
        }

        private void TryToAutoHDTToolsAction()
        {
	        if (((AutoHDTToolsActionEnabled && HdtToolsActionState == HDTToolsActionState.Ready) ||
	             HdtToolsActionState == HDTToolsActionState.QueuedForExecution) &&
	            IsCombatState)
	        {
		        ExecuteHDTToolsActionAsync();
	        }
        }

        private void HandleStateTransitions(HDTToolsActionState newActionState)
        {
            if (newActionState is HDTToolsActionState.Succeeded or HDTToolsActionState.Failed or HDTToolsActionState.SetupFailed)
            {
                // Cancel any existing delayed reset
                _delayedResetCts?.Cancel();
                _delayedResetCts = new CancellationTokenSource();
                _ = HandleDelayedResetAsync(_delayedResetCts.Token);
            }
        }

        private async Task HandleDelayedResetAsync(CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(DelayedResetTimeMs, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                HdtToolsActionState = HDTToolsActionState.WaitingCombat;
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void NotifyButtonStateChanged()
        {
	        OnPropertyChanged(nameof(IsCombatState));
            OnPropertyChanged(nameof(ButtonState));
            OnPropertyChanged(nameof(ButtonEnabled));
            OnPropertyChanged(nameof(ChinaModuleButtonText));
            OnPropertyChanged(nameof(ShowWaitingTooltip));
        }

        private bool _isUpToDate;
        private bool _isExecuting;

        private async void ExecuteHDTToolsActionAsync()
        {
	        if(_isExecuting)
		        return;

	        _isExecuting = true;
	        try
	        {
		        if(Core.Game.MetaData.ServerInfo == null || !IsCombatState)
			        return;

		        if(HdtToolsActionState != HDTToolsActionState.QueuedForExecution
		           && HdtToolsActionState != HDTToolsActionState.Ready)
			        return;

		        // We should only run this once per session.
		        // If the state gets reset, it's also fine to run it again.
		        // If this fails, we will try again the next time the button is pressed.
		        // It should not prevent the user from continuing with the action if it fails.
		        if(_isUpToDate && BattlegroundsChinaModule.GetHdtToolsPath() == null)
			        _isUpToDate = false;

		        if(!_isUpToDate)
					_isUpToDate = await Task.Run(HDTToolsManager.EnsureLatestHDTTools);

		        if(!BattlegroundsChinaModule.IsHDTToolsTaskReady())
		        {
			        HdtToolsActionState = HDTToolsActionState.SetupInProgress;
			        if(!await BattlegroundsChinaModule.SetupHDTToolsTask())
			        {
				        HdtToolsActionState = HDTToolsActionState.SetupFailed;
				        return;
			        }
		        }

		        var serverInfo = Core.Game.MetaData.ServerInfo;
		        HdtToolsActionState = HDTToolsActionState.InProgress;

		        var succeeded = await BattlegroundsChinaModule.RunHDTTools(
			        serverInfo.Address,
			        serverInfo.Port);

		        if(succeeded)
		        {
			        Core.Game.IsChinaModuleActive = true;
			        HdtToolsActionState = HDTToolsActionState.Succeeded;
			        if(Core.Game.CurrentGameStats != null)
			        {
				        var invoker = BobsBuddyInvoker.GetInstance(Core.Game.CurrentGameStats.GameId,
					        Core.Game.GetTurnNumber());
				        invoker.State = BobsBuddyState.Shopping;
				        invoker.StartShoppingAsync();
			        }

			        Core.Game.Metrics.BattlegroundsChinaModuleActionSuccessCount++;
		        }
		        else
		        {
			        HdtToolsActionState = HDTToolsActionState.Failed;
		        }
	        }
	        catch(Exception e)
	        {
		        HdtToolsActionState = HDTToolsActionState.Failed;
		        Log.Warn($"BattlegroundsChinaModule: HDTTools Action Failed: {e.Message}");
	        }
	        finally
	        {
		        _isExecuting = false;
	        }
        }
    }

    public enum HDTToolsActionState
    {
        WaitingCombat,
        Ready,
        QueuedForExecution,
        InProgress,
        Succeeded,
        Failed,
        SetupFailed,
        SetupInProgress,
    }

    public enum ButtonState
    {
        Enabled,
        Disabled,
        Loading,
    }
}
