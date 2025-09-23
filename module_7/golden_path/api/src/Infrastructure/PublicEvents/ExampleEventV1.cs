using System.Diagnostics;
using Paramore.Brighter;

namespace Infrastructure.PublicEvents;

public class ExampleEventV1 : IRequest
{
    public string Identifier { get; set; }
    
    public Guid Id { get; set; }
    public Activity Span { get; set; }
}