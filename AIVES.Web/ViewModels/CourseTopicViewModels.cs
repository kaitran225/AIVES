using Microsoft.AspNetCore.Mvc.Rendering;

namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for the Course &amp; Topic manager. Courses and topics are created
/// and maintained here, which makes this page the single source of truth for
/// every course/topic dropdown in the application.
/// </summary>
public class CourseManagerViewModel
{
    public List<CourseWithTopicsViewModel> Courses { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class CourseWithTopicsViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaterialCount { get; set; }
    public int QuestionCount { get; set; }
    public List<TopicRowViewModel> Topics { get; set; } = new();
}

public class TopicRowViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
}

public class CourseFormViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Seed topics for a new course, one per line.</summary>
    public string? InitialTopics { get; set; }
}

public class TopicFormViewModel
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseLabel { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<SelectListItem> Courses { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
