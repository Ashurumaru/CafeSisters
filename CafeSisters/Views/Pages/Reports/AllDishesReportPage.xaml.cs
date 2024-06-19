using CafeSisters.Utils;
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

namespace CafeSisters.Views.Pages.Reports
{
    /// <summary>
    /// Логика взаимодействия для AllDishesReportPage.xaml
    /// </summary>
    public partial class AllDishesReportPage : Page
    {
        public AllDishesReportPage()
        {
            InitializeComponent();
        }

        private void GenerateAllDishesReport_Click(object sender, RoutedEventArgs e)
        {
            ReportGenerator.GenerateAllDishesReport();
            MessageBox.Show("Отчет по всем блюдам сгенерирован.");
        }
    }
}
