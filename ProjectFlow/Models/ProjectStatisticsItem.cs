
namespace ProjectFlow.Models;

public class ProjectStatisticsItem
{
    public string ProjectName { get; set; } = string.Empty;

    public int TotalTasks { get; set; }

    public int CompletedTasks { get; set; }

    public int ActiveTasks { get; set; }

    public int OverdueTasks { get; set; }

    public double ProgressPercentage =>
        TotalTasks == 0
            ? 0
            : Math.Round(
                (double)CompletedTasks / TotalTasks * 100, 1);

}
