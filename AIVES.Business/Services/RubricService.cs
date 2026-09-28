using AIVES.Business.DTOs;
using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Business.Services
{
    /// <summary>
    /// Manages grading rubrics and their criteria.
    /// </summary>
    public class RubricService : IRubricService
    {
        private readonly AIVESDbContext _context;

        public RubricService(AIVESDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<RubricDto>> GetAllRubricsAsync()
        {
            var rubrics = await _context.QuestionRubrics
                .Include(r => r.Criteria)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return rubrics.Select(MapToDto);
        }

        public async Task<RubricDto?> GetRubricByIdAsync(int id)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.Criteria)
                .FirstOrDefaultAsync(r => r.Id == id);

            return rubric == null ? null : MapToDto(rubric);
        }

        public async Task<RubricDto> CreateRubricAsync(RubricCreateDto dto)
        {
            var rubric = new QuestionRubric
            {
                Name = dto.Name,
                Description = dto.Description,
                MaxScore = dto.MaxScore,
                Criteria = dto.Criteria.Select(c => new RubricCriterion
                {
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList()
            };

            _context.QuestionRubrics.Add(rubric);
            await _context.SaveChangesAsync();

            return MapToDto(rubric);
        }

        public async Task<RubricDto> UpdateRubricAsync(RubricUpdateDto dto)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.Criteria)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (rubric == null)
                throw new KeyNotFoundException($"Rubric with ID {dto.Id} not found.");

            rubric.Name = dto.Name;
            rubric.Description = dto.Description;
            rubric.MaxScore = dto.MaxScore;

            // Update or create criteria
            var existingCriteria = rubric.Criteria.ToList();
            foreach (var criterionDto in dto.Criteria)
            {
                if (criterionDto.Id > 0)
                {
                    var existing = existingCriteria.FirstOrDefault(c => c.Id == criterionDto.Id);
                    if (existing != null)
                    {
                        existing.Criterion = criterionDto.Criterion;
                        existing.Description = criterionDto.Description;
                        existing.MaxScore = criterionDto.MaxScore;
                        existing.ScoringGuidance = criterionDto.ScoringGuidance;
                    }
                }
                else
                {
                    rubric.Criteria.Add(new RubricCriterion
                    {
                        Criterion = criterionDto.Criterion,
                        Description = criterionDto.Description,
                        MaxScore = criterionDto.MaxScore,
                        ScoringGuidance = criterionDto.ScoringGuidance
                    });
                }
            }

            // Remove deleted criteria
            var dtoIds = dto.Criteria.Select(c => c.Id).ToHashSet();
            var toRemove = existingCriteria.Where(c => !dtoIds.Contains(c.Id)).ToList();
            _context.RubricCriteria.RemoveRange(toRemove);

            await _context.SaveChangesAsync();
            return MapToDto(rubric);
        }

        public async Task DeleteRubricAsync(int id)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.Criteria)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rubric != null)
            {
                _context.RubricCriteria.RemoveRange(rubric.Criteria);
                _context.QuestionRubrics.Remove(rubric);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<RubricDto>> GetRubricsByCourseAsync(int courseId)
        {
            // Rubrics are not directly tied to courses; return all
            return await GetAllRubricsAsync();
        }

        private RubricDto MapToDto(QuestionRubric rubric)
        {
            return new RubricDto
            {
                Id = rubric.Id,
                Name = rubric.Name,
                Description = rubric.Description,
                MaxScore = rubric.MaxScore,
                Criteria = rubric.Criteria.Select(c => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    Description = c.Description,
                    MaxScore = c.MaxScore,
                    ScoringGuidance = c.ScoringGuidance
                }).ToList()
            };
        }
    }
}
