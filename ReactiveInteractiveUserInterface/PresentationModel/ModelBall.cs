//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2023, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//  by introducing yourself and telling us what you do with this community.
//_____________________________________________________________________________________________________________________________________

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
    internal class ModelBall : IBall
    {
        #region IBall
        private double topBackingField;
        private double leftBackingField;

        public ModelBall(double centerX, double centerY, LogicIBall underneathBall, double diameter)
        {
            Diameter = diameter;
            topBackingField = centerY - diameter / 2;
            leftBackingField = centerX - diameter / 2;
            underneathBall.NewPositionNotification += NewPositionNotification;
        }

        public double Top
        {
            get => topBackingField;
            private set
            {
                if (topBackingField == value) return;
                topBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Left
        {
            get => leftBackingField;
            private set
            {
                if (leftBackingField == value) return;
                leftBackingField = value;
                RaisePropertyChanged();
            }
        }

        public double Diameter { get; init; } = 0;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #endregion IBall

        #region private
        private void NewPositionNotification(object sender, IPosition e)
        {
            Top = e.y - Diameter / 2;
            Left = e.x - Diameter / 2;
        }

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion private

        #region testing instrumentation

        [Conditional("DEBUG")]
        internal void SetLeft(double x)
        { Left = x; }

        [Conditional("DEBUG")]
        internal void SettTop(double x)
        { Top = x; }

        #endregion testing instrumentation
    }
}