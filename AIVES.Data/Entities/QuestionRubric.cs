using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class QuestionRubric
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; } // Highest level value (e.g., 4)

        public List<PerformanceLevel> PerformanceLevels { get; set; } = new();
        public List<RubricCriterion> Criteria { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
    }

    public class PerformanceLevel
    {
        public int Id { get; set; }
        public int QuestionRubricId { get; set; }
        public QuestionRubric? QuestionRubric { get; set; }
        public int Level { get; set; } // 1, 2, 3, 4
        public string Label { get; set; } = string.Empty; // e.g., "Beginning", "Developing", "Proficient", "Exemplary"
        public int SortOrder { get; set; }
    }

    public class RubricCriterion
    {
        public int Id { get; set; }
        public string Criterion { get; set; } = string.Empty;
        public int QuestionRubricId { get; set; }
        public QuestionRubric? QuestionRubric { get; set; }
        public int SortOrder { get; set; }

        // Descriptions per performance level (matrix cells)
        public List<CriterionLevelDescription> LevelDescriptions { get; set; } = new();
    }

    public class CriterionLevelDescription
    {
        public int Id { get; set; }
        public int RubricCriterionId { get; set; }
        public RubricCriterion? RubricCriterion { get; set; }
        public int PerformanceLevelId { get; set; }
        public PerformanceLevel? PerformanceLevel { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}