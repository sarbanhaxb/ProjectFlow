
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.ViewModels;

namespace ProjectFlow.Views;

public partial class ProjectsView : UserControl
{
    public ProjectsView()
    {
        InitializeComponent();

        Loaded += ProjectsView_Loaded;
    }

    private async void ProjectsView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        if (DataContext is ProjectsViewModel viewModel)
        {
            await viewModel.LoadProjectsCommand
                .ExecuteAsync(null);
        }
    }
}
