using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class InterviewQuestion
    {
        public int Id { get; set; }
        public bool IsFollowUp { get; set; } = false;
        public int? PreviousQuestionId { get; set; }
        public InterviewQuestion? PreviousQuestion { get; set; }

        public int ExamQuestionId { get; set; }
        public ExamQuestion ExamQuestion { get; set; } = null!;

        public string QuestionText { get; set; } = string.Empty;
        public string? AIAnalysis { get; set; }

        public List<StudentAnswer> Answers { get; set; } = new();
        public AIScoreSuggestion? ScoreSuggestion { get; set; }
    }
}