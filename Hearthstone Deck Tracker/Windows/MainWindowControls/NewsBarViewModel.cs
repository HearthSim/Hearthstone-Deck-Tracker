using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Hearthstone_Deck_Tracker.Utility;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Windows.MainWindowControls
{
	public class NewsBarViewModel : ViewModel
	{
		private const int NewsTickerUpdateInterval = 15;

		private static int _currentLine;
		private static string _currentNewsLine;
		private static DateTime _lastNewsUpdate = DateTime.MinValue;
		private static bool _updating;
		private RemoteConfig.ConfigData.NewsData _data;

		private TextBlock _dataContent;
		private string _indexContent;
		private Visibility _visibility = Visibility.Collapsed;

		public NewsBarViewModel()
		{
			RemoteConfig.Instance.Loaded += data =>
			{
				_data = data?.News;
				if(Config.Instance.IgnoreNewsId < _data?.Id)
					ShowNewsBar();
			};

			ConfigWrapper.IgnoreNewsIdChanged += () =>
			{
				if(ConfigWrapper.IgnoreNewsId == -1)
					ShowNewsBar();
				else
				{
					_updating = false;
					Visibility = Visibility.Collapsed;
				}
			};
		}

		public ICommand PreviousItemCommand => new Command(() =>
		{
			if((_data?.Items.Count ?? 0) <= 1)
				return;
			_currentLine--;
			if(_currentLine < 0)
				_currentLine = _data.Items.Count - 1;
			SetCurrentLine(_currentLine);
		});

		public ICommand NextItemCommand => new Command(() =>
		{
			if((_data?.Items.Count ?? 0) <= 1)
				return;
			_currentLine++;
			if(_currentLine > _data.Items.Count - 1)
				_currentLine = 0;
			SetCurrentLine(_currentLine);
		});

		public ICommand CloseCommand => new Command(() =>
		{
			ConfigWrapper.IgnoreNewsId = _data?.Id ?? 0;
		});

		public Visibility Visibility
		{
			get => _visibility;
			set
			{
				if(_visibility != value)
				{
					_visibility = value;
					OnPropertyChanged();
				}
			}
		}

		public TextBlock NewsContent
		{
			get => _dataContent;
			set
			{
				_dataContent = value;
				OnPropertyChanged();
			}
		}

		public string IndexContent
		{
			get => _indexContent;
			set
			{
				_indexContent = value;
				OnPropertyChanged();
			}
		}

		public double Height { get; } = 20;

		private async void UpdateNewsAsync()
		{
			_updating = true;
			while(_updating)
			{
				await Task.Delay(10000);
				if(DateTime.Now - _lastNewsUpdate > TimeSpan.FromSeconds(NewsTickerUpdateInterval))
				{
					if(_data == null || _data.Items.Count == 0)
					{
						if(Visibility == Visibility.Visible)
							Visibility = Visibility.Collapsed;
						break;
					}
					if(Visibility == Visibility.Collapsed)
						Visibility = Visibility.Visible;
					_currentLine++;
					if(_currentLine > _data.Items.Count - 1)
						_currentLine = 0;
					SetCurrentLine(_currentLine);
				}
			}
		}

		private void ShowNewsBar()
		{
			if(_data?.Items.Count > 0)
			{
				Visibility = Visibility.Visible;
				SetCurrentLine(0);
			}
			if(!_updating)
				UpdateNewsAsync();
		}

		private void SetCurrentLine(int newsLine)
		{
			if(_data == null || _data.Items.Count == 0)
				return;
			if(newsLine < _data.Items.Count && _currentNewsLine != _data.Items[newsLine])
			{
				_currentNewsLine = _data.Items[newsLine];
				NewsContent = MarkupParser.StringToTextBlock(_currentNewsLine);
			}

			IndexContent = $"({_currentLine + 1}/{_data.Items.Count})";
			_lastNewsUpdate = DateTime.Now;
		}
	}
}
