using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Domain.Interfaces;
using OnlineCourses.Infrastructure.Data;

namespace OnlineCourses.Infrastructure.Repositories
{
    public class CourseRepository : EfRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<(IEnumerable<Course> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            CourseStatus? statusFilter,
            int page,
            int pageSize)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(c => c.Title.Contains(searchTerm));
            }

            if (statusFilter.HasValue)
            {
                query = query.Where(c => c.Status == statusFilter.Value);
            }

            // Order by most recent by default
            query = query.OrderByDescending(c => c.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }
    }
}
