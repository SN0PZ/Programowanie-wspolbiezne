//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            // Zmniejszona wartość interwału dla płynniejszego ruchu
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();

            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(random.Next(100, 300), random.Next(100, 300));
                Vector randomVelocity = new(random.Next(-3, 4), random.Next(-3, 4));

                // Upewniamy się, że piłka ma prędkość różną od zera
                if (randomVelocity.x == 0 && randomVelocity.y == 0)
                {
                    randomVelocity = new(1, 1);
                }

                Ball newBall = new(startingPosition, randomVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
        }

        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    MoveTimer.Dispose();
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private readonly Timer MoveTimer;
        private readonly Random RandomGenerator = new();
        private readonly List<Ball> BallsList = [];

        private void Move(object? x)
        {
            const double CanvasWidth = 400;
            const double CanvasHeight = 420;
            const double BarrierY = 420;
            const double Diameter = 20;

            foreach (Ball item in BallsList)
            {
                Vector position = (Vector)item.PositionReadOnly;
                Vector velocity = (Vector)item.Velocity;

                Vector newPosition = position + velocity;

                // Odbicie od lewej/prawej ściany
                if ((newPosition.x <= 0 && velocity.x < 0) || (newPosition.x + Diameter >= CanvasWidth && velocity.x > 0))
                {
                    velocity = new Vector(-velocity.x, velocity.y); // Odbicie poziome
                }

                // Odbicie od sufitu
                if (newPosition.y <= 0 && velocity.y < 0)
                {
                    velocity = new Vector(velocity.x, -velocity.y); // Odbicie od góry
                }

                // Odbicie od dolnej granicy
                if (newPosition.y + Diameter >= CanvasHeight && velocity.y > 0)
                {
                    velocity = new Vector(velocity.x, -velocity.y); // Odbicie od dołu
                    newPosition = new Vector(newPosition.x, CanvasHeight - Diameter); // Zapewniamy, że piłka nie wyjdzie poza granicę
                }

                // Odbicie od poziomej bariery
                if (position.y + Diameter <= BarrierY && newPosition.y + Diameter >= BarrierY && velocity.y > 0)
                {
                    velocity = new Vector(velocity.x, -velocity.y); // Odbicie od poziomej bariery
                    newPosition = new Vector(newPosition.x, BarrierY - Diameter); // Zapewniamy, że piłka zatrzyma się na barierze
                }

                // Zapisujemy nową prędkość i wykonujemy ruch
                item.Velocity = velocity;
                Vector delta = newPosition - position;
                item.Move(delta);
            }
        }


        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
        {
            returnBallsList(BallsList);
        }

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
        {
            returnNumberOfBalls(BallsList.Count);
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}
