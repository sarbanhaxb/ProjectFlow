
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.ViewModels;

namespace ProjectFlow.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += DashboardView_Loaded;
    }

    private async void DashboardView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel viewModel)
        {
            await viewModel.LoadDashboardCommand
                .ExecuteAsync(null);
        }
    }
}
