using AIVES.Business.DTOs;

namespace AIVES.Business.Interfaces;

/// <summary>
/// Single source of truth for courses and their topics.
/// Every course/topic dropdown in the web layer is fed from here.
/// </summary>
public interface ICourseService
{
    Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
    Task<CourseDto?> GetCourseByIdAsync(int id);
    Task<CourseDto> CreateCourseAsync(CourseCreateDto dto);
    Task<CourseDto> UpdateCourseAsync(CourseUpdateDto dto);
    Task<CourseDeleteResultDto> DeleteCourseAsync(int id);

    Task<IEnumerable<QuestionTopicDto>> GetAllTopicsAsync();
    Task<IEnumerable<QuestionTopicDto>> GetTopicsByCourseAsync(int courseId);
    Task<QuestionTopicDto?> GetTopicByIdAsync(int id);
    Task<QuestionTopicDto> CreateTopicAsync(QuestionTopicCreateDto dto);
    Task<QuestionTopicDto> UpdateTopicAsync(QuestionTopicUpdateDto dto);
    Task<TopicDeleteResultDto> DeleteTopicAsync(int id);
}
