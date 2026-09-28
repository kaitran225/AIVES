using AIVES.Business.DTOs;
using AIVES.Data.Entities;

namespace AIVES.Business.Interfaces
{
    public interface IRubricService
    {
        Task<IEnumerable<RubricDto>> GetAllRubricsAsync();
        Task<RubricDto?> GetRubricByIdAsync(int id);
        Task<RubricDto> CreateRubricAsync(RubricCreateDto dto);
        Task<RubricDto> UpdateRubricAsync(RubricUpdateDto dto);
        Task DeleteRubricAsync(int id);
        Task<IEnumerable<RubricDto>> GetRubricsByCourseAsync(int courseId);
    }
}
