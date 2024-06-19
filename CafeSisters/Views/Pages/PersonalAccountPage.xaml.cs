using CafeSisters.Data;
using CafeSisters.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CafeSisters.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для PersonalAccountPage.xaml
    /// </summary>
    public partial class PersonalAccountPage : Page
    {
        private CafeSistersEntities _context;

        public PersonalAccountPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadUserData();
            LoadStatusList();
            LoadOrderStatistics();
        }

        private void LoadUserData()
        {
            var currentUser = _context.Employees.FirstOrDefault(e => e.EmployeeId == CurrentUser.EmployeeId);
            if (currentUser != null)
            {
                FirstNameTextBox.Text = currentUser.FirstName;
                PatronymicTextBox.Text = currentUser.Patronymic;
                LastNameTextBox.Text = currentUser.LastName;
                PhoneTextBox.Text = currentUser.Phone;
                LoginTextBox.Text = currentUser.Login;
                PasswordTextBox.Text = currentUser.Password;
            }
        }

        private void LoadStatusList()
        {
            var statuses = _context.OrderStatuses.ToList();
            StatusFilterComboBox.ItemsSource = statuses;
        }

        private void LoadOrderStatistics()
        {
            var orders = _context.Orders.Where(o => o.EmployeeId == CurrentUser.EmployeeId).Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.TotalCost,
                StatusName = o.OrderStatuses.StatusName
            }).ToList();

            OrderStatisticsList.ItemsSource = orders;
        }

        private void ApplyDateFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterOrderStatistics();
        }

        private void ApplyStatusFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterOrderStatistics();
        }

        private void FilterOrderStatistics()
        {
            var orders = _context.Orders.Where(o => o.EmployeeId == CurrentUser.EmployeeId);

            if (StartDatePicker.SelectedDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= StartDatePicker.SelectedDate.Value);
            }

            if (EndDatePicker.SelectedDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= EndDatePicker.SelectedDate.Value);
            }

            if (StatusFilterComboBox.SelectedValue != null)
            {
                var selectedStatusId = (int)StatusFilterComboBox.SelectedValue;
                orders = orders.Where(o => o.StatusId == selectedStatusId);
            }

            var filteredOrders = orders.Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.TotalCost,
                StatusName = o.OrderStatuses.StatusName
            }).ToList();

            OrderStatisticsList.ItemsSource = filteredOrders;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var currentUser = _context.Employees.FirstOrDefault(u => u.EmployeeId == CurrentUser.EmployeeId);
            if (currentUser != null)
            {
                currentUser.FirstName = FirstNameTextBox.Text;
                currentUser.Patronymic = PatronymicTextBox.Text;
                currentUser.LastName = LastNameTextBox.Text;
                currentUser.Phone = PhoneTextBox.Text;
                currentUser.Login = LoginTextBox.Text;
                currentUser.Password = PasswordTextBox.Text;

                _context.SaveChanges();
                MessageBox.Show("Данные успешно сохранены.");
            }
        }
    }
}