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
using CafeSisters.Models;

namespace CafeSisters.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderCreationPage.xaml
    /// </summary>
    public partial class OrderCreationPage : Page
    {
        private CafeSistersEntities _context;
        private List<CartItem> _cartItems; 

        public OrderCreationPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            _cartItems = new List<CartItem>();
            StopListCheckBox.IsChecked = true;
            MenuTypeCheckBox.IsChecked = true;
            LoadMenus();
            LoadCategory();
        }

        private void LoadMenus()
        {
            var menus = _context.Menus.ToList();
            MenuComboBox.ItemsSource = menus;
            MenuComboBox.ItemsSource = _context.Menus.Select(c => new
            {
                c.MenuId,
                c.MenuName
            }).ToList();
            if (MenuComboBox.Items.Count > 0)
                MenuComboBox.SelectedIndex = 0;
        }

        private  void LoadCategory()
        {
            var categories = _context.RecipeCategories.ToList();
            CategoryComboBox.ItemsSource = categories;
        }

        private void MenuComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMenuWithStopList();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var recipe = button.DataContext as dynamic;
            var cartItem = _cartItems.FirstOrDefault(ci => ci.RecipeId == recipe.RecipeId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
                cartItem.TotalCost += recipe.Cost;
            }
            else
            {
                _cartItems.Add(new CartItem
                {
                    RecipeId = recipe.RecipeId,
                    RecipeName = recipe.RecipeName,
                    Quantity = 1,
                    TotalCost = recipe.Cost
                });
            }

            UpdateCart();
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var cartItem = button.DataContext as CartItem;

            if (cartItem != null)
            {
                _cartItems.Remove(cartItem);
                UpdateCart();
            }
        }

        private void UpdateCart()
        {
            CartItemsControl.ItemsSource = null;
            CartItemsControl.ItemsSource = _cartItems;

            var totalCost = _cartItems.Sum(ci => ci.TotalCost);
            TotalCostTextBlock.Text = totalCost.ToString("C");

            var totalItems = _cartItems.Sum(ci => ci.Quantity);
            TotalItemsTextBlock.Text = totalItems.ToString();
            TotalCostTextBlockMain.Text = totalCost.ToString("C");
        }

        private void PlaceOrder_Click(object sender, RoutedEventArgs e)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    var order = new Orders
                    {
                        EmployeeId = CurrentUser.EmployeeId, 
                        OrderDate = DateTime.Now,
                        StatusId = 1,
                        TotalCost = _cartItems.Sum(ci => ci.TotalCost)
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    foreach (var cartItem in _cartItems)
                    {
                        var orderDetail = new OrderDetails
                        {
                            OrderId = order.OrderId,
                            RecipeId = cartItem.RecipeId,
                            Quantity = cartItem.Quantity
                        };

                        _context.OrderDetails.Add(orderDetail);

                        var recipeIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == cartItem.RecipeId).ToList();
                        foreach (var ingredient in recipeIngredients)
                        {
                            var stockItem = _context.Ingredients.First(i => i.IngredientId == ingredient.IngredientId);
                            stockItem.Quantity -= ingredient.Quantity * cartItem.Quantity;
                        }
                    }

                    _context.SaveChanges();
                    transaction.Commit();
                    MessageBox.Show("Заказ успешно оформлен!");

                    _cartItems.Clear();
                    UpdateCart();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show("Произошла ошибка при оформлении заказа: " + ex.Message);
                }
            }
        }

        private void StopListCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateMenuWithStopList();
        }

        private void StopListCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateMenuWithStopList();
        }

        private void MenuTypeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            LoadMenus();
        }

        private void MenuTypeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            LoadMenus();
        }

        private void UpdateMenuWithStopList()
        {
            List<MenuRecipeItem> menuRecipes;

            if (MenuTypeCheckBox.IsChecked == true && MenuComboBox.SelectedValue != null)
            {
                var selectedMenuId = (int)MenuComboBox.SelectedValue;
                menuRecipes = _context.MenuRecipes
                    .Where(mr => mr.MenuId == selectedMenuId)
                    .Select(mr => new MenuRecipeItem
                    {
                        RecipeId = mr.Recipes.RecipeId,
                        RecipeName = mr.Recipes.RecipeName,
                        Cost = Math.Round(mr.Recipes.Cost, 2),
                        Instruction = mr.Recipes.Instruction,
                        Weight = Math.Round(mr.Recipes.RecipeIngredients.Sum(ri => ri.Quantity), 2),
                        Proteins = Math.Round((decimal)mr.Recipes.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Proteins * ri.Quantity)), 2),
                        Fats = Math.Round((decimal)mr.Recipes.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Fats * ri.Quantity)), 2),
                        Carbohydrates = Math.Round((decimal)mr.Recipes.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Carbohydrates * ri.Quantity)), 2),
                        EnergyValue = Math.Round((decimal)mr.Recipes.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.EnergyValue * ri.Quantity)), 2)
                    })
                    .ToList();
            }
            else
            {
                menuRecipes = _context.Recipes
                    .Select(r => new MenuRecipeItem
                    {
                        RecipeId = r.RecipeId,
                        RecipeName = r.RecipeName,
                        Cost = Math.Round(r.Cost, 2),
                        Instruction = r.Instruction,
                        Weight = Math.Round(r.RecipeIngredients.Sum(ri => ri.Quantity), 2),
                        Proteins = Math.Round((decimal)r.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Proteins * ri.Quantity)), 2),
                        Fats = Math.Round((decimal)r.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Fats * ri.Quantity)), 2),
                        Carbohydrates = Math.Round((decimal)r.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.Carbohydrates * ri.Quantity)), 2),
                        EnergyValue = Math.Round((decimal)r.RecipeIngredients.Sum(ri => ri.Ingredients.NutritionalValues.Sum(nv => nv.EnergyValue * ri.Quantity)), 2)
                    })
                    .ToList();
            }

            if (StopListCheckBox.IsChecked == true)
            {
                var availableRecipes = menuRecipes.Where(r => CheckIngredientsAvailability(r.RecipeId)).ToList();
                MenuItemsControl.ItemsSource = availableRecipes;
            }
            else
            {
                MenuItemsControl.ItemsSource = menuRecipes;
            }
        }



        private bool CheckIngredientsAvailability(int recipeId)
        {
            var recipeIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == recipeId).ToList();
            foreach (var ingredient in recipeIngredients)
            {
                var stockItem = _context.Ingredients.First(i => i.IngredientId == ingredient.IngredientId);
                if (stockItem.Quantity < ingredient.Quantity)
                {
                    return false;
                }
            }
            return true;
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMenuWithStopList();
        }

        private void CategoryComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
