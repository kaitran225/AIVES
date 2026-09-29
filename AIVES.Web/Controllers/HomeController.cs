using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AIVES.Business.Interfaces;
using AIVES.Business.DTOs;
using AIVES.Web.ViewModels;
using AIVES.Web.Models;

namespace AIVES.Web.Controllers;

public class HomeController : Controller
{
    private readonly IQuestionService _questionService;
    private readonly IRubricService _rubricService;
    private readonly ICourseMaterialService _materialService;

    public HomeController(
        IQuestionService questionService,
        IRubricService rubricService,
        ICourseMaterialService materialService)
    {
        _questionService = questionService;
        _rubricService = rubricService;
        _materialService = materialService;
    }

    public async Task<IActionResult> Index()
    {
        var questions = await _questionService.GetAllQuestionsAsync();
        var rubrics = await _rubricService.GetAllRubricsAsync();
        var materials = await _materialService.GetAllMaterialsAsync();

        var questionList = questions.ToList();
        var rubricList = rubrics.ToList();
        var materialList = materials.ToList();

        var viewModel = new HomeDashboardViewModel
        {
            TotalQuestions = questionList.Count,
            ApprovedQuestions = questionList.Count(q => q.Status == "Approved"),
            PendingQuestions = questionList.Count(q => q.Status == "Pending"),
            TotalRubrics = rubricList.Count,
            TotalMaterials = materialList.Count,
            RecentQuestions = questionList.Take(5).Select(MapToIndex).ToList(),
            RecentMaterials = materialList.Take(5).Select(MapToMaterialIndex).ToList(),
            RecentRubrics = rubricList.Take(5).Select(MapToRubricIndex).ToList(),
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // ===== Mapping Methods (DTO → ViewModel) =====
    private QuestionIndexViewModel MapToIndex(QuestionDto dto)
    {
        return new QuestionIndexViewModel
        {
            Id = dto.Id,
            Text = dto.Text,
            BloomLevel = dto.BloomLevel,
            Status = dto.Status,
            SourceType = dto.SourceType,
            CourseName = dto.CourseName,
            TopicName = dto.TopicName,
            RubricName = dto.RubricName,
            CreatedAt = dto.CreatedAt
        };
    }

    private CourseMaterialIndexViewModel MapToMaterialIndex(CourseMaterialDto dto)
    {
        return new CourseMaterialIndexViewModel
        {
            Id = dto.Id,
            Title = dto.Title,
            CourseName = dto.CourseName,
            SourceType = dto.SourceType,
            FileName = dto.FileName,
            ChunkCount = dto.ChunkCount,
            ImportedAt = dto.ImportedAt
        };
    }

    private RubricIndexViewModel MapToRubricIndex(RubricDto dto)
    {
        return new RubricIndexViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            MaxScore = dto.MaxScore,
            CriteriaCount = dto.Criteria?.Count ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
