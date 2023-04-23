using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DreamBig.Repository.Cosmos.Tests;

public static class TestHelper
{
    public static Task Verify<TGenerator>(string source, TGenerator generator) where TGenerator : IIncrementalGenerator
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        IEnumerable<PortableExecutableReference> references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
        };

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references
            );

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGenerators(compilation);
        return Verifier.Verify(driver).UseDirectory("SnapshotResults");
    }
}
