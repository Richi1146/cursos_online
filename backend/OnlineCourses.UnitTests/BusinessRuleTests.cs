using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Services;
using OnlineCourses.Domain.Entities;
using OnlineCourses.Domain.Enums;
using OnlineCourses.Domain.Interfaces;
using Xunit;

namespace OnlineCourses.UnitTests
{
    public class BusinessRuleTests
    {
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<ILessonRepository> _lessonRepoMock;
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;

        public BusinessRuleTests()
        {
            _courseRepoMock = new Mock<ICourseRepository>();
            _lessonRepoMock = new Mock<ILessonRepository>();
            _courseService = new CourseService(_courseRepoMock.Object, _lessonRepoMock.Object);
            _lessonService = new LessonService(_lessonRepoMock.Object, _courseRepoMock.Object);
        }

        [Fact]
        public async Task PublishCourse_WithLessons_ShouldSucceed()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course { Id = courseId, Status = CourseStatus.Draft };
            var lessons = new List<Lesson> { new Lesson { Id = Guid.NewGuid(), CourseId = courseId } };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _lessonRepoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(lessons);

            // Act
            var result = await _courseService.PublishAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(CourseStatus.Published, course.Status);
            _courseRepoMock.Verify(r => r.UpdateAsync(course), Times.Once);
        }

        [Fact]
        public async Task PublishCourse_WithoutLessons_ShouldFail()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course { Id = courseId, Status = CourseStatus.Draft };
            var lessons = new List<Lesson>(); // No lessons

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _lessonRepoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(lessons);

            // Act
            var result = await _courseService.PublishAsync(courseId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(CourseStatus.Draft, course.Status);
            _courseRepoMock.Verify(r => r.UpdateAsync(course), Times.Never);
        }

        [Fact]
        public async Task CreateLesson_WithUniqueOrder_ShouldSucceed()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var dto = new CreateLessonDto { CourseId = courseId, Order = 1, Title = "Lesson 1" };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(new Course());
            _lessonRepoMock.Setup(r => r.IsOrderUniqueAsync(courseId, 1, null)).ReturnsAsync(true);

            // Act
            var result = await _lessonService.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            _lessonRepoMock.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Once);
        }

        [Fact]
        public async Task CreateLesson_WithDuplicateOrder_ShouldFail()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var dto = new CreateLessonDto { CourseId = courseId, Order = 1, Title = "Lesson 1" };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(new Course());
            _lessonRepoMock.Setup(r => r.IsOrderUniqueAsync(courseId, 1, null)).ReturnsAsync(false); // Not unique

            // Act
            var result = await _lessonService.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            _lessonRepoMock.Verify(r => r.AddAsync(It.IsAny<Lesson>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCourse_ShouldBeSoftDelete()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course { Id = courseId, IsDeleted = false };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

            // Act
            var result = await _courseService.DeleteAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.True(course.IsDeleted);
            _courseRepoMock.Verify(r => r.UpdateAsync(course), Times.Once); // Should update, not delete physically
        }
    }
}
