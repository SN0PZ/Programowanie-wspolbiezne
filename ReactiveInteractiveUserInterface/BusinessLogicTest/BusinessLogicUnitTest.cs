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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TP.ConcurrentProgramming.BusinessLogic;
using Data = TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
    [TestClass]
    public class BusinessLogicImplementationUnitTest
    {
        [TestMethod]
        public void Constructor_DoesNotDisposeUnderlyingLayer()
        {
            using var sut = new BusinessLogicImplementation(new DataLayerConstructorFixture());
            bool disposed = true;
            sut.CheckObjectDisposed(x => disposed = x);
            Assert.IsFalse(disposed);
        }

        [TestMethod]
        public void Dispose_DisposesUnderlyingLayer_AndFurtherCallsThrow()
        {
            var fixture = new DataLayerDisposeFixture();
            var sut = new BusinessLogicImplementation(fixture);
            Assert.IsFalse(fixture.Disposed);

            bool wasDisposed = false;
            sut.CheckObjectDisposed(x => wasDisposed = x);
            Assert.IsFalse(wasDisposed);

            sut.Dispose();
            sut.CheckObjectDisposed(x => wasDisposed = x);
            Assert.IsTrue(wasDisposed);
            Assert.IsTrue(fixture.Disposed);

            Assert.ThrowsException<ObjectDisposedException>(() => sut.Dispose());

            Assert.ThrowsException<ObjectDisposedException>(() =>
                sut.Start(0, 100, 100, (_, __) => { }));
        }

        [TestMethod]
        public void Start_CallsUnderlyingStart_AndRaisesHandlerOnce()
        {
            var fixture = new DataLayerStartFixture();
            using var sut = new BusinessLogicImplementation(fixture);

            int callCount = 0;
            int num = 7;
            sut.Start(num, 50, 60, (pos, ball) =>
            {
                Assert.IsNotNull(pos);
                Assert.IsNotNull(ball);
                callCount++;
            });

            Assert.IsTrue(fixture.StartCalled);
            Assert.AreEqual(num, fixture.NumberOfBallsCreated);
            Assert.AreEqual(1, callCount, "handler should have been called exactly once per Start()");
        }

        [TestMethod]
        public void AddBall_CallsUnderlyingAddBall_AndRaisesHandlerOnce()
        {
            var fixture = new DataLayerAddFixture();
            using var sut = new BusinessLogicImplementation(fixture);

            int callCount = 0;
            sut.AddBall((pos, ball) =>
            {
                Assert.IsNotNull(pos);
                Assert.IsNotNull(ball);
                callCount++;
            });

            Assert.IsTrue(fixture.AddCalled);
            Assert.AreEqual(1, callCount, "handler should have been called exactly once per AddBall()");
        }

        [TestMethod]
        public void RemoveLastBall_CallsUnderlyingRemoveLastBall()
        {
            var fixture = new DataLayerRemoveFixture();
            using var sut = new BusinessLogicImplementation(fixture);

            sut.RemoveLastBall();
            Assert.IsTrue(fixture.RemoveCalled);
        }

        #region Fixtures

        private class DataLayerConstructorFixture : Data.DataAbstractAPI
        {
            public override void Start(int numberOfBalls, double w, double h, Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void AddBall(Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void RemoveLastBall()
                => throw new NotImplementedException();
            public override void MoveBall(Data.IBall ball, Data.IVector delta)
                => throw new NotImplementedException();
            public override void Dispose() { /* no-op */ }
        }

        private class DataLayerDisposeFixture : Data.DataAbstractAPI
        {
            internal bool Disposed;
            public override void Start(int numberOfBalls, double w, double h, Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void AddBall(Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void RemoveLastBall()
                => throw new NotImplementedException();
            public override void MoveBall(Data.IBall ball, Data.IVector delta)
                => throw new NotImplementedException();
            public override void Dispose() => Disposed = true;
        }

        private class DataLayerStartFixture : Data.DataAbstractAPI
        {
            internal bool StartCalled;
            internal int NumberOfBallsCreated;
            public override void Start(int numberOfBalls, double w, double h, Action<Data.IVector, Data.IBall> upper)
            {
                StartCalled = true;
                NumberOfBallsCreated = numberOfBalls;
                upper(new VectorFixture(1, 2), new BallFixture());
            }
            public override void AddBall(Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void RemoveLastBall()
                => throw new NotImplementedException();
            public override void MoveBall(Data.IBall ball, Data.IVector delta)
                => throw new NotImplementedException();
            public override void Dispose() { /* no-op */ }

            private record VectorFixture(double x, double y) : Data.IVector;
            private class BallFixture : Data.IBall
            {
                public event EventHandler<Data.IVector>? NewPositionNotification;
                public Data.IVector Velocity { get => new VectorFixture(0, 0); set => throw new NotImplementedException(); }
                public double Mass => 1.234;
            }
        }

        private class DataLayerAddFixture : Data.DataAbstractAPI
        {
            internal bool AddCalled;
            public override void Start(int numberOfBalls, double w, double h, Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void AddBall(Action<Data.IVector, Data.IBall> upper)
            {
                AddCalled = true;
                upper(new VectorFixture(3, 4), new BallFixture());
            }
            public override void RemoveLastBall()
                => throw new NotImplementedException();
            public override void MoveBall(Data.IBall ball, Data.IVector delta)
                => throw new NotImplementedException();
            public override void Dispose() { /* no-op */ }

            private record VectorFixture(double x, double y) : Data.IVector;
            private class BallFixture : Data.IBall
            {
                public event EventHandler<Data.IVector>? NewPositionNotification;
                public Data.IVector Velocity { get => new VectorFixture(0, 0); set => throw new NotImplementedException(); }
                public double Mass => 0.999;
            }
        }

        private class DataLayerRemoveFixture : Data.DataAbstractAPI
        {
            internal bool RemoveCalled;
            public override void Start(int numberOfBalls, double w, double h, Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void AddBall(Action<Data.IVector, Data.IBall> upper)
                => throw new NotImplementedException();
            public override void RemoveLastBall() => RemoveCalled = true;
            public override void MoveBall(Data.IBall ball, Data.IVector delta)
                => throw new NotImplementedException();
            public override void Dispose() { /* no-op */ }
        }

        #endregion
    }
}
