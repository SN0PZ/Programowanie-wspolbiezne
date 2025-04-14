namespace TP.ConcurrentProgramming.Data
{
    internal class Boundary
    {
        private readonly double minX;
        private readonly double maxX;
        private readonly double minY;
        private readonly double maxY;

        public Boundary(double minX, double maxX, double minY, double maxY)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
        }

        // Sprawdza i koryguje pozycjê oraz prêdkoœæ pi³ki, jeœli wykracza poza granice
        public Vector ConstrainPosition(Vector position, ref Vector velocity)
        {
            double newX = position.x;
            double newY = position.y;
            var newVelocity = velocity;

            // Sprawdzanie i odbijanie od granic X
            if (position.x <= minX)
            {
                newX = minX;
                newVelocity = new Vector(-velocity.x, velocity.y); // Odbicie od lewej œciany
            }
            else if (position.x >= maxX)
            {
                newX = maxX;
                newVelocity = new Vector(-velocity.x, velocity.y); // Odbicie od prawej œciany
            }

            // Sprawdzanie i odbijanie od granic Y
            if (position.y <= minY)
            {
                newY = minY;
                newVelocity = new Vector(velocity.x, -velocity.y); // Odbicie od górnej œciany
            }
            else if (position.y >= maxY)
            {
                newY = maxY;
                newVelocity = new Vector(velocity.x, -velocity.y); // Odbicie od dolnej œciany
            }

            velocity = newVelocity;
            return new Vector(newX, newY);
        }
    }
}
