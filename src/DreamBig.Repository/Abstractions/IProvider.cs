namespace DreamBig.Repository.Abstractions;

public interface IProvider<TDataClient, TDatabase>
{
    TDataClient? GetClient();
    Task<TDatabase?> GetDatabaseAsync();
}