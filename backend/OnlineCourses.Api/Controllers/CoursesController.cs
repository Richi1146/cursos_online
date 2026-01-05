using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _courseService.GetByIdAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpGet("{id}/summary")]
        public async Task<IActionResult> GetSummary(Guid id)
        {
            var result = await _courseService.GetSummaryAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var result = await _courseService.CreateAsync(dto);
            if (result.Data == null) return BadRequest("Failed to create course data.");
            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto)
        {
            var result = await _courseService.UpdateAsync(id, dto);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _courseService.DeleteAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return NoContent();
        }

        [HttpPatch("{id}/publish")]
        public async Task<IActionResult> Publish(Guid id)
        {
            var result = await _courseService.PublishAsync(id);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage }); // Business rule failed
            return Ok(new { message = "Course published successfully." });
        }

        [HttpPatch("{id}/unpublish")]
        public async Task<IActionResult> Unpublish(Guid id)
        {
            var result = await _courseService.UnpublishAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return Ok(new { message = "Course unpublished successfully." });
        }
    }
}
