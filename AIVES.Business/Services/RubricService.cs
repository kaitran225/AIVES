using AIVES.Business.DTOs;
using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIVES.Business.Services
{
    /// <summary>
    /// Manages grading rubrics and their criteria (matrix structure).
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
                .Include(r => r.PerformanceLevels.OrderBy(p => p.SortOrder))
                .Include(r => r.Criteria.OrderBy(c => c.SortOrder))
                    .ThenInclude(c => c.LevelDescriptions)
                .OrderBy(r => r.Name)
                .ToListAsync();

            return rubrics.Select(MapToDto);
        }

        public async Task<RubricDto?> GetRubricByIdAsync(int id)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.PerformanceLevels.OrderBy(p => p.SortOrder))
                .Include(r => r.Criteria.OrderBy(c => c.SortOrder))
                    .ThenInclude(c => c.LevelDescriptions)
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
                PerformanceLevels = dto.PerformanceLevels.Select(p => new PerformanceLevel
                {
                    Level = p.Level,
                    Label = p.Label,
                    SortOrder = p.SortOrder
                }).ToList(),
                Criteria = dto.Criteria.Select(c => new RubricCriterion
                {
                    Criterion = c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions.Select(d => new CriterionLevelDescription
                    {
                        PerformanceLevelId = d.PerformanceLevelId,
                        Description = d.Description
                    }).ToList()
                }).ToList()
            };

            _context.QuestionRubrics.Add(rubric);
            await _context.SaveChangesAsync();

            return MapToDto(rubric);
        }

        public async Task<RubricDto> UpdateRubricAsync(RubricUpdateDto dto)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.PerformanceLevels)
                .Include(r => r.Criteria)
                    .ThenInclude(c => c.LevelDescriptions)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);

            if (rubric == null)
                throw new KeyNotFoundException($"Rubric with ID {dto.Id} not found.");

            rubric.Name = dto.Name;
            rubric.Description = dto.Description;
            rubric.MaxScore = dto.MaxScore;

            // Update performance levels
            var existingLevels = rubric.PerformanceLevels.ToList();
            foreach (var levelDto in dto.PerformanceLevels)
            {
                if (levelDto.Id > 0)
                {
                    var existing = existingLevels.FirstOrDefault(l => l.Id == levelDto.Id);
                    if (existing != null)
                    {
                        existing.Level = levelDto.Level;
                        existing.Label = levelDto.Label;
                        existing.SortOrder = levelDto.SortOrder;
                    }
                }
                else
                {
                    rubric.PerformanceLevels.Add(new PerformanceLevel
                    {
                        Level = levelDto.Level,
                        Label = levelDto.Label,
                        SortOrder = levelDto.SortOrder
                    });
                }
            }
            var dtoLevelIds = dto.PerformanceLevels.Select(l => l.Id).ToHashSet();
            var levelsToRemove = existingLevels.Where(l => !dtoLevelIds.Contains(l.Id)).ToList();
            _context.PerformanceLevels.RemoveRange(levelsToRemove);

            // Update criteria
            var existingCriteria = rubric.Criteria.ToList();
            foreach (var criterionDto in dto.Criteria)
            {
                if (criterionDto.Id > 0)
                {
                    var existing = existingCriteria.FirstOrDefault(c => c.Id == criterionDto.Id);
                    if (existing != null)
                    {
                        existing.Criterion = criterionDto.Criterion;
                        existing.SortOrder = criterionDto.SortOrder;

                        // Update level descriptions
                        var existingDescriptions = existing.LevelDescriptions.ToList();
                        foreach (var descDto in criterionDto.LevelDescriptions)
                        {
                            if (descDto.Id > 0)
                            {
                                var existingDesc = existingDescriptions.FirstOrDefault(d => d.Id == descDto.Id);
                                if (existingDesc != null)
                                {
                                    existingDesc.PerformanceLevelId = descDto.PerformanceLevelId;
                                    existingDesc.Description = descDto.Description;
                                }
                            }
                            else
                            {
                                existing.LevelDescriptions.Add(new CriterionLevelDescription
                                {
                                    PerformanceLevelId = descDto.PerformanceLevelId,
                                    Description = descDto.Description
                                });
                            }
                        }
                        var dtoDescIds = criterionDto.LevelDescriptions.Select(d => d.Id).ToHashSet();
                        var descsToRemove = existingDescriptions.Where(d => !dtoDescIds.Contains(d.Id)).ToList();
                        _context.CriterionLevelDescriptions.RemoveRange(descsToRemove);
                    }
                }
                else
                {
                    rubric.Criteria.Add(new RubricCriterion
                    {
                        Criterion = criterionDto.Criterion,
                        SortOrder = criterionDto.SortOrder,
                        LevelDescriptions = criterionDto.LevelDescriptions.Select(d => new CriterionLevelDescription
                        {
                            PerformanceLevelId = d.PerformanceLevelId,
                            Description = d.Description
                        }).ToList()
                    });
                }
            }
            var dtoCriteriaIds = dto.Criteria.Select(c => c.Id).ToHashSet();
            var criteriaToRemove = existingCriteria.Where(c => !dtoCriteriaIds.Contains(c.Id)).ToList();
            _context.RubricCriteria.RemoveRange(criteriaToRemove);

            await _context.SaveChangesAsync();
            return MapToDto(rubric);
        }

        public async Task DeleteRubricAsync(int id)
        {
            var rubric = await _context.QuestionRubrics
                .Include(r => r.Criteria)
                    .ThenInclude(c => c.LevelDescriptions)
                .Include(r => r.PerformanceLevels)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rubric != null)
            {
                foreach (var criterion in rubric.Criteria)
                {
                    _context.CriterionLevelDescriptions.RemoveRange(criterion.LevelDescriptions);
                }
                _context.RubricCriteria.RemoveRange(rubric.Criteria);
                _context.PerformanceLevels.RemoveRange(rubric.PerformanceLevels);
                _context.QuestionRubrics.Remove(rubric);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<RubricDto>> GetRubricsByCourseAsync(int courseId)
        {
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
                PerformanceLevels = rubric.PerformanceLevels.Select(p => new PerformanceLevelDto
                {
                    Id = p.Id,
                    Level = p.Level,
                    Label = p.Label,
                    SortOrder = p.SortOrder
                }).ToList(),
                Criteria = rubric.Criteria.Select(c => new CriterionDto
                {
                    Id = c.Id,
                    Criterion = c.Criterion,
                    SortOrder = c.SortOrder,
                    LevelDescriptions = c.LevelDescriptions.Select(d => new CriterionLevelDescriptionDto
                    {
                        Id = d.Id,
                        PerformanceLevelId = d.PerformanceLevelId,
                        Description = d.Description
                    }).ToList()
                }).ToList()
            };
        }
    }
}