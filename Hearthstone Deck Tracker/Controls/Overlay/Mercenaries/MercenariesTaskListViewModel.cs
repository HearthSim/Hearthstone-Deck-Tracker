using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.Extensions;
using Hearthstone_Deck_Tracker.Utility.MVVM;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Mercenaries
{
	public class MercenariesTaskListViewModel : ViewModel
	{
		private string _buttonText = "Show Tasks";
		public string ButtonText
		{
			get => _buttonText;
			set { _buttonText = value; OnPropertyChanged(); }
		}

		private Visibility _gameNoticeVisibility = Visibility.Collapsed;
		public Visibility GameNoticeVisibility
		{
			get => _gameNoticeVisibility;
			set
			{
				if(_gameNoticeVisibility == value)
					return;
				_gameNoticeVisibility = value;
				OnPropertyChanged();
			}
		}


		private List<MercenariesTaskViewModel>? _tasks;

		public List<MercenariesTaskViewModel>? Tasks
		{
			get => _tasks;
			set { _tasks = value; OnPropertyChanged(); }
		}

		private List<MercenariesTaskData>? _taskData;
		public bool Update()
		{
			if(_taskData == null)
				_taskData = Reflection.GetMercenariesTasksData();

			if(_taskData == null)
				return false;

			var tasks = Reflection.GetMercenariesVisitorTasks();
			if(tasks == null || tasks.Count == 0)
				return false;
			Tasks = tasks.Select(task =>
			{
				var taskData = _taskData.FirstOrDefault(x => x.Id == task.TaskId);
				if(taskData == null || taskData.MercenaryDefaultDbfId == null)
					return null;
				var card = Database.GetCardFromDbfId(taskData.MercenaryDefaultDbfId.Value, false);
				if(card == null)
					return null;
				var title = taskData.Title.Contains(":") ? taskData.Title : string.Format(LocUtil.Get("MercenariesTaskList_TaskTitle"), task.TaskChainProgress + 1, taskData.Title);
				return new MercenariesTaskViewModel(card, title, taskData.Description, taskData.Quota, task.Progress);
			}).WhereNotNull().ToList();
			return true;
		}
	}
}
