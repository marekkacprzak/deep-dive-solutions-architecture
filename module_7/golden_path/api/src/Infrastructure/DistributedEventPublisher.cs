using System.Text.Json;
using Infrastructure.PublicEvents;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Paramore.Brighter;

namespace Infrastructure;

public class DistributedEventPublisher(IAmACommandProcessor processor, IDistributedCache orderCache, IServiceScopeFactory serviceScope) : ExampleEventPublisher
{
    public async Task Publish(ExampleEventV1 evt)
    {
        using var scope = serviceScope.CreateScope();
        
        await orderCache.SetStringAsync($"entity:{evt.Identifier}", JsonSerializer.Serialize(new
        {
            identifier = evt.Identifier
        }), new DistributedCacheEntryOptions()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        });
        await processor.PostAsync(evt);
    }
}