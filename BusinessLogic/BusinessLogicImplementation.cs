// Wartswa logiki biznesowaj, która zarządza cyklem życia obiektów, aby uniknąć wycieków pamięci
// Pośredniczy między warstwą danych a górną warstwą aplikacji, mapując obiekty z jednej warstwy na drugą

using System.Diagnostics;
// alias dla API niższej warstwy (UnderneathLayerAPI), które reprezentuje dostęp do warstwy danych
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  // internal – klasa jest dostępna tylko w obrębie bieżącego zestawu (assembly)
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {
    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      // jeśli warstwa niższa (underneathLayer) nie jest przekazana (null), to instancja zostaje pobrana przez UnderneathLayerAPI.GetDataLayer()
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override void Dispose()
    {
     // czy obiekt został już zniszczony
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      // wywołuje metodę Dispose() na warstwie poniżej - zwalnai zasoby
      layerBellow.Dispose();
      Disposed = true;
    }

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      if (upperLayerHandler == null)
        throw new ArgumentNullException(nameof(upperLayerHandler));
      // wywołuje Start w warstwie danych (layerBellow), przekazując liczbę piłek
      // upperLayerHandler mapuje obiekt startingPosition na Position oraz databall na Ball - transformacja obiektów między warstwami
      layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall)));
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    // instancja warstwy niższej 
    private readonly UnderneathLayerAPI layerBellow;

    #endregion private

    #region TestingInfrastructure

    // metoda jest dostępna tylko w trybie debugowania
    [Conditional("DEBUG")]
    
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      // sprawdza, czy obiekt został zniszczony
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}