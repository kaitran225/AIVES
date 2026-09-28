using System;

namespace AIVES.Data.Entities
{
    public class RubricCriterion
    {
        public int Id { get; set; }
        public string Criterion { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxScore { get; set; }
        public string ScoringGuidance { get; set; } = string.Empty;

        public int RubricId { get; set; }
        public QuestionRubric? Rubric { get; set; }
    }
}