using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class ExamSession
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TimePerStudentMinutes { get; set; } = 15;
        public int AnswerTimeSeconds { get; set; } = 60;
        public int MainQuestionsCount { get; set; } = 5;
        public int MaxFollowUpsPerQuestion { get; set; } = 2;
        public string Status { get; set; } = "Draft"; // Draft, Scheduled, InProgress, Completed

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public List<ExamParticipant> Participants { get; set; } = new();
        public List<ExamQuestion> ExamQuestions { get; set; } = new();
        public List<InterviewSession> InterviewSessions { get; set; } = new();
    }
}