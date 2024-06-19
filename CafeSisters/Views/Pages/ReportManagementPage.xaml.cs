using CafeSisters.Views.Pages.Reports;
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
    /// Логика взаимодействия для ReportManagementPage.xaml
    /// </summary>
    public partial class ReportManagementPage : Page
    {
        public ReportManagementPage()
        {
            InitializeComponent();
        }
        private void NavigateToOrderReportPage(object sender, RoutedEventArgs e)
        {
            ReportContentFrame.Navigate(new OrderReportPage());
        }

        private void NavigateToAllOrdersReportPage(object sender, RoutedEventArgs e)
        {
            ReportContentFrame.Navigate(new AllOrdersReportPage());
        }

        private void NavigateToMenuReportPage(object sender, RoutedEventArgs e)
        {
            ReportContentFrame.Navigate(new MenuReportPage());
        }

        private void NavigateToAllDishesReportPage(object sender, RoutedEventArgs e)
        {
            ReportContentFrame.Navigate(new AllDishesReportPage());
        }
    }
}
