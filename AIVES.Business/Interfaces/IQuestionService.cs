using AIVES.Business.DTOs;

namespace AIVES.Business.Interfaces;

public interface IQuestionService
{
    Task<IEnumerable<QuestionDto>> GetAllQuestionsAsync();
    Task<QuestionDto?> GetQuestionByIdAsync(int id);
    Task<IEnumerable<QuestionDto>> GetQuestionsByCourseAsync(int courseId);
    Task<IEnumerable<QuestionDto>> GetQuestionsByTopicAsync(int? topicId);
    Task<IEnumerable<QuestionDto>> GetQuestionsByBloomLevelAsync(string bloomLevel);
    Task<IEnumerable<QuestionDto>> GetPendingQuestionsAsync();
    Task<QuestionDto> CreateQuestionAsync(QuestionCreateDto dto);
    Task<QuestionDto> UpdateQuestionAsync(QuestionUpdateDto dto);
    Task DeleteQuestionAsync(int id);
    Task<QuestionDto> ReviewQuestionAsync(QuestionReviewDto dto);
    Task<List<QuestionDto>> ImportQuestionsAsync(string content, int courseId, string delimiter = "\n");
    Task<AIGeneratedQuestionDto> GenerateQuestionFromMaterialAsync(string courseCode, string topic, string bloomLevel, string? rubricDescription = null);
}

