
using System;
using System.Threading.Tasks;
using System.Windows;
using ProjectFlow.Services;
using ProjectFlow.ViewModels;

namespace ProjectFlow;

public partial class MainWindow : Window
{
    private bool _isInitializing;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainViewModel();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        Loaded -= MainWindow_Loaded;

        await InitializeDatabaseAsync();
    }


    private async Task InitializeDatabaseAsync()
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        // Не запускаем повторную инициализацию,
        // если подключение уже выполняется
        // или база успешно подключена.
        if (_isInitializing || viewModel.IsDatabaseReady)
            return;

        _isInitializing = true;
        viewModel.IsConnecting = true;

        try
        {
            viewModel.DatabaseStatus =
                "Подключение к MongoDB...";

            var mongoDbService = MongoDbService.Instance;

            bool connected =
                await mongoDbService.CheckConnectionAsync();

            if (!connected)
            {
                viewModel.DatabaseStatus =
                    "MongoDB недоступна";

                MessageBox.Show(
                    "Не удалось подключиться к MongoDB.\n" +
                    "Проверьте состояние службы MongoDB.",
                    "Ошибка подключения",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            viewModel.DatabaseStatus =
                "Проверка индексов...";

            var indexService =
                new MongoDbIndexService(mongoDbService);

            await indexService.CreateIndexesAsync();

            // Инициализация завершена успешно
            viewModel.IsDatabaseReady = true;

            viewModel.DatabaseStatus =
                "MongoDB подключена";

            // Открываем Dashboard только при
            // первоначальном успешном подключении
            viewModel.NavigateCommand.Execute("Главная");
        }
        catch (Exception ex)
        {
            viewModel.DatabaseStatus =
                "Ошибка инициализации MongoDB";

            MessageBox.Show(
                $"Ошибка настройки базы данных:\n\n{ex.Message}",
                "ProjectFlow",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isInitializing = false;
            viewModel.IsConnecting = false;
        }
    }


    private async void RetryConnection_Click(
        object sender,
        RoutedEventArgs e)
    {
        await InitializeDatabaseAsync();
    }
}
