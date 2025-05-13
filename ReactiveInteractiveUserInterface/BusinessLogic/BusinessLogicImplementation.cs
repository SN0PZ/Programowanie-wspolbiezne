using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;
using Data = TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        private readonly List<BallState> _balls = new();
        private double _tableW, _tableH;
        private readonly UnderneathLayerAPI layerBellow;
        private CancellationTokenSource? _cts;
        private bool Disposed = false;

        // mapping masa → rozmiar
        private const double MinMass = 0.5, MaxMass = 2.0;
        private const double MinDiam = 10.0, MaxDiam = 20.0;
        private static double MassToDiameter(double m)
            => MinDiam + (m - MinMass) / (MaxMass - MinMass) * (MaxDiam - MinDiam);
        private static double MassToRadius(double m)
            => MassToDiameter(m) / 2;

        // Domyślny konstruktor korzystający z GetDataLayer()
        public BusinessLogicImplementation()
            : this(UnderneathLayerAPI.GetDataLayer())
        { }

        // Konstruktor przyjmujący warstwę danych
        internal BusinessLogicImplementation(UnderneathLayerAPI underneathLayer)
        {
            layerBellow = underneathLayer;
        }

        private class RawVector : Data.IVector
        {
            public double x { get; }
            public double y { get; }
            public RawVector(double x, double y) => (this.x, this.y) = (x, y);
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            _tableW = tableWidth;
            _tableH = tableHeight;

            layerBellow.Start(numberOfBalls, tableWidth, tableHeight,
                (vec, dataBall) =>
                {
                    double r = MassToRadius(dataBall.Mass);
                    var state = new BallState(
                        dataBall,
                        new Vector(vec.x, vec.y),
                        new Vector(dataBall.Velocity.x, dataBall.Velocity.y),
                        r
                    );
                    dataBall.NewPositionNotification += (_, v)
                        => state.Position = new Vector(v.x, v.y);
                    lock (_balls) { _balls.Add(state); }
                    upperLayerHandler(
                        new Position(state.Position.x, state.Position.y),
                        new Ball(dataBall)
                    );
                });

            // Przygotuj token anulowania i uruchom pętlę symulacji
            _cts = new CancellationTokenSource();
            _ = Task.Run(() => RunLoopAsync(_cts.Token));
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    DoStep();
                    await Task.Delay(5, token); // 5 ms delay
                }
            }
            catch (OperationCanceledException)
            {
                // normalne zakończenie pętli
            }
        }

        private void DoStep()
        {
            lock (_balls)
            {
                var oldPos = _balls.Select(s => s.Position).ToArray();
                PhysicsEngine.Step(_balls, _tableW, _tableH);
                for (int i = 0; i < _balls.Count; i++)
                {
                    var st = _balls[i];
                    var delta = st.Position - oldPos[i];
                    layerBellow.MoveBall(st.Underlying, new RawVector(delta.x, delta.y));
                }
            }
        }

        public override void AddBall(Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            layerBellow.AddBall((vec, dataBall) =>
            {
                double r = MassToRadius(dataBall.Mass);
                var state = new BallState(
                    dataBall,
                    new Vector(vec.x, vec.y),
                    new Vector(dataBall.Velocity.x, dataBall.Velocity.y),
                    r
                );
                dataBall.NewPositionNotification += (_, v)
                    => state.Position = new Vector(v.x, v.y);
                lock (_balls) { _balls.Add(state); }
                upperLayerHandler(
                    new Position(state.Position.x, state.Position.y),
                    new Ball(dataBall)
                );
            });
        }

        public override void RemoveLastBall()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            lock (_balls)
            {
                if (_balls.Count > 0)
                    _balls.RemoveAt(_balls.Count - 1);
            }
            layerBellow.RemoveLastBall();
        }

        public override void Dispose()
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            _cts?.Cancel();
            layerBellow.Dispose();
            Disposed = true;
        }
    }
}
