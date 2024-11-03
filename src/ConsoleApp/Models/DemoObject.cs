namespace ConsoleApp.Models;

public record DemoObject
{
    public string Name { get; init; }
    public int Age { get; init; }
    public NestedDemoObject Nested { get; init; }
}

public record NestedDemoObject
{
    public string Name { get; init; }
    public int Age { get; init; }
}
