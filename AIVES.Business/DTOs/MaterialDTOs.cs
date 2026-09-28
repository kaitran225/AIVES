using Microsoft.AspNetCore.Http;

namespace AIVES.Business.DTOs
{
    public class CourseMaterialCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string SourceType { get; set; } = "TextPaste";
        public string? FileName { get; set; }
        public IFormFile? FileUpload { get; set; }
    }

    public class CourseMaterialDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string SourceType { get; set; } = "TextPaste";
        public string? FileName { get; set; }
        public int ChunkCount { get; set; }
        public DateTime ImportedAt { get; set; }
    }

    public class AIGeneratedQuestionDto
    {
        public string GeneratedQuestion { get; set; } = string.Empty;
        public string? ReferenceAnswer { get; set; }
        public string BloomLevel { get; set; } = "Remember";
        public string? CourseCode { get; set; }
        public string? Topic { get; set; }
    }

    public class MaterialImportResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int MaterialId { get; set; }
        public int ChunkCount { get; set; }
    }
}
