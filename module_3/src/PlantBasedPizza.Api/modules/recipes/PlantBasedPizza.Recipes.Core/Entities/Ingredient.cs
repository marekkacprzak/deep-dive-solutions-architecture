using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Entities
{
    public class Ingredient
    {
        [JsonConstructor]
        private Ingredient()
        {
        }
        
        public Ingredient(string name, int quantity)
        {
            this.Name = name;
            this.Quantity = quantity;
        }
        
        public int IngredientIdentifier { get; init; }

        public string RecipeIdentifier { get; init; } = "";

        public string Name { get; init; } = "";

        public int Quantity { get; init; }
    }
}