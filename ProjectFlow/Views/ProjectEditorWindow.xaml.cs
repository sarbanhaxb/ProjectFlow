
using System.Windows;
using ProjectFlow.Models;
using ProjectFlow.Models.Enums;

namespace ProjectFlow.Views;

public partial class ProjectEditorWindow : Window
{
    public Project Project { get; }

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
            Project.StartDate.ToLocalTime().Date;

        EndDatePicker.SelectedDate =
            Project.EndDate?.ToLocalTime().Date;
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

        Project.StartDate =
            StartDatePicker.SelectedDate.Value
                .Date.ToUniversalTime();

        Project.EndDate =
            endDate?.Date.ToUniversalTime();

        DialogResult = true;
    }
}
