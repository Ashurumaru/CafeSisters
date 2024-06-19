using CafeSisters.Data;
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
using CafeSisters.Utils;


namespace CafeSisters.Views.Pages.Reports
{
    /// <summary>
    /// Логика взаимодействия для AllOrdersReportPage.xaml
    /// </summary>
    public partial class AllOrdersReportPage : Page
    {
        private CafeSistersEntities _context;

        public AllOrdersReportPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.LastName
                })
                .ToList();

            EmployeeComboBox.ItemsSource = employees;
        }

        private void GenerateAllOrdersReport_Click(object sender, RoutedEventArgs e)
        {
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;
            int? employeeId = (int?)EmployeeComboBox.SelectedValue;

            ReportGenerator.GenerateAllOrdersReport(startDate, endDate, employeeId);
            MessageBox.Show("Отчет по всем заказам сгенерирован.");
        }
    }
}