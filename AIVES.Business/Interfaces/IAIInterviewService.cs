namespace AIVES.Business.Interfaces
{
    /// <summary>
    /// AI Interview service interface. Uses a simplified RAG approach
    /// for question generation from imported course materials.
    /// </summary>
    public interface IAIInterviewService
    {
        /// <summary>
        /// Generate a question from imported course materials using simplified RAG.
        /// </summary>
        Task<string> GenerateQuestionFromMaterialAsync(
            string courseCode,
            string topic,
            string bloomLevel,
            string? rubricDescription = null);

        /// <summary>
        /// Analyze a student answer and provide feedback.
        /// </summary>
        Task<string> AnalyzeAnswerAsync(
            string question,
            string transcript,
            string rubricDescription);

        /// <summary>
        /// Generate an adaptive follow-up question.
        /// </summary>
        Task<string> GenerateFollowUpQuestionAsync(
            string originalQuestion,
            string studentAnswer,
            string topic);

        /// <summary>
        /// Produce a grading recommendation.
        /// </summary>
        Task<string> EvaluateAnswerAsync(
            string question,
            string transcript,
            string rubricDescription);
    }
}
