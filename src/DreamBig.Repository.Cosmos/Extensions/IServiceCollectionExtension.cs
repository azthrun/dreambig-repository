using System.Reflection;
using System.Linq;
using DreamBig.Repository.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DreamBig.Repository.Cosmos.Extensions;

public static class IServiceCollectionExtension
{
    public static IServiceCollection UseCosmos(this IServiceCollection services, string? connectionString, string? databaseId, CosmosClientOptions? options = null)
    {
        services.AddSingleton<IProvider<CosmosClient, Database>>(provider => new BaseProvider(connectionString, databaseId, options, provider.GetService<ILogger<BaseProvider>>()));
        return services;
    }

    public static IServiceCollection UseCosmos(this IServiceCollection services, string? endpoint, string? primaryKey, string? databaseId, CosmosClientOptions? options = null)
    {
        services.AddSingleton<IProvider<CosmosClient, Database>>(provider => new BaseProvider(endpoint, primaryKey, databaseId, options, provider.GetService<ILogger<BaseProvider>>()));
        return services;
    }

    public static IServiceCollection AddCosmosRepositories(this IServiceCollection services)
    {
        var repositoryTypes = Assembly.GetAssembly(typeof(BaseRepository<>))?.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(BaseRepository<>)));
        if (repositoryTypes is null)
        {
            return services;
        }

        foreach (var repositoryType in repositoryTypes)
        {
            var entityType = repositoryType.BaseType?.GetGenericArguments()[0];
            if (entityType is null)
            {
                continue;
            }
            services.AddScoped(repositoryType);
            services.AddScoped(typeof(IRepository<>).MakeGenericType(entityType), provider => provider.GetRequiredService(repositoryType));
        }
        return services;
    }
}

