using System.Collections.Generic;
using System.Threading.Tasks;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Domain.Interfaces
{
    public interface ICourseRepository : IRepository<Course>
    {
        Task<(IEnumerable<Course> Items, int TotalCount)> GetPagedAsync(
            string? searchTerm,
            CourseStatus? statusFilter,
            int page,
            int pageSize
        );
    }
}
