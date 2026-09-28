using System;

namespace AIVES.Data.Entities
{
    public class AIScoreSuggestion
    {
        public int Id { get; set; }
        public int SuggestedScore { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string MissingInformation { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        public int InterviewQuestionId { get; set; }
        public InterviewQuestion InterviewQuestion { get; set; } = null!;
    }
}