////__________________________________________________________________________________________
////
////  Copyright 2024 Mariusz Postol LODZ POLAND.
////
////  To be in touch join the community by pressing the `Watch` button and to get started
////  comment using the discussion panel at
////  https://github.com/mpostol/TP/discussions/182
////__________________________________________________________________________________________

//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.ComponentModel;
//using System.Reactive;
//using System.Reactive.Linq;
//using TP.ConcurrentProgramming.Presentation.ViewModel;
//using TP.ConcurrentProgramming.Presentation.Model;

//namespace TP.ConcurrentProgramming.Presentation.ViewModel.Test
//{
//    [TestClass]
//    public class MainWindowViewModelUnitTest
//    {
//        [TestMethod]
//        public void ConstructorAndDisposeTest()
//        {
//            // Arrange: a null‐model fixture that counts calls
//            var nullModel = new ModelNullFixture();
//            Assert.AreEqual(0, nullModel.DisposedCalls);
//            Assert.AreEqual(0, nullModel.StartCalls);
//            Assert.AreEqual(0, nullModel.SubscribeCalls);

//            // Act: create VM, set number, start, then dispose
//            using var vm = new MainWindowViewModel(nullModel);
//            vm.NumberOfBallsToAdd = 7;
//            vm.StartSimulationWithSize(300, 200);

//            // Assert: Start() and Subscribe() happened
//            Assert.AreEqual(0, nullModel.DisposedCalls, "Model not disposed yet");
//            Assert.AreEqual(1, nullModel.StartCalls, "Start called once");
//            Assert.AreEqual(1, nullModel.SubscribeCalls, "Subscribe called in ctor");
//            Assert.IsNotNull(vm.Balls);

//            // Dispose and verify
//            vm.Dispose();
//            Assert.AreEqual(1, nullModel.DisposedCalls, "Dispose should propagate to model");
//        }

//        [TestMethod]
//        public void BehaviorTestMethod()
//        {
//            // Arrange: a simulator that pushes N balls through the BallChanged event
//            var sim = new ModelSimulatorFixture();
//            using var vm = new MainWindowViewModel(sim);

//            Assert.AreEqual(0, vm.Balls.Count);

//            // Act: set how many, then StartSimulationWithSize(...)
//            int count = 5;
//            vm.NumberOfBallsToAdd = count;
//            vm.StartSimulationWithSize(400, 300);

//            // Assert: the VM’s collection has exactly that many
//            Assert.AreEqual(count, vm.Balls.Count);

//            // Dispose should clear the viewmodel’s list and dispose the model
//            vm.Dispose();
//            Assert.IsTrue(sim.DisposedFlag, "Model was not disposed");
//            Assert.AreEqual(0, vm.Balls.Count, "VM should clear its Balls on dispose");
//        }


//        #region Fixtures

//        // A no-op ModelAbstractApi that just counts calls
//        private class ModelNullFixture : ModelAbstractApi
//        {
//            public int DisposedCalls;
//            public int StartCalls;
//            public int SubscribeCalls;

//            public override void Dispose() => DisposedCalls++;

//            public override void Start(int numberOfBalls, double tableWidth, double tableHeight)
//                => StartCalls++;

//            public override IDisposable Subscribe(IObserver<IBall> observer)
//            {
//                SubscribeCalls++;
//                return new DisposableStub();
//            }

//            public override void AddBall(Action<IBall> observer) { }
//            public override void RemoveLastBall() { }

//            private class DisposableStub : IDisposable { public void Dispose() { } }
//        }

//        // A simulator ModelAbstractApi that publishes exactly N ModelBall instances
//        private class ModelSimulatorFixture : ModelAbstractApi
//        {
//            public bool DisposedFlag;
//            private readonly IObservable<EventPattern<BallChangedEventArgs>> _stream;

//            public event EventHandler<BallChangedEventArgs>? BallChanged;

//            public ModelSimulatorFixture()
//            {
//                // create an Rx stream from the BallChanged event
//                _stream = Observable.FromEventPattern<BallChangedEventArgs>(
//                    h => BallChanged += h,
//                    h => BallChanged -= h);
//            }

//            public override IDisposable Subscribe(IObserver<IBall> observer)
//            {
//                // forward only the Ball payload
//                return _stream.Subscribe(evt => observer.OnNext(evt.EventArgs.Ball));
//            }

//            public override void Start(int numberOfBalls, double tableWidth, double tableHeight)
//            {
//                for (int i = 0; i < numberOfBalls; i++)
//                {
//                    // fire an event with a simple dummy IBall
//                    var dummy = new DummyModelBall();
//                    BallChanged?.Invoke(this, new BallChangedEventArgs { Ball = dummy });
//                }
//            }

//            public override void Dispose() => DisposedFlag = true;

//            public override void AddBall(Action<IBall> observer) { }
//            public override void RemoveLastBall() { }

//            // a minimal IBall for the simulation
//            private class DummyModelBall : TP.ConcurrentProgramming.Presentation.Model.IBall
//            {
//                public event PropertyChangedEventHandler? PropertyChanged;
//                public double Diameter => 10;
//                public double Top => 0;
//                public double Left => 0;
//            }
//        }

//        #endregion
//    }
//}