//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Threading;
//using TP.ConcurrentProgramming.Presentation.Model;

//namespace TP.ConcurrentProgramming.Presentation.Model.Test
//{
//    [TestClass]
//    public class ModelAbstractApiTest
//    {
//        [TestMethod]
//        public void SingletonConstructorTestMethod()
//        {
//            Exception caught = null;
//            ModelAbstractApi first = null;
//            ModelAbstractApi second = null;

//            var staThread = new Thread(() =>
//            {
//                try
//                {
//                    first = ModelAbstractApi.CreateModel();
//                    second = ModelAbstractApi.CreateModel();
//                    Assert.AreSame(first, second);
//                }
//                catch (Exception ex)
//                {
//                    caught = ex;
//                }
//            });

//            staThread.SetApartmentState(ApartmentState.STA);
//            staThread.Start();
//            staThread.Join();

//            if (caught != null)
//            {
//                // rethrow so MSTest sees the failure
//                throw caught;
//            }
//        }
//    }
//}