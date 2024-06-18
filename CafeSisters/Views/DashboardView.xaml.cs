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

namespace CafeSisters.Views
{
    /// <summary>
    /// Логика взаимодействия для DashboardView.xaml
    /// </summary>
    public partial class DashboardView : Window
    {
        public DashboardView()
        {
            InitializeComponent();
            txblName.Text = CurrentUser.GetFullName();
            if (CurrentUser.EmployeeId != 1)
            {
                rdBtnEmployeeManagement.Visibility = Visibility.Collapsed;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton != null && radioButton.IsChecked == true)
            {
                switch (radioButton.Content.ToString())
                {
                    case "Создать заказ":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Заказы":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Меню":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Продукты":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Техническая карта":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Сотрудники":
                        PagesNavigation.Navigate(new OrderCreationPage());
                        break;
                    case "Кабинет":
                        PagesNavigation.Navigate(new OrderCreationPage());
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

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
