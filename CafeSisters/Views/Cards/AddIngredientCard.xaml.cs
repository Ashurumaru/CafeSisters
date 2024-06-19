using CafeSisters.Data;
using System.Collections.Generic;
using System.Windows;

namespace CafeSisters.Views.Cards
{
    public partial class AddIngredientCard : Window
    {
        public decimal Quantity { get; private set; }
        public int ProcessingTypeId { get; private set; }
        public List<ProcessingTypes> ProcessingTypesList { get; set; }

        public AddIngredientCard(string ingredientName, string unit, List<ProcessingTypes> processingTypes)
        {
            InitializeComponent();
            IngredientNameTextBlock.Text = ingredientName;
            UnitTextBlock.Text = unit;
            ProcessingTypesList = processingTypes;
            ProcessingTypeComboBox.ItemsSource = ProcessingTypesList;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(QuantityTextBox.Text, out decimal quantity))
            {
                Quantity = quantity;
                ProcessingTypeId = (int)ProcessingTypeComboBox.SelectedValue;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите правильное количество.");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void QuantityTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !decimal.TryParse(QuantityTextBox.Text + e.Text, out _);
        }
    }
}
