using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class StudentAnswer
    {
        public int Id { get; set; }
        public string Transcript { get; set; } = string.Empty;
        public int DurationSeconds { get; set; }
        public int SpeechDurationSeconds { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        public int InterviewQuestionId { get; set; }
        public InterviewQuestion InterviewQuestion { get; set; } = null!;
    }
}