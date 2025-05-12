namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal struct Vector
    {
        public double x { get; init; }
        public double y { get; init; }

        public Vector(double x, double y) => (this.x, this.y) = (x, y);

        public double Dot(Vector other) => x * other.x + y * other.y;
        public static Vector operator -(Vector v)
            => new Vector(-v.x, -v.y);

        public static Vector operator +(Vector a, Vector b)
            => new Vector(a.x + b.x, a.y + b.y);

        public static Vector operator -(Vector a, Vector b)
            => new Vector(a.x - b.x, a.y - b.y);

        public static Vector operator *(Vector v, double s)
            => new Vector(v.x * s, v.y * s);

        public static Vector operator *(double s, Vector v)
            => v * s;
    }

    internal class BallState
    {
        public Data.IBall Underlying { get; }
        public Vector Position { get; set; }
        public Vector Velocity { get; set; }
        public double Mass => Underlying.Mass;
        public BallState(Data.IBall b, Vector pos, Vector vel)
        {
            Underlying = b; Position = pos; Velocity = vel;
        }
    }

    internal static class PhysicsEngine
    {
        public static void Step(
          List<BallState> balls,
          double tableWidth,
          double tableHeight,
          double ballDiameter
        )
        {
            int n = balls.Count;
            // 1) zderzenia kula–kula
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    var A = balls[i];
                    var B = balls[j];
                    var delta = A.Position - B.Position;
                    double dist2 = delta.Dot(delta);
                    double minDist = ballDiameter;
                    if (dist2 <= minDist * minDist)
                    {
                        // elastic collision
                        var m1 = A.Mass; var m2 = B.Mass;
                        var v1 = A.Velocity; var v2 = B.Velocity;
                        A.Velocity = v1
                          - (2 * m2 / (m1 + m2))
                            * ((v1 - v2).Dot(delta) / dist2)
                            * delta;
                        B.Velocity = v2
                          - (2 * m1 / (m1 + m2))
                            * ((v2 - v1).Dot(-delta) / dist2)
                            * (-delta);
                    }
                }
            }
            // 2) ruch + odbicia od ścian
            foreach (var s in balls)
            {
                var newPos = s.Position + s.Velocity;
                var vx = s.Velocity.x;
                var vy = s.Velocity.y;
                if (newPos.x <= 0 || newPos.x >= tableWidth - ballDiameter)
                {
                    vx = -vx;
                    newPos = new Vector(
                      Math.Clamp(newPos.x, 0, tableWidth - ballDiameter),
                      newPos.y);
                }
                if (newPos.y <= 0 || newPos.y >= tableHeight - ballDiameter)
                {
                    vy = -vy;
                    newPos = new Vector(
                      newPos.x,
                      Math.Clamp(newPos.y, 0, tableHeight - ballDiameter));
                }
                s.Velocity = new Vector(vx, vy);
                s.Position = newPos;
            }
        }
    }
}