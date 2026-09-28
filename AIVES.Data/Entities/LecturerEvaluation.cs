using System;

namespace AIVES.Data.Entities
{
    public class LecturerEvaluation
    {
        public int Id { get; set; }
        public int FinalScore { get; set; }
        public string? LecturerComment { get; set; }
        public DateTime EvaluatedAt { get; set; } = DateTime.Now;

        public int InterviewQuestionId { get; set; }
        public InterviewQuestion InterviewQuestion { get; set; } = null!;

        public int LecturerId { get; set; }
        public User Lecturer { get; set; } = null!;
    }
}