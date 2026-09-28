using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ReferenceAnswer { get; set; }
        public string BloomLevel { get; set; } = "Remember"; // Remember, Understand, Apply, Analyze
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string SourceType { get; set; } = "Manual"; // Manual, AI, Imported
        public string? ReferenceMaterial { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int? TopicId { get; set; }
        public QuestionTopic? Topic { get; set; }

        public int? RubricId { get; set; }
        public QuestionRubric? Rubric { get; set; }

        public List<ExamQuestion> ExamQuestions { get; set; } = new();
    }
}