using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CafeSisters.Data;

namespace CafeSisters.Views.Pages
{
    public partial class EmployeeManagementPage : Page
    {
        private CafeSistersEntities _context;
        private bool isEditing = false;
        private int editingEmployeeId;

        public EmployeeManagementPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadPositions();
            LoadEmployees();
        }

        private void LoadPositions()
        {
            var positions = _context.Positions.ToList();
            PositionComboBox.ItemsSource = positions;
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.Patronymic + " " + e.LastName,
                    e.Positions.PositionName,
                    e.Phone,
                    e.Login,
                    TotalRevenue = _context.Orders
                        .Where(o => o.EmployeeId == e.EmployeeId)
                        .Sum(o => o.TotalCost),
                    OrderCount = _context.Orders
                        .Count(o => o.EmployeeId == e.EmployeeId)
                }).ToList();

            EmployeeList.ItemsSource = employees;
        }

        private void CreateEmployee_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            EmployeeForm.Visibility = Visibility.Visible;
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingEmployeeId = (int)button.CommandParameter;
            var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == editingEmployeeId);

            if (employee != null)
            {
                isEditing = true;
                FirstNameTextBox.Text = employee.FirstName;
                PatronymicTextBox.Text = employee.Patronymic;
                LastNameTextBox.Text = employee.LastName;
                PositionComboBox.SelectedValue = employee.PositionId;
                PhoneTextBox.Text = employee.Phone;
                LoginTextBox.Text = employee.Login;
                PasswordTextBox.Text = employee.Password;
                EmployeeForm.Visibility = Visibility.Visible;
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.CommandParameter;
            var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);

            if (employee != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var orders = _context.Orders.Where(o => o.EmployeeId == employeeId).ToList();
                    foreach (var order in orders)
                    {
                        var orderDetails = _context.OrderDetails.Where(od => od.OrderId == order.OrderId).ToList();
                        _context.OrderDetails.RemoveRange(orderDetails);
                    }
                    _context.Orders.RemoveRange(orders);
                    _context.Employees.Remove(employee);
                    _context.SaveChanges();
                    LoadEmployees();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == editingEmployeeId);
                if (employee != null)
                {
                    employee.FirstName = FirstNameTextBox.Text;
                    employee.Patronymic = PatronymicTextBox.Text;
                    employee.LastName = LastNameTextBox.Text;
                    employee.PositionId = (int)PositionComboBox.SelectedValue;
                    employee.Phone = PhoneTextBox.Text;
                    employee.Login = LoginTextBox.Text;
                    employee.Password = PasswordTextBox.Text;
                }
            }
            else
            {
                var newEmployee = new Employees
                {
                    FirstName = FirstNameTextBox.Text,
                    Patronymic = PatronymicTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    PositionId = (int)PositionComboBox.SelectedValue,
                    Phone = PhoneTextBox.Text,
                    Login = LoginTextBox.Text,
                    Password = PasswordTextBox.Text
                };
                _context.Employees.Add(newEmployee);
            }

            _context.SaveChanges();
            EmployeeForm.Visibility = Visibility.Collapsed;
            LoadEmployees();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            EmployeeForm.Visibility = Visibility.Collapsed;
        }

        private void SearchEmployeeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchEmployeeTextBox.Text = string.Empty;
            LoadEmployees();
        }

        private void FilterEmployees()
        {
            var searchText = SearchEmployeeTextBox.Text.ToLower();
            var filteredEmployees = _context.Employees
                .Where(e => e.FirstName.ToLower().Contains(searchText) ||
                            e.Patronymic.ToLower().Contains(searchText) ||
                            e.LastName.ToLower().Contains(searchText) ||
                            e.Login.ToLower().Contains(searchText) ||
                            e.Phone.Contains(searchText))
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.Patronymic + " " + e.LastName,
                    e.Positions.PositionName,
                    e.Phone,
                    e.Login,
                    TotalRevenue = _context.Orders
                        .Where(o => o.EmployeeId == e.EmployeeId)
                        .Sum(o => o.TotalCost),
                    OrderCount = _context.Orders
                        .Count(o => o.EmployeeId == e.EmployeeId)
                }).ToList();

            EmployeeList.ItemsSource = filteredEmployees;
        }

        private void ClearForm()
        {
            FirstNameTextBox.Text = string.Empty;
            PatronymicTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            PositionComboBox.SelectedIndex = -1;
            PhoneTextBox.Text = string.Empty;
            LoginTextBox.Text = string.Empty;
            PasswordTextBox.Text = string.Empty;
        }
    }
}
