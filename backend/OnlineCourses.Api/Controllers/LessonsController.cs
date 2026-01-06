using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;

namespace OnlineCourses.Api.Controllers
{
    [ApiVersion("1.0")]
    [Authorize]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        /// <summary>
        /// Gets all lessons for a specific course.
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(Guid courseId)
        {
            var result = await _lessonService.GetByCourseIdAsync(courseId);
            return Ok(result.Data);
        }

        /// <summary>
        /// Creates a new lesson for a course.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            var result = await _lessonService.CreateAsync(dto);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }

        /// <summary>
        /// Updates an existing lesson's information.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto dto)
        {
            var result = await _lessonService.UpdateAsync(id, dto);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }

        /// <summary>
        /// Soft deletes a lesson.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _lessonService.DeleteAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return NoContent();
        }

        /// <summary>
        /// Reorders lessons within a course.
        /// </summary>
        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder(Guid courseId, [FromBody] List<ReorderLessonDto> newOrders)
        {
            var result = await _lessonService.ReorderAsync(courseId, newOrders);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(new { message = "Lessons reordered successfully." });
        }
    }
}
