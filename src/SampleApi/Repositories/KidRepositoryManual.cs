using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Cosmos;
using Microsoft.Azure.Cosmos;
using SampleApi.Models;

namespace SampleApi.Repositories;

public class KidRepositoryManual : BaseRepository<Kid>
{
    public KidRepositoryManual(IProvider<CosmosClient, Database>? cosmosProvider, string? containerName = null, ILogger<BaseRepository<Kid>>? logger = null) : base(cosmosProvider, containerName, logger)
    {
    }
}
