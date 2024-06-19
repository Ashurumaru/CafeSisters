using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CafeSisters.Models
{
    public class MenuRecipeItem
    {
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public decimal? Cost { get; set; }
        public string Instruction { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Proteins { get; set; }
        public decimal? Fats { get; set; }
        public decimal? Carbohydrates { get; set; }
        public decimal? EnergyValue { get; set; }
    }
}
