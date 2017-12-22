#region

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;

#endregion

namespace Hearthstone_Deck_Tracker
{
	/// <summary>
	/// Interaction logic for MultiProgressBar.xaml
	/// </summary>
	public partial class ManaCostBar
	{
		private readonly Rectangle[] _bars;
		private readonly double[] _previousBarHeights;
		private bool _cancelCurrentAnimation;
		private bool _isAnimationRunning;
		private double[] _nextAnimation;

		public ManaCostBar()
		{
			InitializeComponent();
			_previousBarHeights = new[] {0.0, 0.0, 0.0, 0.0};
			_bars = new[] {WeaponsRect, SpellsRect, MinionsRect, HeroesRect};
			AnimationDuration = 300;
			FrameDelay = 20;
			_nextAnimation = new double[4];
			_isAnimationRunning = false;
			McbToolTip.SetValue(DataContextProperty, this);
		}

		public int AnimationDuration { get; set; }
		public int FrameDelay { get; set; }

		public void SetTooltipValues(int weapons, int spells, int minions, int heroes)
		{
			McbToolTip.TextBlockWeaponsCount.Text = weapons.ToString();
			McbToolTip.TextBlockSpellsCount.Text = spells.ToString();
			McbToolTip.TextBlockMinionsCount.Text = minions.ToString();
			McbToolTip.TextBlockHeroesCount.Text = heroes.ToString();
		}

		public void SetValues(double weapons, double spells, double minions, double heroes, int count)
		{
			TextBlockCount.Text = count.ToString();

			_nextAnimation = new[] {ActualHeight * weapons / 100, ActualHeight * spells / 100, ActualHeight * minions / 100, ActualHeight * heroes / 100 };

			if(!_isAnimationRunning)
				Animate();
			else
				_cancelCurrentAnimation = true;
		}

		private bool AnimateBar(Rectangle bar, double from, double to)
		{
			if(to > from)
			{
				if(bar.Height < to)
				{
					bar.Height += (Math.Abs(from - to) * FrameDelay) / AnimationDuration;
					if(bar.Height > to)
					{
						bar.Height = to;
						return true;
					}
				}
				else
				{
					bar.Height = to;
					return true;
				}
			}
			else if(to < from)
			{
				if(bar.Height > to)
				{
					var newHeight = bar.Height - (Math.Abs(from - to) * FrameDelay) / AnimationDuration;
					if(newHeight < to || newHeight < 0)
					{
						bar.Height = to;
						return true;
					}

					bar.Height -= (Math.Abs(from - to) * FrameDelay) / AnimationDuration;
				}
				else
				{
					bar.Height = to;
					return true;
				}
			}
			else
				return true;
			return false;
		}

		private async void Animate()
		{
			_isAnimationRunning = true;

			while(_nextAnimation != null)
			{
				var targetValues = _nextAnimation;
				_nextAnimation = null;

				bool[] done = {false, false, false, false};

				if(double.IsNaN(targetValues[0]) || targetValues[0] < 0)
					targetValues[0] = 0.0;
				if(double.IsNaN(targetValues[1]) || targetValues[1] < 0)
					targetValues[1] = 0.0;
				if(double.IsNaN(targetValues[2]) || targetValues[2] < 0)
					targetValues[2] = 0.0;
				if(double.IsNaN(targetValues[3]) || targetValues[3] < 0)
					targetValues[3] = 0.0;

				while(!done[0] || !done[1] || !done[2] || !done[3])
				{
					if(_cancelCurrentAnimation)
						break;
					//minions first, heroes last
					if(!done[3])
						done[3] = AnimateBar(_bars[3], _previousBarHeights[3], targetValues[3]);

					if (!done[2])
						done[2] = AnimateBar(_bars[2], _previousBarHeights[2], targetValues[2]);

					if(!done[1])
						done[1] = AnimateBar(_bars[1], _previousBarHeights[1], targetValues[1]);

					if(!done[0])
						done[0] = AnimateBar(_bars[0], _previousBarHeights[0], targetValues[0]);

					var offset = _bars[0].ActualHeight + _bars[1].ActualHeight + _bars[2].ActualHeight + _bars[3].ActualHeight - TextBlockCount.ActualHeight - 5;
					if(offset < -4)
						offset = -4;
					TextBlockCount.Margin = new Thickness(0, 0, 0, offset);

					await Task.Delay(FrameDelay);
				}
				_cancelCurrentAnimation = false;
				_previousBarHeights[0] = _bars[0].Height;
				_previousBarHeights[1] = _bars[1].Height;
				_previousBarHeights[2] = _bars[2].Height;
				_previousBarHeights[3] = _bars[3].Height;
			}

			_isAnimationRunning = false;
		}
	}
}
