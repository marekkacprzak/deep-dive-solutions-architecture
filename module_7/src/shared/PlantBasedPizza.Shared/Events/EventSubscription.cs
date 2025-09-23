using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.Kafka;

namespace PlantBasedPizza.Shared.Events;

public class EventSubscription<T>
    : KafkaSubscription<T> where T : IRequest
{
    public EventSubscription(string applicationName, string channelName, string eventName)
        : base(new SubscriptionName(applicationName), new ChannelName(channelName), new RoutingKey(eventName),
            groupId: applicationName, messagePumpType: MessagePumpType.Proactor, timeOut: TimeSpan.FromMilliseconds(300), makeChannels: OnMissingChannel.Validate)
    {
    }
}