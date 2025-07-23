using System;
using System.Windows.Media.Media3D;

namespace Hearthstone_Deck_Tracker.Utility.Particles;

public class EmberParticle : Particle
{
	private readonly double _seed;
	private readonly double _swaySpeed;
	private readonly double _swayDist;
	private readonly double _startVelocityY;

	public EmberParticle(Random random) : base(random)
	{
		_seed = random.NextDouble();
		_swaySpeed = 10 * random.NextDouble() + 20;
		_swayDist = 5 * random.NextDouble() + 15;
		StartLife = Life = 2 * random.NextDouble() + 4;
		StartSize = Size = 5 * random.NextDouble() + 2;
		Decay = 1;
		_startVelocityY = 4 * random.NextDouble() + 20;

		Velocity = new Vector3D(0, _startVelocityY, 0);
	}

	public override void Update(double time, double dt)
	{
		var fractAlive = Life / StartLife;
		var swayDecay = 0.1 + (1 - fractAlive) * _seed * 0.9;
		Velocity.X = Math.Sin(dt * _swaySpeed + _seed * Math.PI) * _swayDist * swayDecay;
		Velocity.Y = _startVelocityY * (1 - MathUtil.CubicEaseIn(1 - fractAlive));
		Size *= (1 - MathUtil.CubicEaseIn(1 - fractAlive));

		base.Update(time, dt);
	}
}
