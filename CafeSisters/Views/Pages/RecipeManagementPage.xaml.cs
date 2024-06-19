using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CafeSisters.Data;
using CafeSisters.Models;
using CafeSisters.Views.Cards;

namespace CafeSisters.Views.Pages
{
    public partial class RecipeManagementPage : Page
    {
        private CafeSistersEntities _context;
        private bool isEditing = false;
        private int editingRecipeId;
        private List<Ingredients> allIngredients;
        private List<IngredientCategories> categories;
        private List<ProcessingTypes> processingTypes;
        private List<RecipeIngredientItem> recipeIngredients;

        public RecipeManagementPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadIngredients();
            LoadProcessingTypes();
            LoadCategories();
            FilterRecipes();
            LoadRecipeCategory();
        }

        private void LoadRecipeCategory()
        {
            var categoriesRecipe = _context.RecipeCategories.ToList();
            CategoryRecipeComboBox.ItemsSource = categoriesRecipe;
        }
        private void LoadIngredients()
        {
            allIngredients = _context.Ingredients.ToList();
            IngredientList.ItemsSource = allIngredients;
        }

        private void LoadProcessingTypes()
        {
            processingTypes = _context.ProcessingTypes.ToList();
        }

        private void LoadCategories()
        {
            categories = _context.IngredientCategories.ToList();
            CategoryFilterComboBox.ItemsSource = categories;
        }

        private void CreateRecipe_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            RecipeForm.Visibility = Visibility.Visible;
            TopPanel.Visibility = Visibility.Collapsed;
            RecipeList.Visibility = Visibility.Collapsed;
        }

        private void EditRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingRecipeId = (int)button.CommandParameter;
            var recipe = _context.Recipes.FirstOrDefault(r => r.RecipeId == editingRecipeId);

            if (recipe != null)
            {
                isEditing = true;
                RecipeNameTextBox.Text = recipe.RecipeName;
                RecipeCostTextBox.Text = recipe.Cost.ToString("F2");
                RecipePrepTimeTextBox.Text = recipe.PreparationTime.ToString();
                RecipeInstructionTextBox.Text = recipe.Instruction;

                recipeIngredients = _context.RecipeIngredients
                    .Where(ri => ri.RecipeId == editingRecipeId)
                    .Select(ri => new RecipeIngredientItem
                    {
                        RecipeId = ri.RecipeId,
                        IngredientId = ri.IngredientId,
                        IngredientName = ri.Ingredients.IngredientName,
                        Quantity = ri.Quantity,
                        Unit = ri.Ingredients.Unit,
                        ProcessingTypeId = ri.ProcessingTypeId,
                        ProcessingTypeName = ri.ProcessingTypes.ProcessingTypeName
                    })
                    .ToList();

                RecipeIngredientsControl.ItemsSource = recipeIngredients;
                RecipeForm.Visibility = Visibility.Visible;
                TopPanel.Visibility = Visibility.Collapsed;
                RecipeList.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var recipeId = (int)button.CommandParameter;
            var recipe = _context.Recipes.FirstOrDefault(r => r.RecipeId == recipeId);

            if (recipe != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот рецепт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var recipeIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == recipeId).ToList();
                    _context.RecipeIngredients.RemoveRange(recipeIngredients);

                    _context.Recipes.Remove(recipe);
                    _context.SaveChanges();
                    FilterRecipes();
                }
            }
        }

        private void SaveRecipe_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var recipe = _context.Recipes.FirstOrDefault(r => r.RecipeId == editingRecipeId);
                if (recipe != null)
                {
                    recipe.RecipeName = RecipeNameTextBox.Text;
                    recipe.Cost = decimal.Parse(RecipeCostTextBox.Text);
                    recipe.PreparationTime = Convert.ToInt32(RecipePrepTimeTextBox.Text);
                    recipe.Instruction = RecipeInstructionTextBox.Text;

                    var existingIngredients = _context.RecipeIngredients.Where(ri => ri.RecipeId == editingRecipeId).ToList();
                    _context.RecipeIngredients.RemoveRange(existingIngredients);
                    foreach (var item in recipeIngredients)
                    {
                        _context.RecipeIngredients.Add(new RecipeIngredients
                        {
                            RecipeId = item.RecipeId,
                            IngredientId = item.IngredientId,
                            Quantity = item.Quantity,
                            ProcessingTypeId = item.ProcessingTypeId
                        });
                    }
                }
            }
            else
            {
                var newRecipe = new Recipes
                {
                    RecipeName = RecipeNameTextBox.Text,
                    Cost = decimal.Parse(RecipeCostTextBox.Text),
                    PreparationTime = Convert.ToInt32(RecipePrepTimeTextBox.Text),
                    Instruction = RecipeInstructionTextBox.Text
                };
                _context.Recipes.Add(newRecipe);
                _context.SaveChanges();

                foreach (var item in recipeIngredients)
                {
                    _context.RecipeIngredients.Add(new RecipeIngredients
                    {
                        RecipeId = newRecipe.RecipeId,
                        IngredientId = item.IngredientId,
                        Quantity = item.Quantity,
                        ProcessingTypeId = item.ProcessingTypeId
                    });
                }
            }

            _context.SaveChanges();
            RecipeForm.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            RecipeList.Visibility = Visibility.Visible;
            FilterRecipes();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeForm.Visibility = Visibility.Collapsed;
            TopPanel.Visibility = Visibility.Visible;
            RecipeList.Visibility = Visibility.Visible;
        }

        private void SearchRecipeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterRecipes();
        }

        private void ResetRecipeSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchRecipeTextBox.Text = string.Empty;
            FilterRecipes();
        }

        private void FilterRecipes()
        {
            var searchText = SearchRecipeTextBox.Text.ToLower();
            var filteredRecipes = _context.Recipes
                .Where(r => string.IsNullOrEmpty(searchText) || r.RecipeName.ToLower().Contains(searchText))
                .Select(r => new
                {
                    r.RecipeId,
                    r.RecipeName,
                    r.Cost,
                    r.PreparationTime,
                    r.Instruction
                })
                .ToList()
                .Select(r => new
                {
                    r.RecipeId,
                    r.RecipeName,
                    Cost = Math.Round((double)r.Cost, 2).ToString("F2"),
                    r.PreparationTime,
                    r.Instruction
                })
                .OrderBy(r => r.RecipeName)
                .ToList();

            RecipeList.ItemsSource = filteredRecipes;
        }

        private void SearchIngredientTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterIngredients();
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterIngredients();
        }

        private void FilterIngredients()
        {
            var searchText = SearchIngredientTextBox.Text.ToLower();
            var selectedCategoryId = (int?)CategoryFilterComboBox.SelectedValue;

            var filteredIngredients = allIngredients
                .Where(i => (string.IsNullOrEmpty(searchText) || i.IngredientName.ToLower().Contains(searchText)) &&
                            (!selectedCategoryId.HasValue || i.CategoryId == selectedCategoryId.Value))
                .ToList();

            IngredientList.ItemsSource = filteredIngredients;
        }

        private void AddIngredientToRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ingredientId = (int)button.CommandParameter;

            var ingredient = allIngredients.FirstOrDefault(i => i.IngredientId == ingredientId);
            if (ingredient != null && !recipeIngredients.Any(ri => ri.IngredientId == ingredientId))
            {
                var addIngredientWindow = new AddIngredientCard(ingredient.IngredientName, ingredient.Unit, processingTypes);
                if (addIngredientWindow.ShowDialog() == true)
                {
                    recipeIngredients.Add(new RecipeIngredientItem
                    {
                        RecipeId = editingRecipeId,
                        IngredientId = ingredient.IngredientId,
                        IngredientName = ingredient.IngredientName,
                        Quantity = addIngredientWindow.Quantity,
                        Unit = ingredient.Unit,
                        ProcessingTypeId = addIngredientWindow.ProcessingTypeId,
                        ProcessingTypeName = processingTypes.FirstOrDefault(pt => pt.ProcessingTypeId == addIngredientWindow.ProcessingTypeId)?.ProcessingTypeName
                    });
                    RecipeIngredientsControl.ItemsSource = null;
                    RecipeIngredientsControl.ItemsSource = recipeIngredients;
                }
            }
            else
            {
                MessageBox.Show("Этот ингредиент уже добавлен в рецепт.");
            }
        }

        private void DeleteIngredientFromRecipe_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ingredientId = (int)button.CommandParameter;

            var ingredient = recipeIngredients.FirstOrDefault(ri => ri.IngredientId == ingredientId);
            if (ingredient != null)
            {
                recipeIngredients.Remove(ingredient);
                RecipeIngredientsControl.ItemsSource = null;
                RecipeIngredientsControl.ItemsSource = recipeIngredients;
            }
        }

        private void ClearForm()
        {
            RecipeNameTextBox.Text = string.Empty;
            RecipeCostTextBox.Text = string.Empty;
            RecipePrepTimeTextBox.Text = string.Empty;
            RecipeInstructionTextBox.Text = string.Empty;
            recipeIngredients = new List<RecipeIngredientItem>();
            RecipeIngredientsControl.ItemsSource = recipeIngredients;
        }

        private void RecipeCostTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(RecipeCostTextBox.Text + e.Text, out _);
        }

        private void RecipePrepTimeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(((TextBox)sender).Text + e.Text, out _);
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
