using AIVES.Business.DTOs;
using AIVES.Data.Entities;

namespace AIVES.Business.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> CreateAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
