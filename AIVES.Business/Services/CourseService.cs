using AIVES.Business.DTOs;
using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Business.Services
{
    /// <summary>
    /// Manages the course catalogue and the topics that belong to each course.
    /// Courses and topics are created/edited here only, so every dropdown in the
    /// web layer reads the same data.
    /// </summary>
    public class CourseService : ICourseService
    {
        private readonly AIVESDbContext _context;

        public CourseService(AIVESDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses
                .OrderBy(c => c.Code)
                .ToListAsync();

            return courses.Select(MapToDto);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.QuestionTopics)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course == null ? null : MapToDto(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CourseCreateDto dto)
        {
            var code = dto.Code.Trim();
            var duplicate = await _context.Courses
                .FirstOrDefaultAsync(c => c.Code.ToLower() == code.ToLower());

            if (duplicate != null)
                throw new InvalidOperationException($"A course with code '{code}' already exists.");

            var course = new Course
            {
                Code = code,
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            foreach (var topicName in ParseTopicLines(dto.InitialTopics))
            {
                course.QuestionTopics.Add(new QuestionTopic { Name = topicName });
            }

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return MapToDto(course);
        }

        public async Task<CourseDto> UpdateCourseAsync(CourseUpdateDto dto)
        {
            var course = await _context.Courses.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Course with ID {dto.Id} not found.");

            var code = dto.Code.Trim();
            var duplicate = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id != dto.Id && c.Code.ToLower() == code.ToLower());

            if (duplicate != null)
                throw new InvalidOperationException($"A course with code '{code}' already exists.");

            course.Code = code;
            course.Name = dto.Name.Trim();
            course.Description = dto.Description;

            await _context.SaveChangesAsync();
            return MapToDto(course);
        }

        public async Task<CourseDeleteResultDto> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return new CourseDeleteResultDto { Success = false, Message = "Course not found." };

            var blockers = new List<string>();

            var questionCount = await _context.Questions.CountAsync(q => q.CourseId == id);
            if (questionCount > 0)
                blockers.Add($"{questionCount} question(s)");

            var materialCount = await _context.CourseMaterials.CountAsync(m => m.CourseId == id);
            if (materialCount > 0)
                blockers.Add($"{materialCount} material(s)");

            var sessionCount = await _context.ExamSessions.CountAsync(s => s.CourseId == id);
            if (sessionCount > 0)
                blockers.Add($"{sessionCount} exam session(s)");

            if (blockers.Count > 0)
            {
                return new CourseDeleteResultDto
                {
                    Success = false,
                    Message = "This course is still referenced by other records and cannot be deleted.",
                    Blockers = blockers
                };
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return new CourseDeleteResultDto { Success = true, Message = "Course deleted." };
        }

        public async Task<IEnumerable<QuestionTopicDto>> GetAllTopicsAsync()
        {
            var topics = await _context.QuestionTopics
                .Include(t => t.Course)
                .OrderBy(t => t.Course!.Code)
                .ThenBy(t => t.Name)
                .ToListAsync();

            return topics.Select(MapToTopicDto);
        }

        public async Task<IEnumerable<QuestionTopicDto>> GetTopicsByCourseAsync(int courseId)
        {
            var topics = await _context.QuestionTopics
                .Where(t => t.CourseId == courseId)
                .OrderBy(t => t.Name)
                .ToListAsync();

            return topics.Select(MapToTopicDto);
        }

        public async Task<QuestionTopicDto?> GetTopicByIdAsync(int id)
        {
            var topic = await _context.QuestionTopics
                .Include(t => t.Course)
                .FirstOrDefaultAsync(t => t.Id == id);

            return topic == null ? null : MapToTopicDto(topic);
        }

        public async Task<QuestionTopicDto> CreateTopicAsync(QuestionTopicCreateDto dto)
        {
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == dto.CourseId);
            if (!courseExists)
                throw new InvalidOperationException("The selected course does not exist.");

            var name = dto.Name.Trim();
            var duplicate = await _context.QuestionTopics
                .FirstOrDefaultAsync(t => t.CourseId == dto.CourseId && t.Name.ToLower() == name.ToLower());

            if (duplicate != null)
                throw new InvalidOperationException($"Topic '{name}' already exists for this course.");

            var topic = new QuestionTopic
            {
                Name = name,
                Description = dto.Description,
                CourseId = dto.CourseId
            };

            _context.QuestionTopics.Add(topic);
            await _context.SaveChangesAsync();

            return MapToTopicDto(topic);
        }

        public async Task<QuestionTopicDto> UpdateTopicAsync(QuestionTopicUpdateDto dto)
        {
            var topic = await _context.QuestionTopics.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException($"Topic with ID {dto.Id} not found.");

            var name = dto.Name.Trim();
            var duplicate = await _context.QuestionTopics
                .FirstOrDefaultAsync(t => t.Id != dto.Id && t.CourseId == dto.CourseId && t.Name.ToLower() == name.ToLower());

            if (duplicate != null)
                throw new InvalidOperationException($"Topic '{name}' already exists for this course.");

            topic.Name = name;
            topic.Description = dto.Description;
            topic.CourseId = dto.CourseId;

            await _context.SaveChangesAsync();
            return MapToTopicDto(topic);
        }

        public async Task<TopicDeleteResultDto> DeleteTopicAsync(int id)
        {
            var topic = await _context.QuestionTopics.FindAsync(id);
            if (topic == null)
                return new TopicDeleteResultDto { Success = false, Message = "Topic not found." };

            var questionCount = await _context.Questions.CountAsync(q => q.TopicId == id);
            if (questionCount > 0)
            {
                return new TopicDeleteResultDto
                {
                    Success = false,
                    Message = "This topic is still used by questions and cannot be deleted.",
                    Blockers = new List<string> { $"{questionCount} question(s)" }
                };
            }

            _context.QuestionTopics.Remove(topic);
            await _context.SaveChangesAsync();

            return new TopicDeleteResultDto { Success = true, Message = "Topic deleted." };
        }

        private static IEnumerable<string> ParseTopicLines(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return Enumerable.Empty<string>();

            return raw
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }

        private static CourseDto MapToDto(Course course) => new()
        {
            Id = course.Id,
            Code = course.Code,
            Name = course.Name,
            Description = course.Description
        };

        private static QuestionTopicDto MapToTopicDto(QuestionTopic topic) => new()
        {
            Id = topic.Id,
            Name = topic.Name,
            Description = topic.Description,
            CourseId = topic.CourseId
        };
    }
}
