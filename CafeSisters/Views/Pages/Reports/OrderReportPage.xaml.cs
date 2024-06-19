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
    /// Логика взаимодействия для OrderReportPage.xaml
    /// </summary>
    public partial class OrderReportPage : Page
    {
        private CafeSistersEntities _context;

        public OrderReportPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadOrders();
        }

        private void LoadOrders()
        {
            OrderList.ItemsSource = _context.Orders.ToList();
        }

        private void GenerateOrderReport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;

            ReportGenerator.GenerateOrderReport(orderId);
            MessageBox.Show("Отчет по заказу сгенерирован.");
        }
    }
}