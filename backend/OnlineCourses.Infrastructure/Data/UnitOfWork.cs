using System.Threading.Tasks;
using OnlineCourses.Domain.Interfaces;
using OnlineCourses.Infrastructure.Data;

namespace OnlineCourses.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public ICourseRepository Courses { get; }
        public ILessonRepository Lessons { get; }

        public UnitOfWork(AppDbContext context, ICourseRepository courses, ILessonRepository lessons)
        {
            _context = context;
            Courses = courses;
            Lessons = lessons;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
