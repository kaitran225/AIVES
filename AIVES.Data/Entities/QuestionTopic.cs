using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class QuestionTopic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public List<Question> Questions { get; set; } = new();
    }
}