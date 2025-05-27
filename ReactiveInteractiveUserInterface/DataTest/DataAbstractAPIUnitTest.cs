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
using TP.ConcurrentProgramming.Data;

namespace TP.ConcurrentProgramming.Data.Test
{
    [TestClass]
    public class DataAbstractAPIUnitTest
    {
        [TestMethod]
        public void GetDataLayer_IsSingletonAndDisposeThrowsOnSecondCall()
        {
            var instance1 = DataAbstractAPI.GetDataLayer();
            var instance2 = DataAbstractAPI.GetDataLayer();

            Assert.AreSame(instance1, instance2);

            instance1.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => instance2.Dispose());
        }
    }
}
