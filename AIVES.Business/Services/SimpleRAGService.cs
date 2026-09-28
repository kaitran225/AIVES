using AIVES.Business.Interfaces;
using AIVES.Data;
using AIVES.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace AIVES.Business.Services
{
    public class SimpleRAGService : IAIInterviewService
    {
        private readonly AIVESDbContext _context;
        private readonly ICourseMaterialService _materialService;

        public SimpleRAGService(AIVESDbContext context, ICourseMaterialService materialService)
        {
            _context = context;
            _materialService = materialService;
        }

        public async Task<string> GenerateQuestionFromMaterialAsync(string courseCode, string topic, string bloomLevel)
        {
            var relevantChunks = await _materialService.RetrieveRelevantChunksAsync(courseCode, topic, maxChunks: 5);
            if (string.IsNullOrWhiteSpace(relevantChunks))
                return $"[AI Helper] No course materials found for course {courseCode} and topic {topic}. Please import learning materials first.";
            var prompt = $"Based on the following course materials, generate a viva examination question at Bloom's level {bloomLevel}:\n\n" +
                         $"Course: {courseCode}\nTopic: {topic}\n\nRelevant Materials:\n{relevantChunks}\n\n" +
                         "Requirements:\n- Generate ONE clear question\n- Include a reference answer\n" +
                         "- Match the specified Bloom's taxonomy level\n- The question should test understanding, not just recall\n" +
                         "- Format: QUESTION: <question text>\nREFERENCE ANSWER: <answer>";
            return GenerateTemplateQuestion(topic, bloomLevel, relevantChunks);
        }

        public async Task<string> AnalyzeAnswerAsync(string question, string transcript, string rubricDescription)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Question: {question}");
            sb.AppendLine($"Student Answer:\n{transcript}");
            sb.AppendLine($"Rubric:\n{rubricDescription}");
            var wordCount = transcript.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            sb.AppendLine($"Word count: {wordCount}");
            if (wordCount < 10)
                sb.AppendLine("AI Feedback: The answer is too brief. Expand with more details and examples.");
            else if (wordCount < 30)
                sb.AppendLine("AI Feedback: The answer covers the basics but could be more detailed.");
            else
                sb.AppendLine("AI Feedback: The answer appears comprehensive. Check against rubric criteria for accuracy.");
            return sb.ToString();
        }

        public async Task<string> GenerateFollowUpQuestionAsync(string originalQuestion, string studentAnswer, string topic)
        {
            var truncatedAnswer = studentAnswer.Length > 200 ? studentAnswer.Substring(0, 200) : studentAnswer;
            var followUp = $"Based on the student's response to {originalQuestion}:\n{truncatedAnswer}\n\n" +
                           $"Generate a follow-up question that probes deeper into topic: {topic}.\nThe follow-up should:";
            if (studentAnswer.Length < 50)
                followUp += "\n- Ask for clarification or elaboration";
            else
                followUp += "\n- Challenge assumptions or explore edge cases";
            return followUp;
        }

        public async Task<string> EvaluateAnswerAsync(string question, string transcript, string rubricDescription)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== AI Grading Recommendation ===");
            sb.AppendLine($"Question: {question}");
            sb.AppendLine($"Rubric Criteria:\n{rubricDescription}");
            var wordCount = transcript.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var lines = rubricDescription.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var hasKeywords = lines.Any(k => !string.IsNullOrWhiteSpace(k) && transcript.Contains(k, StringComparison.OrdinalIgnoreCase));
            int score;
            if (wordCount >= 30 && hasKeywords) score = 3;
            else if (wordCount >= 15) score = 2;
            else if (wordCount >= 5) score = 1;
            else score = 0;
            sb.AppendLine($"Suggested Score: {score}/4");
            if (score == 0) sb.AppendLine("Strengths: None detected.\nWeaknesses: Answer does not address the question.");
            else if (score == 1) sb.AppendLine("Strengths: Shows basic awareness.\nWeaknesses: Lacks detail and specific examples.");
            else if (score == 2) sb.AppendLine("Strengths: Covers main points.\nWeaknesses: Could include more specific examples.");
            else sb.AppendLine("Strengths: Comprehensive and well-supported.\nWeaknesses: Minor improvements possible.");
            return sb.ToString();
        }

        private string GenerateTemplateQuestion(string topic, string bloomLevel, string context)
        {
            var bloomTemplates = new Dictionary<string, string>
            {
                ["Remember"] = "What is the definition of {topic}? Please explain the key concepts and principles.",
                ["Understand"] = "Explain the concept of {topic} and describe how it applies in practice.",
                ["Apply"] = "How would you apply the principles of {topic} to solve a real-world problem? Provide a concrete example.",
                ["Analyze"] = "Analyze the strengths and weaknesses of the approaches used in {topic}. What are the trade-offs?"
            };
            var template = bloomTemplates.GetValueOrDefault(bloomLevel, bloomTemplates["Understand"]);
            var question = template.Replace("{topic}", topic);
            var contextLines = context.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Take(3).ToList();
            var contextPreview = string.Join("\n", contextLines);
            return $"QUESTION: {question}\n\nREFERENCE ANSWER: A comprehensive answer should cover the key concepts of {topic}, " +
                   $"demonstrating understanding at the {bloomLevel} level of Bloom's taxonomy. " +
                   $"Key points from course materials include:\n{contextPreview}";
        }
    }
}
