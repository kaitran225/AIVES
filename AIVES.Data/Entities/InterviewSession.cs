using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = "InProgress"; // InProgress, Completed

        public int ExamParticipantId { get; set; }
        public ExamParticipant ExamParticipant { get; set; } = null!;

        public List<InterviewQuestion> Questions { get; set; } = new();
    }
}