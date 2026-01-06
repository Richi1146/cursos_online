using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Api.Controllers
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        /// <summary>
        /// Searches for courses with optional filtering and pagination.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string? q,
            [FromQuery] CourseStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _courseService.GetCoursesAsync(q, status, page, pageSize);
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a course by its unique identifier.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _courseService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        /// <summary>
        /// Gets a summary of a course, including lesson count.
        /// </summary>
        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(Guid id)
        {
            var result = await _courseService.GetSummaryAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new course in draft status.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var result = await _courseService.CreateAsync(dto);
            if (result.Data == null) return BadRequest("Failed to create course data.");
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() }, result.Data);
        }

        /// <summary>
        /// Updates an existing course's information.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
        {
            var result = await _courseService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        /// <summary>
        /// Soft deletes a course.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _courseService.DeleteAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return NoContent();
        }

        /// <summary>
        /// Publishes a course, making it visible to users. Requires at least one lesson.
        /// </summary>
        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var result = await _courseService.PublishAsync(id);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(new { message = "Course published successfully." });
        }

        /// <summary>
        /// Unpublishes a course, returning it to draft status.
        /// </summary>
        [HttpPatch("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            var result = await _courseService.UnpublishAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(new { message = "Course unpublished successfully." });
        }
    }
}
