namespace PlantBasedPizza.Shared.Events
{
    public interface IPublicEvent
    {
        string EventName { get; }
        
        string EventVersion { get; }
        
        DateTime EventDate { get; }
    }
}