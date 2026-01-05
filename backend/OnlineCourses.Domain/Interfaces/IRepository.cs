using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Domain.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity); // Soft delete handled by service or impl
        Task SaveChangesAsync();
    }
}
