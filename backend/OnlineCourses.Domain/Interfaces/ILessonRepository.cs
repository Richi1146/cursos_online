using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OnlineCourses.Domain.Entities;

namespace OnlineCourses.Domain.Interfaces
{
    public interface ILessonRepository : IRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByCourseIdAsync(Guid courseId);
        Task<bool> IsOrderUniqueAsync(Guid courseId, int order, Guid? excludeLessonId = null);
    }
}
