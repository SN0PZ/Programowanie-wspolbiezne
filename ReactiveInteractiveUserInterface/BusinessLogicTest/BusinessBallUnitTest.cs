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
    public class BallUnitTest
    {
        [TestMethod]
        public void MoveTestMethod()
        {
            var dataBall = new DataBallFixture();
            var wrapper = new Ball(dataBall);

            int callbackCount = 0;
            wrapper.NewPositionNotification += (sender, position) =>
            {
                Assert.AreSame(wrapper, sender);
                Assert.AreEqual(5.5, position.x, 1e-9);
                Assert.AreEqual(7.7, position.y, 1e-9);
                callbackCount++;
            };

            dataBall.RaiseMove(5.5, 7.7);

            Assert.AreEqual(1, callbackCount);
        }

        #region Testing instrumentation

        private class DataBallFixture : Data.IBall
        {
            public event EventHandler<Data.IVector>? NewPositionNotification;
            public Data.IVector Velocity { get; set; }
            public double Mass => 1.0;
            public void RaiseMove(double x, double y)
            {
                NewPositionNotification?.Invoke(this, new VectorFixture(x, y));
            }
        }
        private record VectorFixture(double x, double y) : Data.IVector;

        #endregion
    }
}
