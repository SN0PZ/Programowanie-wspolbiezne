using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class PresentationModel : ModelAbstractApi
    {
        private readonly BusinessLogicAbstractAPI _logicLayer;
        private readonly IObservable<EventPattern<BallChangedEventArgs>> _eventStream;
        private readonly SynchronizationContext _uiContext;
        private bool _disposed = false;

        public PresentationModel() : this(null) { }

        public PresentationModel(BusinessLogicAbstractAPI logicLayer)
        {
            _logicLayer = logicLayer ?? BusinessLogicAbstractAPI.GetBusinessLogicLayer();

            _uiContext = SynchronizationContext.Current
                         ?? throw new InvalidOperationException("PresentationModel must be created on UI thread");

            var src = Observable.FromEventPattern<BallChangedEventArgs>(
                this, nameof(BallChanged));
            _eventStream = src.ObserveOn(_uiContext);
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PresentationModel));
            _logicLayer.Start(numberOfBalls, tableWidth, tableHeight, BallCreatedHandler);
        }

        public override IDisposable Subscribe(IObserver<IBall> observer)
            => _eventStream.Subscribe(x => observer.OnNext(x.EventArgs.Ball));

        public override void AddBall(Action<IBall> observer)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PresentationModel));
            _logicLayer.AddBall((pos, logicBall) =>
            {
                double diameter = MassToDiameter(logicBall.Mass);
                var modelBall = new ModelBall(pos.x, pos.y, logicBall, diameter);
                observer(modelBall);
            });
        }

        public override void RemoveLastBall()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(PresentationModel));
            _logicLayer.RemoveLastBall();
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _logicLayer.Dispose();
            _disposed = true;
        }

        public event EventHandler<BallChangedEventArgs>? BallChanged;

        private void BallCreatedHandler(IPosition position, BusinessLogic.IBall ball)
        {
            double diameter = MassToDiameter(ball.Mass);
            var newBall = new ModelBall(position.x, position.y, ball, diameter);
            BallChanged?.Invoke(this, new BallChangedEventArgs { Ball = newBall });
        }

        private static double MassToDiameter(double mass)
        {
            const double minMass = 0.5, maxMass = 2.0;
            const double minDiam = 10.0, maxDiam = 20.0;
            var m = Math.Clamp(mass, minMass, maxMass);
            return minDiam + (m - minMass) / (maxMass - minMass) * (maxDiam - minDiam);
        }
    }

    public class BallChangedEventArgs : EventArgs
    {
        public IBall Ball { get; init; }
    }
}
