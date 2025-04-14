// Abstrakcyjna warstwa logiki biznesowej
// Zapewnia obsługę interakcji z warstwą danych i interfejsem użytkownika

namespace TP.ConcurrentProgramming.BusinessLogic
{
  // wymaga zaimplementowania metody Dispose() poprzez IDisposable
  public abstract class BusinessLogicAbstractAPI : IDisposable
  {
    #region Layer Factory

    public static BusinessLogicAbstractAPI GetBusinessLogicLayer()
    {
      return modelInstance.Value;
    }

    #endregion Layer Factory

    #region Layer API

    // zmienna reprezentuje wymiary stołu i piłki
    public static readonly Dimensions GetDimensions = new(10.0, 10.0, 10.0);

    public abstract void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler);

    #region IDisposable

    public abstract void Dispose();

    #endregion IDisposable

    #endregion Layer API

    #region private

    //  BusinessLogicImplementation zostanie utworzony tylko raz i będzie dostępny globalnie, a Lazy<T> opóźniona inicjalizacja obiektu(dopiero przy pierwszym użyciu)
    private static Lazy<BusinessLogicAbstractAPI> modelInstance = new Lazy<BusinessLogicAbstractAPI>(() => new BusinessLogicImplementation());

    #endregion private
  }
  /// <summary>
  /// Immutable type representing table dimensions
  /// </summary>
  /// <param name="BallDimension"></param>
  /// <param name="TableHeight"></param>
  /// <param name="TableWidth"></param>
  /// <remarks>
  /// Must be abstract
  /// </remarks>
  public record Dimensions(double BallDimension, double TableHeight, double TableWidth);

  // reprezentuje pozycję piłki, którą nie mona zmienić (init)
  public interface IPosition
  {
    double x { get; init; }
    double y { get; init; }
  }

  // reprezentacji piłki 
  public interface IBall 
  {
    // zdarzenie powiadamia o mowej pozycji (IPosition) 
    event EventHandler<IPosition> NewPositionNotification;
  }
}