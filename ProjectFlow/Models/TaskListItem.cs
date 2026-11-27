
using ProjectFlow.Helpers;

namespace ProjectFlow.Models;

public class TaskListItem
{
    public TaskItem Task { get; set; } = new();

    public string Title => Task.Title;

    public string Description => Task.Description;

    public string ProjectName { get; set; } = "";

    public string AssigneeName { get; set; } = "";

    public string Status => Task.Status switch
    {
        Models.Enums.TaskStatus.ToDo => "К выполнению",
        Models.Enums.TaskStatus.InProgress => "В работе",
        Models.Enums.TaskStatus.Completed => "Выполнена",
        _ => "Неизвестно"
    };

    public string Priority => Task.Priority switch
    {
        Models.Enums.TaskPriority.Low => "Низкий",
        Models.Enums.TaskPriority.Medium => "Средний",
        Models.Enums.TaskPriority.High => "Высокий",
        Models.Enums.TaskPriority.Critical => "Критический",
        _ => "Неизвестно"
    };

    public DateTime? DueDate => Task.DueDate;

    public bool IsOverdue => DateHelper.IsOverdue(
        Task.DueDate,
        Task.Status == Models.Enums.TaskStatus.Completed);
}
