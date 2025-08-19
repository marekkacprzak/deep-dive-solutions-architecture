using System.Text.Json;
using PlantBasedPizza.Recipes.Core.Entities;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Recipes.Core.Events
{
    internal class RecipeCreatedEvent : DomainEvent
    {
        public RecipeCreatedEvent(Recipe recipe, string correlationId)
        {
            this.Recipe = recipe;
            this.CorrelationId = correlationId;
            this.EventId = Guid.NewGuid().ToString();
            this.EventDate = DateTime.Now.ToUniversalTime();
        }
        
        public override string EventName => "recipes.recipe-created";

        public override string EventVersion => "v1";
        public override string AsString()
        {
            return JsonSerializer.Serialize(this);
        }


        public override string EventId { get; }
        public override DateTime EventDate { get; }
        public override string CorrelationId { get; set; }
        public Recipe Recipe { get; }
    }
}