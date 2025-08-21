using System.Text.Json.Serialization;

namespace PlantBasedPizza.Recipes.Core.Entities
{
    public class Recipe
    {
        [JsonConstructor]
        private Recipe()
        {
            this.RecipeIdentifier = "";
            this.Name = "";
            this.Ingredients = new List<Ingredient>();
        }
        
        public Recipe(string recipeIdentifier, string name, decimal price)
        {
            this.RecipeIdentifier = recipeIdentifier;
            this.Name = name;
            this.Price = price;
            this.Ingredients = new List<Ingredient>();

            // Domain event will be raised by RecipeFactory
        }
        
        [JsonPropertyName("recipeIdentifier")]
        public string RecipeIdentifier { get; init; }
        
        [JsonPropertyName("name")]
        public string Name { get; init; }
        
        [JsonPropertyName("price")]
        public decimal Price { get; init; }
        
        [JsonPropertyName("ingredients")]
        public List<Ingredient> Ingredients { get; set; }

        public void AddIngredient(string name, int quantity)
        {
            if (this.Ingredients == null)
            {
                this.Ingredients = new List<Ingredient>();
            }
            
            this.Ingredients.Add(new Ingredient(name, quantity));
        }
    }
}