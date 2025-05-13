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

namespace TP.ConcurrentProgramming.Data
{
    internal class DataImplementation : DataAbstractAPI
    {
        #region ctor
        public DataImplementation() { }
        #endregion

        #region DataAbstractAPI

        const double MinMass = 0.5;
        const double MaxMass = 2.0;
        const double MinDiam = 10.0;
        const double MaxDiam = 20.0;

        private static double MassToDiameter(double mass)
            => MinDiam + (mass - MinMass) / (MaxMass - MinMass) * (MaxDiam - MinDiam);

        private static double MassToRadius(double mass)
            => MassToDiameter(mass) / 2;

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            TableWidth = tableWidth;
            TableHeight = tableHeight;

            Random random = new Random();
            BallsList.Clear();

            for (int i = 0; i < numberOfBalls; i++)
            {
                // wygeneruj masę i promień
                double mass = MinMass + random.NextDouble() * (MaxMass - MinMass);
                double radius = MassToRadius(mass);

                // znajdź pozycję, która nie zachodzi na inne kule
                Vector pos;
                bool ok;
                do
                {
                    ok = true;
                    double x = random.NextDouble() * (TableWidth - 2 * radius) + radius;
                    double y = random.NextDouble() * (TableHeight - 2 * radius) + radius;
                    pos = new Vector(x, y);

                    foreach (var other in BallsList)
                    {
                        double otherR = MassToRadius(other.Mass);
                        if ((pos - other.Position).Length < radius + otherR)
                        {
                            ok = false;
                            break;
                        }
                    }
                } while (!ok);

                // prędkość
                Vector vel = new Vector((random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2);

                // utwórz kulę
                var newBall = new Ball(pos, vel, mass);
                BallsList.Add(newBall);
                upperLayerHandler(pos, newBall);
            }
        }

        public override void AddBall(Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();

            // wygeneruj masę i promień
            double mass = MinMass + random.NextDouble() * (MaxMass - MinMass);
            double radius = MassToRadius(mass);

            // znajdź pozycję bez zachodzenia
            Vector pos;
            bool ok;
            do
            {
                ok = true;
                double x = random.NextDouble() * (TableWidth - 2 * radius) + radius;
                double y = random.NextDouble() * (TableHeight - 2 * radius) + radius;
                pos = new Vector(x, y);

                foreach (var other in BallsList)
                {
                    double otherR = MassToRadius(other.Mass);
                    if ((pos - other.Position).Length < radius + otherR)
                    {
                        ok = false;
                        break;
                    }
                }
            } while (!ok);

            // prędkość
            Vector vel = new Vector((random.NextDouble() - 0.5) * 2, (random.NextDouble() - 0.5) * 2);

            // utwórz kulę
            var newBall = new Ball(pos, vel, mass);
            BallsList.Add(newBall);
            upperLayerHandler(pos, newBall);
        }

        public override void RemoveLastBall()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (BallsList.Count > 0)
                BallsList.RemoveAt(BallsList.Count - 1);
        }


        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                    BallsList.Clear();
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void MoveBall(IBall ball, IVector delta)
        {
            var b = (Ball)ball;
            b.Move(new Vector(delta.x, delta.y));
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region private

        private bool Disposed = false;
        private List<Ball> BallsList = new();
        private double TableWidth;
        private double TableHeight;

        #endregion

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
            => returnBallsList(BallsList);

        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
            => returnNumberOfBalls(BallsList.Count);

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
            => returnInstanceDisposed(Disposed);

        #endregion
    }
}