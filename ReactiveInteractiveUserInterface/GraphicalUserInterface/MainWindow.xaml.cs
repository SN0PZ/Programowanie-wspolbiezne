//__________________________________________________________________________________________
//
//  Copyright 2024 Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and to get started
//  comment using the discussion panel at
//  https://github.com/mpostol/TP/discussions/182
//__________________________________________________________________________________________

using System;
using System.Windows;
using System.Windows.Controls;
using TP.ConcurrentProgramming.Presentation.ViewModel;

namespace TP.ConcurrentProgramming.PresentationView
{
    /// <summary>
    /// View implementation
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                btn.IsEnabled = false;

            if (DataContext is MainWindowViewModel viewModel)
            {
                const double width = 395;
                const double height = 415;
                viewModel.StartSimulationWithSize(width, height);
            }
        }
        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
                vm.Dispose();
            base.OnClosed(e);
        }
    }
}
