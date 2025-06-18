using System.Linq.Expressions;

namespace ThreeTP.Payment.Application.Interfaces;

public interface IGenericRepository<T> where  T:class
{
   
    Task<T?> GetOneAsync(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[]? includes);
    Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, params Expression<Func<T, object>>[]? includes);
    Task AddAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    
}