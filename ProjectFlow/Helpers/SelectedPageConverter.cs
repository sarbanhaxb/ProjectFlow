
using System;
using System.Globalization;
using System.Windows.Data;

namespace ProjectFlow.Helpers;

public class SelectedPageConverter : IValueConverter
{
    public object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is string currentPage &&
            parameter is string buttonPage)
        {
            return currentPage == buttonPage;
        }

        return false;
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
