
namespace ProjectFlow.Helpers;

public static class DateHelper
{
    public static DateTime ToUtcDate(DateTime date)
    {
        return DateTime.SpecifyKind(
            date.Date,
            DateTimeKind.Utc);
    }

    public static DateTime? ToUtcDate(DateTime? date)
    {
        return date.HasValue
            ? ToUtcDate(date.Value)
            : null;
    }

    public static bool IsOverdue(
        DateTime? dueDate,
        bool isCompleted)
    {
        return !isCompleted &&
               dueDate.HasValue &&
               dueDate.Value.Date < DateTime.Today;
    }
}
