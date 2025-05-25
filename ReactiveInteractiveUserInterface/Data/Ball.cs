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
        private readonly double _tableW, _tableH;
        private readonly CancellationTokenSource _cts = new();
        private const int FrameTime = 20;

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
            double tableWidth,
            double tableHeight)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            _tableW = tableWidth;
            _tableH = tableHeight;

            var thread = new Thread(RunLoop) { IsBackground = true };
            thread.Start();
        }

        private void RunLoop()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    Move(_velocity);
                    Thread.Sleep(FrameTime);
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        internal void Move(Vector delta)
        {
            _position = new Vector(
                _position.x + delta.x,
                _position.y + delta.y);
            NewPositionNotification?.Invoke(this, _position);
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
