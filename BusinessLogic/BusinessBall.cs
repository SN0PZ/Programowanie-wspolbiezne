// Klasa Ball implementuje interfejs IBall,
// służy jako pośrednik między warstwą danych a warstwą logiki biznesowej

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class Ball : IBall
  {
    // konstryktor przyjmuje obiekt piłki pochodzący z warstwy danych
    public Ball(Data.IBall ball)
    {
      // gdy warstwa danych wysyła nowe współrzędne, metoda RaisePositionChangeEvent zostaje wywołana
      ball.NewPositionNotification += RaisePositionChangeEvent;
    }

    #region IBall

    // zdarzenie informujące warstwą wyższą o nowej pozycji piłki za pomocą metody RaisePositionChangeEvent
    public event EventHandler<IPosition>? NewPositionNotification;

    #endregion IBall

    #region private

    // metoda jest wywoływana po odebraniu nowej pozycji piłki z warstwy danych
    private void RaisePositionChangeEvent(object? sender, Data.IVector e)
    {
      // tworzy nową pozycję i wywołuje NewPositionNotification, powiadamiając warstwę wyższą o tym
      NewPositionNotification?.Invoke(this, new Position(e.x, e.y));
    }

    #endregion private
  }
}