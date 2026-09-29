namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for displaying a list of rubrics in the Index view.
/// </summary>
public class RubricIndexViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxScore { get; set; }
    public int CriteriaCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // UI helpers
    public string DetailsUrl => $"/Rubrics/Details/{Id}";
    public string EditUrl => $"/Rubrics/Edit/{Id}";
    public string DeleteUrl => $"/Rubrics/Delete/{Id}";
}

/// <summary>
/// ViewModel for the Create/Edit rubric form.
/// Contains nested criteria for the form.
/// </summary>
public class RubricFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxScore { get; set; }

    // Nested criteria (for the form)
    public List<RubricCriterionFormViewModel> Criteria { get; set; } = new();

    // UI helpers
    public string? ErrorMessage { get; set; }
}

public class RubricCreateViewModel : RubricFormViewModel { }
public class RubricUpdateViewModel : RubricFormViewModel { }

/// <summary>
/// ViewModel for displaying rubric details.
/// </summary>
public class RubricDetailViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxScore { get; set; }
    public List<RubricCriterionDetailViewModel> Criteria { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string DetailsUrl => $"/Rubrics/Details/{Id}";
    public string EditUrl => $"/Rubrics/Edit/{Id}";
}

/// <summary>
/// ViewModel for a single criterion in the rubric form.
/// </summary>
public class RubricCriterionFormViewModel
{
    public int Id { get; set; }
    public string Criterion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public string ScoringGuidance { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel for displaying a single criterion in the rubric details.
/// </summary>
public class RubricCriterionDetailViewModel
{
    public int Id { get; set; }
    public string Criterion { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public string ScoringGuidance { get; set; } = string.Empty;
}
