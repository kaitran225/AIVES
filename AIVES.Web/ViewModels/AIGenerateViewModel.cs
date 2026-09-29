namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for the AI Question Generator page.
/// Contains form inputs and the generated result.
/// </summary>
public class AIGenerateViewModel
{
    // Form inputs
    public string CourseCode { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string BloomLevel { get; set; } = "Remember";

    // Generated result
    public string? GeneratedQuestion { get; set; }
    public string? ReferenceAnswer { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasResult { get; set; }

    // UI helpers
    public List<string> BloomLevels => new() { "Remember", "Understand", "Apply", "Analyze" };
}
