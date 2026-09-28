using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class ExamParticipant
    {
        public int Id { get; set; }
        public string StudentNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Registered"; // Registered, Completed, Absent

        public int ExamSessionId { get; set; }
        public ExamSession ExamSession { get; set; } = null!;

        public int StudentId { get; set; }
        public User Student { get; set; } = null!;

        public InterviewSession? InterviewSession { get; set; }
    }
}