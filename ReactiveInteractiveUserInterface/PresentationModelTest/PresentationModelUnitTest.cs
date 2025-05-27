////____________________________________________________________________________________________________________________________________
////
////  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
////
////  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
////
////  https://github.com/mpostol/TP/discussions/182
////
////_____________________________________________________________________________________________________________________________________

//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using TP.ConcurrentProgramming.Presentation.Model;
//using TP.ConcurrentProgramming.BusinessLogic;

//namespace TP.ConcurrentProgramming.Presentation.Model.Test
//{
//    [TestClass]
//    public class PresentationModelUnitTest
//    {
//        private class UnderlyingLogicFixture : BusinessLogicAbstractAPI
//        {
//            internal bool DisposedCalled;
//            internal int StartCalledWith;
//            public override void Dispose()
//            {
//                DisposedCalled = true;
//            }
//            public override void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IPosition, IBall> handler)
//            {
//                StartCalledWith = numberOfBalls;
//            }
//            public override void AddBall(Action<IPosition, IBall> handler) => throw new NotImplementedException();
//            public override void RemoveLastBall() => throw new NotImplementedException();
//        }

//        [TestMethod]
//        public void Dispose_ShouldDisposeUnderlyingLogic_AndPreventFurtherStart()
//        {
//            // Arrange
//            var fixture = new UnderlyingLogicFixture();
//            var model = new PresentationModel(fixture);

//            // Act
//            model.Dispose();

//            // Assert
//            Assert.IsTrue(fixture.DisposedCalled, "Dispose on model should call Dispose on logic layer");
//            Assert.ThrowsException<ObjectDisposedException>(() => model.Start(1, 100, 100));
//        }

//        [TestMethod]
//        public void Start_ShouldCallUnderlyingLogicStart_AndForwardBallsToSubscribers()
//        {
//            // Arrange
//            var fixture = new UnderlyingLogicFixture();
//            var model = new PresentationModel(fixture);
//            int observedCount = 0;

//            // Subscribe to new balls
//            using var subscription = model.Subscribe(ball => observedCount++);

//            // Act
//            model.Start(5, 200, 200);

//            // Assert
//            Assert.AreEqual(5, fixture.StartCalledWith, "Model.Start should forward the ball count to logic layer");
//            // We didn't hook up the BallChanged event in the fixture, so observedCount remains 0.
//            // If you want to test actual BallCreatedHandler invocation, you can extend the fixture to raise it.
//            Assert.AreEqual(0, observedCount, "By default the fixture does not invoke callbacks");
//        }
//    }
//}
