using Microsoft.AspNetCore.Mvc.Rendering;
using AIVES.Business.DTOs;

namespace AIVES.Web.ViewModels;

/// <summary>
/// ViewModel for displaying the question bank grouped by course.
/// </summary>
public class QuestionBankViewModel
{
    public int? SelectedCourseId { get; set; }
    public string? SelectedCourseName { get; set; }
    public List<SelectListItem> Courses { get; set; } = new();
    public List<QuestionGroupViewModel> Groups { get; set; } = new();
    public int TotalCount => Groups.Sum(g => g.Count);

    public static QuestionBankViewModel AllQuestions(IEnumerable<QuestionIndexViewModel> questions, IEnumerable<CourseDto> courses)
    {
        var courseList = new List<SelectListItem> { new SelectListItem("All Courses", "") };
        courseList.AddRange(courses.Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())));

        var group = new QuestionGroupViewModel
        {
            CourseName = "All Questions",
            CourseId = null,
            Questions = questions.ToList()
        };

        return new QuestionBankViewModel
        {
            SelectedCourseId = null,
            SelectedCourseName = "All Questions",
            Courses = courseList,
            Groups = new List<QuestionGroupViewModel> { group }
        };
    }

    public static QuestionBankViewModel ByCourse(IEnumerable<QuestionIndexViewModel> questions, IEnumerable<CourseDto> courses, int courseId)
    {
        var courseList = new List<SelectListItem> { new SelectListItem("All Courses", "") };
        courseList.AddRange(courses.Select(c => new SelectListItem($"{c.Code} - {c.Name}", c.Id.ToString())));

        var selectedCourse = courses.FirstOrDefault(c => c.Id == courseId);
        var courseName = selectedCourse != null ? $"{selectedCourse.Code} - {selectedCourse.Name}" : "Unknown Course";

        var group = new QuestionGroupViewModel
        {
            CourseName = courseName,
            CourseId = courseId,
            Questions = questions.ToList()
        };

        return new QuestionBankViewModel
        {
            SelectedCourseId = courseId,
            SelectedCourseName = courseName,
            Courses = courseList,
            Groups = new List<QuestionGroupViewModel> { group }
        };
    }
}

/// <summary>
/// Groups questions by course for the question bank view.
/// </summary>
public class QuestionGroupViewModel
{
    public int? CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public List<QuestionIndexViewModel> Questions { get; set; } = new();
    public int Count => Questions.Count;
}
