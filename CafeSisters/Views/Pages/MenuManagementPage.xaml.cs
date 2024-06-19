using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CafeSisters.Data;
using CafeSisters.Models;

namespace CafeSisters.Views.Pages
{
    public partial class MenuManagementPage : Page
    {
        private CafeSistersEntities _context;
        private bool isEditing = false;
        private int editingMenuId;
        private List<Recipes> allRecipes;
        private List<Recipes> menuRecipes;

        public MenuManagementPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadMenus();
            LoadRecipes();
            LoadCategory();
        }

        private void LoadMenus()
        {
            var menus = _context.Menus.ToList();
            MenuList.ItemsSource = menus;
        }

        private void LoadRecipes()
        {
            allRecipes = _context.Recipes.ToList();
            RecipeList.ItemsSource = allRecipes;
        }

        private void LoadCategory()
        {
            var categories = _context.RecipeCategories.ToList();
            CategoryComboBox.ItemsSource = categories;
        }

        private void CreateMenu_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            menuRecipes = new List<Recipes>();
            MenuItemsControl.ItemsSource = menuRecipes;
            MenuForm.Visibility = Visibility.Visible;
            TopPanel.Visibility = Visibility.Collapsed;
            MenuList.Visibility = Visibility.Collapsed;
        }

        private void EditMenuAndItems_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingMenuId = (int)button.CommandParameter;
            var menu = _context.Menus.FirstOrDefault(m => m.MenuId == editingMenuId);

            if (menu != null)
            {
                isEditing = true;
                MenuNameTextBox.Text = menu.MenuName;
                LoadMenuItems(editingMenuId);
                MenuForm.Visibility = Visibility.Visible;
                TopPanel.Visibility = Visibility.Collapsed;
                MenuList.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadMenuItems(int menuId)
        {
            menuRecipes = _context.MenuRecipes
                .Where(mr => mr.MenuId == menuId)
                .Select(mr => mr.Recipes)
                .ToList();

            MenuItemsControl.ItemsSource = menuRecipes;
        }

        private void DeleteMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var menuId = (int)button.CommandParameter;
            var menu = _context.Menus.FirstOrDefault(m => m.MenuId == menuId);

            if (menu != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это меню?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var menuRecipes = _context.MenuRecipes.Where(mr => mr.MenuId == menuId).ToList();
                    _context.MenuRecipes.RemoveRange(menuRecipes);

                    _context.Menus.Remove(menu);
                    _context.SaveChanges();
                    LoadMenus();
                }
            }
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(MenuNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название меню.");
                return;
            }

            Menus menu;
            if (isEditing)
            {
                menu = _context.Menus.FirstOrDefault(m => m.MenuId == editingMenuId);
                if (menu != null)
                {
                    menu.MenuName = MenuNameTextBox.Text;
                }
            }
            else
            {
                menu = new Menus
                {
                    MenuName = MenuNameTextBox.Text
                };
                _context.Menus.Add(menu);
                _context.SaveChanges();
                editingMenuId = menu.MenuId;
            }

            var existingMenuRecipes = _context.MenuRecipes.Where(mr => mr.MenuId == editingMenuId).ToList();
            _context.MenuRecipes.RemoveRange(existingMenuRecipes);

            foreach (var recipe in menuRecipes)
            {
                var menuRecipe = new MenuRecipes
                {
                    MenuId = editingMenuId,
                    RecipeId = recipe.RecipeId
                };
                _context.MenuRecipes.Add(menuRecipe);
            }

            _context.SaveChanges();
            MenuForm.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            MenuList.Visibility = Visibility.Visible;
            LoadMenus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MenuForm.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            MenuList.Visibility = Visibility.Visible;
        }

        private void AddRecipeToMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var recipeId = (int)button.CommandParameter;

            var recipe = allRecipes.FirstOrDefault(r => r.RecipeId == recipeId);
            if (recipe != null && !menuRecipes.Contains(recipe))
            {
                menuRecipes.Add(recipe);
                MenuItemsControl.ItemsSource = null;
                MenuItemsControl.ItemsSource = menuRecipes;
            }
            else
            {
                MessageBox.Show("Данное блюдо уже добавлено в меню.");
            }
        }

        private void DeleteRecipeFromMenu_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var recipeId = (int)button.CommandParameter;

            var recipe = menuRecipes.FirstOrDefault(r => r.RecipeId == recipeId);
            if (recipe != null)
            {
                menuRecipes.Remove(recipe);
                MenuItemsControl.ItemsSource = null;
                MenuItemsControl.ItemsSource = menuRecipes;
            }
        }

        private void SearchMenuTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterMenus();
        }

        private void ResetMenuSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchMenuTextBox.Text = string.Empty;
            FilterMenus();
        }

        private void SearchRecipeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecipes();
        }

        private void FilterMenus()
        {
            var searchText = SearchMenuTextBox.Text.ToLower();
            var filteredMenus = _context.Menus
                .Where(m => string.IsNullOrEmpty(searchText) || m.MenuName.ToLower().Contains(searchText))
                .ToList();
            MenuList.ItemsSource = filteredMenus;
        }

        private void FilterRecipes()
        {
            var searchText = SearchRecipeTextBox.Text.ToLower();
            var selectedCategoryId = CategoryComboBox.SelectedValue as int?;
            var filteredRecipes = allRecipes
                .Where(r => (string.IsNullOrEmpty(searchText) || r.RecipeName.ToLower().Contains(searchText)) &&
                            (!selectedCategoryId.HasValue || r.RecipeCategoryId == selectedCategoryId.Value))
                .ToList();

            if (HideExistingRecipesCheckBox.IsChecked == true)
            {
                filteredRecipes = filteredRecipes.Where(r => !menuRecipes.Any(mr => mr.RecipeId == r.RecipeId)).ToList();
            }

            RecipeList.ItemsSource = filteredRecipes;
        }

        private void ClearForm()
        {
            MenuNameTextBox.Text = string.Empty;
            MenuItemsControl.ItemsSource = null;
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterRecipes();
        }

        private void HideExistingRecipesCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            FilterRecipes();
        }

        private void HideExistingRecipesCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            FilterRecipes();
        }
        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchRecipeTextBox.Text = string.Empty;
            CategoryComboBox.SelectedIndex = -1;
            HideExistingRecipesCheckBox.IsChecked = false;
            FilterRecipes();
        }
    }
}
