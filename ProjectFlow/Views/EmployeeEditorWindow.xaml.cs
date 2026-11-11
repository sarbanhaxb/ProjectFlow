
using System.Net.Mail;
using System.Windows;
using ProjectFlow.Models;

namespace ProjectFlow.Views;

public partial class EmployeeEditorWindow : Window
{
    public Employee Employee { get; }

    public EmployeeEditorWindow(Employee? employee = null)
    {
        InitializeComponent();

        Employee = employee == null
            ? new Employee()
            : new Employee
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Position = employee.Position
            };

        Title = employee == null
            ? "Создание сотрудника"
            : "Редактирование сотрудника";

        FullNameTextBox.Text = Employee.FullName;
        EmailTextBox.Text = Employee.Email;
        PositionTextBox.Text = Employee.Position;
    }

    private void SaveButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        string fullName = FullNameTextBox.Text.Trim();
        string email = EmailTextBox.Text.Trim().ToLowerInvariant();
        string position = PositionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            MessageBox.Show("Введите ФИО сотрудника.");
            return;
        }

        if (string.IsNullOrWhiteSpace(position))
        {
            MessageBox.Show("Введите должность.");
            return;
        }

        try
        {
            var address = new MailAddress(email);

            if (!string.Equals(
                    address.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException();
            }
        }
        catch (FormatException)
        {
            MessageBox.Show(
                "Введите корректный адрес электронной почты.");
            return;
        }

        Employee.FullName = fullName;
        Employee.Email = email;
        Employee.Position = position;

        DialogResult = true;
    }
}
