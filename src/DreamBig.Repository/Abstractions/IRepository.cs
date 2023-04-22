using System.Linq.Expressions;

namespace DreamBig.Repository.Abstractions;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task CreateAsync(TEntity? entity);
    Task DeleteAsync(string? id, string? partitionKey);   
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetByIdAsync(string? id, string? partitionKey);
    Task UpdateAsync(TEntity? entity);
}
