using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Interfaces;
using OnlineCourses.Infrastructure.Data;

namespace OnlineCourses.Infrastructure.Repositories
{
    public class LessonRepository : EfRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId)
        {
            return await _dbSet
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<bool> IsOrderUniqueAsync(Guid courseId, int order, Guid? excludeLessonId = null)
        {
            var query = _dbSet.Where(l => l.CourseId == courseId && l.Order == order);

            if (excludeLessonId.HasValue)
            {
                query = query.Where(l => l.Id != excludeLessonId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}
