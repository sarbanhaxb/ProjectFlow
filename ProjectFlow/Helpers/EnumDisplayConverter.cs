
using System;
using System.Globalization;
using System.Windows.Data;
using ProjectFlow.Models.Enums;

namespace ProjectFlow.Helpers;

public class EnumDisplayConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return value switch
        {
            ProjectStatus.Planned => "Запланирован",
            ProjectStatus.InProgress => "В работе",
            ProjectStatus.Completed => "Завершён",
            ProjectStatus.OnHold => "Приостановлен",

            ProjectFlow.Models.Enums.TaskStatus.ToDo =>
                "К выполнению",

            ProjectFlow.Models.Enums.TaskStatus.InProgress =>
                "В работе",

            ProjectFlow.Models.Enums.TaskStatus.Completed =>
                "Выполнена",

            TaskPriority.Low => "Низкий",
            TaskPriority.Medium => "Средний",
            TaskPriority.High => "Высокий",
            TaskPriority.Critical => "Критический",

            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
