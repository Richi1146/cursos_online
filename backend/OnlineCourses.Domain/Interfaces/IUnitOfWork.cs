using System;
using System.Threading.Tasks;

namespace OnlineCourses.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        ILessonRepository Lessons { get; }
        Task<int> SaveChangesAsync();
    }
}
