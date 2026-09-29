using System.Collections.Generic;

namespace AIVES.Web.Models;

/// <summary>
/// Enhanced error view model with detailed error information.
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Error details
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public string? StackTrace { get; set; }
    public string? InnerErrorMessage { get; set; }
    public string? InnerErrorType { get; set; }
    public string? InnerStackTrace { get; set; }

    // Context
    public string? Url { get; set; }
    public string? HttpMethod { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Environment { get; set; }
    public bool IsDevelopment { get; set; }

    // Debug info
    public Dictionary<string, string>? UserAgent { get; set; }
    public Dictionary<string, string>? QueryString { get; set; }
    public Dictionary<string, string>? Cookies { get; set; }
}

