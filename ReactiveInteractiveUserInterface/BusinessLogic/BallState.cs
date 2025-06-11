namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal struct Vector
    {
        public double x { get; set; }
        public double y { get; set; }

        public Vector(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

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
        const double MIN_BOUNCE_SPEED = 0.1;

        public static void Step(
            List<BallState> balls,
            double tableWidth,
            double tableHeight
        )
        {
            int n = balls.Count;

            // 1) Kolizje kula–kula (separacja + impuls)
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    var A = balls[i];
                    var B = balls[j];
                    var delta = A.Position - B.Position;
                    double dist2 = delta.Dot(delta);
                    double minDist = A.Radius + B.Radius;

                    if (dist2 < minDist * minDist)
                    {
                        double dist = Math.Sqrt(dist2);
                        if (dist == 0)
                        {
                            dist = minDist - 1e-6;
                            delta = new Vector(minDist, 0);
                        }

                        var normal = delta * (1.0 / dist);

                        double penetration = minDist - dist;
                        double totalMass = A.Mass + B.Mass;
                        A.Position += normal * (penetration * (B.Mass / totalMass));
                        B.Position -= normal * (penetration * (A.Mass / totalMass));

                        var v1 = A.Velocity;
                        var v2 = B.Velocity;
                        double vRel = (v1 - v2).Dot(normal);
                        if (vRel < 0)
                        {
                            double impulse = (2 * vRel) / totalMass;
                            A.Velocity = v1 - impulse * B.Mass * normal;
                            B.Velocity = v2 + impulse * A.Mass * normal;
                        }
                    }
                }
            }

            // 2) Ruch + odbicia od ścian (overshoot-reflect + minimal bounce)
            foreach (var s in balls)
            {
                var newPos = s.Position + s.Velocity;
                double vx = s.Velocity.x;
                double vy = s.Velocity.y;

                double minX = s.Radius;
                double maxX = tableWidth - s.Radius;
                double minY = s.Radius;
                double maxY = tableHeight - s.Radius;

                if (newPos.x < minX)
                {
                    double over = minX - newPos.x;
                    newPos.x = minX + over;
                    vx = -vx;
                    if (Math.Abs(vx) < MIN_BOUNCE_SPEED)
                        vx = Math.Sign(vx) * MIN_BOUNCE_SPEED;
                }
                else if (newPos.x > maxX)
                {
                    double over = newPos.x - maxX;
                    newPos.x = maxX - over;
                    vx = -vx;
                    if (Math.Abs(vx) < MIN_BOUNCE_SPEED)
                        vx = Math.Sign(vx) * MIN_BOUNCE_SPEED;
                }

                if (newPos.y < minY)
                {
                    double over = minY - newPos.y;
                    newPos.y = minY + over;
                    vy = -vy;
                    if (Math.Abs(vy) < MIN_BOUNCE_SPEED)
                        vy = Math.Sign(vy) * MIN_BOUNCE_SPEED;
                }
                else if (newPos.y > maxY)
                {
                    double over = newPos.y - maxY;
                    newPos.y = maxY - over;
                    vy = -vy;
                    if (Math.Abs(vy) < MIN_BOUNCE_SPEED)
                        vy = Math.Sign(vy) * MIN_BOUNCE_SPEED;
                }

                s.Position = newPos;
                s.Velocity = new Vector(vx, vy);
            }
        }
    }
}
