// Interfejs piłki pozwalający na powiadamianie widoku o zmianach pozycji

using System;
using System.ComponentModel;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  public interface IBall : INotifyPropertyChanged
  {
    // odległość od góry
    double Top { get; }
    // odległość od lewej krawędzi
    double Left { get; }
    double Diameter { get; }
  }

  public abstract class ModelAbstractApi : IObservable<IBall>, IDisposable
  {
    // fabryka dla instancji dannej klasy 
    public static ModelAbstractApi CreateModel()
    {
      return modelInstance.Value;
    }

    public abstract void Start(int numberOfBalls);

    #region IObservable

    // pozwala widokowi śledzić ruch piłek
    public abstract IDisposable Subscribe(IObserver<IBall> observer);

    #endregion IObservable

    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #region private

    private static Lazy<ModelAbstractApi> modelInstance = new Lazy<ModelAbstractApi>(() => new ModelImplementation());

    #endregion private
  }
}