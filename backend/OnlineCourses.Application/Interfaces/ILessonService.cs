using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OnlineCourses.Application.DTOs;

namespace OnlineCourses.Application.Interfaces
{
    public interface ILessonService
    {
        Task<ApiResult<IEnumerable<LessonDto>>> GetByCourseIdAsync(Guid courseId);
        Task<ApiResult<LessonDto>> CreateAsync(CreateLessonDto dto);
        Task<ApiResult<LessonDto>> UpdateAsync(Guid id, UpdateLessonDto dto);
        Task<ApiResult<bool>> DeleteAsync(Guid id);
        Task<ApiResult<bool>> ReorderAsync(Guid courseId, List<ReorderLessonDto> newOrders);
    }
}
