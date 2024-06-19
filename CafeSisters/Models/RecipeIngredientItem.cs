using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeSisters.Models
{
    public class RecipeIngredientItem
    {
        public int RecipeId { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        private string Instruction { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public int ProcessingTypeId { get; set; }
        public string ProcessingTypeName { get; set; }
    }
}
