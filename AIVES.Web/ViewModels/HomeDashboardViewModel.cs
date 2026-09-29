namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for the Home/Index dashboard page.
/// Aggregates statistics from multiple services.
/// </summary>
public class HomeDashboardViewModel
{
    public int TotalQuestions { get; set; }
    public int ApprovedQuestions { get; set; }
    public int PendingQuestions { get; set; }
    public int TotalRubrics { get; set; }
    public int TotalMaterials { get; set; }
    public int TotalCourses { get; set; }

    public List<QuestionIndexViewModel> RecentQuestions { get; set; } = new();
    public List<CourseMaterialIndexViewModel> RecentMaterials { get; set; } = new();
    public List<RubricIndexViewModel> RecentRubrics { get; set; } = new();
}
