using System.Diagnostics;
using System.Text.Json;
using Paramore.Brighter;

namespace Infrastructure.PublicEventHandlers;

public class ExternalEventV1 : IRequest
{
    public const string EventTypeName = "external.event.v1";
    public Guid Id { get; set; }
    public Activity Span { get; set; }
    
    public string Identifier { get; set; }
    
    public string AsString()
    {
        return JsonSerializer.Serialize(this);
    }
}