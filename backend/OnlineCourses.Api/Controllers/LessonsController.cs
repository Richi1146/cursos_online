using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineCourses.Application.DTOs;
using OnlineCourses.Application.Interfaces;

namespace OnlineCourses.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonsController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(Guid courseId)
        {
            var result = await _lessonService.GetByCourseIdAsync(courseId);
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLessonDto dto)
        {
            var result = await _lessonService.CreateAsync(dto);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonDto dto)
        {
            var result = await _lessonService.UpdateAsync(id, dto);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _lessonService.DeleteAsync(id);
            if (!result.Success) return NotFound(result.ErrorMessage);
            return NoContent();
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> Reorder(Guid courseId, [FromBody] List<ReorderLessonDto> newOrders)
        {
            // Note: courseId in query or Body? Requirement says list of inputs.
            // I added courseId to method arg, often passed in query or route, but here let's assume body contains IDs.
            // Actually, validating courseId helps safety.
            var result = await _lessonService.ReorderAsync(courseId, newOrders);
            if (!result.Success) return BadRequest(new { message = result.ErrorMessage });
            return Ok(new { message = "Lessons reordered successfully." });
        }
    }
}
