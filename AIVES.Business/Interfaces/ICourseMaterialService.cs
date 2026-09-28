using AIVES.Business.DTOs;
using AIVES.Data.Entities;

namespace AIVES.Business.Interfaces
{
    public interface ICourseMaterialService
    {
        Task<IEnumerable<CourseMaterialDto>> GetAllMaterialsAsync();
        Task<IEnumerable<CourseMaterialDto>> GetMaterialsByCourseAsync(int courseId);
        Task<CourseMaterialDto?> GetMaterialByIdAsync(int id);
        Task<MaterialImportResultDto> ImportMaterialAsync(CourseMaterialCreateDto dto);
        Task<CourseMaterialDto> UpdateMaterialAsync(int id, string title, string content);
        Task DeleteMaterialAsync(int id);
        Task<string> RetrieveRelevantChunksAsync(string courseCode, string query, int maxChunks = 5);
    }
}
