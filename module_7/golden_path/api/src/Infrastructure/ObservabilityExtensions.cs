using System.Diagnostics;
using Core.Handlers;

namespace Infrastructure;

public static class ObservabilityExtensions
{
    public static void AddToTelemetry(this ExampleEvent evt)
    {
        if (Activity.Current is null)
        {
            return;
        }

        Activity.Current.AddTag("identifier", evt.Identifier);
    }
}