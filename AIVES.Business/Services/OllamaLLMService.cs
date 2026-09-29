using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AIVES.Business.Interfaces;

namespace AIVES.Business.Services;

public class OllamaLLMService : ILLMService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaLLMService> _logger;
    private readonly string _model;
    private readonly string _endpoint;

    public OllamaLLMService(HttpClient httpClient, IConfiguration configuration,
        ILogger<OllamaLLMService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _endpoint = _configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        _model = _configuration["Ollama:Model"] ?? "phi3:mini";
    }

    public async Task<string> GenerateAsync(
        string prompt,
        string? systemPrompt = null,
        int maxTokens = 512,
        float temperature = 0.7f)
    {
        try
        {
            var request = new OllamaGenerateRequest
            {
                Model = _model,
                Prompt = prompt,
                System = systemPrompt,
                Stream = false,
                Options = new OllamaOptions
                {
                    NumPredict = maxTokens,
                    Temperature = temperature
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_endpoint}/api/generate", request);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
            return content?.Response ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning("Ollama service unavailable at {_Endpoint}: {Message}", _endpoint, ex.Message);
            return string.Empty;
        }
    }

    public class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string? System { get; set; }
        public bool Stream { get; set; } = false;
        public OllamaOptions? Options { get; set; }
    }

    public class OllamaOptions
    {
        public int? NumPredict { get; set; }
        public float? Temperature { get; set; }
    }

    public class OllamaGenerateResponse
    {
        public string? Response { get; set; }
        public bool Done { get; set; }
    }
}
