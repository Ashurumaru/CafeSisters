//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CafeSisters.Data
{
    using System;
    using System.Collections.Generic;
    
    public partial class RecipeInstructions
    {
        public int InstructionId { get; set; }
        public int RecipeId { get; set; }
        public string Instructions { get; set; }
    
        public virtual Recipes Recipes { get; set; }
    }
}
