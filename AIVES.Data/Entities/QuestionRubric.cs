using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class QuestionRubric
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MaxScore { get; set; }

        public List<RubricCriterion> Criteria { get; set; } = new();
        public List<Question> Questions { get; set; } = new();
    }
}