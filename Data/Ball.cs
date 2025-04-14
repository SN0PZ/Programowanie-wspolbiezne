// Klasa reprezentuje piłkę w systemie wielowątkowym,
// obsługuje ruch i powiadamianie o zmianie pozycji

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    // paramenty konstruktora to wektory, reprezentujące odpowiednio położenie i prędkość piłki
    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
    }

    #endregion ctor

    #region IBall

    // zdarzenie, które informuje warstwę wyższą o zmianie pozycji piłki
    public event EventHandler<IVector>? NewPositionNotification;

    // prędkość piłki może być modyfikowana (set)
    public IVector Velocity { get; set; }

        #endregion IBall

        #region private

        internal Vector Position { get; private set; }  // Zamiast private Vector Position;

        // zdarzenie zostanie wywołane tylko wtedy, gdy ma subskrybentów (czyli ktoś słucha zmian pozycji)
        private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    // symulacja ruchu za pomocą dodania do niej bierzących wspórzędnych wartości delta i powiadamia o tym subskrybentów
    internal void Move(Vector delta)
    {
      Position = new Vector(Position.x + delta.x, Position.y + delta.y);
      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}