using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Services;

namespace ProjectFlow.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly MongoDbService _mongoDbService =
        MongoDbService.Instance;

    [ObservableProperty]
    private string pageTitle = "Главная панель";

    [ObservableProperty]
    private ObservableObject? currentPage;

    [ObservableProperty]
    private bool isDatabaseReady;
    [ObservableProperty]
    private bool isConnecting;

    [ObservableProperty]
    private string databaseStatus = "Подключение не проверено";

    [ObservableProperty]
    private string selectedPage = "Главная";



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
        if (!IsDatabaseReady)
            return;

        SelectedPage = page;
        PageTitle = page;

        CurrentPage = page switch
        {
            "Главная" => new DashboardViewModel(),

            "Проекты" => new ProjectsViewModel(),

            "Задачи" => new TasksViewModel(),

            "Сотрудники" => new EmployeesViewModel(),

            "Статистика" => new StatisticsViewModel(),

            _ => new DashboardViewModel()
        };
    }
}