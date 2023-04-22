using VerifyXunit;

namespace DreamBig.Repository.Cosmos.Tests;

[UsesVerify]
public class GeneratorSnapshotTests
{
    [Fact]
    public Task RepoGeneratorCreateSourceCorrectly()
    {
        string source = @"
namespace DreamBig.Repository.Cosmos.Tests;

[UseFastRepo]
public class Model
{
    public string? Id { get; set; }
}
        ";

        return TestHelper.Verify(source);
    }
}