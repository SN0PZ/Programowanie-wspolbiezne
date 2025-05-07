using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using TP.ConcurrentProgramming.BusinessLogic;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class PresentationModel : ModelAbstractApi
    {
        public PresentationModel() : this(null) { }

        public PresentationModel(BusinessLogicAbstractAPI logicLayer)
        {
            _logicLayer = logicLayer ?? BusinessLogicAbstractAPI.GetBusinessLogicLayer();
            _eventStream = Observable.FromEventPattern<BallChangedEventArgs>(this, nameof(BallChanged));
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight)
        {
            _logicLayer.Start(numberOfBalls, tableWidth, tableHeight, BallCreatedHandler);
        }

        public override IDisposable Subscribe(IObserver<IBall> observer)
        {
            return _eventStream.Subscribe(x => observer.OnNext(x.EventArgs.Ball));
        }

        public override void Dispose()
        {
            if (_disposed)
                return;

            _logicLayer.Dispose();
            _disposed = true;
        }

        public event EventHandler<BallChangedEventArgs>? BallChanged;

        #region Helpers

        private bool _disposed = false;
        private readonly BusinessLogicAbstractAPI _logicLayer;
        private readonly IObservable<EventPattern<BallChangedEventArgs>> _eventStream;

        private void BallCreatedHandler(IPosition position, BusinessLogic.IBall ball)
        {
            double diameter = MassToDiameter(ball.Mass);
            var newBall = new ModelBall(position.x, position.y, ball)
            { Diameter = diameter };
            BallChanged?.Invoke(this, new BallChangedEventArgs { Ball = newBall });
        }

        public override void AddBall(Action<IBall> observer)
        {
            _logicLayer.AddBall((pos, logicBall) =>
            {
                double diameter = MassToDiameter(logicBall.Mass);
                var modelBall = new ModelBall(pos.x, pos.y, logicBall) { Diameter = diameter };
                observer(modelBall);
            });
        }

        public override void RemoveLastBall()
        {
            _logicLayer.RemoveLastBall();
        }
        private static double MassToDiameter(double mass)
        {
            const double minMass = 0.5, maxMass = 2.0;
            const double minDiam = 10.0, maxDiam = 20.0;
            var m = Math.Clamp(mass, minMass, maxMass);
            return minDiam + (m - minMass) / (maxMass - minMass) * (maxDiam - minDiam);
        }

        #endregion
    }

    public class BallChangedEventArgs : EventArgs
    {
        public IBall Ball { get; init; }
    }
}