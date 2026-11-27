
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using ProjectFlow.Views;
using MongoDB.Driver;


namespace ProjectFlow.ViewModels;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly EmployeeService _employeeService;
    private readonly ProjectService _projectService;
    private readonly TaskService _taskService;
    public int TotalEmployees => Employees.Count;

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public EmployeesViewModel()
    {
        var mongoDbService = MongoDbService.Instance;

        _employeeService = new EmployeeService(mongoDbService);
        _projectService = new ProjectService(mongoDbService);
        _taskService = new TaskService(mongoDbService);
    }


    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await ReloadEmployeesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ReloadEmployeesAsync()
    {
        var employees = await _employeeService.GetAllAsync();

        Employees.Clear();

        foreach (var employee in employees)
            Employees.Add(employee);

        OnPropertyChanged(nameof(TotalEmployees));

        StatusMessage =
            $"Загружено сотрудников: {Employees.Count}";
    }



    [RelayCommand]
    private async Task CreateEmployeeAsync()
    {
        if (IsBusy)
            return;

        var window = new EmployeeEditorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            if (await _employeeService.EmailExistsAsync(
                    window.Employee.Email))
            {
                StatusMessage =
                    "Сотрудник с таким email уже существует.";
                return;
            }

            await _employeeService.CreateAsync(window.Employee);

            await ReloadEmployeesAsync();

            StatusMessage = "Сотрудник успешно создан.";
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category ==
                  ServerErrorCategory.DuplicateKey)
        {
            StatusMessage =
                "Сотрудник с таким email уже существует.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка создания или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EditEmployeeAsync()
    {
        if (IsBusy || SelectedEmployee == null)
            return;

        var employee = SelectedEmployee;

        var window = new EmployeeEditorWindow(employee)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            if (await _employeeService.EmailExistsAsync(
                    window.Employee.Email,
                    window.Employee.Id))
            {
                StatusMessage =
                    "Сотрудник с таким email уже существует.";
                return;
            }

            bool updated = await _employeeService.UpdateAsync(
                window.Employee);

            if (!updated)
            {
                StatusMessage = "Сотрудник не найден.";
                return;
            }

            await ReloadEmployeesAsync();

            StatusMessage = "Данные сотрудника обновлены.";
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category ==
                  ServerErrorCategory.DuplicateKey)
        {
            StatusMessage =
                "Сотрудник с таким email уже существует.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка редактирования или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteEmployeeAsync()
    {
        if (IsBusy || SelectedEmployee?.Id is not string employeeId)
            return;

        string employeeName = SelectedEmployee.FullName;

        var confirmation = MessageBox.Show(
            $"Удалить сотрудника «{employeeName}»?",
            "Удаление сотрудника",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            bool isAssigned = await _projectService
                .IsEmployeeAssignedAsync(employeeId);

            if (isAssigned)
            {
                StatusMessage =
                    "Нельзя удалить сотрудника: он участвует в проекте.";
                return;
            }

            bool hasTasks = await _taskService
                .HasTasksForEmployeeAsync(employeeId);

            if (hasTasks)
            {
                StatusMessage =
                    "Нельзя удалить сотрудника: ему назначены задачи.";
                return;
            }

            bool deleted =
                await _employeeService.DeleteAsync(employeeId);

            if (!deleted)
            {
                StatusMessage = "Сотрудник не найден.";
                return;
            }

            await ReloadEmployeesAsync();

            StatusMessage = "Сотрудник удалён.";
        }
        catch (Exception ex)
        {
            StatusMessage =
                $"Ошибка удаления или обновления списка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

}
