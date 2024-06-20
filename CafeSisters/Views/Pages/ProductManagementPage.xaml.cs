using CafeSisters.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CafeSisters.Views.Pages
{
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
            CategoryList.ItemsSource = categories;
        }

        private void LoadStorageLocations()
        {
            var locations = _context.StorageLocations.ToList();
            IngredientStorageLocationComboBox.ItemsSource = locations;
            StorageLocationFilterComboBox.ItemsSource = locations;
            StorageLocationList.ItemsSource = locations;
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
                IngredientUnitTextBox.Text = ingredient.Unit;

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
        private void ShowAddQuantityForm_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ingredientId = (int)button.CommandParameter;

            var ingredient = _context.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId);
            if (ingredient != null)
            {
                IngredientComboBox.SelectedValue = ingredientId;
                AddIngredientForm.Visibility = Visibility.Visible;
            }
        }
        private void CreateIngredients_Click(object sender, RoutedEventArgs e)
        {
            IngredientForm.Visibility = Visibility.Visible;
            ClearForm();
        }
        private void CancelAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddIngredientForm.Visibility = Visibility.Collapsed;
        }
        private void SaveAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (IngredientComboBox.SelectedValue == null || string.IsNullOrEmpty(AddIngredientQuantityTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, выберите продукт и введите количество.");
                return;
            }

            if (!decimal.TryParse(AddIngredientQuantityTextBox.Text, out decimal quantity))
            {
                MessageBox.Show("Пожалуйста, введите правильное количество.");
                return;
            }

            var ingredientId = (int)IngredientComboBox.SelectedValue;
            var ingredient = _context.Ingredients.FirstOrDefault(i => i.IngredientId == ingredientId);
            if (ingredient != null)
            {
                ingredient.Quantity += quantity;
                _context.SaveChanges();
                LoadIngredients();
                AddIngredientForm.Visibility = Visibility.Collapsed;
                MessageBox.Show("Ингредиент успешно добавлен!");
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
                    ingredient.Unit = IngredientUnitTextBox.Text;

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
                    Unit = IngredientUnitTextBox.Text
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
            IngredientUnitTextBox.Text = string.Empty;
            ProteinsTextBox.Text = string.Empty;
            FatsTextBox.Text = string.Empty;
            CarbohydratesTextBox.Text = string.Empty;
            CaloriesTextBox.Text = string.Empty;
        }

        private void IngredientQuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(e.Text, out _);
        }

        private void CreateIngridients_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            IngredientForm.Visibility = Visibility.Visible;
        }

        private void AddIngridients_Click(object sender, RoutedEventArgs e)
        {
            IngredientForm.Visibility = Visibility.Collapsed;
            CategoryManagementForm.Visibility = Visibility.Collapsed;
            StorageLocationManagementForm.Visibility = Visibility.Collapsed;
        }

        private void ManageCategories_Click(object sender, RoutedEventArgs e)
        {
            CategoryManagementForm.Visibility = Visibility.Visible;
            IngredientForm.Visibility = Visibility.Collapsed;
            StorageLocationManagementForm.Visibility = Visibility.Collapsed;
        }

        private void ManageStorageLocations_Click(object sender, RoutedEventArgs e)
        {
            StorageLocationManagementForm.Visibility = Visibility.Visible;
            IngredientForm.Visibility = Visibility.Collapsed;
            CategoryManagementForm.Visibility = Visibility.Collapsed;
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoryId = (int)button.CommandParameter;
            var category = _context.IngredientCategories.FirstOrDefault(c => c.CategoryId == categoryId);

            if (category != null)
            {
                CategoryNameTextBox.Text = category.CategoryName;
                CategoryManagementForm.Tag = categoryId;
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoryId = (int)button.CommandParameter;
            var category = _context.IngredientCategories.FirstOrDefault(c => c.CategoryId == categoryId);

            if (category != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту категорию?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.IngredientCategories.Remove(category);
                    _context.SaveChanges();
                    LoadCategories();
                }
            }
        }

        private void SaveCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CategoryNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название категории.");
                return;
            }

            var categoryId = (int?)CategoryManagementForm.Tag;

            if (categoryId.HasValue)
            {
                var category = _context.IngredientCategories.FirstOrDefault(c => c.CategoryId == categoryId.Value);
                if (category != null)
                {
                    category.CategoryName = CategoryNameTextBox.Text;
                }
            }
            else
            {
                var newCategory = new IngredientCategories
                {
                    CategoryName = CategoryNameTextBox.Text
                };
                _context.IngredientCategories.Add(newCategory);
            }

            _context.SaveChanges();
            CategoryManagementForm.Tag = null;
            CategoryNameTextBox.Text = string.Empty;
            LoadCategories();
            MessageBox.Show("Категория успешно сохранена!");
        }

        private void CancelCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            CategoryManagementForm.Visibility = Visibility.Collapsed;
            IngredientForm.Visibility = Visibility.Visible;
            CategoryNameTextBox.Text = string.Empty;
        }

        private void EditStorageLocation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var locationId = (int)button.CommandParameter;
            var location = _context.StorageLocations.FirstOrDefault(l => l.StorageLocationId == locationId);

            if (location != null)
            {
                StorageLocationNameTextBox.Text = location.LocationName;
                StorageLocationManagementForm.Tag = locationId;
            }
        }

        private void DeleteStorageLocation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var locationId = (int)button.CommandParameter;
            var location = _context.StorageLocations.FirstOrDefault(l => l.StorageLocationId == locationId);

            if (location != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить это место хранения?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.StorageLocations.Remove(location);
                    _context.SaveChanges();
                    LoadStorageLocations();
                }
            }
        }

        private void SaveStorageLocationButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(StorageLocationNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название места хранения.");
                return;
            }

            var locationId = (int?)StorageLocationManagementForm.Tag;

            if (locationId.HasValue)
            {
                var location = _context.StorageLocations.FirstOrDefault(l => l.StorageLocationId == locationId.Value);
                if (location != null)
                {
                    location.LocationName = StorageLocationNameTextBox.Text;
                }
            }
            else
            {
                var newLocation = new StorageLocations
                {
                    LocationName = StorageLocationNameTextBox.Text
                };
                _context.StorageLocations.Add(newLocation);
            }

            _context.SaveChanges();
            StorageLocationManagementForm.Tag = null;
            StorageLocationNameTextBox.Text = string.Empty;
            LoadStorageLocations();
            MessageBox.Show("Место хранения успешно сохранено!");
        }

        private void CancelStorageLocationButton_Click(object sender, RoutedEventArgs e)
        {
            StorageLocationManagementForm.Visibility = Visibility.Collapsed;
            IngredientForm.Visibility = Visibility.Visible;
            StorageLocationNameTextBox.Text = string.Empty;
        }
    }
}
