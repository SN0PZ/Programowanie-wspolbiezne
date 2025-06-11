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
            {
                _states.Clear();
            }

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

            _logger = new FileLogger("simulation_log.txt");
            _logTimer = new System.Timers.Timer(1000.0);
            _logTimer.Elapsed += OnLogTimerElapsed;
            _logTimer.AutoReset = true;
            _logTimer.Start();

            _tickerTimer = new System.Timers.Timer(16.0); 
            _tickerTimer.Elapsed += OnTickerTimerElapsed;
            _tickerTimer.AutoReset = true;
            _tickerTimer.Start();

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => RunLoopAsync(_cts.Token), _cts.Token);
        }

        private void OnTickerTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _tickEvent.Set();
            _tickEvent.Reset(); 
            //Task.Delay(1).ContinueWith(_ => _tickEvent.Reset());
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
            catch (OperationCanceledException) { }
        }

        private void Step()
        {
            BallState[] snapshot;
            lock (_lock)
            {
                snapshot = _states.ToArray();
            }

            if (snapshot.Length < 2)
                return;

            PhysicsEngine.Step(snapshot.ToList(), _tableW, _tableH);

            for (int i = 0; i < snapshot.Length; i++)
            {
                var st = snapshot[i];
                st.Underlying.Velocity = new RawVector(st.Velocity.x, st.Velocity.y);
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
            if (_disposed) return;

            _cts?.Cancel();

            _tickerTimer?.Stop();
            _tickerTimer?.Dispose();

            _logTimer?.Stop();
            _logTimer?.Dispose();

            _logger?.Dispose();
            _tickEvent.Dispose();

            _dataLayer.Dispose();
            _disposed = true;
        }

        private void OnLogTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            BallState[] snapshot;
            lock (_lock)
            {
                snapshot = _states.ToArray();
            }

            _logger?.Log($"INFO: Logging state of {snapshot.Length} balls.");
            foreach (var state in snapshot)
            {
                var pos = state.Position;
                var vel = state.Velocity;
                string message = $"Ball[{state.Underlying.GetHashCode()}]: Pos=({pos.x:F2}, {pos.y:F2}), Vel=({vel.x:F2}, {vel.y:F2})";
                _logger?.Log(message);
            }
        }

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> cb) => cb(_disposed);
    }
}