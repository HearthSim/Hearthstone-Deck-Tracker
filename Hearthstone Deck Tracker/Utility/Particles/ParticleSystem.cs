using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Hearthstone_Deck_Tracker.Utility.Particles;

public class ParticleSystem
{
	private readonly Func<Particle> _createParticle;
	public int MaxParticles { get; }
	public readonly GeometryModel3D ParticleModel = new() { Geometry = new MeshGeometry3D() };
	private readonly List<Particle> _particles = new();

	public ParticleSystem(Visual particleVisual, int particleSize, Func<Particle> createParticle, int maxParticles)
	{
		_createParticle = createParticle;
		MaxParticles = maxParticles;

		var renderTarget = new RenderTargetBitmap(particleSize, particleSize, 96, 96, PixelFormats.Pbgra32);
		renderTarget.Render(particleVisual);
		renderTarget.Freeze();
		var brush = new ImageBrush(renderTarget);

		ParticleModel.Material = new DiffuseMaterial(brush);
	}


	public void Update(double time, double dt)
	{
		var dead = new List<Particle>();
		foreach(var p in _particles)
		{
			p.Update(time, dt);
			if(p.Life <= 0)
				dead.Add(p);
		}

		foreach(var particle in dead)
			_particles.Remove(particle);

		UpdateGeometry();

	}

	public void SpawnParticle(Point3D position)
	{
		if(_particles.Count >= MaxParticles)
			return;
		var particle = _createParticle();
		particle.Position = position;
		_particles.Add(particle);
	}

	private void UpdateGeometry()
	{
		var positions = new Point3DCollection(_particles.Count * 4);
		var indices = new Int32Collection(_particles.Count * 6);
		var texcoords = new PointCollection(_particles.Count * 4);

		for(var i = 0; i < _particles.Count; ++i)
		{
			var p = _particles[i];


			var rotate = new RotateTransform3D
			{
				Rotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), p.Rotation),
				CenterX = p.Position.X,
				CenterY = p.Position.Y,
			};

			var p1 = new Point3D(p.Position.X - p.Size / 2, p.Position.Y - p.Size / 2, p.Position.Z);
			var p2 = new Point3D(p.Position.X - p.Size / 2, p.Position.Y + p.Size / 2, p.Position.Z);
			var p3 = new Point3D(p.Position.X + p.Size / 2, p.Position.Y + p.Size / 2, p.Position.Z);
			var p4 = new Point3D(p.Position.X + p.Size / 2, p.Position.Y - p.Size / 2, p.Position.Z);

			positions.Add(rotate.Transform(p1));
			positions.Add(rotate.Transform(p2));
			positions.Add(rotate.Transform(p3));
			positions.Add(rotate.Transform(p4));

			texcoords.Add(new Point(0.0, 0.0));
			texcoords.Add(new Point(0.0, 1.0));
			texcoords.Add(new Point(1.0, 1.0));
			texcoords.Add(new Point(1.0, 0.0));

			var positionIndex = i * 4;
			indices.Add(positionIndex);
			indices.Add(positionIndex + 2);
			indices.Add(positionIndex + 1);
			indices.Add(positionIndex);
			indices.Add(positionIndex + 3);
			indices.Add(positionIndex + 2);
		}

		((MeshGeometry3D)ParticleModel.Geometry).Positions = positions;
		((MeshGeometry3D)ParticleModel.Geometry).TriangleIndices = indices;
		((MeshGeometry3D)ParticleModel.Geometry).TextureCoordinates = texcoords;
	}

}
