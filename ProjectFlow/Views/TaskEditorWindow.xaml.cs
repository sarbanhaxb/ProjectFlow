
using System.Windows;
using System.Windows.Controls;
using ProjectFlow.Models;
using ProjectFlow.Models.Enums;
using ProjectFlow.Services;
using ProjectFlow.Helpers;

namespace ProjectFlow.Views;

public partial class TaskEditorWindow : Window
{
    private readonly ProjectService _projectService;
    private readonly EmployeeService _employeeService;

    private List<Employee> _employees = new();
    private bool _isInitializing;

    public TaskItem Task { get; }

    public TaskEditorWindow(TaskItem? task = null)
    {
        InitializeComponent();

        var mongoDbService = MongoDbService.Instance;

        _projectService = new ProjectService(mongoDbService);
        _employeeService = new EmployeeService(mongoDbService);

        Task = task == null
            ? new TaskItem()
            : new TaskItem
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                AssigneeId = task.AssigneeId,
                DueDate = task.DueDate
            };

        Title = task == null
            ? "Создание задачи"
            : "Редактирование задачи";

        TitleTextBox.Text = Task.Title;
        DescriptionTextBox.Text = Task.Description;

        StatusComboBox.ItemsSource =
            Enum.GetValues<ProjectFlow.Models.Enums.TaskStatus>();

        PriorityComboBox.ItemsSource =
            Enum.GetValues<TaskPriority>();

        StatusComboBox.SelectedItem = Task.Status;
        PriorityComboBox.SelectedItem = Task.Priority;

        DueDatePicker.SelectedDate =
            Task.DueDate?.Date;

        Loaded += TaskEditorWindow_Loaded;
    }

    private async void TaskEditorWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        _isInitializing = true;
        SaveButton.IsEnabled = false;

        try
        {
            var projects = await _projectService.GetAllAsync();
            _employees = await _employeeService.GetAllAsync();

            ProjectComboBox.ItemsSource = projects;

            ProjectComboBox.SelectedItem = projects
                .FirstOrDefault(p => p.Id == Task.ProjectId);

            // Для новой задачи проект можно выбрать вручную.
            // При редактировании восстанавливаем старого исполнителя.
            RefreshAssignees(Task.AssigneeId);

            SaveButton.IsEnabled = projects.Count > 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось загрузить данные: {ex.Message}",
                "Ошибка MongoDB",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isInitializing = false;
        }
    }

    private void ProjectComboBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (_isInitializing)
            return;

        // При изменении проекта прежний исполнитель
        // сохраняется только если он участвует в новом проекте.
        var previousId =
            (AssigneeComboBox.SelectedItem as Employee)?.Id;

        RefreshAssignees(previousId);
    }

    private void RefreshAssignees(string? selectedId)
    {
        if (ProjectComboBox.SelectedItem is not Project project)
        {
            AssigneeComboBox.ItemsSource = null;
            return;
        }

        var available = _employees
            .Where(employee =>
                employee.Id != null &&
                project.MemberIds.Contains(employee.Id))
            .ToList();

        AssigneeComboBox.ItemsSource = available;

        AssigneeComboBox.SelectedItem = available
            .FirstOrDefault(e => e.Id == selectedId);
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string title = TitleTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Введите название задачи.");
            TitleTextBox.Focus();
            return;
        }

        if (ProjectComboBox.SelectedItem is not Project project ||
            string.IsNullOrWhiteSpace(project.Id))
        {
            MessageBox.Show("Выберите проект.");
            return;
        }

        if (StatusComboBox.SelectedItem is not
            ProjectFlow.Models.Enums.TaskStatus status)
        {
            MessageBox.Show("Выберите статус задачи.");
            return;
        }

        if (PriorityComboBox.SelectedItem is not TaskPriority priority)
        {
            MessageBox.Show("Выберите приоритет.");
            return;
        }

        Task.Title = title;
        Task.Description = DescriptionTextBox.Text.Trim();
        Task.ProjectId = project.Id;
        Task.Status = status;
        Task.Priority = priority;

        Task.AssigneeId =
            (AssigneeComboBox.SelectedItem as Employee)?.Id;

        Task.DueDate = DateHelper.ToUtcDate(
            DueDatePicker.SelectedDate);

        DialogResult = true;
    }
}
