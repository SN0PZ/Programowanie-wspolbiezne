//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;
using System.Threading;
using Data = TP.ConcurrentProgramming.Data; 

namespace TP.ConcurrentProgramming.BusinessLogic
{
    internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
    {
        #region ctor
        private readonly List<BallState> _balls = new();
        private Timer _timer;
        private double _tableW, _tableH, _ballDiameter;
        public BusinessLogicImplementation() : this(null)
        { }

        internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
        {
            layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
        }

        private class RawVector : Data.IVector
        {
            public double x { get; }
            public double y { get; }
            public RawVector(double x, double y) => (this.x, this.y) = (x, y);
        }

        #endregion ctor

        #region BusinessLogicAbstractAPI

        public override void Dispose()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            layerBellow.Dispose();
            Disposed = true;
        }

        public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed) throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            _tableW = tableWidth; _tableH = tableHeight;
            _ballDiameter = GetDimensions.BallDimension;

            layerBellow.Start(numberOfBalls, tableWidth, tableHeight,
              (vector, dataBall) =>
              {
                  var state = new BallState(
          dataBall,
          new Vector(vector.x, vector.y),
          new Vector(dataBall.Velocity.x, dataBall.Velocity.y)
        );
                  dataBall.NewPositionNotification += (_, v)
          => state.Position = new Vector(v.x, v.y);

                  lock (_balls) { _balls.Add(state); }

                  upperLayerHandler(
          new Position(state.Position.x, state.Position.y),
          new Ball(dataBall)
        );
              }
            );

            _timer = new Timer(_ => DoStep(), null,
                               TimeSpan.Zero,
                               TimeSpan.FromMilliseconds(5));
        }
        private void DoStep()
        {
            lock (_balls)
            {
                var oldPositions = _balls.Select(s => s.Position).ToArray();
                PhysicsEngine.Step(_balls, _tableW, _tableH, _ballDiameter);
                for (int i = 0; i < _balls.Count; i++)
                {
                    var st = _balls[i];
                    var delta = st.Position - oldPositions[i];
                    var dv = new RawVector(delta.x, delta.y);
                    layerBellow.MoveBall(st.Underlying, dv);
                }
            }
        }
        public override void AddBall(Action<IPosition, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            layerBellow.AddBall((vector, dataBall) =>
            {
                var state = new BallState(
                    dataBall,
                    new Vector(vector.x, vector.y),
                    new Vector(dataBall.Velocity.x, dataBall.Velocity.y)
                );

                dataBall.NewPositionNotification += (_, v) =>
                    state.Position = new Vector(v.x, v.y);

                lock (_balls)
                {
                    _balls.Add(state);
                }

                upperLayerHandler(
                    new Position(state.Position.x, state.Position.y),
                    new Ball(dataBall)
                );
            });
        }

        public override void RemoveLastBall()
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(BusinessLogicImplementation));

            layerBellow.RemoveLastBall();
        }
        #endregion BusinessLogicAbstractAPI

        #region private

        private bool Disposed = false;

        private readonly UnderneathLayerAPI layerBellow;

        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
        internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
        {
            returnInstanceDisposed(Disposed);
        }

        #endregion TestingInfrastructure
    }
}