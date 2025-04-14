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


        public DataImplementation()
    {
            boundary = new Boundary(0, 380, 0, 400);
            // timer (MoveTimer) co 100 ms wywołuje metodę Move(), aby przesuwać piłki
            MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(20));
    }

        #endregion ctor

        #region DataAbstractAPI

        public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(DataImplementation));
            if (upperLayerHandler == null)
                throw new ArgumentNullException(nameof(upperLayerHandler));

            Random random = new Random();
            for (int i = 0; i < numberOfBalls; i++)
            {
                // Generujemy pozycję początkową w granicach obszaru
                Vector startingPosition = new(
                    random.Next(20, 380),  // x: od 20 do 380
                    random.Next(20, 400)   // y: od 20 do 400
                );
                // Ustawiamy mniejszą prędkość początkową dla bardziej kontrolowanego ruchu
                Vector initialVelocity = new(
                    (random.NextDouble() - 0.5) * 2,  // Prędkość x: -1 do 1
                    (random.NextDouble() - 0.5) * 2   // Prędkość y: -1 do 1
                );
                Ball newBall = new(startingPosition, initialVelocity);
                upperLayerHandler(startingPosition, newBall);
                BallsList.Add(newBall);
            }
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
          MoveTimer.Dispose();
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

    private readonly Timer MoveTimer;
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