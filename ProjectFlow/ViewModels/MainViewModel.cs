using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Services;

namespace ProjectFlow.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly MongoDbService _mongoDbService = new();

    [ObservableProperty]
    private string pageTitle = "Главная панель";

    [ObservableProperty]
    private ObservableObject currentPage = new DashboardViewModel();

    [ObservableProperty]
    private string databaseStatus = "Подключение не проверено";


    [RelayCommand]
    private async Task CheckDatabaseAsync()
    {
        DatabaseStatus = "Проверка подключения...";

        bool connected = await _mongoDbService.CheckConnectionAsync();

        DatabaseStatus = connected ? "MongoDB подключена" : "Ошибка подключения к MongoDB";
    }

    [RelayCommand]
    private void Navigate(string page)
    {
        PageTitle = page;

        CurrentPage = page switch
        {
            "Главная" => new DashboardViewModel(),

            "Проекты" => new ProjectsViewModel(),

            "Задачи" => new PlaceholderViewModel(
                "Раздел управления задачами"),

            "Сотрудники" => new PlaceholderViewModel(
                "Раздел управления сотрудниками"),

            "Статистика" => new PlaceholderViewModel(
                "Раздел статистики"),

            _ => new DashboardViewModel()
        };
    }
}