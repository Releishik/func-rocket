using System;

namespace func_rocket
{
	public class ControlTask
	{
		public static Turn ControlRocket(Rocket rocket, Vector target)
		{
			Vector v = new Vector(1, 0).Rotate(rocket.Direction);
			v = (rocket.Velocity + 5 * v).Normalize();
			Vector d = target - rocket.Location;

			var ang = GetAngle(v, d);

			return ang == 0 ? Turn.None : ang < 0 ? Turn.Right : Turn.Left;
		}

		static double GetAngle(Vector from, Vector too)
		{
			var a = from.Normalize();
			var b = too.Normalize();
			double cos = a.X * b.X + a.Y * b.Y;
			double sin = a.X * b.Y - b.X * a.Y;
			if (sin == 0) sin = CheckIsOpposite(a, b);
			int rotDir = sin < 0 ? 1 : -1;
			return Math.Acos(cos) * rotDir;
		}

		static int CheckIsOpposite(Vector v1, Vector v2) => v1.X * v2.X < 0 && v1.Y * v2.Y < 0 ? -1 : 1;
	}
}