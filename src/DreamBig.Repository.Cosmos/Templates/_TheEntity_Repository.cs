using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Cosmos;
using Microsoft.Extensions.Logging;

namespace _TheNamespace_;

public partial class _TheEntity_Repository<T> : BaseRepository<T> where T : IEntity
{
    public _TheEntity_Repository(BaseProvider? cosmosProvider, string? containerName = null, ILogger<_TheEntity_Repository<T>>? logger = null) : base(cosmosProvider, containerName, logger)
    {
    }
}