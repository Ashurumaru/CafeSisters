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
    /// Логика взаимодействия для MenuReportPage.xaml
    /// </summary>
    public partial class MenuReportPage : Page
    {
        private CafeSistersEntities _context;

        public MenuReportPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadMenus();
        }

        private void LoadMenus()
        {
            MenuList.ItemsSource = _context.Menus.ToList();
        }

        private void GenerateMenuReport_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var menuId = (int)button.CommandParameter;

            ReportGenerator.GenerateMenuReport(menuId);
            MessageBox.Show("Отчет по меню сгенерирован.");
        }
    }
}