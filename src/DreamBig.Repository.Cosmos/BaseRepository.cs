using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Exceptions;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Net;

namespace DreamBig.Repository.Cosmos;

public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : IEntity
{
    private readonly IProvider<CosmosClient, Database> cosmosProvider;
    private readonly string? containerName;
    private readonly ILogger<BaseRepository<TEntity>>? logger;

    protected readonly CosmosClient cosmosClient;
    protected readonly Database database;
    protected readonly Container container;

    public BaseRepository(IProvider<CosmosClient, Database>? cosmosProvider, string? containerName = null, ILogger<BaseRepository<TEntity>>? logger = null)
    {
        if (cosmosProvider is null)
        {
            throw new RepositoryException("Cosmos Provider was not instantiated");
        }

        this.cosmosProvider = cosmosProvider;
        this.containerName = containerName ?? typeof(TEntity).Name;
        this.logger = logger;

        try
        {
            cosmosClient = this.cosmosProvider.GetClient() ?? throw new RepositoryException("Unable to retrieve Cosmos client");
            database = this.cosmosProvider.GetDatabaseAsync().Result ?? throw new RepositoryException("Unable to retrieve database");
            container = database!.GetContainer(this.containerName);
        }
        catch (Exception ex) when (ex is not RepositoryException)
        {
            throw new RepositoryException(ex.Message, ex);
        }
    }

    public virtual async Task CreateAsync(TEntity? entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        logger?.LogInformation("Creating document");

        try
        {
            await container.CreateItemAsync(entity);
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to create document", ex);
        }
    }

    public virtual async Task DeleteAsync(string? id, string? partitionKey)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (partitionKey is null) throw new ArgumentNullException(nameof(partitionKey));
        logger?.LogInformation("Deleting document (id: {id})", id);

        try
        {
            await container.DeleteItemAsync<TEntity>(id, new(partitionKey));
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to delete document", ex);
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        logger?.LogInformation("Getting documents");

        try
        {
            QueryDefinition queryDef = new("SELECT * FROM c");
            var query = container.GetItemQueryIterator<TEntity>(queryDef);
            List<TEntity> results = new();
            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response.ToList());
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to get documents", ex);
        }
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
    {
        logger?.LogInformation("Getting documents");

        try
        {
            var query = container.GetItemLinqQueryable<TEntity>().Where(predicate);
            List<TEntity> results = new();
            var iterator = query.ToFeedIterator();
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);
            }
            return results;
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to get documents", ex);
        }
    }

    public virtual async Task<TEntity?> GetByIdAsync(string? id, string? partitionKey)
    {
        if (id is null) throw new ArgumentNullException(nameof(id));
        if (partitionKey is null) throw new ArgumentNullException(nameof(partitionKey));
        logger?.LogInformation("Getting document with id: {id}", id);

        try
        {
            var response = await container.ReadItemAsync<TEntity>(id, new(partitionKey));
            return response.Resource;
        }
        catch (CosmosException cosEx) when (cosEx.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return default;
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to get document", ex);
        }
    }

    public virtual async Task UpdateAsync(TEntity? entity)
    {
        if (entity is null) throw new ArgumentNullException(nameof(entity));
        logger?.LogInformation("Updating document with id: {id}", entity.Id);

        try
        {
            await container.ReplaceItemAsync(entity, entity.Id);
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to update document", ex);
        }
    }
}