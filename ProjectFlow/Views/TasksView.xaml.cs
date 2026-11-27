
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.ViewModels;

namespace ProjectFlow.Views;

public partial class TasksView : UserControl
{
    public TasksView()
    {
        InitializeComponent();
        Loaded += TasksView_Loaded;
    }

    private async void TasksView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is TasksViewModel viewModel)
        {
            await viewModel.LoadTasksCommand.ExecuteAsync(null);
        }
    }
}
