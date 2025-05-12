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
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void MoveBall(IBall ball, IVector delta)
        {
            var b = (Ball)ball;
            var dv = new Vector(delta.x, delta.y);
            b.Move(dv);
        }

        public override void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable

        #region private

        private bool Disposed = false;
        private Random RandomGenerator = new();
        private List<Ball> BallsList = new();

        private double TableWidth;
        private double TableHeight;
        private const double BallRadius = 10;

        
        public override void AddBall(Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            bool positionIsValid;

            Vector startingPosition = null;

            do
            {
                double startX = random.NextDouble() * (TableWidth - 2 * BallRadius);
                double startY = random.NextDouble() * (TableHeight - 2 * BallRadius);
                startingPosition = new Vector(startX, startY);
                positionIsValid = true;

                foreach (var ball in BallsList)
                {
                    double distance = (startingPosition - ball.Position).Length;
                    if (distance < 2 * BallRadius) 
                    {
                        positionIsValid = false;
                        break;
                    }
                }
            }
            while (!positionIsValid);

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