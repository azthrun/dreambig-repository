using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DreamBig.Repository.Cosmos.Generators;

[Generator]
public sealed class RepoGenerator : IIncrementalGenerator
{
    private string tests = @"
namespace TestNamespace;

public class Test
{
    public string? Id { get; set; }
}
";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => 
            ctx.AddSource(
                "Test.g.cs",
                SourceText.From(tests, Encoding.UTF8)
            ));
    }
}
