
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using ProjectFlow.Views;

namespace ProjectFlow.ViewModels;

public partial class TasksViewModel : ObservableObject
{
    private readonly TaskService _taskService;
    private readonly ProjectService _projectService;
    private readonly EmployeeService _employeeService;
    private static string? _savedProjectFilterId;

    public ObservableCollection<TaskListItem> Tasks { get; } = new();

    public ObservableCollection<Project> Projects { get; } = new();

    [ObservableProperty]
    private TaskListItem? selectedTask;

    [ObservableProperty]
    private Project? selectedProjectFilter;

    partial void OnSelectedProjectFilterChanged(Project? value)
    {
        if (!IsBusy)
            _savedProjectFilterId = value?.Id;
    }


    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public TasksViewModel()
    {
        var mongoDbService = MongoDbService.Instance;

        _taskService = new TaskService(mongoDbService);
        _projectService = new ProjectService(mongoDbService);
        _employeeService = new EmployeeService(mongoDbService);
    }

    [RelayCommand]
    private async Task LoadTasksAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await ReloadTasksAsync();
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

    private async Task ReloadTasksAsync()
    {
        // Сохраняем выбранный проект
        string? previousFilterId =
            SelectedProjectFilter?.Id ?? _savedProjectFilterId;

        // Получаем актуальные данные MongoDB
        var projects = await _projectService.GetAllAsync();
        var employees = await _employeeService.GetAllAsync();

        // Проверяем, существует ли выбранный проект
        bool filterProjectMissing =
            previousFilterId != null &&
            !projects.Any(p => p.Id == previousFilterId);

        // Формируем справочники
        var projectNames = projects
            .Where(p => p.Id != null)
            .ToDictionary(p => p.Id!, p => p.Name);

        var employeeNames = employees
            .Where(e => e.Id != null)
            .ToDictionary(e => e.Id!, e => e.FullName);

        // Если выбранный проект исчез,
        // явно переключаемся на все задачи
        var filterId = filterProjectMissing
            ? null
            : previousFilterId;

        var tasks = filterId is string projectId
            ? await _taskService.GetByProjectAsync(projectId)
            : await _taskService.GetAllAsync();

        // Обновляем список проектов
        Projects.Clear();

        foreach (var project in projects)
            Projects.Add(project);

        // Восстанавливаем выбранный фильтр
        SelectedProjectFilter = filterId == null
            ? null
            : Projects.FirstOrDefault(p => p.Id == filterId);
        _savedProjectFilterId = SelectedProjectFilter?.Id;
        // Обновляем таблицу задач
        Tasks.Clear();

        foreach (var task in tasks)
        {
            Tasks.Add(new TaskListItem
            {
                Task = task,

                ProjectName = projectNames.GetValueOrDefault(
                    task.ProjectId,
                    "Неизвестный проект"),

                AssigneeName = task.AssigneeId == null
                    ? "Не назначен"
                    : employeeNames.GetValueOrDefault(
                        task.AssigneeId,
                        "Неизвестный сотрудник")
            });
        }

        // Отображаем результат
        if (filterProjectMissing)
        {
            StatusMessage =
                "Выбранный проект больше не существует. " +
                "Фильтр сброшен. " +
                $"Показано задач: {Tasks.Count}";
        }
        else
        {
            StatusMessage =
                $"Загружено задач: {Tasks.Count}";
        }
    }

    [RelayCommand]
    private async Task ApplyFilterAsync()
    {
        if (IsBusy)
            return;

        await LoadTasksAsync();
    }

    [RelayCommand]
    private async Task ResetFilterAsync()
    {
        if (IsBusy)
            return;

        _savedProjectFilterId = null;
        SelectedProjectFilter = null;

        await LoadTasksAsync();
    }


    [RelayCommand]
    private async Task CreateTaskAsync()
    {
        if (IsBusy)
            return;

        var window = new TaskEditorWindow
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
            await _taskService.CreateAsync(window.Task);

            await ReloadTasksAsync();

            StatusMessage = "Задача успешно создана.";
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
    private async Task EditTaskAsync()
    {
        if (IsBusy || SelectedTask == null)
            return;

        var selectedTask = SelectedTask.Task;

        var window = new TaskEditorWindow(selectedTask)
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
            bool updated =
                await _taskService.UpdateAsync(window.Task);

            if (!updated)
            {
                StatusMessage = "Задача не найдена.";
                return;
            }

            await ReloadTasksAsync();

            StatusMessage = "Задача успешно обновлена.";
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
    private async Task DeleteTaskAsync()
    {
        if (IsBusy || SelectedTask?.Task.Id is not string taskId)
            return;

        string taskTitle = SelectedTask.Title;

        var confirmation = MessageBox.Show(
            $"Удалить задачу «{taskTitle}»?",
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
            bool deleted =
                await _taskService.DeleteAsync(taskId);

            if (!deleted)
            {
                StatusMessage = "Задача не найдена.";
                return;
            }

            await ReloadTasksAsync();

            StatusMessage = "Задача удалена.";
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
