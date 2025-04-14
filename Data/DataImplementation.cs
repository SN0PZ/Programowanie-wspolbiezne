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

    public DataImplementation()
    {
      // timer (MoveTimer) co 100 ms wywołuje metodę Move(), aby przesuwać piłki
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    #endregion ctor

    #region DataAbstractAPI

    public override void Start(int numberOfBalls, Action<IVector, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(DataImplementation));
      // czy przekazano delegata
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      Random random = new Random();
      for (int i = 0; i < numberOfBalls; i++)
      {
        Vector startingPosition = new(random.Next(100, 400 - 100), random.Next(100, 400 - 100));
        Ball newBall = new(startingPosition, startingPosition);
        // pozwala warstwie biznesowej otrzymywać powiadomienia o nowo utworzonych piłkach
        upperLayerHandler(startingPosition, newBall);
        // dodanie piłki do listy
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
        item.Move(new Vector((RandomGenerator.NextDouble() - 0.5) * 10, (RandomGenerator.NextDouble() - 0.5) * 10));
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