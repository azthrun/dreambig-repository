using DreamBig.Repository.Abstractions;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

    public static IServiceCollection AddCosmosRepositories(this IServiceCollection services, Assembly assembly)
    {
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRepository<>)));
        var nonBaseRepositories = repositoryTypes.Where(t => t != typeof(BaseRepository<>));
        if (repositoryTypes is null)
        {
            return services;
        }

        foreach (var repositoryType in repositoryTypes)
        {
            var interfaces = repositoryType.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>))
                .ToList();
            if (interfaces.Count != 1)
            {
                continue;
            }
            services.AddScoped(interfaces[0], repositoryType);
        }
        return services;
    }
}

