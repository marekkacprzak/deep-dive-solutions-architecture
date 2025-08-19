using System.Text.Json.Serialization;

namespace PlantBasedPizza.OrderManager.Core
{
    public class OrderHistory
    {
        [JsonConstructor]
        private OrderHistory()
        {
            Description = "";
        }
        
        public OrderHistory(string description, DateTime historyDate)
        {
            Description = description;
            HistoryDate = historyDate;
        }
        
        [JsonPropertyName("orderHistoryId")]
        public int OrderHistoryId { get; private set; }
        
        [JsonPropertyName("description")]
        public string Description { get; private set; }
        
        [JsonPropertyName("historyDate")]
        public DateTime HistoryDate { get; private set; }
    }
}