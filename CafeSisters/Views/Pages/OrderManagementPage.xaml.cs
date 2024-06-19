using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CafeSisters.Data;
using CafeSisters.Models;
using CafeSisters.Views.Pages;

namespace CafeSisters.Views.Pages
{
    public partial class OrderManagementPage : Page
    {
        private CafeSistersEntities _context;
        private bool isEditing = false;
        private int editingOrderId;
        private int currentOrderId;

        public OrderManagementPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadStatusList();
            LoadEmployees();
            FilterOrders();      
                        LoadRecipes();

        }


        private void LoadStatusList()
        {
            var statuses = _context.OrderStatuses.ToList();
            StatusComboBox.ItemsSource = statuses;
            StatusFilterComboBox.ItemsSource = statuses;
        }

        private void LoadRecipes()
        {
            var recipes = _context.Recipes.ToList();
            NewItemRecipeComboBox.ItemsSource = recipes;
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees.Where(e => e.PositionId == 1).Select(e => new
            {
                e.EmployeeId,
                FullName = e.FirstName + " " + e.LastName
            }).ToList();

            EmployeeComboBox.ItemsSource = employees;
            EmployeeFilterComboBox.ItemsSource = employees;
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.Windows.OfType<DashboardView>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.rdBtnCreateOrder.IsChecked = true;
            }
        }


        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingOrderId = (int)button.CommandParameter;
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == editingOrderId);

            if (order != null)
            {
                isEditing = true;
                OrderDatePicker.SelectedDate = order.OrderDate;
                TotalCostTextBox.Text = Math.Round(Math.Round((double)order.TotalCost, 2), 2).ToString("F2");
                StatusComboBox.SelectedValue = order.StatusId;
                EmployeeComboBox.SelectedValue = order.EmployeeId;
                OrderForm.Visibility = Visibility.Visible;
                TopPanel.Visibility = Visibility.Collapsed;
                OrderList.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var orderDetails = _context.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                    _context.OrderDetails.RemoveRange(orderDetails);

                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                    FilterOrders();
                }
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == editingOrderId);
                if (order != null)
                {
                    order.OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Now;
                    order.TotalCost = decimal.Parse(TotalCostTextBox.Text);
                    order.StatusId = (int)StatusComboBox.SelectedValue;
                    order.EmployeeId = (int)EmployeeComboBox.SelectedValue;
                }
            }
            else
            {
                var newOrder = new Orders
                {
                    OrderDate = OrderDatePicker.SelectedDate ?? DateTime.Now,
                    TotalCost = decimal.Parse(TotalCostTextBox.Text),
                    StatusId = (int)StatusComboBox.SelectedValue,
                    EmployeeId = (int)EmployeeComboBox.SelectedValue
                };
                _context.Orders.Add(newOrder);
            }

            _context.SaveChanges();
            OrderForm.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            OrderList.Visibility = Visibility.Visible;
            FilterOrders();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OrderForm.Visibility = Visibility.Collapsed;
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            OrderList.Visibility = Visibility.Visible;

        }

        private void SearchOrderTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterOrders();
        }

        private void StatusFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        private void EmployeeFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterOrders();
        }

        private void ResetOrderSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchOrderTextBox.Text = string.Empty;
            StatusFilterComboBox.SelectedIndex = -1;
            EmployeeFilterComboBox.SelectedIndex = -1;
            FilterOrders();
        }

        private void FilterOrders()
        {
            var searchText = SearchOrderTextBox.Text.ToLower();
            var selectedStatusId = (int?)StatusFilterComboBox.SelectedValue;
            var selectedEmployeeId = (int?)EmployeeFilterComboBox.SelectedValue;

            var filteredOrders = _context.Orders
                .Where(o =>
                    (string.IsNullOrEmpty(searchText) || o.OrderId.ToString().Contains(searchText) || o.OrderStatuses.StatusName.ToLower().Contains(searchText) || (o.Employees.FirstName + " " + o.Employees.LastName).ToLower().Contains(searchText)) &&
                    (!selectedStatusId.HasValue || o.StatusId == selectedStatusId.Value) &&
                    (!selectedEmployeeId.HasValue || o.EmployeeId == selectedEmployeeId.Value))
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    o.TotalCost,
                    StatusName = o.OrderStatuses.StatusName,
                    EmployeeFullName = o.Employees.FirstName + " " + o.Employees.LastName,
                    IsInProgress = o.StatusId == 1 
                })
                .ToList()
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    TotalCost = Math.Round((double)o.TotalCost, 2).ToString("F2"),
                    o.StatusName,
                    o.EmployeeFullName,
                    o.IsInProgress
                })
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            OrderList.ItemsSource = filteredOrders;
        }

        private void ClearForm()
        {
            OrderDatePicker.SelectedDate = DateTime.Now;
            TotalCostTextBox.Text = string.Empty;
            StatusComboBox.SelectedIndex = -1;
            EmployeeComboBox.SelectedIndex = -1;
        }

        private void TotalCostTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void CloseOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                order.StatusId = 2;
                _context.SaveChanges();
                FilterOrders();
            }
        }

        private void ShowOrderItems_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            currentOrderId = (int)button.CommandParameter;
            OrderItemsPanel.Visibility = Visibility.Visible;
            TopPanel.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Collapsed;
            ShowOrderItems(currentOrderId);
        }

        private void ShowOrderItems(int orderId)
        {
            var orderItems = _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .ToList()
                .Select(od => new OrderItemItem
                {
                    RecipeId = od.RecipeId,
                    RecipeName = od.Recipes.RecipeName,
                    Quantity = od.Quantity,
                    UnitCost = Math.Round(od.Recipes.Cost, 2),
                    TotalCost = Math.Round(od.Quantity * od.Recipes.Cost, 2)
                })
                .ToList();
            numberOrder.Text = orderId.ToString();
            OrderItemsControl.ItemsSource = orderItems;
            OrderItemsPanel.Visibility = Visibility.Visible;
            UpdateTotalCost();
        }

        private void SaveOrderItems_Click(object sender, RoutedEventArgs e)
        {
            var updatedItems = OrderItemsControl.ItemsSource as List<OrderItemItem>;

            foreach (var item in updatedItems)
            {
                var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderId == currentOrderId && od.RecipeId == item.RecipeId);
                if (orderDetail != null)
                {
                    orderDetail.Quantity = item.Quantity;
                }
            }

            var order = _context.Orders.FirstOrDefault(o => o.OrderId == currentOrderId);
            if (order != null)
            {
                order.TotalCost = updatedItems.Sum(i => i.TotalCost);
            }

            _context.SaveChanges();
            FilterOrders();
            MessageBox.Show("Позиции заказа успешно обновлены!");
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            OrderList.Visibility = Visibility.Visible;
        }

        private void AddOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedRecipe = NewItemRecipeComboBox.SelectedItem as Recipes;
            if (selectedRecipe == null)
            {
                MessageBox.Show("Пожалуйста, выберите блюдо для добавления.");
                return;
            }

            if (!int.TryParse(NewItemQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Пожалуйста, введите правильное количество.");
                return;
            }

            var existingOrderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderId == currentOrderId && od.RecipeId == selectedRecipe.RecipeId);

            if (existingOrderDetail != null)
            {
                existingOrderDetail.Quantity += quantity;
            }
            else
            {
                var orderDetail = new OrderDetails
                {
                    OrderId = currentOrderId,
                    RecipeId = selectedRecipe.RecipeId,
                    Quantity = quantity
                };

                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            ShowOrderItems(currentOrderId);
            MessageBox.Show("Позиция успешно добавлена!");
        }


        private void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderItem = button.CommandParameter as OrderItemItem;

            if (orderItem == null)
            {
                MessageBox.Show("Не удалось удалить позицию заказа.");
                return;
            }

            var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderId == currentOrderId && od.RecipeId == orderItem.RecipeId);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                _context.SaveChanges();

                ShowOrderItems(currentOrderId);
                MessageBox.Show("Позиция успешно удалена!");
            }
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.DataContext is OrderItemItem item)
            {
                if (int.TryParse(textBox.Text, out int quantity))
                {
                    item.Quantity = quantity;
                    item.TotalCost = Math.Round(quantity * item.UnitCost, 2);
                    UpdateTotalCost();
                }
            }
        }

        private void UpdateTotalCost()
        {
            var items = OrderItemsControl.ItemsSource as List<OrderItemItem>;
            if (items != null)
            {
                var totalCost = items.Sum(i => i.TotalCost);
                TotalCostTextBlock.Text = $"Итоговая стоимость: {totalCost:F2} ₽";
            }
        }

    }
}