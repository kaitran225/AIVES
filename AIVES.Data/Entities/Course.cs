using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class Course
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<Question> Questions { get; set; } = new();
        public List<QuestionTopic> QuestionTopics { get; set; } = new();
        public List<ExamSession> ExamSessions { get; set; } = new();
        public List<User> Lecturers { get; set; } = new();
    }
}