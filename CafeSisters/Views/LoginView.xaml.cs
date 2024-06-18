using CafeSisters.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
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

namespace CafeSisters.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private string _password;
        private CafeSistersEntities _context;
        public LoginView()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = txLogin.Text;
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(_password))
            {
                ErroeMessage.Text = "Введите логин и пароль.";
                return;
            }

            try
            {
                var user = _context.Employees.SingleOrDefault(u => u.Login == login && u.Password == _password);
                if (user != null)
                {
                    DashboardView dashbord = new DashboardView();
                    MessageBox.Show($"Добро пожаловать {user.FirstName} {user.LastName}", "Окно приветствия");
                    dashbord.Show();
                    this.Close();
                }
                else
                {
                    ErroeMessage.Text = "Неверный логин или пароль.";
                    ErroeMessage.Opacity = 1;
                }
            }
            catch (Exception ex)
            {
                ErroeMessage.Text = "Ошибка: " + ex.Message;
                ErroeMessage.Opacity = 1;
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            _password = passwordBox.Password;
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
    }
}
