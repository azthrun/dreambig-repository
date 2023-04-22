using DreamBig.Repository.Cosmos.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DreamBig.Repository.Cosmos.Tests;

public static class TestHelper
{
    public static Task Verify(string source)
    {
        SyntaxTree tree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Test",
            syntaxTrees: new[] { tree },
            references: references
        );

        var generator = new RepoGenerator();
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver.RunGenerators(compilation);
        return Verifier.Verify(driver).UseDirectory("Snapshots");
    }
}