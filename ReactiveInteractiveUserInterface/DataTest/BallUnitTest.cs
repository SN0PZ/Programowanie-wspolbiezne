//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class BallUnitTest
    {
        private const double TableSizeW = 100.0;
        private const double TableSizeH = 100.0;

        [TestMethod]
        public void ConstructorTestMethod()
        {
            var v = new Vector(0.0, 0.0);
            double mass = 1.23;
            Ball newInstance = new Ball(v, v, mass, TableSizeW, TableSizeH);
            Assert.IsNotNull(newInstance);
        }

        [TestMethod]
        public void MoveTestMethod()
        {
            var initialPosition = new Vector(10.0, 10.0);
            var zeroVelocity = new Vector(0.0, 0.0);
            double mass = 1.23;

            Ball newInstance = new Ball(initialPosition, zeroVelocity, mass, TableSizeW, TableSizeH);

            IVector currentPosition = null!;
            int callbacks = 0;

            newInstance.NewPositionNotification += (_, pos) =>
            {
                Assert.IsNotNull(pos);
                currentPosition = pos;
                callbacks++;
            };

            newInstance.Move(new Vector(0.0, 0.0));

            Assert.AreEqual(1, callbacks, "Move should fire exactly one NewPositionNotification");
            Assert.AreEqual(initialPosition, currentPosition, "Position after zero‐delta Move should stay the same");
        }
    }
}
