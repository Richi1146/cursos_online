using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Domain.Interfaces;

namespace OnlineCourses.Application.Services
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CourseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResult<PagedResult<CourseDto>>> GetCoursesAsync(string? search, CourseStatus? status, int page, int pageSize)
        {
            var (items, total) = await _unitOfWork.Courses.GetPagedAsync(search, status, page, pageSize);

            var dtos = items.Select(c => new CourseDto
            {
                Id = c.Id,
                Title = c.Title,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            });

            return ApiResult<PagedResult<CourseDto>>.Ok(new PagedResult<CourseDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        public async Task<ApiResult<CourseDto>> GetByIdAsync(Guid id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<CourseDto>.Fail("Course not found");

            return ApiResult<CourseDto>.Ok(new CourseDto
            {
                Id = course.Id,
                Title = course.Title,
                Status = course.Status,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt
            });
        }

        public async Task<ApiResult<CourseSummaryDto>> GetSummaryAsync(Guid id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<CourseSummaryDto>.Fail("Course not found");

            var lessons = await _unitOfWork.Lessons.GetByCourseIdAsync(id);

            return ApiResult<CourseSummaryDto>.Ok(new CourseSummaryDto
            {
                Id = course.Id,
                Title = course.Title,
                TotalLessons = lessons.Count(),
                LastModified = course.UpdatedAt > DateTime.MinValue ? course.UpdatedAt : course.CreatedAt
            });
        }

        public async Task<ApiResult<CourseDto>> CreateAsync(CreateCourseDto dto)
        {
            var course = new Course
            {
                Title = dto.Title,
                Status = CourseStatus.Draft
            };

            await _unitOfWork.Courses.AddAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(course.Id);
        }

        public async Task<ApiResult<CourseDto>> UpdateAsync(Guid id, UpdateCourseDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<CourseDto>.Fail("Course not found");

            course.Title = dto.Title;
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task<ApiResult<bool>> DeleteAsync(Guid id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<bool>.Fail("Course not found");

            // Soft Delete logic in Business Layer
            course.IsDeleted = true;
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<bool>.Ok(true);
        }

        public async Task<ApiResult<bool>> PublishAsync(Guid id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<bool>.Fail("Course not found");

            var lessons = await _unitOfWork.Lessons.GetByCourseIdAsync(id);
            if (!lessons.Any())
            {
                return ApiResult<bool>.Fail("Cannot publish a course with no lessons.");
            }

            course.Status = CourseStatus.Published;
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<bool>.Ok(true);
        }

        public async Task<ApiResult<bool>> UnpublishAsync(Guid id)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(id);
            if (course == null) return ApiResult<bool>.Fail("Course not found");

            course.Status = CourseStatus.Draft;
            await _unitOfWork.Courses.UpdateAsync(course);
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<bool>.Ok(true);
        }
    }
}
