namespace AIVES.Business.DTOs
{
    public class RubricCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<CriterionDto> Criteria { get; set; } = new();
    }

    public class RubricUpdateDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<CriterionDto> Criteria { get; set; } = new();
    }

    public class CriterionDto
    {
        public int Id { get; set; }
        public string Criterion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string ScoringGuidance { get; set; } = string.Empty;
    }

    public class RubricDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }
        public List<CriterionDto> Criteria { get; set; } = new();
    }
}
