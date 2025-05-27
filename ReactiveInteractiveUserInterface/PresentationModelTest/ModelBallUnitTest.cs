//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TP.ConcurrentProgramming.BusinessLogic;
using TP.ConcurrentProgramming.Presentation.Model;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model.Test
{
    [TestClass]
    public class ModelBallUnitTest
    {
        [TestMethod]
        public void ConstructorTestMethod()
        {
            var fixture = new BusinessLogicIBallFixture();
            double centerX = 10.0, centerY = 20.0, diameter = 10.0;

            var ball = new ModelBall(centerX, centerY, fixture, diameter);

            Assert.AreEqual(15.0, ball.Top, 1e-6);
            Assert.AreEqual(5.0, ball.Left, 1e-6);
            Assert.AreEqual(diameter, ball.Diameter, 1e-6);
        }

        [TestMethod]
        public void PositionChangeNotificationTestMethod()
        {
            int propertyChangedCount = 0;
            var fixture = new BusinessLogicIBallFixture();
            var ball = new ModelBall(0.0, 0.0, fixture, diameter: 10.0);
            ball.PropertyChanged += (_, __) => propertyChangedCount++;

            var newPos = new PositionFixture { x = 50.0, y = 60.0 };
            fixture.RaisePositionChanged(newPos);

            Assert.AreEqual(55.0, ball.Top, 1e-6);
            Assert.AreEqual(45.0, ball.Left, 1e-6);
            Assert.AreEqual(2, propertyChangedCount);
        }

        #region Fixtures

        private class BusinessLogicIBallFixture : LogicIBall
        {
            public event EventHandler<IPosition>? NewPositionNotification;
            public double Mass => 1.0;
            public void RaisePositionChanged(IPosition pos)
                => NewPositionNotification?.Invoke(this, pos);
        }

        private class PositionFixture : IPosition
        {
            public double x { get; init; }
            public double y { get; init; }
        }

        #endregion
    }
}
