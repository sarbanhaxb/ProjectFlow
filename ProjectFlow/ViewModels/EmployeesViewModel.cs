
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProjectFlow.Models;
using ProjectFlow.Services;
using ProjectFlow.Views;

namespace ProjectFlow.ViewModels;

public partial class EmployeesViewModel : ObservableObject
{
    private readonly EmployeeService _employeeService;

    public ObservableCollection<Employee> Employees { get; } = new();

    [ObservableProperty]
    private Employee? selectedEmployee;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public EmployeesViewModel()
    {
        _employeeService = new EmployeeService(
            new MongoDbService());
    }

    [RelayCommand]
    private async Task LoadEmployeesAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var employees = await _employeeService.GetAllAsync();

            Employees.Clear();

            foreach (var employee in employees)
                Employees.Add(employee);

            StatusMessage =
                $"Загружено сотрудников: {Employees.Count}";
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

    [RelayCommand]
    private async Task CreateEmployeeAsync()
    {
        var window = new EmployeeEditorWindow
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        try
        {
            if (await _employeeService.EmailExistsAsync(
                    window.Employee.Email))
            {
                StatusMessage = "Сотрудник с таким email уже существует.";
                return;
            }

            await _employeeService.CreateAsync(window.Employee);
            await LoadEmployeesAsync();

            StatusMessage = "Сотрудник успешно создан.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка создания: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task EditEmployeeAsync()
    {
        if (SelectedEmployee == null)
            return;

        var window = new EmployeeEditorWindow(SelectedEmployee)
        {
            Owner = Application.Current.MainWindow
        };

        if (window.ShowDialog() != true)
            return;

        try
        {
            if (await _employeeService.EmailExistsAsync(
                    window.Employee.Email,
                    window.Employee.Id))
            {
                StatusMessage = "Сотрудник с таким email уже существует.";
                return;
            }

            bool updated = await _employeeService.UpdateAsync(
                window.Employee);

            if (updated)
            {
                await LoadEmployeesAsync();
                StatusMessage = "Данные сотрудника обновлены.";
            }
            else
            {
                StatusMessage = "Сотрудник не найден.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка обновления: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteEmployeeAsync()
    {
        if (SelectedEmployee?.Id == null)
            return;

        var confirmation = MessageBox.Show(
            $"Удалить сотрудника «{SelectedEmployee.FullName}»?",
            "Удаление сотрудника",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmation != MessageBoxResult.Yes)
            return;

        try
        {
            bool deleted = await _employeeService.DeleteAsync(
                SelectedEmployee.Id);

            if (deleted)
            {
                await LoadEmployeesAsync();
                StatusMessage = "Сотрудник удалён.";
            }
            else
            {
                StatusMessage = "Сотрудник не найден.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка удаления: {ex.Message}";
        }
    }
}
