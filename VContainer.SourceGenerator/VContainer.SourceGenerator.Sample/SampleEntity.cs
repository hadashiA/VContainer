using Generators;

namespace VContainer.SourceGenerator.Sample;

// This code will not compile until you build the project with the Source Generators

[Report]
public partial class SampleEntity
{
    public int Id { get; } = 42;
    public string? Name { get; } = "Sample";
}