using System;
using System.Collections.Generic;

namespace AIVES.Data.Entities
{
    public class ExamQuestion
    {
        public int Id { get; set; }
        public int OrderIndex { get; set; }

        public int ExamSessionId { get; set; }
        public ExamSession ExamSession { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public List<InterviewQuestion> InterviewQuestions { get; set; } = new();
    }
}