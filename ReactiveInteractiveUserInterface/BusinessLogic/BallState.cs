using System;
using System.Collections.Generic;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal struct Vector
    {
        public double x { get; init; }
        public double y { get; init; }
        public Vector(double x, double y) => (this.x, this.y) = (x, y);
        public double Dot(Vector other) => x * other.x + y * other.y;
        public static Vector operator -(Vector v) => new(-v.x, -v.y);
        public static Vector operator +(Vector a, Vector b) => new(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => new(a.x - b.x, a.y - b.y);
        public static Vector operator *(Vector v, double s) => new(v.x * s, v.y * s);
        public static Vector operator *(double s, Vector v) => v * s;
    }

    internal class BallState
    {
        public Data.IBall Underlying { get; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
        public double Mass => Underlying.Mass;
        public double Radius { get; }

        public BallState(Data.IBall b, Vector pos, Vector vel, double radius)
        {
            Underlying = b;
            Position = pos;
            Velocity = vel;
            Radius = radius;
        }
    }

    internal static class PhysicsEngine
    {
        public static void Step(
            List<BallState> balls,
            double tableWidth,
            double tableHeight
        )
        {
            int n = balls.Count;
            // 1) kolizje kula–kula
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    var A = balls[i];
                    var B = balls[j];
                    var d = A.Position - B.Position;
                    double dist2 = d.Dot(d);
                    double minDist = A.Radius + B.Radius;
                    if (dist2 <= minDist * minDist)
                    {
                        // elastic collision
                        var m1 = A.Mass; var m2 = B.Mass;
                        var v1 = A.Velocity; var v2 = B.Velocity;
                        A.Velocity = v1
                          - (2 * m2 / (m1 + m2))
                            * ((v1 - v2).Dot(d) / dist2)
                            * d;
                        B.Velocity = v2
                          - (2 * m1 / (m1 + m2))
                            * ((v2 - v1).Dot(-d) / dist2)
                            * (-d);
                    }
                }
            }
            // 2) ruch + odbicia od ścian
            foreach (var s in balls)
            {
                var np = s.Position + s.Velocity;
                var vx = s.Velocity.x;
                var vy = s.Velocity.y;

                if (np.x <= s.Radius || np.x >= tableWidth - s.Radius)
                {
                    vx = -vx;
                    np = new Vector(
                        Math.Clamp(np.x, s.Radius, tableWidth - s.Radius),
                        np.y);
                }
                if (np.y <= s.Radius || np.y >= tableHeight - s.Radius)
                {
                    vy = -vy;
                    np = new Vector(
                        np.x,
                        Math.Clamp(np.y, s.Radius, tableHeight - s.Radius));
                }
                s.Velocity = new Vector(vx, vy);
                s.Position = np;
            }
        }
    }
}
