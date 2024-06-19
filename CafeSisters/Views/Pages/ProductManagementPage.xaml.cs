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

namespace CafeSisters.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductManagementPage.xaml
    /// </summary>
    public partial class ProductManagementPage : Page
    {
        private CafeSistersEntities _context;
        private bool isEditing = false;
        private int editingIngredientId;

        public ProductManagementPage()
        {
            InitializeComponent();
            _context = new CafeSistersEntities();
            LoadCategories();
            LoadStorageLocations();
            LoadIngredients();
        }

        private void LoadCategories()
        {
            var categories = _context.IngredientCategories.ToList();
            IngredientCategoryComboBox.ItemsSource = categories;
            CategoryFilterComboBox.ItemsSource = categories;
        }

        private void LoadStorageLocations()
        {
            var locations = _context.StorageLocations.ToList();
            IngredientStorageLocationComboBox.ItemsSource = locations;
            StorageLocationFilterComboBox.ItemsSource = locations;
        }

        private void LoadIngredients()
        {
            var ingredients = _context.Ingredients
                .Select(i => new
                {
                    i.IngredientId,
                    i.IngredientName,
                    CategoryName = i.IngredientCategories.CategoryName,
                    StorageLocationName = i.StorageLocations.LocationName,
                    i.Quantity,
                    i.Unit,
                    Proteins = i.NutritionalValues.Select(nv => nv.Proteins).FirstOrDefault(),
                    Fats = i.NutritionalValues.Select(nv => nv.Fats).FirstOrDefault(),
                    Carbohydrates = i.NutritionalValues.Select(nv => nv.Carbohydrates).FirstOrDefault(),
                    EnergyValue = i.NutritionalValues.Select(nv => nv.EnergyValue).FirstOrDefault()
                })
                .ToList();

            IngredientList.ItemsSource = ingredients;
        }

        private void CategoryFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterIngredients();
        }

        private void StorageLocationFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FilterIngredients();
        }

        private void SearchIngredientTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterIngredients();
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            CategoryFilterComboBox.SelectedIndex = -1;
            StorageLocationFilterComboBox.SelectedIndex = -1;
            SearchIngredientTextBox.Text = string.Empty;
            LoadIngredients();
        }

        private void FilterIngredients()
        {
            var selectedCategoryId = (int?)CategoryFilterComboBox.SelectedValue;
            var selectedStorageLocationId = (int?)StorageLocationFilterComboBox.SelectedValue;
            var searchText = SearchIngredientTextBox.Text.ToLower();

            var filteredIngredients = _context.Ingredients
                .Where(i => (!selectedCategoryId.HasValue || i.CategoryId == selectedCategoryId.Value)
                         && (!selectedStorageLocationId.HasValue || i.StorageLocationId == selectedStorageLocationId.Value)
                         && (string.IsNullOrEmpty(searchText) || i.IngredientName.ToLower().Contains(searchText)))
                .Select(i => new
                {
                    i.IngredientId,
                    i.IngredientName,
                    CategoryName = i.IngredientCategories.CategoryName,
                    StorageLocationName = i.StorageLocations.LocationName,
                    i.Quantity,
                    i.Unit,
                    Proteins = i.NutritionalValues.Select(nv => nv.Proteins).FirstOrDefault(),
                    Fats = i.NutritionalValues.Select(nv => nv.Fats).FirstOrDefault(),
                    Carbohydrates = i.NutritionalValues.Select(nv => nv.Carbohydrates).FirstOrDefault(),
                    EnergyValue = i.NutritionalValues.Select(nv => nv.EnergyValue).FirstOrDefault()
                })
                .ToList();

            IngredientList.ItemsSource = filteredIngredients;
        }

        private void EditIngredient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingIngredientId = (int)button.CommandParameter;
            var ingredient = _context.Ingredients.FirstOrDefault(i => i.IngredientId == editingIngredientId);

            if (ingredient != null)
            {
                isEditing = true;
                IngredientNameTextBox.Text = ingredient.IngredientName;
                IngredientCategoryComboBox.SelectedValue = ingredient.CategoryId;
                IngredientStorageLocationComboBox.SelectedValue = ingredient.StorageLocationId;
                IngredientQuantityTextBox.Text = ingredient.Quantity.ToString();
                IngredientUnitTextBlock.Text = ingredient.Unit;

                var nutritionalValues = ingredient.NutritionalValues.FirstOrDefault();
                if (nutritionalValues != null)
                {
                    ProteinsTextBox.Text = nutritionalValues.Proteins.ToString();
                    FatsTextBox.Text = nutritionalValues.Fats.ToString();
                    CarbohydratesTextBox.Text = nutritionalValues.Carbohydrates.ToString();
                    CaloriesTextBox.Text = nutritionalValues.EnergyValue.ToString();
                }
                else
                {
                    ProteinsTextBox.Text = FatsTextBox.Text = CarbohydratesTextBox.Text = CaloriesTextBox.Text = string.Empty;
                }

                IngredientForm.Visibility = Visibility.Visible;
            }
        }

        private void DeleteIngredient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ingredientId = (int)button.CommandParameter;
            var ingredient = _context.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId);

            if (ingredient != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот продукт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Ingredients.Remove(ingredient);
                    _context.SaveChanges();
                    LoadIngredients();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(IngredientNameTextBox.Text) || IngredientCategoryComboBox.SelectedValue == null || IngredientStorageLocationComboBox.SelectedValue == null || string.IsNullOrEmpty(IngredientQuantityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            if (!decimal.TryParse(IngredientQuantityTextBox.Text, out decimal quantity))
            {
                MessageBox.Show("Пожалуйста, введите правильное количество.");
                return;
            }
            if (!decimal.TryParse(ProteinsTextBox.Text, out decimal proteins) || !decimal.TryParse(FatsTextBox.Text, out decimal fats) ||
                           !decimal.TryParse(CarbohydratesTextBox.Text, out decimal carbohydrates) || !decimal.TryParse(CaloriesTextBox.Text, out decimal calories))
            {
                MessageBox.Show("Пожалуйста, введите правильные значения для белков, жиров, углеводов и калорий.");
                return;
            }

            if (isEditing)
            {
                var ingredient = _context.Ingredients.FirstOrDefault(i => i.IngredientId == editingIngredientId);
                if (ingredient != null)
                {
                    ingredient.IngredientName = IngredientNameTextBox.Text;
                    ingredient.CategoryId = (int)IngredientCategoryComboBox.SelectedValue;
                    ingredient.StorageLocationId = (int)IngredientStorageLocationComboBox.SelectedValue;
                    ingredient.Quantity = quantity;

                    var nutritionalValues = ingredient.NutritionalValues.FirstOrDefault();
                    if (nutritionalValues != null)
                    {
                        nutritionalValues.Proteins = proteins;
                        nutritionalValues.Fats = fats;
                        nutritionalValues.Carbohydrates = carbohydrates;
                        nutritionalValues.EnergyValue = calories;
                    }
                    else
                    {
                        nutritionalValues = new NutritionalValues
                        {
                            IngredientId = ingredient.IngredientId,
                            Proteins = proteins,
                            Fats = fats,
                            Carbohydrates = carbohydrates,
                            EnergyValue = calories
                        };
                        _context.NutritionalValues.Add(nutritionalValues);
                    }
                }
            }
            else
            {
                var newIngredient = new Ingredients
                {
                    IngredientName = IngredientNameTextBox.Text,
                    CategoryId = (int)IngredientCategoryComboBox.SelectedValue,
                    StorageLocationId = (int)IngredientStorageLocationComboBox.SelectedValue,
                    Quantity = quantity,
                    Unit = IngredientUnitTextBlock.Text
                };

                _context.Ingredients.Add(newIngredient);
                _context.SaveChanges();

                var newNutritionalValues = new NutritionalValues
                {
                    IngredientId = newIngredient.IngredientId,
                    Proteins = proteins,
                    Fats = fats,
                    Carbohydrates = carbohydrates,
                    EnergyValue = calories
                };
                _context.NutritionalValues.Add(newNutritionalValues);
            }

            _context.SaveChanges();
            ClearForm();
            IngredientForm.Visibility = Visibility.Collapsed;
            LoadIngredients();
            MessageBox.Show("Ингредиент успешно сохранен!");
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            IngredientForm.Visibility = Visibility.Collapsed;
        }

        private void ClearForm()
        {
            isEditing = false;
            editingIngredientId = 0;
            IngredientNameTextBox.Text = string.Empty;
            IngredientCategoryComboBox.SelectedIndex = -1;
            IngredientStorageLocationComboBox.SelectedIndex = -1;
            IngredientQuantityTextBox.Text = string.Empty;
            IngredientUnitTextBlock.Text = string.Empty;
            ProteinsTextBox.Text = string.Empty;
            FatsTextBox.Text = string.Empty;
            CarbohydratesTextBox.Text = string.Empty;
            CaloriesTextBox.Text = string.Empty;
        }

        private void IngredientQuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void ds_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddIngridients_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CreateIngridients_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}