using System;
using System.Collections.Generic;
using OnlineCourses.Domain.Enums;

namespace OnlineCourses.Domain.Entities
{
    public class Course : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public CourseStatus Status { get; set; } = CourseStatus.Draft;
        
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
