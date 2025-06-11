﻿// Plik: Data/DataAbstractAPI.cs

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
using System.Threading; 

namespace TP.ConcurrentProgramming.Data
{
    public abstract class DataAbstractAPI : IDisposable
    {
        #region Layer Factory

        public static DataAbstractAPI GetDataLayer()
        {
            return modelInstance.Value;
        }

        #endregion Layer Factory

        #region public API

        public abstract void Start(int numberOfBalls, double tableWidth, double tableHeight, Action<IVector, IBall> upperLayerHandler);
        public abstract void AddBall(Action<IVector, IBall> upperLayerHandler);
        public abstract void RemoveLastBall();
        public abstract void MoveBall(IBall ball, IVector delta);


        public abstract void SetTickEvent(ManualResetEvent tickEvent);


        #endregion public API

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable

        #region private

        private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

        #endregion private
    }
    public interface IVector
    {
        double x { get; }
        double y { get; }
    }

    public interface IBall
    {
        event EventHandler<IVector> NewPositionNotification;
        IVector Velocity { get; set; }
        double Mass { get; }
    }
}