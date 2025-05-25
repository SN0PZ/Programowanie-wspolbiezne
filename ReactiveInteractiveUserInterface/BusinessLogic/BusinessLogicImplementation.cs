using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Data = TP.ConcurrentProgramming.Data;
using Underneath = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly object _lock = new();
        private readonly List<BallState> _states = new();
        private readonly Underneath _dataLayer;
        private CancellationTokenSource? _cts;
        private bool _disposed;
        private double _tableW, _tableH;

        private const double MinMass = 0.5, MaxMass = 2.0;
        private const double MinDiam = 10.0, MaxDiam = 20.0;
        private static double MassToDiameter(double m)
            => MinDiam + (m - MinMass) / (MaxMass - MinMass) * (MaxDiam - MinDiam);
        private static double MassToRadius(double m)
            => MassToDiameter(m) / 2;

        private struct RawVector : Data.IVector
        {
            public double x { get; }
            public double y { get; }
            public RawVector(double x, double y) => (this.x, this.y) = (x, y);
        }

        public BusinessLogicImplementation()
            : this(Underneath.GetDataLayer())
        { }

        internal BusinessLogicImplementation(Underneath dataLayer)
            => _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));

        public override void Start(
            int numberOfBalls,
            double tableWidth,
            double tableHeight,
            Action<IPosition, IBall> upperLayerHandler)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            _tableW = tableWidth;
            _tableH = tableHeight;

            lock (_lock)
            {
                _states.Clear();
            }

            _dataLayer.Start(
                numberOfBalls,
                tableWidth,
                tableHeight,
                (vec, dataBall) =>
                {
                    double r = MassToRadius(dataBall.Mass);
                    var state = new BallState(
                        dataBall,
                        new Vector(vec.x, vec.y),
                        new Vector(dataBall.Velocity.x, dataBall.Velocity.y),
                        r);

                    dataBall.NewPositionNotification += (_, v)
                        => state.Position = new Vector(v.x, v.y);

                    lock (_lock)
                    {
                        _states.Add(state);
                    }

                    upperLayerHandler(
                        new Position(state.Position.x, state.Position.y),
                        new Ball(dataBall));
                });

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => RunLoopAsync(_cts.Token));
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Step();
                    await Task.Delay(5, token);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void Step()
        {
            BallState[] snapshot;
            Vector[] oldPositions;

            lock (_lock)
            {
                snapshot = _states.ToArray();
                oldPositions = snapshot.Select(s => s.Position).ToArray();
            }

            if (snapshot.Length == 0)
                return;

            PhysicsEngine.Step(snapshot.ToList(), _tableW, _tableH);

            var deltas = new RawVector[snapshot.Length];
            var newVels = new RawVector[snapshot.Length];
            for (int i = 0; i < snapshot.Length; i++)
            {
                var st = snapshot[i];
                var dp = st.Position - oldPositions[i];
                deltas[i] = new RawVector(dp.x, dp.y);
                newVels[i] = new RawVector(st.Velocity.x, st.Velocity.y);
            }

            lock (_lock)
            {
                for (int i = 0; i < snapshot.Length; i++)
                {
                    var real = _states[i];
                    real.Position = snapshot[i].Position;
                    real.Velocity = snapshot[i].Velocity;
                }
            }

            for (int i = 0; i < snapshot.Length; i++)
            {
                var st = snapshot[i];
                _dataLayer.MoveBall(st.Underlying, deltas[i]);
                st.Underlying.Velocity = newVels[i];
            }
        }

        public override void AddBall(Action<IPosition, IBall> upperLayerHandler)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            _dataLayer.AddBall((vec, dataBall) =>
            {
                double r = MassToRadius(dataBall.Mass);
                var state = new BallState(
                    dataBall,
                    new Vector(vec.x, vec.y),
                    new Vector(dataBall.Velocity.x, dataBall.Velocity.y),
                    r);

                dataBall.NewPositionNotification += (_, v)
                    => state.Position = new Vector(v.x, v.y);

                lock (_lock)
                {
                    _states.Add(state);
                }

                upperLayerHandler(
                    new Position(state.Position.x, state.Position.y),
                    new Ball(dataBall));
            });
        }

        public override void RemoveLastBall()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            lock (_lock)
            {
                if (_states.Count > 0)
                    _states.RemoveAt(_states.Count - 1);
            }

            _dataLayer.RemoveLastBall();
        }

        public override void Dispose()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            _cts?.Cancel();
            _dataLayer.Dispose();
            _disposed = true;
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> cb) => cb(_disposed);
    }
}
