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
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<ILessonRepository> _lessonRepoMock;
        private readonly CourseService _courseService;
        private readonly LessonService _lessonService;

        public BusinessRuleTests()
        {
            _uowMock = new Mock<IUnitOfWork>();
            _courseRepoMock = new Mock<ICourseRepository>();
            _lessonRepoMock = new Mock<ILessonRepository>();

            _uowMock.Setup(u => u.Courses).Returns(_courseRepoMock.Object);
            _uowMock.Setup(u => u.Lessons).Returns(_lessonRepoMock.Object);

            _courseService = new CourseService(_uowMock.Object);
            _lessonService = new LessonService(_uowMock.Object);
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
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
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
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
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
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
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
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Never);
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
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UnpublishCourse_ShouldSucceed()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course { Id = courseId, Status = CourseStatus.Published };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

            // Act
            var result = await _courseService.UnpublishAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(CourseStatus.Draft, course.Status);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetCourseSummary_ShouldReturnCorrectCount()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course = new Course { Id = courseId, Title = "Test Course" };
            var lessons = new List<Lesson> { new Lesson(), new Lesson() };

            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _lessonRepoMock.Setup(r => r.GetByCourseIdAsync(courseId)).ReturnsAsync(lessons);

            // Act
            var result = await _courseService.GetSummaryAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data.TotalLessons);
        }

        [Fact]
        public async Task ReorderLessons_WithDuplicates_ShouldFail()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var newOrders = new List<ReorderLessonDto>
            {
                new ReorderLessonDto { LessonId = Guid.NewGuid(), NewOrder = 1 },
                new ReorderLessonDto { LessonId = Guid.NewGuid(), NewOrder = 1 } // Duplicate
            };

            // Act
            var result = await _lessonService.ReorderAsync(courseId, newOrders);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Duplicate order", result.ErrorMessage);
        }

        [Fact]
        public async Task DeleteLesson_ShouldBeSoftDelete()
        {
            // Arrange
            var lessonId = Guid.NewGuid();
            var lesson = new Lesson { Id = lessonId, IsDeleted = false };

            _lessonRepoMock.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(lesson);

            // Act
            var result = await _lessonService.DeleteAsync(lessonId);

            // Assert
            Assert.True(result.Success);
            Assert.True(lesson.IsDeleted);
            _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateCourse_ShouldStartAsDraft()
        {
            // Arrange
            var dto = new CreateCourseDto { Title = "New Course" };
            _courseRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new Course { Status = CourseStatus.Draft });

            // Act
            var result = await _courseService.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(CourseStatus.Draft, result.Data.Status);
        }
    }
}
