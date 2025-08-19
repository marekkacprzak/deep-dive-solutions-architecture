using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using PlantBasedPizza.Events;
using PlantBasedPizza.Kitchen.Core.Handlers;

namespace PlantBasedPizza.Kitchen.Infrastructure.EventHandlers;

public class OrderSubmittedKafkaEventHandler : RequestHandler<OrderSubmittedEvent>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<OrderSubmittedKafkaEventHandler> _logger;

    public OrderSubmittedKafkaEventHandler(IServiceScopeFactory serviceScopeFactory, ILogger<OrderSubmittedKafkaEventHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    [RequestLogging(step: 1, timing: HandlerTiming.Before)]
    public override OrderSubmittedEvent Handle(OrderSubmittedEvent command)
    {
        _logger.LogInformation("Handling OrderSubmittedEvent for OrderId: {OrderId}", command.OrderIdentifier);

        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<OrderSubmittedEventHandler>();

        handler.Handle(command).GetAwaiter().GetResult();

        return base.Handle(command);
    }
}