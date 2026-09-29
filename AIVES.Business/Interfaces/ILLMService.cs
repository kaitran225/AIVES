namespace AIVES.Business.Interfaces;

public interface ILLMService
{
    Task<string> GenerateAsync(
        string prompt,
        string? systemPrompt = null,
        int maxTokens = 512,
        float temperature = 0.7f);
}
