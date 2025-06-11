using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
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

        private readonly ManualResetEvent _tickEvent = new ManualResetEvent(false);
        private System.Timers.Timer? _tickerTimer;

        private System.Timers.Timer? _logTimer;
        private FileLogger? _logger;

        private DateTime _lastTickTime;

        private const double MinMass = 0.5, MaxMass = 2.0;
        private const double MinDiam = 10.0, MaxDiam = 20.0;
        private static double MassToDiameter(double m) => MinDiam + (m - MinMass) / (MaxMass - MinMass) * (MaxDiam - MinDiam);
        private static double MassToRadius(double m) => MassToDiameter(m) / 2;

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
        {
            _dataLayer = dataLayer ?? throw new ArgumentNullException(nameof(dataLayer));
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Nie udało się ustawić priorytetu: {ex.Message}");
            }
        }

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
                _states.Clear();

            _dataLayer.SetTickEvent(_tickEvent);
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
                    dataBall.NewPositionNotification += (_, v) => state.Position = new Vector(v.x, v.y);
                    lock (_lock)
                        _states.Add(state);

                    upperLayerHandler(
                        new Position(state.Position.x, state.Position.y),
                        new Ball(dataBall));
                });

            _logger = new FileLogger("simulation_log.txt");
            _logTimer = new System.Timers.Timer(1000);
            _logTimer.Elapsed += OnLogTimerElapsed;
            _logTimer.AutoReset = true;
            _logTimer.Start();

            _lastTickTime = DateTime.Now;
            _tickerTimer = new System.Timers.Timer(16.0);
            _tickerTimer.Elapsed += OnTickerTimerElapsed;
            _tickerTimer.AutoReset = true;
            _tickerTimer.Start();

        }

        private void OnTickerTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            double deltaTime = (e.SignalTime - _lastTickTime).TotalSeconds;
            _lastTickTime = e.SignalTime;

            Step(deltaTime);
        }

        private void Step(double deltaTime)
        {
            BallState[] snapshot;
            lock (_lock)
                snapshot = _states.ToArray();

            if (snapshot.Length < 2)
                return;

            PhysicsEngine.Step(snapshot.ToList(), _tableW, _tableH, deltaTime);

            foreach (var st in snapshot)
            {
                var moveVec = new RawVector(st.Velocity.x * deltaTime, st.Velocity.y * deltaTime);
                _dataLayer.MoveBall(st.Underlying, moveVec);
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
                dataBall.NewPositionNotification += (_, v) => state.Position = new Vector(v.x, v.y);
                lock (_lock)
                    _states.Add(state);

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
            if (_disposed) return;

            _tickerTimer?.Stop();
            if (_tickerTimer != null)
            {
                _tickerTimer.Elapsed -= OnTickerTimerElapsed;
                _tickerTimer.Dispose();
            }

            _logTimer?.Stop();
            if (_logTimer != null)
            {
                _logTimer.Elapsed -= OnLogTimerElapsed;
                _logTimer.Dispose();
            }

            _logger?.Dispose();
            _tickEvent.Dispose();
            _dataLayer.Dispose();

            _disposed = true;
        }

        private void OnLogTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            BallState[] snapshot;
            lock (_lock)
                snapshot = _states.ToArray();

            _logger?.Log($"INFO: Logging state of {snapshot.Length} balls.");
            foreach (var state in snapshot)
            {
                var pos = state.Position;
                var vel = state.Velocity;
                string message = $"Ball[{state.Underlying.GetHashCode()}]: Pos=({pos.x:F2}, {pos.y:F2}), Vel=({vel.x:F2}, {vel.y:F2})";
                _logger?.Log(message);
            }
        }
    }
}
