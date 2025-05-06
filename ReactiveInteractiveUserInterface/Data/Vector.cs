//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
    internal record Vector : IVector
    {
        public double x { get; init; }
        public double y { get; init; }

        public Vector(double XComponent, double YComponent)
        {
            x = XComponent;
            y = YComponent;
        }

        public double Length => Math.Sqrt(x * x + y * y);

        // odejmowanie
        public static Vector operator -(Vector a, Vector b)
            => new Vector(a.x - b.x, a.y - b.y);

        public static Vector operator -(Vector v)
        => new Vector(-v.x, -v.y);

        // dodawanie
        public static Vector operator +(Vector a, Vector b)
            => new Vector(a.x + b.x, a.y + b.y);

        // mnożenie przez skalar
        public static Vector operator *(Vector v, double s)
            => new Vector(v.x * s, v.y * s);
        public static Vector operator *(double s, Vector v)
            => v * s;

        // iloczyn skalarny
        public double Dot(Vector other)
            => x * other.x + y * other.y;
    }
}
