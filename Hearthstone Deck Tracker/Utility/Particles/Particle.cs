using System;
using System.Windows.Media.Media3D;

namespace Hearthstone_Deck_Tracker.Utility.Particles;

public class Particle
{
	protected readonly Random Random;

	public Point3D Position;
	public double Rotation;
	public double Size;
	public double Life;
	public double Decay;
	public double StartLife;
	public double StartSize;
	public Vector3D Velocity;

	public Particle(Random random)
	{
		Random = random;
	}

	public virtual void Update(double time, double dt)
	{
		Position += Velocity * dt;
		Life -= Decay * dt;
	}
}
