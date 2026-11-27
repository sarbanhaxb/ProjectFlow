
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Helpers;
using ProjectFlow.Models;
using ProjectFlow.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TaskLevel = ProjectFlow.Models.Enums.TaskPriority;
using TaskState = ProjectFlow.Models.Enums.TaskStatus;

namespace ProjectFlow.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    private readonly ProjectService _projectService;
    private readonly TaskService _taskService;

    public ObservableCollection<ProjectStatisticsItem> ProjectStatistics
    { get; } = new();

    [ObservableProperty]
    private int totalTasks;

    [ObservableProperty]
    private int plannedTasks;

    [ObservableProperty]
    private int activeTasks;

    [ObservableProperty]
    private int completedTasks;

    [ObservableProperty]
    private int overdueTasks;

    [ObservableProperty]
    private int highPriorityTasks;

    [ObservableProperty]
    private double overallProgress;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public StatisticsViewModel()
    {
        var mongoDbService = MongoDbService.Instance;

        _projectService = new ProjectService(mongoDbService);
        _taskService = new TaskService(mongoDbService);
    }

    [RelayCommand]
    private async Task LoadStatisticsAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        StatusMessage = "Загрузка статистики...";

        try
        {
            var projects = await _projectService.GetAllAsync();
            var tasks = await _taskService.GetAllAsync();

            DateTime today = DateTime.UtcNow.Date;

            TotalTasks = tasks.Count;

            PlannedTasks = tasks.Count(t =>
                t.Status == TaskState.ToDo);

            ActiveTasks = tasks.Count(t =>
                t.Status == TaskState.InProgress);

            CompletedTasks = tasks.Count(t =>
                t.Status == TaskState.Completed);

            OverdueTasks = tasks.Count(t =>
                DateHelper.IsOverdue(
                    t.DueDate,
                    t.Status == TaskState.Completed));

            HighPriorityTasks = tasks.Count(t =>
                t.Priority == TaskLevel.High ||
                t.Priority == TaskLevel.Critical);

            OverallProgress = TotalTasks == 0
                ? 0
                : Math.Round(
                    (double)CompletedTasks / TotalTasks * 100, 1);

            var groupedTasks = tasks
                .GroupBy(t => t.ProjectId)
                .ToDictionary(g => g.Key, g => g.ToList());

            ProjectStatistics.Clear();

            foreach (var project in projects.OrderBy(p => p.Name))
            {
                var projectTasks =
                    project.Id != null &&
                    groupedTasks.TryGetValue(project.Id, out var found)
                        ? found
                        : new System.Collections.Generic.List<TaskItem>();

                ProjectStatistics.Add(new ProjectStatisticsItem
                {
                    ProjectName = project.Name,

                    TotalTasks = projectTasks.Count,

                    CompletedTasks = projectTasks.Count(t =>
                        t.Status == TaskState.Completed),

                    ActiveTasks = projectTasks.Count(t =>
                        t.Status == TaskState.InProgress),

                    OverdueTasks = projectTasks.Count(t =>
                        DateHelper.IsOverdue(
                            t.DueDate,
                            t.Status == TaskState.Completed))
                });
            }

            StatusMessage = "Статистика обновлена.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
