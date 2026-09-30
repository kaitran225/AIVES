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
    public int PerformanceLevelsCount { get; set; }

    public string DetailsUrl => $"/Rubrics/Details/{Id}";
    public string EditUrl => $"/Rubrics/Edit/{Id}";
    public string DeleteUrl => $"/Rubrics/Delete/{Id}";
}

/// <summary>
/// ViewModel for the Create/Edit rubric form.
/// Contains performance levels and criteria with descriptions per level.
/// </summary>
public class RubricFormViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxScore { get; set; } = 4;

    // Two-step creation
    public int Step { get; set; } = 1;
    public int CriteriaCount { get; set; }
    public List<string> LevelLabels { get; set; } = new();
    public List<string> CriterionNames { get; set; } = new();

    public List<PerformanceLevelFormViewModel> PerformanceLevels { get; set; } = new();
    public List<RubricCriterionFormViewModel> Criteria { get; set; } = new();

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
    public List<PerformanceLevelDetailViewModel> PerformanceLevels { get; set; } = new();
    public List<RubricCriterionDetailViewModel> Criteria { get; set; } = new();

    public string DetailsUrl => $"/Rubrics/Details/{Id}";
    public string EditUrl => $"/Rubrics/Edit/{Id}";
}

public class PerformanceLevelFormViewModel
{
    public int Id { get; set; }
    public int Level { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class PerformanceLevelDetailViewModel
{
    public int Id { get; set; }
    public int Level { get; set; }
    public string Label { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

/// <summary>
/// ViewModel for a single criterion in the rubric form.
/// Contains descriptions for each performance level.
/// </summary>
public class RubricCriterionFormViewModel
{
    public int Id { get; set; }
    public string Criterion { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<CriterionLevelDescriptionFormViewModel> LevelDescriptions { get; set; } = new();
}

public class CriterionLevelDescriptionFormViewModel
{
    public int Id { get; set; }
    public int PerformanceLevelId { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class RubricCriterionDetailViewModel
{
    public int Id { get; set; }
    public string Criterion { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<CriterionLevelDescriptionDetailViewModel> LevelDescriptions { get; set; } = new();
}

public class CriterionLevelDescriptionDetailViewModel
{
    public int Id { get; set; }
    public int PerformanceLevelId { get; set; }
    public string Description { get; set; } = string.Empty;
}