using Microsoft.Extensions.DependencyInjection;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Application.Services;

namespace OnlineCourses.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ICourseService, CourseService>();
            services.AddScoped<ILessonService, LessonService>();
            // AuthService moved to Infrastructure - Registered there

            return services;
        }
    }
}
