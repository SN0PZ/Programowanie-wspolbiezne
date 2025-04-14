// Implementacją warstwy danych,
// obsługuje tworzenie piłek, ich ruch oraz
// powiadamianie wyższej warstwy (BusinessLogic) o zmianach pozycji

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
        #region ctor

        private readonly Boundary boundary;
        private Timer? MoveTimer; // Tylko jedna deklaracja MoveTimer jako nullable

        public DataImplementation()
    {
            boundary = new Boundary(0, 375, 0, 395);
    }

        private void StartTimer()
        {
            if (MoveTimer != null)
            {
                MoveTimer.Dispose(); 
            }
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
        }
        private void StopTimer()
        {
            if (MoveTimer != null)
            {
                MoveTimer.Dispose();
                MoveTimer = null;
            }
        }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            StopTimer(); // Zatrzymujemy poprzedni timer, jeśli istnieje
            BallsList.Clear(); // Czyścimy poprzednie piłki

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                Vector startingPosition = new(
                    random.Next(20, 380),
                    random.Next(20, 400)
                );
                Vector initialVelocity = new(
                    (random.NextDouble() - 0.5) * 2,
                    (random.NextDouble() - 0.5) * 2
                );
                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }

            StartTimer(); // Timer uruchamiany tylko tutaj
        }


        #endregion DataAbstractAPI

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    // wyłącza timer -> zatrzymuje ruch piłek
                    MoveTimer?.Dispose(); 
                                          // czyszczenie listy piłek -> zwolnienie zasobów 
                    BallsList.Clear();
                }
                Disposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(DataImplementation));
        }

        public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      // mówi Garbage Collectorowi, żeby nie wywoływał destruktora, bo zasoby zostały już zwolnione ręcznie
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

        // losowo zmienia pozycję każdej piłki w BallsList (piłka jest przesuwana w zakresie (-5, 5) jednostek)
        private void Move(object? x)
        {
            foreach (Ball item in BallsList)
            {
                Vector currentVelocity = (Vector)item.Velocity;

                // Pobierz aktualną pozycję piłki
                Vector currentPosition = item.Position;

                // Oblicz następną pozycję dodając aktualną prędkość (nie podwajając jej jak było wcześniej)
                Vector nextPosition = new Vector(
                    currentPosition.x + currentVelocity.x,
                    currentPosition.y + currentVelocity.y
                );

                // Sprawdź i skoryguj pozycję oraz prędkość
                Vector correctedPosition = boundary.ConstrainPosition(nextPosition, ref currentVelocity);

                // Aktualizuj prędkość piłki jeśli została zmieniona
                if (!currentVelocity.Equals(item.Velocity))
                {
                    item.Velocity = currentVelocity;
                }

                // Przesuń piłkę na skorygowaną pozycję
                Vector delta = new Vector(
                    correctedPosition.x - currentPosition.x,
                    correctedPosition.y - currentPosition.y
                );

                item.Move(delta);
            }
        }
        #endregion private

        #region TestingInfrastructure

        [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}