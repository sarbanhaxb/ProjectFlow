
namespace ProjectFlow.Models;

public class ProjectListItem
{
    public Project Project { get; set; } = new();

    public string Id => Project.Id ?? string.Empty;

    public string Name => Project.Name;

    public string Description => Project.Description;


    public string Status => Project.Status switch
    {
        ProjectFlow.Models.Enums.ProjectStatus.Planned =>
            "Запланирован",

        ProjectFlow.Models.Enums.ProjectStatus.InProgress =>
            "В работе",

        ProjectFlow.Models.Enums.ProjectStatus.Completed =>
            "Завершён",

        ProjectFlow.Models.Enums.ProjectStatus.OnHold =>
            "Приостановлен",

        _ => "Неизвестно"
    };

    public DateTime StartDate => Project.StartDate;

    public DateTime? EndDate => Project.EndDate;

    public string Members { get; set; } = string.Empty;

    public int TaskCount { get; set; }

    public int CompletedTaskCount { get; set; }

    public double ProgressPercentage =>
        TaskCount == 0
            ? 0
            : Math.Round(
                (double)CompletedTaskCount / TaskCount * 100, 1);

}
