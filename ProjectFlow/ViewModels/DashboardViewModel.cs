
using System.Collections.ObjectModel;
using ProjectFlow.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using TaskState = ProjectFlow.Models.Enums.TaskStatus;

namespace ProjectFlow.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly ProjectService _projectService;
    private readonly EmployeeService _employeeService;
    private readonly TaskService _taskService;

    public ObservableCollection<ProjectListItem> RecentProjects
    { get; } = new();

    [ObservableProperty]
    private int totalProjects;

    [ObservableProperty]
    private int activeTasks;

    [ObservableProperty]
    private int completedTasks;

    [ObservableProperty]
    private int overdueTasks;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;


    [ObservableProperty]
    private int plannedTasks;




    public DashboardViewModel()
    {
        var mongoDbService = MongoDbService.Instance;

        _projectService = new ProjectService(mongoDbService);
        _employeeService = new EmployeeService(mongoDbService);
        _taskService = new TaskService(mongoDbService);
    }

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StatusMessage = "Загрузка статистики...";

        try
        {
            var projects = await _projectService.GetAllAsync();
            var employees = await _employeeService.GetAllAsync();
            var tasks = await _taskService.GetAllAsync();

            // Общая статистика
            int total = projects.Count;
            int planned = tasks.Count(t =>
    t.Status == TaskState.ToDo);
            int active = tasks.Count(t =>
                t.Status == TaskState.InProgress);

            int completed = tasks.Count(t =>
                t.Status == TaskState.Completed);

            DateTime today = DateTime.UtcNow.Date;

            int overdue = tasks.Count(t =>
                DateHelper.IsOverdue(
                    t.DueDate,
                    t.Status == TaskState.Completed));

            // Словарь сотрудников
            var employeeNames = employees
                .Where(e => !string.IsNullOrEmpty(e.Id))
                .ToDictionary(e => e.Id!, e => e.FullName);

            // Группируем задачи по проектам
            var taskGroups = tasks
                .GroupBy(t => t.ProjectId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Total = g.Count(),
                        Completed = g.Count(t =>
                            t.Status == TaskState.Completed)
                    });

            // Формируем таблицу проектов
            var recent = new System.Collections.Generic
                .List<ProjectListItem>();

            foreach (var project in projects
                .OrderByDescending(p => p.StartDate)
                .Take(6))
            {
                var members = project.MemberIds
                    .Where(id => employeeNames.ContainsKey(id))
                    .Select(id => employeeNames[id]);

                int taskCount = 0;
                int completedCount = 0;

                if (project.Id != null &&
                    taskGroups.TryGetValue(
                        project.Id, out var statistics))
                {
                    taskCount = statistics.Total;
                    completedCount = statistics.Completed;
                }

                recent.Add(new ProjectListItem
                {
                    Project = project,
                    Members = string.Join(", ", members),
                    TaskCount = taskCount,
                    CompletedTaskCount = completedCount
                });
            }

            // Обновляем интерфейс только после расчётов
            TotalProjects = total;
            ActiveTasks = active;
            CompletedTasks = completed;
            OverdueTasks = overdue;
            PlannedTasks = planned;

            RecentProjects.Clear();

            foreach (var item in recent)
                RecentProjects.Add(item);

            StatusMessage = "Статистика обновлена.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка загрузки статистики: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    public int TotalTasks =>
        PlannedTasks + ActiveTasks + CompletedTasks;

    public double PlannedPercentage =>
        CalculatePercentage(PlannedTasks);

    public double ActivePercentage =>
        CalculatePercentage(ActiveTasks);

    public double CompletedPercentage =>
        CalculatePercentage(CompletedTasks);

    private double CalculatePercentage(int count)
    {
        return TotalTasks == 0
            ? 0
            : Math.Round((double)count / TotalTasks * 100, 1);
    }


    partial void OnPlannedTasksChanged(int value)
    {
        NotifyChartChanged();
    }

    partial void OnActiveTasksChanged(int value)
    {
        NotifyChartChanged();
    }

    partial void OnCompletedTasksChanged(int value)
    {
        NotifyChartChanged();
    }

    private void NotifyChartChanged()
    {
        OnPropertyChanged(nameof(TotalTasks));
        OnPropertyChanged(nameof(PlannedPercentage));
        OnPropertyChanged(nameof(ActivePercentage));
        OnPropertyChanged(nameof(CompletedPercentage));
    }

}

