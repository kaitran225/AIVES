using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for creating/editing a question (used by the Create/Edit form).
/// Maps from QuestionCreateDto / QuestionUpdateDto in the controller.
/// </summary>
public class QuestionFormViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ReferenceAnswer { get; set; }
    public string BloomLevel { get; set; } = "Remember";
    public string SourceType { get; set; } = "Manual";
    public string? ReferenceMaterial { get; set; }
    public int CourseId { get; set; }
    public int? TopicId { get; set; }
    public int? RubricId { get; set; }
    public string Status { get; set; } = "Pending";

    // UI helpers (populated by controller)
    public List<SelectListItem> Courses { get; set; } = new();
    public List<SelectListItem> Topics { get; set; } = new();
    public List<SelectListItem> Rubrics { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class QuestionCreateViewModel : QuestionFormViewModel { }
public class QuestionUpdateViewModel : QuestionFormViewModel { }

public class QuestionDetailViewModel
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ReferenceAnswer { get; set; }
    public string BloomLevel { get; set; } = "Remember";
    public string Status { get; set; } = "Pending";
    public string SourceType { get; set; } = "Manual";
    public string? ReferenceMaterial { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CourseId { get; set; }
    public string? CourseName { get; set; }
    public int? TopicId { get; set; }
    public string? TopicName { get; set; }
    public int? RubricId { get; set; }
    public string? RubricName { get; set; }

    public string StatusBadge => Status switch
    {
        "Approved" => "bg-success",
        "Pending" => "bg-warning text-dark",
        "Rejected" => "bg-danger",
        _ => "bg-secondary"
    };
    public string DetailsUrl => $"/Questions/Details/{Id}";
    public string EditUrl => $"/Questions/Edit/{Id}";
}
