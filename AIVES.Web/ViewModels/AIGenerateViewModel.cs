using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIVES.Web.ViewModels;

public class AIGenerateViewModel
{
    public int CourseId { get; set; }
    public string CourseCode { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int? TopicId { get; set; }
    public string BloomLevel { get; set; } = "Remember";
    public int? RubricId { get; set; }

    public string? GeneratedQuestion { get; set; }
    public string? ReferenceAnswer { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasResult { get; set; }

    public List<string> BloomLevels => new() { "Remember", "Understand", "Apply", "Analyze" };
    public List<SelectListItem> Courses { get; set; } = new();
    public List<SelectListItem> Topics { get; set; } = new();
    public List<SelectListItem> Rubrics { get; set; } = new();
    public bool ShowNewTopicInput { get; set; } = false;
}
