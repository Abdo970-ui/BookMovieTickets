using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace BookMovieTickets.Repositories
{

    public interface IRepository<T> where T : class
    {
        Task<EntityEntry<T>> AddAsync(T entity);
        Task<IEnumerable<T>> GetAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true
            );
        Task<T?> GetOneAsync(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, object>>[]? includes = null,
            bool tracked = true
            );
        void Update(T entity);
        void Delete(T entity);
        Task<int> CommitAsync();
    }
}
