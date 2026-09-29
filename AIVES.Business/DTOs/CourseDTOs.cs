namespace AIVES.Business.DTOs;

public class CourseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CourseCreateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// Optional seed topics, one per line. Created together with the course.
    /// </summary>
    public string? InitialTopics { get; set; }
}

public class CourseUpdateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class QuestionTopicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseId { get; set; }
}

public class QuestionTopicCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseId { get; set; }
}

public class QuestionTopicUpdateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CourseId { get; set; }
}

/// <summary>
/// Result of a delete attempt, carrying the blocking dependencies when the
/// entity cannot be removed (e.g. a course that still has questions).
/// </summary>
public class CourseDeleteResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Blockers { get; set; } = new List<string>();
}

public class TopicDeleteResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IReadOnlyList<string> Blockers { get; set; } = new List<string>();
}
