using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ProjectFlow.ViewModels;

public partial class ProjectsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;

    public ObservableCollection<Project> Projects { get; } = new();

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
        catch(Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AddTestProjectAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var project = new Project
            {
                Name = "Разработка информационной системы",
                Description = "Тестовый проект ProjectFlow",
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(2),
                Status = Models.Enums.ProjectStatus.InProgress
            };

            await _projectService.CreateAsync(project);

            Projects.Add(project);

            StatusMessage = "Проект успешно добавлен";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ощибка создания: {ex.Message}";
        }
        finally { IsBusy = false; }
    }
}