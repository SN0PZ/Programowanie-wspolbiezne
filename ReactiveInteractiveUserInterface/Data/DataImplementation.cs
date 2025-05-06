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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor

        public DataImplementation()
        {
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(5));
        }

        #endregion ctor

        #region DataAbstractAPI

        const double MinMass = 0.5;
        const double MaxMass = 2.0;

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler)
        {
            this.TableWidth = tableWidth;
            this.TableHeight = tableHeight;

            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                double startX = random.NextDouble() * (TableWidth - 2 * BallRadius);
                double startY = random.NextDouble() * (TableHeight - 2 * BallRadius);

                Vector startingPosition = new(startX, startY);
                Vector velocity = new Vector((random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2);

                double mass = MinMass + RandomGenerator.NextDouble() * (MaxMass - MinMass);
                Ball newBall = new(startingPosition, velocity, mass);
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
        private Random RandomGenerator = new();
        private List<Ball> BallsList = new();

        private double TableWidth;
        private double TableHeight;
        private const double BallRadius = 10;

        private void Move(object? _)
        {
            lock (BallsList) // chronimy dostęp do listy
            {
                // 1) obsłuż zderzenia kula–kula
                int n = BallsList.Count;
                for (int i = 0; i < n; i++)
                {
                    for (int j = i + 1; j < n; j++)
                    {
                        var A = BallsList[i];
                        var B = BallsList[j];

                        // wektory położeń
                        var x1 = A.Position;
                        var x2 = B.Position;
                        var delta = x1 - x2;
                        double dist2 = delta.Dot(delta);
                        double minDist = 2 * BallRadius;
                        if (dist2 <= minDist * minDist)
                        {
                            // zachowujemy oryginalne prędkości
                            var v1 = A.Velocity as Vector;
                            var v2 = B.Velocity as Vector;
                            double m1 = A.Mass, m2 = B.Mass;

                            // wzory dla sprężystego zderzenia
                            var v1Prime = v1 - (2 * m2 / (m1 + m2))
                                          * ((v1 - v2).Dot(delta) / dist2)
                                          * delta;
                            var v2Prime = v2 - (2 * m1 / (m1 + m2))
                                          * ((v2 - v1).Dot(-delta) / dist2)
                                          * (-delta);

                            A.Velocity = v1Prime;
                            B.Velocity = v2Prime;
                        }
                    }
                }

                // 2) dotychczasowa obsługa ruchu i odbić od ścian
                foreach (Ball ball in BallsList.ToList())
                {
                    Vector newPos = (Vector)ball.Position + (Vector)ball.Velocity;
                    double newVelX = ball.Velocity.x;
                    double newVelY = ball.Velocity.y;

                    // odbicie od pionowych ścian
                    if (newPos.x <= 0 || newPos.x >= TableWidth - 2 * BallRadius)
                    {
                        newVelX *= -1;
                        newPos = new Vector(
                            Math.Clamp(newPos.x, 0, TableWidth - 2 * BallRadius),
                            newPos.y
                        );
                    }
                    // odbicie od poziomych
                    if (newPos.y <= 0 || newPos.y >= TableHeight - 2 * BallRadius)
                    {
                        newVelY *= -1;
                        newPos = new Vector(
                            newPos.x,
                            Math.Clamp(newPos.y, 0, TableHeight - 2 * BallRadius)
                        );
                    }

                    ball.Velocity = new Vector(newVelX, newVelY);
                    ball.Move(newPos - ball.Position);
                }
            }
        }


        public override void AddBall(Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            double startX = random.NextDouble() * (TableWidth - 2 * BallRadius);
            double startY = random.NextDouble() * (TableHeight - 2 * BallRadius);
            Vector startingPosition = new(startX, startY);
            Vector velocity = new((random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2);

            double mass = MinMass + RandomGenerator.NextDouble() * (MaxMass - MinMass);
            Ball newBall = new(startingPosition, velocity, mass);
            BallsList.Add(newBall);
            upperLayerHandler(startingPosition, newBall);
        }

        public override void RemoveLastBall()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (BallsList.Count > 0)
                BallsList.RemoveAt(BallsList.Count - 1);
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