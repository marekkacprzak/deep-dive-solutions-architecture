



using Infrastructure.PublicEvents;

namespace Infrastructure;

public interface ExampleEventPublisher
{
    Task Publish(ExampleEventV1 evt);
}