using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Lecturer or Student
        public string? StudentNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Course? Course { get; set; }
        public int? CourseId { get; set; }
        public List<ExamParticipant> Participants { get; set; } = new();
        public List<LecturerEvaluation> LecturerEvaluations { get; set; } = new();
    }
}