using AIVES.Business.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AIVES.Business.Services;

public class SimpleRAGService : IAIInterviewService
{
    private readonly ICourseMaterialService _materialService;
    private readonly ILLMService _llmService;
    private readonly ILogger<SimpleRAGService> _logger;

    private const string SYSTEM_PROMPT =
        "You are AIVES, an AI assistant specialized in AI-powered viva examinations. " +
        "Generate clear, concise, academically appropriate questions and responses. " +
        "Format responses with clear labels: QUESTION:, REFERENCE ANSWER:, FEEDBACK:, SCORE:, FOLLOW-UP:.";

    public SimpleRAGService(
        ICourseMaterialService materialService,
        ILLMService llmService,
        ILogger<SimpleRAGService> logger)
    {
        _materialService = materialService;
        _llmService = llmService;
        _logger = logger;
    }

    public async Task<string> GenerateQuestionFromMaterialAsync(string courseCode, string topic, string bloomLevel)
    {
        var relevantChunks = await _materialService.RetrieveRelevantChunksAsync(courseCode, topic, maxChunks: 5);
        if (string.IsNullOrWhiteSpace(relevantChunks) || relevantChunks.StartsWith("[No"))
        {
            return $"No relevant course materials found for course {courseCode} and topic {topic}. " +
                   "Please import learning materials first.";
        }

        var userPrompt = $@"Based on the following course materials, generate a viva examination question:

Course: {courseCode}
Topic: {topic}
Bloom's Taxonomy Level: {bloomLevel}

Relevant Materials:
{relevantChunks}

Requirements:
- Generate ONE clear viva examination question
- Include a brief reference/expected answer
- Match the specified Bloom's taxonomy level
- Format exactly as:
  QUESTION: <your question here>
  REFERENCE ANSWER: <your reference answer here>";

        var result = await _llmService.GenerateAsync(userPrompt, systemPrompt: SYSTEM_PROMPT, maxTokens: 512, temperature: 0.7f);

        if (string.IsNullOrWhiteSpace(result))
        {
            _logger.LogWarning("LLM generation returned empty result for course {Course}, topic {Topic}", courseCode, topic);
            return GenerateTemplateQuestion(topic, bloomLevel, relevantChunks);
        }

        return result.Trim();
    }

    public async Task<string> AnalyzeAnswerAsync(string question, string transcript, string rubricDescription)
    {
        var wordCount = Math.Max(1, transcript.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length);
        var userPrompt = $@"Analyze the following student answer and provide feedback:

QUESTION: {question}

STUDENT ANSWER ({wordCount} words):
{transcript}

RUBRIC CRITERIA:
{rubricDescription}

Provide structured feedback including:
1. Strengths of the answer
2. Areas for improvement
3. Specific suggestions for a better answer";

        var result = await _llmService.GenerateAsync(userPrompt, systemPrompt: SYSTEM_PROMPT, maxTokens: 1024, temperature: 0.5f);

        if (string.IsNullOrWhiteSpace(result))
        {
            return GenerateTemplateFeedback(wordCount);
        }

        return result.Trim();
    }

    public async Task<string> GenerateFollowUpQuestionAsync(string originalQuestion, string studentAnswer, string topic)
    {
        var truncatedAnswer = studentAnswer.Length > 500
            ? studentAnswer.Substring(0, 500) + "..."
            : studentAnswer;

        var wordCount = studentAnswer.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length;

        var guidance = wordCount < 50
            ? "Ask for clarification or elaboration on a key point."
            : "Challenge the student's assumptions or explore an edge case they haven't addressed.";

        var userPrompt = $@"Generate an adaptive follow-up question for a viva examination.

ORIGINAL QUESTION: {originalQuestion}

STUDENT'S ANSWER:
{truncatedAnswer}

TOPIC: {topic}

Guidance: {guidance}

Generate ONE probing follow-up question that tests deeper understanding.";

        var result = await _llmService.GenerateAsync(userPrompt, systemPrompt: SYSTEM_PROMPT, maxTokens: 256, temperature: 0.6f);

        if (string.IsNullOrWhiteSpace(result))
        {
            if (wordCount < 50)
                return $"Could you elaborate on your answer to: \"{originalQuestion}\"? What specific examples or evidence support your response?";
            return $"Regarding \"{originalQuestion}\": How would your answer change if the conditions were different? What are the limitations or edge cases of your approach?";
        }

        return result.Trim();
    }

    public async Task<string> EvaluateAnswerAsync(string question, string transcript, string rubricDescription)
    {
        var wordCount = Math.Max(1, transcript.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length);
        var userPrompt = $@"Evaluate the following student answer for a viva examination and provide a grading recommendation:

QUESTION: {question}

STUDENT ANSWER:
{transcript}

RUBRIC:
{rubricDescription}

Provide:
1. A suggested score out of 4
2. Key strengths
3. Areas for improvement
4. A brief explanation justifying the score";

        var result = await _llmService.GenerateAsync(userPrompt, systemPrompt: SYSTEM_PROMPT, maxTokens: 1024, temperature: 0.4f);

        if (string.IsNullOrWhiteSpace(result))
        {
            return GenerateTemplateEvaluation(wordCount, rubricDescription);
        }

        return result.Trim();
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

    private string GenerateTemplateFeedback(int wordCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AI Feedback ===");
        sb.AppendLine($"Word count: {wordCount}");
        if (wordCount < 10)
            sb.AppendLine("Feedback: The answer is too brief. Expand with more details and examples.");
        else if (wordCount < 30)
            sb.AppendLine("Feedback: The answer covers the basics but could be more detailed.");
        else
            sb.AppendLine("Feedback: The answer appears comprehensive. Check against rubric criteria for accuracy.");
        return sb.ToString();
    }

    private string GenerateTemplateEvaluation(int wordCount, string rubricDescription)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== AI Grading Recommendation ===");
        sb.AppendLine($"Rubric Criteria:\n{rubricDescription}");
        int score = wordCount >= 30 ? 3 : wordCount >= 15 ? 2 : wordCount >= 5 ? 1 : 0;
        sb.AppendLine($"Suggested Score: {score}/4");
        if (score == 0) sb.AppendLine("Strengths: None detected.\nWeaknesses: Answer does not address the question.");
        else if (score == 1) sb.AppendLine("Strengths: Shows basic awareness.\nWeaknesses: Lacks detail and specific examples.");
        else if (score == 2) sb.AppendLine("Strengths: Covers main points.\nWeaknesses: Could include more specific examples.");
        else sb.AppendLine("Strengths: Comprehensive and well-supported.\nWeaknesses: Minor improvements possible.");
        return sb.ToString();
    }
}
