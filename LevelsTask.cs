using System;
using System.Collections.Generic;

namespace func_rocket
{
	public interface ILevel
	{
		Level CreateLevel(Physics physics);
	}

	public abstract class LevelConstructor : ILevel
	{
		public string Name { get; set; }
		public Vector RocketPosition { get; set; }
		public Vector TargetPosition { get; set; }
		public Func<double, double> Amplitude;
		public Func<Vector> GForcePoint;
		public Func<Vector, Vector> GForceDirection;
		public Gravity G;

		public abstract Level CreateLevel(Physics physics);
	}

	public class LevelsTask
	{
		static readonly Physics standardPhysics = new Physics();

		public static IEnumerable<Level> CreateLevels()
		{
			yield return new ZeroLevel().CreateLevel(standardPhysics);
			yield return new HeavyLevel().CreateLevel(standardPhysics);
			yield return new UpLevel().CreateLevel(standardPhysics);
			yield return new WhiteHoleLevel().CreateLevel(standardPhysics);
			yield return new BlackHoleLevel().CreateLevel(standardPhysics);
			yield return new BlackAndWhiteLevel().CreateLevel(standardPhysics);
		}
	}

	public class ZeroLevel : LevelConstructor
	{
		public override Level CreateLevel(Physics physics) => new Level("Zero",         //Name
				new Rocket(new Vector(200, 500), Vector.Zero, -0.5 * Math.PI),  //Rocket
				new Vector(600, 200),                                           //Blackhole position
				(size, v) => Vector.Zero,                                       //Gravity
				physics);                                                       //Phisics
	}

	public class HeavyLevel : LevelConstructor
	{
		public override Level CreateLevel(Physics physics) => new Level("Heavy",
				new Rocket(new Vector(200, 500), Vector.Zero, -0.5 * Math.PI),
				new Vector(600, 200),
				(size, v) => new Vector(0, 0.9),
				physics);
	}

	public class UpLevel : LevelConstructor
	{
		public override Level CreateLevel(Physics physics) => new Level("Up",           //Name
				new Rocket(new Vector(200, 500), Vector.Zero, -0.5 * Math.PI),  //Rocket
				new Vector(700, 500),                                           //Blackhole position
				(size, v) => new Vector(0, -300 / (size.Height - v.Y + 300.0)), //G-Force                                       
				physics);                                                       //Phisics
	}

	public class WhiteHoleLevel : LevelConstructor
	{
		BlackHoleLevel bh;

		public WhiteHoleLevel()
		{
			bh = new BlackHoleLevel();
			bh.Name = "WhiteHole";
			bh.Amplitude = d => 140 * d / (d * d + 1);
			bh.GForcePoint = () => bh.TargetPosition;
			bh.GForceDirection = v => v - bh.GForcePoint();
			G = bh.G;
		}

		public override Level CreateLevel(Physics physics)
		{
			return bh.CreateLevel(physics);
		}
	}

	public class BlackHoleLevel : LevelConstructor
	{
		public BlackHoleLevel()
		{
			Name = "BlackHole";
			RocketPosition = new Vector(200, 500);
			TargetPosition = new Vector(700, 500);
			Amplitude = d => 300 * d / (d * d + 1);
			GForcePoint = () => RocketPosition + (TargetPosition - RocketPosition) * 0.5;
			GForceDirection = v => GForcePoint() - v;
			G = (size, v) =>
			{
				Vector direction = GForceDirection(v);
				double d = direction.Length;
				direction = direction.Normalize();
				direction *= Amplitude(d);
				return direction;
			};
		}

		public override Level CreateLevel(Physics physics) => new Level(Name,     //Name
				new Rocket(RocketPosition, Vector.Zero, -0.5 * Math.PI),        //Rocket
				TargetPosition,                                                 //Blackhole position
				G,                                                              //G-Force                                       
				physics);                                                       //Phisics
	}

	public class BlackAndWhiteLevel : LevelConstructor
	{
		public override Level CreateLevel(Physics physics)
		{
			var bh = new BlackHoleLevel();
			var wh = new WhiteHoleLevel();
			G = (size, v) => (bh.G(size, v) + wh.G(size, v)) * 0.5;
			return new Level(
				"BlackAndWhite",
				new Rocket(bh.RocketPosition, Vector.Zero, -0.5 * Math.PI),
				bh.TargetPosition,
				G,
				physics);
		}
	}
}