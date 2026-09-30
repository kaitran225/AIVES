namespace AIVES.Business.DTOs
{
    public class RubricCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<PerformanceLevelDto> PerformanceLevels { get; set; } = new();
        public List<CriterionDto> Criteria { get; set; } = new();
    }

    public class RubricUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<PerformanceLevelDto> PerformanceLevels { get; set; } = new();
        public List<CriterionDto> Criteria { get; set; } = new();
    }

    public class PerformanceLevelDto
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Label { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class CriterionDto
    {
        public int Id { get; set; }
        public string Criterion { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public List<CriterionLevelDescriptionDto> LevelDescriptions { get; set; } = new();
    }

    public class CriterionLevelDescriptionDto
    {
        public int Id { get; set; }
        public int PerformanceLevelId { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class RubricDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<PerformanceLevelDto> PerformanceLevels { get; set; } = new();
        public List<CriterionDto> Criteria { get; set; } = new();
    }
}