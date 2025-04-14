// Implementacja Modelu MVVM:
// 1) Łączy warstwę Presentation (UI) z BusinessLogic
// 2) Obsługuje zdarzenia ruchu piłek (BallChanged)
// 3) Umożliwia reaktywne programowanie (IObservable<IBall>) - reagowanie na zmiany danych i zdarzeń
// 4) Dba o poprawne zarządzanie pamięcią (Dispose)

using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using UnderneathLayerAPI = TP.ConcurrentProgramming.BusinessLogic.BusinessLogicAbstractAPI;

namespace TP.ConcurrentProgramming.Presentation.Model
{
  /// <summary>
  /// Class Model - implements the <see cref="ModelAbstractApi" />
  /// </summary>
  internal class ModelImplementation : ModelAbstractApi
  {
    internal ModelImplementation() : this(null)
    { }

    internal ModelImplementation(UnderneathLayerAPI underneathLayer)
    {
      // pobiera warstwę logiki
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetBusinessLogicLayer() : underneathLayer;
      // pozwala subskrybować zdarzenia o ruchu piłek
      eventObservable = Observable.FromEventPattern<BallChaneEventArgs>(this, "BallChanged");
    }

    #region ModelAbstractApi

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(Model));
      layerBellow.Dispose();
      Disposed = true;
    }

    // gdy zdarzenie BallChanged wystąpi, wywołuje OnNext(), obsługuje błędy OnError() i zakończenie OnCompleted()
      public override IDisposable Subscribe(IObserver<IBall> observer)
    {
      return eventObservable.Subscribe(x => observer.OnNext(x.EventArgs.Ball), ex => observer.OnError(ex), () => observer.OnCompleted());
    }

    // gdy piłka zostanie dodana, warstwa dostanie o tym powiadomione
    // tartHandler -  metoda, która obsługuje dodawanie nowych piłek do modelu
    public override void Start(int numberOfBalls)
    {
      layerBellow.Start(numberOfBalls, StartHandler);
    }

    #endregion ModelAbstractApi

    #region API

    public event EventHandler<BallChaneEventArgs> BallChanged;

    #endregion API

    #region private

    private bool Disposed = false;
    private readonly IObservable<EventPattern<BallChaneEventArgs>> eventObservable = null;
    private readonly UnderneathLayerAPI layerBellow = null;

    // tworzy nową piłkę (ModelBall) na podstawie danych z BusinessLogic i powiadamia o tym za pomocą BallChanged
    private void StartHandler(BusinessLogic.IPosition position, BusinessLogic.IBall ball)
    {
      ModelBall newBall = new ModelBall(position.x, position.y, ball) { Diameter = 20.0 };
      BallChanged.Invoke(this, new BallChaneEventArgs() { Ball = newBall });
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    [Conditional("DEBUG")]
    internal void CheckUnderneathLayerAPI(Action<UnderneathLayerAPI> returnNumberOfBalls)
    {
      returnNumberOfBalls(layerBellow);
    }

    [Conditional("DEBUG")]
    internal void CheckBallChangedEvent(Action<bool> returnBallChangedIsNull)
    {
      returnBallChangedIsNull(BallChanged == null);
    }

    #endregion TestingInfrastructure
  }

  public class BallChaneEventArgs : EventArgs
  {
    public IBall Ball { get; init; }
  }
}