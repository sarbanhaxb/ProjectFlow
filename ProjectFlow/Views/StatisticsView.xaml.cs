
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.ViewModels;

namespace ProjectFlow.Views;

public partial class StatisticsView : UserControl
{
    public StatisticsView()
    {
        InitializeComponent();

        Loaded += StatisticsView_Loaded;
    }

    private async void StatisticsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is StatisticsViewModel viewModel)
        {
            await viewModel.LoadStatisticsCommand
                .ExecuteAsync(null);
        }
    }
}