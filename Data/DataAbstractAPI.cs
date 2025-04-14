// Abstrakcyjna warstwę dostępu do danych

namespace TP.ConcurrentProgramming.Data
{
  // klasa wymaga zwolnienia zasobów (Dispose())
  public abstract class DataAbstractAPI : IDisposable
  {
    #region Layer Factory

    // fabryka, która zwraca instancję warstwy danych (DataAbstractAPI)
    public static DataAbstractAPI GetDataLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region public API

    public abstract void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler);

    #endregion public API

    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #region private

    // opóźniona inicjalizacja Lazy
    private static Lazy<DataAbstractAPI> modelInstance = new Lazy<DataAbstractAPI>(() => new DataImplementation());

    #endregion private
  }

  // niezmienny (iniy) typ reprezentujący wektor 2D
  public interface IVector
  {
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    double x { get; init; }

    /// <summary>
    /// The y component of the vector.
    /// </summary>
    double y { get; init; }
  }

  // model piłki
  public interface IBall
  {
    // warstwa biznesowa (BusinessLogic) może śledzić zmiany pozycji piłek za pomocą tego zdarzenia
    event EventHandler<IVector> NewPositionNotification;

    IVector Velocity { get; set; }
  }
}