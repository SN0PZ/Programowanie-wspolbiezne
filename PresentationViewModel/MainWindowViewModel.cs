//Klasa jest pośrednikiem między interfejsem użytkownika a warstwą logiki aplikacji

using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using TP.ConcurrentProgramming.Presentation.Model;
using TP.ConcurrentProgramming.Presentation.ViewModel.MVVMLight;
using ModelIBall = TP.ConcurrentProgramming.Presentation.Model.IBall;

namespace TP.ConcurrentProgramming.Presentation.ViewModel
{
  public class MainWindowViewModel : ViewModelBase, IDisposable
  {
        #region ctor
        private int numberOfBalls = 10;
        private string numberInput = "10";
        public string NumberInput
        {
            get => numberInput;
            set
            {
                numberInput = value;
                if (int.TryParse(value, out int number) && number >= 0)
                {
                    NumberOfBalls = number;
                }
                RaisePropertyChanged();
            }
        }

        public int NumberOfBalls
        {
            get => numberOfBalls;
            private set // Zmiana na private set
            {
                if (value >= 0)
                {
                    numberOfBalls = value;
                    RaisePropertyChanged();
                    (StartCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }


        private RelayCommand? startCommand;
        public ICommand StartCommand
        {
            get
            {
                return startCommand ??= new RelayCommand(
                    () => Start(NumberOfBalls), // Wywołanie Start
                    () => NumberOfBalls > 0 && !Disposed && int.TryParse(NumberInput, out _)
                );
            }
        }

        private ICommand? stopCommand;
        public ICommand StopCommand
        {
            get
            {
                return stopCommand ??= new RelayCommand(
                    () => Dispose(),
                    () => !Disposed
                );
            }
        }
        public MainWindowViewModel() : this(null)
    { }

        internal MainWindowViewModel(ModelAbstractApi modelLayerAPI)
        {
            // Inicjalizujemy tylko ModelLayer, bez tworzenia Observer
            ModelLayer = modelLayerAPI ?? ModelAbstractApi.CreateModel();
        }

        #endregion ctor

        #region public API

        public void Start(int numberOfBalls)
        {
            if (Disposed)
                throw new ObjectDisposedException(nameof(MainWindowViewModel));

            try
            {
                // Czyszczenie poprzedniego stanu
                Balls.Clear();
                Observer?.Dispose();

                // Tworzenie nowej subskrypcji przed startem
                Observer = ModelLayer.Subscribe<ModelIBall>(x =>
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(() => Balls.Add(x));
                    }
                });

                // Uruchomienie symulacji
                ModelLayer.Start(numberOfBalls);
            }
            catch (Exception)
            {
                Observer?.Dispose();
                Observer = null;
                throw;
            }
        }


        public ObservableCollection<ModelIBall> Balls { get; } = new ObservableCollection<ModelIBall>();

        #endregion public API

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!Disposed)
            {
                if (disposing)
                {
                    Balls.Clear();
                    Observer?.Dispose();  // Dodajemy operator ?, bo Observer może być null
                    ModelLayer.Dispose();
                }
                Disposed = true;
            }
        }

        public void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(MainWindowViewModel));
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

        #endregion IDisposable

        #region private

        private IDisposable? Observer;
        private readonly ModelAbstractApi ModelLayer;
        private bool Disposed;

        #endregion private
    }
}