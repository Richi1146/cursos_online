using System;
using System.Threading.Tasks;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Application.Interfaces
{
    public interface ICourseService
    {
        Task<ApiResult<PagedResult<CourseDto>>> GetCoursesAsync(string? search, CourseStatus? status, int page, int pageSize);
        Task<ApiResult<CourseDto>> GetByIdAsync(Guid id);
        Task<ApiResult<CourseSummaryDto>> GetSummaryAsync(Guid id);
        Task<ApiResult<CourseDto>> CreateAsync(CreateCourseDto dto);
        Task<ApiResult<CourseDto>> UpdateAsync(Guid id, UpdateCourseDto dto);
        Task<ApiResult<bool>> DeleteAsync(Guid id);
        Task<ApiResult<bool>> PublishAsync(Guid id);
        Task<ApiResult<bool>> UnpublishAsync(Guid id);
    }

    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
