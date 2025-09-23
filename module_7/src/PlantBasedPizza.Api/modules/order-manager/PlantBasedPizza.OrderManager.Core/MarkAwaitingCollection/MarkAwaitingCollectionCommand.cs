



namespace PlantBasedPizza.OrderManager.Core.MarkAwaitingCollection;

public record MarkAwaitingCollectionCommand
{
    public string OrderIdentifier { get; init; } = string.Empty;
    public string CorrelationId { get; init; } = string.Empty;
}