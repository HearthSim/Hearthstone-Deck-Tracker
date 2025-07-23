using System;
using System.Collections.Generic;
using System.Windows.Media;
using Hearthstone_Deck_Tracker.Utility.MVVM;

namespace Hearthstone_Deck_Tracker.Controls.Overlay.Arena;

public class ArenaPlaqueViewModel : ViewModel
{
	private readonly int _seed;

	public ArenaPlaqueViewModel(string score, int level, int randomSeed, bool isUnderground)
	{
		_seed = randomSeed;
		Score = score;
		Level = level;
		IsUnderground = isUnderground;

		var random = new Random(_seed);
		double RandomScale(double value)
		{
			return value * (1 + (random.NextDouble() * 0.1 - 0.05));
		}

		InnerFlames = new[]
		{
			new FlameData(RandomScale(25), RandomScale(-1), RandomScale(.8)),
			new FlameData(RandomScale(25), RandomScale(-1), RandomScale(.7)),
			new FlameData(RandomScale(4), RandomScale(-1.2), RandomScale(1)),
		};

		OuterFlames = new[]
		{
			new FlameData(RandomScale(-30), RandomScale(1), RandomScale(1)),
			new FlameData(RandomScale(29), RandomScale(1), RandomScale(.75)),
		};
	}

	public string Score { get; }

	public int Level { get; }
	public bool IsUnderground { get; }

	public bool IsLoading => string.IsNullOrEmpty(Score);

	public bool IsLevel5 => Level == 5;
	public bool IsLevel4OrHigher => Level >= 4;
	public bool IsLevel3OrHigher => Level >= 3;
	public bool IsLevel2OrHigher => Level >= 2;
	public bool IsLevel1 => Level == 1;

	public int SingleBoltIndex => Math.Abs(_seed) % 4;

	public bool HasTopLeftBolt => Level != 1 || SingleBoltIndex == 0;
	public bool HasTopRightBolt => Level != 1 || SingleBoltIndex == 1;
	public bool HasBottomRightBolt => Level != 1 || SingleBoltIndex == 2;
	public bool HasBottomLeftBolt => Level != 1 || SingleBoltIndex == 3;

	public int Angle
	{
		get
		{
			if(Level != 1)
				return 0;
			var sign = SingleBoltIndex is 0 or 3 ? 1 : -1;
			var angle = 2 + Math.Abs(_seed) % 3;
			return sign * angle;
		}
	}

	public (double, double) RotateOrigin
	{
		get
		{
			const int width = 90;
			const int height = 58;
			const int boltRadius = 4;
			var boltCenter = boltRadius + (Level >= 4 ? 6 : Level == 2 ? 5 : 4);
			return SingleBoltIndex switch
			{
				0 => (boltCenter, boltCenter),
				1 => (width - boltCenter, boltCenter),
				2 => (width - boltCenter, height - boltCenter),
				_ => (boltCenter, height - boltCenter),
			};
		}
	}

	public double RotateOriginX => RotateOrigin.Item1;
	public double RotateOriginY => RotateOrigin.Item2;

	public FlameData[] InnerFlames { get; }
	public FlameData[] OuterFlames { get; }

	private static readonly List<Brush> _defaultParticleBrushes = new()
	{
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0x01, 0xDD, 0xFE), Offset = 1 },
			}
		},
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0x01, 0xFE, 0xDD), Offset = 1 },
			}
		},
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0xBA, 0xDD, 0xFE), Offset = 1 },
			}
		},
	};

	private static readonly List<Brush> _undergroundParticleBrushes = new()
	{
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFB, 0xD9, 0x6F), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0xCD, 0x3E, 0x00), Offset = 1 },
			},
		},
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFB, 0xD9, 0x6F), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0xE1, 0x3E, 0x08), Offset = 1 },
			}
		},
		new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb(0xFF, 0xFB, 0xD9, 0x6F), Offset = 0 },
				new GradientStop { Color = Color.FromArgb(0x00, 0xFB, 0xB0, 0x52), Offset = 1 },
			}
		}
	};

	public List<Brush> ParticleColors => IsUnderground ? _undergroundParticleBrushes : _defaultParticleBrushes;

	public record FlameData(double Angle, double ScaleX, double ScaleY);
}
