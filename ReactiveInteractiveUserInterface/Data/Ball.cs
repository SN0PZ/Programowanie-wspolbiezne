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
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace TP.ConcurrentProgramming.Data
{
    internal class Ball : IBall, IDisposable
    {
        private readonly double _tableW, _tableH;
        private readonly CancellationTokenSource _cts = new();
        private readonly Thread _thread;
        private const int FrameTime = 20; 

        private Vector _position;
        private Vector _velocity;

        private readonly string _id;
        private readonly BallLogger _logger;

        public event EventHandler<IVector>? NewPositionNotification;
        public double Mass { get; }
        public IVector Velocity
        {
            get => new Vector(_velocity.x, _velocity.y);
            set => _velocity = new Vector(value.x, value.y);
        }

        internal Ball(Vector initialPosition, Vector initialVelocity, double mass, double tableWidth, double tableHeight)
        {
            _position = initialPosition;
            _velocity = initialVelocity;
            Mass = mass;
            _tableW = tableWidth;
            _tableH = tableHeight;

            _id = Guid.NewGuid().ToString("N");
            _logger = new BallLogger(_id);

            _thread = new Thread(RunLoop)
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            _thread.Start();
        }

        private void RunLoop()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                   
                    Move(_velocity);

                    sw.Stop();
                    _logger.Log(sw.ElapsedTicks, _position, _velocity);

                    Thread.Sleep(FrameTime);
                }
            }
            finally
            {
                _logger.Shutdown();
            }
        }

        internal void Move(Vector delta)
        {
            double nx = _position.x + delta.x;
            double ny = _position.y + delta.y;
            nx = Math.Clamp(nx, 0, _tableW);
            ny = Math.Clamp(ny, 0, _tableH);
            _position = new Vector(nx, ny);

            NewPositionNotification?.Invoke(this, _position);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _thread.Join();
        }
    }


    internal class BallLogger
    {
        private readonly BlockingCollection<string> _queue = new();
        private readonly Thread _logThread;
        private readonly StreamWriter _writer;

        public BallLogger(string id)
        {
            var path = $"ball_{id}.log";
            _writer = new StreamWriter(path, false) { AutoFlush = true };
            _logThread = new Thread(() =>
            {
                foreach (var line in _queue.GetConsumingEnumerable())
                    _writer.WriteLine(line);
                _writer.Close();
            })
            {
                IsBackground = true
            };
            _logThread.Start();
        }

        public void Log(long ticks, Vector pos, Vector vel)
        {
            var entry = $"{DateTime.Now:O},{ticks},{pos.x:F2},{pos.y:F2},{vel.x:F2},{vel.y:F2}";
            _queue.Add(entry);
        }

        public void Shutdown()
        {
            _queue.CompleteAdding();
            _logThread.Join();
        }
    }
}
