using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for displaying a list of course materials in the Index view.
/// </summary>
public class CourseMaterialIndexViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public string SourceType { get; set; } = "TextPaste";
    public string? FileName { get; set; }
    public int ChunkCount { get; set; }
    public DateTime ImportedAt { get; set; }

    // UI helpers
    public string TypeBadge => SourceType switch
    {
        "TextPaste" => "bg-info",
        "FileUpload" => "bg-primary",
        "URL" => "bg-success",
        _ => "bg-secondary"
    };
    public string PreviewUrl => $"/CourseMaterials/Preview/{Id}";
    public string DeleteUrl => $"/CourseMaterials/Delete/{Id}";
}

/// <summary>
/// ViewModel for the Create/Import course material form.
/// </summary>
public class CourseMaterialCreateViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string SourceType { get; set; } = "TextPaste";
    public string? FileName { get; set; }
    public IFormFile? FileUpload { get; set; }

    // UI helpers
    public List<SelectListItem> Courses { get; set; } = new();
    public List<string> SourceTypes => new() { "TextPaste", "FileUpload", "URL" };
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// ViewModel for displaying course material details/preview.
/// </summary>
public class CourseMaterialDetailViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public string SourceType { get; set; } = "TextPaste";
    public string? FileName { get; set; }
    public int ChunkCount { get; set; }
    public DateTime ImportedAt { get; set; }

    public string PreviewUrl => $"/CourseMaterials/Preview/{Id}";
}

/// <summary>
/// ViewModel for the import result (success/failure message).
/// </summary>
public class ImportResultViewModel
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int MaterialId { get; set; }
    public int ChunkCount { get; set; }
}
