namespace TP.ConcurrentProgramming.Data
{
    /// <summary>
    ///  Two dimensions immutable vector
    /// </summary>
    internal record Vector : IVector
    {
        #region IVector

        public double x { get; init; }
        public double y { get; init; }

        #endregion IVector

        public Vector(double XComponent, double YComponent)
        {
            x = XComponent;
            y = YComponent;
        }

        // 🔽 Operatory arytmetyczne
        public static Vector operator +(Vector a, Vector b) => new(a.x + b.x, a.y + b.y);
        public static Vector operator -(Vector a, Vector b) => new(a.x - b.x, a.y - b.y);
        public static Vector operator *(Vector v, double scalar) => new(v.x * scalar, v.y * scalar);
    }
}
