
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
    private readonly EmployeeService _employeeService;
    private readonly TaskService _taskService;
    public ObservableCollection<ProjectListItem> Projects { get; } = new();

    [ObservableProperty]
    private ProjectListItem? selectedProject;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public ProjectsViewModel()
    {
        var mongoDbService = MongoDbService.Instance;

        _projectService = new ProjectService(mongoDbService);
        _employeeService = new EmployeeService(mongoDbService);
        _taskService = new TaskService(mongoDbService);
    }


    [RelayCommand]
    private async Task LoadProjectsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await ReloadProjectsAsync();
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

    private async Task ReloadProjectsAsync()
    {
        var projects = await _projectService.GetAllAsync();
        var employees = await _employeeService.GetAllAsync();

        var employeeDictionary = employees
            .Where(e => e.Id != null)
            .ToDictionary(e => e.Id!, e => e.FullName);

        var items = new List<ProjectListItem>();

        foreach (var project in projects)
        {
            var memberNames = project.MemberIds
                .Where(id => employeeDictionary.ContainsKey(id))
                .Select(id => employeeDictionary[id]);

            items.Add(new ProjectListItem
            {
                Project = project,
                Members = string.Join(", ", memberNames)
            });
        }

        Projects.Clear();

        foreach (var item in items)
            Projects.Add(item);

        StatusMessage = $"Загружено проектов: {Projects.Count}";
    }




    [RelayCommand]
    private async Task CreateProjectAsync()
    {
        if (IsBusy)
            return;

        var window = new ProjectEditorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await _projectService.CreateAsync(window.Project);

            await ReloadProjectsAsync();

            StatusMessage = "Проект успешно создан.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка создания или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }


    [RelayCommand]
    private async Task EditProjectAsync()
    {
        if (IsBusy || SelectedProject == null)
            return;

        var selectedProject = SelectedProject.Project;

        var window = new ProjectEditorWindow(selectedProject)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var projectTasks = await _taskService.GetByProjectAsync(
                window.Project.Id!);

            var invalidTasks = projectTasks
                .Where(t =>
                    !string.IsNullOrEmpty(t.AssigneeId) &&
                    !window.Project.MemberIds.Contains(t.AssigneeId))
                .ToList();

            if (invalidTasks.Count > 0)
            {
                MessageBox.Show(
                    "Невозможно сохранить проект.\n\n" +
                    "Исключённые сотрудники являются " +
                    "исполнителями задач.\n" +
                    "Сначала переназначьте эти задачи.",
                    "Проверка участников проекта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            bool updated = await _projectService.UpdateAsync(
                window.Project);

            if (!updated)
            {
                StatusMessage = "Проект не найден.";
                return;
            }

            await ReloadProjectsAsync();

            StatusMessage = "Проект успешно обновлён.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка редактирования или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }



    [RelayCommand]
    private async Task DeleteProjectAsync()
    {
        if (IsBusy || SelectedProject?.Id is not string projectId)
            return;

        string projectName = SelectedProject.Name;

        var confirmation = MessageBox.Show(
            $"Удалить проект «{projectName}»?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            bool hasTasks =
                await _taskService.HasTasksForProjectAsync(projectId);

            if (hasTasks)
            {
                StatusMessage =
                    "Нельзя удалить проект, пока в нём есть задачи.";
                return;
            }

            bool deleted = await _projectService.DeleteAsync(projectId);

            if (!deleted)
            {
                StatusMessage = "Проект не найден.";
                return;
            }

            await ReloadProjectsAsync();

            StatusMessage = "Проект удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка удаления или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }


}
