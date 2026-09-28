namespace AIVES.Business.DTOs
{
    public class QuestionCreateDto
    {
        public string Text { get; set; } = string.Empty;
        public string? ReferenceAnswer { get; set; }
        public string BloomLevel { get; set; } = "Remember";
        public string SourceType { get; set; } = "Manual";
        public string? ReferenceMaterial { get; set; }
        public int CourseId { get; set; }
        public int? TopicId { get; set; }
        public int? RubricId { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ReferenceAnswer { get; set; }
        public string BloomLevel { get; set; } = "Remember";
        public string Status { get; set; } = "Pending";
        public string? ReferenceMaterial { get; set; }
        public int CourseId { get; set; }
        public int? TopicId { get; set; }
        public int? RubricId { get; set; }
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? ReferenceAnswer { get; set; }
        public string BloomLevel { get; set; } = "Remember";
        public string Status { get; set; } = "Pending";
        public string SourceType { get; set; } = "Manual";
        public string? ReferenceMaterial { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public int? TopicId { get; set; }
        public string? TopicName { get; set; }
        public int? RubricId { get; set; }
        public string? RubricName { get; set; }
    }

    public class QuestionReviewDto
    {
        public int QuestionId { get; set; }
        public string Status { get; set; } = "Approved";
        public string? ReviewerNotes { get; set; }
    }
}
