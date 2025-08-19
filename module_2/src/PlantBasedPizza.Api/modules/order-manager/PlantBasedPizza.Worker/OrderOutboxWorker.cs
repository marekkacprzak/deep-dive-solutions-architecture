using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlantBasedPizza.Events;
using PlantBasedPizza.OrderManager.Core;
using PlantBasedPizza.Shared.Events;

namespace PlantBasedPizza.OrderManager.Infrastructure;

public class OrderOutboxWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderOutboxWorker> _logger;
    private readonly ActivitySource _source;

    public OrderOutboxWorker(ILogger<OrderOutboxWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _source = new ActivitySource(ApplicationDefaults.ServiceName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

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
                            var orderCreatedEvent = JsonSerializer.Deserialize<OrderCreatedEvent>(outboxItem.EventData);
                            await domainEventDispatcher.PublishAsync(orderCreatedEvent);
                            await eventPublisher.Publish(orderCreatedEvent);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderSubmittedEvent):
                            var orderSubmittedEvent = JsonSerializer.Deserialize<OrderSubmittedEvent>(outboxItem.EventData);
                            await domainEventDispatcher.PublishAsync(orderSubmittedEvent);
                            await eventPublisher.Publish(orderSubmittedEvent);
                            outboxItem.Processed = true;
                            break;
                        case nameof(OrderCompletedEvent):
                            var orderCompletedEvent = JsonSerializer.Deserialize<OrderCompletedEvent>(outboxItem.EventData);
                            await domainEventDispatcher.PublishAsync(orderCompletedEvent);
                            await eventPublisher.Publish(orderCompletedEvent);
                            outboxItem.Processed = true;
                            break;
                        default:
                            _logger.LogWarning("Unknown event type: {EventType}", outboxItem.EventType);
                            outboxItem.Failed = true;
                            outboxItem.FailureReason = $"Unknown event type: {outboxItem.EventType}";
                            break;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occured while processing outbox item.");
                    outboxItem.Failed = true;
                    outboxItem.FailureReason =
                        $"An error occured while processing outbox item.: {e.Message} - {e.StackTrace}";
                }

                try
                {
                    dbContext.OutboxItems.Update(outboxItem);
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception e)
                {
                    Activity.Current?.AddException(e);
                    _logger.LogError(e, "An error occured while processing outbox item.");
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
                _logger.LogWarning(ex, "Failure parsing tracecontext from outbox item");
            }

        return null;
    }
}