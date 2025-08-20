namespace PlantBasedPizza.Shared.Events
{
    public interface IPublicEvent
    {
        string EventName { get; }
        
        string EventVersion { get; }
        
        string EventId { get; }
        
        DateTime EventDate { get; }
    }
}