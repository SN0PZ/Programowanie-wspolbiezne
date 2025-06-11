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
using System.Threading;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall, IDisposable
    {
        private const double SpeedMultiplier = 3.0;

        private readonly CancellationTokenSource _cts = new();
        private readonly ManualResetEvent _tickEvent;

        private Vector _position;
        private Vector _velocity;

        public event EventHandler<IVector>? NewPositionNotification;
        public double Mass { get; }
        public IVector Velocity
        {
            get => new Vector(_velocity.x, _velocity.y);
            set => _velocity = new Vector(value.x, value.y);
        }

        internal Ball(
            Vector initialPosition,
            Vector initialVelocity,
            double mass,
            ManualResetEvent tickEvent)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            _tickEvent = tickEvent;

            var thread = new Thread(RunLoop)
            {
                IsBackground = true,
                Name = $"Ball Thread {this.GetHashCode()}" 
            };
            thread.Start();
        }

        private void RunLoop()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    _tickEvent.WaitOne();

                    if (!_cts.Token.IsCancellationRequested)
                    {
                        Move(_velocity);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
            }
        }

        internal void Move(Vector delta)
        {
            _position = new Vector(
                _position.x + delta.x * SpeedMultiplier,
                _position.y + delta.y * SpeedMultiplier);
            NewPositionNotification?.Invoke(this, _position);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}