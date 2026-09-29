using AIVES.Business.DTOs;
using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Business.Services
{
    /// <summary>
    /// Manages questions: CRUD, import, review, and AI generation.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        private readonly AIVESDbContext _context;
        private readonly IAIInterviewService _aiInterviewService;

        public QuestionService(AIVESDbContext context, IAIInterviewService aiInterviewService)
        {
            _context = context;
            _aiInterviewService = aiInterviewService;
        }

        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync()
        {
            var questions = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        public async Task<QuestionDto?> GetQuestionByIdAsync(int id)
        {
            var question = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .FirstOrDefaultAsync(q => q.Id == id);

            return question == null ? null : MapToDto(question);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByCourseAsync(int courseId)
        {
            var questions = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .Where(q => q.CourseId == courseId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByTopicAsync(int? topicId)
        {
            var questions = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .Where(q => topicId == null || q.TopicId == topicId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        public async Task<IEnumerable<QuestionDto>> GetQuestionsByBloomLevelAsync(string bloomLevel)
        {
            var questions = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .Where(q => q.BloomLevel.Equals(bloomLevel, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        public async Task<IEnumerable<QuestionDto>> GetPendingQuestionsAsync()
        {
            var questions = await _context.Questions
                .Include(q => q.Course)
                .Include(q => q.Topic)
                .Include(q => q.Rubric)
                .Where(q => q.Status == "Pending")
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return questions.Select(MapToDto);
        }

        public async Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto)
        {
            var question = new Question
            {
                Text = dto.Text,
                ReferenceAnswer = dto.ReferenceAnswer,
                BloomLevel = dto.BloomLevel,
                SourceType = dto.SourceType,
                ReferenceMaterial = dto.ReferenceMaterial,
                CourseId = dto.CourseId,
                TopicId = dto.TopicId,
                RubricId = dto.RubricId,
                Status = dto.SourceType == "AI" ? "Pending" : "Approved",
                CreatedAt = DateTime.Now
            };

            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return MapToDto(question);
        }

        public async Task<QuestionDto> UpdateQuestionAsync(QuestionUpdateDto dto)
        {
            var question = await _context.Questions.FindAsync(dto.Id);
            if (question == null)
                throw new KeyNotFoundException($"Question with ID {dto.Id} not found.");

            question.Text = dto.Text;
            question.ReferenceAnswer = dto.ReferenceAnswer;
            question.BloomLevel = dto.BloomLevel;
            question.Status = dto.Status;
            question.ReferenceMaterial = dto.ReferenceMaterial;
            question.CourseId = dto.CourseId;
            question.TopicId = dto.TopicId;
            question.RubricId = dto.RubricId;

            await _context.SaveChangesAsync();
            return MapToDto(question);
        }

        public async Task DeleteQuestionAsync(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<QuestionDto> ReviewQuestionAsync(QuestionReviewDto dto)
        {
            var question = await _context.Questions.FindAsync(dto.QuestionId);
            if (question == null)
                throw new KeyNotFoundException($"Question with ID {dto.QuestionId} not found.");

            question.Status = dto.Status;
            // Store reviewer notes in ReferenceMaterial as audit trail
            if (!string.IsNullOrWhiteSpace(dto.ReviewerNotes))
            {
                question.ReferenceMaterial = $"Reviewed: {dto.Status} | Notes: {dto.ReviewerNotes} | Date: {DateTime.Now:yyyy-MM-dd HH:mm}";
            }

            await _context.SaveChangesAsync();
            return MapToDto(question);
        }

        public async Task<List<QuestionDto>> ImportQuestionsAsync(string content, int courseId, string delimiter = "\n")
        {
            var questions = new List<QuestionDto>();
            var lines = content.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var question = new Question
                {
                    Text = line.Trim(),
                    BloomLevel = "Remember",
                    SourceType = "Imported",
                    CourseId = courseId,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Questions.Add(question);
                questions.Add(MapToDto(question));
            }

            await _context.SaveChangesAsync();
            return questions;
        }

        public async Task<AIGeneratedQuestionDto> GenerateQuestionFromMaterialAsync(string courseCode, string topic, string bloomLevel, string? rubricDescription = null)
        {
            var generatedQuestion = await _aiInterviewService.GenerateQuestionFromMaterialAsync(courseCode, topic, bloomLevel, rubricDescription);

            return new AIGeneratedQuestionDto
            {
                GeneratedQuestion = generatedQuestion,
                CourseCode = courseCode,
                Topic = topic,
                BloomLevel = bloomLevel
            };
        }

        private QuestionDto MapToDto(Question question)
        {
            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                ReferenceAnswer = question.ReferenceAnswer,
                BloomLevel = question.BloomLevel,
                Status = question.Status,
                SourceType = question.SourceType,
                ReferenceMaterial = question.ReferenceMaterial,
                CreatedAt = question.CreatedAt,
                CourseId = question.CourseId,
                CourseName = question.Course?.Name,
                TopicId = question.TopicId,
                TopicName = question.Topic?.Name,
                RubricId = question.RubricId,
                RubricName = question.Rubric?.Name
            };
        }
    }
}
