using DreamBig.Repository.Abstractions;
using DreamBig.Repository.Exceptions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DreamBig.Repository.Cosmos;

public class BaseProvider : IProvider<CosmosClient, Database>
{
    private readonly CosmosClient cosmosClient;
    private string? databaseId;
    private readonly ILogger<BaseProvider>? logger;

    public BaseProvider(string? connectionString, string? databaseId, CosmosClientOptions? clientOptions = null, ILogger<BaseProvider>? logger = null)
    {
        if (connectionString is null) throw new ArgumentNullException(nameof(connectionString));
        if (databaseId is null) throw new ArgumentNullException(nameof(databaseId));

        this.databaseId = databaseId;
        this.logger = logger;

        try
        {
            cosmosClient = new(connectionString, clientOptions);
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to initialize Cosmos Client", ex);
        }
    }

    public BaseProvider(string? endpoint, string? primaryKey, string? databaseId, CosmosClientOptions? clientOptions = null, ILogger<BaseProvider>? logger = null)
    {
        if (endpoint is null) throw new ArgumentNullException(nameof(endpoint));
        if (primaryKey is null) throw new ArgumentNullException(nameof(primaryKey));
        if (databaseId is null) throw new ArgumentNullException(nameof(databaseId));

        this.databaseId = databaseId;
        this.logger = logger;

        try
        {
            cosmosClient = new(endpoint, primaryKey, clientOptions);
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Failed to initialize Cosmos Client", ex);
        }
    }

    public virtual CosmosClient GetClient() => cosmosClient;

    public virtual async Task<Database?> GetDatabaseAsync()
    {
        var response = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId).ConfigureAwait(false);
        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            logger?.LogInformation("Database ({databaseId}) created", databaseId);
            return response.Database;
        }
        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            logger?.LogInformation("Database ({databaseId}) returned", databaseId);
            return response.Database;
        }
        throw new RepositoryException($"Failed to retrieve database ({databaseId}). Status Code: {response.StatusCode}");
    }
}
