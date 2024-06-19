using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeSisters.Models
{
    public class IngredientItem
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string CategoryName { get; set; }
        public string StorageLocation { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal Proteins { get; set; }
        public decimal Fats { get; set; }
        public decimal Carbohydrates { get; set; }
        public decimal EnergyValue { get; set; }
    }

}
