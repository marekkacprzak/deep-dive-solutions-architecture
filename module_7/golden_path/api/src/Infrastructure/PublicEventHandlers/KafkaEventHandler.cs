using Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Paramore.Brighter;
using Paramore.Brighter.Logging.Attributes;
using Saunter.Attributes;

namespace Infrastructure.PublicEventHandlers;

[AsyncApi]
public class KafkaEventHandler(ILogger<KafkaEventHandler> logger, IServiceScopeFactory serviceScopeFactory, IAmACommandProcessor processor) : RequestHandlerAsync<ExternalEventV1>
{
    [Channel("delivery.driver-collected")] // Creates a Channel
    [SubscribeOperation(typeof(ExampleEvent), Summary = "Handle a driver order collected event.", OperationId = "delivery.driver-collected")]
    [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
    // [UseResiliencePipeline(step: 2, policy: Retry.EXPONENTIAL_RETRYPOLICYASYNC)]
    public override async Task<ExternalEventV1> HandleAsync(ExternalEventV1 command, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<ExampleEventHandler>();

            await handler.Handle(new ExampleEvent(command.Identifier));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failure handling {EventType} for Identifier {Identifier}", ExternalEventV1.EventTypeName, command.Identifier);
            await FallbackAsync(command, cancellationToken);
        }

        return command;
    }

    public override async Task<ExternalEventV1> FallbackAsync(ExternalEventV1 command, CancellationToken cancellationToken = default)
    {
        await processor.PostAsync(new DLQMessage(ExternalEventV1.EventTypeName)
        {
            Data = command.AsString(),
        });
        
        return await base.FallbackAsync(command, cancellationToken);
    }
}