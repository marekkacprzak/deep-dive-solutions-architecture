namespace Core.Handlers;

public record ExampleEvent(string Identifier)
{
    public string Identifier { get; } = Identifier;
}