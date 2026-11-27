
using System.Windows;
using ProjectFlow.Models;
using ProjectFlow.Models.Enums;
using ProjectFlow.Services;
using ProjectFlow.Helpers;

namespace ProjectFlow.Views;

public partial class ProjectEditorWindow : Window
{
    public Project Project { get; }

    private readonly EmployeeService _employeeService;

    public ProjectEditorWindow(Project? project = null)
    {
        InitializeComponent();

        Project = project == null
            ? new Project()
            : new Project
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                MemberIds = new List<string>(project.MemberIds)
            };

        Title = project == null
            ? "Создание проекта"
            : "Редактирование проекта";

        StatusComboBox.ItemsSource =
            Enum.GetValues<ProjectStatus>();

        NameTextBox.Text = Project.Name;
        DescriptionTextBox.Text = Project.Description;
        StatusComboBox.SelectedItem = Project.Status;

        StartDatePicker.SelectedDate =
            Project.StartDate.Date;

        EndDatePicker.SelectedDate =
            Project.EndDate?.Date;

        _employeeService = new EmployeeService(MongoDbService.Instance);

        Loaded += ProjectEditorWindow_Loaded;
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string name = NameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(
                "Введите название проекта.",
                "Проверка данных",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            NameTextBox.Focus();
            return;
        }

        if (!StartDatePicker.SelectedDate.HasValue)
        {
            MessageBox.Show("Укажите дату начала проекта.");
            return;
        }

        DateTime? endDate = EndDatePicker.SelectedDate;

        if (endDate.HasValue &&
            endDate.Value.Date <
            StartDatePicker.SelectedDate.Value.Date)
        {
            MessageBox.Show(
                "Дата окончания не может быть раньше даты начала.");
            return;
        }

        Project.Name = name;
        Project.Description = DescriptionTextBox.Text.Trim();

        Project.Status = (ProjectStatus)
            StatusComboBox.SelectedItem;

        Project.StartDate = DateHelper.ToUtcDate(
            StartDatePicker.SelectedDate!.Value);

        Project.EndDate = DateHelper.ToUtcDate(
            EndDatePicker.SelectedDate);


        Project.MemberIds = MembersListBox.SelectedItems
            .Cast<Employee>()
            .Where(employee => employee.Id != null)
            .Select(employee => employee.Id!)
            .ToList();


        DialogResult = true;
    }


    private async void ProjectEditorWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        // Не запускаем загрузку повторно
        Loaded -= ProjectEditorWindow_Loaded;

        SaveButton.IsEnabled = false;

        try
        {
            var employees = await _employeeService.GetAllAsync();

            MembersListBox.ItemsSource = employees;

            foreach (var employee in employees)
            {
                if (employee.Id != null &&
                    Project.MemberIds.Contains(employee.Id))
                {
                    MembersListBox.SelectedItems.Add(employee);
                }
            }

            SaveButton.IsEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить сотрудников:\n{ex.Message}",
                "Ошибка MongoDB",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            // Сохранение остаётся заблокированным
        }
    }


}
