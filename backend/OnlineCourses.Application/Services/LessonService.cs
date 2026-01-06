using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Interfaces;

namespace OnlineCourses.Application.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LessonService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResult<IEnumerable<LessonDto>>> GetByCourseIdAsync(Guid courseId)
        {
            var lessons = await _unitOfWork.Lessons.GetByCourseIdAsync(courseId);
            var dtos = lessons.Select(l => new LessonDto
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Order = l.Order,
                CreatedAt = l.CreatedAt
            });
            return ApiResult<IEnumerable<LessonDto>>.Ok(dtos);
        }

        public async Task<ApiResult<LessonDto>> CreateAsync(CreateLessonDto dto)
        {
            var course = await _unitOfWork.Courses.GetByIdAsync(dto.CourseId);
            if (course == null) return ApiResult<LessonDto>.Fail("Course not found");

            if (!await _unitOfWork.Lessons.IsOrderUniqueAsync(dto.CourseId, dto.Order))
            {
                return ApiResult<LessonDto>.Fail($"A lesson with order {dto.Order} already exists in this course.");
            }

            var lesson = new Lesson
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Order = dto.Order
            };

            await _unitOfWork.Lessons.AddAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<LessonDto>.Ok(new LessonDto
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                Title = lesson.Title,
                Order = lesson.Order,
                CreatedAt = lesson.CreatedAt
            });
        }

        public async Task<ApiResult<LessonDto>> UpdateAsync(Guid id, UpdateLessonDto dto)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
            if (lesson == null) return ApiResult<LessonDto>.Fail("Lesson not found");

            if (!await _unitOfWork.Lessons.IsOrderUniqueAsync(lesson.CourseId, dto.Order, excludeLessonId: id))
            {
                return ApiResult<LessonDto>.Fail($"A lesson with order {dto.Order} already exists in this course.");
            }

            lesson.Title = dto.Title;
            lesson.Order = dto.Order;

            await _unitOfWork.Lessons.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<LessonDto>.Ok(new LessonDto
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                Title = lesson.Title,
                Order = lesson.Order,
                CreatedAt = lesson.CreatedAt
            });
        }

        public async Task<ApiResult<bool>> DeleteAsync(Guid id)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdAsync(id);
            if (lesson == null) return ApiResult<bool>.Fail("Lesson not found");

            lesson.IsDeleted = true;
            await _unitOfWork.Lessons.UpdateAsync(lesson);
            await _unitOfWork.SaveChangesAsync();
            return ApiResult<bool>.Ok(true);
        }

        public async Task<ApiResult<bool>> ReorderAsync(Guid courseId, List<ReorderLessonDto> newOrders)
        {
            // Validate duplicates in input
            if (newOrders.GroupBy(x => x.NewOrder).Any(g => g.Count() > 1))
            {
                return ApiResult<bool>.Fail("Duplicate order values provided in the request.");
            }
            if (newOrders.GroupBy(x => x.LessonId).Any(g => g.Count() > 1))
            {
                return ApiResult<bool>.Fail("Duplicate lesson IDs provided in the request.");
            }

            var lessons = (await _unitOfWork.Lessons.GetByCourseIdAsync(courseId)).ToList();

            // Check if all provided IDs exist in the course
            var lessonIds = lessons.Select(l => l.Id).ToHashSet();
            if (newOrders.Any(o => !lessonIds.Contains(o.LessonId)))
            {
                return ApiResult<bool>.Fail("One or more lessons do not belong to this course.");
            }

            // Apply changes in memory
            foreach (var item in newOrders)
            {
                var lesson = lessons.First(l => l.Id == item.LessonId);
                lesson.Order = item.NewOrder;
            }

            // Validate uniqueness across ALL lessons of the course
            var orders = lessons.Select(l => l.Order).ToList();
            if (orders.Distinct().Count() != orders.Count)
            {
                return ApiResult<bool>.Fail("The resulting orders would contain duplicates.");
            }

            // To avoid temporary unique constraint violations in PostgreSQL, 
            // we first set all orders to a temporary negative range.
            var allLessonsInDb = (await _unitOfWork.Lessons.GetByCourseIdAsync(courseId)).ToList();
            for (int i = 0; i < allLessonsInDb.Count; i++)
            {
                allLessonsInDb[i].Order = -(i + 1);
                await _unitOfWork.Lessons.UpdateAsync(allLessonsInDb[i]);
            }
            await _unitOfWork.SaveChangesAsync();

            // Now apply the final orders
            foreach (var lesson in lessons)
            {
                await _unitOfWork.Lessons.UpdateAsync(lesson);
            }
            await _unitOfWork.SaveChangesAsync();

            return ApiResult<bool>.Ok(true);
        }
    }
}
