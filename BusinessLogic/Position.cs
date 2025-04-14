// implementacja interfejsu IPosition
  
namespace TP.ConcurrentProgramming.BusinessLogic
{
  // typ record jest niemutowalny (jego właściwości nie mogą być zmieniane po utworzeniu);
  // ma wbudowaną obsługę porównywania wartości zamiast referencji i
  // automatycznie implementuje metody Equals(), GetHashCode() i ToString()
  
  // record -> łatwo tworzyć kopie obiektów z modyfikacją tylko jednej właściwości
  internal record Position : IPosition
  {
    #region IPosition

    public double x { get; init; }
    public double y { get; init; }

    #endregion IPosition

    /// <summary>
    /// Creates new instance of <seealso cref="IPosition"/> and initialize all properties
    /// </summary>
    public Position(double posX, double posY)
    {
      x = posX;
      y = posY;
    }
  }
}