
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.ViewModels;

namespace ProjectFlow.Views;

public partial class EmployeesView : UserControl
{
    public EmployeesView()
    {
        InitializeComponent();

        Loaded += EmployeesView_Loaded;
    }

    private async void EmployeesView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is EmployeesViewModel viewModel)
        {
            await viewModel.LoadEmployeesCommand
                .ExecuteAsync(null);
        }
    }
}
