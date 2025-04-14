// Implementacja piłki (IBall) dla warstwy prezentacji

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TP.ConcurrentProgramming.BusinessLogic;
using LogicIBall = TP.ConcurrentProgramming.BusinessLogic.IBall;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  internal class ModelBall : IBall
  {
    // konstruktor inicjalizuje początkowe wartości pozycji i subskrybuje zdarzenie
    public ModelBall(double top, double left, LogicIBall underneathBall)
    {
      TopBackingField = top;
      LeftBackingField = left;
      underneathBall.NewPositionNotification += NewPositionNotification;
    }

    #region IBall

    public double Top
    {
      get { return TopBackingField; }
      private set
      {
        // czy wartość się zmieniła
        if (TopBackingField == value)
          return;
        TopBackingField = value;
        // powiadamia widok o zmianie
        RaisePropertyChanged();
      }
    }

    public double Left
    {
      get { return LeftBackingField; }
      private set
      {
        if (LeftBackingField == value)
          return;
        LeftBackingField = value;
        RaisePropertyChanged();
      }
    }

    public double Diameter { get; init; } = 0;

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion INotifyPropertyChanged

    #endregion IBall

    #region private

    private double TopBackingField;
    private double LeftBackingField;

    private void NewPositionNotification(object sender, IPosition e)
    {
      Top = e.y; Left = e.x;
    }

    // [CallerMemberName] → automatyczne przekazanie nazwy zmienionej właściwości
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