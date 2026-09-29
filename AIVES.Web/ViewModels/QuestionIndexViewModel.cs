namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for displaying a list of questions in the Index view.
/// UI-specific shape: formatted fields, URLs, badges.
/// </summary>
public class QuestionIndexViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string BloomLevel { get; set; } = "Remember";
    public string Status { get; set; } = "Pending";
    public string SourceType { get; set; } = "Manual";
    public string? CourseName { get; set; }
    public string? TopicName { get; set; }
    public string? RubricName { get; set; }
    public DateTime CreatedAt { get; set; }

    // UI helpers
    public string StatusBadge => Status switch
    {
        "Approved" => "bg-success",
        "Pending" => "bg-warning text-dark",
        "Rejected" => "bg-danger",
        _ => "bg-secondary"
    };
    public string BloomBadge => BloomLevel switch
    {
        "Remember" => "bg-info",
        "Understand" => "bg-primary",
        "Apply" => "bg-success",
        "Analyze" => "bg-warning text-dark",
        _ => "bg-secondary"
    };
    public string DetailsUrl => $"/Questions/Details/{Id}";
    public string EditUrl => $"/Questions/Edit/{Id}";
    public string DeleteUrl => $"/Questions/Delete/{Id}";
}
