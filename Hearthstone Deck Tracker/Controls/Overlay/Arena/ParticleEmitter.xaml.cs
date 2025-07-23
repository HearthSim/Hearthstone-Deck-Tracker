using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using Hearthstone_Deck_Tracker.Utility.Particles;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public partial class ParticleEmitter
{
	private static readonly Random Rnd = new();

	private readonly List<ParticleSystem> _particleSystems = new();

	public static readonly DependencyProperty ParticleColorsProperty = DependencyProperty.Register(nameof(ParticleColors), typeof(List<Brush>), typeof(ParticleEmitter), new PropertyMetadata(new List<Brush>(), PropertyChangedCallback));

	private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if(d is not ParticleEmitter { IsInitialized: true } emitter)
			return;
		emitter.InitParticleSystems((List<Brush>)e.NewValue);
	}

	public List<Brush> ParticleColors
	{
		get => (List<Brush>)GetValue(ParticleColorsProperty);
		set => SetValue(ParticleColorsProperty, value);
	}

	private void InitParticleSystems(List<Brush> brushes)
	{
		Models.Children.Clear();
		Models.Children.Add(new AmbientLight(Colors.White));
		_particleSystems.Clear();
		foreach(var brush in brushes)
		{
			var ps = CreateParticleSystem(brush);
			_particleSystems.Add(ps);
			Models.Children.Add(ps.ParticleModel);
		}
	}

	private static ParticleSystem CreateParticleSystem(Brush brush)
	{
		const int size = 32;
		var ellipse = new Ellipse
		{
			Width = 32,
			Height = 32,
			Fill = brush
		};
		ellipse.Measure(new Size(size, size));
		ellipse.Arrange(new Rect(0, 0, size, size));

		return new ParticleSystem(ellipse, size, () => new EmberParticle(Rnd), 50);
	}

	public ParticleEmitter()
	{
		// Reference type dependency properties are shared
		SetValue(ParticleColorsProperty, new List<Brush>());

		InitializeComponent();
		_timer.Tick += OnTick;
	}

	private readonly DispatcherTimer _timer = new()
	{
		Interval = TimeSpan.FromSeconds(1.0 / 60)
	};

	private void ParticleEmitter_OnLoaded(object sender, RoutedEventArgs e)
	{
		InitParticleSystems(ParticleColors);
		_lastTick = Environment.TickCount;
		_timer.Start();
	}

	private void ParticleEmitter_OnUnloaded(object sender, RoutedEventArgs e)
	{
		_timer.Stop();
	}

	private int _nextSpawn;
	private int _lastTick;

	private void OnTick(object sender, EventArgs e)
	{

		var tick = Environment.TickCount;
		var time = tick / 1000.0;
		var dt = (tick - _lastTick) / 1000.0;
		_lastTick = tick;

		if(!IsVisible || _particleSystems.Count == 0)
			return;

		foreach(var ps in _particleSystems)
			ps.Update(time, dt);

		if(_nextSpawn < tick)
		{
			var ps = _particleSystems[Rnd.Next(0, _particleSystems.Count)];
			var width = ActualWidth - 20 - 6;
			var x = Rnd.NextDouble() * width - width / 2;
			var y = -(ActualHeight / 2) * (1 + Rnd.NextDouble() * 0.1);
			ps.SpawnParticle(new Point3D(x, y, 0));
			_nextSpawn = tick + Rnd.Next(250, 500);
		}
	}
}
