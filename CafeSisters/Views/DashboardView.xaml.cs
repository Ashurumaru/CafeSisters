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
using System.Windows.Shapes;
using CafeSisters.Views.Pages;
using System.Windows.Threading;

namespace CafeSisters.Views
{
    /// <summary>
    /// Логика взаимодействия для DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Window
    {
        private DispatcherTimer _timer;

        public DashboardView()
        {
            InitializeComponent();
            PagesNavigation.Navigate(new OrderCreationPage());
            txblName.Text = CurrentUser.GetFullName();
            if (CurrentUser.PositionId != 2)
            {
                rdBtnEmployeeManagement.Visibility = Visibility.Collapsed;
            }
            StartClock();
        }
        private void StartClock()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            DateTimeTextBlock.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm:ss");
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked == true)
            {
                switch (radioButton.Content.ToString())
                {
                    case "Создание заказа":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Заказы":
                        PagesNavigation.Navigate(new OrderManagementPage());
                        break;
                    case "Меню":
                        PagesNavigation.Navigate(new MenuManagementPage());
                        break;
                    case "Продукты":
                        PagesNavigation.Navigate(new ProductManagementPage());
                        break;
                    case "Техническая карта":
                        PagesNavigation.Navigate(new RecipeManagementPage());
                        break;
                    case "Сотрудники":
                        PagesNavigation.Navigate(new EmployeeManagementPage());
                        break;
                    case "Кабинет":
                        PagesNavigation.Navigate(new PersonalAccountPage());
                        break;
                    case "Отчеты":
                        PagesNavigation.Navigate(new ReportManagementPage());
                        break;
                }
            }
        }
        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(this).WindowState = WindowState.Minimized;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                SwitchWindowState();
                return;
            }
            if (Window.GetWindow(this).WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed) Window.GetWindow(this).DragMove();
            }
        }

        private void MaximizeWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Maximized;
        }

        private void RestoreWindow()
        {
            Window.GetWindow(this).WindowState = WindowState.Normal;
        }

        private void SwitchWindowState()
        {
            if (Window.GetWindow(this).WindowState == WindowState.Normal) MaximizeWindow();
            else RestoreWindow();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void RBtnClose_Checked(object sender, RoutedEventArgs e)
        {
            LoginView loginView = new LoginView();
            loginView.Show();
            this.Close();
        }
    }
}
