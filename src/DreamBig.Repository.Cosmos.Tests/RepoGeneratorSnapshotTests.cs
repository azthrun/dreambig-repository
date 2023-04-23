using DreamBig.Repository.Cosmos.Generators;

namespace DreamBig.Repository.Cosmos.Tests;

[UsesVerify]
public class RepoGeneratorSnapshotTests
{
    [Fact]
    public Task GeneratorCorrectlyProduceResults()
    {
        var source = @"using DreamBig.Repository.Cosmos.Attributes;

namespace SomeNamespace.Models;

[UseRepo]
public class User
{
    public string DisplayName { get; set; }
}
";
        return TestHelper.Verify(source, new RepoGenerator());
    }
}