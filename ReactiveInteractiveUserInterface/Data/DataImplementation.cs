﻿// Plik: Data/DataImplementation.cs

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
        private readonly List<Ball> _balls = new();
        private bool _disposed;
        private double _tableW, _tableH;
        private readonly Random _rnd = new();

        private ManualResetEvent? _tickEvent;

        public override void SetTickEvent(ManualResetEvent tickEvent)
        {
            _tickEvent = tickEvent;
        }

        public override void Start(
            int numberOfBalls,
            double tableWidth,
            double tableHeight,
            Action<IVector, IBall> creationHandler)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            if (creationHandler == null) throw new ArgumentNullException(nameof(creationHandler));

            _tableW = tableWidth;
            _tableH = tableHeight;

            foreach (var b in _balls) b.Dispose();
            _balls.Clear();

            for (int i = 0; i < numberOfBalls; i++)
                CreateAndRegisterBall(creationHandler);
        }

        public override void AddBall(Action<IVector, IBall> creationHandler)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            if (creationHandler == null) throw new ArgumentNullException(nameof(creationHandler));

            CreateAndRegisterBall(creationHandler);
        }

        private void CreateAndRegisterBall(Action<IVector, IBall> creationHandler)
        {
            if (_tickEvent == null)
            {
                throw new InvalidOperationException("TickEvent has not been set by the Logic layer before creating a ball.");
            }

            double mass = 0.5 + _rnd.NextDouble() * 1.5;
            double radius = MassToRadius(mass);

            double x = _rnd.NextDouble() * (_tableW - 2 * radius) + radius;
            double y = _rnd.NextDouble() * (_tableH - 2 * radius) + radius;
            var pos = new Vector(x, y);

            const double speedFactor = 70.0;
            var vel = new Vector(
                (_rnd.NextDouble() - 0.5) * 2 * speedFactor,
                (_rnd.NextDouble() - 0.5) * 2 * speedFactor
            );


            var ball = new Ball(pos, vel, mass, _tickEvent);

            _balls.Add(ball);
            creationHandler(pos, ball);
        }

        public override void RemoveLastBall()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            if (_balls.Count == 0) return;

            var last = _balls[^1];
            _balls.RemoveAt(_balls.Count - 1);
            last.Dispose();
        }

        public override void MoveBall(IBall ball, IVector delta)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DataImplementation));
            ((Ball)ball).Move(new Vector(delta.x, delta.y));
        }

        public override void Dispose()
        {
            if (_disposed) return;
            foreach (var b in _balls) b.Dispose();
            _balls.Clear();
            _disposed = true;
        }

        #region TestingInstrumentation (DEBUG)
        [Conditional("DEBUG")]
        internal void CheckBallsList(Action<IEnumerable<IBall>> cb) => cb(_balls);
        [Conditional("DEBUG")]
        internal void CheckNumberOfBalls(Action<int> cb) => cb(_balls.Count);
        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> cb) => cb(_disposed);
        #endregion

        #region Helpers
        private static double MassToDiameter(double mass) => 10.0 + (mass - 0.5) / (2.0 - 0.5) * (20.0 - 10.0);
        private static double MassToRadius(double mass) => MassToDiameter(mass) / 2.0;
        #endregion
    }
}