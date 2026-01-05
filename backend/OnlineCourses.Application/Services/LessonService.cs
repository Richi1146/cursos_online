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
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;

        public LessonService(ILessonRepository lessonRepository, ICourseRepository courseRepository)
        {
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
        }

        public async Task<ApiResult<IEnumerable<LessonDto>>> GetByCourseIdAsync(Guid courseId)
        {
            var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
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
            var course = await _courseRepository.GetByIdAsync(dto.CourseId);
            if (course == null) return ApiResult<LessonDto>.Fail("Course not found");

            if (!await _lessonRepository.IsOrderUniqueAsync(dto.CourseId, dto.Order))
            {
                return ApiResult<LessonDto>.Fail($"A lesson with order {dto.Order} already exists in this course.");
            }

            var lesson = new Lesson
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                Order = dto.Order
            };

            await _lessonRepository.AddAsync(lesson);

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
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null) return ApiResult<LessonDto>.Fail("Lesson not found");

            if (!await _lessonRepository.IsOrderUniqueAsync(lesson.CourseId, dto.Order, excludeLessonId: id))
            {
                return ApiResult<LessonDto>.Fail($"A lesson with order {dto.Order} already exists in this course.");
            }

            lesson.Title = dto.Title;
            lesson.Order = dto.Order;

            await _lessonRepository.UpdateAsync(lesson);

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
            var lesson = await _lessonRepository.GetByIdAsync(id);
            if (lesson == null) return ApiResult<bool>.Fail("Lesson not found");

            lesson.IsDeleted = true;
            await _lessonRepository.UpdateAsync(lesson);
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

            var lessons = (await _lessonRepository.GetByCourseIdAsync(courseId)).ToList();

            // Check if all provided IDs exist in the course
            var lessonIds = lessons.Select(l => l.Id).ToHashSet();
            if (newOrders.Any(o => !lessonIds.Contains(o.LessonId)))
            {
                return ApiResult<bool>.Fail("One or more lessons do not belong to this course.");
            }

            // Check if new orders invoke any collision with existing lessons NOT in the list?
            // The requirement says "reordenar". Usually updates all or a subset.
            // If we update a subset, we must ensure collisions are handled.
            // Simplest safe way: update the lessons in memory, check for collisions among ALL active lessons of the course.

            // Apply changes in memory
            foreach (var item in newOrders)
            {
                var lesson = lessons.First(l => l.Id == item.LessonId);
                lesson.Order = item.NewOrder;
                // Mark for update?
            }

            // Validate uniqueness across ALL lessons of the course
            var orders = lessons.Select(l => l.Order).ToList();
            if (orders.Distinct().Count() != orders.Count)
            {
                return ApiResult<bool>.Fail("The resulting orders would contain duplicates.");
            }

            // apply updates
            foreach (var lesson in lessons)
            {
                // We just call generic repository Update which marks state Modified
                await _lessonRepository.UpdateAsync(lesson);
            }

            return ApiResult<bool>.Ok(true);
        }
    }
}
