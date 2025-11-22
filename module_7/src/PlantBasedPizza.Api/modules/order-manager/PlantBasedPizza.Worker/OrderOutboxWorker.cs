using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.OrderManager.DataTransfer;
using PlantBasedPizza.OrderManager.Infrastructure;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.Worker;

/// <summary>
/// 
/// </summary>
/// <param name="logger"></param>
/// <param name="scopeFactory"></param>
public class OrderOutboxWorker(ILogger<OrderOutboxWorker> logger, IServiceScopeFactory scopeFactory)
    : BackgroundService
{
    private readonly ActivitySource _source = new(ApplicationDefaults.ServiceName);
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stoppingToken"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagerDbContext>();
        var domainEventDispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventDispatcher>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<OrderEventPublisher>();

        while (!stoppingToken.IsCancellationRequested)
        {
            var outboxItems = await dbContext.OutboxItems.Where(p => !p.Processed && !p.Failed).ToListAsync(stoppingToken);

            foreach (var outboxItem in outboxItems)
            {
                using var processingActivity = StartFromOutboxItem(outboxItem);
                processingActivity?.Start();

                try
                {
                    switch (outboxItem.EventType)
                    {
                        case nameof(OrderCreatedEvent):
                            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(outboxItem.EventData, _jsonSerializerOptions);
                            
                            // Call domain event handlers
                            await domainEventDispatcher.PublishAsync(orderCreatedEvent);
                            
                            // Publish the public event externally
                            await eventPublisher.Publish(new OrderCreatedEventV1(orderCreatedEvent!.OrderIdentifier));
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderSubmittedEvent):
                            var orderSubmittedEvent = JsonSerializer.Deserialize<OrderSubmittedEvent>(outboxItem.EventData, _jsonSerializerOptions);
                            await domainEventDispatcher.PublishAsync(orderSubmittedEvent);
                            await eventPublisher.Publish(new OrderSubmittedEventV1(orderSubmittedEvent!.OrderIdentifier));
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderReadyForDeliveryEvent):
                            var orderReadyForDeliveryEvent = JsonSerializer.Deserialize<OrderReadyForDeliveryEvent>(outboxItem.EventData, _jsonSerializerOptions);
                            await domainEventDispatcher.PublishAsync(orderReadyForDeliveryEvent);
                            await eventPublisher.Publish(new OrderReadyForDeliveryEventV1(
                                orderReadyForDeliveryEvent!.OrderIdentifier,
                                orderReadyForDeliveryEvent.DeliveryAddressLine1,
                                orderReadyForDeliveryEvent.DeliveryAddressLine2,
                                orderReadyForDeliveryEvent.DeliveryAddressLine3,
                                orderReadyForDeliveryEvent.DeliveryAddressLine4,
                                orderReadyForDeliveryEvent.DeliveryAddressLine5,
                                orderReadyForDeliveryEvent.Postcode));
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderCompletedEvent):
                            var orderCompletedEvent = JsonSerializer.Deserialize<OrderCompletedEvent>(outboxItem.EventData, _jsonSerializerOptions);
                            await domainEventDispatcher.PublishAsync(orderCompletedEvent);
                            await eventPublisher.Publish(new OrderCompletedEventV1(orderCompletedEvent!.OrderIdentifier));
                            outboxItem.Processed = true;
                            break;
                        default:
                            logger.LogWarning("Unknown event type: {EventType}", outboxItem.EventType);
                            outboxItem.Failed = true;
                            outboxItem.FailureReason = $"Unknown event type: {outboxItem.EventType}";
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "An error occured while processing outbox item.");
                    outboxItem.Failed = true;
                    outboxItem.FailureReason =
                        $"An error occured while processing outbox item.: {e.Message} - {e.StackTrace}";
                }

                try
                {
                    var item = await dbContext.OutboxItems.FindAsync(outboxItem.ItemId);
                    item!.Failed = outboxItem.Failed;
                    item.Processed = outboxItem.Processed;
                    item.FailureReason = outboxItem.FailureReason;
                    dbContext.OutboxItems.Update(item);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    Activity.Current?.AddException(e);
                    logger.LogError(e, "An error occured while processing outbox item.");
                }
            }

            await Task.Delay(5000, stoppingToken);
        }
    }

    private Activity? StartFromOutboxItem(OutboxItem outboxItem)
    {
        if (!string.IsNullOrEmpty(outboxItem.TraceId))
            try
            {
                var context = ActivityContext.Parse(outboxItem.TraceId, null);
                var messageProcessingActivity = _source.StartActivity("process", ActivityKind.Internal, context);

                return messageProcessingActivity;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failure parsing tracecontext from outbox item");
            }

        return null;
    }
}