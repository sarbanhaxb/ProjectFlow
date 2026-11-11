
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using ProjectFlow.Views;

namespace ProjectFlow.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    public ObservableCollection<Project> Projects { get; } = new();

    [ObservableProperty]
    private Project? selectedProject;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public ProjectsViewModel()
    {
        var mongoDbService = new MongoDbService();
        _projectService = new ProjectService(mongoDbService);
    }

    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var projects = await _projectService.GetAllAsync();

            Projects.Clear();

            foreach (var project in projects)
                Projects.Add(project);

            StatusMessage = $"Загружено проектов: {Projects.Count}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        var window = new ProjectEditorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        try
        {
            await _projectService.CreateAsync(window.Project);
            await LoadProjectsAsync();

            StatusMessage = "Проект успешно создан.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка создания: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditProjectAsync()
    {
        if (SelectedProject == null)
            return;

        var window = new ProjectEditorWindow(SelectedProject)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        try
        {
            bool updated =
                await _projectService.UpdateAsync(window.Project);

            if (updated)
            {
                await LoadProjectsAsync();
                StatusMessage = "Проект успешно обновлён.";
            }
            else
            {
                StatusMessage = "Проект не найден.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка обновления: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteProjectAsync()
    {
        if (SelectedProject?.Id == null)
            return;

        var result = MessageBox.Show(
            $"Удалить проект «{SelectedProject.Name}»?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            bool deleted =
                await _projectService.DeleteAsync(SelectedProject.Id);

            if (deleted)
            {
                await LoadProjectsAsync();
                StatusMessage = "Проект удалён.";
            }
            else
            {
                StatusMessage = "Проект не найден.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }
}
